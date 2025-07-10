using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.OceanCarrier;

[TestedType(typeof(ConvertPKGCargoToBBK))]
sealed class ConvertPKGCargoToBBKTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new ConvertPKGCargoToBBK();

	const string BreakBulkCode = "BBK";
	const string PackageCode = "PKG";
	const string ContainerCode = "CNT";
	const string RoRoCode = "ROR";

	static readonly string breakBulkPK = Guid.NewGuid().ToString();
	static readonly string packagePK = Guid.NewGuid().ToString();
	static readonly string containerPK = Guid.NewGuid().ToString();
	static readonly string roroPK = Guid.NewGuid().ToString();

	protected override void PrepareTestData()
	{
		var creator = new TransformationTestDataCreator();

		using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, CarrierShipmentCargoSchema.Constants.SqlSchemaName, CarrierShipmentCargoSchema.Constants.TableName, "Constraint_CSC_CargoType"))
		{
			var shipment = creator.CreateCarrierShipment();
			var testCases = GetTestCases(shipment.ToString());

			CreateCargoes(testCases);
		}
	}

	protected override void AssertTransformationResults()
	{
		foreach (var testCase in TestCases)
		{
			var sql = $@"SELECT {CarrierShipmentCargoSchema.Constants.CSC_CargoType}
FROM {CarrierShipmentCargoSchema.Constants.SqlSchemaName}.{CarrierShipmentCargoSchema.Constants.TableName}
WHERE {CarrierShipmentCargoSchema.Constants.PK} = {GetSqlValue(testCase.PK)}";

			string transformedCargoType = "";
			TestConnection.ExecuteReader(sql, row => transformedCargoType = row[CarrierShipmentCargoSchema.Constants.CSC_CargoType].ToString());

			AssertEquals("PKG (and only PKG) cargo type should be converted to BBK", testCase.ExpectedTransformedCargoType, transformedCargoType);
		}
	}

	List<List<(string column, object value)>> GetTestCases(string carrierShipment)
	{
		var fullTestCases = new List<List<(string column, object value)>>();
		var i = 1;

		foreach (var testCase in TestCases)
		{
			fullTestCases.Add([
				(CarrierShipmentCargoSchema.Constants.PK, testCase.PK),
				(CarrierShipmentCargoSchema.Constants.CSC_CSH_CarrierShipment, carrierShipment),
				(CarrierShipmentCargoSchema.Constants.CSC_CargoType, testCase.CargoType),
				(CarrierShipmentCargoSchema.Constants.CSC_ReceiptDrayage, testCase.CargoType != "CNT" ? "" : "ANY"),
				(CarrierShipmentCargoSchema.Constants.CSC_DeliveryDrayage, testCase.CargoType != "CNT" ? "" : "ANY"),
				(CarrierShipmentCargoSchema.Constants.CSC_CargoMovementTypeOrigin, "FCL"),
				(CarrierShipmentCargoSchema.Constants.CSC_CargoMovementTypeDestination, "FCL"),
				(CarrierShipmentCargoSchema.Constants.CSC_SystemCreateTimeUtc, DateTime.UtcNow),
				(CarrierShipmentCargoSchema.Constants.CSC_SystemCreateUser, "XXX"),
				(CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditTimeUtc, DateTime.UtcNow),
				(CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditUser, "XXX"),
				(CarrierShipmentCargoSchema.Constants.CSC_CargoID, $"XXX{i++:D5}"),
			]);
		}

		return fullTestCases;
	}

	void CreateCargoes(List<List<(string column, object value)>> testCases)
	{
		var sql =
			$@"INSERT INTO  {CarrierShipmentCargoSchema.Constants.SqlSchemaName}.{CarrierShipmentCargoSchema.Constants.TableName}
(
	{string.Join(",\n\t", testCases[0].Select(x => x.column))}
)
VALUES
	{string.Join(",\n\t", testCases.Select(x => $"({string.Join(", ", x.Select(y => GetSqlValue(y.value)))})"))}";

		TestConnection.Command(sql).ExecuteNonQuery();
	}

	string GetSqlValue(object value) =>
		value switch
		{
			null => "NULL",
			string s => $"'{s}'",
			bool b => b ? "1" : "0",
			decimal d => d.ToString(CultureInfo.InvariantCulture),
			DateTime d => $"'{d:s}'",
			DateTimeOffset d => $"'{d:O}'",
			_ => throw new InvalidOperationException("Unknown data type: " + value.GetType().FullName)
		};

	class CarrierShipmentCargoTestCase
	{
		internal CarrierShipmentCargoTestCase(string pK, string cargoType, string expectedTransformedCargoType)
		{
			PK = pK;
			CargoType = cargoType;
			ExpectedTransformedCargoType = expectedTransformedCargoType;
		}

		public string PK { get; }

		public string CargoType { get; }

		public string ExpectedTransformedCargoType { get; }
	}

	readonly List<CarrierShipmentCargoTestCase> TestCases = new()
	{
		new CarrierShipmentCargoTestCase(containerPK, ContainerCode, ContainerCode),
		new CarrierShipmentCargoTestCase(breakBulkPK, BreakBulkCode, BreakBulkCode),
		new CarrierShipmentCargoTestCase(packagePK, PackageCode, BreakBulkCode),
		new CarrierShipmentCargoTestCase(roroPK, RoRoCode, RoRoCode)
	};
}

