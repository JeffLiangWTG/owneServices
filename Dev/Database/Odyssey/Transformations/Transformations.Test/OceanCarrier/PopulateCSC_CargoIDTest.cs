using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.OceanCarrier;

[TestedType(typeof(PopulateCSC_CargoID))]
sealed class PopulateCSC_CargoIDTest : NumberFountainDataTransformationTestCase<PopulateCSC_CargoID>
{
	public void TestTransformationShouldRun_WhenColumnPresentButEmpty()
	{
		TestRunAndAssertResultsTwice<object>(() =>
			{
				PrepareTestData();
				TestConnection.ExecuteNonQuery(
					"ALTER TABLE dbo.CarrierShipmentCargo ADD CSC_CargoID varchar(20) NOT NULL CONSTRAINT [DF_CarrierShipmentCargo_CSC_CargoID] DEFAULT ('');"); // this is the bit that happens before pre-upgrade transformations.

				return null;
			},
			_ => AssertPreConditions(),
			_ => AssertTransformationResults());
	}

	public void TestTransformationShouldRun_WhenTableIsEmpty()
	{
		TestRunAndAssertResultsTwice<object>(() =>
			{
				LoadCurrentSequenceValue("CarrierShipmentCargoID-f7394bf7-2295-4ac7-9aa6-61ad7c07d6e2");

				DropColumnCSC_CargoIDIfExists();
				DeleteFountainProcedures();

				return null;
			},
			_ => AssertPreConditions(),
			_ => AssertTransformationResults());
	}

	protected override void PrepareTestData()
	{
		LoadCurrentSequenceValue("CarrierShipmentCargoID-f7394bf7-2295-4ac7-9aa6-61ad7c07d6e2");

		DropColumnCSC_CargoIDIfExists();
		DeleteFountainProcedures();

		// -------------------------------------
		// Create Shipment Header entry.
		// -------------------------------------
		var carrierHeaderValues = new List<(string column, object value)>
		{
			("CSH_PK", carrierShipment),
			("CSH_CarrierShipmentReference", carrierShipment.Substring(0, 20)),
			("CSH_SystemCreateTimeUtc", DateTime.UtcNow),
			("CSH_SystemCreateUser", "XXX"),
			("CSH_SystemLastEditTimeUtc", DateTime.UtcNow),
			("CSH_SystemLastEditUser", "XXX"),
		};

		DatabaseHelper.InsertIntoTable(TestConnection, "CarrierShipmentHeader", carrierHeaderValues);

		// -------------------------------------
		// Create Shipment Cargo entries.
		// -------------------------------------
		foreach (var testCase in testCases)
		{
			var carrierCargoValues = new List<(string column, object value)>
			{
				("CSC_PK", testCase.PK),
				("CSC_CSH_CarrierShipment", carrierShipment),
				("CSC_CargoType", testCase.CargoType),
				("CSC_ReceiptDrayage", testCase.CargoType != "CNT" ? "" : "ANY"),
				("CSC_DeliveryDrayage", testCase.CargoType != "CNT" ? "" : "ANY"),
				("CSC_VGMWeighingMethod", testCase.VgmWeighingMethod),
				("CSC_VGMWeight", testCase.VgmWeight),
				("CSC_VGMWeightUnit", testCase.VgmWeightUnit),
				("CSC_VGMWeighingDateTime", testCase.VgmWeighingDateTime),
				("CSC_IsTopLevel", true),
				("CSC_CargoMovementTypeOrigin", "FCL"),
				("CSC_CargoMovementTypeDestination", "FCL"),
				("CSC_SystemCreateTimeUtc", DateTime.UtcNow),
				("CSC_SystemCreateUser", "XXX"),
				("CSC_SystemLastEditTimeUtc", DateTime.UtcNow),
				("CSC_SystemLastEditUser", "XXX"),
			};

			DatabaseHelper.InsertIntoTable(TestConnection, "CarrierShipmentCargo", carrierCargoValues);
		}
	}
	class CarrierShipmentCargoTestCase
	{
		internal CarrierShipmentCargoTestCase(string cargoType, string vgmWeighingMethod = null, decimal? vgmWeight = null, string vgmWeightUnit = null, DateTimeOffset? vgmWeighingDateTime = null)
		{
			PK = Guid.NewGuid().ToString();
			CargoType = cargoType;
			VgmWeighingMethod = vgmWeighingMethod;
			VgmWeight = vgmWeight;
			VgmWeightUnit = vgmWeightUnit;

			if (vgmWeighingDateTime != null)
			{
				VgmWeighingDateTime = new DateTimeOffset(vgmWeighingDateTime.Value.Date.ToSmallDateTimeFloor(), vgmWeighingDateTime.Value.Offset);
			}
		}

		public string PK { get; }
		public string CargoType { get; }
		public string VgmWeighingMethod { get; }
		public decimal? VgmWeight { get; }
		public string VgmWeightUnit { get; }
		public DateTimeOffset? VgmWeighingDateTime { get; }
	}

	readonly string carrierShipment = Guid.NewGuid().ToString();

	readonly List<CarrierShipmentCargoTestCase> testCases =
	[
		new CarrierShipmentCargoTestCase("CNT", "RTL", 27, "KG", DateTimeOffset.Now),
		new CarrierShipmentCargoTestCase("ROR"),
		new CarrierShipmentCargoTestCase("BBK"),
	];

	protected override void AssertTransformationResults()
	{
		var actualCargoIds = new List<string>();
		TestConnection.ExecuteReader("SELECT CSC_CargoID FROM dbo.CarrierShipmentCargo ORDER BY CSC_CargoID ASC", record => actualCargoIds.Add(record["CSC_CargoID"] as string));

		var expectedCargoIds = Enumerable.Range(CurrentSequenceValue + 1, actualCargoIds.Count).Select(index => $"CRG{index:00000000000000000}").ToList();
		AssertSequencesEqual(expectedCargoIds, actualCargoIds);
	}

	void DropColumnCSC_CargoIDIfExists()
	{
		if (!DbObjectCreator.ColumnExists(TestConnection, "CarrierShipmentCargo", "CSC_CargoID"))
		{
			return;
		}

		TestConnection.ExecuteNonQuery(@"
ALTER TABLE dbo.CarrierShipmentCargo DROP CONSTRAINT Constraint_CSC_CargoID
ALTER TABLE dbo.CarrierShipmentCargo DROP CONSTRAINT DF_CarrierShipmentCargo_CSC_CargoID
DROP INDEX NR_UX__CSC_CargoID ON dbo.CarrierShipmentCargo
ALTER TABLE dbo.CarrierShipmentCargo DROP COLUMN CSC_CargoID");
	}
}
