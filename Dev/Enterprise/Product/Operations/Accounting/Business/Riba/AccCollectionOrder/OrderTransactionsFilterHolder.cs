using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.Riba
{
	public class OrderTransactionsFilterHolder : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrderTransactionsFilterHolder(AccCollectionOrder order)
			: base(order.Factory)
		{
			relatedOrder = order;
		}

		public AccCollectionOrder RelatedOrder
		{
			get
			{
				return relatedOrder;
			}
		}
		readonly AccCollectionOrder relatedOrder;

		public TransactionHeaderCollection Transactions
		{
			get
			{
				if (fTransactions == null)
				{
					fTransactions = new TransactionHeaderCollection(Factory);
					fTransactions.SetReadOnlyIncludingChildren(true);
				}
				return fTransactions;
			}
		}
		protected TransactionHeaderCollection fTransactions;

		AddTransactionsToOrderFilterBusinessObject fAddTransactionsToOrderFilter;
		public AddTransactionsToOrderFilterBusinessObject AddTransactionsToOrderFilter
		{
			get
			{
				if (fAddTransactionsToOrderFilter == null)
				{
					fAddTransactionsToOrderFilter = new AddTransactionsToOrderFilterBusinessObject(RelatedOrder);
				}
				return fAddTransactionsToOrderFilter;
			}
		}

		public void AddTransactionsIntoOrder(TransactionHeaderCollection selectedTransactions)
		{
			if (RelatedOrder != null && selectedTransactions.Count > 0)
			{
				foreach (var transaction in selectedTransactions)
				{
					AccCollectionOrderLine line = RelatedOrder.CollectionOrderLines.AddNew();
					line.AOL_ACO = RelatedOrder.PK;
					line.AOL_AH = transaction.PK;
					line.IncludeInOrder = true;
				}
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				RelatedOrder.CollectionBatch.Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "New transactions added to Order Number {0}.", RelatedOrder.ACO_OrderNumber));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		public ZString CheckAnyTransactionsUsedByActiveOrderLine(TransactionHeaderCollection selectedTransactions)
		{
			ZString result = ZString.Empty;
			if (RelatedOrder != null && selectedTransactions.Count > 0)
			{
				foreach (TransactionHeader transaction in selectedTransactions)
				{
					if (transaction.IsUsedByActiveCollectionOrderLine)
					{
						result = transaction.AH_TransactionNum;
						break;
					}
				}
			}
			return result;
		}

		public ZBool CheckAnyTransactionsHaveInvalidDueDate(TransactionHeaderCollection selectedTransactions)
		{
			return RelatedOrder != null &&
					selectedTransactions.Count > 0 &&
					selectedTransactions.Cast<TransactionHeader>().Any(x => x.AH_DueDate.Date > RelatedOrder.ACO_CollectionDate);
		}
	}
}


