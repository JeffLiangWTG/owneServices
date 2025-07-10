using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Accounting.Testing
{
	[TestedType(typeof(Report_JobProfitForwardingAndCustomsSummaryByJob))]
	class Report_JobProfitForwardingAndCustomsSummaryByJobEDWTest : BiCreateScriptTest
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestForwardingJobProfitAndCustomsSummaryReport_ForShipmentWithoutConsigneeOrConsignor()
		{
			using (var edwConnection = Db.NewAdminConnection(ScriptDbName))
			{
				var companyPK = TestHelper.InsertCompany("CNY", "DCN");
				fTestHelper = new AccountingFunctionTestingHelper(edwConnection, ScriptDbName);
				TestHelper.CreateChargeCode();
				TestHelper.CreateShipment();
				TestHelper.CreateConsolShipmentPivot("J0001");
				TestHelper.CreateJobHeader("S0001", companyPK);
				TestHelper.CreateJobHeader("S0002", companyPK);
				TestHelper.InsertAccGLTransactionLine("WIP", companyPK, "2023-09-08", "");
				TestHelper.InsertAccGLTransactionLine("REV", companyPK, "2023-09-08", "2023-09-08");
				TestHelper.InsertAccGLTransactionLine("ACR", companyPK, "2023-09-08", "", jobHeaderKey: 2);

				var dt = GetResultSetAll(edwConnection, companyPK);

				AssertEquals("Report should contain shipment even without consigee and consignor", 2, dt.Rows.Count);
				AssertEquals("Should use local client's AR Settlement Group code", 2, dt.Select("JobLocalClientARSettlementGroupCode is null ").Length);
				AssertEquals("Should use overseas agent's AR Settlement Group code", 2, dt.Select("JobOverseasAgentARSettlementGroupCode is null").Length);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestOutstandingWIPACR()
		{
			using (var edwConnection = Db.NewAdminConnection(ScriptDbName))
			{
				var companyPK = TestHelper.InsertCompany("CNY", "DCN");
				fTestHelper = new AccountingFunctionTestingHelper(edwConnection, ScriptDbName);
				TestHelper.CreateChargeCode();
				TestHelper.CreateShipment();
				TestHelper.CreateConsolShipmentPivot("J0001");
				TestHelper.CreateJobHeader("S0001", companyPK);
				TestHelper.CreateJobHeader("S0002", companyPK);
				TestHelper.InsertAccGLTransactionLine("WIP", companyPK, "2023-09-08", "");
				TestHelper.InsertAccGLTransactionLine("REV", companyPK, "2023-09-08", "2023-09-08", lineAmount: 20);
				TestHelper.InsertAccGLTransactionLine("CST", companyPK, "2023-09-08", "2023-09-08", lineAmount: 10);
				TestHelper.InsertAccGLTransactionLine("ACR", companyPK, "2023-09-08", "", jobHeaderKey: 2);

				var dt = GetResultSetAll(edwConnection, companyPK, outstandingWIP: "Y");

				AssertEquals("OutstandingWIP is contained", 1, dt.Rows.Count);
				AssertEquals("OutstandingWIP", 1, dt.Select("JH_JobNum = 'S0001' and CSTAmount = 10 and REVAmount = 20 and WIPAmount = -1 and AL_LineAMount = 29").Length);

				dt = GetResultSetAll(edwConnection, companyPK, outstandingACR: "Y");
				AssertEquals("OutstandingACR", 1, dt.Select("JH_JobNum = 'S0002'").Length);
				AssertEquals("Not Contained", 0, dt.Select("JH_JobNum = 'S0001'").Length);
			}
		}

		DataTable GetResultSetAll(AdminConnection edwConnection, Guid companyPK, string outstandingWIP = "", string outstandingACR = "")
		{
			var sql = string.Format(
				@"SELECT * FROM [{0}].[dbo].[Report_JobProfitForwardingAndCustomsSummaryByJob]
					(
						'AU'
						, '{1}'
						, '1900-01-01 00:00:00'
						, '2079-06-06 23:59:29'
						, ''
						, '{2}'
						, '{3}'
						, NULL
						, NULL
						, NULL
						, NULL
						, NULL
						, NULL
						, NULL
						, NULL
						, NULL
						, NULL
						, '1900-01-01 00:00:00'
						, '2079-06-06 23:59:29'	
						, NULL
						, NULL
						, NULL
						, NULL
						, NULL
						, NULL
						, 1
						, @mngList
						, 1
						, ''
						, ''
					)"
			,
				ScriptDbName, companyPK, outstandingWIP, outstandingACR);
			var command = edwConnection.Command(sql);
			var mngList = new TVPParamInfo("@mngList", "dbo.TVP_uniqueidentifier", typeof(Guid), Array.Empty<object>());
			mngList.AddTVPParameters(command);
			return DataUtils.GetDataTableFromCommand(command);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;
	}
}


