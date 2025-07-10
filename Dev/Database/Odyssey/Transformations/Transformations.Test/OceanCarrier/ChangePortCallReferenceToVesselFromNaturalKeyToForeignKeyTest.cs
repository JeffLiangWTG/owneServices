using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.OceanCarrier;

[TestedType(typeof(ChangePortCallReferenceToVesselFromNaturalKeyToForeignKey))]
sealed class ChangePortCallReferenceToVesselFromNaturalKeyToForeignKeyTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new ChangePortCallReferenceToVesselFromNaturalKeyToForeignKey();

	protected override void PrepareTestData()
	{
		EnsureColumnDoesNotExist();
		EnsureVesselWithRefVesselNameDoesNotExist();

		var org = testDataCreator.CreateOrg(code: "OH_CPO");
		orgAddress = testDataCreator.CreateOrgAddress(org, "Address 1", "OA_CPO", "Company");
		refVesselPkOne = testDataCreator.CreateRefVessel(RefVesselNameOne, "Number 1");
		refVesselPkTwo = testDataCreator.CreateRefVessel(RefVesselNameTwo, "Number 2");

		var sql = $"""
			INSERT INTO dbo.CarrierVoyagePortCall (CPO_PK, CPO_PortCallId, CPO_VesselName, CPO_OA_Port, CPO_RL_NKDisplayAsPort, CPO_SystemCreateTimeUtc, CPO_SystemCreateUser, CPO_SystemLastEditTimeUtc, CPO_SystemLastEditUser)
			VALUES ('{portCallPkOne}', 'P1', '{RefVesselNameOne}', '{orgAddress:D}', 'DEHAM', GETUTCDATE(), '---', GETUTCDATE(), '---')

			INSERT INTO dbo.CarrierVoyagePortCall (CPO_PK, CPO_PortCallId, CPO_VesselName, CPO_OA_Port, CPO_RL_NKDisplayAsPort, CPO_SystemCreateTimeUtc, CPO_SystemCreateUser, CPO_SystemLastEditTimeUtc, CPO_SystemLastEditUser)
			VALUES ('{portCallPkTwo}', 'P2', '{RefVesselNameTwo}', '{orgAddress:D}', 'DEHAM', GETUTCDATE(), '---', GETUTCDATE(), '---')
			""";
		Db.Connection.ExecuteNonQuery(sql);
	}

	protected override void AssertTransformationResults()
	{
		AssertVesselFkAndVesselName(portCallPkOne, refVesselPkOne, RefVesselNameOne, "Keep vessel name");
		AssertVesselFkAndVesselName(portCallPkTwo, refVesselPkTwo, RefVesselNameTwo, "Keep vessel name");
	}

	public void TestShouldCreateAndAssignPlaceholderVesselWhenVesselNameIsNotFoundInRefVessel()
	{
		TestRunAndAssertResultsTwice<object>(
			() =>
			{
				PrepareTestData();

				var sql = $"""
					INSERT INTO dbo.CarrierVoyagePortCall (CPO_PK, CPO_PortCallId, CPO_VesselName, CPO_OA_Port, CPO_RL_NKDisplayAsPort, CPO_SystemCreateTimeUtc, CPO_SystemCreateUser, CPO_SystemLastEditTimeUtc, CPO_SystemLastEditUser)
					VALUES ('{portCallPkThree}', 'P3', 'Unknown Vessel With a Long Name', '{orgAddress:D}', 'DEHAM', GETUTCDATE(), '---', GETUTCDATE(), '---')
					""";
				Db.Connection.ExecuteNonQuery(sql);
				return null;
			},
			_ => AssertPreConditions(),
			_ =>
			{
				bool? isActive = null;

				TestConnection.ExecuteReader(
					$"""
					 SELECT RV_PK, RV_IsActive
					 FROM dbo.RefVessel
					 WHERE RV_Code = '{PlaceholderVesselName}'
					 """,
					record =>
					{
						placeholderVesselPk = record[RefVesselSchema.Constants.PK] as Guid?;
						isActive = record[RefVesselSchema.Constants.RV_IsActive] as bool?;
					});
				AssertNotNull(placeholderVesselPk);
				AssertEquals(false, isActive);

				AssertVesselFkAndVesselName(portCallPkThree, placeholderVesselPk!.Value, "Unknown Vessel With a Long Name", "Prepend 'Placeholder', Vessel Name must be shortened to column length");
			});
	}

	public void TestShouldAssignExistingPlaceholderVesselWhenVesselNameIsNotFoundInRefVessel()
	{
		TestRunAndAssertResultsTwice<object>(
			() =>
			{
				PrepareTestData();

				placeholderVesselPk = testDataCreator.CreateRefVessel(PlaceholderVesselName, "Number 3");

				var sql = $"""
					INSERT INTO dbo.CarrierVoyagePortCall (CPO_PK, CPO_PortCallId, CPO_VesselName, CPO_OA_Port, CPO_RL_NKDisplayAsPort, CPO_SystemCreateTimeUtc, CPO_SystemCreateUser, CPO_SystemLastEditTimeUtc, CPO_SystemLastEditUser)
					VALUES ('{portCallPkThree}', 'P3', 'Unknown Vessel', '{orgAddress:D}', 'DEHAM', GETUTCDATE(), '---', GETUTCDATE(), '---')
					""";
				Db.Connection.ExecuteNonQuery(sql);
				return null;
			},
			_ => AssertPreConditions(),
			_ =>
			{
				AssertNotNull(placeholderVesselPk);
				AssertVesselFkAndVesselName(portCallPkThree, placeholderVesselPk!.Value, "Unknown Vessel", "Prepend 'Placeholder'");
			});
	}

	public void TestShouldCreatePlaceholderVesselEvenWhenAllVesselNamesAreFoundInRefVessel()
	{
		TestRunAndAssertResultsTwice<object>(
			() =>
			{
				PrepareTestData();
				return null;
			},
			_ => AssertPreConditions(),
			_ =>
			{
				var countPlaceholderVessel = TestConnection.ExecuteScalar(
					$"""
					 SELECT count(1)
					 FROM dbo.RefVessel
					 WHERE RV_Code = '{PlaceholderVesselName}'
					 """);
				AssertEquals(1, countPlaceholderVessel);
			});
	}

	public void TestShouldNotOverwriteExistingVesselReferenceWhenVesselNameCouldNotBeFoundInRefVessel()
	{
		TestRunAndAssertResultsTwice<object>(
			() =>
			{
				PrepareTestData();

				DbObjectCreator.CreateColumn(TestConnection, CarrierVoyagePortCallSchema.Constants.TableName, CarrierVoyagePortCallSchema.Constants.CPO_RV_Vessel, CarrierVoyagePortCallSchema.CPO_RV_Vessel.TypeInfo);

				var sql = $"""
					INSERT INTO dbo.CarrierVoyagePortCall (CPO_PK, CPO_PortCallId, CPO_RV_Vessel, CPO_VesselName, CPO_OA_Port, CPO_RL_NKDisplayAsPort, CPO_SystemCreateTimeUtc, CPO_SystemCreateUser, CPO_SystemLastEditTimeUtc, CPO_SystemLastEditUser)
					VALUES ('{portCallPkThree}', 'P3', '{refVesselPkTwo}', 'Renamed Vessel', '{orgAddress:D}', 'DEHAM', GETUTCDATE(), '---', GETUTCDATE(), '---')
					""";
				Db.Connection.ExecuteNonQuery(sql);
				return null;
			},
			_ => AssertPreConditions(),
			_ =>
			{
				AssertVesselFkAndVesselName(portCallPkThree, refVesselPkTwo, "Renamed Vessel", "Keep vessel name");
			});
	}

	public void TestShouldNotOverwriteExistingVesselReferenceWhenColumnExistsAndIsNotNullable()
	{
		TestRunAndAssertResultsTwice<object>(
			() =>
			{
				PrepareTestData();

				DbObjectCreator.CreateColumn(TestConnection, CarrierVoyagePortCallSchema.Constants.TableName, CarrierVoyagePortCallSchema.Constants.CPO_RV_Vessel, CarrierVoyagePortCallSchema.CPO_RV_Vessel.TypeInfo);

				var sql = $"""
					UPDATE dbo.CarrierVoyagePortCall
					SET
						CPO_RV_Vessel = '{refVesselPkTwo}',
						CPO_VesselName = '{RefVesselNameTwo}',
						CPO_SystemLastEditTimeUtc = GETUTCDATE(),
						CPO_SystemLastEditUser = '~BP'
					WHERE CPO_RV_Vessel IS NULL

					ALTER TABLE dbo.CarrierVoyagePortCall ALTER COLUMN CPO_RV_Vessel UNIQUEIDENTIFIER NOT NULL
					""";
				Db.Connection.ExecuteNonQuery(sql);
				return null;
			},
			_ => AssertPreConditions(),
			_ =>
			{
				AssertVesselFkAndVesselName(portCallPkOne, refVesselPkTwo, RefVesselNameTwo, "Keep vessel name");
				AssertVesselFkAndVesselName(portCallPkTwo, refVesselPkTwo, RefVesselNameTwo, "Keep vessel name");
			});
	}

	public void TestShouldNotExecuteTransformationWhenTableDoesNotExist()
	{
		TestRunAndAssertResultsTwice<object>(
			() =>
			{
				DbObjectCreator.RenameTable(TestConnection, Db.DatabaseName, "dbo", "CarrierVoyagePortCall", "CarrierVoyagePortCall_X");
				return null;
			},
			_ => { },
			_ => { });
	}

	void EnsureColumnDoesNotExist()
	{
		if (!DbObjectCreator.ColumnExists(TestConnection, CarrierVoyagePortCallSchema.Constants.TableName, CarrierVoyagePortCallSchema.Constants.CPO_RV_Vessel))
		{
			return;
		}

		new DbColumnDependencyRemover(CarrierVoyagePortCallSchema.Constants.TableName, CarrierVoyagePortCallSchema.Constants.CPO_RV_Vessel).DropRelateObjects(TestConnection);
		TestConnection.ExecuteNonQuery("ALTER TABLE dbo.CarrierVoyagePortCall DROP COLUMN CPO_RV_Vessel");
	}

	void EnsureVesselWithRefVesselNameDoesNotExist()
	{
		const string sql = $"""
			UPDATE
				dbo.RefVessel
			SET
				RV_Code = '1d5e8150802e4c169018ceeddfeb2cad',
				RV_SystemLastEditTimeUtc = GETUTCDATE(),
				RV_SystemLastEditUser = '~BP'
			WHERE RV_Code = '{PlaceholderVesselName}'
			""";
		TestConnection.ExecuteNonQuery(sql);
	}

	void AssertVesselFkAndVesselName(Guid portCallPk, Guid expectedVesselFk, string expectedVesselName, string vesselNameMessage)
	{
		Guid? vesselFk = null;
		string vesselName = null;

		TestConnection.ExecuteReader(
			$"""
			 SELECT CPO_RV_Vessel, CPO_VesselName
			 FROM dbo.CarrierVoyagePortCall
			 WHERE CPO_PK = '{portCallPk}'
			 """, record =>
			{
				vesselFk = record[CarrierVoyagePortCallSchema.Constants.CPO_RV_Vessel] as Guid?;
				vesselName = record[CarrierVoyagePortCallSchema.Constants.CPO_VesselName] as string;
			});
		AssertEquals(expectedVesselFk, vesselFk);
		AssertEquals(vesselNameMessage, expectedVesselName, vesselName);
	}

	readonly TransformationTestDataCreator testDataCreator = new TransformationTestDataCreator();

	Guid orgAddress;
	Guid refVesselPkOne;
	Guid refVesselPkTwo;
	readonly Guid portCallPkOne = Guid.NewGuid();
	readonly Guid portCallPkTwo = Guid.NewGuid();
	readonly Guid portCallPkThree = Guid.NewGuid();
	const string RefVesselNameOne = "My First Vessel";
	const string RefVesselNameTwo = "My Second Vessel";

	Guid? placeholderVesselPk;
	const string PlaceholderVesselName = "Placeholder (OCS - CPO_RV_Vessel)";
}
