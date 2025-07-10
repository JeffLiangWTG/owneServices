using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageDocsCollectionView))]
	public class StorageDocsCollectionViewTest : BusinessObjectCollectionViewTestCase<StorageDocsCollectionView>
	{
		protected override BusinessObjectFactory NewFactory()
		{
			return MasterFactory;
		}

		protected override StorageDocsCollectionView GetCollectionToTest()
		{
			StorageMain main = MasterFactory.New<StorageMain>();
			return main.Documents;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return StorageDocs.New_DEBUG(MasterFactory);
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew should not be allowed on collection view", false, CollectionView.AllowNew);
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

		public void TestTypeOfElements()
		{
			AssertEquals("Type of elements", typeof(StorageDocs), CollectionView.TypeOfElements);
		}

		public void TestIsThisPartOfTheCollection()
		{
			Doc1.SC_IsDeleted = true;
			Doc2.SC_IsDeleted = true;

			AssertEquals("CollectionView should have 3 elements when excluding deleted docs by default", 3, CollectionView.Count);

			CollectionView.IncludeDeletedDocuments = true;
			AssertEquals("Collection should have all elements when including deleted docs", 5, CollectionView.Count);

			CollectionView.IncludeDeletedDocuments = false;
			Assert("Deleted doc - not part of collection", !CollectionView.IsThisPartOfTheCollectionExposed(Doc1));
			Assert("Deleted doc - not part of collection", !CollectionView.IsThisPartOfTheCollectionExposed(Doc2));
			Assert("Not deleted - part of collection", CollectionView.IsThisPartOfTheCollectionExposed(Doc3));
			Assert("Not deleted - part of collection", CollectionView.IsThisPartOfTheCollectionExposed(Doc4));
			Assert("Not deleted - part of collection", CollectionView.IsThisPartOfTheCollectionExposed(Doc5));

			Doc2.SC_IsDeleted = false;
			Doc2.SC_DataType = "PDF";
			AssertEquals("CollectionView should have 4 elements - even having PDF SC_DataType its still StorageDocs", 4, CollectionView.Count);

			Assert("Deleted doc - not part of collection", !CollectionView.IsThisPartOfTheCollectionExposed(Doc1));
			Assert("PDF storagedoc - part of collection", CollectionView.IsThisPartOfTheCollectionExposed(Doc2));
			Assert("Not deleted - part of collection", CollectionView.IsThisPartOfTheCollectionExposed(Doc3));
			Assert("Not deleted - part of collection", CollectionView.IsThisPartOfTheCollectionExposed(Doc4));
			Assert("Not deleted - part of collection", CollectionView.IsThisPartOfTheCollectionExposed(Doc5));
		}

		public void TestIncludeDeletedDocuments()
		{
			AssertEquals("Collection count should be all elements", 5, CollectionView.Count);
			Doc1.SC_IsDeleted = true;
			Doc2.SC_IsDeleted = true;

			AssertEquals("Collection view count is two less than collection count", 3, CollectionView.Count);

			CollectionView.IncludeDeletedDocuments = true;
			AssertEquals("Collection count should be all elements", 5, CollectionView.Count);
		}

		public void TestExcludeUnpublishedDocuments()
		{
			AssertEquals("Collection count should be all elements", 5, CollectionView.Count);

			Doc1.SC_IsPublished = false;
			Doc2.SC_IsPublished = false;
			Doc3.SC_IsPublished = true;
			Doc4.SC_IsPublished = true;
			Doc5.SC_IsPublished = true;

			AssertEquals("Collection count includes all - exclude unpublished should be false by default", 5, CollectionView.Count);

			CollectionView.ExcludeUnpublishedDocuments = true;
			AssertEquals("Collection count is two less", 3, CollectionView.Count);

			CollectionView.ExcludeUnpublishedDocuments = false;
			AssertEquals("Collection count includes all", 5, CollectionView.Count);
		}

		public void TestExcludeUnpublishedAndDeletedDocuments()
		{
			Doc1.SC_IsPublished = false;
			Doc1.SC_IsDeleted = true;
			Doc2.SC_IsPublished = false;
			Doc3.SC_IsPublished = true;
			Doc4.SC_IsPublished = true;
			Doc5.SC_IsPublished = true;
			Doc5.SC_IsDeleted = true;

			AssertEquals("Collection view count should be 3 - not including deleted docs by default", 3, CollectionView.Count);

			CollectionView.ExcludeUnpublishedDocuments = true;
			AssertEquals("Collection view count is 2 - not including deleted docs or unpublished docs", 2, CollectionView.Count);

			CollectionView.IncludeDeletedDocuments = true;
			AssertEquals("Collection view count is 3 - now including deleted docs", 3, CollectionView.Count);

			CollectionView.ExcludeUnpublishedDocuments = false;
			AssertEquals("Collection view count should equals collection count", 5, CollectionView.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			using (var auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbDropExisting(auxConnection, storageDocsDataBaseName);
			}

			TestCaseHelper.ClearTable(StorageDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StorageMainSchema.Constants.TableName);
			TestCaseHelper.ClearTable(storageDocsDataBaseName + ".dbo." + StorageDocsSchema.Constants.TableName);
			Main = MasterFactory.New<StorageMain>();
			CollectionView = Main.Documents;
			Doc1 = Main.Documents.AddNew();
			Doc2 = Main.Documents.AddNew();
			Doc3 = Main.Documents.AddNew();
			Doc4 = Main.Documents.AddNew();
			Doc5 = Main.Documents.AddNew();
		}

		protected override void OnAfterBaseTestCaseRunBare()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, storageDocsDataBaseName);
			}

			base.OnAfterBaseTestCaseRunBare();
		}

		protected DocumentFactory MasterFactory
		{
			get
			{
				if (fMasterFactory == null)
				{
					fMasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				}
				return fMasterFactory;
			}
		}
		DocumentFactory fMasterFactory;

		StorageDocsCollectionView CollectionView;
		StorageMain Main;
		StorageDocs Doc1;
		StorageDocs Doc2;
		StorageDocs Doc3;
		StorageDocs Doc4;
		StorageDocs Doc5;

		readonly string storageDocsDataBaseName = new DocManagerDBHelperTestClass().GetDatabaseName(1);
	}
}
