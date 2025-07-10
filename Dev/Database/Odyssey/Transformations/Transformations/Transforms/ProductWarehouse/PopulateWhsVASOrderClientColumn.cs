using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class PopulateWhsVASOrderClientColumn : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Populate WhsVASOrder WVO_OH_Client column.";

		protected override void OnlinePreUpgradeTransform()
		{
			const string UpdateClientCount = "PopulateWhsVASOrderClientColumn.WHO_OH_Client.UpdateCnt";
			const string LastUpdatedJobID = "PopulateWhsVASOrderClientColumn.WHO_OH_Client.LastUpdatedJob";

			if (DbObjectCreator.ColumnExists(Db.Connection, WhsVASOrderSchema.Constants.TableName, "WVO_OA_ClientAddress"))
			{
				var mostCommonValue = DataUtils.GetTheMostPopularValueForTheColumn(Db.Connection, Db.DatabaseName, Db.SqlDbOwnerSchema, WhsVASOrderSchema.Constants.TableName, "WVO_OA_ClientAddress");

				var orgAddressOA_OH = mostCommonValue != null
				  ? (Guid)Db.Connection.ExecuteScalar("SELECT OA_OH FROM dbo.OrgAddress WHERE OA_PK = @mostCommonValue",
					c => c.AddParameter("@mostCommonValue", SqlDbType.UniqueIdentifier, Guid.Parse(mostCommonValue)))
				  : Guid.Empty;

				DbObjectCreator.CreateColumnIfNotExists(Db.Connection, WhsVASOrderSchema.Constants.TableName, WhsVASOrderSchema.Constants.WVO_OH_Client, "uniqueidentifier", Invariant($"'{orgAddressOA_OH}'"));

				EnsureTriggerExists();

				var fromJobId = ExtProperty.Database.Select(Db.Connection, LastUpdatedJobID) ?? "";

				var totalUpdatedClientCntString = ExtProperty.Database.Select(Db.Connection, UpdateClientCount);
				var totalUpdatedClientCnt = !string.IsNullOrEmpty(totalUpdatedClientCntString) && int.TryParse(totalUpdatedClientCntString, out var result) ? result : 0;
				var stopWatch = Stopwatch.StartNew();

				var updatedCount = 0;
				do
				{
					updatedCount = DoTransform(ref fromJobId);
					totalUpdatedClientCnt += updatedCount;

					if (stopWatch.Elapsed.TotalMinutes > 1 || updatedCount == 0)
					{
						ExtProperty.Database.Update(Db.Connection, LastUpdatedJobID, fromJobId);
						ExtProperty.Database.Update(Db.Connection, UpdateClientCount, totalUpdatedClientCnt.ToString());
						manager?.ShowInfoMessage(Invariant($"Finished updating WhsVASOrder client for '{totalUpdatedClientCnt}' entries."));

						stopWatch.Restart();
					}
				} while (updatedCount > 0);

				ExtProperty.Database.Delete(Db.Connection, UpdateClientCount);
				ExtProperty.Database.Delete(Db.Connection, LastUpdatedJobID);
			}

			static void EnsureTriggerExists()
			{
				const string triggerName = "TG_WhsVASOrder_KeepClientInSync";

				if (!DbObjectCreator.TriggerExists(Db.Connection, WhsVASOrderSchema.Constants.TableName, triggerName))
				{
					var triggerScript = Invariant($@"
CREATE TRIGGER dbo.{triggerName}
	ON dbo.WhsVASOrder
	FOR INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE (WVO_OA_ClientAddress)
	BEGIN
		UPDATE dbo.WhsVASOrder SET WVO_OH_Client = OA_OH,
		WVO_SystemLastEditTimeUtc = GETUTCDATE(),
		WVO_SystemLastEditUser = WVO_SystemLastEditUser
		FROM dbo.WhsVASOrder WVO
		JOIN dbo.OrgAddress OA on OA_PK = WVO_OA_ClientAddress
		WHERE WVO_PK in (SELECT inserted.WVO_PK FROM inserted)
	END
END");

					Db.Connection.ExecuteNonQuery(triggerScript);
				}
			}

			int DoTransform(ref string fromJobId)
			{
				const int batchSize = 1000;

				var updateCommand = Invariant($@"
DECLARE @MaxJobID varchar(12);
 
WITH DATA AS
(
		SELECT TOP({batchSize})
		WVO_JobID, WVO_OH_Client, WVO_SystemLastEditTimeUtc, WVO_SystemLastEditUser, OA_OH
		FROM
			dbo.WhsVASOrder
		JOIN dbo.OrgAddress ON OA_PK = WVO_OA_ClientAddress
		WHERE
			WVO_JobID > @fromJobId
			AND WVO_OH_Client <> OA_OH
		ORDER BY
			WVO_JobID
), MaxJobIDData AS
(
	SELECT DATA.*,
		MaxJobID =  MAX(WVO_JobID) OVER()
	FROM
		DATA
)

UPDATE MaxJobIDData
	SET @maxJobID = MaxJobID,
		MaxJobIDData.WVO_OH_Client = OA_OH,
		WVO_SystemLastEditTimeUtc = GETUTCDATE(),
		WVO_SystemLastEditUser = '~BP';
 
SELECT @@ROWCOUNT AS UpdatedCount, ISNULL(@MaxJobID, '') as MaxJobID");

				using (var command = Db.Connection.Command(updateCommand))
				{
					command.AddParameterBasedOnDbColumn("@fromJobId", fromJobId, WhsVASOrderSchema.WVO_JobID);

					using (var reader = command.ExecuteReader())
					{
						reader.Read();

						var updateCount = (int)reader["UpdatedCount"];
						fromJobId = (string)reader["MaxJobID"];
						return (updateCount);
					}
				}
			}
		}

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.ColumnExists(Db.Connection, WhsVASOrderSchema.Constants.TableName, WhsVASOrderSchema.Constants.WVO_OH_Client))
			{
				var autoVersionExists = DbObjectCreator.ColumnExists(Db.Connection, WhsVASOrderSchema.Constants.TableName, WVO_AutoVersionColumnName);
				var autoVersionColumn = autoVersionExists ? $"{WVO_AutoVersionColumnName}," : string.Empty;
				var updateString = autoVersionExists ? $",{WVO_AutoVersionColumnName} = (WhsVASOrder.{WVO_AutoVersionColumnName} + 1) % 32768" : string.Empty;
				var disableTrigger = !autoVersionExists ? string.Empty : @"
IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_WhsVASOrder_UpdateAutoVersion')
BEGIN
	DISABLE TRIGGER TG_WhsVASOrder_UpdateAutoVersion ON dbo.WhsVASOrder
END";
				var enableTrigger = !autoVersionExists ? string.Empty : @"
IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_WhsVASOrder_UpdateAutoVersion')
BEGIN
	ENABLE TRIGGER TG_WhsVASOrder_UpdateAutoVersion ON dbo.WhsVASOrder
END";

				var sql = $@"
{disableTrigger}

;WITH DuplicateCTE1 (WVO_PK, WVO_OH_Client, WVO_CustomerReferenceNo, WVO_SystemCreateTimeUtc, WVO_SystemLastEditTimeUtc, WVO_SystemLastEditUser,{autoVersionColumn} DuplicateCount)
AS
(
	SELECT
		WVO_PK,
		WVO_OH_Client,
		WVO_CustomerReferenceNo,
		WVO_SystemCreateTimeUtc,
		WVO_SystemLastEditTimeUtc,
		WVO_SystemLastEditUser,{autoVersionColumn}
		ROW_NUMBER() OVER (PARTITION BY WVO_OH_Client, WVO_CustomerReferenceNo ORDER BY WVO_SystemCreateTimeUtc) AS DuplicateCount
	FROM 
		dbo.WhsVASOrder
)
SELECT WVO_PK, WVO_OH_Client, WVO_CustomerReferenceNo, WVO_SystemCreateTimeUtc,{autoVersionColumn} WVO_SystemLastEditTimeUtc, WVO_SystemLastEditUser
INTO #Duplicates
FROM DuplicateCTE1
WHERE DuplicateCount > 1

;WITH DuplicateCTE2 (WVO_PK, WVO_CustomerReferenceNo, WVO_SystemLastEditTimeUtc, WVO_SystemLastEditUser,{autoVersionColumn} BaseReferenceNumber, PostfixIndex)
AS
(
	SELECT
		WVO_PK,
		WVO_CustomerReferenceNo,
		WVO_SystemLastEditTimeUtc,
		WVO_SystemLastEditUser,{autoVersionColumn}
		LEFT(WVO_CustomerReferenceNo, 31),
		ISNULL(ExistingMaxAppendIndex.ExistingNumber, 0) + ROW_NUMBER() OVER (PARTITION BY WVO_OH_Client, LEFT(WVO_CustomerReferenceNo, 31) ORDER BY WVO_SystemCreateTimeUtc) AS PostfixIndex
	FROM 
		#Duplicates Duplicates
		CROSS APPLY 
		(
			SELECT
				MAX(CAST(RIGHT(Existing.WVO_CustomerReferenceNo, 3) AS smallint)) ExistingNumber
			FROM 
				dbo.WhsVASOrder Existing
			WHERE 
				Existing.WVO_OH_Client = Duplicates.WVO_OH_Client AND
				Existing.WVO_CustomerReferenceNo LIKE LEFT(Duplicates.WVO_CustomerReferenceNo, 31) + '-[0-9][0-9][0-9]'
		) AS ExistingMaxAppendIndex
)
UPDATE
	dbo.WhsVASOrder
SET
	WVO_CustomerReferenceNo = BaseReferenceNumber + '-' + RIGHT('000' + CAST(PostfixIndex AS varchar(3)), 3),
	WVO_SystemLastEditTimeUtc = SYSUTCDATETIME(),
	WVO_SystemLastEditUser = '~BP'
	{updateString}
FROM
	dbo.WhsVASOrder
	JOIN DuplicateCTE2 ON DuplicateCTE2.WVO_PK = WhsVASOrder.WVO_PK

{enableTrigger}

DROP TABLE #Duplicates";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);

				if (DbObjectCreator.ColumnExists(Db.Connection, WhsVASOrderSchema.Constants.TableName, WhsVASOrderSchema.Constants.WVO_OH_Client))
				{
					var autoVersionExists = DbObjectCreator.ColumnExists(Db.Connection, WhsVASOrderSchema.Constants.TableName, WVO_AutoVersionColumnName);
					var includeColumns = new[] { WhsVASOrderSchema.Constants.WVO_SystemLastEditTimeUtc, WhsVASOrderSchema.Constants.WVO_SystemLastEditUser };
					includeColumns = !autoVersionExists ? includeColumns : includeColumns.Append(WVO_AutoVersionColumnName).ToArray();

					indexProvider.New(WhsVASOrderSchema.Instance)
						.Key(WhsVASOrderSchema.Constants.WVO_OH_Client)
						.Key(WhsVASOrderSchema.Constants.WVO_CustomerReferenceNo)
						.Key(WhsVASOrderSchema.Constants.WVO_SystemCreateTimeUtc)
						.Include(includeColumns)
						.GetInfo();
				}

				return indexProvider;
			}
		}

		const string WVO_AutoVersionColumnName = "WVO_AutoVersion";
	}
}
