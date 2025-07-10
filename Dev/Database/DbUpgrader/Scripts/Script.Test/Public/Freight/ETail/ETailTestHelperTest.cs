using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;

namespace Enterprise.Build.Database.Script.Public.Freight.ETail.Testing
{
	class ETailTestHelper
	{
		readonly DbConnection connection;

		public ETailTestHelper(DbConnection connection)
		{
			this.connection = connection;
		}

		public void ClearUsageTimeUtc(Guid itemPK)
		{
			var sql = @"UPDATE dbo.HVLVItem SET HVI_ShipperFirstUsageTimeUtc = null,
HVI_OriginFirstUsageTimeUtc = null,
HVI_DestinationFirstUsageTimeUtc = null
WHERE HVI_PK=@itemPK"; // sql statement for developers only
			using (var command = connection.Command(sql))
			{
				command.AddParameter("@itemPK", SqlDbType.UniqueIdentifier, itemPK);
				command.ExecuteNonQuery();
			}
		}

		public void SetLoadList(Guid loadListPK, Guid itemPK)
		{
			var sql = @"UPDATE dbo.HVLVItem SET HVI_HVL_LoadList = @loadListPK WHERE HVI_PK=@itemPK"; // sql statement for developers only
			using (var command = connection.Command(sql))
			{
				command.AddParameter("@loadListPK", SqlDbType.UniqueIdentifier, loadListPK);
				command.AddParameter("@itemPK", SqlDbType.UniqueIdentifier, itemPK);
				command.ExecuteNonQuery();
			}
		}

		public void SetLoadedOnShipment(Guid shipmentPK, Guid itemPK)
		{
			var sql = @"UPDATE dbo.HVLVItem SET HVI_JS_LoadedOnShipment = @shipmentPK WHERE HVI_PK=@itemPK"; // sql statement for developers only
			using (var command = connection.Command(sql))
			{
				command.AddParameter("@shipmentPK", SqlDbType.UniqueIdentifier, shipmentPK);
				command.AddParameter("@itemPK", SqlDbType.UniqueIdentifier, itemPK);
				command.ExecuteNonQuery();
			}
		}

		public Guid CreateHVLVBookingHeader(int clusterKey, string bookingReference)
		{
			var headerPK = Guid.NewGuid();
			var sql = @"
DECLARE @OrgAddressPK uniqueidentifier = (SELECT TOP 1 OA_PK FROM dbo.OrgAddress)
INSERT INTO dbo.HVLVBookingHeader
(HVH_PK, HVH_ClusterKey, HVH_BookingReference, HVH_OA_BillToParty, HVH_SystemCreateTimeUtc, HVH_SystemCreateUser, HVH_SystemLastEditTimeUtc, HVH_SystemLastEditUser, HVH_GrossWeightUQ, HVH_GrossVolumeUQ)
VALUES
(@HeaderPK, @ClusterKey, @BookingReference, @OrgAddressPK, '2016-06-15 00:00:00', 'E', '2016-06-15 00:00:00', 'E', 'KG', 'M3')
";
			using (var command = connection.Command(sql))
			{
				command.AddParameter("@HeaderPK", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@BookingReference", SqlDbType.VarChar, bookingReference);
				command.ExecuteNonQuery();
			}
			return headerPK;
		}

		public Guid CreateHVLVConsignment(Guid headerPK, int clusterKey, string consignmentId, string weightUQ = "KG", string volumeUQ = "M3", bool isActive = true)
		{
			var consignmentPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.HVLVConsignment (HVC_PK, HVC_ClusterKey, HVC_HVH_BookingHeader, HVC_ConsignmentId, HVC_WaybillNumber, HVC_Status, HVC_SystemCreateTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditTimeUtc, HVC_SystemLastEditUser, HVC_WeightUQ, HVC_VolumeUQ, HVC_IsActive)
VALUES (@ConsignmentPK, @ClusterKey, @HeaderPK, @ConsignmentId, @ConsignmentId, 'BKD', '2016-06-15 00:00:00', 'E', '2016-06-15 00:00:00', 'E', @WeightUQ, @VolumeUQ, @IsActive)
";
			using (DbCommand command = connection.Command(sql))
			{
				command.AddParameter("@ConsignmentPK", SqlDbType.UniqueIdentifier, consignmentPK);
				command.AddParameter("@HeaderPK", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@ConsignmentId", SqlDbType.VarChar, consignmentId);
				command.AddParameter("@WeightUQ", SqlDbType.VarChar, weightUQ);
				command.AddParameter("@VolumeUQ", SqlDbType.VarChar, volumeUQ);
				command.AddParameter("@IsActive", SqlDbType.Bit, isActive);
				command.ExecuteNonQuery();
			}
			return consignmentPK;
		}

		public void CreateHVLVConsignmentsWithDisplayOrders(Guid headerPK, int clusterKey, int[] displayOrders)
		{
			Func<int, string> getInsertLineFromDisplayOrder = displayOrder =>
			{
				var consignId = $"CONSIGN{currentId++}";
				return $"(newid(), @ClusterKey, @HeaderPK, '{consignId}', '{consignId}', 'CON', {displayOrder}, '2016-06-15 00:00:00', 'E', '2016-06-15 00:00:00', 'E')";
			};

			var sql = $@"
INSERT INTO dbo.HVLVConsignment (HVC_PK, HVC_ClusterKey, HVC_HVH_BookingHeader, HVC_ConsignmentId, HVC_WaybillNumber, HVC_Status, HVC_DisplayOrder, HVC_SystemCreateTimeUtc, HVC_SystemCreateUser, HVC_SystemLastEditTimeUtc, HVC_SystemLastEditUser)
VALUES
{string.Join($",{System.Environment.NewLine}", displayOrders.Select(getInsertLineFromDisplayOrder))}
";

			using (var command = connection.Command(sql))
			{
				command.AddParameter("@HeaderPK", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@ClusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
		}

		int currentId;

		public Guid CreateHVLVItem(Guid consignmentPK, int clusterKey, string itemID, decimal manifestedWeight, decimal actualWeight, decimal manifestedVolume, decimal actualVolume, bool isActive = true)
		{
			var itemPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.HVLVItem (HVI_PK, HVI_ClusterKey, HVI_ItemId, HVI_ManifestedWeight, HVI_ManifestedVolume, HVI_HVC_Consignment, HVI_ActualWeight, HVI_ActualVolume, HVI_IsActive, HVI_SystemCreateTimeUtc, HVI_SystemCreateUser, HVI_SystemLastEditTimeUtc, HVI_SystemLastEditUser)
VALUES (@ItemPK, @ClusterKey, @ItemID, @ManifestedWeight, @ManifestedVolume, @ConsignmentPK, @ActualWeight, @ActualVolume, @IsActive, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = connection.Command(sql))
			{
				command.AddParameter("@ItemPK", SqlDbType.UniqueIdentifier, itemPK);
				command.AddParameter("@ConsignmentPK", SqlDbType.UniqueIdentifier, consignmentPK);
				command.AddParameter("@ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@ItemID", SqlDbType.VarChar, itemID);
				command.AddParameter("@ManifestedWeight", SqlDbType.Decimal, manifestedWeight);
				command.AddParameter("@ManifestedVolume", SqlDbType.Decimal, manifestedVolume);
				command.AddParameter("@ActualWeight", SqlDbType.Decimal, actualWeight);
				command.AddParameter("@ActualVolume", SqlDbType.Decimal, actualVolume);
				command.AddParameter("@IsActive", SqlDbType.Bit, isActive);
				command.ExecuteNonQuery();
			}
			return itemPK;
		}

		public Guid CreateHVLVItem(string itemId, string consignmentId, string bookingHeaderReference, int clusterKey, object loadListPK, object shipmentPK, string status = "SHP", string importReleaseStatus = "C", string exportReleaseStatus = "C", bool isScannedAtDestination = false)
		{
			var itemPK = Guid.NewGuid();
			string sql = @"
INSERT INTO dbo.HVLVItem (HVI_PK, HVI_ClusterKey, HVI_ItemId, HVI_ManifestedWeight, HVI_ManifestedVolume, HVI_HVC_Consignment, HVI_ActualWeight, HVI_ActualVolume, HVI_JS_LoadedOnShipment, HVI_HVL_LoadList, HVI_Status, HVI_ImportReleaseStatus, HVI_ExportReleaseStatus, HVI_IsScannedAtDestination, HVI_SystemCreateTimeUtc, HVI_SystemCreateUser, HVI_SystemLastEditTimeUtc, HVI_SystemLastEditUser)
VALUES (@ItemPK, @ClusterKey, @ItemID, @ManifestedWeight, @ManifestedVolume, @ConsignmentPK, @ActualWeight, @ActualVolume, @ShipmentPK, @LoadListPK, @Status, @ImportReleaseStatus, @ExportReleaseStatus, @IsScannedAtDestination, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			using (var command = connection.Command(sql))
			{
				command.AddParameter("@ItemPK", SqlDbType.UniqueIdentifier, itemPK);
				command.AddParameter("@ConsignmentPK", SqlDbType.UniqueIdentifier, CreateHVLVConsignment(CreateHVLVBookingHeader(clusterKey, bookingHeaderReference), clusterKey, consignmentId, "KG", "M3"));
				command.AddParameter("@ClusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@ItemID", SqlDbType.VarChar, itemId);
				command.AddParameter("@ManifestedWeight", SqlDbType.Decimal, 1);
				command.AddParameter("@ManifestedVolume", SqlDbType.Decimal, 1);
				command.AddParameter("@ActualWeight", SqlDbType.Decimal, 1);
				command.AddParameter("@ActualVolume", SqlDbType.Decimal, 1);
				command.AddParameter("@ShipmentPK", SqlDbType.UniqueIdentifier, shipmentPK);
				command.AddParameter("@LoadListPK", SqlDbType.UniqueIdentifier, loadListPK);
				command.AddParameter("@Status", SqlDbType.VarChar, status);
				command.AddParameter("@ImportReleaseStatus", SqlDbType.VarChar, importReleaseStatus);
				command.AddParameter("@ExportReleaseStatus", SqlDbType.VarChar, exportReleaseStatus);
				command.AddParameter("@IsScannedAtDestination", SqlDbType.Bit, isScannedAtDestination);
				command.ExecuteNonQuery();
			}

			return itemPK;
		}

		public Guid CreateHVLVOriginLoadList(string uniqueReference)
		{
			var loadListPK = Guid.NewGuid();
			string sql = @"
INSERT INTO [dbo].[HVLVOriginLoadList]
	([HVL_PK]
	,[HVL_UniqueReference]
	,[HVL_Status]
	,[HVL_SystemCreateTimeUtc]
	,[HVL_SystemCreateUser]
	,[HVL_SystemLastEditTimeUtc]
	,[HVL_SystemLastEditUser])
VALUES
	(@LoadListPK,
	@UniqueReference,
	'LDG',
	'2016-06-15 00:00:00',
	'E',
	'2016-06-15 00:00:00',
	'E')";
			using (var command = connection.Command(sql))
			{
				command.AddParameter("@LoadListPK", SqlDbType.UniqueIdentifier, loadListPK);
				command.AddParameter("@UniqueReference", SqlDbType.VarChar, uniqueReference);
				command.ExecuteNonQuery();
			}

			return loadListPK;
		}

		internal IEnumerable<Guid> CreateMultipleHVLVItem(Guid consignmentPK, int clusterKey, int num, string itemIdPrefix)
		{
			var itemPK = new Guid[num];
			var sql = new StringBuilder(@"
INSERT INTO dbo.HVLVItem (HVI_PK, HVI_ClusterKey, HVI_ItemId, HVI_ManifestedWeight, HVI_ManifestedVolume, HVI_HVC_Consignment, HVI_ActualWeight, HVI_ActualVolume, HVI_SystemCreateTimeUtc, HVI_SystemCreateUser, HVI_SystemLastEditTimeUtc, HVI_SystemLastEditUser)
VALUES
");
			for (int i = 0; i < num; i++)
			{
				itemPK[i] = Guid.NewGuid();
				if (i > 0)
				{
					sql.Append(",");
				}

				sql.Append(string.Format(CultureInfo.InvariantCulture, "('{0}', @ClusterKey, '{1}{2}', '{2}', '{2}', @ConsignmentPK,  '{2}',  '{2}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", itemPK[i], itemIdPrefix, i + 1)); // sql statement for developers only
			}

			using (DbCommand command = connection.Command(sql.ToString()))
			{
				command.AddParameter("@ConsignmentPK", SqlDbType.UniqueIdentifier, consignmentPK);
				command.AddParameter("@ClusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return itemPK;
		}

		public DataRow GetRow(string tableOrViewName, string pkColumn, Guid pk)
		{
			string commandText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0} WHERE {1} = '{2}'", tableOrViewName, pkColumn, pk);
			var table = DataUtils.GetDataTableFromQuery(connection, commandText);

			if (table.Rows.Count != 1)
			{
				throw new InvalidOperationException("Expected row count to be 1 but was " + table.Rows.Count + ".");
			}

			return table.Rows[0];
		}

		internal void UpdateHVLVItem(Guid itemPK, decimal manifestedWeight, decimal actualWeight, decimal manifestedVolume, decimal actualVolume, bool isActive = true)
		{
			var sql = @"
UPDATE dbo.HVLVItem
SET
	HVI_ManifestedWeight = @ManifestedWeight,
	HVI_ManifestedVolume = @ManifestedVolume,
	HVI_ActualWeight = @ActualWeight,
	HVI_ActualVolume = @ActualVolume,
	HVI_IsActive = @IsActive
WHERE HVI_PK = @ItemPK
";
			using (DbCommand command = connection.Command(sql))
			{
				command.AddParameter("@ItemPK", SqlDbType.UniqueIdentifier, itemPK);
				command.AddParameter("@ManifestedWeight", SqlDbType.Decimal, manifestedWeight);
				command.AddParameter("@ManifestedVolume", SqlDbType.Decimal, manifestedVolume);
				command.AddParameter("@ActualWeight", SqlDbType.Decimal, actualWeight);
				command.AddParameter("@ActualVolume", SqlDbType.Decimal, actualVolume);
				command.AddParameter("@IsActive", SqlDbType.Bit, isActive);
				command.ExecuteNonQuery();
			}
		}

		internal void UpdateHVLVItems(Guid[] guids, string column, decimal updateValue)
		{
			var itemPKs = string.Join("','", guids);
			var sql = string.Format(CultureInfo.InvariantCulture, "UPDATE dbo.HVLVItem SET {0} = @UpdateValue WHERE HVI_PK in ('{1}')", column, itemPKs); // sql statement for developers only
			using (DbCommand command = connection.Command(sql))
			{
				command.AddParameter("@UpdateValue", SqlDbType.Decimal, updateValue);
				command.ExecuteNonQuery();
			}
		}

		public void DeteteRow(string tableOrViewName, string pkColumn, Guid pk)
		{
			var deleteSql = string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE {1} = '{2}'", tableOrViewName, pkColumn, pk);

			using (var command = connection.Command(deleteSql))
			{
				command.ExecuteNonQuery();
			}
		}

		public void DeteteRows(string tableOrViewName, string pkColumn, Guid[] pks)
		{
			var pklist = string.Join("','", pks);
			var deleteSql = string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE {1} in ('{2}')", tableOrViewName, pkColumn, pklist);

			using (var command = connection.Command(deleteSql))
			{
				command.ExecuteNonQuery();
			}
		}
	}
}

