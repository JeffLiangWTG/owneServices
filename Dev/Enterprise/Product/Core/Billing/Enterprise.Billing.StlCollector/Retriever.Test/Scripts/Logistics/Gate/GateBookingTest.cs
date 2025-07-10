using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(GateBooking))]
	sealed class GateBookingTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @GTBPk1 UNIQUEIDENTIFIER = newid();
				DECLARE @GTBPk2 UNIQUEIDENTIFIER = newid();
				DECLARE @GTBPk3 UNIQUEIDENTIFIER = newid();
				DECLARE @GTBPk4 UNIQUEIDENTIFIER = newid();
				DECLARE @LOGPK UNIQUEIDENTIFIER = newid();

				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk);

				INSERT INTO [dbo].[GateBooking] ([GTB_PK], [GTB_VehicleRegistration],[GTB_DriverName],[GTB_SlotStartTime],
												[GTB_BookingNumber],[GTB_SystemCreateTimeUtc],[GTB_SystemCreateUser],[GTB_SystemLastEditTimeUtc],[GTB_SystemLastEditUser], [GTB_JobReference], [GTB_GB_Branch]) VALUES
					(@GTBPk1, 'A2-755345', 'Mark Williams', '2021-04-01', 'Booking1', '2021-04-02', 'BP~', '2021-04-04', 'BP~', 'Job1', @GbPk),
					(@GTBPk3, 'A2-755345', 'Mark Williams', '2021-04-02', 'Booking2', '2021-04-03', 'BP~', '2021-04-04', 'BP~', 'Job2', @GbPk),
					(@GTBPk2, 'A2-755345', 'Mark Williams', '2021-03-31', 'Booking3', '2021-03-31', 'BP~', '2021-04-04', 'BP~', 'Job3', @GbPk),
					(@GTBPk4, 'A2-755345', 'Mark Williams', '2021-04-01', 'Booking4', '2021-04-01', 'BP~', '2021-04-04', 'BP~', 'Job4', @GbPk);
				INSERT INTO [dbo].[StmALog] ([SL_PK],[SL_Table],[SL_Parent],[SL_IsEstimate],[SL_IsCancelled],[SL_Reference],[SL_PostedTimeUtc],[SL_EventTime],[SL_GS_NKUser],[SL_SE_NKEvent],[SL_FireWorkflow]) VALUES
					(@LOGPK, 'GateBooking', @GTBPK4, 'N', 'N', '', '2021-04-01', '2021-04-01', 'BP~', 'CNC', 1);";

			TestConnection.Command(sqlText).ExecuteNonQuery();
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 3, transactions.Count());
				AssertRowMatchingRef1(transactions, "1", "DEM", "DEM", new DateTime(2021, 4, 1), string.Empty, 1, "Booking4", "Job4");
				AssertRowMatchingRef1(transactions, "2", "DEM", "DEM", new DateTime(2021, 4, 2), string.Empty, 1, "Booking1", "Job1");
				AssertRowMatchingRef1(transactions, "3", "DEM", "DEM", new DateTime(2021, 4, 3), string.Empty, 1, "Booking2", "Job2");
			});
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2021, 4);
	}
}
