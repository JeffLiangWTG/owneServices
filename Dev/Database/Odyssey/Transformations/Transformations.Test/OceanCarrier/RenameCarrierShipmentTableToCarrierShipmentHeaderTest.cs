using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.OceanCarrier;

[UseSnapshotProtection]
[TestedType(typeof(RenameCarrierShipmentTableToCarrierShipmentHeader))]
sealed class RenameCarrierShipmentTableToCarrierShipmentHeaderTest : RenameTableTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new RenameCarrierShipmentTableToCarrierShipmentHeader();

	protected override void PrepareTestData()
	{
		DbObjectCreator.CreateTableIfNotExists(TestConnection, "CarrierShipment", @"
			CREATE TABLE [dbo].[CarrierShipment](
				[CSH_PK] [uniqueidentifier] NOT NULL,
				[CSH_SystemCreateTimeUtc] SMALLDATETIME NOT NULL,
				[CSH_SystemLastEditTimeUtc] SMALLDATETIME NOT NULL,
				[CSH_CarrierShipmentReference] VARCHAR(20) NOT NULL DEFAULT '',
				[CSH_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
				[CSH_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT ''
			)");
		DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(Db.Connection, Db.SqlDbOwnerSchema, "CarrierShipmentHeader");

		var oldTableColumns = DbObjectCreator.GetTableColumns(Db.Connection, "CarrierShipmentHeader");
		foreach (var oldColumn in oldTableColumns)
		{
			new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, "CarrierShipmentHeader", oldColumn).DropRelateObjects(Db.Connection);
		}

		TestConnection.ExecuteNonQuery("DROP TABLE IF EXISTS dbo.CarrierShipmentHeader");
		TestConnection.ExecuteNonQuery("DELETE FROM dbo.CarrierShipment");

		TestConnection.ExecuteNonQuery(@"
				DECLARE @shipmentPk UNIQUEIDENTIFIER = NEWID();
				INSERT INTO dbo.CarrierShipment (
					CSH_PK,
					CSH_SystemCreateTimeUtc,
					CSH_SystemLastEditTimeUtc,
					CSH_CarrierShipmentReference,
					CSH_SystemCreateUser,
					CSH_SystemLastEditUser
				) VALUES (
					@shipmentPk,
					GETDATE(),
					GETDATE(),
					'SHP_REF',
					'NON',
					'NON'
				);
			");
	}

	protected override void AssertTransformationResults()
	{
		var rows = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.CarrierShipmentHeader");
		AssertEquals(1, rows);
	}

	#region Implementations

	protected override void SetUp()
	{
		base.SetUp();
		disposableAdminConnection = ((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade();
	}

	protected override sealed void TearDown()
	{
		disposableAdminConnection.Dispose();
		base.TearDown();
	}

	IDisposable disposableAdminConnection;

	#endregion
}

