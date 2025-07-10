using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Accounting.Business.Riba.AccCollectionOrder;

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionBatchPoster : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AccCollectionBatchPoster(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		AccCollectionBatch fBatch;
		public AccCollectionBatch Batch
		{
			get
			{
				if (fBatch == null)
				{
					fBatch = Factory.New<AccCollectionBatch>();
				}
				return fBatch;
			}
		}

		public static ZString CalculateCurrencyToUse(AccTransactionHeaderCollection transactionCollection)
		{
			ZString result = ZString.Empty;
			var transactions = transactionCollection.Cast<AccTransactionHeader>();
			if (transactions.Any(x => x.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency))
			{
				result = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
			else
			{
				if (transactions.Select(x => x.AH_RX_NKTransactionCurrency).Distinct().Count() > 1) // multiple foreign currency found, use local currency
				{
					result = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				}
				else // only 1 foreign currency
				{
					if (transactions.Any())
					{
						result = transactions.First().AH_RX_NKTransactionCurrency;
					}
					else
					{
						result = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					}
				}
			}
			return result;
		}

		void CreateOrders(IEnumerable<GroupedTransactions> transactions, ZString createOption)
		{
			foreach (var groupedTransactions in transactions)
			{
				var order = Batch.CollectionOrders.AddNew();
				order.ACO_ACB = fBatch.PK;
				if (createOption == AccountingConstants.CreateColletionOrdersBatchOption.GroupAllSelected)
				{
					order.DebtorValidationType = DebtorValidation.DebtorShouldBeEmpty;
				}
				else
				{
					order.DebtorValidationType = DebtorValidation.DebtorIsRequired;
				}
				order.ACO_OH_Debtor = groupedTransactions.OrgPK;
				order.ACO_CollectionDate = groupedTransactions.DueDate;
				foreach (var transaction in groupedTransactions.Transactions)
				{
					var line = order.CollectionOrderLines.AddNew();
					line.AOL_ACO = order.PK;
					line.AOL_AH = transaction.PK;
					line.IncludeInOrder = ZBool.True;
				}
				order.IncludeInBatch = ZBool.True;
			}
		}

		public void CreateBatchOrdersAndFillByTransactions(AccTransactionHeaderCollection transactionCollection, ZString createOption)
		{
			Batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			Batch.ACB_RX_NKCurrency = CalculateCurrencyToUse(transactionCollection);
			Batch.ACB_SystemLastEditTimeUtc = Batch.ACB_SystemCreateTimeUtc = Environment.Env.Time.CurrentUtcDateTime;
			ZDate today = ZDate.Today;

			if (createOption == AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtorAndDueDate)
			{
				// a. For transactions due today or earlier, group them in a single order per Debtor and set the Collection Date to be today.
				var transactionsDueEarlierThanToday = from t in transactionCollection.Cast<AccTransactionHeader>()
													  where t.AH_DueDate.Date <= today
													  group t by t.AH_OH into g
													  select new GroupedTransactions
													  {
														  OrgPK = g.Key,
														  DueDate = today,
														  Transactions = g.ToList()
													  };

				CreateOrders(transactionsDueEarlierThanToday, createOption);

				// b. For transactions due later than today, group them into multiple orders per Debtor and Due Date, and set the Collection Date equal the Transaction Due Date.
				var transactionsDueInFuture = from t in transactionCollection.Cast<AccTransactionHeader>()
											  where t.AH_DueDate.Date > today
											  group t by new
											  {
												  ah_oh = t.AH_OH,
												  dueDate = t.AH_DueDate.Date
											  } into g
											  select new GroupedTransactions
											  {
												  DueDate = g.Key.dueDate,
												  OrgPK = g.Key.ah_oh,
												  Transactions = g.ToList()
											  };

				CreateOrders(transactionsDueInFuture, createOption);
			}
			else if (createOption == AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtor)
			{
				var transactionsGroupByDebtor = from t in transactionCollection.Cast<AccTransactionHeader>()
												group t by t.AH_OH into g
												select new GroupedTransactions
												{
													DueDate = g.Max(x => x.AH_DueDate.Date),
													OrgPK = g.Key,
													Transactions = g.ToList()
												};

				CreateOrders(transactionsGroupByDebtor, createOption);
			}
			else if (createOption == AccountingConstants.CreateColletionOrdersBatchOption.GroupAllSelected)
			{
				var transactions = from t in transactionCollection.Cast<AccTransactionHeader>() select t;
				var transactionsGroupAllSelected = new[]
					{
						new GroupedTransactions()
						{
							DueDate = today,
							Transactions = transactions.ToList()
						}
					};

				CreateOrders(transactionsGroupAllSelected, createOption);
			}
		}

		class GroupedTransactions
		{
			public ZGuid OrgPK { get; set; }
			public ZDate DueDate { get; set; }
			public List<AccTransactionHeader> Transactions { get; set; }
		}
	}
}


