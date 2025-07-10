using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.CommissionManagement;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.CommissionManagement.Testing
{
	[TestedType(typeof(Report_EntityCommissionReport_LineDetails))]
	class Report_EntityCommissionReport_LineDetailsTest : DbCreateScriptTest
	{
		public void TestDoNotIncludeNonCommissionableUnlessApprovedOrPaid()
		{
			var insertSql = @"
DECLARE @CommissionableChargeCodePk UNIQUEIDENTIFIER = '42D6B105-3659-401A-A648-C3EEA6364186';
DECLARE @UncommissionableChargeCodePk UNIQUEIDENTIFIER = '123F836C-E145-4505-B418-9F12936047A7';
INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_IsCommissionable)
VALUES
	(@CommissionableChargeCodePk, 'XXX', 'FRT', 1),
	(@UncommissionableChargeCodePk, 'YYY', 'FRT', 0)

DECLARE @CompanyPk UNIQUEIDENTIFIER = '915F75BF-50F9-4ED8-A024-F9F4AF1510F8';
DECLARE @BranchPk UNIQUEIDENTIFIER = '9C60AE3F-44F9-4E97-B404-040FA896626E';
DECLARE @DepartmentPk UNIQUEIDENTIFIER = 'DD3E23A1-7C44-4DAE-8F38-ACE05FDBA455';
DECLARE @OrgPk UNIQUEIDENTIFIER = '127D0DCD-D392-4364-8642-4D64A0FAB5DF';

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'CAU', 'AU company', 'AU', 'AUD')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CompanyPk)
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk, 'TESTORG')

DECLARE @PerPk UNIQUEIDENTIFIER = newid()
INSERT INTO dbo.GlbPerson(PER_PK, PER_FullName) values (@PerPk, 'name')

DECLARE @StaffCode VARCHAR(3) = 'ADL'
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (NEWID(), @StaffCode, @PerPk, GETUTCDATE(), 'E', GETUTCDATE(), 'E')

DECLARE @InvoicePk UNIQUEIDENTIFIER = '28083B6E-95DC-4925-9B82-370A7D7C939D';
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType) VALUES (@InvoicePk, @CompanyPk, @BranchPk, @DepartmentPk, '2002-2-2', 'AR', 'JNL')

DECLARE @CommissionHeaderPk UNIQUEIDENTIFIER = '6A5829A6-D401-44B6-BF8D-26B90B578BFA';
INSERT INTO dbo.AccCommissionHeader
	(
		CH0_PK, CH0_GC, CH0_AH_Source, CH0_GroupingSourceID, CH0_GroupingSourceTableCode,
		CH0_CA0, CH0_OH_Debtor, CH0_OH_Customer, CH0_Product, CH0_Service, CH0_SubModule,
		CH0_CommissionDate, CH0_SnapshotDateTime, CH0_SnapshotEventCode, CH0_OverridenDateTimeUtc,
		CH0_SystemCreateTimeUtc, CH0_SystemCreateUser, CH0_SystemLastEditTimeUtc, CH0_SystemLastEditUser
	)
	VALUES
	(
		@CommissionHeaderPk, @CompanyPk, @InvoicePk, @InvoicePk, 'AH',
		NULL, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		'2002-2-2', '2002-2-2', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	)

DECLARE @CommissionableLineGroupPk UNIQUEIDENTIFIER = '48D3C6FB-E595-4723-881F-99929BFA5325';
DECLARE @UncommissionableLineGroupPk UNIQUEIDENTIFIER = '85DF893A-3198-4F46-A485-27B6CF09EE43';
INSERT INTO dbo.AccCommissionLineGroup
	(CLG_PK, CLG_CH0, CLG_AC, CLG_RX_NKCommissionCurrency, CLG_TotalCommissionableAmount, CLG_SystemCreateTimeUtc, CLG_SystemCreateUser, CLG_SystemLastEditTimeUtc, CLG_SystemLastEditUser)
VALUES
	(@CommissionableLineGroupPk, @CommissionHeaderPk, @CommissionableChargeCodePk, 'AUD', 100, GetUtcDate(), 'E', GetUtcDate(), 'E'),
	(@UncommissionableLineGroupPk, @CommissionHeaderPk, @UncommissionableChargeCodePk, 'AUD', 100, GetUtcDate(), 'E', GetUtcDate(), 'E')

DECLARE @CommissionableUnapprovedLinePk UNIQUEIDENTIFIER = 'CC2C014A-DEC8-4133-B222-A8A55285A9D7';
DECLARE @CommissionableApprovedLinePk UNIQUEIDENTIFIER = '2D937830-1E7B-4C44-9334-BCC955A54303';
DECLARE @CommissionablePaidLinePk UNIQUEIDENTIFIER = 'B41BD189-1393-43E0-A073-EA13259563A0';
DECLARE @UncommissionableUnapprovedLinePk UNIQUEIDENTIFIER = 'E5C497A5-DA55-434D-B2D0-E58DB95E5B7D';
DECLARE @UncommissionableApprovedLinePk UNIQUEIDENTIFIER = 'B9FAA32A-5478-4F8A-855B-320773548135';
DECLARE @UncommissionablePaidLinePk UNIQUEIDENTIFIER = '357C0B86-2C74-4234-A760-DCD6797CFD7E';

INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
VALUES
	(@CommissionableUnapprovedLinePk, @CommissionableLineGroupPk, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', 100, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL, GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX'),

	(@CommissionableApprovedLinePk, @CommissionableLineGroupPk, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', 100, 4, 1, 25, 25,
	'2004-4-4', NULL, NULL, NULL, NULL, GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX'),

	(@CommissionablePaidLinePk, @CommissionableLineGroupPk, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', 100, 4, 1, 25, 25,
	'2004-4-4', '2005-5-5', NULL, NULL, NULL, GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX'),


	(@UncommissionableUnapprovedLinePk, @UncommissionableLineGroupPk, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', 100, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL, GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX'),

	(@UncommissionableApprovedLinePk, @UncommissionableLineGroupPk, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', 100, 4, 1, 25, 25,
	'2004-4-4', NULL, NULL, NULL, NULL, GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX'),

	(@UncommissionablePaidLinePk, @UncommissionableLineGroupPk, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', 100, 4, 1, 25, 25,
	'2004-4-4', '2005-5-5', NULL, NULL, NULL, GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')
		";

			TestConnection.ExecuteNonQuery(insertSql);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					Guid.Parse("CC2C014A-DEC8-4133-B222-A8A55285A9D7"),
					Guid.Parse("2D937830-1E7B-4C44-9334-BCC955A54303"),
					Guid.Parse("B41BD189-1393-43E0-A073-EA13259563A0"),
					Guid.Parse("B9FAA32A-5478-4F8A-855B-320773548135"),
					Guid.Parse("357C0B86-2C74-4234-A760-DCD6797CFD7E"),
				},
				GetAllCommissionLinePks());
		}

		public void TestDoNotIncludeNonCommissionableUnlessApprovedOrPaid2()
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

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef) VALUES (@ShipmentPk, 'S00001005')

INSERT INTO dbo.JobHeader 
	(JH_PK, JH_JobNum, JH_GB, JH_GE, JH_GC, JH_OA_LocalChargesAddr, JH_ParentTableCode, JH_ParentID, JH_GS_NKRepSales, JH_Status) 
VALUES 
	(@ShipmentJobPk, 'S00001005',  @BranchPk, @DepartmentPk, @CompanyPk, @OrgAddress, 'JS', @ShipmentPk, 'SCW', 'CLS')


DECLARE @ARInvoicePk UNIQUEIDENTIFIER = '28083B6E-95DC-4925-9B82-370A7D7C939D';
DECLARE @APInvoicePk UNIQUEIDENTIFIER = '55A34AE6-BCB1-466D-8DC0-7E17515B1F46';
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_FullyPaidDate, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_JH) 
VALUES 
	(@ARInvoicePk, 'AR', 'INV', GETUTCDATE(), @CompanyPk, @BranchPk, @DepartmentPk, '2002-2-2', '2002-2-2', @ShipmentJobPk),
	(@APInvoicePk, 'AP', 'INV', NULL, @CompanyPk, @BranchPk, @DepartmentPk, '2001-1-1', '2001-1-1', @ShipmentJobPk)

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
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, Cl0_SystemLastEditUser)
VALUES
	(@CommissionLinePk1, @CommissionableLineGroupPk1, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', 100, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL, GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX'),
	(@CommissionLinePk2, @CommissionableLineGroupPk2, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', -50, 4, 1, 25, 25,
	GETUTCDATE(), NULL, NULL, NULL, NULL, GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')
";

			TestConnection.ExecuteNonQuery(sql);
			var reportList = GetReportResult().ToList();

			var commissionHeader1Pk = new Guid("6A5829A6-D401-44B6-BF8D-26B90B578BFA");
			var commissionHeader2Pk = new Guid("F6D37314-EE51-42AE-A788-08C3E3A0EAC9");

			Assert(reportList.Any(x => x.ApprovalStatus == "Pending" && x.CommissionHeaderPK == commissionHeader1Pk));
			Assert(reportList.Any(x => x.ApprovalStatus == "Approved" && x.CommissionHeaderPK == commissionHeader2Pk));

			sql = @"UPDATE dbo.AccTransactionHeader SET AH_FullyPaidDate = NULL, AH_TransactionType = 'INV', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_Ledger = 'AR' AND AH_PK = '28083B6E-95DC-4925-9B82-370A7D7C939D'";
			TestConnection.ExecuteNonQuery(sql);
			reportList = GetReportResult();
			Assert(reportList.Any(x => x.HasARInvoicesFullyPaid == "No" && x.CommissionHeaderPK == commissionHeader1Pk));

			sql = @"UPDATE dbo.AccTransactionHeader SET AH_FullyPaidDate = GETUTCDATE(), AH_TransactionType = 'INV', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_Ledger = 'AR' AND AH_PK = '28083B6E-95DC-4925-9B82-370A7D7C939D'";
			TestConnection.ExecuteNonQuery(sql);
			reportList = GetReportResult();
			Assert(reportList.Any(x => x.HasARInvoicesFullyPaid == "Yes" && x.CommissionHeaderPK == commissionHeader1Pk));
		}

		IEnumerable<Guid> GetAllCommissionLinePks()
		{
			var selectSql = @"SELECT PK FROM Report_EntityCommissionReport_LineDetails()";

			using (var command = TestConnection.Command(selectSql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					yield return (Guid)reader["PK"];
				}
			}
		}

		public void TestArchivedJob()
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

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef) VALUES (@ShipmentPk, 'S00001005')

INSERT INTO dbo.JobHeader 
	(JH_PK, JH_JobNum, JH_GB, JH_GE, JH_GC, JH_OA_LocalChargesAddr, JH_ParentTableCode, JH_ParentID, JH_GS_NKRepSales, JH_Status) 
VALUES 
	(@ShipmentJobPk, 'S00001005',  @BranchPk, @DepartmentPk, @CompanyPk, @OrgAddress, 'JS', @ShipmentPk, 'SCW', 'CLS')


DECLARE @ARInvoicePk UNIQUEIDENTIFIER = '28083B6E-95DC-4925-9B82-370A7D7C939D';
DECLARE @APInvoicePk UNIQUEIDENTIFIER = '55A34AE6-BCB1-466D-8DC0-7E17515B1F46';
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_FullyPaidDate, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_JH) 
VALUES 
	(@ARInvoicePk, 'AR', 'INV', GETUTCDATE(), @CompanyPk, @BranchPk, @DepartmentPk, '2002-2-2', '2002-2-2', @ShipmentJobPk),
	(@APInvoicePk, 'AP', 'INV', NULL, @CompanyPk, @BranchPk, @DepartmentPk, '2001-1-1', '2001-1-1', NULL)

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
		@CommissionHeaderPk1, @CompanyPk, @ARInvoicePk, @ShipmentJobPk, 'JH',
		'S00001005', NULL, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		'2002-8-8', '2002-2-2', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	),
	(
		@CommissionHeaderPk2, @CompanyPk, @APInvoicePk, NULL, 'JH',
		'S00001006', NULL, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
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
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, Cl0_SystemLastEditUser)
VALUES
	(@CommissionLinePk1, @CommissionableLineGroupPk1, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', 100, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL, GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX'),
	(@CommissionLinePk2, @CommissionableLineGroupPk2, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', -50, 4, 1, 25, 25,
	GETUTCDATE(), NULL, NULL, NULL, NULL, GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')
";

			TestConnection.ExecuteNonQuery(sql);
			var reportList = GetReportResult().ToList();

			AssertEquals(2, reportList.Count);
			AssertEquals(1, reportList.Where(r => r.GroupingNumber == "S00001005").Count());
			AssertEquals(1, reportList.Where(r => r.GroupingNumber == "S00001006").Count());
		}

		public void TestRecognitionDate()
		{
			SetupTestData();

			var line1Pk = Guid.Parse("CC2C014A-DEC8-4133-B222-A8A55285A9D7");
			var line2Pk = Guid.Parse("C91C2179-AD92-4506-A6CB-9A0568EAC6DD");

			var reportList = GetReportResult();
			AssertEquals(2, reportList.Count);
			Assert(reportList.Any(x => x.CommissionLinePK == line1Pk && x.RecognitionDate == new DateTime(2002, 8, 8)));
			Assert(reportList.Any(x => x.CommissionLinePK == line2Pk && x.RecognitionDate == new DateTime(2002, 9, 9)));
		}

		public void TestCommissionHeaderPK()
		{
			SetupTestData();

			var line1Pk = Guid.Parse("CC2C014A-DEC8-4133-B222-A8A55285A9D7");
			var line2Pk = Guid.Parse("C91C2179-AD92-4506-A6CB-9A0568EAC6DD");
			var reportList = GetReportResult();

			AssertEquals(2, reportList.Count);
			Assert(reportList.Any(x => x.CommissionLinePK == line1Pk && x.CommissionHeaderPK == new Guid("6A5829A6-D401-44B6-BF8D-26B90B578BFA")));
			Assert(reportList.Any(x => x.CommissionLinePK == line2Pk && x.CommissionHeaderPK == new Guid("F6D37314-EE51-42AE-A788-08C3E3A0EAC9")));
		}

		public void TestCommissionHeaderDetails()
		{
			SetupTestData();

			var line1Pk = Guid.Parse("CC2C014A-DEC8-4133-B222-A8A55285A9D7");
			var line2Pk = Guid.Parse("C91C2179-AD92-4506-A6CB-9A0568EAC6DD");

			var reportList = GetReportResult();

			AssertEquals(2, reportList.Count);
			Assert("Report should return Product, Service and SubModule", reportList.Any(x => x.CommissionLinePK == line1Pk && x.Product == "ALL" && x.Service == "ALL" && x.SubModule == "ALL"));
			Assert("Report should return Product, Service and SubModule", reportList.Any(x => x.CommissionLinePK == line2Pk && x.Product == "ABC" && x.Service == "DEF" && x.SubModule == "GHI"));
		}

		void SetupTestData()
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

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef) VALUES (@ShipmentPk, 'S00001005')

INSERT INTO dbo.JobHeader 
	(JH_PK, JH_JobNum, JH_GB, JH_GE, JH_GC, JH_OA_LocalChargesAddr, JH_ParentTableCode, JH_ParentID, JH_GS_NKRepSales, JH_Status) 
VALUES 
	(@ShipmentJobPk, 'S00001005',  @BranchPk, @DepartmentPk, @CompanyPk, @OrgAddress, 'JS', @ShipmentPk, 'SCW', 'CLS')


DECLARE @ARInvoicePk1 UNIQUEIDENTIFIER = '28083B6E-95DC-4925-9B82-370A7D7C939D';
DECLARE @ARInvoicePk2 UNIQUEIDENTIFIER = 'A9A49FB3-5916-4BF9-82F0-468B54921E6F';
DECLARE @APInvoicePk  UNIQUEIDENTIFIER = '55A34AE6-BCB1-466D-8DC0-7E17515B1F46';
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_JH, AH_TransactionNum) 
VALUES 
	(@ARInvoicePk1, 'AR', 'INV', @CompanyPk, @BranchPk, @DepartmentPk, '2002-2-2', '2002-2-2', @ShipmentJobPk, '00001004'),
	(@ARInvoicePk2, 'AR', 'INV', @CompanyPk, @BranchPk, @DepartmentPk, '2002-2-3', '2002-2-3', @ShipmentJobPk, '00001005'),
	(@APInvoicePk,  'AP', 'INV', @CompanyPk, @BranchPk, @DepartmentPk, '2001-1-1', '2001-1-1', @ShipmentJobPk, '00001006')

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
		@CommissionHeaderPk1, @CompanyPk, @ARInvoicePk1, @ShipmentJobPk, 'JH',
		NULL, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		'2002-8-8', '2002-2-2', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	),
	(
		@CommissionHeaderPk2, @CompanyPk, @APInvoicePk, @ShipmentJobPk, 'JH',
		NULL, @OrgPk, @OrgPk, 'ABC', 'DEF', 'GHI',
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
	NULL, NULL, NULL, NULL, NULL, GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX'),
	(@CommissionLinePk2, @CommissionableLineGroupPk2, 'CLG', NULL, '', @OrgPk, 'FIX',
	'AUD', -50, 4, 1, 25, 25,
	NULL, NULL, NULL, NULL, NULL, GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')
";

			TestConnection.ExecuteNonQuery(insertSql);
		}

		List<ReportData> GetReportResult()
		{
			var reportList = new List<ReportData>();
			var selectSql = $@"
SELECT
	PK, CommissionHeaderPk, GroupingNumber, RecognitionDate, ApprovalStatus, HasARInvoicesFullyPaid, PaymentStatus, Product, Service, SubModule
FROM
	Report_EntityCommissionReport_LineDetails()";
			using (var command = TestConnection.Command(selectSql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var reportData = new ReportData();

					reportData.CommissionLinePK = (Guid)reader["PK"];
					reportData.CommissionHeaderPK = (Guid)reader["CommissionHeaderPk"];
					reportData.GroupingNumber = reader["GroupingNumber"] == DBNull.Value ? string.Empty : (string)reader["GroupingNumber"];
					reportData.RecognitionDate = (DateTime)reader["RecognitionDate"];
					reportData.ApprovalStatus = (string)reader["ApprovalStatus"];
					reportData.HasARInvoicesFullyPaid = (string)reader["HasARInvoicesFullyPaid"];
					reportData.PaymentStatus = (string)reader["PaymentStatus"];
					reportData.Product = (string)reader["Product"];
					reportData.Service = (string)reader["Service"];
					reportData.SubModule = (string)reader["SubModule"];

					reportList.Add(reportData);
				}
			}

			return reportList;
		}

		struct ReportData
		{
			public Guid CommissionLinePK;
			public Guid CommissionHeaderPK;
			public string GroupingNumber;
			public DateTime RecognitionDate;
			public string ApprovalStatus;
			public string HasARInvoicesFullyPaid;
			public string PaymentStatus;
			public string Product;
			public string Service;
			public string SubModule;
		}
	}
}

