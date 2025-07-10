using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageDocsUnallocatedCollectionView))]
	public class StorageDocsUnallocatedCollectionViewTest : BusinessObjectCollectionViewTestCase<StorageDocsUnallocatedCollectionView>
	{
		protected new DocumentFactory Factory
		{
			get { return (DocumentFactory)base.Factory; }
		}

		protected override StorageDocsUnallocatedCollectionView GetCollectionToTest()
		{
			return new StorageDocsUnallocatedCollectionView(new StorageDocsUnallocatedCollection(Factory));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<StorageDocsUnallocated>();
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
		}
		public void TestReadOnly()
		{
			AssertEquals("should not be readonly by default", false, CollectionView.ReadOnly);

			CollectionView.CollectionToFilter.SetReadOnlyIncludingChildren(true);
			AssertEquals("Should now be readonly because filtered collection is readonly", true, CollectionView.ReadOnly);

			CollectionView.CollectionToFilter.SetReadOnlyIncludingChildren(false);
			AssertEquals("Should be back to not readonly", false, CollectionView.ReadOnly);

			CollectionView.SetReadOnlyIncludingChildren(true);
			AssertEquals("Should be readonly if SetReadOnlyIncludingChildren is called", true, CollectionView.ReadOnly);
		}

		protected override void SetUp()
		{
			base.SetUp();
			(new DocManagerDBHelper()).LastWritableDatabaseWithFreeSpace(); //to ensure SD001 exists
			Manager = new AllocateDocumentsManager(Factory);
			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable((new DocManagerDBHelperTestClass()).GetDatabaseName(1) + ".dbo." + StorageDocsSchema.Constants.TableName);

			CollectionView = new StorageDocsUnallocatedCollectionView(Manager.UnallocatedDocuments);
		}

		AllocateDocumentsManager Manager;

		StorageDocsUnallocatedCollectionView CollectionView;
	}
}
