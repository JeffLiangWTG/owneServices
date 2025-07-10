using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	public class UpdateCM_MessageReference : DataTransformation
	{
		public override string UserDescription => "Update CM_MessageReference for all CusMAWB";

		const string LastProcessedDTPropertyString = "UpdateTableCusMAWBColumnCM_MessageReference_LastProcessedDT";
		const string NextReferenceIDPropertyString = "UpdateTableCusMAWBColumnCM_MessageReference_NextReferenceID";
		internal const int MinReferenceValue = 1000;

		static string GetUpdateString(string additionalWhere)
		{
			return $@"
;WITH
	ToUpdate AS (
		SELECT 
			[CM_PK], 
			[CM_MessageReference],
			[CM_SystemLastEditTimeUtc],
			[CM_SystemLastEditUser],
			ROW_NUMBER() OVER(ORDER BY [CM_SystemCreateTimeUtc]) AS [RowNumber]
		FROM [dbo].[CusMAWB]
		WHERE [CM_MessageReference] = '' AND [CM_ApplicationCode] NOT IN ('TSW', 'NZE') {additionalWhere}
	)
UPDATE ToUpdate
SET [CM_MessageReference] = CONCAT('X', FORMAT(RowNumber + @LastEndId - 1, '00000000')),
	[CM_SystemLastEditTimeUtc] = GETUTCDATE(),
	[CM_SystemLastEditUser] = '~BP'

SELECT @@ROWCOUNT
";
		}

		protected override void OfflinePostUpgradeTransform()
		{
			var nextReferenceIDString = ExtProperty.Database.Select(Db.Connection, NextReferenceIDPropertyString);
			if (!int.TryParse(nextReferenceIDString, out var nextProcessedReferenceID))
			{
				nextProcessedReferenceID = MinReferenceValue;
			}

			var script = GetUpdateString(string.Empty);
			using (var cmd = Db.Connection.Command(script))
			{
				cmd.AddParameter("@LastEndId", SqlDbType.Int, nextProcessedReferenceID);
				var updatedAmount = (int)cmd.ExecuteScalar();
				nextProcessedReferenceID += updatedAmount;
			}

			UpdateFountain(nextProcessedReferenceID - MinReferenceValue);
			ExtProperty.Database.Delete(Db.Connection, LastProcessedDTPropertyString);
			ExtProperty.Database.Delete(Db.Connection, NextReferenceIDPropertyString);
		}

		static void UpdateFountain(int numberOfUpdatedReferences)
		{
			const string sql = @"
EXECUTE FountainGetNexts
      @Name = 'CusMAWBMessageReferenceNumberFountain'
    , @Owner='00000000-0000-0000-0000-000000000000'
    , @Amount = @UpdateAmount
    , @MinValue = 1000
    , @MaxValue = 9220000000000000000
    , @CanRollover = 0
    , @CallInSameTransaction = 1
";
			if (numberOfUpdatedReferences > 0)
			{
				using (var cmd = Db.Connection.Command(sql))
				{
					cmd.AddParameter("@UpdateAmount", SqlDbType.Int, numberOfUpdatedReferences);
					cmd.ExecuteNonQuery();
				}
			}
		}

		protected override void OnlinePreUpgradeTransform()
		{
			if (!DbObjectCreator.TableExists(Db.Connection, CusMAWBSchema.Constants.TableName))
			{
				return;
			}

			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CusMAWBSchema.Constants.TableName, CusMAWBSchema.Constants.CM_MessageReference, "VARCHAR(15)", "''");

			var lastProcessedDTString = ExtProperty.Database.Select(Db.Connection, LastProcessedDTPropertyString);
			var lastProcessedDT = long.TryParse(lastProcessedDTString, out var parsedDT) ? new DateTime(parsedDT) : (DateTime?)null;
			var nextReferenceIDString = ExtProperty.Database.Select(Db.Connection, NextReferenceIDPropertyString);
			int.TryParse(nextReferenceIDString, out var nextReferenceID);

			if (string.IsNullOrEmpty(nextReferenceIDString))
			{
				nextReferenceID = MinReferenceValue;
				var updateCreateTimeNullScript = GetUpdateString("AND [CM_SystemCreateTimeUtc] is NULL");
				using (var cmd = Db.Connection.Command(updateCreateTimeNullScript))
				{
					cmd.AddParameter("@LastEndId", SqlDbType.Int, nextReferenceID);

					var updatedAmount = (int)cmd.ExecuteScalar();
					nextReferenceID += updatedAmount;

					ExtProperty.Database.Update(Db.Connection, NextReferenceIDPropertyString, nextReferenceID.ToString());
				}
			}

			var updateChunkScript = GetUpdateString("AND [CM_SystemCreateTimeUtc] BETWEEN @LowerBound AND @UpperBound");
			foreach (var chunk in DateTimeChunker.GenerateChunks(10000, lastProcessedDT, CusMAWBSchema.CM_SystemCreateTimeUtc))
			{
				using (var cmd = Db.Connection.Command(updateChunkScript))
				{
					cmd.AddParameter("@LowerBound", CusMAWBSchema.CM_SystemCreateTimeUtc.SqlDbType, chunk.LowerBound);
					cmd.AddParameter("@UpperBound", CusMAWBSchema.CM_SystemCreateTimeUtc.SqlDbType, chunk.UpperBound);
					cmd.AddParameter("@LastEndId", SqlDbType.Int, nextReferenceID);

					var updatedAmount = (int)cmd.ExecuteScalar();
					nextReferenceID += updatedAmount;
				}

				ExtProperty.Database.Update(Db.Connection, LastProcessedDTPropertyString, chunk.UpperBound.Ticks.ToString());
				ExtProperty.Database.Update(Db.Connection, NextReferenceIDPropertyString, nextReferenceID.ToString());
			}
		}
	}
}
