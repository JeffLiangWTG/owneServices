using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.DocumentScanning
{
	class RemoveDuplicatedMSCJobRequiredDocuments : DataTransformation
	{
		public override string UserDescription => "Remove duplicated MSC records in JobRequiredDocument";

		const string LastProcessedJobRequiredDocumentPK = "LastProcessedJobRequiredDocumentPK";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var lastProcessedPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedJobRequiredDocumentPK);
			var lastProcessedPK = Guid.TryParse(lastProcessedPKString, out var parsedPK) ? parsedPK : (Guid?)null;

			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, JobRequiredDocumentSchema.Constants.TableName);
			var chunks = GuidChunker.GenerateChunks(chunkSize: 1000, rowCount, lastProcessedPK);

			var stopWatch = Stopwatch.StartNew();
			foreach (var chunk in chunks)
			{
				ProcessBatch(chunk.LowerBound, chunk.UpperBound);

				if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
				{
					ExtProperty.Database.Update(Db.Connection, LastProcessedJobRequiredDocumentPK, chunk.UpperBound.ToString());
					manager.ShowInfoMessage($"Last processed batch chunk: {chunk.UpperBound}.");

					token.ThrowIfCancellationRequested();
					stopWatch.Restart();
				}
			}

			ExtProperty.Database.Delete(Db.Connection, LastProcessedJobRequiredDocumentPK);
		}

		void ProcessBatch(Guid startGuid, Guid endGuid)
		{
			var sql = $@"
WITH CTE AS
(
	SELECT [EQ_PK],
		ROW_NUMBER() OVER(
			PARTITION BY
				[EQ_ParentID], [EQ_ParentTableCode], [EQ_DocDescription],
				[EQ_DocCategory], [EQ_DocPeriod], [EQ_DateReceived], [EQ_ValidToDate],
				[EQ_DocNumber], [EQ_DocUsage], [EQ_RN_NKRelatedCountry],
				[EQ_OriginalDocRequired], [EQ_CreditControlDoc], [EQ_OH_DocumentOwner]
			ORDER BY [EQ_SystemCreateTimeUtc]) as [ROW_NUMBER]
	FROM [dbo].[JobRequiredDocument]
	WHERE [EQ_DocType] = 'MSC'
	AND [EQ_ParentID] BETWEEN @startGuid AND @endGuid
)
DELETE FROM CTE
WHERE [ROW_NUMBER] > 1
AND NOT EXISTS (SELECT 1 FROM [dbo].[JobRequiredDocumentAddInfo] WHERE [EX_EQ_RequiredDocument] = [EQ_PK])
AND NOT EXISTS (SELECT 1 FROM [dbo].[JobRequiredDocAttrib] WHERE [D0_EQ] = [EQ_PK])
OPTION (MAXDOP 1);
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@startGuid", SqlDbType.UniqueIdentifier, startGuid);
				command.AddParameter("@endGuid", SqlDbType.UniqueIdentifier, endGuid);

				command.ExecuteNonQuery();
			}
		}
	}
}
