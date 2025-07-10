using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Accounting;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Accounting
{
	[TestedType(typeof(AccCashAdvanceRequestUsageCollector))]
	sealed class AccCashAdvanceRequestlUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 3);

		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlQuery = @"
DECLARE @EDICompanyPk UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
DECLARE @SYDBranchPk UNIQUEIDENTIFIER = 'FDD429D2-648C-4895-8F9F-06E90DED2BE5'
DECLARE @DepartmentPk UNIQUEIDENTIFIER = '86BB1C22-0865-4685-996E-D56CBD136491'
DECLARE @accChargeCodePK UNIQUEIDENTIFIER = '2091ef5a-9e4d-4e87-8b63-6b334189105d'
DECLARE @accChargeCodePK2 UNIQUEIDENTIFIER = 'f2b6442f-802e-4e94-8433-b424d6c86ae8'
DECLARE @ohPK UNIQUEIDENTIFIER = '04451e63-733f-48bb-b32b-79585f63b3ae'

DECLARE @transactionHeaderPk_AR UNIQUEIDENTIFIER = '79bb3858-934e-4126-b0be-9d97e6983750'
DECLARE @transactionLinePk_AR UNIQUEIDENTIFIER = '1164b021-65ab-4fe6-ade6-516e7750950c'
DECLARE @jobChargePK_AR UNIQUEIDENTIFIER = 'e52e4733-0e35-4776-8507-011bb470accb'
DECLARE @CAL_ARLinePK UNIQUEIDENTIFIER = '740D453D-A406-4F72-896F-28A5C500C891'
DECLARE @CAH_ARPK UNIQUEIDENTIFIER = '332df345-d5e2-418c-9169-c478b50dfd11'
DECLARE @jobHeaderPK_AR UNIQUEIDENTIFIER = '52a607ff-9502-46d6-909d-bc5432eb5cd5'

DECLARE @transactionHeaderPk_AP UNIQUEIDENTIFIER = '6e97414b-49db-433b-82c8-d7bbbaaef4f9'
DECLARE @paymentPk_AP UNIQUEIDENTIFIER = '67c63ace-93ed-408f-a0c3-26b80ecc70ab'
DECLARE @transactionLinePk_AP UNIQUEIDENTIFIER = 'aa8e35f9-7a0d-4623-a243-e6b3c976e0f4'
DECLARE @jobChargePK_AP UNIQUEIDENTIFIER = 'd3894e9d-c0a0-4f66-ba65-2c765fe67387'
DECLARE @CAL_APLinePK UNIQUEIDENTIFIER = 'c71f8df4-af35-42d7-b702-f8c86468cf1c'
DECLARE @transactionLinePk_AP2 UNIQUEIDENTIFIER = '05d7316a-998d-4d67-9550-621e6fcd7e08'
DECLARE @jobChargePK_AP2 UNIQUEIDENTIFIER = 'b3f1dcb4-3502-4bfa-9844-39134c645dd8'
DECLARE @CAL_APLinePK2 UNIQUEIDENTIFIER = '0c3e68f7-bfcc-43b5-b199-56246f5092f1'
DECLARE @CAH_APPK UNIQUEIDENTIFIER = '68070031-23d0-4475-b230-fd9793ec12de'
DECLARE @CAH_APPK2 UNIQUEIDENTIFIER = '649800bf-294d-476c-8640-c338e13ba813'
DECLARE @jobHeaderPK_AP UNIQUEIDENTIFIER = '3690efed-0256-4ba4-be89-22b847d16169'
DECLARE @jobHeaderPK_AP2 UNIQUEIDENTIFIER = '76558cf5-ce37-4d2c-a650-eb6144bebcd0'

DECLARE @newCreatedRecordPK UNIQUEIDENTIFIER

BEGIN TRAN

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal) VALUES
(@transactionHeaderPk_AR, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-03-11', 'AR', 'INV', '001', '2022-03-12 21:59', 'UT', 'AUD', 400.00)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@transactionLinePk_AR, @transactionHeaderPk_AR, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, 'CST', 400.0, 400.00, 'AUD', '2022-03-12 23:59', 'UT')

INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_SystemCreateTimeUtc, AC_SystemCreateUser) VALUES
(@accChargeCodePK, 'OAQF', 'FRT', '2022-03-11 23:59', 'UT')

INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_SystemCreateTimeUtc, AC_SystemCreateUser) VALUES
(@accChargeCodePK2, 'OAQA', 'ODO', '2022-03-11 23:59', 'UT')

INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) VALUES
(@ohPK, 'ARORG')

INSERT INTO dbo.JobHeader(JH_PK, JH_GC, JH_GB, JH_GE, JH_HeaderType, JH_JobNum, JH_ParentID, JH_ParentTableCode, JH_Status) VALUES
(@jobHeaderPK_AR, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, 'JOB', 'JH001', NEWID(), 'SHP', 'WRK')

INSERT INTO dbo.AccCashAdvanceRequestHeader(CAH_PK, CAH_GC_Company, CAH_OH_Organization, CAH_JH_Job, CAH_Ledger, CAH_RequestReferenceNumber, CAH_RX_NKTransactionCurrency, CAH_Status, CAH_SystemCreateTimeUtc, CAH_SystemCreateUser, CAH_SystemLastEditTimeUtc, CAH_SystemLastEditUser) VALUES
(@CAH_ARPK, @EDICompanyPk, @ohPK, @jobHeaderPK_AR, 'AR', '0001', 'AUD', 'PAI', '2022-03-12 23:59', 'UT', '2022-03-12 23:59', 'UT')

INSERT INTO dbo.AccCashAdvanceRequestLine (CAL_PK, CAL_CAH_RequestHeader, CAL_GC_Company, CAL_OSAmount, CAL_SystemCreateTimeUtc, CAL_SystemCreateUser, CAL_SystemLastEditTimeUtc, CAL_SystemLastEditUser) VALUES
(@CAL_ARLinePK, @CAH_ARPK, @EDICompanyPk, 400, '2022-03-12 23:59', 'UT', '2022-03-11 23:59', 'UT')

INSERT INTO dbo.JobCharge (JR_PK, JR_GC, JR_GB, JR_GE, JR_AL_ARLine, JR_AC, JR_CAL_ARLine, JR_JH, JR_SystemCreateTimeUtc, JR_SystemCreateUser) VALUES
(@jobChargePK_AR, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @transactionLinePk_AR, @accChargeCodePK, @CAL_ARLinePK, @jobHeaderPK_AR, '2022-03-12 23:59', 'UT')

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal) VALUES
(@transactionHeaderPk_AP, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-03-11', 'AP', 'INV', '001', '2022-03-11 21:59', 'UT', 'AUD', 66.00)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@transactionLinePk_AP, @transactionHeaderPk_AP, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, 'CST', 44.0, 44.00, 'AUD', '2022-03-11 23:59', 'UT')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@transactionLinePk_AP2, @transactionHeaderPk_AP, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, 'CST', 22.0, 22.00, 'AUD', '2022-03-11 23:59', 'UT')

INSERT INTO dbo.JobHeader(JH_PK, JH_GC, JH_GB, JH_GE, JH_HeaderType, JH_JobNum, JH_ParentID, JH_ParentTableCode, JH_Status) VALUES
(@jobHeaderPK_AP, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, 'JOB', 'JH002', NEWID(), 'SHP', 'WRK')

INSERT INTO dbo.JobHeader(JH_PK, JH_GC, JH_GB, JH_GE, JH_HeaderType, JH_JobNum, JH_ParentID, JH_ParentTableCode, JH_Status) VALUES
(@jobHeaderPK_AP2, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, 'JOB', 'JH003', NEWID(), 'SHP', 'WRK')

INSERT INTO dbo.AccCashAdvanceRequestHeader(CAH_PK, CAH_GC_Company, CAH_OH_Organization, CAH_JH_Job, CAH_Ledger, CAH_RequestReferenceNumber, CAH_RX_NKTransactionCurrency, CAH_Status, CAH_SystemCreateTimeUtc, CAH_SystemCreateUser, CAH_SystemLastEditTimeUtc, CAH_SystemLastEditUser) VALUES
(@CAH_APPK, @EDICompanyPk, @ohPK, @jobHeaderPK_AP, 'AP', '0001', 'AUD', 'PAI', '2022-03-11 23:59', 'UT', '2022-03-11 23:59', 'UT')

INSERT INTO dbo.AccCashAdvanceRequestHeader(CAH_PK, CAH_GC_Company, CAH_OH_Organization, CAH_JH_Job, CAH_Ledger, CAH_RequestReferenceNumber, CAH_RX_NKTransactionCurrency, CAH_Status, CAH_SystemCreateTimeUtc, CAH_SystemCreateUser, CAH_SystemLastEditTimeUtc, CAH_SystemLastEditUser) VALUES
(@CAH_APPK2, @EDICompanyPk, @ohPK, @jobHeaderPK_AP2, 'AP', '0002', 'USD', 'REQ', '2022-03-11 23:59', 'UT', '2022-03-11 23:59', 'UT')

INSERT INTO dbo.AccCashAdvanceRequestLine (CAL_PK, CAL_CAH_RequestHeader, CAL_GC_Company, CAL_OSAmount, CAL_SystemCreateTimeUtc, CAL_SystemCreateUser, CAL_SystemLastEditTimeUtc, CAL_SystemLastEditUser) VALUES
(@CAL_APLinePK, @CAH_APPK, @EDICompanyPk, 44, '2022-03-11 23:59', 'UT', '2022-03-11 23:59', 'UT')

INSERT INTO dbo.AccCashAdvanceRequestLine (CAL_PK, CAL_CAH_RequestHeader, CAL_GC_Company, CAL_OSAmount, CAL_SystemCreateTimeUtc, CAL_SystemCreateUser, CAL_SystemLastEditTimeUtc, CAL_SystemLastEditUser) VALUES
(@CAL_APLinePK2, @CAH_APPK2, @EDICompanyPk, 22, '2022-03-11 23:59', 'UT', '2022-03-11 23:59', 'UT')

INSERT INTO dbo.JobCharge (JR_PK, JR_GC, JR_GB, JR_GE, JR_AL_APLine, JR_AC, JR_CAL_APLine, JR_JH, JR_SystemCreateTimeUtc, JR_SystemCreateUser) VALUES
(@jobChargePK_AP, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @transactionLinePk_AP, @accChargeCodePK, @CAL_APLinePK, @jobHeaderPK_AP, '2022-03-11 23:59', 'UT')

INSERT INTO dbo.JobCharge (JR_PK, JR_GC, JR_GB, JR_GE, JR_AL_APLine, JR_AC, JR_CAL_APLine, JR_JH, JR_SystemCreateTimeUtc, JR_SystemCreateUser) VALUES
(@jobChargePK_AP2, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @transactionLinePk_AP2, @accChargeCodePK2, @CAL_APLinePK2, @jobHeaderPK_AP2, '2022-03-11 23:59', 'UT')

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_CAH_CashAdvanceRequestHeader, AH_TransactionCategory) VALUES
(@paymentPk_AP, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2022-07-11', 'AP', 'JNL', '005', '2022-07-11 21:59', 'UT', 'AUD', 66.00, @CAH_APPK2, 'APP')
";

			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Cash Advance Usage Statistics Records", 2, transactions.Count());

			var transaction1 = transactions.Single(t => t.ServiceOccuredUTC == new DateTime(2022, 3, 11, 21, 59, 0));
			AssertEquals("CompanyCode should be EDI", "EDI", transaction1.GetCompanyCode());
			AssertEquals("BranchCode should be SYD", "SYD", transaction1.GetBranchCode());
			AssertEquals("Test Case 1 AdditionalRefs should be as expected",
				"{\"Create Date\":\"2022-03-11T21:59:00\",\"Ledger\":\"AP\",\"Country code\":\"AU\",\"Inv currency\":\"AUD\",\"OS amount\":66.0000,\"No\":{\" of CA\":2},\"Earliest CA Date\":\"2022-03-11T23:59:00\",\"CA Status\":\"PAI, REQ\",\"CA Currency\":\"AUD, USD\",\"Job Type\":\"SHP\",\"Number of CA payments\":1,\"Charge Groups\":\"FRT, ODO\",\"Total line OS Amount\":0.0000}"
				, transaction1.AdditionalRefs);

			var transaction2 = transactions.Single(t => t.ServiceOccuredUTC == new DateTime(2022, 3, 12, 21, 59, 0));
			AssertEquals("Test Case 2 AdditionalRefs should be as expected",
				"{\"Create Date\":\"2022-03-12T21:59:00\",\"Ledger\":\"AR\",\"Country code\":\"AU\",\"Inv currency\":\"AUD\",\"OS amount\":400.0000,\"No\":{\" of CA\":1},\"Earliest CA Date\":\"2022-03-12T23:59:00\",\"CA Status\":\"PAI\",\"CA Currency\":\"AUD\",\"Job Type\":\"SHP\",\"Number of CA payments\":0,\"Charge Groups\":\"FRT\",\"Total line OS Amount\":400.0000}"
				, transaction2.AdditionalRefs);
		}
	}
}
