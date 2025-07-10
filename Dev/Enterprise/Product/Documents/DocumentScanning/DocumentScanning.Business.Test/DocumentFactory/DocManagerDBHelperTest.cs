using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Testing
{
	sealed class DocManagerDBHelperTest : TestCaseWithFactory
	{
		public void TestCanNotAcquireLockForCreatingSDDatabaseException()
		{
			var helper = new DocManagerDBHelperTestClassUsingRegistry();
			var dbNumbers = helper.GetStorageDocDbNumbersIncludingMainDb().ToList();
			dbNumbers.ForEach(d =>
			{
				if (d != 0)
				{
					Db.Connection.AlterDbWriteableStateForDocManager(helper.GetDatabaseName(d), false);
				}
			});

			using (var connection1 = Db.NewExtraConnectionToMainDb())
			{
				Assert(connection1.TryGetLock(MutexIDs.StorageDocsDBBeingCreated.Name, out var dbLock));
				Assert(dbLock.IsHoldingLock());

				try
				{
					var stopWatch = new Stopwatch();
					stopWatch.Start();
					AssertExceptionThrown<CanNotAcquireLockForCreatingSDDatabaseException>(() => helper.LastWritableDatabaseWithFreeSpace());
					stopWatch.Stop();

					var seconds = stopWatch.Elapsed.Seconds;
					AssertGreaterThan(seconds, 1);
				}
				finally
				{
					dbLock.Dispose();
				}
			}
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return MasterFactory;
		}

		public void TestGetTableNameWithDatabasePrefix()
		{
			DocManagerDBHelper helper = new DocManagerDBHelper();
			string baseDBName = helper.GetDatabaseName(0);
			string dbName = helper.GetTableNameWithDatabasePrefix(1, StorageDocsSchema.Constants.TableName);
			AssertEquals("DB name should come out with database.dbo.tablename format", baseDBName + "_SD001.dbo.StorageDocs", dbName);

			dbName = helper.GetTableNameWithDatabasePrefix(0, StorageMainSchema.Constants.TableName);
			AssertEquals("Db name should come out with database.dbo.tablename format", baseDBName + ".dbo.StorageMain", dbName);
		}

		public void TestGetDatabaseName()
		{
			AssertEquals(MasterDBName + "_SD001", DbHelper.GetDatabaseName(1));
		}

		public void TestDatabaseExists()
		{
			Assert("database 1 should exist", DbHelper.DatabaseExists(1));
			Assert("database 50 shouldn't exist", !DbHelper.DatabaseExists(50));
		}

		[ExpectException(typeof(OdysseyException))]
		public void TestDatabaseExistsWithDBOutOfBounds()
		{
			Assert("database 1000 definitely shouldn't exist", !DbHelper.DatabaseExists(1000));
		}

		public void TestSavingStorageMainWithEmptyFKShouldBlow()
		{
			StorageMain record1 = MasterFactory.New<StorageMain>();
			try
			{
				MasterFactory.Save();
				Fail("Should have thrown an exception");
			}
			catch (Exception ex)
			{
				while (ex.InnerException != null)
				{
					ex = ex.InnerException;
				}
				AssertContains("Cannot insert duplicate key row in object 'dbo.StorageMain' with unique index 'NR_UC__SM_ParentFK'.", ex.Message);
			}
		}

		public void TestSavingTwoStorageMainWithSameFKSHouldBlow()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "E");
			OrgHeader org = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			StorageMain record1 = MasterFactory.New<StorageMain>();
			StorageMain record2 = MasterFactory.New<StorageMain>();
			record1.SM_ParentFK = org.PK;
			record2.SM_ParentFK = org.PK;
			record1.SM_DB = 1;
			record2.SM_DB = 1;

			try
			{
				MasterFactory.Save();
				Fail("Should have thrown an exception");
			}
			catch (Exception ex)
			{
				while (ex.InnerException != null)
				{
					ex = ex.InnerException;
				}
				AssertContains("Cannot insert duplicate key row in object 'dbo.StorageMain' with unique index 'NR_UC__SM_ParentFK'.", ex.Message);
			}
		}

		public void TestGetDBNamesMissing()
		{
			DocManagerDBHelper helper = new DocManagerDBHelper();
			string[] missingDBs = helper.GetDBNamesMissing();
			AssertEquals("Should be no missing DBs", 0, missingDBs.Length);
			Assert("Precondition: db " + helper.GetDatabaseName(3) + " should not exist", !helper.DatabaseExists(3));
			Assert("Precondition: db " + helper.GetDatabaseName(4) + " should not exist", !helper.DatabaseExists(4));

			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "E");
			OrgHeader org1 = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "F");
			OrgHeader org2 = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			StorageMain mainInDB3 = MasterFactory.New<StorageMain>();
			mainInDB3.SM_ParentFK = org1.PK;
			mainInDB3.SM_DB = 3;

			StorageMain mainInDB4 = MasterFactory.New<StorageMain>();
			mainInDB4.SM_DB = 4;
			mainInDB4.SM_ParentFK = org2.PK;
			MasterFactory.Save();

			missingDBs = DbHelper.GetDBNamesMissing();
			AssertEquals("Should contain 2 listings", 2, missingDBs.Length);
			Assert("DB names should be contained in the list of missing dbs", ((IList)missingDBs).Contains(MasterDBName + "_SD003"));
			Assert("DB names should be contained in the list of missing dbs", ((IList)missingDBs).Contains(MasterDBName + "_SD004"));

			mainInDB3.Delete();
			mainInDB4.Delete();
			MasterFactory.Save();

			missingDBs = DbHelper.GetDBNamesMissing();
			AssertEquals("Should be no missing DBs", 0, missingDBs.Length);
		}

		public void TestCreateDatabase_Raw_CheckMutexNotLockedException()
		{
			DocManagerDBHelperTestClass docManagerDBHelper = new DocManagerDBHelperTestClass();
			using (ZGlobalMutex testMutex = new ZGlobalMutex(MutexIDs.StorageDocsDBBeingCreated))
			{
				testMutex.Lock();
				AssertNoExceptionThrown(() => docManagerDBHelper.CreateDatabase(1));
			}
		}

		#region TestEnableMainUserLoginOnNewDatabase

		public void TestEnableMainUserLoginOnNewDatabase()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				DocManagerDBHelperTestClass helper = new DocManagerDBHelperTestClass();
				string newDBName = null;
				try
				{
					const int newDBNumber = 12;
					Assert("Precondition: DB " + newDBNumber + " Doesn't exist", !(helper.DatabaseExists(newDBNumber)));
					newDBName = helper.GetDatabaseName(helper.CreateDatabase(newDBNumber));
					AdminConnectionNonTransactionalTest.AssertAllLoginsAreMappedToDatabase(adminConnection, newDBName);
				}
				finally
				{
					if (!string.IsNullOrEmpty(newDBName))
					{
						helper.DropDatabase(newDBName);
					}
				}
			}
		}

		#endregion

		public void TestCreateDatabase_WhenMutexIsNotTaken_DoNothing()
		{
			const int TestDbNo = 111;

			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				SqlApplicationLock dbLock;
				var locked = extraConnection.TryGetLock(MutexIDs.StorageDocsDBBeingCreated.Name, out dbLock);
				using (dbLock)
				{
					Assert("Should get lock in extra connection.", locked);

					var helper = new DocManagerDBHelperTestClass();

					Assert($"Precondition: database {TestDbNo} should not exist", !helper.DatabaseExists(TestDbNo));

					try
					{
						AssertEquals("Should not create database", -1, helper.CreateDatabase(TestDbNo));
						Assert($"Postcondition: database {TestDbNo} should not exist", !helper.DatabaseExists(TestDbNo));
					}
					finally
					{
						if (helper.DatabaseExists(TestDbNo))
						{
							helper.DropDatabase(helper.GetDatabaseName(TestDbNo));
						}
					}
				}
			}
		}

		public void TestCreateDatabase_WhenAlreadyExists_DoNothing()
		{
			const int TestDbNo = 111;
			var helper = new DocManagerDBHelperTestClass();

			Assert($"Precondition: database {TestDbNo} should not exist", !helper.DatabaseExists(TestDbNo));

			try
			{
				AssertEquals("Should create database", TestDbNo, helper.CreateDatabase(TestDbNo));
				Assert($"Postcondition: database {TestDbNo} should exist", helper.DatabaseExists(TestDbNo));

				AssertEquals("Should return successful result when db already exists (but not try to do anything to it)", 111, helper.CreateDatabase(TestDbNo));
				Assert($"Database {TestDbNo} should still exist", helper.DatabaseExists(TestDbNo));
			}
			finally
			{
				if (helper.DatabaseExists(TestDbNo))
				{
					helper.DropDatabase(helper.GetDatabaseName(TestDbNo));
				}
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DbHelper = new DocManagerDBHelper();
			MasterDBName = Db.DatabaseName.Trim();

			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A");
			Org1 = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B");
			Org2 = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C");
			Org3 = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			filter = new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "D");
			Org4 = MasterFactory.LoadTop1(typeof(OrgHeader), filter) as OrgHeader;

			var smallTiffBytes = resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");
			ParentFirstSet = MasterFactory.New<StorageMain>();
			ParentFirstSet.SM_DB = 1;
			ParentFirstSet.SM_ParentFK = Org1.PK;
			ParentFirstSet.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1FirstSet = ParentFirstSet.Documents.AddNew();
			Doc1FirstSet.SC_IsDeleted = true;
			Doc1FirstSet.SC_ImageData = smallTiffBytes;
			Doc2FirstSet = ParentFirstSet.Documents.AddNew();
			Doc2FirstSet.SC_IsDeleted = true;
			Doc2FirstSet.SC_ImageData = smallTiffBytes;
			Doc3FirstSet = ParentFirstSet.Documents.AddNew();
			Doc3FirstSet.SC_ImageData = smallTiffBytes;

			ParentSecondSet = MasterFactory.New<StorageMain>();
			ParentSecondSet.SM_DB = 1;
			ParentSecondSet.SM_ParentFK = Org2.PK;
			ParentSecondSet.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1SecondSet = ParentSecondSet.Documents.AddNew();
			Doc1SecondSet.SC_IsDeleted = true;
			Doc1SecondSet.SC_ImageData = smallTiffBytes;
			Doc2SecondSet = ParentSecondSet.Documents.AddNew();
			Doc2SecondSet.SC_IsDeleted = true;
			Doc2SecondSet.SC_ImageData = smallTiffBytes;
			Doc3SecondSet = ParentSecondSet.Documents.AddNew();
			Doc3SecondSet.SC_ImageData = smallTiffBytes;

			ParentThirdSet = MasterFactory.New<StorageMain>();
			ParentThirdSet.SM_DB = 1;
			ParentThirdSet.SM_ParentFK = Org3.PK;
			ParentThirdSet.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1ThirdSet = ParentThirdSet.Documents.AddNew();
			Doc1ThirdSet.SC_IsDeleted = true;
			Doc1ThirdSet.SC_ImageData = smallTiffBytes;
			Doc2ThirdSet = ParentThirdSet.Documents.AddNew();
			Doc2ThirdSet.SC_IsDeleted = true;
			Doc2ThirdSet.SC_ImageData = smallTiffBytes;
			Doc3ThirdSet = ParentThirdSet.Documents.AddNew();
			Doc3ThirdSet.SC_ImageData = smallTiffBytes;

			ParentFourthSet = MasterFactory.New<StorageMain>();
			ParentFourthSet.SM_DB = 1;
			ParentFourthSet.SM_ParentFK = Org4.PK;
			ParentFourthSet.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			Doc1FourthSet = ParentFourthSet.Documents.AddNew();
			Doc1FourthSet.SC_IsDeleted = true;
			Doc1FourthSet.SC_ImageData = smallTiffBytes;
			Doc2FourthSet = ParentFourthSet.Documents.AddNew();
			Doc2FourthSet.SC_IsDeleted = true;
			Doc2FourthSet.SC_ImageData = smallTiffBytes;
			Doc3FourthSet = ParentFourthSet.Documents.AddNew();
			Doc3FourthSet.SC_ImageData = smallTiffBytes;

			ParentUnallocatedSet = MasterFactory.New<StorageMain>();
			ParentUnallocatedSet.SM_Type = Core.Constants.DocManagerCodes.Organisation;
			ParentUnallocatedSet.SM_DB = 1;
			Doc1UnallocatedSet = ParentUnallocatedSet.Documents.AddNew();
			Doc1UnallocatedSet.SC_ImageData = smallTiffBytes;

			MasterFactory.Save();
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			DocManagerDBHelperTestClass helper = new DocManagerDBHelperTestClass();
			if (!helper.DatabaseExists(1))
			{
				helper.CreateDatabase(1);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

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

		DocManagerDBHelper DbHelper;
		string MasterDBName;
		OrgHeader Org1;
		OrgHeader Org2;
		OrgHeader Org3;
		OrgHeader Org4;

		StorageMain ParentFirstSet;
		StorageDocs Doc1FirstSet;
		StorageDocs Doc2FirstSet;
		StorageDocs Doc3FirstSet;

		StorageMain ParentSecondSet;
		StorageDocs Doc1SecondSet;
		StorageDocs Doc2SecondSet;
		StorageDocs Doc3SecondSet;

		StorageMain ParentThirdSet;
		StorageDocs Doc1ThirdSet;
		StorageDocs Doc2ThirdSet;
		StorageDocs Doc3ThirdSet;

		StorageMain ParentFourthSet;
		StorageDocs Doc1FourthSet;
		StorageDocs Doc2FourthSet;
		StorageDocs Doc3FourthSet;

		StorageMain ParentUnallocatedSet;
		StorageDocs Doc1UnallocatedSet;

		#endregion
	}
}
