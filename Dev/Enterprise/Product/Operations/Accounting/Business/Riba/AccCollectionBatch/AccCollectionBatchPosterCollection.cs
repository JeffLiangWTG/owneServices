using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.Riba
{
	public class AccCollectionBatchPosterCollection : NonPersistentBusinessObjectCollection<AccCollectionBatchPoster>	{
		public AccCollectionBatchPosterCollection(BusinessObjectFactory factory, TransactionHeaderCollection transactionCollection, ZString createOption)
			: base(factory)
		{
			CreateOption = createOption;
			TransactionCollectionForDefault = transactionCollection;
			ReloadTransactionsInTheLocalFactory();
		}

		readonly ZString CreateOption;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AccCollectionBatchPoster(Factory);
		}

		#region Implementation

		readonly TransactionHeaderCollection TransactionCollectionForDefault;

		TransactionHeaderCollection ReloadedTransactionCollection
		{
			get
			{
				if (fReloadedTransactionCollection == null)
				{
					fReloadedTransactionCollection = new TransactionHeaderCollection(Factory);
				}
				return fReloadedTransactionCollection;
			}
		}
		TransactionHeaderCollection fReloadedTransactionCollection;

		void ReloadTransactionsInTheLocalFactory()
		{
			foreach (TransactionHeader transaction in TransactionCollectionForDefault)
			{
				Type typeOfTransaction = transaction.GetType();
				ReloadedTransactionCollection.Add(Factory.Load(transaction.GetType(), transaction.PK));
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (child is AccCollectionBatchPoster)
			{
				AccCollectionBatchPoster paymentBatchPoster = child as AccCollectionBatchPoster;
				paymentBatchPoster.CreateBatchOrdersAndFillByTransactions(ReloadedTransactionCollection, CreateOption);
			}
		}

		#endregion
	}
}
