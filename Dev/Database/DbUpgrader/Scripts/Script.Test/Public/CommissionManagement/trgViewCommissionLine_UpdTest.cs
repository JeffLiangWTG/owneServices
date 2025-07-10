using System;
using CargoWise.DbUpgrader.Scripts.Definitions.CommissionManagement;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.CommissionManagement
{
	[TestedType(typeof(trgViewCommissionLine_Upd))]
	class trgViewCommissionLine_Upd_Test : DbCreateScriptTest
	{
		public void TestUpdate()
		{
			var insertSql = @"
DECLARE @BranchPk UNIQUEIDENTIFIER = '9C60AE3F-44F9-4E97-B404-040FA896626E';
DECLARE @RevenueAudCompanyPk UNIQUEIDENTIFIER = '56D49995-9742-491F-8C7E-B244C7E30FFD';
DECLARE @PreferredAudCompanyPk UNIQUEIDENTIFIER = '35CB76A6-6317-4339-8EB5-50BB5127563F';
DECLARE @PreferredGbpCompanyPk UNIQUEIDENTIFIER = '5C0888A6-6B3C-4BAA-AF01-0E6D3B4BD08F';
DECLARE @DepartmentPk UNIQUEIDENTIFIER = 'DD3E23A1-7C44-4DAE-8F38-ACE05FDBA455';
DECLARE @OrgPk UNIQUEIDENTIFIER = '127D0DCD-D392-4364-8642-4D64A0FAB5DF'
DECLARE @PartyPk UNIQUEIDENTIFIER = 'D42F1D28-F99A-4051-9F50-3C1A3ECF1C3C'
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES
	(@RevenueAudCompanyPk, 'RAU', 'AU company1', 'AU', 'AUD'),
	(@PreferredAudCompanyPk, 'PAU', 'AU company2', 'AU', 'AUD'),
	(@PreferredGbpCompanyPk, 'PGB', 'AU company3', 'AU', 'GBP')

INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @RevenueAudCompanyPk)
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk, 'TESTORG')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@PartyPk, 'TESTPARTY')
INSERT INTO dbo.OrgMiscServ (OM_PK, OM_OH, OM_GC_CMPreferredPaymentCompany) VALUES (NEWID(), @PartyPk, @PreferredAudCompanyPk)

DECLARE @ChargeCodePk UNIQUEIDENTIFIER = '42D6B105-3659-401A-A648-C3EEA6364186';
INSERT INTO dbo.AccChargeCode (AC_PK, AC_ChargeGroup) VALUES (@ChargeCodePk, 'FRT')

DECLARE @PerPk UNIQUEIDENTIFIER = newid()
INSERT INTO dbo.GlbPerson(PER_PK, PER_FullName) values (@PerPk, 'name')

DECLARE @StaffCode VARCHAR(3) = 'ADL'
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_GC_PreferredPaymentCompany, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (NEWID(), @StaffCode, @PreferredGbpCompanyPk, @PerPk, GETUTCDATE(), 'E', GETUTCDATE(), 'E')

DECLARE @InvoicePk UNIQUEIDENTIFIER = '28083B6E-95DC-4925-9B82-370A7D7C939D';
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType) VALUES (@InvoicePk, @RevenueAudCompanyPk, @BranchPk, @DepartmentPk, '2002-2-2', 'AR', 'JNL')

DECLARE @OpportunityPk UNIQUEIDENTIFIER = '2AA124A1-ADC2-430E-9569-393F28F3D1BB';
DECLARE @AgreementPk UNIQUEIDENTIFIER = '6669A19A-4469-483F-9AA4-E8A2FDC34DB8';
DECLARE @StaffAgreementRecipientPk UNIQUEIDENTIFIER = '8714558F-8A43-46FD-82F3-577C7695DD87';
DECLARE @PartyAgreementRecipientPk UNIQUEIDENTIFIER = '5AD20841-A761-4BDF-A8CB-F6BE5D86F2D4';
DECLARE @StaffAgreementRatePk UNIQUEIDENTIFIER = 'F2297418-DF7B-4328-896F-DCE1F1F11DA9';
DECLARE @PartyAgreementRatePk UNIQUEIDENTIFIER = '84B25660-09B4-42BD-806E-D7CDCB7AB366';

INSERT INTO dbo.OrgOpportunity (P8_PK, P8_GC, P8_OH, P8_OpportunityID, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser) VALUES (@OpportunityPk, @RevenueAudCompanyPk, @OrgPk, 'OppId', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_P8, CA0_Name, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser) VALUES (@AgreementPk, @OpportunityPk, '#1', @OrgPk, '1AR', 'REV', GETUTCDATE(), GetUtcDate(), 'XX', GetUtcDate(), 'XX')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_GS_NKStaff, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser) VALUES (@StaffAgreementRecipientPk, @AgreementPk, 'ADL', GetUtcDate(), 'XX', GetUtcDate(), 'XX')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser) VALUES (@PartyAgreementRecipientPk, @AgreementPk, @PartyPk, GetUtcDate(), 'XX', GetUtcDate(), 'XX')
INSERT INTO dbo.OrgCommissionAgreementRecipientRate (CAT_PK, CAT_CAR, CAT_SystemCreateTimeUtc, CAT_SystemCreateUser, CAT_SystemLastEditTimeUtc, CAT_SystemLastEditUser) VALUES (@StaffAgreementRatePk, @StaffAgreementRecipientPk, GetUtcDate(), 'XX', GetUtcDate(), 'XX')
INSERT INTO dbo.OrgCommissionAgreementRecipientRate (CAT_PK, CAT_CAR, CAT_SystemCreateTimeUtc, CAT_SystemCreateUser, CAT_SystemLastEditTimeUtc, CAT_SystemLastEditUser) VALUES (@PartyAgreementRatePk, @PartyAgreementRecipientPk, GetUtcDate(), 'XX', GetUtcDate(), 'XX')


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
		@CommissionHeaderPk, @RevenueAudCompanyPk, @InvoicePk, @InvoicePk, 'AH',
		@AgreementPk, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		'2002-2-2', '2002-2-2', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	)

	
DECLARE @CommissionLineGroupPk UNIQUEIDENTIFIER = '48D3C6FB-E595-4723-881F-99929BFA5325';
DECLARE @PctCommissionLinePk UNIQUEIDENTIFIER = '2D937830-1E7B-4C44-9334-BCC955A54303';
DECLARE @FixCommissionLinePk UNIQUEIDENTIFIER = '8AC47D5C-43B2-4F68-A527-A861AC8135DF';


INSERT INTO dbo.AccCommissionLineGroup (CLG_PK, CLG_CH0, CLG_AC, CLG_RX_NKCommissionCurrency, CLG_TotalCommissionableAmount, CLG_SystemCreateTimeUtc, CLG_SystemCreateUser, CLG_SystemLastEditTimeUtc, CLG_SystemLastEditUser)
	VALUES (@CommissionLineGroupPk, @CommissionHeaderPk, @ChargeCodePk, 'AUD', 100, GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityPercentage, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
	VALUES (@PctCommissionLinePk, @CommissionLineGroupPk, 'CLG', @StaffAgreementRatePk, 'ADL', NULL, 'PCT',
		'AUD', 150, 3, 1, 50, 9.6, 4.8,
		'2003-3-3', NULL, '2003-3-3', '2004-4-4', NULL,
		GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')

INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityPercentage, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
	VALUES (@FixCommissionLinePk, @CommissionHeaderPk, 'CH0', @PartyAgreementRatePk, '', @PartyPk, 'FIX',
		'USD', 50, 2, 1, 25, 100, 25,
		'2004-4-4', NULL, NULL, NULL, NULL,
		GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')


INSERT INTO dbo.RefExchangeRate (RE_PK, RE_GC, RE_RX_NKExCurrency, RE_ExRateType, RE_StartDate, RE_ExpiryDate, RE_SellRate, RE_AsPublished) VALUES
	(newid(), @RevenueAudCompanyPk, 'USD', 'SEL', '2002-2-1', '2002-3-1', 0.8, ''),
	(newid(), @RevenueAudCompanyPk, 'USD', 'SEL', '2002-3-1', '2002-4-1', 0.9, ''),
	(newid(), @RevenueAudCompanyPk, 'GBP', 'SEL', '2002-2-1', '2002-3-1', 0.5, ''),
	(newid(), @RevenueAudCompanyPk, 'GBP', 'SEL', '2002-3-1', '2002-4-1', 0.6, '')
";

			using (var command = TestConnection.Command(insertSql))
			{
				command.ExecuteNonQuery();
			}

			var pctLine = GetLine("PCT");
			AssertEquals("Pre-condition", new DateTime(2004, 4, 4), pctLine.VCL_CancelledDateTimeUtc);
			AssertEquals("Pre-condition", new DateTime(2003, 3, 3), pctLine.VCL_ApprovedDateTimeUtc);
			var fixLine = GetLine("FIX");
			AssertEquals("Pre-condition", DBNull.Value, fixLine.VCL_CancelledDateTimeUtc);
			AssertEquals("Pre-condition", new DateTime(2004, 4, 4), fixLine.VCL_ApprovedDateTimeUtc);

			// Update one
			TestConnection.ExecuteNonQuery("Update dbo.ViewCommissionLine set VCL_CancelledDateTimeUtc = '2020-5-1' where VCL_CommissionType = 'FIX'");
			pctLine = GetLine("PCT");
			AssertEquals("no change", new DateTime(2004, 4, 4), pctLine.VCL_CancelledDateTimeUtc);
			AssertEquals("no change", new DateTime(2003, 3, 3), pctLine.VCL_ApprovedDateTimeUtc);
			fixLine = GetLine("FIX");
			AssertEquals("should have updated", new DateTime(2020, 5, 1), fixLine.VCL_CancelledDateTimeUtc);
			AssertEquals("no change", new DateTime(2004, 4, 4), fixLine.VCL_ApprovedDateTimeUtc);

			TestConnection.ExecuteNonQuery("Update dbo.ViewCommissionLine set VCL_ApprovedDateTimeUtc = '2020-6-1' where VCL_CommissionType = 'FIX'");
			pctLine = GetLine("PCT");
			AssertEquals("no change", new DateTime(2004, 4, 4), pctLine.VCL_CancelledDateTimeUtc);
			AssertEquals("no change", new DateTime(2003, 3, 3), pctLine.VCL_ApprovedDateTimeUtc);
			fixLine = GetLine("FIX");
			AssertEquals("no change", new DateTime(2020, 5, 1), fixLine.VCL_CancelledDateTimeUtc);
			AssertEquals("should have updated", new DateTime(2020, 6, 1), fixLine.VCL_ApprovedDateTimeUtc);

			TestConnection.ExecuteNonQuery("Update dbo.ViewCommissionLine set VCL_CancelledDateTimeUtc = '2020-7-1', VCL_ApprovedDateTimeUtc = '2020-8-1' where VCL_CommissionType = 'FIX'");
			pctLine = GetLine("PCT");
			AssertEquals("no change", new DateTime(2004, 4, 4), pctLine.VCL_CancelledDateTimeUtc);
			AssertEquals("no change", new DateTime(2003, 3, 3), pctLine.VCL_ApprovedDateTimeUtc);
			fixLine = GetLine("FIX");
			AssertEquals("should have updated", new DateTime(2020, 7, 1), fixLine.VCL_CancelledDateTimeUtc);
			AssertEquals("should have updated", new DateTime(2020, 8, 1), fixLine.VCL_ApprovedDateTimeUtc);

			// Update both
			TestConnection.ExecuteNonQuery("Update dbo.ViewCommissionLine set VCL_CancelledDateTimeUtc = null");
			pctLine = GetLine("PCT");
			AssertEquals("should have updated", DBNull.Value, pctLine.VCL_CancelledDateTimeUtc);
			AssertEquals("no change", new DateTime(2003, 3, 3), pctLine.VCL_ApprovedDateTimeUtc);
			fixLine = GetLine("FIX");
			AssertEquals("should have updated", DBNull.Value, fixLine.VCL_CancelledDateTimeUtc);
			AssertEquals("no change", new DateTime(2020, 8, 1), fixLine.VCL_ApprovedDateTimeUtc);

			TestConnection.ExecuteNonQuery("Update dbo.ViewCommissionLine set VCL_ApprovedDateTimeUtc = null");
			pctLine = GetLine("PCT");
			AssertEquals("no change", DBNull.Value, pctLine.VCL_CancelledDateTimeUtc);
			AssertEquals("should have updated", DBNull.Value, pctLine.VCL_ApprovedDateTimeUtc);
			fixLine = GetLine("FIX");
			AssertEquals("no change", DBNull.Value, fixLine.VCL_CancelledDateTimeUtc);
			AssertEquals("should have updated", DBNull.Value, fixLine.VCL_ApprovedDateTimeUtc);

			TestConnection.ExecuteNonQuery("Update dbo.ViewCommissionLine set VCL_CancelledDateTimeUtc = '2020-9-1', VCL_ApprovedDateTimeUtc = '2020-10-1'");
			pctLine = GetLine("PCT");
			AssertEquals("should have updated", new DateTime(2020, 9, 1), pctLine.VCL_CancelledDateTimeUtc);
			AssertEquals("should have updated", new DateTime(2020, 10, 1), pctLine.VCL_ApprovedDateTimeUtc);
			fixLine = GetLine("FIX");
			AssertEquals("should have updated", new DateTime(2020, 9, 1), fixLine.VCL_CancelledDateTimeUtc);
			AssertEquals("should have updated", new DateTime(2020, 10, 1), fixLine.VCL_ApprovedDateTimeUtc);

			AssertExceptionThrown<SqlException>("ViewCommissionLine only supports update of VCL_CancelledDateTimeUtc and VCL_ApprovedDateTimeUtc", () =>
			{
				TestConnection.ExecuteNonQuery("Update dbo.ViewCommissionLine set VCL_OverridenDateTimeUtc = '2020-11-2'");
			});

			(object VCL_CancelledDateTimeUtc, object VCL_ApprovedDateTimeUtc) GetLine(string commissionType)
			{
				var selectSql = "SELECT VCL_CancelledDateTimeUtc, VCL_ApprovedDateTimeUtc FROM dbo.ViewCommissionLine where VCL_CommissionType = @commissionType";
				using (var command = TestConnection.Command(selectSql))
				{
					command.AddParameterBasedOnDbColumn("@commissionType", commissionType, ViewCommissionLineSchema.VCL_CommissionType);
					using (var reader = command.ExecuteReader())
					{
						reader.Read();
						return (reader[ViewCommissionLineSchema.Constants.VCL_CancelledDateTimeUtc], reader[ViewCommissionLineSchema.Constants.VCL_ApprovedDateTimeUtc]);
					}
				}
			}
		}
	}
}

