using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.OceanCarrier;

[UseSnapshotProtection]
[TestedType(typeof(RenameCommercialAndOperationalMeasurementsToChargeableAndActual))]
sealed class RenameCommercialAndOperationalMeasurementsToChargeableAndActualTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new RenameCommercialAndOperationalMeasurementsToChargeableAndActual();

	RenamedColumnCase[] RenamedColumnCases;

	readonly Guid carrierShipmentPk = Guid.NewGuid();
	readonly Guid carrierShipmentCargoPk = Guid.NewGuid();
	readonly Guid refContainerChargeable = Guid.NewGuid();
	readonly Guid refContainerActual = Guid.NewGuid();

	protected override void AssertTransformationResults()
	{
		var sql = $@"
SELECT {string.Join(",", RenamedColumnCases.Select(x => x.NewName))}
FROM dbo.CarrierShipmentCargo
WHERE CSC_PK = '{carrierShipmentCargoPk}'
";

		TestConnection.ExecuteReader(sql, r => RenamedColumnCases.ForEach(x => x.ValueAfterRename = r[x.NewName]));

		foreach (var renamedColumnCase in RenamedColumnCases)
		{
			AssertEquals("Old column does not exist", false, DbObjectCreator.ColumnExists(TestConnection, CarrierShipmentCargoSchema.Constants.TableName, renamedColumnCase.OldName));
			AssertEquals("New column exists", true, DbObjectCreator.ColumnExists(TestConnection, CarrierShipmentCargoSchema.Constants.TableName, renamedColumnCase.NewName));
			AssertEquals("Value did not change", renamedColumnCase.SetValue, renamedColumnCase.ValueAfterRename);
		}
	}

	protected override void PrepareTestData()
	{
		InitColumns();

		foreach (var renamedColumnCase in RenamedColumnCases)
		{
			if (DbObjectCreator.ColumnExists(TestConnection, CarrierShipmentCargoSchema.Constants.TableName, renamedColumnCase.NewName))
			{
				new DbColumnDependencyRemover(CarrierShipmentCargoSchema.Constants.TableName, renamedColumnCase.NewName).DropRelateObjects(TestConnection);
				DbObjectCreator.RenameColumn(TestConnection, Db.SqlDbOwnerSchema, CarrierShipmentCargoSchema.Constants.TableName, renamedColumnCase.NewName, renamedColumnCase.OldName);
			}

			else if (!DbObjectCreator.ColumnExists(TestConnection, CarrierShipmentCargoSchema.Constants.TableName, renamedColumnCase.OldName))
			{
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, CarrierShipmentCargoSchema.Constants.TableName,
					renamedColumnCase.OldName, renamedColumnCase.DataType, renamedColumnCase.DefaultValue);
			}
		}

		var insertSql = $@"
INSERT INTO dbo.CarrierShipmentHeader (CSH_PK, CSH_CarrierShipmentReference, CSH_SystemCreateTimeUtc, CSH_SystemCreateUser, CSH_SystemLastEditTimeUtc, CSH_SystemLastEditUser)
VALUES ('{carrierShipmentPk}', 'AQM20240614111', GETUTCDATE(), '---', GETUTCDATE(), '---')";

		TestConnection.ExecuteNonQuery(insertSql);

		insertSql = $@"
INSERT INTO [dbo].[RefContainer] ([RC_PK], [RC_Code])
VALUES ('{refContainerChargeable}', 'CODE11')";

		TestConnection.ExecuteNonQuery(insertSql);

		insertSql = $@"
INSERT INTO [dbo].[RefContainer] ([RC_PK], [RC_Code])
VALUES ('{refContainerActual}', 'CODE22')";

		TestConnection.ExecuteNonQuery(insertSql);

		insertSql = $@"
INSERT INTO [dbo].[CarrierShipmentCargo]
		   ([CSC_PK], [CSC_CargoID] ,[CSC_CSH_CarrierShipment], [CSC_CargoMovementTypeOrigin], [CSC_CargoMovementTypeDestination], [CSC_ReceiptDrayage], [CSC_DeliveryDrayage],
			[CSC_SystemCreateTimeUtc], [CSC_SystemCreateUser], [CSC_SystemLastEditTimeUtc], [CSC_SystemLastEditUser],
			{string.Join(",\n\t\t\t", RenamedColumnCases.Select(x => $"[{x.OldName}]"))})
VALUES
		   ('{carrierShipmentCargoPk}','TESTCARGOID123','{carrierShipmentPk}','FCL','FCL', 'ANY', 'ANY',
			getutcdate(),'---',getutcdate(),'---', {string.Join(",\n\t\t\t", RenamedColumnCases.Select(x => x.ValueAsSql))})
";
		TestConnection.ExecuteNonQuery(insertSql);
	}

	void InitColumns()
	{
		RenamedColumnCases = new[]
		{
			new RenamedColumnCase(
				"CSC_CommercialGrossWeight",
				CarrierShipmentCargoSchema.CSC_ChargeableGrossWeight.Name,
				CarrierShipmentCargoSchema.CSC_ChargeableGrossWeight.TypeInfo,
				10m,
				"0"),

			new RenamedColumnCase(
				"CSC_CommercialGrossWeightUnit",
				CarrierShipmentCargoSchema.CSC_ChargeableGrossWeightUnit.Name,
				CarrierShipmentCargoSchema.CSC_ChargeableGrossWeightUnit.TypeInfo,
				"G",
				"''"),

			new RenamedColumnCase(
				"CSC_CommercialLength",
				CarrierShipmentCargoSchema.CSC_ChargeableLength.Name,
				CarrierShipmentCargoSchema.CSC_ChargeableLength.TypeInfo,
				11m,
				"0"),

			new RenamedColumnCase(
				"CSC_CommercialWidth",
				CarrierShipmentCargoSchema.CSC_ChargeableWidth.Name,
				CarrierShipmentCargoSchema.CSC_ChargeableWidth.TypeInfo,
				12m,
				"0"),

			new RenamedColumnCase(
				"CSC_CommercialHeight",
				CarrierShipmentCargoSchema.CSC_ChargeableHeight.Name,
				CarrierShipmentCargoSchema.CSC_ChargeableHeight.TypeInfo,
				13m,
				"0"),

			new RenamedColumnCase(
				"CSC_CommercialArea",
				CarrierShipmentCargoSchema.CSC_ChargeableArea.Name,
				CarrierShipmentCargoSchema.CSC_ChargeableArea.TypeInfo,
				19m,
				"0"),

			new RenamedColumnCase(
				"CSC_CommercialVolume",
				CarrierShipmentCargoSchema.CSC_ChargeableVolume.Name,
				CarrierShipmentCargoSchema.CSC_ChargeableVolume.TypeInfo,
				20m,
				"0"),

			new RenamedColumnCase(
				"CSC_CommercialUnitOfDimension",
				CarrierShipmentCargoSchema.CSC_ChargeableUnitOfDimension.Name,
				CarrierShipmentCargoSchema.CSC_ChargeableUnitOfDimension.TypeInfo,
				"FT",
				"''"),

			new RenamedColumnCase(
				"CSC_CommercialUnitOfArea",
				CarrierShipmentCargoSchema.CSC_ChargeableUnitOfArea.Name,
				CarrierShipmentCargoSchema.CSC_ChargeableUnitOfArea.TypeInfo,
				"MM2",
				"''"),

			new RenamedColumnCase(
				"CSC_CommercialUnitOfVolume",
				CarrierShipmentCargoSchema.CSC_ChargeableUnitOfVolume.Name,
				CarrierShipmentCargoSchema.CSC_ChargeableUnitOfVolume.TypeInfo,
				"CC",
				"''"),

			new RenamedColumnCase(
				"CSC_CommercialRevenueTons",
				CarrierShipmentCargoSchema.CSC_ChargeableRevenueTons.Name,
				CarrierShipmentCargoSchema.CSC_ChargeableRevenueTons.TypeInfo,
				14m,
				"0"),

			new RenamedColumnCase(
				"CSC_OperationalGrossWeight",
				CarrierShipmentCargoSchema.CSC_ActualGrossWeight.Name,
				CarrierShipmentCargoSchema.CSC_ActualGrossWeight.TypeInfo,
				15m,
				"0"),

			new RenamedColumnCase(
				"CSC_OperationalGrossWeightUnit",
				CarrierShipmentCargoSchema.CSC_ActualGrossWeightUnit.Name,
				CarrierShipmentCargoSchema.CSC_ActualGrossWeightUnit.TypeInfo,
				"T",
				"''"),

			new RenamedColumnCase(
				"CSC_OperationalLength",
				CarrierShipmentCargoSchema.CSC_ActualLength.Name,
				CarrierShipmentCargoSchema.CSC_ActualLength.TypeInfo,
				17m,
				"0"),

			new RenamedColumnCase(
				"CSC_OperationalWidth",
				CarrierShipmentCargoSchema.CSC_ActualWidth.Name,
				CarrierShipmentCargoSchema.CSC_ActualWidth.TypeInfo,
				18m,
				"0"),

			new RenamedColumnCase(
				"CSC_OperationalHeight",
				CarrierShipmentCargoSchema.CSC_ActualHeight.Name,
				CarrierShipmentCargoSchema.CSC_ActualHeight.TypeInfo,
				16m,
				"0"),

			new RenamedColumnCase(
				"CSC_OperationalArea",
				CarrierShipmentCargoSchema.CSC_ActualArea.Name,
				CarrierShipmentCargoSchema.CSC_ActualArea.TypeInfo,
				21m,
				"0"),

			new RenamedColumnCase(
				"CSC_OperationalVolume",
				CarrierShipmentCargoSchema.CSC_ActualVolume.Name,
				CarrierShipmentCargoSchema.CSC_ActualVolume.TypeInfo,
				22m,
				"0"),

			new RenamedColumnCase(
				"CSC_OperationalUnitOfDimension",
				CarrierShipmentCargoSchema.CSC_ActualUnitOfDimension.Name,
				CarrierShipmentCargoSchema.CSC_ActualUnitOfDimension.TypeInfo,
				"YD",
				"''"),

			new RenamedColumnCase(
				"CSC_OperationalUnitOfArea",
				CarrierShipmentCargoSchema.CSC_ActualUnitOfArea.Name,
				CarrierShipmentCargoSchema.CSC_ActualUnitOfArea.TypeInfo,
				"CM2",
				"''"),

			new RenamedColumnCase(
				"CSC_OperationalUnitOfVolume",
				CarrierShipmentCargoSchema.CSC_ActualUnitOfVolume.Name,
				CarrierShipmentCargoSchema.CSC_ActualUnitOfVolume.TypeInfo,
				"GA",
				"''"),

			new RenamedColumnCase(
				"CSC_RC_EquipmentType",
				CarrierShipmentCargoSchema.CSC_RC_ChargeableEquipmentType.Name,
				"uniqueidentifier",
				refContainerChargeable),

			new RenamedColumnCase(
				"CSC_RC_OperationalSizeType",
				CarrierShipmentCargoSchema.CSC_RC_ActualEquipmentType.Name,
				"uniqueidentifier",
				refContainerActual),

			new RenamedColumnCase(
				"CSC_OOGLostSlotsCommercial",
				CarrierShipmentCargoSchema.CSC_OOGLostSlotsChargeable.Name,
				CarrierShipmentCargoSchema.CSC_OOGLostSlotsChargeable.TypeInfo,
				23m),
		};
	}

	class RenamedColumnCase
	{
		public string OldName { get; set; }

		public string NewName { get; set; }

		public string DataType { get; set; }

		public object SetValue { get; set; }

		public string DefaultValue { get; set; }

		public object ValueAfterRename { get; set; }

		public RenamedColumnCase(string oldName, string newName, string dataType, object setValue, string defaultValue = null)
		{
			OldName = oldName;
			NewName = newName;
			DataType = dataType;
			SetValue = setValue;
			DefaultValue = defaultValue;
		}

		public string ValueAsSql =>
			SetValue switch
			{
				string s => $"'{s}'",
				decimal d => d.ToString(CultureInfo.InvariantCulture),
				Guid g => $"'{g}'",
				_ => throw new InvalidOperationException("Unknown data type: " + SetValue.GetType().FullName)
			};
	}

	protected override void SetUp()
	{
		base.SetUp();
		disposableAdminConnection = ((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade();
	}

	protected sealed override void TearDown()
	{
		disposableAdminConnection.Dispose();
		base.TearDown();
	}

	IDisposable disposableAdminConnection;
}
