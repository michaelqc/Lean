﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Logging;
using QuantConnect.Orders;

namespace QuantConnect.Securities 
{
    /// <summary>
    /// Portfolio manager class groups popular properties and makes them accessible through one interface.
    /// It also provide indexing by the vehicle symbol to get the Security.Holding objects.
    /// </summary>
    public class SecurityPortfolioManager : IDictionary<string, SecurityHolding> 
    {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        /// <summary>
        /// Local access to the securities collection for the portfolio summation.
        /// </summary>
        public SecurityManager Securities;

        /// <summary>
        /// Local access to the transactions collection for the portfolio summation and updates.
        /// </summary>
        public SecurityTransactionManager Transactions;

        /// <summary>
        /// Gets the cash book that keeps track of all currency holdings
        /// </summary>
        public CashBook CashBook { get; private set; }
        
        //Record keeping variables
        private readonly Cash _baseCurrencyCash;

        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Initialise security portfolio manager.
        /// </summary>
        public SecurityPortfolioManager(SecurityManager securityManager, SecurityTransactionManager transactions) 
        {
            Securities = securityManager;
            Transactions = transactions;
            MarginCallModel = new MarginCallModel(this);

            CashBook = new CashBook();
            _baseCurrencyCash = CashBook[CashBook.AccountCurrency];

            // default to $100,000.00
            _baseCurrencyCash.Quantity = 100000;
        }

        #region IDictionary Implementation

        /******************************************************** 
        * DICTIONARY IMPLEMENTATION
        *********************************************************/
        /// <summary>
        /// Add a new securities string-security to the portfolio.
        /// </summary>
        /// <param name="symbol">Symbol of dictionary</param>
        /// <param name="holding">SecurityHoldings object</param>
        /// <exception cref="NotImplementedException">Portfolio object is an adaptor for Security Manager. This method is not applicable for PortfolioManager class.</exception>
        /// <remarks>This method is not implemented and using it will throw an exception</remarks>
        public void Add(string symbol, SecurityHolding holding) { throw new NotImplementedException("Portfolio object is an adaptor for Security Manager. To add a new asset add the required data during initialization."); }

        /// <summary>
        /// Add a new securities key value pair to the portfolio.
        /// </summary>
        /// <param name="pair">Key value pair of dictionary</param>
        /// <exception cref="NotImplementedException">Portfolio object is an adaptor for Security Manager. This method is not applicable for PortfolioManager class.</exception>
        /// <remarks>This method is not implemented and using it will throw an exception</remarks>
        public void Add(KeyValuePair<string, SecurityHolding> pair) { throw new NotImplementedException("Portfolio object is an adaptor for Security Manager. To add a new asset add the required data during initialization."); }

        /// <summary>
        /// Clear the portfolio of securities objects.
        /// </summary>
        /// <exception cref="NotImplementedException">Portfolio object is an adaptor for Security Manager. This method is not applicable for PortfolioManager class.</exception>
        /// <remarks>This method is not implemented and using it will throw an exception</remarks>
        public void Clear() { throw new NotImplementedException("Portfolio object is an adaptor for Security Manager and cannot be cleared."); }

        /// <summary>
        /// Remove this keyvalue pair from the portfolio.
        /// </summary>
        /// <exception cref="NotImplementedException">Portfolio object is an adaptor for Security Manager. This method is not applicable for PortfolioManager class.</exception>
        /// <param name="pair">Key value pair of dictionary</param>
        /// <remarks>This method is not implemented and using it will throw an exception</remarks>
        public bool Remove(KeyValuePair<string, SecurityHolding> pair) { throw new NotImplementedException("Portfolio object is an adaptor for Security Manager and objects cannot be removed."); }

        /// <summary>
        /// Remove this symbol from the portfolio.
        /// </summary>
        /// <exception cref="NotImplementedException">Portfolio object is an adaptor for Security Manager. This method is not applicable for PortfolioManager class.</exception>
        /// <param name="symbol">Symbol of dictionary</param>
        /// <remarks>This method is not implemented and using it will throw an exception</remarks>
        public bool Remove(string symbol) { throw new NotImplementedException("Portfolio object is an adaptor for Security Manager and objects cannot be removed."); }

        /// <summary>
        /// Check if the portfolio contains this symbol string.
        /// </summary>
        /// <param name="symbol">String search symbol for the security</param>
        /// <returns>Boolean true if portfolio contains this symbol</returns>
        public bool ContainsKey(string symbol)
        {
            return Securities.ContainsKey(symbol);
        }

        /// <summary>
        /// Check if the key-value pair is in the portfolio.
        /// </summary>
        /// <remarks>IDictionary implementation calling the underlying Securities collection</remarks>
        /// <param name="pair">Pair we're searching for</param>
        /// <returns>True if we have this object</returns>
        public bool Contains(KeyValuePair<string, SecurityHolding> pair)
        {
            return Securities.ContainsKey(pair.Key);
        }

        /// <summary>
        /// Count the securities objects in the portfolio.
        /// </summary>
        /// <remarks>IDictionary implementation calling the underlying Securities collection</remarks>
        public int Count
        {
            get
            {
                return Securities.Count;
            }
        }

        /// <summary>
        /// Check if the underlying securities array is read only.
        /// </summary>
        /// <remarks>IDictionary implementation calling the underlying Securities collection</remarks>
        public bool IsReadOnly
        {
            get
            {
                return Securities.IsReadOnly;
            }
        }

        /// <summary>
        /// Copy contents of the portfolio collection to a new destination.
        /// </summary>
        /// <remarks>IDictionary implementation calling the underlying Securities collection</remarks>
        /// <param name="array">Destination array</param>
        /// <param name="index">Position in array to start copying</param>
        public void CopyTo(KeyValuePair<string, SecurityHolding>[] array, int index)
        {
            array = new KeyValuePair<string, SecurityHolding>[Securities.Count];
            var i = 0;
            foreach (var asset in Securities)
            {
                if (i >= index)
                {
                    array[i] = new KeyValuePair<string, SecurityHolding>(asset.Key, asset.Value.Holdings);
                }
                i++;
            }
        }

        /// <summary>
        /// Symbol keys collection of the underlying assets in the portfolio.
        /// </summary>
        /// <remarks>IDictionary implementation calling the underlying securities key symbols</remarks>
        public ICollection<string> Keys
        {
            get
            {
                return Securities.Keys;
            }
        }

        /// <summary>
        /// Collection of securities objects in the portfolio.
        /// </summary>
        /// <remarks>IDictionary implementation calling the underlying securities values collection</remarks>
        public ICollection<SecurityHolding> Values
        {
            get
            {
                return (from asset in Securities.Values
                        select asset.Holdings).ToList();
            }
        }

        /// <summary>
        /// Attempt to get the value of the securities holding class if this symbol exists.
        /// </summary>
        /// <param name="symbol">String search symbol</param>
        /// <param name="holding">Holdings object of this security</param>
        /// <remarks>IDictionary implementation</remarks>
        /// <returns>Boolean true if successful locating and setting the holdings object</returns>
        public bool TryGetValue(string symbol, out SecurityHolding holding)
        {
            Security security;
            var success = Securities.TryGetValue(symbol, out security);
            holding = success ? security.Holdings : null;
            return success;
        }

        /// <summary>
        /// Get the enumerator for the underlying securities collection.
        /// </summary>
        /// <remarks>IDictionary implementation</remarks>
        /// <returns>Enumerable key value pair</returns>
        IEnumerator<KeyValuePair<string, SecurityHolding>> IEnumerable<KeyValuePair<string, SecurityHolding>>.GetEnumerator()
        {
            return Securities.GetInternalPortfolioCollection().GetEnumerator();
        }

        /// <summary>
        /// Get the enumerator for the underlying securities collection.
        /// </summary>
        /// <remarks>IDictionary implementation</remarks>
        /// <returns>Enumerator</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Securities.GetInternalPortfolioCollection().GetEnumerator();
        }

        #endregion

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/

        /// <summary>
        /// Sum of all currencies in account in US dollars
        /// </summary>
        /// <remarks>
        /// This should not be mistaken for margin available because Forex uses margin
        /// even though the total cash value is not impact
        /// </remarks>
        public decimal Cash
        {
            get { return CashBook.TotalValueInAccountCurrency; }
        }

        /// <summary>
        /// Absolute value of cash discounted from our total cash by the holdings we own.
        /// </summary>
        /// <remarks>When account has leverage the actual cash removed is a fraction of the purchase price according to the leverage</remarks>
        public decimal TotalUnleveredAbsoluteHoldingsCost
        {
            get
            {
                //Sum of unlevered cost of holdings
                return (from position in Securities.Values
                        select position.Holdings.UnleveredAbsoluteHoldingsCost).Sum();
            }
        }


        /// <summary>
        /// Absolute sum the individual items in portfolio.
        /// </summary>
        public decimal TotalHoldingsValue
        {
            get
            {
                //Sum sum of holdings
                return (from position in Securities.Values
                        select position.Holdings.AbsoluteHoldingsValue).Sum();
            }
        }

        /// <summary>
        /// Boolean flag indicating we have any holdings in the portfolio.
        /// </summary>
        /// <remarks>Assumes no asset can have $0 price and uses the sum of total holdings value</remarks>
        /// <seealso cref="Invested"/>
        public bool HoldStock
        {
            get { return TotalHoldingsValue > 0; }
        }


        /// <summary>
        /// Alias for HoldStock. Check if we have and holdings.
        /// </summary>
        /// <seealso cref="HoldStock"/>
        public bool Invested
        {
            get { return HoldStock; }
        }

        /// <summary>
        /// Get the total unrealised profit in our portfolio from the individual security unrealized profits.
        /// </summary>
        public decimal TotalUnrealisedProfit 
        {
            get
            {
                return (from position in Securities.Values
                        select position.Holdings.UnrealizedProfit).Sum();
            }
        }


        /// <summary>
        /// Get the total unrealised profit in our portfolio from the individual security unrealized profits.
        /// </summary>
        /// <remarks>Added alias for American spelling</remarks>
        public decimal TotalUnrealizedProfit
        {
            get { return TotalUnrealisedProfit; }
        }

        /// <summary>
        /// Total portfolio value if we sold all holdings at current market rates.
        /// </summary>
        /// <remarks>Cash + TotalUnrealisedProfit + TotalUnleveredAbsoluteHoldingsCost</remarks>
        /// <seealso cref="Cash"/>
        /// <seealso cref="TotalUnrealizedProfit"/>
        /// <seealso cref="TotalUnleveredAbsoluteHoldingsCost"/>
        public decimal TotalPortfolioValue
        {
            get
            {
                // we can't include forex in this calculation since we would be double account with respect to the cash book
                var totalUnrealizedProfitWithoutForex = (from position in Securities.Values
                                                         where position.Type != SecurityType.Forex
                                                         select position.Holdings.UnrealizedProfit).Sum();

                // we can't include forex in this calculation since we would be double account with respect to the cash book
                var totalHoldingsCostWithoutForex = (from position in Securities.Values
                                                     where position.Type != SecurityType.Forex
                                                     select position.Holdings.AbsoluteHoldingsCost).Sum();

                return CashBook.TotalValueInAccountCurrency + totalUnrealizedProfitWithoutForex + totalHoldingsCostWithoutForex;
            }
        }

        /// <summary>
        /// Total fees paid during the algorithm operation across all securities in portfolio.
        /// </summary>
        public decimal TotalFees 
        {
            get 
            {
                return (from position in Securities.Values
                        select position.Holdings.TotalFees).Sum();
            }
        }

        /// <summary>
        /// Sum of all gross profit across all securities in portfolio.
        /// </summary>
        public decimal TotalProfit 
        {
            get 
            {
                return (from position in Securities.Values
                        select position.Holdings.Profit).Sum();
            }
        }

        /// <summary>
        /// Total sale volume since the start of algorithm operations.
        /// </summary>
        public decimal TotalSaleVolume 
        {
            get 
            {
                return (from position in Securities.Values
                        select position.Holdings.TotalSaleVolume).Sum();
            }
        }

        /// <summary>
        /// Gets the total margin used across all securities in the account's currency
        /// </summary>
        public decimal TotalMarginUsed
        {
            get
            {
                return (from security in Securities.Values
                        select security.MarginModel.GetMaintenanceMargin(security)).Sum();
            }
        }

        /// <summary>
        /// Gets the remaining margin on the account in the account's currency
        /// </summary>
        public decimal MarginRemaining
        {
            get { return TotalPortfolioValue - TotalMarginUsed; }
        }

        /// <summary>
        /// Gets or sets the <see cref="MarginCallModel"/> for the portfolio. This
        /// is used to executed margin call orders.
        /// </summary>
        public MarginCallModel MarginCallModel { get; set; }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/

        /// <summary>
        /// Indexer for the PortfolioManager class to access the underlying security holdings objects.
        /// </summary>
        /// <param name="symbol">Search string symbol as indexer</param>
        /// <returns>SecurityHolding class from the algorithm securities</returns>
        public SecurityHolding this[string symbol]
        {
            get { return Securities[symbol].Holdings; }
            set { Securities[symbol].Holdings = value; }
        }

        /// <summary>
        /// Set the base currrency cash this algorithm is to manage.
        /// </summary>
        /// <param name="cash">Decimal cash value of portfolio</param>
        public void SetCash(decimal cash) 
        {
            _baseCurrencyCash.Quantity = cash;
        }

        /// <summary>
        /// Set the cash for the specified symbol
        /// </summary>
        /// <param name="symbol">The cash symbol to set</param>
        /// <param name="cash">Decimal cash value of portfolio</param>
        /// <param name="conversionRate">The current conversion rate for the</param>
        public void SetCash(string symbol, decimal cash, decimal conversionRate)
        {
            Cash item;
            if (CashBook.TryGetValue(symbol, out item))
            {
                item.Quantity = cash;
                item.ConversionRate = conversionRate;
            }
            else
            {
                CashBook.Add(symbol, cash, conversionRate);
            }
        }

        /// <summary>
        /// Calculate the new average price after processing a partial/complete order fill event. 
        /// </summary>
        /// <remarks>
        ///     For purchasing stocks from zero holdings, the new average price is the sale price.
        ///     When simply partially reducing holdings the average price remains the same.
        ///     When crossing zero holdings the average price becomes the trade price in the new side of zero.
        /// </remarks>
        public virtual void ProcessFill(OrderEvent fill)
        {
            var security = Securities[fill.Symbol];
            security.PortfolioModel.ProcessFill(this, security, fill);
        }


        /// <summary>
        /// Scan the portfolio and the updated data for a potential margin call situation which may get the holdings below zero! 
        /// If there is a margin call, liquidate the portfolio immediately before the portfolio gets sub zero.
        /// </summary>
        /// <returns>True for a margin call on the holdings.</returns>
        public List<Order> ScanForMarginCall()
        {
            // if we still have margin remaining then there's no need for a margin call
            if (MarginRemaining > 0)
            {
                return new List<Order>();
            }

            // generate a listing of margin call orders
            var marginCallOrders = new List<Order>();
            foreach (var security in Securities.Values)
            {
                // if the price is zero don't bother, we just don't have data for it
                if (security.Price == 0) continue;
                
                var marginCallOrder = security.MarginModel.GenerateMarginCallOrder(security, TotalPortfolioValue, TotalMarginUsed);
                if (marginCallOrder != null && marginCallOrder.Quantity != 0)
                {
                    marginCallOrders.Add(marginCallOrder);
                }
            }

            return marginCallOrders;
        }

        /// <summary>
        /// Applies a dividend to the portfolio
        /// </summary>
        /// <param name="dividend">The dividend to be applied</param>
        public void ApplyDividend(Dividend dividend)
        {
            var security = Securities[dividend.Symbol];

            // only apply dividends when we're in raw mode or split adjusted mode
            var mode = security.SubscriptionDataConfig.DataNormalizationMode;
            if (mode != DataNormalizationMode.Raw || mode != DataNormalizationMode.SplitAdjusted)
            {
                return;
            }

            // longs get benefits, shorts get clubbed on dividends
            var total = security.Holdings.Quantity*dividend.Distribution;

            // assuming USD, we still need to add Currency to the security object
            _baseCurrencyCash.Quantity += total;
        }

        /// <summary>
        /// Applies a split to the portfolio
        /// </summary>
        /// <param name="split">The split to be applied</param>
        public void ApplySplit(Split split)
        {
            var security = Securities[split.Symbol];

            // don't apply splits in adjusted modes
            var mode = security.SubscriptionDataConfig.DataNormalizationMode;
            if (mode == DataNormalizationMode.Adjusted || mode == DataNormalizationMode.SplitAdjusted)
            {
                return;
            }

            var quantity = security.Holdings.Quantity/split.SplitFactor;
            var avgPrice = security.Holdings.AveragePrice*split.SplitFactor;

            // we'll model this as a cash adjustment
            var leftOver = quantity - (int) quantity;
            var extraCash = leftOver*split.ReferencePrice;
            _baseCurrencyCash.Quantity += extraCash;

            security.Holdings.SetHoldings(avgPrice, (int) quantity);
        }

        /// <summary>
        /// Record the transaction value and time in a list to later be processed for statistics creation.
        /// </summary>
        /// <remarks>
        /// Bit of a hack -- but using datetime as dictionary key is dangerous as you can process multiple orders within a second.
        /// For the accounting / statistics generating purposes its not really critical to know the precise time, so just add a millisecond while there's an identical key.
        /// </remarks>
        /// <param name="time">Time of order processed </param>
        /// <param name="transactionProfitLoss">Profit Loss.</param>
        public void AddTransactionRecord(DateTime time, decimal transactionProfitLoss)
        {
            var clone = time;
            while (Transactions.TransactionRecord.ContainsKey(clone))
            {
                clone = clone.AddMilliseconds(1);
            }
            Transactions.TransactionRecord.Add(clone, transactionProfitLoss);
        }
    }

//End Algorithm Portfolio Class
} // End QC Namespace
