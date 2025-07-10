
using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataExportBatch.Testing
{
	[TestedType(typeof(DataExportBatchDependentCollection))]
	class DataExportBatchDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoad_Headers()
		{
			var header1 = TestObjectCreator.CreateDataExportBatchSource_TransactionHeader();
			var batch1 = TestObjectCreator.CreateDataExportBatchForHeader(header1);
			var header2 = TestObjectCreator.CreateDataExportBatchSource_TransactionHeader();
			var batch2 = TestObjectCreator.CreateDataExportBatchForHeader(header2);

			Factory.Save();

			var collection1 = new DataExportBatchDependentCollection(header1);
			collection1.Load();
			AssertCollectionContains("collection1 should contain batch1", batch1, collection1);
			AssertCollectionNotContains("collection1 should not contain batch2", batch2, collection1);
			var collection2 = new DataExportBatchDependentCollection(header2);
			collection2.Load();
			AssertCollectionContains("collection2 should contain batch2", batch2, collection2);
			AssertCollectionNotContains("collection2 should not contain batch1", batch1, collection2);
		}

		public void TestLoad_Lines()
		{
			var line1 = TestObjectCreator.CreateDataExportBatchSource_TransactionLine();
			var batch1 = TestObjectCreator.CreateDataExportBatchForLine(line1);
			var line2 = TestObjectCreator.CreateDataExportBatchSource_TransactionLine();
			var batch2 = TestObjectCreator.CreateDataExportBatchForLine(line2);

			Factory.Save();

			var collection1 = new DataExportBatchDependentCollection(line1);
			collection1.Load();
			AssertCollectionContains("collection1 should contain batch1", batch1, collection1);
			AssertCollectionNotContains("collection1 should not contain batch2", batch2, collection1);
			var collection2 = new DataExportBatchDependentCollection(line2);
			collection2.Load();
			AssertCollectionContains("collection2 should contain batch2", batch2, collection2);
			AssertCollectionNotContains("collection2 should not contain batch1", batch1, collection2);
		}

		public void TestLoad_HeadersAndLines()
		{
			var header1 = TestObjectCreator.CreateDataExportBatchSource_TransactionHeader();
			var header1Line = TestObjectCreator.CreateDataExportBatchSource_TransactionLine((TransactionHeader)header1);
			var batch1 = TestObjectCreator.CreateDataExportBatchForHeader(header1);
			var batch2 = TestObjectCreator.CreateDataExportBatchForLine(header1Line);

			var header2 = TestObjectCreator.CreateDataExportBatchSource_TransactionHeader();
			var header2Line1 = TestObjectCreator.CreateDataExportBatchSource_TransactionLine((TransactionHeader)header2);
			var header2Line2 = TestObjectCreator.CreateDataExportBatchSource_TransactionLine((TransactionHeader)header2);
			var batch3 = TestObjectCreator.CreateDataExportBatchForLine(header2Line1);
			var batch4 = TestObjectCreator.CreateDataExportBatchForLine(header2Line2);

			Factory.Save();

			var collection1 = new DataExportBatchDependentCollection(header1);
			collection1.Load();
			AssertCollectionContains("collection1 should contain batch1", batch1, collection1);
			AssertCollectionContains("collection1 should contain batch2", batch2, collection1);
			AssertCollectionNotContains("collection1 should not contain batch3", batch3, collection1);
			AssertCollectionNotContains("collection1 should not contain batch4", batch4, collection1);

			var collection2 = new DataExportBatchDependentCollection(header2);
			collection2.Load();
			AssertCollectionContains("collection2 should contain batch3", batch3, collection2);
			AssertCollectionContains("collection2 should contain batch4", batch4, collection2);
			AssertCollectionNotContains("collection2 should not contain batch1", batch1, collection2);
			AssertCollectionNotContains("collection2 should not contain batch2", batch2, collection2);
		}

		public void TestLoad_ActualBatches()
		{
			Db.Connection.ExecuteNonQuery("ALTER TABLE dbo.GenExportBatchSequence NOCHECK CONSTRAINT CHK_XB_Type");
			try
			{
				var header = TestObjectCreator.CreateDataExportBatchSource_TransactionHeader();
				var headerLine = TestObjectCreator.CreateDataExportBatchSource_TransactionLine((TransactionHeader)header);
				var batch1 = TestObjectCreator.CreateDataExportBatchForHeader(header);
				var batch2 = TestObjectCreator.CreateDataExportBatchForLine(headerLine);
				// test with some unexpected XB_Type that can be added in the future
				batch1.XB_Type = batch2.XB_Type = "LOL";
				Factory.Save();

				var collection = new DataExportBatchDependentCollection(header);
				collection.Load();
				AssertCollectionNotContains("collection should not contain batch1", batch1, collection);
				AssertCollectionNotContains("collection should not contain batch2", batch2, collection);

				batch1.XB_Type = batch2.XB_Type = Enterprise.Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionHeaderExport;
				Factory.Save();

				collection.Load();
				AssertCollectionContains("collection should contain batch1", batch1, collection);
				AssertCollectionContains("collection should contain batch2", batch2, collection);
			}
			finally
			{
				Db.Connection.ExecuteNonQuery("ALTER TABLE dbo.GenExportBatchSequence CHECK CONSTRAINT CHK_XB_Type");
			}
		}

		public void TestAllowNew()
		{
			AssertEquals("Records should not get added; they are created elsewhere (i.e. during exports)", false, GetCollectionToTest().AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals("Records should not get removed", false, GetCollectionToTest().AllowRemove);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator TestObjectCreator;

		protected override Type GetExpectedCollectionType()
		{
			return typeof(DataExportBatchDependentCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var source = TestObjectCreator.CreateDataExportBatchSource_TransactionHeader();
			return new DataExportBatchDependentCollection(source);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<GenExportBatchSequence>();
		}

		#endregion
	}
}
