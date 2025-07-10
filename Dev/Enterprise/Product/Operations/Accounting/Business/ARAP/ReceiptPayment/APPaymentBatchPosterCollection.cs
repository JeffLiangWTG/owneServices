using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class APPaymentBatchPosterCollection : BusinessObjectCollection<APPaymentBatchPoster>
	{
		public APPaymentBatchPosterCollection(BusinessObjectFactory factory, IEnumerable<ZGuid> pKs) : base(factory)
		{
			PKsForDefault = pKs;
			ReloadTransactionsInTheLocalFactory();
		}

		#region Implementation

		readonly IEnumerable<ZGuid> PKsForDefault;

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
			var query = new ZQuery(AccTransactionHeaderSchema.PK, PKsForDefault);
			query.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, SQLComparisonOperator.Equal, null);

			ReloadedTransactionCollection.AddRange(Factory.Load<TransactionHeader>(query));
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (child is APPaymentBatchPoster)
			{
				APPaymentBatchPoster paymentBatchPoster = child as APPaymentBatchPoster;
				paymentBatchPoster.SetDefaultValuesByTransactions(ReloadedTransactionCollection);
			}
		}

		#endregion
	}
}
