using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	class TransformSuppressedDocsAndRemoveDummyContacts : DataTransformation
	{
		public override string UserDescription => "Transform Suppressed Documents and Remove Dummy Contacts";

		internal virtual int BatchSize => 1000;

		const string LastProcessedChunkPKName = "TransformSuppressedDocsAndRemoveDummyContacts.LastProcessedChunkPK";
		const string TransformationHasRunToCompletion = "TransformSuppressedDocsAndRemoveDummyContacts.TransformationRunToCompletion";
		const string TransformationHasProcessedCount = "TransformSuppressedDocsAndRemoveDummyContacts.TransformationProcessedCount";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var statusValue = ExtProperty.Database.Select(Db.Connection, TransformationHasRunToCompletion);
			if (statusValue != bool.TrueString)
			{
				var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
				var lastProcessedChunkPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;
				var processedCount = long.TryParse(ExtProperty.Database.Select(Db.Connection, TransformationHasProcessedCount), out var count) ? count : 0;

				var approximateRowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, OrgContactSchema.Constants.TableName);
				var chunks = GuidChunker.GenerateChunks(BatchSize, approximateRowCount, lastProcessedChunkPK);

				foreach (var chunk in chunks)
				{
					DeleteChunk(token, chunk.LowerBound, chunk.UpperBound);
					lastProcessedChunkPKString = chunk.UpperBound.ToString();
					processedCount = Math.Min(processedCount + BatchSize, approximateRowCount);
					ExtProperty.Database.Update(Db.Connection, LastProcessedChunkPKName, lastProcessedChunkPKString);
					ExtProperty.Database.Update(Db.Connection, TransformationHasProcessedCount, processedCount.ToString());
					manager?.ShowInfoMessage($"Processed {processedCount} of {approximateRowCount} record(s).");
				}

				ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
				ExtProperty.Database.Delete(Db.Connection, TransformationHasProcessedCount);
				ExtProperty.Database.Update(Db.Connection, TransformationHasRunToCompletion, bool.TrueString);
			}
		}

		void DeleteChunk(CancellationToken token, Guid fromPK, Guid toPK)
		{
			token.ThrowIfCancellationRequested();

			var failedDeletedPKs = new List<Guid>();
			//	ErrorNumber:547, https://learn.microsoft.com/en-us/sql/relational-databases/errors-events/database-engine-events-and-errors-0-to-999?view=sql-server-ver16
			var deleteBatchContactsSql = $@"
DECLARE @ContactPKs TABLE (ContactPK UNIQUEIDENTIFIER);
INSERT INTO @ContactPKs
SELECT
	OC_PK
FROM
	dbo.OrgContact
WHERE
	OC_ContactName = 'DUMMY CONTACT TO SUPPRESS DOCS'
	AND OC_NotifyMode = 'DND'
	AND OC_PK >= @FromPK
	AND OC_PK <= @ToPK

BEGIN TRY
	UPDATE
		dbo.OrgDocument 
	SET
		OrgDocument.OD_OC = NULL, 
		OrgDocument.OD_OH_Suppressed = OrgContact.OC_OH,
		OrgDocument.OD_SystemLastEditTimeUtc = GETUTCDATE(),
		OrgDocument.OD_SystemLastEditUser = '~BP'
	FROM
		dbo.OrgDocument
		INNER JOIN dbo.OrgContact ON OrgDocument.OD_OC = OrgContact.OC_PK
	WHERE
		OrgContact.OC_PK IN (SELECT ContactPK FROM @ContactPKs);

	DELETE FROM dbo.OrgContact WHERE OC_PK IN (SELECT ContactPK FROM @ContactPKs);
END TRY
BEGIN CATCH
	IF (ERROR_NUMBER() = 547)
		SELECT ContactPK FROM @ContactPKs
	ELSE THROW;
END CATCH
";
			using (var command = Db.Connection.Command(deleteBatchContactsSql))
			{
				command.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
				command.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						failedDeletedPKs.Add((Guid)reader["ContactPK"]);
					}
				}
			}

			if (failedDeletedPKs.Count > 0)
			{
				failedDeletedPKs.ForEach(pk =>
				{
					token.ThrowIfCancellationRequested();

					var deleteSingleContactSql = @"
BEGIN TRY
    DELETE FROM dbo.OrgContact WHERE OC_PK = @ContactPK
END TRY
BEGIN CATCH
    IF (ERROR_NUMBER() <> 547) THROW
END CATCH
";
					using (var command = Db.Connection.Command(deleteSingleContactSql))
					{
						command.AddParameter("@ContactPK", SqlDbType.UniqueIdentifier, pk);
						command.ExecuteNonQuery();
					}
				});
			}
		}
	}
}
