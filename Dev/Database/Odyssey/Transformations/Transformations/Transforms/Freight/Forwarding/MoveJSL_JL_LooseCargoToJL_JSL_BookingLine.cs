using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	internal class MoveJSL_JL_LooseCargoToJL_JSL_BookingLine : DataTransformation
	{
		public override string UserDescription => "Use JL_JSL_BookingLine rather than JSL_JL_LooseCargo to link JobSupplierBookingLine and JobPackLines";

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, JobSupplierBookingSchema.Constants.TableName)
				&& DbObjectCreator.TableExists(Db.Connection, JobSupplierBookingLineSchema.Constants.TableName)
				&& DbObjectCreator.TableExists(Db.Connection, JobPackLinesSchema.Constants.TableName)
				&& DbObjectCreator.ColumnExists(Db.Connection, JobSupplierBookingLineSchema.Constants.TableName, "JSL_JL_LooseCargo"))
			{
				DbObjectCreator.CreateColumnIfNotExists(Db.Connection, JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.Constants.JL_JSL_BookingLine, "uniqueidentifier");
				Db.Connection.ExecuteNonQuery(@"UPDATE jpl
SET
	jpl.JL_JSL_BookingLine = jsbl.JSL_PK,
	jpl.JL_SystemLastEditTimeUtc = GETUTCDATE(),
	jpl.JL_SystemLastEditUser = '~BP'
FROM dbo.JobSupplierBooking jsb
INNER JOIN dbo.JobSupplierBookingLine jsbl ON jsbl.JSL_JSB_Booking = jsb.JSB_PK
INNER JOIN dbo.JobPackLines jpl ON jpl.JL_PK = jsbl.JSL_JL_LooseCargo
WHERE jsb.JSB_LoadMode = 'LSE' AND jsb.JSB_Status = 'CNV' AND jpl.JL_JSL_BookingLine IS NULL");
			}
		}
	}
}
