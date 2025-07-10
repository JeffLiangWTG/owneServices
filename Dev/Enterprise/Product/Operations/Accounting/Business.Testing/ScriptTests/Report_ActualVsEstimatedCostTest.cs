using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ActualVsEstimatedCostTest : ScriptTest
	{
		[TestDate(2015, 11, 24, 11, 14, 17)]
		public void TestChargeCodePKFiltering()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 2);
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100);
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC2, 200);
			var charge1 = TestObjectCreator.CreateCharge(line1, job);
			charge1.JR_EstimatedCost = 110;
			var charge2 = TestObjectCreator.CreateCharge(line2, job);
			charge2.JR_EstimatedCost = 210;
			Factory.Save();

			var resultTable = RunScript("", ZDateTime.Empty, ZDateTime.Empty, 0, 0, string.Format("WHERE ChargeCodePK = '{0}'", TestObjectCreator.CC1.PK));
			var pkReplacment = new List<Tuple<ZGuid, string>>(new[]
				{
					new Tuple<ZGuid, string>(job.Branch.PK, "jobBranchPK"),
					new Tuple<ZGuid, string>(job.Department.PK, "jobDepartmentPK"),
					new Tuple<ZGuid, string>(job.LocalChargesPK, "jobLocalChargesPK"),
					new Tuple<ZGuid, string>(job.AgentCollectPK, "jobAgentCollectPK"),
					new Tuple<ZGuid, string>(TestObjectCreator.CC1.PK, "chargeCode1PK"),
					new Tuple<ZGuid, string>(TestObjectCreator.CC2.PK, "chargeCode2PK"),
					new Tuple<ZGuid, string>(TestObjectCreator.Creditor1.PK, "orgPK"),
				});

			var expectedResult = @"
JobType JobNumber            JobStatus JobBranchPK                          JobDepartmentPK                      JobOpenDate             JobCloseDate            JobOperator JobSalesRep JobLocalClientCode JobLocalClientPK                     JobOverseasAgentPK                   TransactionBranch TransactionDepartment ChargeCode ChargeCodePK                         ChargeCodeGroup ChargeCodeSalesGroup ChargeCodeSalesGroupPK               ChargeCodeExpenseGroup ChargeCodeExpenseGroupPK             ChargeDescription                                                                                                                                                                                                                                                OrgPK                                OrgCode      OrgName                                                                                              CurrencyCode ActualCost            ActualLocalCost       EstimatedCost         EstimatedLocalCost                      VarianceOSAmount      VarianceLocalAmount                     VariancePercentage                      TransactionCreatedUser TransactionCreatedTime  TransactionNumber    TransactionPostDate     LineRecognisedDate
------- -------------------- --------- ------------------------------------ ------------------------------------ ----------------------- ----------------------- ----------- ----------- ------------------ ------------------------------------ ------------------------------------ ----------------- --------------------- ---------- ------------------------------------ --------------- -------------------- ------------------------------------ ---------------------- ------------------------------------ ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ------------------------------------ ------------ ---------------------------------------------------------------------------------------------------- ------------ --------------------- --------------------- --------------------- --------------------------------------- --------------------- --------------------------------------- --------------------------------------- ---------------------- ----------------------- -------------------- ----------------------- -----------------------
SHP     S001                 WRK       jobBranchPK                          jobDepartmentPK                      2015-11-24 11:14:00     NULL                    E                       ZLOCCLT            jobLocalChargesPK                    jobAgentCollectPK                    BNE               BRN                   ZZCC1      chargeCode1PK                        NGC             NULL                 NULL                                 NULL                   NULL                                 Charge Code 1                                                                                                                                                                                                                                                    NULL                                 NULL         NULL                                                                                                 AUD          100.00                100.00                110.00                110.00                                  -10.00                -10.00                                  -9.09                                   E                      2015-11-24 11:14:00     INV1                 2015-11-24 11:14:00     2015-11-24 11:14:00
";
			AssertTableAsTextFromSQLServerManagenentStudio("", resultTable, expectedResult, Array.Empty<string>(), pkReplacment);

			resultTable = RunScript("", ZDateTime.Empty, ZDateTime.Empty, 0, 0, string.Format("WHERE ChargeCodePK != '{0}'", TestObjectCreator.CC1.PK));
			expectedResult = @"
JobType JobNumber            JobStatus JobBranchPK                          JobDepartmentPK                      JobOpenDate             JobCloseDate            JobOperator JobSalesRep JobLocalClientCode JobLocalClientPK                     JobOverseasAgentPK                   TransactionBranch TransactionDepartment ChargeCode ChargeCodePK                         ChargeCodeGroup ChargeCodeSalesGroup ChargeCodeSalesGroupPK               ChargeCodeExpenseGroup ChargeCodeExpenseGroupPK             ChargeDescription                                                                                                                                                                                                                                                OrgPK                                OrgCode      OrgName                                                                                              CurrencyCode ActualCost            ActualLocalCost       EstimatedCost         EstimatedLocalCost                      VarianceOSAmount      VarianceLocalAmount                     VariancePercentage                      TransactionCreatedUser TransactionCreatedTime  TransactionNumber    TransactionPostDate     LineRecognisedDate
------- -------------------- --------- ------------------------------------ ------------------------------------ ----------------------- ----------------------- ----------- ----------- ------------------ ------------------------------------ ------------------------------------ ----------------- --------------------- ---------- ------------------------------------ --------------- -------------------- ------------------------------------ ---------------------- ------------------------------------ ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ------------------------------------ ------------ ---------------------------------------------------------------------------------------------------- ------------ --------------------- --------------------- --------------------- --------------------------------------- --------------------- --------------------------------------- --------------------------------------- ---------------------- ----------------------- -------------------- ----------------------- -----------------------
SHP     S001                 WRK       jobBranchPK                          jobDepartmentPK                      2015-11-24 11:14:00     NULL                    E                       ZLOCCLT            jobLocalChargesPK                    jobAgentCollectPK                    BNE               BRN                   ZZCC2      chargeCode2PK                        NGC             NULL                 NULL                                 NULL                   NULL                                 Charge Code 2                                                                                                                                                                                                                                                    NULL                                 NULL         NULL                                                                                                 AUD          200.00                200.00                210.00                210.00                                  -10.00                -10.00                                  -4.76                                   E                      2015-11-24 11:14:00     INV1                 2015-11-24 11:14:00     2015-11-24 11:14:00
";
			AssertTableAsTextFromSQLServerManagenentStudio("", resultTable, expectedResult, Array.Empty<string>(), pkReplacment);
		}

		DataTable RunScript(string jobType, ZDateTime revRecogFrom, ZDateTime revRecogTo, ZDecimal varianceAmtExceeding, ZDecimal variancePercentageExceeding, string additionToQuery = "")
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * FROM Report_ActualVsEstimatedCost 
(
	'{0}',	--@CompanyPK uniqueidentifier,
	'{1}',	--@JobType varchar(10),
	'{2}',	--@RevRecogFrom datetime,
	'{3}',	--@RevRecogTo datetime,
	{4},	--@VarianceAmtExceeding decimal(10, 2),
	{5}	--@VariancePercentageExceeding decimal(10, 2)
)
{6}",
			GlbCompany.CurrentCompany.PK,
			jobType,
			revRecogFrom.IsEmpty ? ZDateTime.MinSmallDateTimeValue.ToISO8601String() : revRecogFrom.ToISO8601String(),
			revRecogTo.IsEmpty ? ZDateTime.MaxSmallDateTimeValue.ToISO8601String() : revRecogTo.ToISO8601String(),
			varianceAmtExceeding,
			variancePercentageExceeding,
			additionToQuery
			));
		}

		public void TestReportActualVsEstimatedCost()
		{
			var shipment = TestObjectCreator.CreateShipment("S002");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 2);
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100);
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC2, 200);
			var charge1 = TestObjectCreator.CreateCharge(line1, job);
			var charge2 = TestObjectCreator.CreateCharge(line2, job);
			Factory.Save();

			TestConnection.ExecuteNonQuery(string.Format(
			@"INSERT INTO dbo.JobCharge (JR_PK, JR_JH, JR_GC, JR_GE, JR_GB, JR_Desc, JR_OSCostAmt, JR_OSCostExRate, JR_LocalCostAmt, JR_AC, JR_AL_APLine, JR_SystemCreateTimeUtc, JR_SystemCreateUser, JR_SystemLastEditTimeUtc, JR_SystemLastEditUser)
			VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", ZGuid.NewZGuid(), job.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, GlbBranch.CurrentBranch.PK, "Test JobCharge", 100, 0.0m, 100, TestObjectCreator.CC3.PK, line1.PK));

			AssertNoExceptionThrown(() => RunScript(job.JobType.ToString(), ZDateTime.Empty, ZDateTime.Empty, 0, 0));
		}

		public void TestReportActualVsEstimatedCostShouldHaveInactiveJob()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S001");
			var job1 = TestObjectCreator.CreateJob(shipment1, TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 2);
			var invoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, job1, TestObjectCreator.CC1, 100);
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice1, job1, TestObjectCreator.CC2, 200);
			var charge1 = TestObjectCreator.CreateCharge(line1, job1);
			charge1.JR_EstimatedCost = 110;
			var charge2 = TestObjectCreator.CreateCharge(line2, job1);
			charge2.JR_EstimatedCost = 210;
			Factory.Save();

			var shipment2 = TestObjectCreator.CreateShipment("S002");
			var job2 = TestObjectCreator.CreateJob(shipment2, TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 2);
			Factory.Save();

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV2", organisation: TestObjectCreator.Creditor1);
			var line3 = TestObjectCreator.CreateInvoiceLine(invoice2, job2, TestObjectCreator.CC1, 100);
			var line4 = TestObjectCreator.CreateInvoiceLine(invoice2, job2, TestObjectCreator.CC2, 200);
			var charge3 = TestObjectCreator.CreateCharge(line3, job2);
			charge3.JR_EstimatedCost = 110;
			var charge4 = TestObjectCreator.CreateCharge(line4, job2);
			charge4.JR_EstimatedCost = 210;

			Factory.Save();

			string deActivateJobSql = $@"
UPDATE dbo.JobHeader
SET
	JH_IsActive = 0,
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_PK = '{job2.PK}'";
			DataUtils.GetDataTableFromQuery(Db.Connection, deActivateJobSql);

			var resultTable = RunScript("", ZDateTime.Empty, ZDateTime.Empty, 0, 0, string.Format("WHERE ChargeCodePK = '{0}'", TestObjectCreator.CC1.PK));

			AssertEquals("ReportActualVsEstimatedCost should have data with inactive job", resultTable.Rows.Count, 2);
		}
	}
}


