using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Marketing;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Marketing
{
	[TestedType(typeof(SalesValueAnalysisManager))]
	sealed class SalesValueAnalysisManagerTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2014, 9);

		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByOccured(transactions, new DateTime(2014, 9, 12, 2, 32, 0));
			AssertEquals("[T1] UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "TSTOPP001", transaction1.Reference1);
			AssertEquals("[T1] AdditionalRefs",
				"{\"ProductName\":\"Forwarding\"," +
				"\"TypeModeCount\":\"2\"," +
				"\"DetailsCount\":5," +
				"\"VerticalMarketCount\":2," +
				"\"ActivityPeriodCount\":3," +
				"\"CompetitorCount\":3," +
				"\"AgentCount\":3," +
				"\"CarrierCount\":1}",
				transaction1.AdditionalRefs);

			var transaction2 = FindRowByOccured(transactions, new DateTime(2014, 9, 30, 4, 54, 0));
			AssertEquals("[T2] UserCode", "US4", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "TSTOPP001", transaction2.Reference1);
			AssertEquals("[T2] AdditionalRefs",
				"{\"ProductName\":\"Warehouse\"," +
				"\"TypeModeCount\":\"N\\/A\"," +
				"\"DetailsCount\":3," +
				"\"VerticalMarketCount\":3," +
				"\"ActivityPeriodCount\":1," +
				"\"CompetitorCount\":2," +
				"\"AgentCount\":1," +
				"\"CarrierCount\":3}",
				transaction2.AdditionalRefs);
		}

		protected override void PrepareTestData()
		{
			const string sqlText = @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @OhPk UNIQUEIDENTIFIER = (SELECT TOP (1) OH_PK FROM dbo.OrgHeader);
				DECLARE @OppPk UNIQUEIDENTIFIER = NEWID();
				DECLARE @SalesPk1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @SalesPk2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @SalesPk3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @SalesPk4 UNIQUEIDENTIFIER = NEWID();
				DECLARE @DetailPk1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @DetailPk2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @DetailPk3 UNIQUEIDENTIFIER = NEWID();

				INSERT INTO dbo.OrgSales (OW_PK, OW_MP_Product, OW_IsTraded, OW_SystemCreateTimeUtc, OW_SystemCreateUser, OW_SystemLastEditTimeUtc, OW_SystemLastEditUser) VALUES
				(@SalesPk1, '4B815685-01C6-4CCF-90A5-565BBA0162AC', 0, '2014-08-31 01:21:00', 'US1', GetUtcDate(), 'E'),
				(@SalesPk2, '24FAB43B-7A5B-4B3E-A2FE-A6B5CFD2503F', 0, '2014-09-12 02:32:00', 'US2', GetUtcDate(), 'E'),
				(@SalesPk3, 'B416C223-820D-4313-B9AB-6616D343255F', 1, '2014-09-20 03:43:00', 'US3', GetUtcDate(), 'E'),
				(@SalesPk4, '471C3735-3EE9-4DD2-BCA6-772A9553EDD3', 0, '2014-09-30 04:54:00', 'US4', GetUtcDate(), 'E'),
				(NEWID()  , '24FAB43B-7A5B-4B3E-A2FE-A6B5CFD2503F', 0, '2014-09-30 05:05:00', 'US5', GetUtcDate(), 'E')

				INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser) VALUES
				(@OppPk, 'TSTOPP001', @OhPk, @GcPk, GetUtcDate(), 'E', GetUtcDate(), 'E')

				INSERT INTO dbo.OrgSalesValueAssociationPivot (SVP_PK, SVP_TradeId, SVP_TradeTableCode, SVP_ActivityId, SVP_ActivityTableCode, SVP_SystemCreateTimeUtc, SVP_SystemCreateUser, SVP_SystemLastEditTimeUtc, SVP_SystemLastEditUser) VALUES
				(NEWID(), @SalesPk1, 'OW', @OppPk, 'P8', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @SalesPk2, 'OW', @OppPk, 'P8', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @SalesPk3, 'OW', @OppPk, 'P8', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @SalesPk4, 'OW', @OppPk, 'P8', GetUtcDate(), 'E', GetUtcDate(), 'E')

				INSERT INTO dbo.OrgTradeDetail (PA_PK, PA_OW, PA_TradeMode, PA_TradeType, PA_SystemCreateTimeUtc, PA_SystemCreateUser, PA_SystemLastEditTimeUtc, PA_SystemLastEditUser) VALUES
				(@DetailPk1, @SalesPk1, 'AIR', 'LSE', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(@DetailPk2, @SalesPk2, 'SEA', 'LCL', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(@DetailPk3, @SalesPk4, ''   ,  ''  , GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID()   , @SalesPk2, 'SEA', 'FCL', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID()   , @SalesPk2, 'SEA', 'FCL', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID()   , @SalesPk3, ''   , ''   , GetUtcDate(), 'E', GetUtcDate(), 'E')

				INSERT INTO dbo.OrgTradeProspect (PAP_PK, PAP_PA, PAP_IndustryVertical, PAP_PeriodOfActivity, PAP_OH_Competitor, PAP_OH_ControllingAgent, PAP_OH_ServiceProvider, PAP_SystemCreateTimeUtc, PAP_SystemCreateUser, PAP_SystemLastEditTimeUtc, PAP_SystemLastEditUser) VALUES
				(NEWID(), @DetailPk1, 'XXX', '111', @OhPk, null , null , GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @DetailPk2, ''   , '222', null , @OhPk, @OhPk, GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @DetailPk2, 'YYY', ''   , @OhPk, null , null , GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @DetailPk2, ''   , '111', null , @OhPk, null , GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @DetailPk2, 'XXX', ''   , @OhPk, null , null , GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @DetailPk2, ''   , '222', @OhPk, @OhPk, null , GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @DetailPk3, 'XXX', '111', @OhPk, null , @OhPk, GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @DetailPk3, 'YYY', ''   , @OhPk, null , @OhPk, GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(NEWID(), @DetailPk3, 'ZZZ', ''   , null , @OhPk, @OhPk, GetUtcDate(), 'E', GetUtcDate(), 'E')";
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
