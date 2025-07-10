using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(ComplianceRiskUsageCollector))]
	class ComplianceRiskUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 3);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 5, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "SHP01");
			var transaction2 = FindRowByRef1(transactions, "TH01");
			var transaction3 = FindRowByRef1(transactions, "TH02");
			var transaction4 = FindRowByRef1(transactions, "SHP03");
			var transaction5 = FindRowByRef1(transactions, "SHP04");

			AssertRow(transaction1, "1", "DEM", "DEM", new DateTime(2023, 3, 1), "US1", 1, "SHP01", null, "2022-01-02", "JS");
			AssertRow(transaction2, "2", "DEM", "DEM", new DateTime(2023, 3, 8), "US2", 1, "TH01", "SHP02", "2022-01-03", "TH");
			AssertRow(transaction3, "3", "DEM", "DEM", new DateTime(2023, 3, 15), "US3", 1, "TH02", null, "2022-01-04", "TH");
			AssertRow(transaction4, "4", null, null, new DateTime(2023, 3, 22), "US4", 1, "SHP03", null, null, "JS");
			AssertRow(transaction5, "5", null, null, new DateTime(2023, 3, 31), "US5", 1, "SHP04", null, null, "JS");
		}

		readonly string shipment1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string shipment2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string shipment3 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string shipment4 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string shipment5 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string ratingHeader1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string ratingHeader2 = Guid.NewGuid().ToString().ToUpperInvariant();

		protected override void PrepareTestData()
		{
			var sqlQuery = $@"
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @PerPk UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk04 UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbPerson (PER_PK, PER_FullName) values (@PerPk, 'Dummy Name')

				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_GB_HomeBranch, GS_GB_LastLogonBranch, GS_IsActive, GS_IsDevice, GS_IsResource, GS_IsSystemAccount, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES
				(@GsPk01, 'US1', 'Staff001', 'staff.001', @GbPk, null, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				(@GsPk02, 'US2', 'Staff002', 'staff.002', null, @GbPk, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				(@GsPk03, 'US3', 'Staff003', 'staff.003', @GbPk, @GbPk, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
				(@GsPk04, 'US4', 'Staff004', 'staff.004', null, null, 1, 0, 0, 0, @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT dbo.RatingHeader (TH_PK, TH_QuoteNumber, TH_RateType, TH_SystemCreateTimeUtc, TH_SystemCreateUser, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_QuoteDate) VALUES
				('{ratingHeader1}', 'TH01', 'QTE', '2022-01-03 10:01:01', '~BP', GetUtcDate(), '~BP', GetDate()),
				('{ratingHeader2}', 'TH02', 'QTE', '2022-01-04 10:01:01', '~BP', GetUtcDate(), '~BP', GetDate())

				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_TransportMode, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser, JS_TH_OneTimeQuote) VALUES
				('{shipment1}', 'SHP01', 'SEA', '2022-01-02 11:22:33', '~BP', GetUtcDate(), '~BP', NULL),
				('{shipment2}', 'SHP02', 'SEA', '2022-03-04 11:33:55', '~BP', GetUtcDate(), '~BP', '{ratingHeader1}'),
				('{shipment3}', 'SHP03', 'SEA', NULL, '~BP', GetUtcDate(), '~BP', NULL),
				('{shipment4}', 'SHP04', 'SEA', NULL, '~BP', GetUtcDate(), '~BP', NULL),
				('{shipment5}', 'SHP05', 'SEA', NULL, '~BP', GetUtcDate(), '~BP', NULL)

				INSERT INTO dbo.[StmComplianceEvent] ([SCE_PK], [SCE_ParentTableCode], [SCE_ParentID], [SCE_EventType], [SCE_EventSubType], [SCE_EventReference], [SCE_EventTimeOffset], [SCE_SystemCreateUser], [SCE_SystemCreateTimeUtc], [SCE_SystemLastEditUser], [SCE_SystemLastEditTimeUtc])
				VALUES
				(newid(), 'JS', '{shipment1}', 'CRI', 'CAI', 'XXX', '2023-03-01 00:00:00', 'US1', GetUtcDate(), 'US1', GetUtcDate()),
				(newid(), 'TH', '{ratingHeader1}', 'CRI', 'CAI', 'XXX', '2023-03-08 00:00:00', 'US2', GetUtcDate(), 'US2', GetUtcDate()),
				(newid(), 'TH', '{ratingHeader2}', 'CRI', 'CAI', 'XXX', '2023-03-15 00:00:00', 'US3', GetUtcDate(), 'US3', GetUtcDate()),
				(newid(), 'JS', '{shipment3}', 'CRI', 'CAI', 'XXX', '2023-03-22 00:00:00', 'US4', GetUtcDate(), 'US4', GetUtcDate()),
				(newid(), 'JS', '{shipment4}', 'CRI', 'CAI', 'XXX', '2023-03-31 00:00:00', 'US5', GetUtcDate(), 'US5', GetUtcDate()),
				(newid(), 'JS', NEWID(), 'CRI', 'CAI', 'XXX', '2023-03-01 00:00:00', 'US1', GetUtcDate(), 'US1', GetUtcDate()),
				(newid(), 'JS', '{shipment5}', 'CRI', 'CAI', 'XXX', '2023-02-28 00:00:00', 'US1', GetUtcDate(), 'US1', GetUtcDate()),
				(newid(), 'JS', '{shipment5}', 'CRI', 'CAD', 'XXX', '2023-03-14 00:00:00', 'US1', GetUtcDate(), 'US1', GetUtcDate()),
				(newid(), 'JS', '{shipment5}', 'CRI', 'CAI', 'XXX', '2023-04-01 00:00:00', 'US1', GetUtcDate(), 'US1', GetUtcDate())
";

			TestConnection.ExecuteNonQuery(sqlQuery);
		}
	}
}
