using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	public class PopulateProcessHeaderEffectiveAgreedDeliveryDate : DataTransformation
	{
		const string LastProcessedChunkForParentIdName = "PopulateProcessHeaderEffectiveAgreedDeliveryDate_LastProcessedParentId";
		const string LastProcessedChunkForPKName = "PopulateProcessHeaderEffectiveAgreedDeliveryDate_LastProcessedPK";

		public override string UserDescription => "Populate FH_EffectiveAgreedDeliveryDateUtc column for Process Headers";

		const string UpdateSQLCommon = @"
UPDATE header
SET
	FH_SystemLastEditTimeUtc = GETUTCDATE(),
	FH_SystemLastEditUser = '~BP',
	FH_EffectiveAgreedDeliveryDateUtc = COALESCE(header.FH_AgreedDeliveryDate, parent.FH_AgreedDeliveryDate)
FROM dbo.ProcessHeader header
JOIN dbo.ProcessHeader parent ON header.FH_FH_ParentHeader = parent.FH_PK
WHERE parent.FH_IsActive = 1
AND header.FH_IsActive = 1
AND header.FH_FC_DedicatedBuffer IS NOT NULL
AND header.FH_Status NOT IN ('CLS', 'COP')
AND COALESCE(header.FH_EffectiveAgreedDeliveryDateUtc, '1900-01-01') <> COALESCE(header.FH_AgreedDeliveryDate, parent.FH_AgreedDeliveryDate, '1900-01-01')
";

		const string UpdateSQLForParentId = $@"
{UpdateSQLCommon}
AND header.FH_ParentId BETWEEN @From AND @To
";

		const string UpdateSQLForPK = $@"
{UpdateSQLCommon}
AND header.FH_ParentId IS NULL
AND header.FH_PK BETWEEN @From AND @To
";

		void ProcessChunkForParentId(Guid from, Guid to) => ProcessChunk(UpdateSQLForParentId, from, to);

		void ProcessChunkForPK(Guid from, Guid to) => ProcessChunk(UpdateSQLForPK, from, to);

		void ProcessChunk(string sqlText, Guid from, Guid to)
		{
			using var command = Db.Connection.Command(sqlText);
			command.AddParameter("@From", SqlDbType.UniqueIdentifier, from);
			command.AddParameter("@To", SqlDbType.UniqueIdentifier, to);
			command.ExecuteNonQuery();
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var pairsToProcess = new (Action<Guid, Guid>, string)[]
			{
				(ProcessChunkForParentId, LastProcessedChunkForParentIdName),
				(ProcessChunkForPK, LastProcessedChunkForPKName),
			};

			foreach (var (processFunc, sqlText) in pairsToProcess)
			{
				var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, ProcessHeaderSchema.Constants.TableName);
				var operation = new GuidChunkingOperation(manager, 5000, rowCount, processFunc, sqlText, token);
				operation.DoChunking();
			}
		}
	}
}
