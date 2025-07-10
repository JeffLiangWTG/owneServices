using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class csfn_AllJobProfitDetailCoreTest : JobProfitFilterTest
	{
		protected override bool ShouldTestActiveStatusFilter => false;

		public void Testcsfn_AllJobProfitDetailCoreWhenPostedUnPostedIsNull()
		{
			var wip1 = Factory.NewWithValidTestData<WIP>();
			var charge = Factory.NewWithValidTestData<BaseCharge>();
			wip1.AL_JH = charge.Job.PK;
			wip1.AL_PostDate = new ZDateTime(2011, 2, 15);
			wip1.AL_AG = TestObjectCreator.GLHeader1.PK;
			wip1.AL_LineAmount = 1;
			wip1.AL_OSAmount = 1;
			charge.JR_AL_ARLine = wip1.PK;
			charge.JR_LocalCostAmt = 1;

			var wip2 = Factory.NewWithValidTestData<WIP>();
			var charge2 = Factory.NewWithValidTestData<BaseCharge>();
			wip2.AL_JH = charge2.Job.PK;
			wip2.AL_PostDate = new ZDateTime(2011, 2, 25);
			wip2.AL_ReverseDate = new ZDateTime(2011, 2, 15);
			wip2.AL_LineAmount = 1;
			wip2.AL_OSAmount = 1;
			wip2.AL_AG = TestObjectCreator.GLHeader1.PK;

			var arLine = TestObjectCreator.CreateCostLine(charge, Factory.NewWithValidTestData<APInvoice>().PK);
			arLine.AL_ReverseDate = new ZDateTime(2011, 2, 15);

			Factory.Save();

			var results = RunScript("NULL", "NULL");
			AssertEquals("Should Have 3 Rows ", 3, results.Rows.Count);
			AssertEquals("Should Return 2 WIP", 2, results.Select(AccTransactionLines.Schema.AL_LineType + " = 'WIP'").Length);
			AssertEquals("Should Return 1 CST", 1, results.Select(AccTransactionLines.Schema.AL_LineType + " = 'CST'").Length);
		}

		public void Testcsfn_AllJobProfitDetailCoreWhenChargeGroupNotEmpty()
		{
			var testObjCreator = new TestObjectCreator(Factory);

			var frtChargeCode = testObjCreator.CreateChargeCode("ABC");
			frtChargeCode.AC_ChargeGroup = "FRT";

			var dstChargeCode = testObjCreator.CreateChargeCode("DEF");
			dstChargeCode.AC_ChargeGroup = "DST";

			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = testObjCreator.CreateJob(shipment);
			var frtCharge = testObjCreator.CreateCharge(job, frtChargeCode, 100m, 100m);
			var dstCharge = testObjCreator.CreateCharge(job, dstChargeCode, 100m, 100m);
			Factory.Save();

			DataTable results = RunScript("FRT");
			AssertEquals("Should Have 2 Row", 2, results.Rows.Count);
			AssertEquals("Should Return 1 WIP", 1, results.Select(AccTransactionLines.Schema.AL_LineType + " = 'WIP'").Length);
			AssertEquals("Should Return 1 ACR", 1, results.Select(AccTransactionLines.Schema.AL_LineType + " = 'ACR'").Length);
			AssertEquals("Should Return 2 FRT", 2, results.Select(AccTransactionLines.Schema.AL_AC + " = '" + frtChargeCode.PK + "'").Length);
			AssertEquals("Should Return 0 DST", 0, results.Select(AccTransactionLines.Schema.AL_AC + " = '" + dstChargeCode.PK + "'").Length);

			results = RunScript("DST");
			AssertEquals("Should Have 2 Row", 2, results.Rows.Count);
			AssertEquals("Should Return 1 WIP", 1, results.Select(AccTransactionLines.Schema.AL_LineType + " = 'WIP'").Length);
			AssertEquals("Should Return 1 ACR", 1, results.Select(AccTransactionLines.Schema.AL_LineType + " = 'ACR'").Length);
			AssertEquals("Should Return 2 DST", 2, results.Select(AccTransactionLines.Schema.AL_AC + " = '" + dstChargeCode.PK + "'").Length);
			AssertEquals("Should Return 0 FRT", 0, results.Select(AccTransactionLines.Schema.AL_AC + " = '" + frtChargeCode.PK + "'").Length);
		}

		[TestDate(2018, 04, 25)]
		public void Testcsfn_AllJobProfitDetailCoreWhenJobOpenDateAndCreationDateAreDifferent()
		{
			var testObjCreator = new TestObjectCreator(Factory);

			var frtChargeCode = testObjCreator.CreateChargeCode("ABC");
			frtChargeCode.AC_ChargeGroup = "FRT";

			var dstChargeCode = testObjCreator.CreateChargeCode("DEF");
			dstChargeCode.AC_ChargeGroup = "DST";

			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = testObjCreator.CreateJob(shipment);
			job.JH_A_JOP = ZDateTime.Today.AddDays(-2);
			var frtCharge = testObjCreator.CreateCharge(job, frtChargeCode, 100m, 100m);
			var dstCharge = testObjCreator.CreateCharge(job, dstChargeCode, 100m, 100m);
			Factory.Save();

			DataTable results = RunScript("FRT");
			AssertEquals("Should Have 2 Row", 2, results.Rows.Count);
			AssertEquals("Should Return 1 WIP", 1, results.Select(AccTransactionLines.Schema.AL_LineType + " = 'WIP'").Length);
			AssertEquals("Should Return 1 ACR", 1, results.Select(AccTransactionLines.Schema.AL_LineType + " = 'ACR'").Length);
			AssertEquals("Should Return 2 FRT", 2, results.Select(AccTransactionLines.Schema.AL_AC + " = '" + frtChargeCode.PK + "'").Length);
			AssertEquals("Should Return 0 DST", 0, results.Select(AccTransactionLines.Schema.AL_AC + " = '" + dstChargeCode.PK + "'").Length);
			AssertEquals("Should Return 2 Row", 2, results.Select(JobHeader.Schema.JH_A_JOP + " = '23 APR 2018'").Length);

			results = RunScript("DST");
			AssertEquals("Should Have 2 Row", 2, results.Rows.Count);
			AssertEquals("Should Return 1 WIP", 1, results.Select(AccTransactionLines.Schema.AL_LineType + " = 'WIP'").Length);
			AssertEquals("Should Return 1 ACR", 1, results.Select(AccTransactionLines.Schema.AL_LineType + " = 'ACR'").Length);
			AssertEquals("Should Return 2 DST", 2, results.Select(AccTransactionLines.Schema.AL_AC + " = '" + dstChargeCode.PK + "'").Length);
			AssertEquals("Should Return 0 FRT", 0, results.Select(AccTransactionLines.Schema.AL_AC + " = '" + frtChargeCode.PK + "'").Length);
			AssertEquals("Should Return 2 Row", 2, results.Select(JobHeader.Schema.JH_A_JOP + " = '23 APR 2018'").Length);
		}

		public void Testcsfn_AllJobProfitDetailCoreWithLegacyGwConsolAndNewGCN()
		{
			var jobs = new List<Job>();
			var legacyGwConsol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var gCNConsol = TestObjectCreator.CreateGatewayConsol("AUSYD", "NZAKL", "C002", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "NZAKL", saveIt: true);
			shipment.JS_IsCFSRegistered = true;
			jobs.Add(TestObjectCreator.CreateJobForLegacyGateway(legacyGwConsol));
			jobs.Add(TestObjectCreator.CreateJob(gCNConsol, false));
			jobs.Add(TestObjectCreator.CreateJob(shipment, false));
			jobs.ForEach(x => TestObjectCreator.CreateCharge(x, TestObjectCreator.CC1, osCostAmt: 10M, creditor: TestObjectCreator.AALSHI, osSellAmt: 35M, debtor: TestObjectCreator.ABIGAS));
			Factory.Save();

			AssertEquals(true, legacyGwConsol.IsLegacyGateway);
			AssertEquals(true, gCNConsol.IsGatewayConsol);

			var sql = string.Format(@"SELECT *
							FROM csfn_AllJobProfitDetailCore('{0}','1900-01-01 00:00:00','2060-01-1 00:00:00','L','','',''
							,NULL,NULL,NULL,NULL,'','','','','','','','','')", GlbCompany.CurrentCompany.PK.ToString());
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			AssertEquals("legacyGwConsol and GCNConsol should not be selected", 2, dataTable.Rows.Count);
		}

		[TestDate(2011, 02, 19)]
		public void Testcsfn_AllJobProfitDetailCore_ActiveStatus()
		{
			var testObjCreator = new TestObjectCreator(Factory);

			var chargeCode1 = testObjCreator.CreateChargeCode("ABC");
			chargeCode1.AC_ChargeGroup = "FRT";

			var shipment1 = TestObjectCreator.CreateShipment("S00000001");
			var job1 = testObjCreator.CreateJob(shipment1);
			var charge1 = testObjCreator.CreateCharge(job1, chargeCode1, 100m, 100m);

			var shipment2 = TestObjectCreator.CreateShipment("S00000002");
			var job2 = testObjCreator.CreateJob(shipment2);
			Factory.Save();

			var deActivateJobSql = $@"
UPDATE dbo.JobHeader
SET
	JH_IsActive = 0,
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_PK = '{job2.PK}'";
			DataUtils.GetDataTableFromQuery(Db.Connection, deActivateJobSql);

			var charge2 = testObjCreator.CreateCharge(job2, chargeCode1, 100m, 100m);
			Factory.Save();

			var results = RunScriptWithFilter(activeStatus: "All");
			AssertEquals("Should have 4 records.", 4, results.Rows.Count);

			results = RunScriptWithFilter(activeStatus: "All");
			AssertEquals("Should have 2 record with active job.", 2, results.Select("JH_JobNum = 'S00000001'").Length);

			results = RunScriptWithFilter(activeStatus: "All");
			AssertEquals("Should have 2 record with inactive job.", 2, results.Select("JH_JobNum = 'S00000002'").Length);
		}

		protected virtual DataTable RunScript(string chargeGroup)
		{
			string sql = $@"SELECT * FROM csfn_AllJobProfitDetailCore('{GlbCompany.CurrentCompany.PK.ToString()}','1900-01-01 00:00:00','2079-06-06 23:59:29','','','',''
							           ,NULL,'{chargeGroup}',NULL,NULL,'','','','',NULL,'',NULL,'','')";
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		protected virtual DataTable RunScript(string postedOnly, string unPostedOnly)
		{
			string sql = $@"SELECT *
							FROM csfn_AllJobProfitDetailCore('{GlbCompany.CurrentCompany.PK.ToString()}','2011-01-20 00:00:00','2011-02-20 23:59:29','','','',''
							,NULL,NULL,NULL,NULL,'','','','',{postedOnly},'',{unPostedOnly},'','')";
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		protected override DataTable RunScriptWithFilter(string activeStatus = "", string notIncludeReversedWIPACR = "")
		{
			var sql = $@"SELECT *
							FROM csfn_AllJobProfitDetailCore('{GlbCompany.CurrentCompany.PK.ToString()}','1900-01-01 00:00:00','2079-06-06 23:59:29','','','',''
							,NULL,NULL,NULL,NULL,'','','','',NULL,'',NULL,'{notIncludeReversedWIPACR}','{activeStatus}')";
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}


