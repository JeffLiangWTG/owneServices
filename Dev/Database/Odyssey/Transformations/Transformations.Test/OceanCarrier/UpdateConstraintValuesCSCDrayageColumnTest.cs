using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.OceanCarrier;

[TestedType(typeof(UpdateConstraintValuesCSCDrayageColumn))]
sealed class UpdateConstraintValuesCSCDrayageColumnTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new UpdateConstraintValuesCSCDrayageColumn();

	protected override void PrepareTestData()
	{
		if (DbObjectCreator.GetColumnTypeAndMaxLength(Db.Connection, CarrierShipmentCargoSchema.Constants.TableName, CarrierShipmentCargoSchema.Constants.CSC_DeliveryDrayage).length == 3 &&
		DbObjectCreator.GetColumnTypeAndMaxLength(Db.Connection, CarrierShipmentCargoSchema.Constants.TableName, CarrierShipmentCargoSchema.Constants.CSC_ReceiptDrayage).length == 3)
		{
			const string sql = """
				ALTER TABLE dbo.CarrierShipmentCargo DROP CONSTRAINT Constraint_CSC_ReceiptDrayage
				ALTER TABLE dbo.CarrierShipmentCargo DROP CONSTRAINT Constraint_CSC_DeliveryDrayage
				ALTER TABLE dbo.CarrierShipmentCargo DROP CONSTRAINT DF_CarrierShipmentCargo_CSC_ReceiptDrayage
				ALTER TABLE dbo.CarrierShipmentCargo DROP CONSTRAINT DF_CarrierShipmentCargo_CSC_DeliveryDrayage
				
				UPDATE
					dbo.CarrierShipmentCargo
				SET
					CSC_ReceiptDrayage = LEFT(CSC_ReceiptDrayage + '  ', 2),
					CSC_DeliveryDrayage = LEFT(CSC_DeliveryDrayage + '  ', 2),
					CSC_SystemLastEditTimeUtc = GETUTCDATE(),
					CSC_SystemLastEditUser = '~BP'
				
				ALTER TABLE dbo.CarrierShipmentCargo ALTER COLUMN CSC_ReceiptDrayage CHAR(2) NOT NULL
				ALTER TABLE dbo.CarrierShipmentCargo ALTER COLUMN CSC_DeliveryDrayage CHAR(2) NOT NULL
				""";
			Db.Connection.ExecuteNonQuery(sql);
		}

		var carrierShipmentGuid = Guid.NewGuid().ToString();
		DatabaseHelper.InsertIntoTable(TestConnection, "CarrierShipmentHeader", [
			(CarrierShipmentHeaderSchema.Constants.PK, carrierShipmentGuid),
			(CarrierShipmentHeaderSchema.Constants.CSH_CarrierShipmentReference, carrierShipmentGuid.Substring(0, 20)),
			(CarrierShipmentHeaderSchema.Constants.CSH_SystemCreateTimeUtc, DateTime.UtcNow),
			(CarrierShipmentHeaderSchema.Constants.CSH_SystemCreateUser, "XXX"),
			(CarrierShipmentHeaderSchema.Constants.CSH_SystemLastEditTimeUtc, DateTime.UtcNow),
			(CarrierShipmentHeaderSchema.Constants.CSH_SystemLastEditUser, "XXX"),
			]);

		DatabaseHelper.InsertIntoTable(TestConnection, "CarrierShipmentCargo", [("CSC_PK", Guid.NewGuid().ToString()),
			(CarrierShipmentCargoSchema.Constants.CSC_CSH_CarrierShipment, carrierShipmentGuid),
			(CarrierShipmentCargoSchema.Constants.CSC_CargoType, "CNT"),
			(CarrierShipmentCargoSchema.Constants.CSC_CargoID, "CNT1"),
			(CarrierShipmentCargoSchema.Constants.CSC_CargoMovementTypeOrigin, "FCL"),
			(CarrierShipmentCargoSchema.Constants.CSC_CargoMovementTypeDestination, "FCL"),
			(CarrierShipmentCargoSchema.Constants.CSC_DeliveryDrayage, "LL"),
			(CarrierShipmentCargoSchema.Constants.CSC_ReceiptDrayage, "DP"),
			(CarrierShipmentCargoSchema.Constants.CSC_SystemCreateTimeUtc, DateTime.UtcNow),
			(CarrierShipmentCargoSchema.Constants.CSC_SystemCreateUser, "XXX"),
			(CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditTimeUtc, DateTime.UtcNow),
			(CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditUser, "XXX")]);

		DatabaseHelper.InsertIntoTable(TestConnection, "CarrierShipmentCargo", [("CSC_PK", Guid.NewGuid().ToString()),
			(CarrierShipmentCargoSchema.Constants.CSC_CSH_CarrierShipment, carrierShipmentGuid),
			(CarrierShipmentCargoSchema.Constants.CSC_CargoType, "ROR"),
			(CarrierShipmentCargoSchema.Constants.CSC_CargoID, "CNT2"),
			(CarrierShipmentCargoSchema.Constants.CSC_CargoMovementTypeOrigin, "FCL"),
			(CarrierShipmentCargoSchema.Constants.CSC_CargoMovementTypeDestination, "FCL"),
			(CarrierShipmentCargoSchema.Constants.CSC_DeliveryDrayage, "LL"),
			(CarrierShipmentCargoSchema.Constants.CSC_ReceiptDrayage, "DP"),
			(CarrierShipmentCargoSchema.Constants.CSC_SystemCreateTimeUtc, DateTime.UtcNow),
			(CarrierShipmentCargoSchema.Constants.CSC_SystemCreateUser, "XXX"),
			(CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditTimeUtc, DateTime.UtcNow),
			(CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditUser, "XXX")]);

		DatabaseHelper.InsertIntoTable(TestConnection, "CarrierShipmentCargo", [("CSC_PK", Guid.NewGuid().ToString()),
			(CarrierShipmentCargoSchema.Constants.CSC_CSH_CarrierShipment, carrierShipmentGuid),
			(CarrierShipmentCargoSchema.Constants.CSC_CargoType, "BBK"),
			(CarrierShipmentCargoSchema.Constants.CSC_CargoID, "CNT3"),
			(CarrierShipmentCargoSchema.Constants.CSC_CargoMovementTypeOrigin, "FCL"),
			(CarrierShipmentCargoSchema.Constants.CSC_CargoMovementTypeDestination, "FCL"),
			(CarrierShipmentCargoSchema.Constants.CSC_DeliveryDrayage, "LL"),
			(CarrierShipmentCargoSchema.Constants.CSC_ReceiptDrayage, "DP"),
			(CarrierShipmentCargoSchema.Constants.CSC_SystemCreateTimeUtc, DateTime.UtcNow),
			(CarrierShipmentCargoSchema.Constants.CSC_SystemCreateUser, "XXX"),
			(CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditTimeUtc, DateTime.UtcNow),
			(CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditUser, "XXX")]);
	}

	protected override void AssertTransformationResults()
	{
		AssertCargo(cargoType: "CNT", expectedTransformationValue: "ANY");
		AssertCargo(cargoType: "ROR", expectedTransformationValue: "");
		AssertCargo(cargoType: "BBK", expectedTransformationValue: "");
	}

	void AssertCargo(string cargoType, string expectedTransformationValue)
	{
		var drayage = new List<string>();
		var sql = $@"SELECT {CarrierShipmentCargoSchema.Constants.CSC_DeliveryDrayage},
			{CarrierShipmentCargoSchema.Constants.CSC_ReceiptDrayage}
			FROM {CarrierShipmentCargoSchema.Constants.SqlSchemaName}.{CarrierShipmentCargoSchema.Constants.TableName}
			WHERE {CarrierShipmentCargoSchema.Constants.CSC_CargoType} = '{cargoType}'";

		TestConnection.ExecuteReader(sql, record =>
		{
			drayage.Add((string)record[CarrierShipmentCargoSchema.Constants.CSC_DeliveryDrayage]);
			drayage.Add((string)record[CarrierShipmentCargoSchema.Constants.CSC_ReceiptDrayage]);
		});

		AssertEquals($"Cargo type: {cargoType}", true, drayage.All(s => s.Equals(expectedTransformationValue)));
	}
}

