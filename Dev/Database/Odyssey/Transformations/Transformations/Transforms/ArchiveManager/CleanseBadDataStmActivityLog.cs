using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager
{
	public class CleanseBadDataStmActivityLog : DataTransformation
	{
		public override string UserDescription => "Cleanse bad/unusable data from the StmActivityLog table";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var batchSize = 1000;
			var lastProcessedPropertyName = $"{nameof(CleanseBadDataStmActivityLog)}.LastProcessedChunkPK";
			var lastProcessedGuid = Guid.Empty;
			long totalNumberOfRows = DataUtils.GetApproximateRowCountForTable(Db.Connection, StmActivityLogSchema.Constants.TableName);

			var operation = new GuidChunkingOperation(manager, batchSize, totalNumberOfRows, ProcessChunk, lastProcessedPropertyName, token);
			operation.DoChunking();
		}

		void ProcessChunk(Guid lowerBound, Guid upperBound)
		{
			var sqlDeleteBatchOfPks = @"
				DELETE FROM dbo.StmActivityLog 
				FROM dbo.StmActivityLog WITH (FORCESEEK, INDEX(PK_UX__S7_PK))
				WHERE S7_PK BETWEEN @startGuid AND @endGuid
				AND S7_OpenDateTimeUTC is NULL
				AND S7_CloseDateTimeUtc is NULL
				AND S7_GS_NKUser = ''
				AND S7_FormCaption = ''
				AND S7_ControllerId NOT LIKE 'd0d2148a-6188-47b4-8d59-70e591518437%'
				AND S7_ParentTableCode = ''
				OPTION (MAXDOP 1)
			";

			using (var command = Db.Connection.Command(sqlDeleteBatchOfPks))
			{
				command.AddParameter("@startGuid", SqlDbType.UniqueIdentifier, lowerBound);
				command.AddParameter("@endGuid", SqlDbType.UniqueIdentifier, upperBound);

				_ = command.ExecuteNonQuery();
			}
		}
	}
}
