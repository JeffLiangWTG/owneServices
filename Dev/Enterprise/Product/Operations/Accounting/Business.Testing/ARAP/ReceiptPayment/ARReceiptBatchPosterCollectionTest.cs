using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(ARReceiptBatchPosterCollection))]
	public class ARReceiptBatchPosterCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ARReceiptBatchPosterCollection>
	{
		protected override ARReceiptBatchPosterCollection GetCollectionToTest()
		{
			return new ARReceiptBatchPosterCollection(Factory);
		}

		public void TestIndexer()
		{
			ARReceiptBatchPosterCollection testCollection = GetCollectionToTest();
			ARReceiptBatchPoster batchPoster1 = (ARReceiptBatchPoster)GetNewElementToAddToTheCollection();
			testCollection.Add(batchPoster1);
			AssertEquals("Index 0", batchPoster1, testCollection[0]);

			ARReceiptBatchPoster batchPoster2 = (ARReceiptBatchPoster)GetNewElementToAddToTheCollection();
			testCollection.Add(batchPoster2);
			AssertEquals("Index 1", batchPoster2, testCollection[1]);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ARReceiptBatchPoster(Factory);
		}
	}
}
