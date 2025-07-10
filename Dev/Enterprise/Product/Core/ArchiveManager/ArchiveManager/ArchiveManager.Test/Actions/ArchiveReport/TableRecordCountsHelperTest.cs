using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Actions.ArchiveReport
{
	class TableRecordCountsHelperTest : TransactionedTestCase
	{
		public void TestGetSDTableRecordCounts_WhenStorageMainPointsToNonExistantSD()
		{
				var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

				var nonExistentSD444 = 444;
				DropDatabase(nonExistentSD444);

				var storageMain = documentFactory.New<StorageMain>();
				storageMain.SM_DB = nonExistentSD444;

				var storageReference = documentFactory.New<StorageReference>();
				storageReference.SR_SM = storageMain.PK;

				documentFactory.Save();

			AssertNoExceptionThrown(() => TableRecordCountsHelper.GetSDTableRecordCounts());
		}

		public void TestGetTableRecordCounts()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithDependentsBusinessObject>();
			_ = dummy.Dependents.AddNew();
			_ = dummy.Dependents.AddNew();

			factory.Save();

			var tableRecordCounts = TableRecordCountsHelper.GetTableRecordCounts();

			Assert(tableRecordCounts.ContainsKey(DummyBizoSchema.Constants.TableName));
			Assert(tableRecordCounts.ContainsKey(DummyDependentBizoSchema.Constants.TableName));

			AssertEquals(1, tableRecordCounts[DummyBizoSchema.Constants.TableName].RowCount);
			AssertEquals(2, tableRecordCounts[DummyDependentBizoSchema.Constants.TableName].RowCount);
		}

		void DropDatabase(int dbNumber)
		{
			if (dbHelper.DatabaseExists(dbNumber))
			{
				var dbName = dbHelper.GetDatabaseName(dbNumber);
				dbHelper.DropDatabase(dbName);
			}
		}

		readonly DocManagerDBHelperTestClass dbHelper = new();
	}
}
