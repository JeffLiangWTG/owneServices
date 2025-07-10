using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.StlCollector.Retriever;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Billing.StlCollector.Service.Testing
{
	[TestedType(typeof(WatermarkCleanupTask))]
	internal class WatermarkCleanupTaskTest : ServiceTaskTestCase<WatermarkCleanupTask>
	{
		[ExpectNoExceptions]
		public void TestRemoveOldWatermarksNotInActiveReferenceList()
		{
			var watermark = CreateWaterMark("LOL", ZDateTime.Now.AddMonths(-2));
			Factory.Save();

			var query = new ZQuery(StmDataSchema.SD_Name, "WaterBillDateLOL");
			var testTask = new WatermarkCleanupTask();
			var logger = InitialiseAndRunTaskSchedule(testTask);

			var deletedFactory = new BusinessObjectFactory();
			var results = deletedFactory.Load<StmData>(query);

			AssertWatermarkLogger(logger, 1);

			AssertEquals("Should have deleted entry as it is over 1 month old", 0, results.Length);
		}

		[ExpectNoExceptions]
		public void TestRemoveInValideDate()
		{
			var watermark = CreateWaterMark("LOL", ZDateTime.Invalid);
			Factory.Save();

			var query = new ZQuery(StmDataSchema.SD_Name, "WaterBillDateINV");
			var testTask = new WatermarkCleanupTask();
			var logger = InitialiseAndRunTaskSchedule(testTask);

			var deletedFactory = new BusinessObjectFactory();
			var results = deletedFactory.Load<StmData>(query);

			AssertWatermarkLogger(logger, 1);

			AssertEquals("Should delete the record with invalid date", 0, results.Length);
		}

		[ExpectNoExceptions]
		public void TestDontRemoveValidWatermarksNotInActiveList()
		{
			var watermark = CreateWaterMark("LOL", ZDateTime.Now.AddDays(-1));
			Factory.Save();

			var query = new ZQuery(StmDataSchema.SD_Name, "WaterBillDateLOL");
			var testTask = new WatermarkCleanupTask();
			var logger = InitialiseAndRunTaskSchedule(testTask);

			var deletedFactory = new BusinessObjectFactory();
			var results = deletedFactory.Load<StmData>(query);

			AssertWatermarkLogger(logger, 0);

			AssertEquals("Should have not deleted entry as it is not over 1 month old", 1, results.Length);
			AssertEquals("Should be same watermark", watermark.SD_Name, results.FirstOrDefault().SD_Name);
		}

		[ExpectNoExceptions]
		public void TestDontRemoveOldWatermarksInActiveReferenceList()
		{
			var watermark = CreateWaterMark("LOL", ZDateTime.Now.AddMonths(-2));
			Factory.Save();

			var testList = new ListObject();
			testList.Add(new DummyScriptItem(isActive: true, "LOL"));

			using (ObjectFactory.Substitute("StlCustomCollectorsList", testList))
			{
				var query = new ZQuery(StmDataSchema.SD_Name, "WaterBillDateLOL");
				var testTask = new WatermarkCleanupTask();
				var logger = InitialiseAndRunTaskSchedule(testTask);

				var deletedFactory = new BusinessObjectFactory();
				var results = deletedFactory.Load<StmData>(query);

				AssertWatermarkLogger(logger, 0);

				AssertEquals("Should not have deleted entry as it is in the active reference list", 1, results.Length);
				AssertEquals("Should be same watermark", watermark.SD_Name, results.FirstOrDefault().SD_Name);
			}
		}

		[ExpectNoExceptions]
		public void TestMultipleWatermarkCleanup()
		{
			var watermarkLOL = CreateWaterMark("LOL", ZDateTime.Now.AddDays(-2));
			var watermarkFTP = CreateWaterMark("FTP", ZDateTime.Now.AddMonths(-2));
			var watermarkCPY = CreateWaterMark("CPY", ZDateTime.Now.AddMonths(-2));
			var watermarkMFT = CreateWaterMark("MFT", ZDateTime.Now.AddMonths(-2));
			var watermarkPOP = CreateWaterMark("POP", ZDateTime.Now.AddDays(-2));

			var testList = new ListObject();
			testList.Add(new DummyScriptItem(isActive: true, "LOL"));
			testList.Add(new DummyScriptItem(isActive: true, "FTP"));

			using (ObjectFactory.Substitute("StlCustomCollectorsList", testList))
			{
				var stlCollectors = ObjectFactory.Get<IScriptFactory>().CreateScripts(new BusinessObjectFactory() { RefreshEnabled = false }).ToDictionary(f => f.Code.ToUpper());
				
				var testTask = new WatermarkCleanupTask();
				var logger = InitialiseAndRunTaskSchedule(testTask);

				var query = new ZQuery(StmDataSchema.SD_Name, "WaterBillDateLOL");
				query.AddToFilter(new ZQuery(StmDataSchema.SD_Name, "WaterBillDateFTP"), JoinCondition.Or);
				query.AddToFilter(new ZQuery(StmDataSchema.SD_Name, "WaterBillDateCPY"), JoinCondition.Or);
				query.AddToFilter(new ZQuery(StmDataSchema.SD_Name, "WaterBillDateMFT"), JoinCondition.Or);
				query.AddToFilter(new ZQuery(StmDataSchema.SD_Name, "WaterBillDatePOP"), JoinCondition.Or);
				var deletedFactory = new BusinessObjectFactory();
				var results = deletedFactory.Load<StmData>(query);

				AssertWatermarkLogger(logger, 2);

				AssertEquals("Should have deleted all old watermarks", 3, results.Length);
				Assert("This watermark (LOL) should not have been deleted.", results.Any(x => x.SD_Name == "WaterBillDateLOL"));
				Assert("This watermark (FTP) should not have been deleted.", results.Any(x => x.SD_Name == "WaterBillDateFTP"));
				Assert("This watermark (POP) should not have been deleted.", results.Any(x => x.SD_Name == "WaterBillDatePOP"));
				Assert("This watermark (CPY) should be deleted.", !results.Any(x => x.SD_Name == "WaterBillDateCPY"));
				Assert("This watermark (MFT) should be deleted.", !results.Any(x => x.SD_Name == "WaterBillDateMFT"));
			}
		}

		void AssertWatermarkLogger(TestServiceLogger logger, int deleted)
		{
			AssertEquals("4 log entries", 4, logger.Count);
			AssertEquals("Information|[WCS Watermark Cleanup] - Start", logger[0]);
			AssertEquals("Information|Removing all Collector Watermarks over 1 month old, and is not in the active collector list.", logger[1]);
			AssertEquals(deleted == 0 ? "Information|No Watermarks removed" : $"Information|Removed {deleted} Watermarks", logger[2]);
			AssertEquals("Information|[WCS Watermark Cleanup] - End", logger[3]);
		}

		StmData CreateWaterMark(string code, ZDateTime date)
		{
			var watermark = Factory.New<StmData>();
			watermark.SD_Name = $"WaterBillDate{code}";
			watermark.SD_BinaryValue = GetEncodedWaterBillDate(date);
			Factory.Save();
			return watermark;
		}

		byte[] GetEncodedWaterBillDate(ZDateTime date)
		{
			return Encoding.Unicode.GetBytes(date.SqlFormat.ToString());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
