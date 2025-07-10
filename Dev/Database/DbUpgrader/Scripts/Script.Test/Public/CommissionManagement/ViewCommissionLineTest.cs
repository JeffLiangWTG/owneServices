using System;
using System.Linq;
using System.Text;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.CommissionManagement;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.CommissionManagement.Test
{
	[TestedType(typeof(ViewCommissionLine))]
	class ViewCommissionLineTest : DbCreateScriptTest
	{
		public void TestGeneralUsage()
		{
			var preferredPaymentSql = @"
DECLARE @PreferredAudCompanyPk UNIQUEIDENTIFIER = '35CB76A6-6317-4339-8EB5-50BB5127563F';
DECLARE @PreferredGbpCompanyPk UNIQUEIDENTIFIER = '5C0888A6-6B3C-4BAA-AF01-0E6D3B4BD08F';

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES
	(@PreferredAudCompanyPk, 'PAU', 'AU company1', 'AU', 'AUD'),
	(@PreferredGbpCompanyPk, 'PGB', 'AU company2', 'AU', 'GBP')

INSERT INTO dbo.OrgMiscServ (OM_PK, OM_OH, OM_GC_CMPreferredPaymentCompany) VALUES (NEWID(), @PartyPk, @PreferredAudCompanyPk)

DECLARE @PerPk UNIQUEIDENTIFIER = newid()
INSERT INTO dbo.GlbPerson(PER_PK, PER_FullName) values (@PerPk, 'name')

DECLARE @StaffCode VARCHAR(3) = 'ADL'
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_GC_PreferredPaymentCompany, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (NEWID(), @StaffCode, @PreferredGbpCompanyPk, @PerPk, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";

			var commissionsSql = @"
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


INSERT INTO dbo.AccCommissionLineGroup (CLG_PK, CLG_CH0, CLG_AC, CLG_RX_NKCommissionCurrency, CLG_TotalCommissionableAmount, CLG_CommissionDate, CLG_SystemCreateTimeUtc, CLG_SystemCreateUser, CLG_SystemLastEditTimeUtc, CLG_SystemLastEditUser)
	VALUES (@CommissionLineGroupPk, @CommissionHeaderPk, @ChargeCodePk, 'AUD', 100, '2002-3-2', GetUtcDate(), 'E', GetUtcDate(), 'E')
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
		'2004-4-4', '2005-5-5', NULL, NULL, NULL,
		GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')
";
			var insertSql = new StringBuilder();
			insertSql.AppendLine(AgreementCreationSql);
			insertSql.AppendLine(InvoiceCreationSql);
			insertSql.AppendLine(preferredPaymentSql);
			insertSql.AppendLine(commissionsSql);
			insertSql.AppendLine(ExchangeRateCreationSql);

			using (var command = TestConnection.Command(insertSql.ToString()))
			{
				command.ExecuteNonQuery();
			}

			var revenueAudCompanyPk = Guid.Parse("56D49995-9742-491F-8C7E-B244C7E30FFD");
			var preferredAudCompanyPk = Guid.Parse("35CB76A6-6317-4339-8EB5-50BB5127563F");
			var preferredGbpCompanyPk = Guid.Parse("5C0888A6-6B3C-4BAA-AF01-0E6D3B4BD08F");
			var partyPk = Guid.Parse("D42F1D28-F99A-4051-9F50-3C1A3ECF1C3C");
			var agreementPk = Guid.Parse("6669A19A-4469-483F-9AA4-E8A2FDC34DB8");
			var staffAgreementRatePk = Guid.Parse("F2297418-DF7B-4328-896F-DCE1F1F11DA9");
			var partyAgreementRatePk = Guid.Parse("84B25660-09B4-42BD-806E-D7CDCB7AB366");
			var commissionHeaderPk = Guid.Parse("6A5829A6-D401-44B6-BF8D-26B90B578BFA");
			var chargeCodePk = Guid.Parse("42D6B105-3659-401A-A648-C3EEA6364186");

			var selectSql = "SELECT * FROM dbo.ViewCommissionLine ORDER BY VCL_CommissionType";
			using (var command = TestConnection.Command(selectSql))
			using (var reader = command.ExecuteReader())
			{
				Assert(reader.Read());
				CombineAssertions(() =>
				{
					AssertEquals("VCL_CommissionType", "FIX", reader[ViewCommissionLineSchema.Constants.VCL_CommissionType]);
					AssertEquals("VCL_AC", DBNull.Value, reader[ViewCommissionLineSchema.Constants.VCL_AC]);
					AssertEquals("VCL_ApprovedDateTimeUtc", new DateTime(2004, 4, 4), reader[ViewCommissionLineSchema.Constants.VCL_ApprovedDateTimeUtc]);
					AssertEquals("VCL_BelongsToGroup", DBNull.Value, reader[ViewCommissionLineSchema.Constants.VCL_BelongsToGroup]);
					AssertEquals("VCL_CA0", agreementPk, reader[ViewCommissionLineSchema.Constants.VCL_CA0]);
					AssertEquals("VCL_OverridenDateTimeUtc", DBNull.Value, reader[ViewCommissionLineSchema.Constants.VCL_OverridenDateTimeUtc]);
					AssertEquals("VCL_CAT", partyAgreementRatePk, reader[ViewCommissionLineSchema.Constants.VCL_CAT]);
					AssertEquals("VCL_CH0", commissionHeaderPk, reader[ViewCommissionLineSchema.Constants.VCL_CH0]);
					AssertEquals("VCL_CommissionDate", new DateTime(2002, 2, 2), reader[ViewCommissionLineSchema.Constants.VCL_CommissionDate]);
					AssertEquals("VCL_EntityCommissionAmount", 25m, reader[ViewCommissionLineSchema.Constants.VCL_EntityCommissionAmount]);
					AssertEquals("VCL_EntityPercentage", 100m, reader[ViewCommissionLineSchema.Constants.VCL_EntityPercentage]);
					AssertEquals("VCL_CommissionToLocalExchangeRate", 1 / 0.8m, reader[ViewCommissionLineSchema.Constants.VCL_CommissionToLocalExchangeRate]);
					AssertEquals("VCL_GC_Company", revenueAudCompanyPk, reader[ViewCommissionLineSchema.Constants.VCL_GC_Company]);
					AssertEquals("VCL_GC_PreferredPaymentCompany", preferredAudCompanyPk, reader[ViewCommissionLineSchema.Constants.VCL_GC_PreferredPaymentCompany]);
					AssertEquals("VCL_GS_NKStaff", "", reader[ViewCommissionLineSchema.Constants.VCL_GS_NKStaff]);
					AssertEquals("VCL_OH_Party", partyPk, reader[ViewCommissionLineSchema.Constants.VCL_OH_Party]);
					AssertEquals("VCL_PaidDateTimeUtc", new DateTime(2005, 5, 5), reader[ViewCommissionLineSchema.Constants.VCL_PaidDateTimeUtc]);
					AssertEquals("VCL_CancelledDateTimeUtc", DBNull.Value, reader[ViewCommissionLineSchema.Constants.VCL_CancelledDateTimeUtc]);
					AssertEquals("VCL_LocalToPreferredExchangeRate", 1m, reader[ViewCommissionLineSchema.Constants.VCL_LocalToPreferredExchangeRate]);
					AssertEquals("VCL_RX_NKCommissionCurrency", "USD", reader[ViewCommissionLineSchema.Constants.VCL_RX_NKCommissionCurrency]);
					AssertEquals("VCL_RX_NKPreferredPaymentCurrency", "AUD", reader[ViewCommissionLineSchema.Constants.VCL_RX_NKPreferredPaymentCurrency]);
					AssertEquals("VCL_RX_NKLocalCurrency", "AUD", reader[ViewCommissionLineSchema.Constants.VCL_RX_NKLocalCurrency]);
					AssertEquals("VCL_ShareCommissionAmount", 25m, reader[ViewCommissionLineSchema.Constants.VCL_ShareCommissionAmount]);
					AssertEquals("VCL_SharePercentage", 50m, reader[ViewCommissionLineSchema.Constants.VCL_SharePercentage]);
					AssertEquals("VCL_SharePortion", (byte)1, reader[ViewCommissionLineSchema.Constants.VCL_SharePortion]);
					AssertEquals("VCL_ShareTotal", (short)2, reader[ViewCommissionLineSchema.Constants.VCL_ShareTotal]);
					AssertEquals("VCL_TotalCommissionableAmount", 50m, reader[ViewCommissionLineSchema.Constants.VCL_TotalCommissionableAmount]);
				});

				Assert(reader.Read());
				CombineAssertions(() =>
				{
					AssertEquals("VCL_CommissionType", "PCT", reader[ViewCommissionLineSchema.Constants.VCL_CommissionType]);
					AssertEquals("VCL_AC", chargeCodePk, reader[ViewCommissionLineSchema.Constants.VCL_AC]);
					AssertEquals("VCL_ApprovedDateTimeUtc", new DateTime(2003, 3, 3), reader[ViewCommissionLineSchema.Constants.VCL_ApprovedDateTimeUtc]);
					AssertEquals("VCL_BelongsToGroup", DBNull.Value, reader[ViewCommissionLineSchema.Constants.VCL_BelongsToGroup]);
					AssertEquals("VCL_CA0", agreementPk, reader[ViewCommissionLineSchema.Constants.VCL_CA0]);
					AssertEquals("VCL_OverridenDateTimeUtc", new DateTime(2003, 3, 3), reader[ViewCommissionLineSchema.Constants.VCL_OverridenDateTimeUtc]);
					AssertEquals("VCL_CAT", staffAgreementRatePk, reader[ViewCommissionLineSchema.Constants.VCL_CAT]);
					AssertEquals("VCL_CH0", commissionHeaderPk, reader[ViewCommissionLineSchema.Constants.VCL_CH0]);
					AssertEquals("VCL_CommissionDate", new DateTime(2002, 3, 2), reader[ViewCommissionLineSchema.Constants.VCL_CommissionDate]);
					AssertEquals("VCL_EntityCommissionAmount", 4.8m, reader[ViewCommissionLineSchema.Constants.VCL_EntityCommissionAmount]);
					AssertEquals("VCL_EntityPercentage", 9.6m, reader[ViewCommissionLineSchema.Constants.VCL_EntityPercentage]);
					AssertEquals("VCL_CommissionToLocalExchangeRate", 1m, reader[ViewCommissionLineSchema.Constants.VCL_CommissionToLocalExchangeRate]);
					AssertEquals("VCL_GC_PreferredPaymentCompany", preferredGbpCompanyPk, reader[ViewCommissionLineSchema.Constants.VCL_GC_PreferredPaymentCompany]);
					AssertEquals("VCL_GS_NKStaff", "ADL", reader[ViewCommissionLineSchema.Constants.VCL_GS_NKStaff]);
					AssertEquals("VCL_OH_Party", DBNull.Value, reader[ViewCommissionLineSchema.Constants.VCL_OH_Party]);
					AssertEquals("VCL_PaidDateTimeUtc", DBNull.Value, reader[ViewCommissionLineSchema.Constants.VCL_PaidDateTimeUtc]);
					AssertEquals("VCL_CancelledDateTimeUtc", new DateTime(2004, 4, 4), reader[ViewCommissionLineSchema.Constants.VCL_CancelledDateTimeUtc]);
					AssertEquals("VCL_LocalToPreferredExchangeRate", 0.5m, reader[ViewCommissionLineSchema.Constants.VCL_LocalToPreferredExchangeRate]);
					AssertEquals("VCL_RX_NKCommissionCurrency", "AUD", reader[ViewCommissionLineSchema.Constants.VCL_RX_NKCommissionCurrency]);
					AssertEquals("VCL_RX_NKPreferredPaymentCurrency", "GBP", reader[ViewCommissionLineSchema.Constants.VCL_RX_NKPreferredPaymentCurrency]);
					AssertEquals("VCL_RX_NKLocalCurrency", "AUD", reader[ViewCommissionLineSchema.Constants.VCL_RX_NKLocalCurrency]);
					AssertEquals("VCL_ShareCommissionAmount", 50m, reader[ViewCommissionLineSchema.Constants.VCL_ShareCommissionAmount]);
					AssertEquals("VCL_SharePercentage", 33.33m, reader[ViewCommissionLineSchema.Constants.VCL_SharePercentage]);
					AssertEquals("VCL_SharePortion", (byte)1, reader[ViewCommissionLineSchema.Constants.VCL_SharePortion]);
					AssertEquals("VCL_ShareTotal", (short)3, reader[ViewCommissionLineSchema.Constants.VCL_ShareTotal]);
					AssertEquals("VCL_TotalCommissionableAmount", 150m, reader[ViewCommissionLineSchema.Constants.VCL_TotalCommissionableAmount]);
				});

				Assert(!reader.Read());
			}
		}

		public void TestExchangeRate()
		{
			var preferredPaymentSql = @"
DECLARE @PreferredUsdCompanyPk UNIQUEIDENTIFIER = '35CB76A6-6317-4339-8EB5-50BB5127563F';
DECLARE @PreferredGbpCompanyPk UNIQUEIDENTIFIER = '5C0888A6-6B3C-4BAA-AF01-0E6D3B4BD08F';

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency) VALUES
	(@PreferredUsdCompanyPk, 'PAU', 'AU company', 'USD'),
	(@PreferredGbpCompanyPk, 'PGB', 'GB company', 'GBP')

INSERT INTO dbo.OrgMiscServ (OM_PK, OM_OH, OM_GC_CMPreferredPaymentCompany) VALUES (NEWID(), @PartyPk, @PreferredUsdCompanyPk)
DECLARE @PerPk UNIQUEIDENTIFIER = newid()
INSERT INTO dbo.GlbPerson(PER_PK, PER_FullName) values (@PerPk, 'name')

DECLARE @StaffCode VARCHAR(3) = 'ADL'
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_GC_PreferredPaymentCompany, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (NEWID(), @StaffCode, @PreferredGbpCompanyPk, @PerPk, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";

			var commissionsSql = @"
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


INSERT INTO dbo.AccCommissionLineGroup (CLG_PK, CLG_CH0, CLG_AC, CLG_RX_NKCommissionCurrency, CLG_TotalCommissionableAmount, CLG_CommissionDate, CLG_SystemCreateTimeUtc, CLG_SystemCreateUser, CLG_SystemLastEditTimeUtc, CLG_SystemLastEditUser)
	VALUES (@CommissionLineGroupPk, @CommissionHeaderPk, @ChargeCodePk, 'AUD', 100, '2002-2-2', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityPercentage, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
	VALUES (@PctCommissionLinePk, @CommissionLineGroupPk, 'CLG', @StaffAgreementRatePk, 'ADL', NULL, 'PCT',
		'GBP', 150, 3, 1, 50, 9.6, 4.8,
		'2003-3-3', NULL, '2003-3-3', '2004-4-4', NULL,
		GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')

INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityPercentage, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
	VALUES (@FixCommissionLinePk, @CommissionHeaderPk, 'CH0', @PartyAgreementRatePk, '', @PartyPk, 'FIX',
		'USD', 50, 2, 1, 25, 100, 25,
		'2004-4-4', '2005-5-5', NULL, NULL, NULL,
		GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')

DECLARE @CommissionHeader2Pk UNIQUEIDENTIFIER = '52E281F0-113D-4FDF-9350-F4E80F2476E0';
INSERT INTO dbo.AccCommissionHeader
	(
		CH0_PK, CH0_GC, CH0_AH_Source, CH0_GroupingSourceID, CH0_GroupingSourceTableCode,
		CH0_CA0, CH0_OH_Debtor, CH0_OH_Customer, CH0_Product, CH0_Service, CH0_SubModule,
		CH0_CommissionDate, CH0_SnapshotDateTime, CH0_SnapshotEventCode, CH0_OverridenDateTimeUtc,
		CH0_SystemCreateTimeUtc, CH0_SystemCreateUser, CH0_SystemLastEditTimeUtc, CH0_SystemLastEditUser
	)
	VALUES
	(
		@CommissionHeader2Pk, @RevenueAudCompanyPk, @InvoicePk, @InvoicePk, 'AH',
		@AgreementPk, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		'2002-3-2', '2002-2-2', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	)

	
DECLARE @CommissionLineGroup2Pk UNIQUEIDENTIFIER = 'DB8169B1-D6F4-4B26-A4FF-C4E2F5547123';
DECLARE @PctCommissionLine2Pk UNIQUEIDENTIFIER = '62179772-679B-411D-8D5B-76356AF2B13C';
DECLARE @FixCommissionLine2Pk UNIQUEIDENTIFIER = '96AADB40-876C-4E9B-B44E-FFB944C40C18';


INSERT INTO dbo.AccCommissionLineGroup (CLG_PK, CLG_CH0, CLG_AC, CLG_RX_NKCommissionCurrency, CLG_TotalCommissionableAmount, CLG_CommissionDate, CLG_SystemCreateTimeUtc, CLG_SystemCreateUser, CLG_SystemLastEditTimeUtc, CLG_SystemLastEditUser)
	VALUES (@CommissionLineGroup2Pk, @CommissionHeader2Pk, @ChargeCodePk, 'AUD', 100, '2002-3-2', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityPercentage, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
	VALUES (@PctCommissionLine2Pk, @CommissionLineGroup2Pk, 'CLG', @StaffAgreementRatePk, '', NULL, 'PCT',
		'AUD', 150, 3, 1, 50, 9.6, 4.8,
		'2003-3-3', NULL, '2003-3-3', '2004-4-4', NULL,
		GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')

INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityPercentage, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
	VALUES (@FixCommissionLine2Pk, @CommissionHeader2Pk, 'CH0', @PartyAgreementRatePk, '', @PartyPk, 'FIX',
		'AUD', 50, 2, 1, 25, 100, 25,
		'2004-4-4', '2005-5-5', NULL, NULL, NULL,
		GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')
";
			var insertSql = new StringBuilder();
			insertSql.AppendLine(AgreementCreationSql);
			insertSql.AppendLine(InvoiceCreationSql); 
			insertSql.AppendLine(preferredPaymentSql);
			insertSql.AppendLine(commissionsSql);
			insertSql.AppendLine(ExchangeRateCreationSql);
			using (var command = TestConnection.Command(insertSql.ToString()))
			{
				command.ExecuteNonQuery();
			}

			var preferredUsdCompanyPk = Guid.Parse("35CB76A6-6317-4339-8EB5-50BB5127563F");
			var preferredGbpCompanyPk = Guid.Parse("5C0888A6-6B3C-4BAA-AF01-0E6D3B4BD08F");
			var revenueAudCompanyPk = Guid.Parse("56D49995-9742-491F-8C7E-B244C7E30FFD");

			var selectSql = "SELECT * FROM dbo.ViewCommissionLine ORDER BY VCL_CommissionDate, VCL_CommissionType";
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				using (var command = TestConnection.Command(selectSql))
				using (var reader = command.ExecuteReader())
				{
					Assert(reader.Read());
					CombineAssertions(() =>
					{
						AssertEquals("VCL_CommissionDate", new DateTime(2002, 2, 2), reader[ViewCommissionLineSchema.Constants.VCL_CommissionDate]);
						AssertEquals("VCL_CommissionType", "FIX", reader[ViewCommissionLineSchema.Constants.VCL_CommissionType]);
						AssertEquals("VCL_CommissionToLocalExchangeRate", 1.25m, reader[ViewCommissionLineSchema.Constants.VCL_CommissionToLocalExchangeRate]);
						AssertEquals("VCL_GC_PreferredPaymentCompany", preferredUsdCompanyPk, reader[ViewCommissionLineSchema.Constants.VCL_GC_PreferredPaymentCompany]);
						AssertEquals("VCL_LocalToPreferredExchangeRate", 0.8m, reader[ViewCommissionLineSchema.Constants.VCL_LocalToPreferredExchangeRate]);
					});

					Assert(reader.Read());
					CombineAssertions(() =>
					{
						AssertEquals("VCL_CommissionDate", new DateTime(2002, 2, 2), reader[ViewCommissionLineSchema.Constants.VCL_CommissionDate]);
						AssertEquals("VCL_CommissionType", "PCT", reader[ViewCommissionLineSchema.Constants.VCL_CommissionType]);
						AssertEquals("VCL_CommissionToLocalExchangeRate", 2m, reader[ViewCommissionLineSchema.Constants.VCL_CommissionToLocalExchangeRate]);
						AssertEquals("VCL_GC_PreferredPaymentCompany", preferredGbpCompanyPk, reader[ViewCommissionLineSchema.Constants.VCL_GC_PreferredPaymentCompany]);
						AssertEquals("VCL_LocalToPreferredExchangeRate", 0.5m, reader[ViewCommissionLineSchema.Constants.VCL_LocalToPreferredExchangeRate]);
					});

					Assert(reader.Read());
					CombineAssertions(() =>
					{
						AssertEquals("VCL_CommissionDate", new DateTime(2002, 3, 2), reader[ViewCommissionLineSchema.Constants.VCL_CommissionDate]);
						AssertEquals("VCL_CommissionType", "FIX", reader[ViewCommissionLineSchema.Constants.VCL_CommissionType]);
						AssertEquals("VCL_CommissionToLocalExchangeRate", 1m, reader[ViewCommissionLineSchema.Constants.VCL_CommissionToLocalExchangeRate]);
						AssertEquals("VCL_GC_PreferredPaymentCompany", preferredUsdCompanyPk, reader[ViewCommissionLineSchema.Constants.VCL_GC_PreferredPaymentCompany]);
						AssertEquals("VCL_LocalToPreferredExchangeRate", 0.9m, reader[ViewCommissionLineSchema.Constants.VCL_LocalToPreferredExchangeRate]);
					});

					Assert(reader.Read());
					CombineAssertions(() =>
					{
						AssertEquals("VCL_CommissionDate", new DateTime(2002, 3, 2), reader[ViewCommissionLineSchema.Constants.VCL_CommissionDate]);
						AssertEquals("VCL_CommissionType", "PCT", reader[ViewCommissionLineSchema.Constants.VCL_CommissionType]);
						AssertEquals("VCL_CommissionToLocalExchangeRate", 1m, reader[ViewCommissionLineSchema.Constants.VCL_CommissionToLocalExchangeRate]);
						AssertEquals("VCL_GC_PreferredPaymentCompany", revenueAudCompanyPk, reader[ViewCommissionLineSchema.Constants.VCL_GC_PreferredPaymentCompany]);
						AssertEquals("VCL_LocalToPreferredExchangeRate", 1m, reader[ViewCommissionLineSchema.Constants.VCL_LocalToPreferredExchangeRate]);
					});

					Assert(!reader.Read());
				}

				var allQueryPlans = TestConnection.ExecutedCommandsAndQueryPlans;
				var viewQueryPlan = allQueryPlans?.FirstOrDefault(p => p.Item1.Contains("ViewCommissionLine"));
				AssertNotNull("Should be Query Plan for ViewCommissionLine", viewQueryPlan);
				var planalyzer = new QueryPlanalyzer(viewQueryPlan.Item2.Last());
				var indexOperations = planalyzer.IndexSeeks.Union(planalyzer.IndexScans);
				var otherOperationFound = indexOperations.Where(o => o.TableName.Contains("RefExchangeRate")).Any(i => i.IndexName != "FK_RC__RE_GC_RE_RX_NKExCurrency_RE_ExRateType_RE_StartDate");

				Assert("Other than Clustered Index found in index operations", !otherOperationFound);

				var scanFound = planalyzer.IndexScans.Any(o => o.TableName.Contains("RefExchangeRate") && o.IndexName == "FK_RC__RE_GC_RE_RX_NKExCurrency_RE_ExRateType_RE_StartDate");
				Assert("Scan of Clustered Index found", !scanFound);
			}
		}

		public void TestPreferredPaymentCompany()
		{
			var preferredPaymentSql = @"
DECLARE @PreferredUsdCompanyPk UNIQUEIDENTIFIER = '35CB76A6-6317-4339-8EB5-50BB5127563F';
DECLARE @PreferredGbpCompanyPk UNIQUEIDENTIFIER = '5C0888A6-6B3C-4BAA-AF01-0E6D3B4BD08F';

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency) VALUES
	(@PreferredUsdCompanyPk, 'PAU', 'AU company', 'USD'),
	(@PreferredGbpCompanyPk, 'PGB', 'GB company', 'GBP')

INSERT INTO dbo.OrgMiscServ (OM_PK, OM_OH, OM_GC_CMPreferredPaymentCompany) VALUES (NEWID(), @PartyPk, @PreferredUsdCompanyPk)
DECLARE @PerPk UNIQUEIDENTIFIER = newid()
INSERT INTO dbo.GlbPerson(PER_PK, PER_FullName) values (@PerPk, 'name')

DECLARE @StaffCode VARCHAR(3) = 'ADL'
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_GC_PreferredPaymentCompany, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (NEWID(), @StaffCode, @PreferredGbpCompanyPk, @PerPk, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";

			var commissionsSql = @"
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


INSERT INTO dbo.AccCommissionLineGroup (CLG_PK, CLG_CH0, CLG_AC, CLG_RX_NKCommissionCurrency, CLG_TotalCommissionableAmount, CLG_CommissionDate, CLG_SystemCreateTimeUtc, CLG_SystemCreateUser, CLG_SystemLastEditTimeUtc, CLG_SystemLastEditUser)
	VALUES (@CommissionLineGroupPk, @CommissionHeaderPk, @ChargeCodePk, 'AUD', 100, '2002-2-2', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityPercentage, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
	VALUES (@PctCommissionLinePk, @CommissionLineGroupPk, 'CLG', @StaffAgreementRatePk, 'ADL', NULL, 'PCT',
		'GBP', 150, 3, 1, 50, 9.6, 4.8,
		'2003-3-3', NULL, '2003-3-3', '2004-4-4', NULL,
		GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')

INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityPercentage, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
	VALUES (@FixCommissionLinePk, @CommissionHeaderPk, 'CH0', @PartyAgreementRatePk, '', @PartyPk, 'FIX',
		'USD', 50, 2, 1, 25, 100, 25,
		'2004-4-4', '2005-5-5', NULL, NULL, NULL,
		GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')

DECLARE @CommissionHeader2Pk UNIQUEIDENTIFIER = '52E281F0-113D-4FDF-9350-F4E80F2476E0';
INSERT INTO dbo.AccCommissionHeader
	(
		CH0_PK, CH0_GC, CH0_AH_Source, CH0_GroupingSourceID, CH0_GroupingSourceTableCode,
		CH0_CA0, CH0_OH_Debtor, CH0_OH_Customer, CH0_Product, CH0_Service, CH0_SubModule,
		CH0_CommissionDate, CH0_SnapshotDateTime, CH0_SnapshotEventCode, CH0_OverridenDateTimeUtc,
		CH0_SystemCreateTimeUtc, CH0_SystemCreateUser, CH0_SystemLastEditTimeUtc, CH0_SystemLastEditUser
	)
	VALUES
	(
		@CommissionHeader2Pk, @RevenueAudCompanyPk, @InvoicePk, @InvoicePk, 'AH',
		@AgreementPk, @OrgPk, @OrgPk, 'ALL', 'ALL', 'ALL',
		'2002-3-2', '2002-2-2', 'POS', NULL,
		GetUtcDate(), 'XX', GetUtcDate(), 'XX'
	)

	
DECLARE @CommissionLineGroup2Pk UNIQUEIDENTIFIER = 'DB8169B1-D6F4-4B26-A4FF-C4E2F5547123';
DECLARE @PctCommissionLine2Pk UNIQUEIDENTIFIER = '62179772-679B-411D-8D5B-76356AF2B13C';

INSERT INTO dbo.AccCommissionLineGroup (CLG_PK, CLG_CH0, CLG_AC, CLG_RX_NKCommissionCurrency, CLG_TotalCommissionableAmount, CLG_CommissionDate, CLG_SystemCreateTimeUtc, CLG_SystemCreateUser, CLG_SystemLastEditTimeUtc, CLG_SystemLastEditUser)
	VALUES (@CommissionLineGroup2Pk, @CommissionHeader2Pk, @ChargeCodePk, 'AUD', 100, '2002-3-2', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_CAT, CL0_GS_NKStaff, CL0_OH_Party, CL0_CommissionType,
		CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityPercentage, CL0_EntityCommissionAmount,
		CL0_ApprovedDateTimeUtc, CL0_PaidDateTimeUtc, CL0_OverridenDateTimeUtc, CL0_CancelledDateTimeUtc, CL0_BelongsToGroup,
		CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
	VALUES (@PctCommissionLine2Pk, @CommissionLineGroup2Pk, 'CLG', @StaffAgreementRatePk, '', NULL, 'PCT',
		'AUD', 150, 3, 1, 50, 9.6, 4.8,
		'2003-3-3', NULL, '2003-3-3', '2004-4-4', NULL,
		GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')
";
			var insertSql = new StringBuilder();
			insertSql.AppendLine(AgreementCreationSql);
			insertSql.AppendLine(InvoiceCreationSql);
			insertSql.AppendLine(preferredPaymentSql);
			insertSql.AppendLine(commissionsSql);
			insertSql.AppendLine(ExchangeRateCreationSql);
			using (var command = TestConnection.Command(insertSql.ToString()))
			{
				command.ExecuteNonQuery();
			}

			var preferredUsdCompanyPk = Guid.Parse("35CB76A6-6317-4339-8EB5-50BB5127563F");
			var preferredGbpCompanyPk = Guid.Parse("5C0888A6-6B3C-4BAA-AF01-0E6D3B4BD08F");
			var revenueAudCompanyPk = Guid.Parse("56D49995-9742-491F-8C7E-B244C7E30FFD");

			var selectSql = "SELECT * FROM dbo.ViewCommissionLine ORDER BY VCL_CommissionDate, VCL_CommissionType";

			using (var command = TestConnection.Command(selectSql))
			using (var reader = command.ExecuteReader())
			{
				Assert(reader.Read());
				CombineAssertions(() =>
				{
					AssertEquals("VCL_CommissionDate", new DateTime(2002, 2, 2), reader[ViewCommissionLineSchema.Constants.VCL_CommissionDate]);
					AssertEquals("VCL_CommissionType", "FIX", reader[ViewCommissionLineSchema.Constants.VCL_CommissionType]);
					AssertEquals("VCL_GC_PreferredPaymentCompany", preferredUsdCompanyPk, reader[ViewCommissionLineSchema.Constants.VCL_GC_PreferredPaymentCompany]);
					AssertEquals("VCL_RX_NKPreferredPaymentCurrency", "USD", reader[ViewCommissionLineSchema.Constants.VCL_RX_NKPreferredPaymentCurrency]);
					AssertEquals("VCL_LocalToPreferredExchangeRate", 0.8m, reader[ViewCommissionLineSchema.Constants.VCL_LocalToPreferredExchangeRate]);
				});

				Assert(reader.Read());
				CombineAssertions(() =>
				{
					AssertEquals("VCL_CommissionDate", new DateTime(2002, 2, 2), reader[ViewCommissionLineSchema.Constants.VCL_CommissionDate]);
					AssertEquals("VCL_CommissionType", "PCT", reader[ViewCommissionLineSchema.Constants.VCL_CommissionType]);
					AssertEquals("VCL_GC_PreferredPaymentCompany", preferredGbpCompanyPk, reader[ViewCommissionLineSchema.Constants.VCL_GC_PreferredPaymentCompany]);
					AssertEquals("VCL_RX_NKPreferredPaymentCurrency", "GBP", reader[ViewCommissionLineSchema.Constants.VCL_RX_NKPreferredPaymentCurrency]);
					AssertEquals("VCL_LocalToPreferredExchangeRate", 0.5m, reader[ViewCommissionLineSchema.Constants.VCL_LocalToPreferredExchangeRate]);
				});

				Assert(reader.Read());
				CombineAssertions(() =>
				{
					AssertEquals("VCL_CommissionDate", new DateTime(2002, 3, 2), reader[ViewCommissionLineSchema.Constants.VCL_CommissionDate]);
					AssertEquals("VCL_CommissionType", "PCT", reader[ViewCommissionLineSchema.Constants.VCL_CommissionType]);
					AssertEquals("VCL_GC_PreferredPaymentCompany", revenueAudCompanyPk, reader[ViewCommissionLineSchema.Constants.VCL_GC_PreferredPaymentCompany]);
					AssertEquals("VCL_RX_NKPreferredPaymentCurrency", "AUD", reader[ViewCommissionLineSchema.Constants.VCL_RX_NKPreferredPaymentCurrency]);
					AssertEquals("VCL_LocalToPreferredExchangeRate", 1m, reader[ViewCommissionLineSchema.Constants.VCL_LocalToPreferredExchangeRate]);
				});

				Assert(!reader.Read());
			}
		}

		const string AgreementCreationSql = @"
DECLARE @RevenueAudCompanyPk UNIQUEIDENTIFIER = '56D49995-9742-491F-8C7E-B244C7E30FFD';
DECLARE @OrgPk UNIQUEIDENTIFIER = '127D0DCD-D392-4364-8642-4D64A0FAB5DF'
DECLARE @PartyPk UNIQUEIDENTIFIER = 'D42F1D28-F99A-4051-9F50-3C1A3ECF1C3C'

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency) VALUES
	(@RevenueAudCompanyPk, 'RAU', 'AU company','AUD')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk, 'TESTORG')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@PartyPk, 'TESTPARTY')

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
";
		const string InvoiceCreationSql = @"
DECLARE @BranchPk UNIQUEIDENTIFIER = '9C60AE3F-44F9-4E97-B404-040FA896626E';
DECLARE @DepartmentPk UNIQUEIDENTIFIER = 'DD3E23A1-7C44-4DAE-8F38-ACE05FDBA455';

INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @RevenueAudCompanyPk)
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)

DECLARE @ChargeCodePk UNIQUEIDENTIFIER = '42D6B105-3659-401A-A648-C3EEA6364186';
INSERT INTO dbo.AccChargeCode (AC_PK, AC_ChargeGroup) VALUES (@ChargeCodePk, 'FRT')

DECLARE @InvoicePk UNIQUEIDENTIFIER = '28083B6E-95DC-4925-9B82-370A7D7C939D';
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_Ledger, AH_TransactionType) VALUES (@InvoicePk, @RevenueAudCompanyPk, @BranchPk, @DepartmentPk, '2002-2-2', 'AR', 'JNL')
";
		const string ExchangeRateCreationSql = @"
INSERT INTO dbo.RefExchangeRate (RE_PK, RE_GC, RE_RX_NKExCurrency, RE_ExRateType, RE_StartDate, RE_ExpiryDate, RE_SellRate, RE_AsPublished) VALUES
	(newid(), @RevenueAudCompanyPk, 'USD', 'SEL', '2002-2-1', '2002-3-1', 0.8, ''),
	(newid(), @RevenueAudCompanyPk, 'USD', 'SEL', '2002-3-1', '2002-4-1', 0.9, ''),
	(newid(), @RevenueAudCompanyPk, 'GBP', 'SEL', '2002-2-1', '2002-3-1', 0.5, ''),
	(newid(), @RevenueAudCompanyPk, 'GBP', 'SEL', '2002-3-1', '2002-4-1', 0.6, '')
";
	}
}

