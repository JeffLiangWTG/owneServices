using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Gate
{
	public class PopulateGateVehicleMovementMainBooking : DataTransformation
	{
		public override string UserDescription => "Set GteVehicleMovement.GVM_GBK_MainBooking to populate new column";

		public override bool IsRequired => base.IsRequired
			&& DbObjectCreator.TableExists(Db.Connection, GteVehicleMovementSchema.Constants.TableName)
			&& !DbObjectCreator.ColumnExists(Db.Connection, GteVehicleMovementSchema.Constants.TableName, GteVehicleMovementSchema.Constants.GVM_GBK_MainBooking);

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, GteVehicleMovementSchema.Constants.TableName, GteVehicleMovementSchema.Constants.GVM_GBK_MainBooking, "UNIQUEIDENTIFIER", null);
			var sql = @"
BEGIN TRY
	WITH CTE AS (
		SELECT 
			[GVM_PK],
			[GVM_GBK_MainBooking],
			[GBK_PK],
			[GVM_SystemLastEditTimeUtc],
			[GVM_SystemLastEditUser]
		FROM
			(SELECT
				[GVM_PK],
				[GBK_PK],
				[GVM_GBK_MainBooking],
				[GVM_SystemLastEditTimeUtc],
				[GVM_SystemLastEditUser],
				ROW_NUMBER() OVER(PARTITION BY GGM_GVM_VehicleMovement ORDER BY
					(case when GGM_CancelledReason != '' THEN 1 ELSE 0 END),
					(case when GBK_BookingType = 'ADH' THEN 1 ELSE 0 END),
					(case when GBM_SlotStartTime IS NULL THEN 1 ELSE 0 END),
					GBM_SlotStartTime ASC) AS RowNum
			FROM [dbo].[GteVehicleMovement] VehicleMovement
				INNER JOIN [dbo].[GteGateMovement] GateMovement
				ON VehicleMovement.GVM_PK = GateMovement.GGM_GVM_VehicleMovement AND VehicleMovement.GVM_GBK_MainBooking IS NULL
				INNER JOIN [dbo].[GteGateMovementBooking] MovementBooking
				ON GateMovement.GGM_GBM_MovementBooking = MovementBooking.GBM_PK
				INNER JOIN .[GteBooking] Booking
				ON MovementBooking.GBM_GBK_Booking = Booking.GBK_PK
			) AS A
		WHERE RowNum = 1
	)

	UPDATE CTE
	SET
		[GVM_GBK_MainBooking] = [GBK_PK],
		[GVM_SystemLastEditTimeUtc] = GETUTCDATE(),
		[GVM_SystemLastEditUser] = '~BP'
	
	DELETE VehicleEntry
	FROM [dbo].[GteVehicleEntry] VehicleEntry
		INNER JOIN [dbo].[GteVehicleMovement] VehicleMovement
		ON VehicleMovement.GVM_GBK_MainBooking IS NULL AND VehicleEntry.GVE_GVM_VehicleMovement = VehicleMovement.GVM_PK
	
	DELETE [dbo].[GteVehicleMovement]
	WHERE [GVM_GBK_MainBooking] IS NULL
END TRY

BEGIN CATCH
	THROW
END CATCH
";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
