using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Riba
{
	[TestedType(typeof(AccCollectionBatchPosterCollection))]
	public class AccCollectionBatchPosterCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AccCollectionBatchPosterCollection>
	{
		protected override AccCollectionBatchPosterCollection GetCollectionToTest()
		{
			return new AccCollectionBatchPosterCollection(Factory, new TransactionHeaderCollection(Factory), AccountingConstants.CreateColletionOrdersBatchOption.GroupByDebtorAndDueDate);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AccCollectionBatchPoster(Factory);
		}
	}
}
