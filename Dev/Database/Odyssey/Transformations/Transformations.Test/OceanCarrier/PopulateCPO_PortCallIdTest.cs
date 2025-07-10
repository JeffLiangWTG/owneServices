using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.OceanCarrier;

[TestedType(typeof(PopulateCPO_PortCallId))]
sealed class PopulateCPO_PortCallIdTest : NumberFountainDataTransformationTestCase<PopulateCPO_PortCallId>
{
	public void TestTransformationShouldRun_WhenColumnPresentButEmpty()
	{
		TestRunAndAssertResultsTwice<object>(() =>
			{
				PrepareTestData();
				TestConnection.ExecuteNonQuery(
					@"ALTER TABLE dbo.CarrierVoyagePortCall ADD CPO_PortCallId varchar(20) NOT NULL CONSTRAINT [DF_CarrierVoyagePortCall_CPO_PortCallId] DEFAULT ('');"); // this is the bit that happens before pre-upgrade transformations.

				return null;
			},
			_ => AssertPreConditions(),
			_ => AssertTransformationResults());
	}

	public void TestTransformationShouldRun_WhenTableIsEmpty()
	{
		TestRunAndAssertResultsTwice<object>(() =>
			{
				LoadCurrentSequenceValue("CarrierVoyagePortCallID");

				DropColumnCPO_PortCallIdIfExists();
				DeleteFountainProcedures();

				return null;
			},
			_ => AssertPreConditions(),
			_ => AssertTransformationResults());
	}

	protected override void PrepareTestData()
	{
		LoadCurrentSequenceValue("CarrierVoyagePortCallID");

		DropColumnCPO_PortCallIdIfExists();
		DeleteFountainProcedures();

		var creator = new TransformationTestDataCreator();
		var orgHeader = creator.CreateOrgHeader("PORTCALLID", "Port Call Id");
		var orgAddress = creator.CreateOrgAddress(orgHeader, "Address 1", "PORTCALLID", "Port Call Id");
		var refVessel = creator.CreateRefVessel("PORTCALLID", "Number");

		for (int i = 0; i < 2; i++)
		{
			var portValues = new List<(string column, object value)>
			{
				("CPO_PK", Guid.NewGuid().ToString()),
				("CPO_RV_Vessel", refVessel),
				("CPO_VesselName", "PORTCALLID"),
				("CPO_OA_Port", orgAddress),
				("CPO_RL_NKDisplayAsPort", "DEHAM"),
				("CPO_SystemCreateTimeUtc", DateTime.UtcNow),
				("CPO_SystemCreateUser", "XXX"),
				("CPO_SystemLastEditTimeUtc", DateTime.UtcNow),
				("CPO_SystemLastEditUser", "XXX")
			};

			DatabaseHelper.InsertIntoTable(TestConnection, "CarrierVoyagePortCall", portValues);
		}
	}

	protected override void AssertTransformationResults()
	{
		var actualPortCallIds = new List<string>();
		TestConnection.ExecuteReader("SELECT CPO_PortCallId FROM dbo.CarrierVoyagePortCall ORDER BY CPO_PortCallId ASC", record => actualPortCallIds.Add(record["CPO_PortCallId"] as string));

		var expectedPortCallIds = Enumerable.Range(CurrentSequenceValue + 1, actualPortCallIds.Count).Select(index => $"PRT{index:00000000000000000}").ToList();

		AssertSequencesEqual(expectedPortCallIds, actualPortCallIds);
	}

	void DropColumnCPO_PortCallIdIfExists()
	{
		if (!DbObjectCreator.ColumnExists(TestConnection, "CarrierVoyagePortCall", "CPO_PortCallId"))
		{
			return;
		}

		TestConnection.ExecuteNonQuery(@"
ALTER TABLE dbo.CarrierVoyagePortCall DROP CONSTRAINT Constraint_CPO_PortCallId
ALTER TABLE dbo.CarrierVoyagePortCall DROP CONSTRAINT DF_CarrierVoyagePortCall_CPO_PortCallId
ALTER TABLE dbo.CarrierVoyagePortCall DROP COLUMN CPO_PortCallId");
	}
}
