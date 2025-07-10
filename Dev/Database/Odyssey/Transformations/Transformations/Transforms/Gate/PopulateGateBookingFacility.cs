using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Gate
{
	public class PopulateGateBookingFacility : DataTransformation
	{
		public override string UserDescription => "Add and populate GBK_WW_Facility linking GteBooking to WhsWarehouse";

		public override bool IsRequired => DbObjectCreator.TableExists(Db.Connection, GteBookingSchema.Constants.TableName) && !DbObjectCreator.ColumnExists(Db.Connection, GteBookingSchema.Constants.TableName, GteBookingSchema.Constants.GBK_WW_Facility); 

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			var warehousePK = GetCYDFacility();
			if (warehousePK.IsNullOrEmpty())
			{
				var sql = @"

	DELETE FROM [dbo].[GteGateMovement]
	DELETE FROM [dbo].[GteVehicleEntry]
	DELETE FROM [dbo].[GteVehicleMovement]
	DELETE FROM [dbo].[GteGateMovementBooking]
	DELETE FROM [dbo].[GteVehicleMovementBooking]
	DELETE FROM [dbo].[GteVehicleDriverBooking]
	DELETE FROM [dbo].[GteBooking]
";
				Db.Connection.ExecuteNonQuery(sql);
			}
			else
			{
				DbObjectCreator.CreateColumnIfNotExists(Db.Connection, GteBookingSchema.Constants.TableName, GteBookingSchema.Constants.GBK_WW_Facility, "UNIQUEIDENTIFIER", warehousePK);
			}
		}

		string GetCYDFacility()
		{
			var sql = @"
SELECT TOP 1 [WW_PK]
FROM [dbo].[WhsWarehouse]
WHERE [WW_WarehouseType] = 'CYD' AND [WW_IsActive] = 1
ORDER BY [WW_WarehouseCode]";

			using (var command = Db.Connection.Command(sql))
			{
				var warehousePK = (Guid?)command.ExecuteScalar();
				return warehousePK.HasValue ? string.Format("'{0}'", warehousePK.ToString()) : string.Empty;
			}
		}
	}
}
