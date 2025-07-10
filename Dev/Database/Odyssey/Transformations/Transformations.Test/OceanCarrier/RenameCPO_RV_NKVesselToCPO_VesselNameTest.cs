using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.OceanCarrier;

[TestedType(typeof(RenameCPO_RV_NKVesselToCPO_VesselName))]
sealed class RenameCPO_RV_NKVesselToCPO_VesselNameTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new RenameCPO_RV_NKVesselToCPO_VesselName();

	protected override void PrepareTestData()
	{
		EnsureOldColumnExists();
		AssertColumnNamesAreOld();

		var testDataCreator = new TransformationTestDataCreator();
		var org = testDataCreator.CreateOrg(code: "OH_CPO");
		var orgAddress = testDataCreator.CreateOrgAddress(org, "Address 1", "OA_CPO", companyOverride: "CPO_VesselName");
		var refVessel = testDataCreator.CreateRefVessel(RefVesselName, "Number");

		var sql = $"""
			INSERT INTO dbo.CarrierVoyagePortCall (CPO_PK, CPO_PortCallId, CPO_RV_Vessel, CPO_RV_NKVessel, CPO_OA_Port, CPO_RL_NKDisplayAsPort, CPO_SystemCreateTimeUtc, CPO_SystemCreateUser, CPO_SystemLastEditTimeUtc, CPO_SystemLastEditUser)
			VALUES ('{portCallPk}', 'P1', '{refVessel}', '{RefVesselName}', '{orgAddress:D}', 'DEHAM', GETUTCDATE(), '---', GETUTCDATE(), '---')
			""";
		Db.Connection.ExecuteNonQuery(sql);
	}

	protected override void AssertTransformationResults()
	{
		AssertColumnNamesAreNew();
		var vesselName = "";

		TestConnection.ExecuteReader($"select top 1 {NewColumnName} from dbo.CarrierVoyagePortCall where CPO_PK = '{portCallPk}'",
			record =>
			{
				vesselName = record[NewColumnName] as string;
			});

		AssertEquals(vesselName, RefVesselName);
	}

	void EnsureOldColumnExists()
	{
		if (DbObjectCreator.ColumnExists(TestConnection, CarrierVoyagePortCallSchema.Constants.TableName, NewColumnName))
		{
			new DbColumnDependencyRemover(CarrierVoyagePortCallSchema.Constants.TableName, NewColumnName).DropRelateObjects(TestConnection);
			DbObjectCreator.RenameColumn(TestConnection, Db.SqlDbOwnerSchema, CarrierVoyagePortCallSchema.Constants.TableName, NewColumnName, OldColumnName);
		}
		else if (!DbObjectCreator.ColumnExists(TestConnection, CarrierVoyagePortCallSchema.Constants.TableName, OldColumnName))
		{
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, CarrierVoyagePortCallSchema.Constants.TableName, OldColumnName, CarrierVoyagePortCallSchema.CPO_VesselName.TypeInfo);
		}
	}

	void AssertColumnNamesAreNew()
	{
		AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, CarrierVoyagePortCallSchema.Constants.TableName, NewColumnName));
		AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, CarrierVoyagePortCallSchema.Constants.TableName, OldColumnName));
	}

	void AssertColumnNamesAreOld()
	{
		AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, CarrierVoyagePortCallSchema.Constants.TableName, OldColumnName));
		AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, CarrierVoyagePortCallSchema.Constants.TableName, NewColumnName));
	}

	readonly Guid portCallPk = Guid.NewGuid();
	const string RefVesselName = "My Vessel";
	const string OldColumnName = "CPO_RV_NKVessel";
	const string NewColumnName = CarrierVoyagePortCallSchema.Constants.CPO_VesselName;
}
