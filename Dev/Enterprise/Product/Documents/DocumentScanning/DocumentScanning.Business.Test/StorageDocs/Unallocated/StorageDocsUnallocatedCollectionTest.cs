using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageDocsUnallocatedCollection))]
	public class StorageDocsUnallocatedCollectionTest : BusinessObjectCollectionTestCase
	{
		protected new DocumentFactory Factory
		{
			get { return (DocumentFactory)base.Factory; }
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StorageDocsUnallocatedCollection(Factory);
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}

		/// <summary>
		/// This behaviour needs to stay the same in Z for my grid to work properly.
		/// </summary>
		public void TestLoadLeavesUnsavedBizOsInCollection()
		{
			new DocManagerDBHelperTestClass().LastWritableDatabaseWithFreeSpace(); //to ensure SD001 exists
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(new DocManagerDBHelperTestClass().GetTableNameWithDatabasePrefix(1, StorageDocsSchema.Constants.TableName));

			StorageDocsUnallocatedCollection originalCollection = new StorageDocsUnallocatedCollection(Factory);
			originalCollection.Load();
			StorageDocsUnallocated savedDoc = originalCollection.AddNew();
			AssertEquals("Collection's count should be 1 + collection count", 1, originalCollection.Count);
			Assert("SavedDoc is in the collection", originalCollection.Contains(savedDoc.PK));
			Factory.Save();

			StorageDocsUnallocated unsavedDoc = originalCollection.AddNew();
			AssertEquals("Collection's count should be 2 + collection count", 2, originalCollection.Count);
			Assert("UnsavedDoc is in the collection", originalCollection.Contains(unsavedDoc.PK));

			// save another storagedocs record in the db
			DocumentFactory separateFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			StorageDocsUnallocated outsideDoc = separateFactory.New<StorageDocsUnallocated>();
			separateFactory.Save();

			originalCollection.Factory.Save();
			originalCollection.Load();

			AssertEquals("Collection now has 3 + collection count elements", 3, originalCollection.Count);
			Assert("SavedDoc is in the collection", originalCollection.Contains(savedDoc.PK));
			Assert("UnsavedDoc is in the collection", originalCollection.Contains(unsavedDoc.PK));
			Assert("Outside doc is in the collection", originalCollection.Contains(outsideDoc.PK));
		}
	}
}
