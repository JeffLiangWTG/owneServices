using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ClientAnalysisByPeriod : ScriptTest
	{
		[TestDate(2014, 11, 25)]
		public void TestClientAnalysisByPeriod()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("001");

			Job job1 = TestObjectCreator.CreateJob(shipment, false);
			Job job2 = TestObjectCreator.Job2;

			job1.JH_JobNum = "001";
			job2.JH_JobNum = "002";

			job2.JH_ParentTableCode = "TH";

			job1.JH_OA_LocalChargesAddr = TestObjectCreator.ABIGAS.MainAddress.PK;
			job2.JH_OA_LocalChargesAddr = TestObjectCreator.ABIGAS.MainAddress.PK;

			Factory.Save();

			DataTable clientAnalysisByPeriod = RunScript(job2.JH_SystemCreateTimeUtc);

			AssertEquals("Result for clientAnalysisByPeriod report", 1, clientAnalysisByPeriod.Rows.Count);

			var rows = clientAnalysisByPeriod.AsEnumerable().Where(r => ((string)r["OH_Code"]).TrimEnd() == TestObjectCreator.ABIGAS.OH_Code.TrimEnd());
			AssertNotNull("rows should not be null", rows);
			AssertEquals("1 row returned by where", 1, rows.Count());

			AssertEquals("Summary Count", 1, (int)rows.First()["Total"]);
			AssertEquals("Summary Count", 1, (int)rows.First()["P1"]);
		}

		DataTable RunScript(ZDateTime perdiodTo)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
EXEC Report_ClientAnalysisByPeriod
'{0}',	--@JH_GC
'{1}'	--@PeriodTo		     			
",
			GlbCompany.CurrentCompany.PK,
			PeriodCalculator.GetPeriodFromDate(perdiodTo)));
		}

		DataTable RunScriptWithOrder(ZDateTime perdiodTo, string orderBy)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
EXEC Report_ClientAnalysisByPeriod
'{0}',	--@JH_GC
'{1}',	--@PeriodTo
'I',
'C',	--Total is the Job Count
NULL,
NULL,
NULL,
NULL,
NULL,
NULL,
'{2}'	--@SortLevel
",
			GlbCompany.CurrentCompany.PK,
			PeriodCalculator.GetPeriodFromDate(perdiodTo),
			orderBy));
		}

		[ExpectNoExceptions]
		public void TestClientAnalysisByPeriodWithTwoYearHaveSamePeriodCount()
		{
			var year1 = new ZDateTime(2011, 01, 01);
			var year2 = new ZDateTime(2012, 01, 01);
			TestObjectCreator.CreateTestPeriods(year1, Core.Constants.ACPeriodFormat.Weeks);
			TestObjectCreator.CreateTestPeriods(year2, Core.Constants.ACPeriodFormat.Weeks);
			Factory.Save();
			var perdiodToA = new ZDateTime(2012, 06, 01);
			RunScript(perdiodToA);

			var year3 = new ZDateTime(2013, 01, 01);
			var year4 = new ZDateTime(2014, 01, 01);
			TestObjectCreator.CreateTestPeriods(year3, Core.Constants.ACPeriodFormat.Month);
			TestObjectCreator.CreateTestPeriods(year4, Core.Constants.ACPeriodFormat.Month);
			Factory.Save();
			var perdiodToB = new ZDateTime(2014, 06, 01);
			RunScript(perdiodToB);

			var year5 = new ZDateTime(2015, 01, 01);
			var year6 = new ZDateTime(2016, 01, 01);
			TestObjectCreator.CreateTestPeriods(year5, Core.Constants.ACPeriodFormat.FourWeeks);
			TestObjectCreator.CreateTestPeriods(year6, Core.Constants.ACPeriodFormat.FourWeeks);
			Factory.Save();
			var perdiodToC = new ZDateTime(2016, 06, 01);
			RunScript(perdiodToB);
		}

		[ExpectNoExceptions]
		public void TestClientAnalysisByPeriodWithTwoYearHaveDifferentPeriodCount()
		{
			var year1 = new ZDateTime(2011, 01, 01);
			var year2 = new ZDateTime(2012, 01, 01);
			TestObjectCreator.CreateTestPeriods(year1, Core.Constants.ACPeriodFormat.Month);
			TestObjectCreator.CreateTestPeriods(year2, Core.Constants.ACPeriodFormat.Weeks);
			Factory.Save();
			var perdiodToA = new ZDateTime(2012, 06, 01);
			RunScript(perdiodToA);

			var year3 = new ZDateTime(2013, 01, 01);
			var year4 = new ZDateTime(2014, 01, 01);
			TestObjectCreator.CreateTestPeriods(year1, Core.Constants.ACPeriodFormat.Month);
			TestObjectCreator.CreateTestPeriods(year2, Core.Constants.ACPeriodFormat.FourWeeks);
			Factory.Save();
			var perdiodToB = new ZDateTime(2014, 06, 01);
			RunScript(perdiodToB);

			var year5 = new ZDateTime(2015, 01, 01);
			var year6 = new ZDateTime(2016, 01, 01);
			TestObjectCreator.CreateTestPeriods(year1, Core.Constants.ACPeriodFormat.Weeks);
			TestObjectCreator.CreateTestPeriods(year2, Core.Constants.ACPeriodFormat.FourWeeks);
			Factory.Save();
			var perdiodToC = new ZDateTime(2016, 06, 01);
			RunScript(perdiodToC);
		}

		[TestDate(2014, 11, 25)]
		public void TestClientAnalysisByPeriod_JobInactive()
		{
			var shipment = TestObjectCreator.CreateShipment("001");
			var shipment2 = TestObjectCreator.CreateShipment("002");

			var job1 = TestObjectCreator.CreateJob(shipment, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			job1.JH_JobNum = "001";
			job2.JH_JobNum = "002";

			job2.MarkAsInactive();

			job1.JH_OA_LocalChargesAddr = TestObjectCreator.ABIGAS.MainAddress.PK;
			job2.JH_OA_LocalChargesAddr = TestObjectCreator.ABIGAS.MainAddress.PK;

			Factory.Save();

			var clientAnalysisByPeriod = RunScript(job2.JH_SystemCreateTimeUtc);

			AssertEquals("Result for clientAnalysisByPeriod report", 1, clientAnalysisByPeriod.Rows.Count);
		}

		[TestDate(2014, 11, 25)]
		public void TestClientAnalysisByPeriod_OrderBy()
		{
			var orgA = TestObjectCreator.CreateOrgHeader("AAA", true, true);
			var orgB = TestObjectCreator.CreateOrgHeader("BBB", true, true);
			var orgC = TestObjectCreator.CreateOrgHeader("CCC", true, true);
			var orgD = TestObjectCreator.CreateOrgHeader("DDD", true, true);

			CreateJob(orgB, "001");
			CreateJob(orgB, "002");
			CreateJob(orgB, "003");

			CreateJob(orgA, "004");
			CreateJob(orgA, "005");

			CreateJob(orgD, "006");
			CreateJob(orgD, "007");

			CreateJob(orgC, "008");

			Factory.Save();

			var clientAnalysisByPeriod = RunScriptWithOrder(ZDateTime.Today, "Total");
			AssertEquals("Result for clientAnalysisByPeriod report", 4, clientAnalysisByPeriod.Rows.Count);
			AssertContainsExactElementsInExactOrder("results are ordered by Total (Total is the number of jobs per org) and then org code",
				new[] { "ZBBB", "ZAAA", "ZDDD", "ZCCC" }, clientAnalysisByPeriod.Rows.Cast<DataRow>().Select(x => x["OH_Code"].ToString().Trim()));

			clientAnalysisByPeriod = RunScriptWithOrder(ZDateTime.Today, "XxX");
			AssertEquals("Result for clientAnalysisByPeriod report", 4, clientAnalysisByPeriod.Rows.Count);
			AssertContainsExactElementsInExactOrder("If SortLevel parameter is not 'Total', then order fallback to order by org code",
				new[] { "ZAAA", "ZBBB", "ZCCC", "ZDDD" }, clientAnalysisByPeriod.Rows.Cast<DataRow>().Select(x => x["OH_Code"].ToString().Trim()));

			void CreateJob(OrgHeader org, string jobNumber)
			{
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment(jobNumber), false);
				job.JH_JobNum = jobNumber;
				job.JH_OA_LocalChargesAddr = org.MainAddress.PK;
			}
		}
	}
}
