using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier
{
	class PopulateCVO_VoyageId : DataTransformation
	{
		public override string UserDescription => "Populating Carrier Voyage CVO_VoyageId column.";

		protected override void OfflinePreUpgradeTransform()
		{
			if (!DbObjectCreator.TableExists(Db.Connection, "CarrierVoyage"))
			{
				ShowInfo("Skip transformation. Table CarrierVoyage does not exist.");
				return;
			}

			if (!DbObjectCreator.ColumnExists(Db.Connection, "CarrierVoyage", "CVO_VoyageId"))
			{
				ShowInfo("Adding CVO_VoyageId column for transformation.");
				Db.Connection.ExecuteNonQuery("ALTER TABLE dbo.CarrierVoyage ADD CVO_VoyageId varchar(20) NOT NULL CONSTRAINT [DF_CarrierVoyage_CVO_VoyageId] DEFAULT ('');");
			}

			const string sequence = "CarrierVoyageID";

			Db.Connection.ExecuteNonQuery($@"
BEGIN TRY
	IF NOT EXISTS(SELECT name FROM sys.sequences WHERE name = '{sequence}')
	BEGIN
		CREATE SEQUENCE [{sequence}] AS BIGINT START WITH 1 INCREMENT BY 1 MAXVALUE 9220000000000000000;
	END

	UPDATE
		dbo.CarrierVoyage
	SET
		CVO_SystemLastEditTimeUtc = CURRENT_TIMESTAMP,
		CVO_SystemLastEditUser = '~BP',
		CVO_VoyageId = CONCAT('VOY', FORMAT(NEXT VALUE FOR dbo.CarrierVoyageID, '00000000000000000' ))
	WHERE
		CVO_VoyageId = ''
END TRY
BEGIN CATCH
	THROW
END CATCH
");
		}
	}
}
