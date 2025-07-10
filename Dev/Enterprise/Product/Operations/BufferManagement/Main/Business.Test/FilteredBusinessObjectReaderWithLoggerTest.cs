using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	class FilteredBusinessObjectReaderWithLoggerTest : BMSTestCaseWithFactory
	{
		public void TestProcessOnEachBatch()
		{
			var system = CreateSystem("ORG");
			for (int i = 0; i < 10; i++)
			{
				CreateBuffer(system, "buffer" + (i + 1));
				CreateBucket(system, "bucket" + (i + 1));
			}

			Factory.Save();

			var query = new ZQuery(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.Buffer);

			BusinessObject lastBizoRead = null;
			IEnumerable<BMComponent> batch;

			var reader = new FilteredBusinessObjectReader(new BusinessObjectFactoryProvider(Factory), query, typeof(BMComponent)) { BatchSize = 5 };
			var batchNumber = 0;

			while ((batch = reader.LoadNextBatchInANewFactory(lastBizoRead).Cast<BMComponent>()).Any())
			{
				AssertEquals(5, batch.Count());
				foreach (var component in batch)
				{
					AssertEquals(BMComponentTypeList.Codes.Buffer, component.FC_Type);
				}

				batchNumber++;
				lastBizoRead = batch.LastOrDefault();
			}

			AssertEquals(2, batchNumber);
		}

		public void TestGetNextBatch()
		{
			var system = CreateSystem("ORG");
			for (int i = 0; i < 10; i++)
			{
				CreateBuffer(system, "buffer" + (i + 1));
				CreateBucket(system, "bucket" + (i + 1));
			}

			Factory.Save();

			var query = new ZQuery(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.Buffer);

			BusinessObject lastBizoRead = null;

			var reader = new FilteredBusinessObjectReader(new BusinessObjectFactoryProvider(Factory), query, typeof(BMComponent)) { BatchSize = 5 };

			var batch1 = reader.LoadNextBatchInANewFactory(lastBizoRead).Cast<BMComponent>();
			lastBizoRead = batch1.LastOrDefault();

			var batch2 = reader.LoadNextBatchInANewFactory(lastBizoRead).Cast<BMComponent>();
			lastBizoRead = batch2.LastOrDefault();

			var batch3 = reader.LoadNextBatchInANewFactory(lastBizoRead).Cast<BMComponent>();

			AssertEquals(5, batch1.Count());
			AssertEquals(5, batch2.Count());
			AssertEquals(0, batch3.Count());

			foreach (var component in batch1.Concat(batch2))
			{
				AssertEquals(BMComponentTypeList.Codes.Buffer, component.FC_Type);
			}
		}

		public void TestGetNextBatchWithParameters()
		{
			const int OtherTimeSpan = 3000;

			var system = CreateSystem("ORG");
			for (int i = 0; i < 15; i++)
			{
				int timeSpan = 5760;
				if (i % 2 == 0)
				{
					timeSpan = OtherTimeSpan;
				}
				CreateBuffer(system, "buffer" + (i + 1), timeSpan);
				CreateBucket(system, "bucket" + (i + 1), timeSpan);
			}

			Factory.Save();

			var query = new ZQuery(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.Buffer);
			query.AddToFilter(BMComponentSchema.FC_BufferTimespanInMinutes, OtherTimeSpan);

			BusinessObject lastBizoRead = null;

			var reader = new FilteredBusinessObjectReader(new BusinessObjectFactoryProvider(Factory), query, typeof(BMComponent)) { BatchSize = 5 };

			var batch1 = reader.LoadNextBatchInANewFactory(lastBizoRead).Cast<BMComponent>();
			lastBizoRead = batch1.LastOrDefault();

			var batch2 = reader.LoadNextBatchInANewFactory(lastBizoRead).Cast<BMComponent>();
			lastBizoRead = batch2.LastOrDefault();

			var batch3 = reader.LoadNextBatchInANewFactory(lastBizoRead).Cast<BMComponent>();

			AssertEquals(5, batch1.Count());
			AssertEquals(3, batch2.Count());
			AssertEquals(0, batch3.Count());

			foreach (var component in batch1.Concat(batch2))
			{
				AssertEquals(BMComponentTypeList.Codes.Buffer, component.FC_Type);
			}
		}

		public void TestOrderBy()
		{
			var enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry3 = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry1.O1_CompanyName = "A";
			enquiry2.O1_CompanyName = "B";
			enquiry3.O1_CompanyName = "C";

			Factory.Save();

			var query = new ZQuery(OrgColdCallRegisterSchema.O1_Address1, string.Empty) { OrderBy = "O1_CompanyName" };

			BusinessObject lastBizoRead = null;
			IEnumerable<SalesEnquiry> batch;

			var reader = new FilteredBusinessObjectReader(new BusinessObjectFactoryProvider(Factory), query, typeof(SalesEnquiry)) { BatchSize = 2 };

			batch = reader.LoadNextBatchInANewFactory(lastBizoRead).Cast<SalesEnquiry>();
			AssertArrayEqualsByElements(new[] { "A", "B" }, batch.Select(x => x.O1_CompanyName.ToString()).ToArray());

			query.OrderBy += " DESC";

			reader = new FilteredBusinessObjectReader(new BusinessObjectFactoryProvider(Factory), query, typeof(SalesEnquiry)) { BatchSize = 2 };
			batch = reader.LoadNextBatchInANewFactory(lastBizoRead).Cast<SalesEnquiry>();
			AssertArrayEqualsByElements(new[] { "C", "B" }, batch.Select(x => x.O1_CompanyName.ToString()).ToArray());
		}

		public void TestQueriesAllowTableValuedParameters()
		{
			for (int i = 0; i < 5; i++)
			{
				DummyBusinessObject dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				dummy.Z0_Description = "in_filter" + i;
			}
			Factory.Save();

			using (TestConnection.TrackExecutedCommands())
			{
				// Create a query without forced table value parameters
				var query = new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.StartsWith, "in_filter");
				var batchLogger = new BatchLogger(new TestServiceLogger(), "dummyLogger");

				// The reader should force TVP
				FilteredBusinessObjectReaderWithLogger reader = new FilteredBusinessObjectReaderWithLogger(new BusinessObjectFactoryProvider(), query, typeof(DummyBusinessObject), batchLogger) { BatchSize = 5 };

				AssertEquals(5, reader.Cast<DummyBusinessObject>().ToArray().Length);

				var executedCommand = TestConnection.ExecutedCommands.Single(c => c.Contains("TOP " + 5));

				AssertContains("SELECT Value FROM ", executedCommand);
				batchLogger.Flush();
			}
		}
	}
}
