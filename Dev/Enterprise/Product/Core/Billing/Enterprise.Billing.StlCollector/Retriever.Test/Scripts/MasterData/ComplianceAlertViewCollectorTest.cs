using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(ComplianceAlertViewCollector))]
	class ComplianceAlertViewCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 11);

		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByOccuredDay(2024, 11, 4);
			AssertEquals(2, transaction1.BillableCount);
			AssertEquals("{\"ViewedInformation\":[{\"HSCode\":\"520620\",\"Count\":1,\"JobType\":\"SHP\",\"OriginOfGoods\":\"CN\"},{\"HSCode\":\"520620\",\"Count\":1,\"JobType\":\"SHP\",\"OriginOfGoods\":\"US\"}]}", transaction1.AdditionalRefs);
			var transaction2 = FindRowByOccuredDay(2024, 11, 6);
			AssertEquals(2, transaction2.BillableCount);
			AssertEquals("{\"ViewedInformation\":[{\"HSCode\":\"520620\",\"Count\":2,\"JobType\":\"SHP\",\"OriginOfGoods\":\"\"}]}", transaction2.AdditionalRefs);
			var transaction3 = FindRowByOccuredDay(2024, 11, 12);
			AssertEquals(5, transaction3.BillableCount);
			AssertEquals("{\"ViewedInformation\":[{\"HSCode\":\"520620\",\"Count\":1,\"JobType\":\"SHP\",\"OriginOfGoods\":\"UK\"},{\"HSCode\":\"520620\",\"Count\":2,\"JobType\":\"TH\",\"OriginOfGoods\":\"US\"},{\"HSCode\":\"520621\",\"Count\":1,\"JobType\":\"SHP\",\"OriginOfGoods\":\"CN\"},{\"HSCode\":\"520621\",\"Count\":1,\"JobType\":\"TH\",\"OriginOfGoods\":\"\"}]}", transaction3.AdditionalRefs);

			IStlTransaction FindRowByOccuredDay(int year, int month, int day) => transactions.Single(t => t.ServiceOccuredUTC.Year == year && t.ServiceOccuredUTC.Month == month && t.ServiceOccuredUTC.Day == day);
		}

		protected override void PrepareTestData()
		{
			var sqlQuery = $@"
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @PerPk UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @RatingHeader1 UNIQUEIDENTIFIER = newid();
				DECLARE @RatingHeader2 UNIQUEIDENTIFIER = newid();
				DECLARE @Shipment1 UNIQUEIDENTIFIER = newid();
				DECLARE @Shipment2 UNIQUEIDENTIFIER = newid();
				DECLARE @Shipment3 UNIQUEIDENTIFIER = newid();
				DECLARE @Shipment4 UNIQUEIDENTIFIER = newid();
				DECLARE @Shipment5 UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbPerson (PER_PK, PER_FullName) values (@PerPk, 'Dummy Name');

				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_GB_HomeBranch, GS_GB_LastLogonBranch, GS_IsActive, GS_IsDevice, GS_IsResource, GS_IsSystemAccount, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES
				(@GsPk01, 'US1', 'Staff001', 'staff.001', @GbPk, null, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				(@GsPk02, 'US2', 'Staff002', 'staff.002', null, @GbPk, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				(@GsPk03, 'US3', 'Staff004', 'staff.004', null, null, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT dbo.RatingHeader (TH_PK, TH_QuoteNumber, TH_RateType, TH_SystemCreateTimeUtc, TH_SystemCreateUser, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_QuoteDate) VALUES
				(@RatingHeader1, 'TH01', 'QTE', '2024-10-01 10:01:01', '~BP', GetUtcDate(), '~BP', GetDate()),
				(@RatingHeader2, 'TH02', 'QTE', '2024-10-02 10:01:01', '~BP', GetUtcDate(), '~BP', GetDate())

				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_TransportMode, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser, JS_TH_OneTimeQuote) VALUES
				(@Shipment1, 'SHP01', 'SEA', '2024-09-01 11:22:33', '~BP', GetUtcDate(), '~BP', NULL),
				(@Shipment2, 'SHP02', 'SEA', '2024-09-02 11:33:55', '~BP', GetUtcDate(), '~BP', @RatingHeader1),
				(@Shipment3, 'SHP03', 'SEA', '2024-09-03 00:00:00', '~BP', GetUtcDate(), '~BP', NULL),
				(@Shipment4, 'SHP04', 'SEA', '2024-09-04 00:00:00', '~BP', GetUtcDate(), '~BP', NULL),
				(@Shipment5, 'SHP05', 'SEA', NULL, '~BP', GetUtcDate(), '~BP', NULL)

				INSERT INTO dbo.[StmComplianceEvent] ([SCE_PK], [SCE_ParentTableCode], [SCE_ParentID], [SCE_EventType], [SCE_EventSubType], [SCE_EventReference], [SCE_EventTimeOffset], [SCE_SystemCreateUser], [SCE_SystemCreateTimeUtc], [SCE_SystemLastEditUser], [SCE_SystemLastEditTimeUtc])
				VALUES
				(newid(), 'TH', @RatingHeader1, 'CRI', 'CAI', 'XXX', '2024-11-11', 'US1', GetUtcDate(), 'US1', GetUtcDate()),
				(newid(), 'TH', @RatingHeader1, 'BWI', 'LBV', '520621', '2024-11-13', 'US1', GetUtcDate(), 'US1', GetUtcDate()),
				(newid(), 'TH', @RatingHeader2, 'BWI', 'LBV', '520620|ORG=US', '2024-11-13', 'US2', GetUtcDate(), 'US2', GetUtcDate()),
				(newid(), 'TH', @RatingHeader2, 'BWI', 'LBV', '520620|ORG=US', '2024-11-13', 'US2', GetUtcDate(), 'US2', GetUtcDate()),
				(newid(), 'JS', @Shipment1, 'CRI', 'CAD', 'XXX', '2024-11-04 00:00:00', 'US1', GetUtcDate(), 'US1', GetUtcDate()),
				(newid(), 'JS', @Shipment1, 'BWI', 'LBV', '520620|ORG=CN', '2024-11-05 00:00:00', 'US1', GetUtcDate(), 'US1', GetUtcDate()),
				(newid(), 'JS', @Shipment3, 'BWI', 'LBV', '520620|ORG=US', '2024-11-05 00:00:00', 'US2', GetUtcDate(), 'US2', GetUtcDate()),
				(newid(), 'JS', @Shipment4, 'BWI', 'LBV', '520620', '2024-11-07 00:00:00', 'US3', GetUtcDate(), 'US3', GetUtcDate()),
				(newid(), 'JS', @Shipment5, 'BWI', 'LBV', '520620', '2024-11-07 00:00:00', 'US2', GetUtcDate(), 'US2', GetUtcDate()),
				(newid(), 'JS', @Shipment5, 'BWI', 'LBV', '520620|ORG=UK', '2024-11-13 00:00:00', 'US3', GetUtcDate(), 'US3', GetUtcDate()),
				(newid(), 'JS', @Shipment5, 'BWI', 'LBV', '520621|ORG=CN', '2024-11-13 00:00:00', 'US3', GetUtcDate(), 'US3', GetUtcDate()),
				(newid(), 'JS', newid(), 'BWI', 'LBV', '520620|ORG=US', '2023-11-09 00:00:00', 'US1', GetUtcDate(), 'US1', GetUtcDate())
			";

			TestConnection.ExecuteNonQuery(sqlQuery);
		}
	}
}
