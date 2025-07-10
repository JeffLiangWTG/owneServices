using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Gate
{
	public class PopulateMovementBookingNumber : DataTransformation
	{
		public override string UserDescription => "Add and populate GteGateMovementBooking.GBM_MovementBookingNumber";

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			if (DbObjectCreator.TableExists(Db.Connection, GteGateMovementBookingSchema.Constants.TableName)
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, GteGateMovementBookingSchema.Constants.TableName, GteGateMovementBookingSchema.Constants.GBM_MovementBookingNumber, "VARCHAR(11)", defaultValue: "''"))
			{
				var sql = @"
IF NOT EXISTS (SELECT 1 FROM [dbo].[StmNums] WHERE [SN_Name] = 'GteMovementBookingNumber')
BEGIN
	DECLARE @EmptyCount INT;

	SELECT @EmptyCount = COUNT(*)
	FROM [dbo].[GteGateMovementBooking]
	WHERE [GBM_MovementBookingNumber] = '';

	INSERT INTO [dbo].[StmNums] ([SN_Name], [SN_Value], [SN_SystemCreateTimeUtc])
	VALUES ('GteMovementBookingNumber', ISNULL(@EmptyCount, 0) + 1, GETUTCDATE());
END;

WITH GteGateMovementBookings AS (
	SELECT 
		ROW_NUMBER() OVER (ORDER BY (SELECT GBM_SystemCreateTimeUtc)) AS FountainValue,
		[GBM_MovementBookingNumber],
		[GBM_SystemLastEditTimeUtc],
		[GBM_SystemLastEditUser]
	FROM [dbo].[GteGateMovementBooking]
	WHERE [GBM_MovementBookingNumber] = ''
)
UPDATE GteGateMovementBookings
SET 
	[GBM_MovementBookingNumber] = 'GBM' + RIGHT('000000000' + CAST(FountainValue AS VARCHAR), 8),
	[GBM_SystemLastEditTimeUtc] = GETUTCDATE(),
	[GBM_SystemLastEditUser] = '~BP'
";

				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
