using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier
{
	class PopulateCPO_PortCallId : DataTransformation
	{
		public override string UserDescription => "Populating Carrier Voyage PortCall CPO_PortCallId column.";

		protected override void OfflinePreUpgradeTransform()
		{
			if (!DbObjectCreator.TableExists(Db.Connection, "CarrierVoyagePortCall"))
			{
				ShowInfo("Skip transformation. Table CarrierVoyagePortCall does not exist.");
				return;
			}

			if (!DbObjectCreator.ColumnExists(Db.Connection, "CarrierVoyagePortCall", "CPO_PortCallId"))
			{
				ShowInfo("Adding CSC_CargoID column for transformation.");
				Db.Connection.ExecuteNonQuery("ALTER TABLE dbo.CarrierVoyagePortCall ADD CPO_PortCallId varchar(20) NOT NULL CONSTRAINT [DF_CarrierVoyagePortCall_CPO_PortCallId] DEFAULT ('');");
			}

			const string sequence = "CarrierVoyagePortCallID";

			Db.Connection.ExecuteNonQuery($@"
BEGIN TRY
	IF NOT EXISTS(SELECT name FROM sys.sequences WHERE name = '{sequence}')
	BEGIN
		CREATE SEQUENCE [{sequence}] AS BIGINT START WITH 1 INCREMENT BY 1 MAXVALUE 9220000000000000000;
	END

	UPDATE
		dbo.CarrierVoyagePortCall
	SET
		CPO_SystemLastEditTimeUtc = CURRENT_TIMESTAMP,
		CPO_SystemLastEditUser = '~BP',
		CPO_PortCallId = CONCAT('PRT', FORMAT(NEXT VALUE FOR dbo.CarrierVoyagePortCallID, '00000000000000000' ))
	WHERE
		CPO_PortCallId = ''
END TRY
BEGIN CATCH
	THROW
END CATCH
");
		}
	}
}
