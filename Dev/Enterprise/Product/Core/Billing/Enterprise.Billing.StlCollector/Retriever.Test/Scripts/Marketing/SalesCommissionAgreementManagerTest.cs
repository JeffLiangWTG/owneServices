using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Marketing;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Marketing
{
	[TestedType(typeof(SalesCommissionAgreementManager))]
	sealed class SalesCommissionAgreementManagerTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2014, 9);

		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "#2");
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 9, 12, 2, 32, 0), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "TSTOPP001", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference03", "ItemCount=1|ModeCount=2", transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", "CATrigger=CCD|WolfPackCount=2|RatesCount=0", transaction1.Reference4);

			var transaction2 = FindRowByRef2(transactions, "#3");
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 9, 30, 3, 43, 0), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US3", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "TSTOPP001", transaction2.Reference1);
			AssertEquals("[T2] TransactionReference03", "ItemCount=2|ModeCount=1", transaction2.Reference3);
			AssertEquals("[T2] TransactionReference04", "CATrigger=MAN|WolfPackCount=1|RatesCount=2", transaction2.Reference4);
		}

		protected override void PrepareTestData()
		{
			const string sqlText = @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @OhPk UNIQUEIDENTIFIER = (SELECT TOP (1) OH_PK FROM dbo.OrgHeader);
				DECLARE @OppPk UNIQUEIDENTIFIER = NEWID();
				DECLARE @CaPk1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @CaPk2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @CaPk3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @ItemPk1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @ItemPk2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @ItemPk3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @RecipientPk1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @RecipientPk2 UNIQUEIDENTIFIER = NEWID();

				INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser) VALUES
				(@OppPk, 'TSTOPP001', @OhPk, @GcPk, GetUtcDate(), 'E', GetUtcDate(), 'E')
				INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_P8, CA0_Name, CA0_OH_Customer, CA0_CommissionBasis, CA0_CommissionTriggerType, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser) VALUES
				(@CaPk1, @OppPk, '#1', @OhPk, 'PRF', 'MAN', '2014-08-31 01:21:00', 'US1', '2014-08-31 03:45:00', 'US3'),
				(@CaPk2, @OppPk, '#2', @OhPk, 'PRF', 'CCD', '2014-09-12 02:32:00', 'US2', '2014-09-12 03:44:00', 'US3'),
				(@CaPk3, @OppPk, '#3', @OhPk, 'REV', 'MAN', '2014-09-30 03:43:00', 'US3', '2014-09-30 03:43:00', 'US3')
				INSERT INTO dbo.OrgCommissionAgreementItem (CAI_PK, CAI_ParentID, CAI_ParentTableCode, CAI_Type, CAI_Code, CAI_SystemCreateTimeUtc, CAI_SystemCreateUser, CAI_SystemLastEditTimeUtc, CAI_SystemLastEditUser) VALUES
				(@ItemPk1, @CaPk1, 'CA0', 'PRD', 'CTO', '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1'),
				(@ItemPk2, @CaPk2, 'CA0', 'PRD', 'ALL', '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1'),
				(@ItemPk3, @CaPk3, 'CA0', 'PRD', 'CSH', '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1'),
				(NEWID(), @ItemPk2, 'CAI', 'SRV', 'ALL', '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1'),
				(NEWID(), @CaPk3, 'CA0', 'PRD', 'FCN', '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1')
				INSERT INTO dbo.OrgCommissionAgreementItemCondition (CIC_PK, CIC_CAI, CIC_Mode, CIC_SystemCreateTimeUtc, CIC_SystemCreateUser, CIC_SystemLastEditTimeUtc, CIC_SystemLastEditUser) VALUES
				(NEWID(), @ItemPk1, 'AIR', '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1'),
				(NEWID(), @ItemPk2, 'SEA', '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1'),
				(NEWID(), @ItemPk2, 'CON', '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1'),
				(NEWID(), @ItemPk3, 'SHP', '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1')
				INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_GS_NKStaff, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser) VALUES
				(@RecipientPk1, @CaPk1, 'US1', '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1'),
				(@RecipientPk2, @CaPk3, 'US2', '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1'),
				(NEWID(), @CaPk2, 'US3', '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1'),
				(NEWID(), @CaPk2, 'US4', '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1')
				INSERT INTO dbo.OrgCommissionAgreementRecipientRate (CAT_PK, CAT_CAR, CAT_SystemCreateTimeUtc, CAT_SystemCreateUser, CAT_SystemLastEditTimeUtc, CAT_SystemLastEditUser) VALUES
				(NEWID(), @RecipientPk1, '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1'),
				(NEWID(), @RecipientPk2, '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1'),
				(NEWID(), @RecipientPk2, '2014-10-12 02:00:00', 'US1', '2014-10-12 02:00:00', 'US1')";
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
