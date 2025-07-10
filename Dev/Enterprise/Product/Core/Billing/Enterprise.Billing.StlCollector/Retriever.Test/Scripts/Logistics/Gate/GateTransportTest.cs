using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(GateTransport))]
	sealed class GateTransportTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @GTTPk1 UNIQUEIDENTIFIER = newid();
				DECLARE @GTTPk2 UNIQUEIDENTIFIER = newid();
				DECLARE @GTTPk3 UNIQUEIDENTIFIER = newid();
				DECLARE @GTTPk4 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk);

				INSERT dbo.GateTransport ([GTT_PK],[GTT_FacilityType],[GTT_JobNumber],[GTT_TimeIn],[GTT_TimeOut],[GTT_VehicleRegistration],[GTT_DriverName],[GTT_DriverLicence],
									[GTT_IsCancelled],[GTT_SystemCreateTimeUtc],[GTT_SystemCreateUser],[GTT_SystemLastEditTimeUtc],[GTT_SystemLastEditUser],[GTT_GB_Branch]) VALUES
					(@GTTPk1, 'AAA', 'Job1', '2021-04-01', '2021-04-02', 'A2-755345', 'Mark Williams', '334355454', 0, '2021-04-04', 'BP~', '2021-04-04', 'BP~', @GbPk),
					(@GTTPk3, 'AAA', 'Job2', '2021-04-02', '2021-04-03', 'A2-755345', 'Mark Williams', '334355454', 0, '2021-04-04', 'BP~', '2021-04-04', 'BP~', @GbPk),
					(@GTTPk2, 'AAA', 'Job3', '2021-03-31', '2021-04-02', 'A2-755345', 'Mark Williams', '334355454', 1, '2021-04-04', 'BP~', '2021-04-04', 'BP~', @GbPk),
					(@GTTPk4, 'AAA', 'Job4', '2021-03-31', '2021-03-31', 'A2-755345', 'Mark Williams', '334355454', 0, '2021-03-31', 'BP~', '2021-04-04', 'BP~', @GbPk)";

			TestConnection.Command(sqlText).ExecuteNonQuery();
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 3, transactions.Count());
				AssertRowMatchingRef1(transactions, "1", "DEM", "DEM", new DateTime(2021, 4, 4), string.Empty, 1, "Job1", "A2-755345");
				AssertRowMatchingRef1(transactions, "2", "DEM", "DEM", new DateTime(2021, 4, 4), string.Empty, 1, "Job2", "A2-755345");
				AssertRowMatchingRef1(transactions, "3", "DEM", "DEM", new DateTime(2021, 4, 4), string.Empty, 1, "Job3", "A2-755345");
			});
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2021, 4);
	}
}
