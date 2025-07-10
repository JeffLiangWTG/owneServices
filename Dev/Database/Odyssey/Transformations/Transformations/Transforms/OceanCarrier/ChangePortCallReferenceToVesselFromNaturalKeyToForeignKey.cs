using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;

sealed class ChangePortCallReferenceToVesselFromNaturalKeyToForeignKey : DataTransformation
{
	public override string UserDescription => "Change the vessel reference on the port call from natural key to foreign key.";

	protected override void OfflinePreUpgradeTransform()
	{
		if (!DbObjectCreator.TableExists(Db.Connection, CarrierVoyagePortCallSchema.Constants.TableName))
		{
			return;
		}

		DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "CarrierVoyagePortCall", "CPO_RV_Vessel", "UNIQUEIDENTIFIER");

		const string sql = """
			DECLARE @placeholderVesselPk UNIQUEIDENTIFIER
			DECLARE @placeholderVesselName VARCHAR(35) = 'Placeholder (OCS - CPO_RV_Vessel)'

			SET @placeholderVesselPk = (SELECT TOP 1 RV_PK FROM dbo.RefVessel WHERE RV_Code = @placeholderVesselName)

			IF @placeholderVesselPk IS NULL
			BEGIN
				SET @placeholderVesselPk = NEWID()
			
				INSERT INTO [dbo].[RefVessel]
					([RV_PK]
					,[RV_Code]
					,[RV_IsActive]
					,[RV_SystemCreateTimeUtc]
					,[RV_SystemCreateUser]
					,[RV_SystemLastEditTimeUtc]
					,[RV_SystemLastEditUser])
				VALUES
					(@placeholderVesselPk
					,@placeholderVesselName
					,0
					,GETUTCDATE()
					,'~BP'
					,GETUTCDATE()
					,'~BP')
			END
			
			UPDATE
				dbo.CarrierVoyagePortCall
			SET
				CPO_RV_Vessel = ISNULL(RV_PK, @placeholderVesselPk),
				CPO_SystemLastEditTimeUtc = GETUTCDATE(),
				CPO_SystemLastEditUser = '~BP'
			FROM
				dbo.CarrierVoyagePortCall
			LEFT JOIN
				dbo.RefVessel ON CPO_VesselName = RV_Code
			WHERE
				CPO_RV_Vessel IS NULL
			""";

		Db.Connection.ExecuteNonQuery(sql);
	}
}
