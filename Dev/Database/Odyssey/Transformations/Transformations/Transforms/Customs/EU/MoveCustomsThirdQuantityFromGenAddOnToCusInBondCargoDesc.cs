using System;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU
{
	sealed class MoveCustomsThirdQuantityFromGenAddOnToCusInBondCargoDesc : DataTransformation
	{
		const int BatchSize = 1000;
		const string TriggerName = "[dbo].[TG_GenAddOnColumn_CTQandCTUQ_InsertAndUpdate]";
		const string LastProcessedChunkPKName = "PopulateCustomsThirdQuantityAndUnit.LastProcessedChunkPK";

		public override string UserDescription => "Move 'CTQ' and 'CTUQ' from GenAddOnColumn to BY_CustomsThirdQuantity and BY_CustomsThirdUnitQty from CusInBondCargoDesc.";

		protected override void OnlinePreUpgradeTransform()
		{
			base.OnlinePreUpgradeTransform();

			if (DbObjectCreator.TableExists(Db.Connection, GenAddOnColumnSchema.Constants.TableName)
				&& DbObjectCreator.ColumnExists(Db.Connection, GenAddOnColumnSchema.Constants.TableName, GenAddOnColumnSchema.Constants.XA_Data)
				&& DbObjectCreator.ColumnExists(Db.Connection, GenAddOnColumnSchema.Constants.TableName, GenAddOnColumnSchema.Constants.XA_Name))
			{
				PrepareDestinationColumns();
				AddInsertAndUpdateTriggerToGenAddOnColumn();
				ExecuteQuery(UpdateCusInBondCargoDescDataSQL);
				manager?.ShowInfoMessage($"Updated 'CTQ' and 'CTUQ' data in '{CusInBondCargoDescSchema.Constants.TableName}' from '{GenAddOnColumnSchema.Constants.TableName}'.");
			}
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			base.OnlinePostUpgradeTransform(token);

			ExecuteQuery(DeleteGenAddOnColumnSQL, token);
			manager?.ShowInfoMessage($"Removed 'CTQ' and 'CTUQ' data from table '{GenAddOnColumnSchema.Constants.TableName}'.");
		}

		void PrepareDestinationColumns()
		{
			var sql = new StringBuilder();
			if (!DbObjectCreator.ColumnExists(Db.Connection, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.Constants.BY_CustomsThirdQuantity))
			{
				sql.Append("ALTER TABLE [dbo].[CusInBondCargoDesc] ADD [BY_CustomsThirdQuantity] [decimal](19, 6) NULL;");
			}
			if (!DbObjectCreator.ColumnExists(Db.Connection, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.Constants.BY_CustomsThirdUnitQty))
			{
				sql.Append("ALTER TABLE [dbo].[CusInBondCargoDesc] ADD [BY_CustomsThirdUnitQty] [varchar](4) NULL;");
			}

			if (sql.Length > 0)
			{
				Db.Connection.ExecuteNonQuery(sql.ToString());
				manager?.ShowInfoMessage($"Added missing columns to '{CusInBondCargoDescSchema.Constants.TableName}'.");
			}
		}

		void ExecuteQuery(string baseSql, CancellationToken token = default)
		{
			var lastProcessedChunkPKString = ExtProperty.Table.Select(Db.Connection, GenAddOnColumnSchema.Constants.SqlSchemaName, GenAddOnColumnSchema.Constants.TableName, LastProcessedChunkPKName);

			var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;

			var approxCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, GenAddOnColumnSchema.Constants.TableName);
			var guidChunks = GuidChunker.GenerateChunks(BatchSize, approxCount, lastProcessedPK);

			var sql = $"{baseSql} AND GenAddOn.[XA_PK] >= @StartGuid AND GenAddOn.[XA_PK] <= @EndGuid;";

			var stopWatch = Stopwatch.StartNew();
			foreach (var chunk in guidChunks)
			{
				var startGuid = chunk.LowerBound;
				var endGuid = chunk.UpperBound;

				Db.Connection.ExecuteNonQuery(sql, cmd =>
				{
					cmd.AddParameter("StartGuid", SqlDbType.UniqueIdentifier, startGuid);
					cmd.AddParameter("EndGuid", SqlDbType.UniqueIdentifier, endGuid);
				});

				if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
				{
					lastProcessedChunkPKString = endGuid.ToString();
					ExtProperty.Table.Update(Db.Connection, GenAddOnColumnSchema.Constants.SqlSchemaName, GenAddOnColumnSchema.Constants.TableName, LastProcessedChunkPKName, lastProcessedChunkPKString);
					manager?.ShowInfoMessage($"PK of Last processed record: {lastProcessedChunkPKString}.");
					token.ThrowIfCancellationRequested();
					stopWatch.Restart();
				}
			}
			ExtProperty.Table.Delete(Db.Connection, GenAddOnColumnSchema.Constants.SqlSchemaName, GenAddOnColumnSchema.Constants.TableName, LastProcessedChunkPKName);
		}

		void AddInsertAndUpdateTriggerToGenAddOnColumn()
		{
			Db.Connection.ExecuteNonQuery(CreateOrAlterTriggerSQL);
			manager?.ShowInfoMessage($"Added trigger '{TriggerName}' to table '{GenAddOnColumnSchema.Constants.TableName}'.");
		}

		const string CreateOrAlterTriggerSQL = $@"
			CREATE OR ALTER TRIGGER {TriggerName}
			ON [dbo].[GenAddOnColumn]
			AFTER INSERT, UPDATE
			AS
			BEGIN
				IF EXISTS (SELECT 1 FROM Inserted WHERE [XA_Name] IN ('CTQ', 'CTUQ'))
				BEGIN
					UPDATE CargoDesc
					SET CargoDesc.[BY_CustomsThirdQuantity]  = CASE WHEN I.[XA_Name] = 'CTQ'  THEN COALESCE(TRY_CONVERT(DECIMAL(19,6), I.[XA_Data]), 0) ELSE CargoDesc.[BY_CustomsThirdQuantity] END
					   ,CargoDesc.[BY_CustomsThirdUnitQty]   = CASE WHEN I.[XA_Name] = 'CTUQ' THEN SUBSTRING(COALESCE(I.[XA_Data], ''), 1, 4)           ELSE CargoDesc.[BY_CustomsThirdUnitQty] END
					   ,CargoDesc.[BY_SystemLastEditUser]    = I.[XA_SystemLastEditUser]
					   ,CargoDesc.[BY_SystemLastEditTimeUtc] = I.[XA_SystemLastEditTimeUtc]
					FROM [dbo].[CusInBondCargoDesc]        AS CargoDesc
					INNER JOIN Inserted                    AS I          ON CargoDesc.[BY_PK]       = I.[XA_ParentID]
					INNER JOIN [dbo].[CusInBondMoveHeader] AS MoveHeader ON CargoDesc.[BY_ParentID] = MoveHeader.[BM_PK]
					INNER JOIN [dbo].[CusInBondHeader]     AS Header     ON MoveHeader.[BM_BH]      = Header.[BH_PK]
					WHERE Header.[BH_ApplicationCode] = 'NCT' AND I.[XA_ParentTableCode] = 'BY' AND I.[XA_Name] IN ('CTQ', 'CTUQ');
				END
			END;
		";

		const string UpdateCusInBondCargoDescDataSQL = @"
			UPDATE CargoDesc
			SET [BY_CustomsThirdQuantity]  = COALESCE(TRY_CONVERT(DECIMAL(19,6), GenAddOn.[XA_Data]), 0)
			   ,[BY_CustomsThirdUnitQty]   = SUBSTRING(COALESCE(CTUQ.[XA_Data], ''), 1, 4)
			   ,[BY_SystemLastEditTimeUtc] = GETUTCDATE()
			   ,[BY_SystemLastEditUser]    = 'E'
			FROM [dbo].[CusInBondCargoDesc]           AS CargoDesc
			   INNER JOIN [dbo].[GenAddOnColumn]      AS GenAddOn   ON GenAddOn.[XA_ParentID]  = CargoDesc.[BY_PK] AND GenAddOn.[XA_Name] = 'CTQ'
			   LEFT OUTER JOIN [dbo].[GenAddOnColumn] AS CTUQ       ON CTUQ.[XA_ParentID]      = CargoDesc.[BY_PK] AND CTUQ.[XA_Name]     = 'CTUQ'
			   INNER JOIN [dbo].[CusInBondMoveHeader] AS MoveHeader ON CargoDesc.[BY_ParentID] = MoveHeader.[BM_PK]
			   INNER JOIN [dbo].[CusInBondHeader]     AS Header     ON MoveHeader.[BM_BH]      = Header.[BH_PK]
			WHERE Header.[BH_ApplicationCode] = 'NCT' AND GenAddOn.[XA_ParentTableCode] = 'BY' 
		";

		const string DeleteGenAddOnColumnSQL = @"
			DELETE GenAddOn
			FROM [dbo].[GenAddOnColumn]               AS GenAddOn
			   INNER JOIN [dbo].[CusInBondCargoDesc]  AS CargoDesc  ON GenAddOn.[XA_ParentID]  = CargoDesc.[BY_PK]
			   INNER JOIN [dbo].[CusInBondMoveHeader] AS MoveHeader ON CargoDesc.[BY_ParentID] = MoveHeader.[BM_PK]
			   INNER JOIN [dbo].[CusInBondHeader]     AS Header     ON MoveHeader.[BM_BH]      = Header.[BH_PK]
			WHERE Header.[BH_ApplicationCode] = 'NCT' AND GenAddOn.[XA_ParentTableCode] = 'BY' AND GenAddOn.[XA_Name] IN ('CTQ', 'CTUQ') 
		";
	}
}
