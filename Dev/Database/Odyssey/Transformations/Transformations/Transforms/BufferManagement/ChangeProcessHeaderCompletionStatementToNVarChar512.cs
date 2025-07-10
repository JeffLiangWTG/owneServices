using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	public class ChangeProcessHeaderCompletionStatementToNVarChar512 : DataTransformation
	{
		public override string UserDescription => "Change ProcessHeader FH_CompletionStatement from NVARCHAR(MAX) to NVARCHAR(512)";
		readonly int BatchSize;
		string TemporaryColumnName => $"{ColumnSynchroniser.ColumnRenamePrefix}{ProcessHeaderSchema.FH_CompletionStatement.Name}";
		const string SyncTriggerName = "TG_ProcessHeader_KeepCompletionStatementInSync";
		const string LastProcessedChunkForParentIdName = "ChangeProcessHeaderCompletionStatementToNVarChar512.LastProcessedChunkForPK";
		const string LastProcessedChunkForPKName = "ChangeProcessHeaderCompletionStatementToNVarChar512.LastProcessedChunkForParentId";
		const string TransformationHasRunToCompletion = "ChangeProcessHeaderCompletionStatementToNVarChar512.TransformationRunToCompletion";

		public ChangeProcessHeaderCompletionStatementToNVarChar512() : this(5000)
		{
		}

		internal ChangeProcessHeaderCompletionStatementToNVarChar512(int batchSize) : base()
		{
			BatchSize = batchSize;
		}

		protected override void OnlinePreUpgradeTransform()
		{
			if (!DbObjectCreator.TableExists(Db.Connection, ProcessHeaderSchema.Constants.TableName))
			{
				return;
			}

			if (DbObjectCreator.GetColumnTypeAndMaxLength(Db.Connection, ProcessHeaderSchema.Constants.TableName, ProcessHeaderSchema.FH_CompletionStatement.Name).length != 512)
			{
				ExtProperty.Database.Delete(Db.Connection, TransformationHasRunToCompletion);
			}

			var statusValue = ExtProperty.Database.Select(Db.Connection, TransformationHasRunToCompletion);
			if (statusValue != bool.TrueString)
			{
				DbObjectCreator.CreateColumnIfNotExists(Db.Connection, ProcessHeaderSchema.Constants.TableName, TemporaryColumnName, "NVARCHAR(512)");
				CreateSyncTrigger();

				var rowCount = (int)Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.ProcessHeader WHERE FH_ParentId IS NOT NULL");
				var chunkingOperation = new GuidChunkingOperation(manager, BatchSize, rowCount, ProcessChunkForParentId, LastProcessedChunkForParentIdName);
				chunkingOperation.DoChunking();
				ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkForParentIdName);

				rowCount = (int)Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.ProcessHeader WHERE FH_ParentId IS NULL");
				chunkingOperation = new GuidChunkingOperation(manager, BatchSize, rowCount, ProcessChunkForPK, LastProcessedChunkForPKName);
				chunkingOperation.DoChunking();
				ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkForPKName);

				ExtProperty.Database.Update(Db.Connection, TransformationHasRunToCompletion, bool.TrueString);
			}
		}

		void ProcessChunkForParentId(Guid from, Guid to) => ProcessChunk(GetUpdateSQLForParentId, from, to);

		void ProcessChunkForPK(Guid from, Guid to) => ProcessChunk(GetUpdateSQLForPK, from, to);

		void ProcessChunk(Func<string> func, Guid from, Guid to)
		{
			using (var command = Db.Connection.Command(func()))
			{
				command.AddParameter("@From", SqlDbType.UniqueIdentifier, from);
				command.AddParameter("@To", SqlDbType.UniqueIdentifier, to);
				command.ExecuteNonQuery();
			}
		}

		public void CreateSyncTrigger()
		{
			if (!DbObjectCreator.TriggerExists(Db.Connection, ProcessHeaderSchema.Constants.TableName, SyncTriggerName))
			{
				var sql = FormattableString.Invariant($@"
CREATE TRIGGER {SyncTriggerName} ON dbo.ProcessHeader FOR INSERT, UPDATE AS
BEGIN
	IF (@@rowcount = 0) RETURN;
	SET NOCOUNT ON;

	IF UPDATE(FH_CompletionStatement)
	BEGIN
		UPDATE tab
		SET
			tab.{TemporaryColumnName} = SUBSTRING(tab.FH_CompletionStatement, 1, 512),
			tab.FH_SystemLastEditTimeUtc = ISNULL(tab.FH_SystemLastEditTimeUtc, GetUtcDate()),
			tab.FH_SystemLastEditUser = CASE WHEN tab.FH_SystemLastEditUser = '' THEN '~BP' ELSE tab.FH_SystemLastEditUser END
		FROM dbo.ProcessHeader AS tab
		INNER JOIN inserted ON tab.FH_PK = inserted.FH_PK

		IF (@@error	!= 0) ROLLBACK TRANSACTION;
	END;
END;");
				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		string GetUpdateSQLForParentId()
		{
			return Invariant($@"
{GetUpdateCommonBit()}
WHERE FH_ParentId IS NOT NULL
	AND FH_ParentId > @From
	AND FH_ParentId <= @To
");
		}

		string GetUpdateSQLForPK()
		{
			return Invariant($@"
{GetUpdateCommonBit()}
WHERE FH_ParentId IS NULL
	AND FH_PK > @From
	AND FH_PK <= @To
");
		}

		string GetUpdateCommonBit()
		{
			return $@"
UPDATE dbo.ProcessHeader SET
	{TemporaryColumnName} = SUBSTRING(FH_CompletionStatement, 1, 512),
	FH_SystemLastEditTimeUtc = GetUtcDate(),
	FH_SystemLastEditUser = '~BP'
FROM
	dbo.ProcessHeader";
		}
	}
}
