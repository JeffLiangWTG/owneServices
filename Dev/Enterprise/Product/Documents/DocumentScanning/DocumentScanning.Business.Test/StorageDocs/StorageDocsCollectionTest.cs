using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine;
using Enterprise.DocumentScanning.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageDocsCollection))]
	sealed class StorageDocsCollectionInFactory0Test : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			StorageDocsCollection collection = new StorageDocsCollection(MasterFactory);
			AssertEquals("AllowNew should not be allowed", false, collection.AllowNew);
		}

		public void TestImplementsIDeliverableCollection()
		{
			StorageDocsCollection docs = new StorageDocsCollection(MasterFactory);
			Assert("This collection must implement IDeliverableCollection for the DocDeliveryForm", typeof(IDeliverableCollection).IsAssignableFrom(docs.GetType()));
		}

		public void TestMasterFactory()
		{
			StorageDocsCollection factory0Collection = new StorageDocsCollection(MasterFactory);
			AssertEquals("StorageDocsCollection from main factory - master factory is same as instance passed in", MasterFactory, factory0Collection.MasterFactory);
			StorageDocsCollection factory1Collection = new StorageDocsCollection(FactoryOne);
			AssertEquals("StorageDocsCollection from factory one - master factory is the same as what retrieved FactoryOne", MasterFactory, factory1Collection.MasterFactory);
		}

		public void TestAddNewAddsParent()
		{
			StorageDocsCollection collection = new StorageDocsCollection(MasterFactory);
			AssertEquals("Collection count should be 0", 0, collection.Count);
			collection.AddNew();
			AssertEquals("Collection count should be 1", 1, collection.Count);
			AssertNotNull("New document in collection should have parent created for it", collection[0].ParentMain);
		}

		public void TestTypeOfElements()
		{
			StorageDocsCollection collection = new StorageDocsCollection(MasterFactory);
			AssertEquals("TypeOfElements", typeof(StorageDocs), collection.TypeOfElements);
		}

		public void TestLazyLoadImagesWorksForStorageDatabases()
		{
			BusinessObjectFactory dummyFactory = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = dummyFactory.New<DummyBusinessObject>();
			dummy1.Z0_Number = 1;
			dummy1.Z0_NVarChar = "Dummy1 Text";
			dummyFactory.Save();

			DocumentFactory docFactory1 = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			StorageMain main = docFactory1.New<StorageMain>();
			main.SM_DB = 1;
			main.SM_ParentFK = dummy1.PK;

			NumberedBusinessObjectFactory docFactoryForDB1 = docFactory1.GetFactory(1);
			StorageDocs doc1 = docFactoryForDB1.New<StorageDocs>();
			doc1.SC_SM = main.PK;
			doc1.SC_FileName = "testfile";
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				doc1.SC_ImageData = resourceRetriever.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");
			}

			docFactory1.Save();

			DocumentFactory docFactory2 = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			StorageMain mainLoaded = docFactory2.Load<StorageMain>(main.PK);

			AssertEquals("eDocs count", 1, mainLoaded.eDocs.Count);
			AssertEquals("PK", doc1.PK, mainLoaded.eDocs[0].PK);
			AssertEquals("ImageData length", doc1.SC_ImageData.Length, mainLoaded.eDocs[0].SC_ImageData.Length);
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return MasterFactory;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StorageDocsCollection(MasterFactory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			using (var auxConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbDropExisting(auxConnection, storageDocsDataBaseName);
			}
			FactoryOne = MasterFactory.GetFactory(1);
		}

		protected override void OnAfterBaseTestCaseRunBare()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, storageDocsDataBaseName);
			}

			base.OnAfterBaseTestCaseRunBare();
		}

		DocumentFactory MasterFactory
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
		NumberedBusinessObjectFactory FactoryOne;
		readonly string storageDocsDataBaseName = new DocManagerDBHelperTestClass().GetDatabaseName(1);
	}

	[TestedType(typeof(StorageDocsCollection))]
	public class StorageDocsCollectionInFactory1Test : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectFactory NewFactory()
		{
			return FactoryOne;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StorageDocsCollection(FactoryOne);
		}

		DocumentFactory MasterFactory
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

		NumberedBusinessObjectFactory FactoryOne
		{
			get { return MasterFactory.GetFactory(1); }
		}

		protected override void SetUp()
		{
			base.SetUp();

			using var auxConnection = Db.NewAdminConnection();
			AdoTestUtils.CreateDbDropExisting(auxConnection, storageDocsDataBaseName);
		}

		protected override void OnAfterBaseTestCaseRunBare()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, storageDocsDataBaseName);
			}

			base.OnAfterBaseTestCaseRunBare();
		}

		readonly string storageDocsDataBaseName = new DocManagerDBHelperTestClass().GetDatabaseName(1);
	}
}
