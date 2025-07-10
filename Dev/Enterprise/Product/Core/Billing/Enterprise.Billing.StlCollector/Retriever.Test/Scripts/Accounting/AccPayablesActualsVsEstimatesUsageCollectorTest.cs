using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Accounting;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Accounting
{
	[TestedType(typeof(AccPayablesActualsVsEstimatesUsageCollector))]
	sealed class AccPayablesActualsVsEstimatesUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 10);

		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlQuery = @"
DECLARE @EDICompanyPk UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
DECLARE @SYDBranchPk UNIQUEIDENTIFIER = 'FDD429D2-648C-4895-8F9F-06E90DED2BE5'
DECLARE @DepartmentPk UNIQUEIDENTIFIER = '86BB1C22-0865-4685-996E-D56CBD136491'
DECLARE @DepartmentPk2 UNIQUEIDENTIFIER = '0DBEC05B-8289-43BD-8AC9-0F32BC496AB2'
DECLARE @ParentDepartmentPk UNIQUEIDENTIFIER = '3BC44454-A9C4-46A5-A5B1-C704C3605AE9'
DECLARE @accChargeCodePK UNIQUEIDENTIFIER = '2091ef5a-9e4d-4e87-8b63-6b334189105d'
DECLARE @accChargeCodePK2 UNIQUEIDENTIFIER = 'f2b6442f-802e-4e94-8433-b424d6c86ae8'
DECLARE @ohPK UNIQUEIDENTIFIER = '04451e63-733f-48bb-b32b-79585f63b3ae'

DECLARE @transactionHeaderPk_APINV UNIQUEIDENTIFIER = '79bb3858-934e-4126-b0be-9d97e6983750'
DECLARE @transactionHeaderPk_APINV2 UNIQUEIDENTIFIER = 'c37d1c4c-d09f-449b-a258-c1e745780b63'
DECLARE @transactionHeaderPk_APINV_FOC UNIQUEIDENTIFIER = '461d6561-4688-4ba5-8f0b-d9c0138f44e5'
DECLARE @transactionHeaderPk_APINV_NextDay UNIQUEIDENTIFIER = '131e0e09-b535-4179-8201-cc7d3de1cc88'
DECLARE @transactionHeaderPk_ARINV UNIQUEIDENTIFIER = '22d72dfb-c243-4231-9be3-1f8b6e375bef'
DECLARE @transactionHeaderPk_APCRD UNIQUEIDENTIFIER = '1a4b518b-42de-41a4-990f-d8b75cb6f026'
DECLARE @transactionHeaderPk_APINV_Inactive UNIQUEIDENTIFIER = 'fe90aab3-e2c6-479e-98d5-e64e18f9abc9'
DECLARE @transactionHeaderPk_APINV_Job UNIQUEIDENTIFIER = 'b6caf537-4c81-46e5-95d5-f69b707e8071'

DECLARE @transactionLinePk_APINV UNIQUEIDENTIFIER = '1164b021-65ab-4fe6-ade6-516e7750950c'
DECLARE @transactionLinePk_APINV_12 UNIQUEIDENTIFIER = '709caa2b-fb75-47d1-91c9-fc7dc0938ebb'
DECLARE @transactionLinePk_APINV_FOC UNIQUEIDENTIFIER = 'c7c36816-e8cf-45e1-b7a2-2e48ed8f2cf4'
DECLARE @transactionLinePk_APINV2 UNIQUEIDENTIFIER = '1fee9e02-abbf-4f50-afb0-84ba94197953'
DECLARE @transactionLinePk_APINV_NextDay UNIQUEIDENTIFIER = 'da51c263-18b5-45fc-8109-94525e38ee82'
DECLARE @transactionLinePk_ARINV UNIQUEIDENTIFIER = '42b7b2a4-81d1-44f3-a298-4eeb11ba0516'
DECLARE @transactionLinePk_APCRD UNIQUEIDENTIFIER = '03c39f72-1757-4e64-8809-8720c210de11'
DECLARE @transactionLinePk_APINV_Inactive UNIQUEIDENTIFIER = '3ea11e18-f82a-4e33-ac33-48f7e9030359'
DECLARE @transactionLinePk_APINV_WithoutJob UNIQUEIDENTIFIER = 'd5ed65ec-8288-42d5-b29b-79fdc82a99b0'
DECLARE @transactionLinePk_APINV_WithJob UNIQUEIDENTIFIER = 'c4a87b1f-44fe-4c48-918a-d29cf79db7e6'

DECLARE @jobChargePK_APINV UNIQUEIDENTIFIER = 'e52e4733-0e35-4776-8507-011bb470accb'
DECLARE @jobChargePK_APINV_FOC UNIQUEIDENTIFIER = '2edff239-41ce-4de9-a47e-a3291cb1853d'
DECLARE @jobChargePK_APINV_NextDay UNIQUEIDENTIFIER = '533e9cab-31a2-427c-bdbe-ded1ac4c1900'
DECLARE @jobChargePK_APINV2 UNIQUEIDENTIFIER = '4c0f7103-b2c3-48bf-9217-ba314bc6faa5'
DECLARE @jobChargePK_ARINV UNIQUEIDENTIFIER = '23ed3c8c-c4ab-4813-b795-ddbc7c38ec46'
DECLARE @jobChargePK_APCRD UNIQUEIDENTIFIER = '075c5db0-42f7-4ceb-b48b-9be87f4838ca'
DECLARE @jobChargePK_APINV_Inactive UNIQUEIDENTIFIER = '874f4e17-801a-42e6-aad3-2b78d6e996ca'

DECLARE @jobHeaderPK_AP UNIQUEIDENTIFIER = '52a607ff-9502-46d6-909d-bc5432eb5cd5'
DECLARE @jobHeaderPK_AP2 UNIQUEIDENTIFIER = 'b2353048-24ba-44c4-aa7e-59f9dcd83bcd'
DECLARE @jobHeaderPK_AP_NextDay UNIQUEIDENTIFIER = 'b79cfa8b-0754-4d78-92be-ad3d2b9dfd2e'
DECLARE @jobHeaderPK_AP_FOC UNIQUEIDENTIFIER = '6710b6a2-dfb3-41bd-9e22-890fdade26b2'
DECLARE @draftInvoiceHeaderPK UNIQUEIDENTIFIER = '9ec7c016-0817-4e6f-9aaa-58084bbfbbce'

INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) VALUES
(@ohPK, 'APORG')

INSERT INTO dbo.JobHeader(JH_PK, JH_GC, JH_GB, JH_GE, JH_HeaderType, JH_JobNum, JH_ParentID, JH_ParentTableCode, JH_Status) VALUES
(@jobHeaderPK_AP, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, 'JOB', 'J0001', NEWID(), 'SHP', 'WRK')

INSERT INTO dbo.JobHeader(JH_PK, JH_GC, JH_GB, JH_GE, JH_HeaderType, JH_JobNum, JH_ParentID, JH_ParentTableCode, JH_Status) VALUES
(@jobHeaderPK_AP2, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, 'JOB', 'J0002', NEWID(), 'SHP', 'WRK')

INSERT INTO dbo.JobHeader(JH_PK, JH_GC, JH_GB, JH_GE, JH_HeaderType, JH_JobNum, JH_ParentID, JH_ParentTableCode, JH_Status) VALUES
(@jobHeaderPK_AP_NextDay, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, 'JOB', 'J0003', NEWID(), 'SHP', 'WRK')

INSERT INTO dbo.JobHeader(JH_PK, JH_GC, JH_GB, JH_GE, JH_HeaderType, JH_JobNum, JH_ParentID, JH_ParentTableCode, JH_Status) VALUES
(@jobHeaderPK_AP_FOC, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, 'JOB', 'J0005', NEWID(), 'SHP', 'WRK')

INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_SystemCreateTimeUtc, AC_SystemCreateUser) VALUES
(@accChargeCodePK, 'OAQF', 'FRT', '2024-10-10 23:59', 'UT')

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_JH, AH_OH, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled) VALUES
(@transactionHeaderPk_APINV, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @jobHeaderPK_AP, @ohPK, '2024-10-10', 'AP', 'INV', '001', '2024-10-10 23:59', 'UT', 'AUD', 400.00, 0)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_AC, AL_JH, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@transactionLinePk_APINV, @transactionHeaderPk_APINV, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @accChargeCodePK, @jobHeaderPK_AP, 'CST', 300.0, 300.00, 'AUD', '2024-10-10 23:59', 'UT')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_AC, AL_JH, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@transactionLinePk_APINV_12, @transactionHeaderPk_APINV, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @accChargeCodePK, @jobHeaderPK_AP, 'CST', 100.0, 100.00, 'AUD', '2024-10-10 22:59', 'UT')

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_JH, AH_OH, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled) VALUES
(@transactionHeaderPk_APINV2, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @jobHeaderPK_AP2, @ohPK, '2024-10-10', 'AP', 'INV', '003', '2024-10-10 22:59', 'UT', 'AUD', 600.00, 0)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_AC, AL_JH, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@transactionLinePk_APINV2, @transactionHeaderPk_APINV2, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @accChargeCodePK, @jobHeaderPK_AP2, 'CST', 600.0, 600.00, 'AUD', '2024-10-10 23:59', 'UT')

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_JH, AH_OH, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled) VALUES
(@transactionHeaderPk_APINV_NextDay, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @jobHeaderPK_AP_NextDay, @ohPK, '2024-10-11', 'AP', 'INV', '004', '2024-10-11 22:59', 'UT', 'AUD', 800.00, 0)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_AC, AL_JH, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@transactionLinePk_APINV_NextDay, @transactionHeaderPk_APINV_NextDay, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @accChargeCodePK, @jobHeaderPK_AP_NextDay, 'CST', 800.0, 800.00, 'AUD', '2024-10-11 23:59', 'UT')

INSERT INTO dbo.JobCharge (JR_PK, JR_GC, JR_GB, JR_GE, JR_AL_APLine, JR_AC, JR_JH, JR_OSCostAmt, JR_EstimatedCost, JR_SystemCreateTimeUtc, JR_SystemCreateUser) VALUES
(@jobChargePK_APINV, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @transactionLinePk_APINV, @accChargeCodePK, @jobHeaderPK_AP, 400.00, 500.00, '2024-10-10 23:59', 'UT')

INSERT INTO dbo.JobCharge (JR_PK, JR_GC, JR_GB, JR_GE, JR_AL_APLine, JR_AC, JR_JH, JR_OSCostAmt, JR_EstimatedCost, JR_SystemCreateTimeUtc, JR_SystemCreateUser) VALUES
(@jobChargePK_APINV_NextDay, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @transactionLinePk_APINV_NextDay, @accChargeCodePK, @jobHeaderPK_AP, 500.00, 500.00, '2024-10-10 23:59', 'UT')

INSERT INTO dbo.AccDraftInvoiceHeader (AIH_PK, AIH_GC_Company, AIH_GB_Branch, AIH_GE_Department, AIH_TransactionDate,
AIH_TransactionType, AIH_RX_NKTransactionCurrency, AIH_Description, AIH_SystemCreateTimeUtc, AIH_SystemCreateUser, AIH_SystemLastEditTimeUtc, AIH_SystemLastEditUser)
VALUES (@draftInvoiceHeaderPK, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, '2024-10-10 23:59', 'INV', 'AUD', 'Invoice', '2024-10-10 23:59', 'UT', '2024-10-10 23:59', 'UT')

----------------------------------------                   Test with Parent Department                             --------------------------------------------
-- DepartmentPk2 (0DBEC05B-8289-43BD-8AC9-0F32BC496AB2 -- CIT ) has the Parent Department 3BC44454-A9C4-46A5-A5B1-C704C3605AE9 (COT)

UPDATE dbo.GlbDepartment SET GE_GE = @ParentDepartmentPk, GE_SystemLastEditUser = 'UT', GE_SystemLastEditTimeUtc = '2024-10-13 23:59'
WHERE GE_PK = @DepartmentPk2

INSERT INTO dbo.JobCharge (JR_PK, JR_GC, JR_GB, JR_GE, JR_AL_APLine, JR_AC, JR_JH, JR_EstimatedCost, JR_SystemCreateTimeUtc, JR_SystemCreateUser) VALUES
(@jobChargePK_APINV2, @EDICompanyPk, @SYDBranchPk, @DepartmentPk2, @transactionLinePk_APINV, @accChargeCodePK, @jobHeaderPK_AP, 600.00, '2024-10-11 23:59', 'UT')

----------------------------------------    Job linked with at least one line should be considered (AH_JH is NULL)   --------------------------------------------

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_OH, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled) VALUES
(@transactionHeaderPk_APINV_Job, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @ohPK, '2024-10-13', 'AP', 'INV', '013', '2024-10-13 23:59', 'UT', 'AUD', 900.00, 0)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_AC, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@transactionLinePk_APINV_WithoutJob, @transactionHeaderPk_APINV_Job, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @accChargeCodePK, 'CST', 400.0, 450.00, 'AUD', '2024-10-13 23:59', 'UT')

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_AC, AL_JH, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@transactionLinePk_APINV_WithJob, @transactionHeaderPk_APINV_Job, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @accChargeCodePK, @jobHeaderPK_AP, 'CST', 500.0, 540.00, 'AUD', '2024-10-13 22:59', 'UT')

----------------------------------------                   Test with Foreign Currency                              --------------------------------------------

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_OH, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled) VALUES
(@transactionHeaderPk_APINV_FOC, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @ohPK, '2024-10-15', 'AP', 'INV', '007', '2024-10-15 23:59', 'UT', 'AUD', 900.00, 0)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_AC, AL_JH, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@transactionLinePk_APINV_FOC, @transactionHeaderPk_APINV_FOC, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @accChargeCodePK, @jobHeaderPK_AP_FOC, 'CST', 950.0, 800.00, 'USD', '2024-10-15 22:59', 'UT')

INSERT INTO dbo.JobCharge (JR_PK, JR_GC, JR_GB, JR_GE, JR_AL_APLine, JR_AC, JR_JH, JR_OSCostAmt, JR_EstimatedCost, JR_SystemCreateTimeUtc, JR_SystemCreateUser) VALUES
(@jobChargePK_APINV_FOC, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @transactionLinePk_APINV_FOC, @accChargeCodePK, @jobHeaderPK_AP, 750.00, 800.00, '2024-10-15 23:59', 'UT')

----------------------------------------      The below transaction headers will be ignored for the collector       --------------------------------------------
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_JH, AH_OH, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled) VALUES
(@transactionHeaderPk_APINV_Inactive, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @jobHeaderPK_AP, @ohPK, '2024-10-10', 'AP', 'INV', '002', '2024-10-10 23:59', 'UT', 'AUD', 600.00, 0)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@transactionLinePk_APINV_Inactive, @transactionHeaderPk_APINV_Inactive, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, 'CST', 600.0, 600.00, 'AUD', '2024-10-10 23:59', 'UT')

INSERT INTO dbo.JobCharge (JR_PK, JR_GC, JR_GB, JR_GE, JR_AL_APLine, JR_AC, JR_JH, JR_SystemCreateTimeUtc, JR_SystemCreateUser) VALUES
(@jobChargePK_APINV_Inactive, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @transactionLinePk_APINV_Inactive, @accChargeCodePK, @jobHeaderPK_AP, '2024-10-10 23:59', 'UT')

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_JH, AH_OH, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled) VALUES
(@transactionHeaderPk_APCRD, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @jobHeaderPK_AP, @ohPK, '2024-10-10', 'AP', 'CRD', '001', '2024-10-10 23:59', 'UT', 'AUD', 1200.00, 0)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@transactionLinePk_APCRD, @transactionHeaderPk_APCRD, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, 'CST', 1200.0, 1200.00, 'AUD', '2024-10-10 23:59', 'UT')

INSERT INTO dbo.JobCharge (JR_PK, JR_GC, JR_GB, JR_GE, JR_AL_APLine, JR_AC, JR_JH, JR_SystemCreateTimeUtc, JR_SystemCreateUser) VALUES
(@jobChargePK_APCRD, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @transactionLinePk_APCRD, @accChargeCodePK, @jobHeaderPK_AP, '2024-10-10 23:59', 'UT')

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_JH, AH_OH, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_RX_NKTransactionCurrency, AH_OSTotal, AH_IsCancelled) VALUES
(@transactionHeaderPk_ARINV, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @jobHeaderPK_AP, @ohPK, '2024-10-10', 'AR', 'INV', '001', '2024-10-10 23:59', 'UT', 'AUD', 400.00, 0)

INSERT INTO dbo.AccTransactionLines (AL_PK, AL_AH, AL_GC, AL_GB, AL_GE, AL_LineType, AL_LineAmount, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_SystemCreateTimeUtc, AL_SystemCreateUser) VALUES
(@transactionLinePk_ARINV, @transactionHeaderPk_ARINV, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, 'CST', 1200.0, 1200.00, 'AUD', '2024-10-10 23:59', 'UT')

INSERT INTO dbo.JobCharge (JR_PK, JR_GC, JR_GB, JR_GE, JR_AL_ARLine, JR_AC, JR_JH, JR_SystemCreateTimeUtc, JR_SystemCreateUser) VALUES
(@jobChargePK_ARINV, @EDICompanyPk, @SYDBranchPk, @DepartmentPk, @transactionLinePk_ARINV, @accChargeCodePK, @jobHeaderPK_AP, '2024-10-10 23:59', 'UT')

UPDATE dbo.AccTransactionHeader SET AH_IsCancelled = 1, AH_SystemLastEditTimeUtc = '2024-10-10 23:59', AH_SystemLastEditUser = 'UT' WHERE AH_PK = @transactionHeaderPk_APINV_Inactive
";
			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of AP Invoice Statistics Records", 6, transactions.Count());

			var transactionList = transactions.ToList();

			var expectedTransactions = new List<ExpectedTransaction>()
			{
				new ExpectedTransaction
				{
					BranchCode = "SYD",
					CompanyCode = "EDI",
					AdditionalRefs = "{\"CountryCode\":\"AU\",\"CurrencyCode\":\"AUD\",\"CompanyName\":\"Eagle Datamation International\",\"HeaderCurrencyCode\":\"AUD\",\"InvoiceCreatedUser\":\"UT\",\"InvoiceCreatedDateTime\":\"2024-10-10T23:59:00\",\"LinkedToDraftInvoice\":0,\"OrganizationName\":\"\",\"IsCarrier\":false,\"IsForwarder\":false,\"IsBroker\":false,\"IsServices\":false,\"IsAirCarrier\":false,\"IsSeaCarrier\":0,\"IsRoadTransport\":false,\"ChargeCode\":\"OAQF\",\"ChargeDescription\":\"\",\"ChargeGroupOfTheChargeCode\":\"FRT\",\"JobNumber\":\"J0001\",\"InvoiceLineCurrency\":\"AUD\",\"Apportioned\":0}"
				},
				new ExpectedTransaction
				{
					BranchCode = "SYD",
					CompanyCode = "EDI",
					AdditionalRefs = "{\"CountryCode\":\"AU\",\"CurrencyCode\":\"AUD\",\"CompanyName\":\"Eagle Datamation International\",\"HeaderCurrencyCode\":\"AUD\",\"InvoiceCreatedUser\":\"UT\",\"InvoiceCreatedDateTime\":\"2024-10-10T23:59:00\",\"LinkedToDraftInvoice\":0,\"OrganizationName\":\"\",\"IsCarrier\":false,\"IsForwarder\":false,\"IsBroker\":false,\"IsServices\":false,\"IsAirCarrier\":false,\"IsSeaCarrier\":0,\"IsRoadTransport\":false,\"ChargeCode\":\"OAQF\",\"ChargeDescription\":\"\",\"ChargeGroupOfTheChargeCode\":\"FRT\",\"JobNumber\":\"J0001\",\"InvoiceLineCurrency\":\"AUD\",\"InvoiceLineOSAmount\":0.0000,\"JobChargeCreatedDateTime\":\"2024-10-11T23:59:00\",\"JobChargeCreatedUser\":\"UT\",\"EstimatedCost\":600.0000,\"Apportioned\":0,\"CostAutoRated\":false,\"CostRatingOverride\":false,\"CostRatingOverrideComment\":\"\",\"Department\":\"COT\"}"
				},
				new ExpectedTransaction
				{
					BranchCode = "SYD",
					CompanyCode = "EDI",
					AdditionalRefs = "{\"CountryCode\":\"AU\",\"CurrencyCode\":\"AUD\",\"CompanyName\":\"Eagle Datamation International\",\"HeaderCurrencyCode\":\"AUD\",\"InvoiceCreatedUser\":\"UT\",\"InvoiceCreatedDateTime\":\"2024-10-10T23:59:00\",\"LinkedToDraftInvoice\":0,\"OrganizationName\":\"\",\"IsCarrier\":false,\"IsForwarder\":false,\"IsBroker\":false,\"IsServices\":false,\"IsAirCarrier\":false,\"IsSeaCarrier\":0,\"IsRoadTransport\":false,\"ChargeCode\":\"OAQF\",\"ChargeDescription\":\"\",\"ChargeGroupOfTheChargeCode\":\"FRT\",\"JobNumber\":\"J0001\",\"InvoiceLineCurrency\":\"AUD\",\"InvoiceLineOSAmount\":400.0000,\"JobChargeCreatedDateTime\":\"2024-10-10T23:59:00\",\"JobChargeCreatedUser\":\"UT\",\"EstimatedCost\":500.0000,\"Apportioned\":0,\"CostAutoRated\":false,\"CostRatingOverride\":false,\"CostRatingOverrideComment\":\"\",\"Department\":\"BRN\"}"
				},
				new ExpectedTransaction
				{
					BranchCode = "SYD",
					CompanyCode = "EDI",
					AdditionalRefs = "{\"CountryCode\":\"AU\",\"CurrencyCode\":\"AUD\",\"CompanyName\":\"Eagle Datamation International\",\"HeaderCurrencyCode\":\"AUD\",\"InvoiceCreatedUser\":\"UT\",\"InvoiceCreatedDateTime\":\"2024-10-11T22:59:00\",\"LinkedToDraftInvoice\":0,\"OrganizationName\":\"\",\"IsCarrier\":false,\"IsForwarder\":false,\"IsBroker\":false,\"IsServices\":false,\"IsAirCarrier\":false,\"IsSeaCarrier\":0,\"IsRoadTransport\":false,\"ChargeCode\":\"OAQF\",\"ChargeDescription\":\"\",\"ChargeGroupOfTheChargeCode\":\"FRT\",\"JobNumber\":\"J0003\",\"InvoiceLineCurrency\":\"AUD\",\"InvoiceLineOSAmount\":500.0000,\"JobChargeCreatedDateTime\":\"2024-10-10T23:59:00\",\"JobChargeCreatedUser\":\"UT\",\"EstimatedCost\":500.0000,\"Apportioned\":0,\"CostAutoRated\":false,\"CostRatingOverride\":false,\"CostRatingOverrideComment\":\"\",\"Department\":\"BRN\"}"
				},
				new ExpectedTransaction
				{
					BranchCode = "SYD",
					CompanyCode = "EDI",
					AdditionalRefs = "{\"CountryCode\":\"AU\",\"CurrencyCode\":\"AUD\",\"CompanyName\":\"Eagle Datamation International\",\"HeaderCurrencyCode\":\"AUD\",\"InvoiceCreatedUser\":\"UT\",\"InvoiceCreatedDateTime\":\"2024-10-13T23:59:00\",\"LinkedToDraftInvoice\":0,\"OrganizationName\":\"\",\"IsCarrier\":false,\"IsForwarder\":false,\"IsBroker\":false,\"IsServices\":false,\"IsAirCarrier\":false,\"IsSeaCarrier\":0,\"IsRoadTransport\":false,\"ChargeCode\":\"OAQF\",\"ChargeDescription\":\"\",\"ChargeGroupOfTheChargeCode\":\"FRT\",\"JobNumber\":\"J0001\",\"InvoiceLineCurrency\":\"AUD\",\"Apportioned\":0}"
				},
				new ExpectedTransaction
				{
					BranchCode = "SYD",
					CompanyCode = "EDI",
					AdditionalRefs = "{\"CountryCode\":\"AU\",\"CurrencyCode\":\"AUD\",\"CompanyName\":\"Eagle Datamation International\",\"HeaderCurrencyCode\":\"AUD\",\"InvoiceCreatedUser\":\"UT\",\"InvoiceCreatedDateTime\":\"2024-10-15T23:59:00\",\"LinkedToDraftInvoice\":0,\"OrganizationName\":\"\",\"IsCarrier\":false,\"IsForwarder\":false,\"IsBroker\":false,\"IsServices\":false,\"IsAirCarrier\":false,\"IsSeaCarrier\":0,\"IsRoadTransport\":false,\"ChargeCode\":\"OAQF\",\"ChargeDescription\":\"\",\"ChargeGroupOfTheChargeCode\":\"FRT\",\"JobNumber\":\"J0005\",\"InvoiceLineCurrency\":\"USD\",\"InvoiceLineOSAmount\":750.0000,\"JobChargeCreatedDateTime\":\"2024-10-15T23:59:00\",\"JobChargeCreatedUser\":\"UT\",\"EstimatedCost\":800.0000,\"Apportioned\":0,\"CostAutoRated\":false,\"CostRatingOverride\":false,\"CostRatingOverrideComment\":\"\",\"Department\":\"BRN\"}"
				}
			};

			for (var i = 0; i < expectedTransactions.Count; i++)
			{
				var expectedTransaction = expectedTransactions[i];
				var actualTransaction = transactionList[i];

				AssertEquals("Branch Code", expectedTransaction.BranchCode, actualTransaction.GetBranchCode());
				AssertEquals("Company Code", expectedTransaction.CompanyCode, actualTransaction.GetCompanyCode());
				AssertEquals("Additional Refs", expectedTransaction.AdditionalRefs, actualTransaction.AdditionalRefs);
			}
		}

		public void TestVersion()
		{
			AssertEquals("24.1.29.720", ScriptToTest.MinCW1Version);
			AssertEquals(string.Empty, ScriptToTest.MaxCW1Version);
		}

		class ExpectedTransaction
		{
			public string BranchCode { get; set; }
			public string CompanyCode { get; set; }
			public string AdditionalRefs { get; set; }
		}
	}
}
