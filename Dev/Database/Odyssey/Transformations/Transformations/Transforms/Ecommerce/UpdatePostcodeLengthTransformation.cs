using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce
{
	public class UpdatePostcodeLengthTransformation : DataTransformation
	{
		public override string UserDescription => "Update Postcode Length to 10 in database";

		protected override void OnlinePreUpgradeTransform()
		{
			var connection = Db.Connection;
			if (DbObjectCreator.TableExists(connection, CusISFHeaderSchema.Constants.TableName)
				&& DbObjectCreator.TableExists(connection, HVLVConsignmentSchema.Constants.TableName)
				&& DbObjectCreator.TableExists(connection, "HVLVDeliveryByArea")
				&& DbObjectCreator.ColumnExists(connection, CusISFHeaderSchema.Constants.TableName, CusISFHeaderSchema.Constants.BF_BuyerPostcode)
				&& DbObjectCreator.ColumnExists(connection, CusISFHeaderSchema.Constants.TableName, CusISFHeaderSchema.Constants.BF_SellerPostcode)
				&& DbObjectCreator.ColumnExists(connection, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ConsigneePostcode)
				&& DbObjectCreator.ColumnExists(connection, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ShipperPostcode)
				&& DbObjectCreator.ColumnExists(connection, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_ReturnPostcode)
				&& DbObjectCreator.ColumnExists(connection, "HVLVDeliveryByArea", "HDB_ConsigneePostcode"))
			{
				manager?.ShowInfoMessage("Create Temporary Trigger For CusISFHeader");
				CreateTemporaryTriggerForCusISFHeader();
				manager?.ShowInfoMessage("Create Temporary Trigger For HVLVConsignment");
				CreateTemporaryTriggerForHVLVConsignment();
				manager?.ShowInfoMessage("Create Temporary Trigger For HVLVDeliveryByArea");
				CreateTemporaryTriggerForHVLVDeliveryByArea();
				manager?.ShowInfoMessage("Update Column For CusISFHeader");
				UpdateColumnForCusISFHeader();
				manager?.ShowInfoMessage("Update Column For HVLVConsignment");
				UpdateColumnForHVLVConsignment();
				manager?.ShowInfoMessage("Update Column For HVLVDeliveryByArea");
				UpdateColumnForHVLVDeliveryByArea();
			}
		}

		void CreateTemporaryTriggerForCusISFHeader()
		{
			Db.Connection.ExecuteNonQuery(@"
CREATE OR ALTER TRIGGER check_postcode_length_on_cusisfheader
ON CusISFHeader
AFTER INSERT, UPDATE
AS
BEGIN
    IF UPDATE(BF_BuyerPostcode) OR UPDATE(BF_SellerPostcode)
    BEGIN
        UPDATE c
        SET 
            c.BF_BuyerPostcode = CASE 
                WHEN UPDATE(BF_BuyerPostcode) THEN LEFT(i.BF_BuyerPostcode, 10)
                ELSE c.BF_BuyerPostcode
            END,
            c.BF_SellerPostcode = CASE
                WHEN UPDATE(BF_SellerPostcode) THEN LEFT(i.BF_SellerPostcode, 10)
                ELSE c.BF_SellerPostcode
            END,
            c.BF_SystemLastEditTimeUtc = GETUTCDATE(),
            c.BF_SystemLastEditUser = '~BP'
        FROM CusISFHeader c
        INNER JOIN inserted i ON c.BF_PK = i.BF_PK;
    END
END");
		}

		void CreateTemporaryTriggerForHVLVConsignment()
		{
			Db.Connection.ExecuteNonQuery(@"
CREATE OR ALTER TRIGGER check_postcode_length_on_hvlvconsignment
ON HVLVConsignment
AFTER INSERT, UPDATE
AS
BEGIN
    IF UPDATE(HVC_ConsigneePostcode) OR UPDATE(HVC_ShipperPostcode) OR UPDATE(HVC_ReturnPostcode)
    BEGIN
        UPDATE c
        SET
            c.HVC_ConsigneePostcode = CASE 
                WHEN UPDATE(HVC_ConsigneePostcode) THEN LEFT(i.HVC_ConsigneePostcode, 10)
                ELSE c.HVC_ConsigneePostcode
            END,
            c.HVC_ShipperPostcode = CASE
                WHEN UPDATE(HVC_ShipperPostcode) THEN LEFT(i.HVC_ShipperPostcode, 10)
                ELSE c.HVC_ShipperPostcode
            END,
            c.HVC_ReturnPostcode = CASE
                WHEN UPDATE(HVC_ReturnPostcode) THEN LEFT(i.HVC_ReturnPostcode, 10)
                ELSE c.HVC_ReturnPostcode
            END,
            c.HVC_SystemLastEditTimeUtc = GETUTCDATE(),
            c.HVC_SystemLastEditUser = '~BP'
        FROM HVLVConsignment c
        INNER JOIN inserted i ON c.HVC_PK = i.HVC_PK;
    END
END");
		}

		void CreateTemporaryTriggerForHVLVDeliveryByArea()
		{
			Db.Connection.ExecuteNonQuery(@"
CREATE OR ALTER TRIGGER check_postcode_length_on_hvlvdeliverybyarea
ON HVLVDeliveryByArea
AFTER INSERT, UPDATE
AS
BEGIN
    IF UPDATE(HDB_ConsigneePostcode)
    BEGIN
        UPDATE c
        SET 
            c.HDB_ConsigneePostcode = CASE 
                WHEN UPDATE(HDB_ConsigneePostcode) THEN LEFT(i.HDB_ConsigneePostcode, 10)
                ELSE c.HDB_ConsigneePostcode
            END,
            c.HDB_SystemLastEditTimeUtc = GETUTCDATE(),
            c.HDB_SystemLastEditUser = '~BP'
        FROM HVLVDeliveryByArea c
        INNER JOIN inserted i ON c.HDB_PK = i.HDB_PK;
    END
END");
		}

		void UpdateColumnForCusISFHeader()
		{
			const int batchSize = 5000;
			var hasMoreRecords = true;

			DateTime lastProcessedCreateTime;
			using (var command = Db.Connection.Command("SELECT ISNULL(MIN(BF_SystemCreateTimeUtc), '2000-01-01') FROM CusISFHeader"))
			{
				lastProcessedCreateTime = (DateTime)command.ExecuteScalar();
			}

			DateTime maxProcessedCreateTime;
			using (var command = Db.Connection.Command("SELECT ISNULL(MAX(BF_SystemCreateTimeUtc), '2000-01-01') FROM CusISFHeader"))
			{
				maxProcessedCreateTime = (DateTime)command.ExecuteScalar();
			}

			while (hasMoreRecords)
			{
				var updateQuery = @"
DROP TABLE IF EXISTS #TempTable;
SELECT TOP (@BatchSize) BF_PK, BF_SystemCreateTimeUtc
INTO #TempTable
FROM CusISFHeader
WHERE BF_SystemCreateTimeUtc >= @LastProcessedCreateTime AND BF_SystemCreateTimeUtc <= DATEADD(DAY, 1, @LastProcessedCreateTime)
    AND ((BF_BuyerPostcode != '' AND LEN(BF_BuyerPostcode) > 10)
	OR (BF_SellerPostcode != '' AND LEN(BF_SellerPostcode) > 10))
ORDER BY BF_SystemCreateTimeUtc
OPTION (MAXDOP 1);

UPDATE h
SET BF_BuyerPostcode = LEFT(BF_BuyerPostcode, 10),
	BF_SellerPostcode = LEFT(BF_SellerPostcode, 10),
    BF_SystemLastEditTimeUtc = GETUTCDATE(),
    BF_SystemLastEditUser = '~BP'
FROM CusISFHeader h
INNER JOIN #TempTable t ON h.BF_PK = t.BF_PK;

SELECT @LastProcessedCreateTime = MAX(BF_SystemCreateTimeUtc) FROM #TempTable;";

				using (var command = Db.Connection.Command(updateQuery))
				{
					command.AddParameter("@BatchSize", SqlDbType.Int, batchSize);
					command.AddParameter("@LastProcessedCreateTime", SqlDbType.SmallDateTime, lastProcessedCreateTime);
					command.GetParameter("@LastProcessedCreateTime").Direction = ParameterDirection.InputOutput;

					command.ExecuteNonQuery();

					var lastProcessedCreateTimeValue = command.GetParameterValue("@LastProcessedCreateTime");
					lastProcessedCreateTime = lastProcessedCreateTimeValue != DBNull.Value ? (DateTime)lastProcessedCreateTimeValue : lastProcessedCreateTime.AddDays(1);

					hasMoreRecords = lastProcessedCreateTime < maxProcessedCreateTime;
				}
			}
		}

		void UpdateColumnForHVLVConsignment()
		{
			const int batchSize = 500;
			var hasMoreRecords = true;

			int lastProcessedClusterKey;
			using (var command = Db.Connection.Command("SELECT ISNULL(MIN(HVC_ClusterKey), 0) FROM HVLVConsignment"))
			{
				lastProcessedClusterKey = (int)command.ExecuteScalar();
			}

			int maxProcessedClusterKey;
			using (var command = Db.Connection.Command("SELECT ISNULL(MAX(HVC_ClusterKey), 0) FROM HVLVConsignment"))
			{
				maxProcessedClusterKey = (int)command.ExecuteScalar();
			}

			while (hasMoreRecords)
			{
				var batchUpdateQuery = @"
DROP TABLE IF EXISTS #TempTable;
SELECT TOP (@BatchSize) HVC_PK, HVC_ClusterKey
INTO #TempTable
FROM HVLVConsignment
WHERE HVC_ClusterKey >= @LastProcessedClusterKey AND HVC_ClusterKey <= @LastProcessedClusterKey + 50
    AND ((HVC_ConsigneePostcode != '' AND LEN(HVC_ConsigneePostcode) > 10)
    OR (HVC_ShipperPostcode != '' AND LEN(HVC_ShipperPostcode) > 10)
    OR (HVC_ReturnPostcode != '' AND LEN(HVC_ReturnPostcode) > 10))
ORDER BY HVC_ClusterKey
OPTION (MAXDOP 1);

UPDATE h
SET HVC_ConsigneePostcode = LEFT(HVC_ConsigneePostcode, 10),
	HVC_ShipperPostcode = LEFT(HVC_ShipperPostcode, 10),
	HVC_ReturnPostcode = LEFT(HVC_ReturnPostcode, 10),
    HVC_SystemLastEditTimeUtc = GETUTCDATE(),
    HVC_SystemLastEditUser = '~BP'
FROM HVLVConsignment h
INNER JOIN #TempTable t ON h.HVC_PK = t.HVC_PK;

SELECT @LastProcessedClusterKey = MAX(HVC_ClusterKey) FROM #TempTable;";

				using (var command = Db.Connection.Command(batchUpdateQuery))
				{
					command.AddParameter("@BatchSize", SqlDbType.Int, batchSize);
					command.AddParameter("@LastProcessedClusterKey", SqlDbType.Int, lastProcessedClusterKey);
					command.GetParameter("@LastProcessedClusterKey").Direction = ParameterDirection.InputOutput;

					command.ExecuteNonQuery();

					var lastProcessedClusterKeyValue = command.GetParameterValue("@LastProcessedClusterKey");
					lastProcessedClusterKey = lastProcessedClusterKeyValue != DBNull.Value ? (int)lastProcessedClusterKeyValue : lastProcessedClusterKey + 50;

					hasMoreRecords = lastProcessedClusterKey < maxProcessedClusterKey;
				}
			}
		}

		void UpdateColumnForHVLVDeliveryByArea()
		{
			Db.Connection.ExecuteNonQuery(@"
-- Use normal update statement instead of batch processing as the table is small
UPDATE HVLVDeliveryByArea 
SET HDB_ConsigneePostcode = LEFT(HDB_ConsigneePostcode, 10),
    HDB_SystemLastEditTimeUtc = GETUTCDATE(),
    HDB_SystemLastEditUser = '~BP' 
WHERE HDB_ConsigneePostcode != '' AND LEN(HDB_ConsigneePostcode) > 10;");
		}

		protected override void OfflinePreUpgradeTransform()
		{
			manager?.ShowInfoMessage("Drop Temporary Trigger");
			DropTemporaryTrigger();
		}

		void DropTemporaryTrigger()
		{
			Db.Connection.ExecuteNonQuery(@"
IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID('check_postcode_length_on_cusisfheader'))
    DROP TRIGGER check_postcode_length_on_cusisfheader;
IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID('check_postcode_length_on_hvlvconsignment'))
    DROP TRIGGER check_postcode_length_on_hvlvconsignment;
IF EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID('check_postcode_length_on_hvlvdeliverybyarea'))
    DROP TRIGGER check_postcode_length_on_hvlvdeliverybyarea;");
		}
	}
}
