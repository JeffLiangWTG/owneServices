using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier
{
	class PopulateCSC_CargoID : DataTransformation
	{
		public override string UserDescription => "Populating Carrier Shipment CSC_CargoID column.";

		protected override void OfflinePreUpgradeTransform()
		{
			if (!DbObjectCreator.TableExists(Db.Connection, "CarrierShipmentCargo"))
			{
				ShowInfo("Skip transformation. Table CarrierShipmentCargo does not exist.");
				return;
			}

			if (!DbObjectCreator.ColumnExists(Db.Connection, "CarrierShipmentCargo", "CSC_CargoID"))
			{
				ShowInfo("Adding CSC_CargoID column for transformation.");
				Db.Connection.ExecuteNonQuery("ALTER TABLE dbo.CarrierShipmentCargo ADD CSC_CargoID varchar(20) NOT NULL CONSTRAINT [DF_CarrierShipmentCargo_CSC_CargoID] DEFAULT ('');");
			}

			const string sequence = "CarrierShipmentCargoID-f7394bf7-2295-4ac7-9aa6-61ad7c07d6e2";

			Db.Connection.ExecuteNonQuery($@"
DECLARE @last_used_value BIGINT = 0
DECLARE @range_size BIGINT = 0
DECLARE	@range_first_value SQL_VARIANT
DECLARE @range_last_value SQL_VARIANT

SELECT
	@last_used_value = CONVERT(BIGINT, last_used_value)
FROM
	sys.sequences
WHERE
	name = '{sequence}'

UPDATE
	dbo.CarrierShipmentCargo
SET
	@range_size = @range_size + 1,
	CSC_SystemLastEditTimeUtc = CURRENT_TIMESTAMP,
	CSC_SystemLastEditUser = '~BP',
	CSC_CargoID = 'CRG' + FORMAT(@last_used_value + @range_size, '00000000000000000')
WHERE
	CSC_CargoID = ''

IF(@range_size > 0)
BEGIN
	IF NOT EXISTS(SELECT name FROM sys.sequences WHERE name = '{sequence}')
	BEGIN
		CREATE SEQUENCE [{sequence}] AS BIGINT START WITH 1 INCREMENT BY 1 MAXVALUE 9220000000000000000;
	END

	EXEC sys.sp_sequence_get_range @sequence_name = '{sequence}', @range_size = @range_size, @range_first_value = @range_first_value OUTPUT, @range_last_value = @range_last_value OUTPUT;
END
");
		}
	}
}
