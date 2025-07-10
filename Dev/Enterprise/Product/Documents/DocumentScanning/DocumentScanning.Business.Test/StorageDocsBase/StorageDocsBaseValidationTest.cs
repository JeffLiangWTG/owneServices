using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageDocs))]
	public class StorageDocsBaseValidationTest : StorageDocsTest
	{
		#region Test Validate Index

		public void TestValidateIndex()
		{
			var tmpCollection = new EDocCollectionView(new DocumentPack(), new DocDeliveryContactCollection(Factory));

			var storageDocs1 = Factory.New<StorageDocs>();
			tmpCollection.Add(storageDocs1);
			var storageDocs2 = Factory.New<StorageDocs>();
			tmpCollection.Add(storageDocs2);
			var storageDocs3 = Factory.New<StorageDocs>();
			tmpCollection.Add(storageDocs3);
			var storageDocs4 = Factory.New<StorageDocs>();
			storageDocs4.IncludedInPrint = false;
			tmpCollection.Add(storageDocs4);

			AssertNoErrors(storageDocs1.IndexInfo);
			AssertNoWarnings(storageDocs1.IndexInfo);
			AssertNoErrors(storageDocs2.IndexInfo);
			AssertNoWarnings(storageDocs2.IndexInfo);
			AssertNoErrors(storageDocs3.IndexInfo);
			AssertNoWarnings(storageDocs3.IndexInfo);
			AssertNoErrors(storageDocs4.IndexInfo);
			AssertNoWarnings(storageDocs4.IndexInfo);

			storageDocs1.Index = 1;
			AssertNoErrors(storageDocs1.IndexInfo);
			AssertNoWarnings(storageDocs1.IndexInfo);
			AssertHasWarnings(storageDocs2.IndexInfo);
			AssertHasWarnings(storageDocs3.IndexInfo);
			AssertNoErrors(storageDocs4.IndexInfo);
			AssertNoWarnings(storageDocs4.IndexInfo);

			storageDocs2.Index = 1;
			AssertHasWarnings(storageDocs1.IndexInfo);
			AssertHasWarnings(storageDocs2.IndexInfo);
			AssertNoErrors(storageDocs3.IndexInfo);
			AssertNoWarnings(storageDocs3.IndexInfo);
			AssertNoErrors(storageDocs4.IndexInfo);
			AssertNoWarnings(storageDocs4.IndexInfo);

			storageDocs2.Index = 2;
			AssertNoErrors(storageDocs1.IndexInfo);
			AssertNoWarnings(storageDocs1.IndexInfo);
			AssertNoErrors(storageDocs2.IndexInfo);
			AssertNoWarnings(storageDocs2.IndexInfo);
			AssertNoErrors(storageDocs3.IndexInfo);
			AssertNoWarnings(storageDocs3.IndexInfo);
			AssertNoErrors(storageDocs4.IndexInfo);
			AssertNoWarnings(storageDocs4.IndexInfo);

			storageDocs3.Index = 1;
			AssertHasWarnings(storageDocs1.IndexInfo);
			AssertNoErrors(storageDocs2.IndexInfo);
			AssertNoWarnings(storageDocs2.IndexInfo);
			AssertHasWarnings(storageDocs3.IndexInfo);
			AssertNoErrors(storageDocs4.IndexInfo);
			AssertNoWarnings(storageDocs4.IndexInfo);

			storageDocs3.Index = 0;
			AssertNoErrors(storageDocs1.IndexInfo);
			AssertNoWarnings(storageDocs1.IndexInfo);
			AssertNoErrors(storageDocs2.IndexInfo);
			AssertNoWarnings(storageDocs2.IndexInfo);
			AssertNoErrors(storageDocs3.IndexInfo);
			AssertNoWarnings(storageDocs3.IndexInfo);
			AssertNoErrors(storageDocs4.IndexInfo);
			AssertNoWarnings(storageDocs4.IndexInfo);

			storageDocs4.IncludedInPrint = true;
			AssertNoErrors(storageDocs1.IndexInfo);
			AssertNoWarnings(storageDocs1.IndexInfo);
			AssertNoErrors(storageDocs2.IndexInfo);
			AssertNoWarnings(storageDocs2.IndexInfo);
			AssertHasWarnings(storageDocs3.IndexInfo);
			AssertHasWarnings(storageDocs4.IndexInfo);

			storageDocs3.IncludedInPrint = false;
			AssertNoErrors(storageDocs1.IndexInfo);
			AssertNoWarnings(storageDocs1.IndexInfo);
			AssertNoErrors(storageDocs2.IndexInfo);
			AssertNoWarnings(storageDocs2.IndexInfo);
			AssertNoErrors(storageDocs3.IndexInfo);
			AssertNoWarnings(storageDocs3.IndexInfo);
			AssertNoErrors(storageDocs4.IndexInfo);
			AssertNoWarnings(storageDocs4.IndexInfo);
		}

		#endregion

	}
}
