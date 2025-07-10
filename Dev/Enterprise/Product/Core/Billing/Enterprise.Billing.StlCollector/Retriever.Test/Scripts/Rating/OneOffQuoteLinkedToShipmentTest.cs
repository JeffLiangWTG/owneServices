using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Rating;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Rating
{
	[TestedType(typeof(OneOffQuoteLinkedToShipment))]
	sealed class OneOffQuoteLinkedToShipmentTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			/* in this test, we have 3 OOQ (QBNE00001001 to QBNE00001003) and 3 Booking (QBNE00001004 to QBNE00001006) and 1 BWQ (QBNE00001007)
			* we link OOQ QBNE00001001 to Consign1 and link OOQ QBNE00001002 to Consign2 and link BWQ QBNE00001004 to QBNE00001007 and
			* should bring only QBNE00001001 and QBNE00001002
			*/

			string sqlText = @"
			DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
			DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
			DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);

			INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(NEWID(), 'SY1',  @GcPk),
					(NEWID(), 'TK1',  @GcPk);

			DECLARE @ThPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @ThPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @ThPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @ThPk04 UNIQUEIDENTIFIER = newid();
			DECLARE @ThPk05 UNIQUEIDENTIFIER = newid();
			DECLARE @ThPk06 UNIQUEIDENTIFIER = newid();
			DECLARE @ThPk07 UNIQUEIDENTIFIER = newid();

			DECLARE @JsPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @JsPk04 UNIQUEIDENTIFIER = newid();

			INSERT dbo.RatingHeader (TH_PK, TH_GC, TH_RateType, TH_QuoteNumber, TH_OneTimeQuote, TH_SystemCreateUser, TH_SystemCreateTimeUtc, TH_QuoteDate, TH_IsCancelled, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser,TH_IsOneOffQuoteConsumed) VALUES
				(@ThPk01, @GcPk, 'QTE', 'QBNE00001001', 0, 'US1', '2022-10-01 01:00:00','2022-11-01', 0, '2022-11-01 01:00:00', '~BP',1),
				(@ThPk02, @GcPk, 'QTE', 'QBNE00001002', 0, 'US2','2022-10-01 02:00:00','2022-11-01', 0, '2022-11-01 01:05:00', '~BP',1),
				(@ThPk03, @GcPk, 'QTE', 'QBNE00001003', 0, 'US3', GetUtcDate(),'2022-11-01', 0, '2022-11-01 01:10:00', '~BP',1),
				(@ThPk04, @GcPk, 'QTE', 'QBNE00001004', 0, 'US4', GetUtcDate(),'2022-11-01', 0, '2022-11-01 01:15:00', '~BP',1),
				(@ThPk05, @GcPk, 'QTE', 'QBNE00001005', 0, 'US5', GetUtcDate(),'2022-11-01', 0, '2022-10-01 01:15:00', '~BP',1),
				(@ThPk06, @GcPk, 'QTE', 'QBNE00001006', 0, 'US6', GetUtcDate(),'2022-11-01', 1, '2022-11-01 01:20:00', '~BP',1),
				(@ThPk07, @GcPk, 'QTE', 'QBNE00001007', 0, 'US7', GetUtcDate(),'2022-11-01', 0, '2022-11-01 01:25:00', '~BP',1),
				(newid(), @GcPk, 'QTE', 'QBNE00001008', 1, 'US8', GetUtcDate(),'2022-11-01', 0, '2022-11-01 01:25:00', '~BP',1);

			INSERT dbo.JobShipment (JS_PK, JS_ShipmentType, JS_RL_NKOrigin, JS_UniqueConsignRef, JS_IsBooking, JS_TH_OneTimeQuote) VALUES
				(@JsPk01, 'HVL', 'AUSYD', 'Consign1', 0, null),
				(@JsPk02, 'STD', 'AUSYD', 'Consign2', 0, null),
				(@JsPk03, 'STD', 'AUSYD', 'Consign3', 0, null),
				(@JsPk04, 'STD', 'AUSYD', 'Consign4', 0, @ThPk07);

			INSERT dbo.JobHeader (JH_PK, JH_GC, JH_ParentID, JH_GB, JH_GE, JH_Status, JH_SystemCreateUser, JH_JobNum, JH_TH_NKQuoteNumber, JH_ParentTableCode) VALUES
				(newid(), @GcPk, @JsPk01, @GbPk, @GePk, 'WRK', 'US1', 'JHS01', 'QBNE00001001', 'JS'),
				(newid(), @GcPk, @JsPk02, @GbPk, @GePk, 'WRK', 'US2', 'JHS02', 'QBNE00001002', 'JS'),
				(newid(), @GcPk, @ThPk07, @GbPk, @GePk, 'WRK', 'US3', 'JHS03', 'QBNE00001004', 'JS');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());
			var transaction1 = FindRowByRef1(transactions, "Header: QBNE00001001");
			AssertEquals(1, transaction1.BillableCount);
			var transaction2 = FindRowByRef1(transactions, "Header: QBNE00001002");
			AssertEquals(1, transaction2.BillableCount);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 11);

		public void TestIsSystemLevelFeature()
		{
			AssertEquals("IsSystemLevel", false, ScriptToTest.IsSystemLevel);
		}
	}
}
