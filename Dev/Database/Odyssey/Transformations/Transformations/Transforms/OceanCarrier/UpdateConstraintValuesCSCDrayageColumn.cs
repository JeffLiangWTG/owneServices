using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;

sealed class UpdateConstraintValuesCSCDrayageColumn : DataTransformation
{
	public override string UserDescription => "Convert Drayage columns to VARCHAR(3) and update constraint";
	protected override void OfflinePreUpgradeTransform()
	{
		if (!DbObjectCreator.TableExists(Db.Connection, CarrierShipmentCargoSchema.Constants.TableName))
		{
			return;
		}

		if (DbObjectCreator.GetColumnTypeAndMaxLength(Db.Connection, CarrierShipmentCargoSchema.Constants.TableName, CarrierShipmentCargoSchema.Constants.CSC_DeliveryDrayage).length == 3 &&
			DbObjectCreator.GetColumnTypeAndMaxLength(Db.Connection, CarrierShipmentCargoSchema.Constants.TableName, CarrierShipmentCargoSchema.Constants.CSC_ReceiptDrayage).length == 3)
		{
			return;
		}
		new DbColumnDependencyRemover(CarrierShipmentCargoSchema.Constants.SqlSchemaName, CarrierShipmentCargoSchema.Constants.TableName, "CSC_DeliveryDrayage").DropRelateObjects(Db.Connection);
		new DbColumnDependencyRemover(CarrierShipmentCargoSchema.Constants.SqlSchemaName, CarrierShipmentCargoSchema.Constants.TableName, "CSC_ReceiptDrayage").DropRelateObjects(Db.Connection);
		Db.Connection.ExecuteNonQuery(FormattableString.Invariant($"ALTER TABLE [{CarrierShipmentCargoSchema.Constants.SqlSchemaName}].[{CarrierShipmentCargoSchema.Constants.TableName}] DROP COLUMN [CSC_DeliveryDrayage]"));
		Db.Connection.ExecuteNonQuery(FormattableString.Invariant($"ALTER TABLE [{CarrierShipmentCargoSchema.Constants.SqlSchemaName}].[{CarrierShipmentCargoSchema.Constants.TableName}] DROP COLUMN [CSC_ReceiptDrayage]"));
		DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CarrierShipmentCargoSchema.Constants.TableName, CarrierShipmentCargoSchema.Constants.CSC_DeliveryDrayage, "VARCHAR(3)", defaultValue: "''");
		DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CarrierShipmentCargoSchema.Constants.TableName, CarrierShipmentCargoSchema.Constants.CSC_ReceiptDrayage, "VARCHAR(3)", defaultValue: "''");

		Db.Connection.ExecuteNonQuery($@"UPDATE {CarrierShipmentCargoSchema.Constants.TableName}
			SET
				{CarrierShipmentCargoSchema.Constants.CSC_DeliveryDrayage} = 'ANY',
				{CarrierShipmentCargoSchema.Constants.CSC_ReceiptDrayage} = 'ANY',
				{CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditTimeUtc} = {CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditTimeUtc},
				{CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditUser} = {CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditUser}
			WHERE {CarrierShipmentCargoSchema.Constants.CSC_CargoType} = 'CNT'");

		Db.Connection.ExecuteNonQuery($@"UPDATE {CarrierShipmentCargoSchema.Constants.TableName}
			SET
				CSC_DeliveryDrayage = '',
				CSC_ReceiptDrayage = '',
				{CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditTimeUtc} = {CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditTimeUtc},
				{CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditUser} = {CarrierShipmentCargoSchema.Constants.CSC_SystemLastEditUser}
			WHERE {CarrierShipmentCargoSchema.Constants.CSC_CargoType} != 'CNT'");
	}
}
