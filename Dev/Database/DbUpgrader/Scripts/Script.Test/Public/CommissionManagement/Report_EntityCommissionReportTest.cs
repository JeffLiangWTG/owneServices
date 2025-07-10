using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.CommissionManagement;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.CommissionManagement.Testing
{
	[TestedType(typeof(Report_EntityCommissionReport))]
	class Report_EntityCommissionReportTest : DbCreateScriptTest
	{
		public void TestRecognitionDate()
		{
			var insertSql = @"
DECLARE @CommissionableChargeCodePk UNIQUEIDENTIFIER = '42D6B105-3659-401A-A648-C3EEA6364186';
INSERT INTO dbo.AccChargeCode
	(AC_PK, AC_Code, AC_ChargeGroup, AC_IsCommissionable)
VALUES
	(@CommissionableChargeCodePk, 'XXX', 'FRT', 1)

DECLARE @CompanyPk UNIQUEIDENTIFIER = '915F75BF-50F9-4ED8-A024-F9F4AF1510F8';
DECLARE @BranchPk UNIQUEIDENTIFIER = '9C60AE3F-44F9-4E97-B404-040FA896626E';
DECLARE @DepartmentPk UNIQUEIDENTIFIER = 'DD3E23A1-7C44-4DAE-8F38-ACE05FDBA455';
DECLARE @OrgPk UNIQUEIDENTIFIER = '127D0DCD-D392-4364-8642-4D64A0FAB5DF';
DECLARE @OrgAddress AS UNIQUEIDENTIFIER = '14FEC16A-88BC-4484-9D42-732D2AEF358E'

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'CAU', 'AU company', 'AU', 'AUD')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CompanyPk)
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk, 'TESTORG')
INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES (@OrgAddress, @OrgPk, 'Address 1')

DECLARE @ShipmentPk UNIQUEIDENTIFIER = '35A3EABF-AC2B-4D3A-BA5C-DD7E8F7D3C79';
DECLARE @ShipmentJobPk UNIQUEIDENTIFIER = '50A14E5E-3929-4494-9F1B-9A27F9D9D764';

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef) VALUES (@ShipmentPk, 'S00000005')

INSERT INTO dbo.JobHeader 
	(JH_PK, JH_JobNum, JH_GB, JH_GE, JH_GC, JH_OA_LocalChargesAddr, JH_ParentTableCode, JH_ParentID, JH_GS_NKRepSales, JH_Status) 
VALUES 
	(@ShipmentJobPk, 'S00000005',  @BranchPk, @DepartmentPk, @CompanyPk, @OrgAddress, 'JS', @ShipmentPk, 'SCW', 'CLS')


DECLARE @ARInvoicePk1 UNIQUEIDENTIFIER = '28083B6E-95DC-4925-9B82-370A7D7C939D';
DECLARE @ARInvoicePk2 UNIQUEIDENTIFIER = 'A9A49FB3-5916-4BF9-82F0-468B54921E6F';
DECLARE @APInvoicePk  UNIQUEIDENTIFIER = '55A34AE6-BCB1-466D-8DC0-7E17515B1F46';
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_JH, AH_TransactionNum)
VALUES 
	(@ARInvoicePk1, 'AR', 'JNL', @CompanyPk, @BranchPk, @DepartmentPk, '2002-2-2', '2002-2-2', @ShipmentJobPk, '00001004'),
	(@ARInvoicePk2, 'AR', 'JNL', @CompanyPk, @BranchPk, @DepartmentPk, '2002-2-3', '2002-2-3', @ShipmentJobPk, '00001005'),
	(@APInvoicePk,  'AP', 'JNL', @CompanyPk, @BranchPk, @DepartmentPk, '2001-1-1', '2001-1-1', @ShipmentJobPk, '00001006')

DECLARE @CommissionHeaderPk1 UNIQUEIDENTIFIER = '6A5829A6-D401-44B6-BF8D-26B90B578BFA';
DECLARE @CommissionHeaderPk2 UNIQUEIDENTIFIER = 'F6D37314-EE51-42AE-A788-08C3E3A0EAC9';
INSERT INTO dbo.AccCommissionHeader
	(
		CH0_PK, CH0_GC, CH0_AH_Source, CH0_GroupingSourceID, CH0_GroupingSourceTableCode, CH0_JobNumber,
		CH0_CA0, CH0_OH_Debtor, CH0_OH_Customer, CH0_Product, CH0_Service, CH0_SubModule,
		CH0_CommissionDate, CH0_SnapshotDateTime, CH0_SnapshotEventCode, CH0_OverridenDateTimeUtc,
		CH0_SystemCreateTimeUtc, CH0_SystemCreateUser, CH0_SystemLastEditTimeUtc, CH0_SystemLastEditUser
	)
	VALUES
	(
		@CommissionHeaderPk1, @CompanyPk, @ARInvoicePk1, @ShipmentJobPk, 'JH', 'S00000005',
		NULL, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		'2002-8-8', '2002-2-2', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	),
	(
		@CommissionHeaderPk2, @CompanyPk, @APInvoicePk, @ShipmentJobPk, 'JH', 'S00000005',
		NULL, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		NULL, '2002-1-1', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	)

DECLARE @CommissionableLineGroupPk1 UNIQUEIDENTIFIER = '48D3C6FB-E595-4723-881F-99929BFA5325';
DECLARE @CommissionableLineGroupPk2 UNIQUEIDENTIFIER = '5F9C5058-4D10-441E-A10C-6B8F07E6F922';
INSERT INTO dbo.AccCommissionLineGroup
	(CLG_PK, CLG_CH0, CLG_AC, CLG_CommissionDate, CLG_RX_NKCommissionCurrency, CLG_TotalCommissionableAmount, CLG_SystemCreateTimeUtc, CLG_SystemCreateUser, CLG_SystemLastEditTimeUtc, CLG_SystemLastEditUser)
VALUES
	(@CommissionableLineGroupPk1, @CommissionHeaderPk1, @CommissionableChargeCodePk, NULL, 'AUD', 100, GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(@CommissionableLineGroupPk2, @CommissionHeaderPk2, @CommissionableChargeCodePk, '2002-9-9', 'AUD', -50, GetUtcDate(), 'E', GetUtcDate(), 'E')

DECLARE @CommissionLinePk1 UNIQUEIDENTIFIER = 'CC2C014A-DEC8-4133-B222-A8A55285A9D7';
DECLARE @CommissionLinePk2 UNIQUEIDENTIFIER = 'C91C2179-AD92-4506-A6CB-9A0568EAC6DD';
INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
VALUES
	(@CommissionLinePk1, @CommissionableLineGroupPk1, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', 100, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL,
	GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX'),
	(@CommissionLinePk2, @CommissionableLineGroupPk2, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', -50, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL,
	GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')
";

			TestConnection.ExecuteNonQuery(insertSql);

			var jobPk = Guid.Parse("50A14E5E-3929-4494-9F1B-9A27F9D9D764");

			var reportList = GetReportResult();
			AssertEquals(1, reportList.Count);
			AssertEquals(jobPk, reportList[0].Item1);
			AssertEquals("S00000005", reportList[0].Item2);
			AssertEquals(new DateTime(2002, 8, 8), reportList[0].Item3);

			AssertEquals("Pending", reportList[0].Item4);
			AssertEquals("Yes", reportList[0].Item5);
		}

		public void TestArchivedJob()
		{
			var insertSql = @"
DECLARE @CommissionableChargeCodePk UNIQUEIDENTIFIER = '42D6B105-3659-401A-A648-C3EEA6364186';
INSERT INTO dbo.AccChargeCode
	(AC_PK, AC_Code, AC_ChargeGroup, AC_IsCommissionable)
VALUES
	(@CommissionableChargeCodePk, 'XXX', 'FRT', 1)

DECLARE @CompanyPk UNIQUEIDENTIFIER = '915F75BF-50F9-4ED8-A024-F9F4AF1510F8';
DECLARE @BranchPk UNIQUEIDENTIFIER = '9C60AE3F-44F9-4E97-B404-040FA896626E';
DECLARE @DepartmentPk UNIQUEIDENTIFIER = 'DD3E23A1-7C44-4DAE-8F38-ACE05FDBA455';
DECLARE @OrgPk UNIQUEIDENTIFIER = '127D0DCD-D392-4364-8642-4D64A0FAB5DF';
DECLARE @OrgAddress AS UNIQUEIDENTIFIER = '14FEC16A-88BC-4484-9D42-732D2AEF358E'

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'CAU', 'AU company', 'AU', 'AUD')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CompanyPk)
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk, 'TESTORG')
INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES (@OrgAddress, @OrgPk, 'Address 1')

DECLARE @ShipmentPk UNIQUEIDENTIFIER = '35A3EABF-AC2B-4D3A-BA5C-DD7E8F7D3C79';
DECLARE @ShipmentJobPk UNIQUEIDENTIFIER = '50A14E5E-3929-4494-9F1B-9A27F9D9D764';

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef) VALUES (@ShipmentPk, 'S00000005')

INSERT INTO dbo.JobHeader 
	(JH_PK, JH_JobNum, JH_GB, JH_GE, JH_GC, JH_OA_LocalChargesAddr, JH_ParentTableCode, JH_ParentID, JH_GS_NKRepSales, JH_Status) 
VALUES 
	(@ShipmentJobPk, 'S00000005',  @BranchPk, @DepartmentPk, @CompanyPk, @OrgAddress, 'JS', @ShipmentPk, 'SCW', 'CLS')


DECLARE @ARInvoicePk1 UNIQUEIDENTIFIER = '28083B6E-95DC-4925-9B82-370A7D7C939D';
DECLARE @ARInvoicePk2 UNIQUEIDENTIFIER = 'A9A49FB3-5916-4BF9-82F0-468B54921E6F';

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_JH, AH_TransactionNum)
VALUES 
	(@ARInvoicePk1, 'AR', 'JNL', @CompanyPk, @BranchPk, @DepartmentPk, '2002-2-2', '2002-2-2', @ShipmentJobPk, '00001004'),
	(@ARInvoicePk2, 'AR', 'JNL', @CompanyPk, @BranchPk, @DepartmentPk, '2002-2-3', '2002-2-3', NULL, '00001005')

DECLARE @CommissionHeaderPk1 UNIQUEIDENTIFIER = '6A5829A6-D401-44B6-BF8D-26B90B578BFA';
DECLARE @CommissionHeaderPk2 UNIQUEIDENTIFIER = 'F6D37314-EE51-42AE-A788-08C3E3A0EAC9';
INSERT INTO dbo.AccCommissionHeader
	(
		CH0_PK, CH0_GC, CH0_AH_Source, CH0_GroupingSourceID, CH0_GroupingSourceTableCode,
		CH0_JobNumber, CH0_CA0, CH0_OH_Debtor, CH0_OH_Customer, CH0_Product, CH0_Service, CH0_SubModule,
		CH0_CommissionDate, CH0_SnapshotDateTime, CH0_SnapshotEventCode, CH0_OverridenDateTimeUtc,
		CH0_SystemCreateTimeUtc, CH0_SystemCreateUser, CH0_SystemLastEditTimeUtc, CH0_SystemLastEditUser
	)
	VALUES
	(
		@CommissionHeaderPk1, @CompanyPk, @ARInvoicePk1, @ShipmentJobPk, 'JH',
		'S00000005', NULL, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		'2002-8-8', '2002-2-2', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	),
	(
		@CommissionHeaderPk2, @CompanyPk, @ARInvoicePk2, NULL, 'JH',
		'S00000006', NULL, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		NULL, '2002-1-1', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	)

DECLARE @CommissionableLineGroupPk1 UNIQUEIDENTIFIER = '48D3C6FB-E595-4723-881F-99929BFA5325';
DECLARE @CommissionableLineGroupPk2 UNIQUEIDENTIFIER = '5F9C5058-4D10-441E-A10C-6B8F07E6F922';
INSERT INTO dbo.AccCommissionLineGroup
	(CLG_PK, CLG_CH0, CLG_AC, CLG_CommissionDate, CLG_RX_NKCommissionCurrency, CLG_TotalCommissionableAmount, CLG_SystemCreateTimeUtc, CLG_SystemCreateUser, CLG_SystemLastEditTimeUtc, CLG_SystemLastEditUser)
VALUES
	(@CommissionableLineGroupPk1, @CommissionHeaderPk1, @CommissionableChargeCodePk, NULL, 'AUD', 100, GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(@CommissionableLineGroupPk2, @CommissionHeaderPk2, @CommissionableChargeCodePk, '2002-9-9', 'AUD', -50, GetUtcDate(), 'E', GetUtcDate(), 'E')

DECLARE @CommissionLinePk1 UNIQUEIDENTIFIER = 'CC2C014A-DEC8-4133-B222-A8A55285A9D7';
DECLARE @CommissionLinePk2 UNIQUEIDENTIFIER = 'C91C2179-AD92-4506-A6CB-9A0568EAC6DD';
INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
VALUES
	(@CommissionLinePk1, @CommissionableLineGroupPk1, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', 100, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL,
	GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX'),
	(@CommissionLinePk2, @CommissionableLineGroupPk2, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', -50, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL,
	GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')
";

			TestConnection.ExecuteNonQuery(insertSql);

			var reportList = GetReportResult();
			AssertEquals(2, reportList.Count);
			AssertEquals(1, reportList.Where(r => r.Item2 == "S00000005").Count());
			AssertEquals(1, reportList.Where(r => r.Item2 == "S00000006").Count());
		}

		public void TestCommissionApprovalStatus()
		{
			var sql = @"
DECLARE @CommissionableChargeCodePk UNIQUEIDENTIFIER = '42D6B105-3659-401A-A648-C3EEA6364186';
INSERT INTO dbo.AccChargeCode
	(AC_PK, AC_Code, AC_ChargeGroup, AC_IsCommissionable)
VALUES
	(@CommissionableChargeCodePk, 'XXX', 'FRT', 1)

DECLARE @CompanyPk UNIQUEIDENTIFIER = '915F75BF-50F9-4ED8-A024-F9F4AF1510F8';
DECLARE @BranchPk UNIQUEIDENTIFIER = '9C60AE3F-44F9-4E97-B404-040FA896626E';
DECLARE @DepartmentPk UNIQUEIDENTIFIER = 'DD3E23A1-7C44-4DAE-8F38-ACE05FDBA455';
DECLARE @OrgPk UNIQUEIDENTIFIER = '127D0DCD-D392-4364-8642-4D64A0FAB5DF';
DECLARE @OrgAddress AS UNIQUEIDENTIFIER = '14FEC16A-88BC-4484-9D42-732D2AEF358E'

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'CAU', 'AU company', 'AU', 'AUD')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CompanyPk)
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk, 'TESTORG')
INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES (@OrgAddress, @OrgPk, 'Address 1')

DECLARE @ShipmentPk UNIQUEIDENTIFIER = '35A3EABF-AC2B-4D3A-BA5C-DD7E8F7D3C79';
DECLARE @ShipmentJobPk UNIQUEIDENTIFIER = '50A14E5E-3929-4494-9F1B-9A27F9D9D764';

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef) VALUES (@ShipmentPk, 'S00000005')

INSERT INTO dbo.JobHeader 
	(JH_PK, JH_JobNum, JH_GB, JH_GE, JH_GC, JH_OA_LocalChargesAddr, JH_ParentTableCode, JH_ParentID, JH_GS_NKRepSales, JH_Status) 
VALUES 
	(@ShipmentJobPk, 'S00000005',  @BranchPk, @DepartmentPk, @CompanyPk, @OrgAddress, 'JS', @ShipmentPk, 'SCW', 'CLS')


DECLARE @ARInvoicePk UNIQUEIDENTIFIER = '28083B6E-95DC-4925-9B82-370A7D7C939D';
DECLARE @APInvoicePk UNIQUEIDENTIFIER = '55A34AE6-BCB1-466D-8DC0-7E17515B1F46';
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_JH) 
VALUES 
	(@ARInvoicePk, 'AR', 'JNL', @CompanyPk, @BranchPk, @DepartmentPk, '2002-2-2', '2002-2-2', @ShipmentJobPk),
	(@APInvoicePk, 'AP', 'JNL', @CompanyPk, @BranchPk, @DepartmentPk, '2001-1-1', '2001-1-1', @ShipmentJobPk)

DECLARE @CommissionHeaderPk1 UNIQUEIDENTIFIER = '6A5829A6-D401-44B6-BF8D-26B90B578BFA';
DECLARE @CommissionHeaderPk2 UNIQUEIDENTIFIER = 'F6D37314-EE51-42AE-A788-08C3E3A0EAC9';
INSERT INTO dbo.AccCommissionHeader
	(
		CH0_PK, CH0_GC, CH0_AH_Source, CH0_GroupingSourceID, CH0_GroupingSourceTableCode,
		CH0_CA0, CH0_OH_Debtor, CH0_OH_Customer, CH0_Product, CH0_Service, CH0_SubModule,
		CH0_CommissionDate, CH0_SnapshotDateTime, CH0_SnapshotEventCode, CH0_OverridenDateTimeUtc,
		CH0_SystemCreateTimeUtc, CH0_SystemCreateUser, CH0_SystemLastEditTimeUtc, CH0_SystemLastEditUser
	)
	VALUES
	(
		@CommissionHeaderPk1, @CompanyPk, @ARInvoicePk, @ShipmentJobPk, 'JH',
		NULL, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		'2002-8-8', '2002-2-2', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	),
	(
		@CommissionHeaderPk2, @CompanyPk, @APInvoicePk, @ShipmentJobPk, 'JH',
		NULL, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		'2002-9-9', '2002-1-1', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	)

DECLARE @CommissionableLineGroupPk1 UNIQUEIDENTIFIER = '48D3C6FB-E595-4723-881F-99929BFA5325';
DECLARE @CommissionableLineGroupPk2 UNIQUEIDENTIFIER = '5F9C5058-4D10-441E-A10C-6B8F07E6F922';
INSERT INTO dbo.AccCommissionLineGroup
	(CLG_PK, CLG_CH0, CLG_AC, CLG_RX_NKCommissionCurrency, CLG_TotalCommissionableAmount, CLG_SystemCreateTimeUtc, CLG_SystemCreateUser, CLG_SystemLastEditTimeUtc, CLG_SystemLastEditUser)
VALUES
	(@CommissionableLineGroupPk1, @CommissionHeaderPk1, @CommissionableChargeCodePk, 'AUD', 100 , GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(@CommissionableLineGroupPk2, @CommissionHeaderPk2, @CommissionableChargeCodePk, 'AUD', -50 , GetUtcDate(), 'E', GetUtcDate(), 'E')

DECLARE @CommissionLinePk1 UNIQUEIDENTIFIER = 'CC2C014A-DEC8-4133-B222-A8A55285A9D7';
DECLARE @CommissionLinePk2 UNIQUEIDENTIFIER = 'C91C2179-AD92-4506-A6CB-9A0568EAC6DD';
INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
VALUES
	(@CommissionLinePk1, @CommissionableLineGroupPk1, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', 100, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL,
	GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX'),
	(@CommissionLinePk2, @CommissionableLineGroupPk2, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', -50, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL,
	GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')
";
			TestConnection.ExecuteNonQuery(sql);

			sql = @"UPDATE dbo.AccCommissionLine SET CL0_ApprovedDateTimeUtc = GETUTCDATE(), CL0_SystemLastEditTimeUtc = GETUTCDATE(), CL0_SystemLastEditUser = 'TST' WHERE CL0_PK IN ('CC2C014A-DEC8-4133-B222-A8A55285A9D7', 'C91C2179-AD92-4506-A6CB-9A0568EAC6DD')";
			TestConnection.ExecuteNonQuery(sql);
			var reportList = GetReportResult();
			AssertEquals("Approved", reportList[0].Item4);
			AssertEquals("Yes", reportList[0].Item5);

			sql = @"UPDATE dbo.AccCommissionLine SET CL0_ApprovedDateTimeUtc = NULL, CL0_SystemLastEditTimeUtc = GETUTCDATE(), CL0_SystemLastEditUser = 'TST' WHERE CL0_PK IN ('CC2C014A-DEC8-4133-B222-A8A55285A9D7', 'C91C2179-AD92-4506-A6CB-9A0568EAC6DD')";
			TestConnection.ExecuteNonQuery(sql);
			reportList = GetReportResult();
			AssertEquals("Pending", reportList[0].Item4);
			AssertEquals("Yes", reportList[0].Item5);
		}

		public void TestHasARInvoicesFullyPaid()
		{
			var sql = @"
DECLARE @CommissionableChargeCodePk UNIQUEIDENTIFIER = '42D6B105-3659-401A-A648-C3EEA6364186';
INSERT INTO dbo.AccChargeCode
	(AC_PK, AC_Code, AC_ChargeGroup, AC_IsCommissionable)
VALUES
	(@CommissionableChargeCodePk, 'XXX', 'FRT', 1)

DECLARE @CompanyPk UNIQUEIDENTIFIER = '915F75BF-50F9-4ED8-A024-F9F4AF1510F8';
DECLARE @BranchPk UNIQUEIDENTIFIER = '9C60AE3F-44F9-4E97-B404-040FA896626E';
DECLARE @DepartmentPk UNIQUEIDENTIFIER = 'DD3E23A1-7C44-4DAE-8F38-ACE05FDBA455';
DECLARE @OrgPk UNIQUEIDENTIFIER = '127D0DCD-D392-4364-8642-4D64A0FAB5DF';
DECLARE @OrgAddress AS UNIQUEIDENTIFIER = '14FEC16A-88BC-4484-9D42-732D2AEF358E'

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'CAU', 'AU company', 'AU', 'AUD')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CompanyPk)
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk, 'TESTORG')
INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES (@OrgAddress, @OrgPk, 'Address 1')

DECLARE @ShipmentPk UNIQUEIDENTIFIER = '35A3EABF-AC2B-4D3A-BA5C-DD7E8F7D3C79';
DECLARE @ShipmentJobPk UNIQUEIDENTIFIER = '50A14E5E-3929-4494-9F1B-9A27F9D9D764';

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef) VALUES (@ShipmentPk, 'S00000005')

INSERT INTO dbo.JobHeader 
	(JH_PK, JH_JobNum, JH_GB, JH_GE, JH_GC, JH_OA_LocalChargesAddr, JH_ParentTableCode, JH_ParentID, JH_GS_NKRepSales, JH_Status) 
VALUES 
	(@ShipmentJobPk, 'S00000005',  @BranchPk, @DepartmentPk, @CompanyPk, @OrgAddress, 'JS', @ShipmentPk, 'SCW', 'CLS')


DECLARE @ARInvoicePk UNIQUEIDENTIFIER = '28083B6E-95DC-4925-9B82-370A7D7C939D';
DECLARE @APInvoicePk UNIQUEIDENTIFIER = '55A34AE6-BCB1-466D-8DC0-7E17515B1F46';
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_JH) 
VALUES 
	(@ARInvoicePk, 'AR', 'INV', @CompanyPk, @BranchPk, @DepartmentPk, '2002-2-2', '2002-2-2', @ShipmentJobPk),
	(@APInvoicePk, 'AP', 'INV', @CompanyPk, @BranchPk, @DepartmentPk, '2001-1-1', '2001-1-1', @ShipmentJobPk)

DECLARE @CommissionHeaderPk1 UNIQUEIDENTIFIER = '6A5829A6-D401-44B6-BF8D-26B90B578BFA';
DECLARE @CommissionHeaderPk2 UNIQUEIDENTIFIER = 'F6D37314-EE51-42AE-A788-08C3E3A0EAC9';
INSERT INTO dbo.AccCommissionHeader
	(
		CH0_PK, CH0_GC, CH0_AH_Source, CH0_GroupingSourceID, CH0_GroupingSourceTableCode,
		CH0_CA0, CH0_OH_Debtor, CH0_OH_Customer, CH0_Product, CH0_Service, CH0_SubModule,
		CH0_CommissionDate, CH0_SnapshotDateTime, CH0_SnapshotEventCode, CH0_OverridenDateTimeUtc,
		CH0_SystemCreateTimeUtc, CH0_SystemCreateUser, CH0_SystemLastEditTimeUtc, CH0_SystemLastEditUser
	)
	VALUES
	(
		@CommissionHeaderPk1, @CompanyPk, @ARInvoicePk, @ShipmentJobPk, 'JH',
		NULL, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		'2002-8-8', '2002-2-2', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	),
	(
		@CommissionHeaderPk2, @CompanyPk, @APInvoicePk, @ShipmentJobPk, 'JH',
		NULL, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		'2002-9-9', '2002-1-1', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	)

DECLARE @CommissionableLineGroupPk1 UNIQUEIDENTIFIER = '48D3C6FB-E595-4723-881F-99929BFA5325';
DECLARE @CommissionableLineGroupPk2 UNIQUEIDENTIFIER = '5F9C5058-4D10-441E-A10C-6B8F07E6F922';
INSERT INTO dbo.AccCommissionLineGroup
	(CLG_PK, CLG_CH0, CLG_AC, CLG_RX_NKCommissionCurrency, CLG_TotalCommissionableAmount, CLG_SystemCreateTimeUtc, CLG_SystemCreateUser, CLG_SystemLastEditTimeUtc, CLG_SystemLastEditUser)
VALUES
	(@CommissionableLineGroupPk1, @CommissionHeaderPk1, @CommissionableChargeCodePk, 'AUD', 100, GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(@CommissionableLineGroupPk2, @CommissionHeaderPk2, @CommissionableChargeCodePk, 'AUD', -50, GetUtcDate(), 'E', GetUtcDate(), 'E')

DECLARE @CommissionLinePk1 UNIQUEIDENTIFIER = 'CC2C014A-DEC8-4133-B222-A8A55285A9D7';
DECLARE @CommissionLinePk2 UNIQUEIDENTIFIER = 'C91C2179-AD92-4506-A6CB-9A0568EAC6DD';
INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
VALUES
	(@CommissionLinePk1, @CommissionableLineGroupPk1, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', 100, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL,
	GETUTCDATE(), 'XXX', GetUtcDate(), 'XXX'),
	(@CommissionLinePk2, @CommissionableLineGroupPk2, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', -50, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL,
	GETUTCDATE(), 'XXX', GetUtcDate(), 'XXX')
";
			TestConnection.ExecuteNonQuery(sql);

			sql = @"UPDATE dbo.AccTransactionHeader SET AH_FullyPaidDate = NULL, AH_TransactionType = 'INV', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_Ledger = 'AR' AND AH_PK = '28083B6E-95DC-4925-9B82-370A7D7C939D'";
			TestConnection.ExecuteNonQuery(sql);
			var reportList = GetReportResult();
			AssertEquals("No", reportList[0].Item5);

			sql = @"UPDATE dbo.AccTransactionHeader SET AH_FullyPaidDate = GETUTCDATE(), AH_TransactionType = 'INV', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_Ledger = 'AR' AND AH_PK = '28083B6E-95DC-4925-9B82-370A7D7C939D'";
			TestConnection.ExecuteNonQuery(sql);
			reportList = GetReportResult();
			AssertEquals("Yes", reportList[0].Item5);
		}

		public void TestAPAndARTransactionsCorrectlyGrouped()
		{
			var sql = @"
DECLARE @CommissionableChargeCodePk UNIQUEIDENTIFIER = '42D6B105-3659-401A-A648-C3EEA6364186';
INSERT INTO dbo.AccChargeCode
	(AC_PK, AC_Code, AC_ChargeGroup, AC_IsCommissionable)
VALUES
	(@CommissionableChargeCodePk, 'XXX', 'FRT', 1)

DECLARE @CompanyPk UNIQUEIDENTIFIER = '915F75BF-50F9-4ED8-A024-F9F4AF1510F8';
DECLARE @BranchPk UNIQUEIDENTIFIER = '9C60AE3F-44F9-4E97-B404-040FA896626E';
DECLARE @DepartmentPk UNIQUEIDENTIFIER = 'DD3E23A1-7C44-4DAE-8F38-ACE05FDBA455';
DECLARE @OrgPk UNIQUEIDENTIFIER = '127D0DCD-D392-4364-8642-4D64A0FAB5DF';
DECLARE @OrgAddress AS UNIQUEIDENTIFIER = '14FEC16A-88BC-4484-9D42-732D2AEF358E'

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'CAU', 'AU company', 'AU', 'AUD')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CompanyPk)
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk, 'TESTORG')
INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES (@OrgAddress, @OrgPk, 'Address 1')

DECLARE @ShipmentPk UNIQUEIDENTIFIER = '35A3EABF-AC2B-4D3A-BA5C-DD7E8F7D3C79';
DECLARE @ShipmentJobPk UNIQUEIDENTIFIER = '50A14E5E-3929-4494-9F1B-9A27F9D9D764';

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef) VALUES (@ShipmentPk, 'S00000005')

INSERT INTO dbo.JobHeader 
	(JH_PK, JH_JobNum, JH_GB, JH_GE, JH_GC, JH_OA_LocalChargesAddr, JH_ParentTableCode, JH_ParentID, JH_GS_NKRepSales, JH_Status) 
VALUES 
	(@ShipmentJobPk, 'S00000005',  @BranchPk, @DepartmentPk, @CompanyPk, @OrgAddress, 'JS', @ShipmentPk, 'SCW', 'CLS')


DECLARE @ARInvoicePk UNIQUEIDENTIFIER = '28083B6E-95DC-4925-9B82-370A7D7C939D';
DECLARE @APInvoicePk UNIQUEIDENTIFIER = '55A34AE6-BCB1-466D-8DC0-7E17515B1F46';
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_JH) 
VALUES 
	(@ARInvoicePk, 'AR', 'INV', @CompanyPk, @BranchPk, @DepartmentPk, '2002-2-2', '2002-2-2', @ShipmentJobPk),
	(@APInvoicePk, 'AP', 'INV', @CompanyPk, @BranchPk, @DepartmentPk, '2001-1-1', '2001-1-1', @ShipmentJobPk)

DECLARE @CommissionHeaderPk1 UNIQUEIDENTIFIER = '6A5829A6-D401-44B6-BF8D-26B90B578BFA';
DECLARE @CommissionHeaderPk2 UNIQUEIDENTIFIER = 'F6D37314-EE51-42AE-A788-08C3E3A0EAC9';
INSERT INTO dbo.AccCommissionHeader
	(
		CH0_PK, CH0_GC, CH0_AH_Source, CH0_GroupingSourceID, CH0_GroupingSourceTableCode,
		CH0_CA0, CH0_OH_Debtor, CH0_OH_Customer, CH0_Product, CH0_Service, CH0_SubModule,
		CH0_CommissionDate, CH0_SnapshotDateTime, CH0_SnapshotEventCode, CH0_OverridenDateTimeUtc,
		CH0_SystemCreateTimeUtc, CH0_SystemCreateUser, CH0_SystemLastEditTimeUtc, CH0_SystemLastEditUser
	)
	VALUES
	(
		@CommissionHeaderPk1, @CompanyPk, @ARInvoicePk, @ShipmentJobPk, 'JH',
		NULL, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		'2002-8-8', '2002-2-2', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	),
	(
		@CommissionHeaderPk2, @CompanyPk, @APInvoicePk, @ShipmentJobPk, 'JH',
		NULL, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		'2002-9-9', '2002-1-1', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	)

DECLARE @CommissionableLineGroupPk1 UNIQUEIDENTIFIER = '48D3C6FB-E595-4723-881F-99929BFA5325';
DECLARE @CommissionableLineGroupPk2 UNIQUEIDENTIFIER = '5F9C5058-4D10-441E-A10C-6B8F07E6F922';
INSERT INTO dbo.AccCommissionLineGroup
	(CLG_PK, CLG_CH0, CLG_AC, CLG_RX_NKCommissionCurrency, CLG_TotalCommissionableAmount, CLG_SystemCreateTimeUtc, CLG_SystemCreateUser, CLG_SystemLastEditTimeUtc, CLG_SystemLastEditUser)
VALUES
	(@CommissionableLineGroupPk1, @CommissionHeaderPk1, @CommissionableChargeCodePk, 'AUD', 100, GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(@CommissionableLineGroupPk2, @CommissionHeaderPk2, @CommissionableChargeCodePk, 'AUD', -50, GetUtcDate(), 'E', GetUtcDate(), 'E')

DECLARE @CommissionLinePk1 UNIQUEIDENTIFIER = 'CC2C014A-DEC8-4133-B222-A8A55285A9D7';
DECLARE @CommissionLinePk2 UNIQUEIDENTIFIER = 'C91C2179-AD92-4506-A6CB-9A0568EAC6DD';
INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
VALUES
	(@CommissionLinePk1, @CommissionableLineGroupPk1, 'CLG', NULL, '', @OrgPk, 'PCT',
	'AUD', 100, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL,
	GETUTCDATE(), 'XXX', GetUtcDate(), 'XXX'),
	(@CommissionLinePk2, @CommissionableLineGroupPk2, 'CLG', NULL, '', @OrgPk, 'PCT',
	'AUD', -50, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL,
	GETUTCDATE(), 'XXX', GetUtcDate(), 'XXX')
";
			TestConnection.ExecuteNonQuery(sql);

			var selectRowCountSql = "SELECT COUNT(*) FROM Report_EntityCommissionReport()";

			using (var command = TestConnection.Command(selectRowCountSql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					AssertEquals("Only 1 row should be returned since AP and AR lines are grouped.", 1, (int)reader[0]);
				}
			}

			sql = @"UPDATE dbo.AccTransactionHeader SET AH_FullyPaidDate = GETUTCDATE(), AH_TransactionType = 'INV', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_Ledger = 'AR' AND AH_PK = '28083B6E-95DC-4925-9B82-370A7D7C939D'";
			TestConnection.ExecuteNonQuery(sql);

			using (var command = TestConnection.Command(selectRowCountSql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					AssertEquals("Only 1 row should be returned since AP and AR lines are grouped.", 1, (int)reader[0]);
				}
			}
		}

		List<Tuple<Guid, string, DateTime, string, string, string>> GetReportResult()
		{
			var reportList = new List<Tuple<Guid, string, DateTime, string, string, string>>();
			var selectSql = $@"SELECT GroupingSourceID, GroupingNumber, RecognitionDate, ApprovalStatus, HasARInvoicesFullyPaid, PaymentStatus FROM Report_EntityCommissionReport()";
			using (var command = TestConnection.Command(selectSql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var groupID = reader["GroupingSourceID"] == DBNull.Value ? Guid.Empty : (Guid)reader["GroupingSourceID"];
					var groupingNumber = reader["GroupingNumber"] == DBNull.Value ? string.Empty : (string)reader["GroupingNumber"];
					var recongnitionDate = (DateTime)reader["RecognitionDate"];
					var approvalStatus = (string)reader["ApprovalStatus"];
					var hasARInvoicesFullyPaid = (string)reader["HasARInvoicesFullyPaid"];
					var paymentStatus = (string)reader["PaymentStatus"];

					reportList.Add(new Tuple<Guid, string, DateTime, string, string, string>(groupID, groupingNumber, recongnitionDate, approvalStatus, hasARInvoicesFullyPaid, paymentStatus));
				}
			}

			return reportList;
		}
	}
}
