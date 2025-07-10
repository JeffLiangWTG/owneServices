using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.OceanCarrier;

[TestedType(typeof(PopulateCVO_VoyageId))]
sealed class PopulateCVO_VoyageIdTest : NumberFountainDataTransformationTestCase<PopulateCVO_VoyageId>
{
	public void TestTransformationShouldRun_WhenColumnPresentButEmpty()
	{
		TestRunAndAssertResultsTwice<object>(() =>
			{
				PrepareTestData();
				TestConnection.ExecuteNonQuery(
					@"ALTER TABLE dbo.CarrierVoyage ADD CVO_VoyageId varchar(20) NOT NULL CONSTRAINT [DF_CarrierVoyage_CVO_VoyageId] DEFAULT ('');"); // this is the bit that happens before pre-upgrade transformations.

				return null;
			},
			_ => AssertPreConditions(),
			_ => AssertTransformationResults());
	}

	public void TestTransformationShouldRun_WhenTableIsEmpty()
	{
		TestRunAndAssertResultsTwice<object>(() =>
			{
				LoadCurrentSequenceValue("CarrierVoyageID");

				DropColumnCVO_VoyageIdIfExists();
				DeleteFountainProcedures();

				return null;
			},
			_ => AssertPreConditions(),
			_ => AssertTransformationResults());
	}

	protected override void PrepareTestData()
	{
		LoadCurrentSequenceValue("CarrierVoyageID");

		DropColumnCVO_VoyageIdIfExists();
		DeleteFountainProcedures();

		// -------------------------------------
		// Create Carrier
		// -------------------------------------
		var carrierValues = new List<(string column, object value)>
		{
			("OH_PK", carrierHeader),
			("OH_Code", "Header"),
			("OH_SystemCreateTimeUtc", DateTime.UtcNow),
			("OH_SystemLastEditTimeUtc", DateTime.UtcNow),
			("OH_SystemCreateUser", "XXX"),
			("OH_SystemLastEditUser", "XXX")
		};

		DatabaseHelper.InsertIntoTable(TestConnection, "OrgHeader", carrierValues);

		// -------------------------------------
		// Create Service
		// -------------------------------------
		var serviceValues = new List<(string column, object value)>
		{
			("CSV_PK", carrierService),
			("CSV_Code", "SRV"),
			("CSV_Name", "Service"),
			("CSV_OH_Carrier", carrierHeader),
			("CSV_SystemCreateTimeUtc", DateTime.UtcNow),
			("CSV_SystemLastEditTimeUtc", DateTime.UtcNow),
			("CSV_SystemCreateUser", "XXX"),
			("CSV_SystemLastEditUser", "XXX")
		};

		DatabaseHelper.InsertIntoTable(TestConnection, "CarrierService", serviceValues);

		for (int i = 0; i < 2; i++)
		{
			// -------------------------------------
			// Create Voyage - Insert two Voyages.
			// -------------------------------------
			var voyageValues = new List<(string column, object value)>
			{
				("CVO_PK", Guid.NewGuid().ToString()),
				("CVO_VoyageNumber", "123"),
				("CVO_CSV_Service", carrierService),
				("CVO_IsPublished", 1),
				("CVO_SystemCreateTimeUtc", DateTime.UtcNow),
				("CVO_SystemLastEditTimeUtc", DateTime.UtcNow),
				("CVO_SystemCreateUser", "XXX"),
				("CVO_SystemLastEditUser", "XXX")
			};

			DatabaseHelper.InsertIntoTable(TestConnection, "CarrierVoyage", voyageValues);
		}
	}

	readonly string carrierHeader = Guid.NewGuid().ToString();
	readonly string carrierService = Guid.NewGuid().ToString();

	protected override void AssertTransformationResults()
	{
		var actualVoyageIds = new List<string>();
		TestConnection.ExecuteReader("SELECT CVO_VoyageId FROM dbo.CarrierVoyage ORDER BY CVO_VoyageId ASC", record => actualVoyageIds.Add(record["CVO_VoyageId"] as string));

		var expectedVoyageIds = Enumerable.Range(CurrentSequenceValue + 1, actualVoyageIds.Count).Select(index => $"VOY{index:00000000000000000}").ToList();

		AssertSequencesEqual(expectedVoyageIds, actualVoyageIds);
	}

	void DropColumnCVO_VoyageIdIfExists()
	{
		if (!DbObjectCreator.ColumnExists(TestConnection, "CarrierVoyage", "CVO_VoyageId"))
		{
			return;
		}

		TestConnection.ExecuteNonQuery(@"
ALTER TABLE dbo.CarrierVoyage DROP CONSTRAINT Constraint_CVO_VoyageId
ALTER TABLE dbo.CarrierVoyage DROP CONSTRAINT DF_CarrierVoyage_CVO_VoyageId
ALTER TABLE dbo.CarrierVoyage DROP COLUMN CVO_VoyageId");
	}
}
