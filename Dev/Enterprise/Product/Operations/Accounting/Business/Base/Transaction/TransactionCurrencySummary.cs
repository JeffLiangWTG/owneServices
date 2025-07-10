using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionCurrencySummary : NonPersistentBusinessObject, IObsoleteValidation
	{
		public TransactionCurrencySummary(AccTransactionHeader[] transactions, BusinessObjectFactory factory)
			: base(factory)
		{
			fTransactions = transactions;
		}
		readonly AccTransactionHeader[] fTransactions;

		#region Properties

		public TransactionCurrencySummaryRowCollection TransactionCurrencySummaryRows
		{
			get
			{
				if (fTransactionCurrencySummaryRows == null)
				{
					fTransactionCurrencySummaryRows = new TransactionCurrencySummaryRowCollection(Factory);
					foreach (var transaction in fTransactions)
					{
						fTransactionCurrencySummaryRows.AddNewByCurrency((TransactionHeader)transaction);
					}
				}

				return fTransactionCurrencySummaryRows;
			}
		}
		TransactionCurrencySummaryRowCollection fTransactionCurrencySummaryRows;

		[List("Currencies")]
		public ZString LocalCurrency => GlbCompany.CurrentCompany.LocalCurrency.Code;

		public ZPropertyInfo LocalCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(LocalCurrency)); }
		}

		public RefCurrencyCollection Currencies
		{
			get
			{
				if (fCurrencies == null)
				{
					fCurrencies = new RefCurrencyCollection(Factory);
				}

				return fCurrencies;
			}
		}
		RefCurrencyCollection fCurrencies;

		public ZInt TransactionsCountTotal => TransactionCurrencySummaryRows.Cast<TransactionCurrencySummaryRow>().Sum(x => x.TransactionCount);

		public ZPropertyInfo TransactionsCountTotalInfo => GetZPropertyInfo(nameof(TransactionsCountTotal));

		public ZDate EarliestDueDate => NoEmptyDueDateTransactionHeaders.Any() ? NoEmptyDueDateTransactionHeaders.Min(y => y.AH_DueDate).Date : ZDate.Empty;

		public ZPropertyInfo EarliestDueDateInfo => GetZPropertyInfo(nameof(EarliestDueDate));

		public ZDate LatestDueDate => NoEmptyDueDateTransactionHeaders.Any() ? NoEmptyDueDateTransactionHeaders.Max(y => y.AH_DueDate).Date : ZDate.Empty;

		public ZPropertyInfo LatestDueDateDateInfo => GetZPropertyInfo(nameof(LatestDueDate));

		IEnumerable<TransactionHeader> NoEmptyDueDateTransactionHeaders => fTransactions.Cast<TransactionHeader>().Where(x => !x.AH_DueDate.IsEmpty);

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal LocalAmountTotal => TransactionCurrencySummaryRows.Cast<TransactionCurrencySummaryRow>().Sum(x => x.LocalAmount);

		public ZPropertyInfo LocalAmountTotalInfo => GetZPropertyInfo(nameof(LocalAmountTotal));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal OutStandingAmountTotal => TransactionCurrencySummaryRows.Cast<TransactionCurrencySummaryRow>().Sum(x => x.OutStandingLocalAmount);

		public ZPropertyInfo OutStandingAmountTotalInfo => GetZPropertyInfo(nameof(OutStandingAmountTotal));

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		#endregion
	}
}
