
using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_OutstandingWIPACRTransactionsSummaryTest : ScriptTest
	{
		public void TestOutstandingWIPACRTransactionsSummary()
		{
			PrepareData();
			DataTable results = RunScript("Charge Code");

			AssertEquals("Row count", 4, results.Rows.Count);
			AssertRow(results.Rows[0], chargeCode, 140.00m, 250.00m, DBNull.Value, DBNull.Value);
			AssertRow(results.Rows[1], chargeCode2, 300.00m, 400.00m, DBNull.Value, DBNull.Value);
			AssertRow(results.Rows[2], chargeCode3, 500.00m, 600.00m, DBNull.Value, DBNull.Value);
			AssertRow(results.Rows[3], chargeCode4, 700.00m, 800.00m, DBNull.Value, DBNull.Value);

			results = RunScript("Transaction Branch");

			AssertEquals("Row count", 5, results.Rows.Count);
			AssertRow(results.Rows[0], chargeCode, 100.00m, 200.00m, branch1.GB_Code.ToString(), DBNull.Value);
			AssertRow(results.Rows[1], chargeCode, 40.00m, 50.00m, branch2.GB_Code.ToString(), DBNull.Value);
			AssertRow(results.Rows[2], chargeCode2, 300.00m, 400.00m, branch2.GB_Code.ToString(), DBNull.Value);
			AssertRow(results.Rows[3], chargeCode3, 500.00m, 600.00m, branch1.GB_Code.ToString(), DBNull.Value);
			AssertRow(results.Rows[4], chargeCode4, 700.00m, 800.00m, branch2.GB_Code.ToString(), DBNull.Value);

			results = RunScript("Transaction Dept");

			AssertEquals("Row count", 5, results.Rows.Count);
			AssertRow(results.Rows[0], chargeCode, 100.00m, 200.00m, DBNull.Value, department.GE_Code.ToString());
			AssertRow(results.Rows[1], chargeCode, 40.00m, 50.00m, DBNull.Value, department2.GE_Code.ToString());
			AssertRow(results.Rows[2], chargeCode2, 300.00m, 400.00m, DBNull.Value, department.GE_Code.ToString());
			AssertRow(results.Rows[3], chargeCode3, 500.00m, 600.00m, DBNull.Value, department2.GE_Code.ToString());
			AssertRow(results.Rows[4], chargeCode4, 700.00m, 800.00m, DBNull.Value, department2.GE_Code.ToString());

			results = RunScript("Transaction Branch, then Dept");

			AssertEquals("Row count", 5, results.Rows.Count);
			AssertRow(results.Rows[0], chargeCode, 100.00m, 200.00m, branch1.GB_Code.ToString(), department.GE_Code.ToString());
			AssertRow(results.Rows[1], chargeCode, 40.00m, 50.00m, branch2.GB_Code.ToString(), department2.GE_Code.ToString());
			AssertRow(results.Rows[2], chargeCode2, 300.00m, 400.00m, branch2.GB_Code.ToString(), department.GE_Code.ToString());
			AssertRow(results.Rows[3], chargeCode3, 500.00m, 600.00m, branch1.GB_Code.ToString(), department2.GE_Code.ToString());
			AssertRow(results.Rows[4], chargeCode4, 700.00m, 800.00m, branch2.GB_Code.ToString(), department2.GE_Code.ToString());

			results = RunScript("Transaction Department, then Branch");

			AssertEquals("Row count", 5, results.Rows.Count);
			AssertRow(results.Rows[0], chargeCode, 100.00m, 200.00m, branch1.GB_Code.ToString(), department.GE_Code.ToString());
			AssertRow(results.Rows[1], chargeCode, 40.00m, 50.00m, branch2.GB_Code.ToString(), department2.GE_Code.ToString());
			AssertRow(results.Rows[2], chargeCode2, 300.00m, 400.00m, branch2.GB_Code.ToString(), department.GE_Code.ToString());
			AssertRow(results.Rows[3], chargeCode3, 500.00m, 600.00m, branch1.GB_Code.ToString(), department2.GE_Code.ToString());
			AssertRow(results.Rows[4], chargeCode4, 700.00m, 800.00m, branch2.GB_Code.ToString(), department2.GE_Code.ToString());
		}

		public void TestOutstandingWIPACRTransactionsSummaryWithContainerModeAsNull()
		{
			PrepareData();
			DataTable results = RunScriptWithContainerModeAsNull();

			AssertEquals("Row count", 4, results.Rows.Count);
		}

		public void TestOutstandingWIPACRTransactionsSummaryForJobBranchManagementCode()
		{
			PrepareData();
			var test = Factory.Load<GlbBranch>(branch1.PK);

			DataTable results = RunScriptForJobBranchManagementCode(branch1.PK.ToString(), "");
			AssertEquals("2 records is for branch1", 2, results.Rows.Count);

			results = RunScriptForJobBranchManagementCode("", "BRA");
			AssertEquals("2 records is for branch1 which has BranchManagementCode BRA", 2, results.Rows.Count);

			results = RunScriptForJobBranchManagementCode(branch2.PK.ToString(), "");
			AssertEquals("3 records is for branch2", 3, results.Rows.Count);

			results = RunScriptForJobBranchManagementCode("", "BRB");
			AssertEquals("3 records is for branch2 which has BranchManagementCode BRB", 3, results.Rows.Count);

			results = RunScriptForJobBranchManagementCode(branch1.PK.ToString(), "BRB");
			AssertEquals("No records for branch1 which has BranchManagementCode BRB", 0, results.Rows.Count);
		}

		[TestDate(2010, 03, 15)]
		public void TestOutstandingWIPACRTransactionsSummaryWithLegacyGwConsolAndNewGCN()
		{
			var jobs = new List<Job>();
			var legacyGwConsol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var gCNConsol = TestObjectCreator.CreateGatewayConsol("AUSYD", "NZAKL", "C002", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "NZAKL", saveIt: true);
			shipment.JS_IsCFSRegistered = true;
			jobs.Add(TestObjectCreator.CreateJobForLegacyGateway(legacyGwConsol));
			jobs.Add(TestObjectCreator.CreateJob(gCNConsol, false));
			jobs.Add(TestObjectCreator.CreateJob(shipment, false));
			TestObjectCreator.CreateCharge(jobs[0], TestObjectCreator.CC1, osCostAmt: 10M, creditor: TestObjectCreator.AALSHI, osSellAmt: 35M, debtor: TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateCharge(jobs[1], TestObjectCreator.CC3, osCostAmt: 11M, creditor: TestObjectCreator.AALSHI, osSellAmt: 45M, debtor: TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateCharge(jobs[2], TestObjectCreator.CC4, osCostAmt: 12M, creditor: TestObjectCreator.AALSHI, osSellAmt: 55M, debtor: TestObjectCreator.ABIGAS);
			Factory.Save();

			AssertEquals(true, legacyGwConsol.IsLegacyGateway);
			AssertEquals(true, gCNConsol.IsGatewayConsol);

			var results = RunScript("Charge Code", jobType: "L", shipmentRegistrationDateFrom: "NULL", shipmentRegistrationDateTo: "NULL");
			AssertEquals("Row count", 1, results.Rows.Count);

			results = RunScript("Charge Code", jobType: "A", shipmentRegistrationDateFrom: "NULL", shipmentRegistrationDateTo: "NULL");
			AssertEquals("Row count", 3, results.Rows.Count);
		}

		public void TestControllingCustomerAndAgentColumns()
		{
			PrepareData(false);
			var shipment1 = CreateWipAndAccrual("S00001000", branch1, department, chargeCode, 100.00m, 200.00m);
			var shipment2 = CreateWipAndAccrual("S00001001", branch2, department, chargeCode2, 300.00m, 400.00m);
			Factory.Save();

			var runScript = new Func<string, ZGuid?, DataTable>((sql, pK) =>
			{
				return RunScript("Charge Code", whereClause: sql, mNGOrgPK: pK);
			});

			var infoChecker = new CAGAndCCBInfoScriptTest();
			infoChecker.VerifyControllingCustomerAndAgentInfo(runScript, shipment1, shipment2);
		}

		AccChargeCode chargeCode;
		AccChargeCode chargeCode2;
		AccChargeCode chargeCode3;
		AccChargeCode chargeCode4;

		GlbBranch branch1;
		GlbBranch branch2;

		GlbDepartment department;
		GlbDepartment department2;

		public void PrepareData(bool createShipment = true)
		{
			branch1 = GlbBranch.CurrentBranch;
			branch2 = TestObjectCreator.NonCurrentBranch;

			department = GlbDepartment.CurrentDepartment;
			department2 = TestObjectCreator.NonCurrentDepartment;

			RefCurrency currency = TestObjectCreator.AUD;

			chargeCode = TestObjectCreator.CC1;
			chargeCode2 = TestObjectCreator.CC2;
			chargeCode3 = TestObjectCreator.CC3;
			chargeCode4 = TestObjectCreator.CC4;

			AccGroups expGroup = Factory.NewWithValidTestData<AccGroups>();
			expGroup.AR_Code = "EG1";
			AccGroups salesGroup = Factory.NewWithValidTestData<AccGroups>();
			salesGroup.AR_Code = "SG1";

			chargeCode.AC_AR_ExpenseGroup = expGroup.PK;
			chargeCode.AC_AR_SalesGroup = salesGroup.PK;
			chargeCode2.AC_AR_ExpenseGroup = expGroup.PK;
			chargeCode2.AC_AR_SalesGroup = salesGroup.PK;
			chargeCode3.AC_AR_ExpenseGroup = expGroup.PK;
			chargeCode3.AC_AR_SalesGroup = salesGroup.PK;
			chargeCode4.AC_AR_ExpenseGroup = expGroup.PK;
			chargeCode4.AC_AR_SalesGroup = salesGroup.PK;

			AssertNotEquals(ZGuid.Empty, chargeCode.AC_AR_ExpenseGroup);
			AssertNotEquals(ZGuid.Empty, chargeCode2.AC_AR_ExpenseGroup);
			AssertNotEquals(ZGuid.Empty, chargeCode3.AC_AR_ExpenseGroup);
			AssertNotEquals(ZGuid.Empty, chargeCode4.AC_AR_ExpenseGroup);

			AssertNotEquals(ZGuid.Empty, chargeCode.AC_AR_SalesGroup);
			AssertNotEquals(ZGuid.Empty, chargeCode2.AC_AR_SalesGroup);
			AssertNotEquals(ZGuid.Empty, chargeCode3.AC_AR_SalesGroup);
			AssertNotEquals(ZGuid.Empty, chargeCode4.AC_AR_SalesGroup);

			if (createShipment)
			{
				CreateWipAndAccrual("S00001000", branch1, department, chargeCode, 100.00m, 200.00m);
				CreateWipAndAccrual("S00001001", branch2, department, chargeCode2, 300.00m, 400.00m);
				CreateWipAndAccrual("S00001002", branch1, department2, chargeCode3, 500.00m, 600.00m);
				CreateWipAndAccrual("S00001003", branch2, department2, chargeCode4, 700.00m, 800.00m);
				CreateWipAndAccrual("S00001004", branch2, department2, chargeCode, 40.00m, 50m);
			}

			Factory.Save();

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRA', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = '{0}' And GB_GC = '{1}'", branch1.GB_Code, branch1.GB_GC));
			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRB', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = '{0}' And GB_GC = '{1}'", branch2.GB_Code, branch2.GB_GC));
		}

		ZGuid CreateWipAndAccrual(string jobNumber, GlbBranch branch, GlbDepartment department, AccChargeCode chargeCode, decimal wipAmount, decimal accrualAmount)
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment(jobNumber);
			shipment.JS_SystemCreateTimeUtc = new ZDateTime(2010, 03, 20);
			Job job = TestObjectCreator.CreateJob(shipment);
			job.JH_GB = branch.PK;
			job.JH_GE = department.PK;
			WIP wip = TestObjectCreator.CreateWIP(job, chargeCode, 1.0m, "Line 1", wipAmount);
			wip.AL_GB = branch.PK;
			wip.AL_GE = department.PK;
			wip.AL_PostDate = new ZDateTime(2010, 03, 15);
			wip.AL_LocalExTaxAmount = wipAmount;
			Accrual accrual = TestObjectCreator.CreateAccrual(job, chargeCode, 1.0m, "Line 2", accrualAmount);
			accrual.AL_GB = branch.PK;
			accrual.AL_GE = department.PK;
			accrual.AL_PostDate = new ZDateTime(2010, 03, 15);
			accrual.AL_LocalExTaxAmount = accrualAmount;
			return shipment.PK;
		}

		DataTable RunScript(string groupBy, string whereClause = "", ZGuid? mNGOrgPK = null, string jobType = "A",
			string shipmentRegistrationDateFrom = "'2010-03-01 00:00:00'", string shipmentRegistrationDateTo = "'2010-04-01 00:00:00'")
		{
			var sql = $@"SELECT *
							FROM Report_OutstandingWIPACRTransactionsSummary('2010-04-01 00:00:00','{GlbCompany.CurrentCompany.PK}','{jobType}',NULL,'','',NULL,NULL,NULL,NULL,
										'1900-01-01 00:00:00','2079-06-06 23:59:29',{shipmentRegistrationDateFrom},{shipmentRegistrationDateTo},'','','',NULL,NULL,NULL,NULL,'',NULL, NULL, NULL,
										'','','','',NULL,NULL,NULL,NULL,'{groupBy}',NULL,NULL, @MNGListValue, @MNGListIsEmptyValue, NULL) {whereClause} 
							ORDER BY ChargeGroup,ChargeCode";
			var command = Db.Connection.Command(sql);
			AddTVPAndIsEmptyParameters(command, "@MNGListValue", "dbo.TVP_uniqueidentifier", "@MNGListIsEmptyValue", mNGOrgPK.HasValue ? new Guid[] { mNGOrgPK.Value.ToGuid() } : Array.Empty<Guid>());
			return DataUtils.GetDataTableFromCommand(command);
		}

		DataTable RunScriptForJobBranchManagementCode(string jobBranch, string jobBranchManagementCode)
		{
			string sql = string.Format(@"SELECT *
							FROM Report_OutstandingWIPACRTransactionsSummary('2010-04-01 00:00:00','{0}','A',NULL,'{1}','',NULL,NULL,NULL,NULL,
										'1900-01-01 00:00:00','2079-06-06 23:59:29','2010-03-01 00:00:00','2010-04-01 00:00:00','','','',NULL,NULL,NULL,NULL,'',NULL, NULL, NULL,
										'','','','',NULL,NULL,NULL,NULL,NULL,NULL,NULL, @MNGListValue, @MNGListIsEmptyValue, '{2}')", GlbCompany.CurrentCompany.PK.ToString(), jobBranch, jobBranchManagementCode);
			var command = Db.Connection.Command(sql);
			AddTVPAndIsEmptyParameters(command, "@MNGListValue", "dbo.TVP_uniqueidentifier", "@MNGListIsEmptyValue", Array.Empty<Guid>());
			return DataUtils.GetDataTableFromCommand(command);
		}

		DataTable RunScriptWithContainerModeAsNull()
		{
			string sql = string.Format(@"SELECT *
							FROM Report_OutstandingWIPACRTransactionsSummary(
								'2012-07-03 00:00:00' --@PeriodToDate
								,'{0}' --@CompanyPK
								,N'A' --@JobType
								,NULL --@JobStatus
								,N'' --@JobBranch
								,N'' --@JobDepartment
								,NULL --@JobOpenedFrom
								,NULL --@JobOpenedTo
								,NULL --@JobClosedFrom
								,NULL --@JobClosedTo
								,'1900-01-01 00:00:00' --@JobRevRecogFrom
								,'2079-06-06 23:59:29' --@JobRevRecogTo
								,NULL --@JobRegistrationFrom
								,NULL --@JobRegistrationTo
								,N'' --@JobLocalClient
								,N'' --@TransportMode
								,NULL --@ContainerMode
								,NULL --@ETDFromDate
								,NULL --@ETDToDate
								,NULL --@ETAFromDate
								,NULL --@ETAToDate
								,NULL --@ChargeCode
								,NULL --@ChargeGroup
								,NULL --@SalesGroup
								,NULL --@ExpenseGroup
								,N'' --@TransactionBranch
								,N'' --@TransactionDepartment
								,N'' --@TransactionDebtor
								,N'' --@TransactionCreditor
								,NULL --@TransactionCreatedFrom
								,NULL --@TransactionCreatedTo
								,NULL --@TransactionReversedFrom
								,NULL --@TransactionReversedTo
								,N'Charge Code' --@GroupBys
								,NULL --@Origin
								,NULL --@Destination								
								,@MNGListValue
								,@MNGListIsEmptyValue	
								,NULL --@BranchManagementCode
							)", GlbCompany.CurrentCompany.PK.ToString());

			var command = Db.Connection.Command(sql);
			AddTVPAndIsEmptyParameters(command, "@MNGListValue", "dbo.TVP_uniqueidentifier", "@MNGListIsEmptyValue", Array.Empty<Guid>());
			return DataUtils.GetDataTableFromCommand(command);
		}

		void AssertRow(DataRow row, AccChargeCode chargeCode, decimal wipAmount, decimal accrualAmount, object branch, object department)
		{
			AssertEquals("ChargeCode", chargeCode.AC_Code, (string)row["ChargeCode"]);
			AssertEquals("ChargeDesc", chargeCode.AC_Desc, (string)row["ChargeDesc"]);
			AssertEquals("ChargeGroup", chargeCode.AC_ChargeGroup, (string)row["ChargeGroup"]);
			AssertEquals("SalesGroupCode", chargeCode.SalesGroup.AR_Code, (string)row["SalesGroupCode"]);
			AssertEquals("ExpenseGroupCode", chargeCode.ExpenseGroup.AR_Code, (string)row["ExpenseGroupCode"]);

			object value = row["BranchCode"];

			if (value is string)
			{
				AssertEquals("BranchCode", branch, value.ToString().TrimEnd());
			}
			else
			{
				AssertEquals("BranchCode", branch, value);
			}

			value = row["DepartmentCode"];

			if (value is string)
			{
				AssertEquals("DepartmentCode", department, value.ToString().TrimEnd());
			}
			else
			{
				AssertEquals("DepartmentCode", department, value);
			}

			AssertEquals("WipAmount", wipAmount, row["WIP"]);
			AssertEquals("AccrualAmount", -accrualAmount, row["Accrual"]);
		}
	}
}


