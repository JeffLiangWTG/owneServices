using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(ComplianceRiskStatusOverrideCollector))]
	class ComplianceRiskStatusOverrideCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 11);

		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 8, transactions.Count());

			var transaction1 = FindRowByOccuredDay(2024, 11, 5);
			AssertEquals(2, transaction1.Count());
			var transaction11 = FindRowsByRef1(transaction1, "SHP01").FirstOrDefault();
			var transaction12 = FindRowsByRef1(transaction1, "SHP03").FirstOrDefault();

			var transaction2 = FindRowByOccuredDay(2024, 11, 6);
			AssertEquals(1, transaction2.Count());
			var transaction21 = transaction2.First();

			var transaction3 = FindRowByOccuredDay(2024, 11, 7);
			AssertEquals(1, transaction3.Count());
			var transaction31 = transaction3.First();

			var transaction4 = FindRowByOccuredDay(2024, 11, 9);
			AssertEquals(1, transaction4.Count());
			var transaction41 = transaction4.First();

			var transaction5 = FindRowByOccuredDay(2024, 11, 11);
			AssertEquals(3, transaction5.Count());
			var transactions5Array = transaction5.OrderBy(u => u.ServiceOccuredUTC).ToArray();
			var transaction51 = transactions5Array[0];
			var transaction52 = transactions5Array[1];
			var transaction53 = transactions5Array[2];

			AssertRow(transaction11, "1", "DEM", "DEM", new DateTime(2024,11, 5), "US2", 1, "SHP01", "AUSYD", "USCHI", "2");
			AssertRow(transaction12, "2", "DEM", "DEM", new DateTime(2024, 11, 5), "US3", 1, "SHP03", "INBOM", "UAIEV", "1");
			AssertRow(transaction21, "3", "DEM", "DEM", new DateTime(2024, 11, 6), "US2", 1, "TH01", "AUMEL", "MYPKG", "1");
			AssertRow(transaction31, "4", "DEM", "DEM", new DateTime(2024, 11, 7), "US1", 1, "SHP03", "INBOM", "UAIEV", "2");
			AssertRow(transaction41, "5", "DEM", "DEM", new DateTime(2024, 11, 9), "US1", 1, "SHP03", "INBOM", "UAIEV", "3");
			AssertRow(transaction51, "6", "DEM", "DEM", new DateTime(2024, 11, 11, 5, 0, 0), "US1", 1, "CON01", "AUSYD", "USCHI", "1");
			AssertRow(transaction52, "7", "DEM", "DEM", new DateTime(2024, 11, 11, 7, 0, 0), "US1", 1, "CON01", "AUSYD", "USCHI", "2");
			AssertRow(transaction53, "8", "DEM", "DEM", new DateTime(2024, 11, 11, 9, 0, 0), "US1", 1, "CON01", "AUSYD", "USCHI", "3");

			IEnumerable<IStlTransaction> FindRowByOccuredDay(int year, int month, int day) => transactions.Where(t => t.ServiceOccuredUTC.Year == year && t.ServiceOccuredUTC.Month == month && t.ServiceOccuredUTC.Day == day);
		}

		protected override void PrepareTestData()
		{
			var sqlQuery = @"
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @PerPk UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @RatingHeader1 UNIQUEIDENTIFIER = newid();
				DECLARE @Shipment1 UNIQUEIDENTIFIER = newid();
				DECLARE @Shipment2 UNIQUEIDENTIFIER = newid();
				DECLARE @Shipment3 UNIQUEIDENTIFIER = newid();
				DECLARE @Consol1 UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbPerson (PER_PK, PER_FullName) values (@PerPk, 'Dummy Name');

				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_GB_HomeBranch, GS_GB_LastLogonBranch, GS_IsActive, GS_IsDevice, GS_IsResource, GS_IsSystemAccount, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES
				(@GsPk01, 'US1', 'Staff001', 'staff.001', @GbPk, null, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				(@GsPk02, 'US2', 'Staff002', 'staff.002', null, @GbPk, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				(@GsPk03, 'US3', 'Staff004', 'staff.004', @GbPk, null, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT dbo.RatingHeader (TH_PK, TH_QuoteNumber, TH_RateType, TH_SystemCreateTimeUtc, TH_SystemCreateUser, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_QuoteDate) VALUES
				(@RatingHeader1, 'TH01', 'QTE', '2024-10-01 10:01:01', '~BP', GetUtcDate(), '~BP', GetDate())

				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser, JS_TH_OneTimeQuote) VALUES
				(@Shipment1, 'SHP01', 'SEA', 'AUSYD', 'USCHI', '2024-09-01 11:22:33', '~BP', GetUtcDate(), '~BP', NULL),
				(@Shipment2, 'SHP02', 'SEA', 'AUMEL', 'MYPKG', '2024-09-01 11:22:33', '~BP', GetUtcDate(), '~BP', @RatingHeader1),
				(@Shipment3, 'SHP03', 'SEA', 'INBOM', 'UAIEV', '2024-09-01 11:22:33', '~BP', GetUtcDate(), '~BP', NULL)

				INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_RL_NKLoadPort, JK_RL_NKDischargePort, JK_SystemCreateTimeUtc, JK_SystemCreateUser, JK_SystemLastEditTimeUtc, JK_SystemLastEditUser)
				VALUES
				(@Consol1, 'CON01', 'AUSYD', 'USCHI', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.[StmComplianceEvent] ([SCE_PK], [SCE_ParentTableCode], [SCE_ParentID], [SCE_EventType], [SCE_EventSubType], [SCE_EventReference], [SCE_NewValue], [SCE_OldValue], [SCE_EventTimeOffset], [SCE_SystemCreateUser], [SCE_SystemCreateTimeUtc], [SCE_SystemLastEditUser], [SCE_SystemLastEditTimeUtc])
				VALUES
				(newid(), 'JS', @Shipment1, 'STU', 'OVL', 'Job Compliance status', 'OVR', 'HLD', '2024-10-04 00:00:00', 'US1', GetUtcDate(), 'US1', GetUtcDate()),
				(newid(), 'JS', @Shipment1, 'STU', 'OVL', 'Job Compliance status', 'OVR', 'BLK', '2024-11-05 00:00:00', 'US2', GetUtcDate(), 'US2', GetUtcDate()),
				(newid(), 'JS', @Shipment3, 'STU', 'OVL', 'Job Compliance status', 'OVR', 'BLK', '2024-11-05 00:00:00', 'US3', GetUtcDate(), 'US3', GetUtcDate()),
				(newid(), 'TH', @RatingHeader1, 'STU', 'OVL', 'Job Compliance status', 'OVR', 'BLK', '2024-11-06 00:00:00', 'US2', GetUtcDate(), 'US2', GetUtcDate()),
				(newid(), 'JS', @Shipment3, 'STU', 'OVL', 'Job Compliance status', 'OVR', 'BLK', '2024-11-07 00:00:00', 'US1', GetUtcDate(), 'US1', GetUtcDate()),
				(newid(), 'JS', @Shipment3, 'STU', 'OVL', 'Job Compliance status', 'OVR', 'HLD', '2024-11-09 00:00:00', 'US1', GetUtcDate(), 'US1', GetUtcDate()),
				(newid(), 'JK', @Consol1, 'STU', 'OVL', 'Job Compliance status', 'OVR', 'HLD', '2024-11-11 05:00:00', 'US1', GetUtcDate(), 'US1', GetUtcDate()),
				(newid(), 'JK', @Consol1, 'STU', 'OVL', 'Job Compliance status', 'OVR', 'HLD', '2024-11-11 07:00:00', 'US1', GetUtcDate(), 'US1', GetUtcDate()),
				(newid(), 'JK', @Consol1, 'STU', 'OVL', 'Job Compliance status', 'OVR', 'HLD', '2024-11-11 09:00:00', 'US1', GetUtcDate(), 'US1', GetUtcDate())
				";

			TestConnection.ExecuteNonQuery(sqlQuery);
		}
	}
}
