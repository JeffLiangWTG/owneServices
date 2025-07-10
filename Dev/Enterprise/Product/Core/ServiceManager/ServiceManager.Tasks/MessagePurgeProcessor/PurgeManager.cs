using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.MessagePurgeProcessor
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods")]
	public static class PurgeManager
	{
		public static readonly ReadOnlyCollection<string> ApplicationCodes = new (new[] { ApplicationCodeList.Codes.KRCustoms });
		const int CommandTimeout = 1000;

		public static int PurgeInterchangesMarkedAsDiscarded(int recordsToPurge)
		{
			var filter = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodes);
			filter.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchangeStatusList.Codes.Discarded);
			var sqlText = string.Format(CultureInfo.InvariantCulture, PurgeInterchangeQuery, recordsToPurge, filter.LiteralTextSqlFormatted);
			using (var cmd = Db.Connection.Command(sqlText, CommandTimeout))
			{
				cmd.AddParameter("@RecordsToPurge", SqlDbType.Int, recordsToPurge);
				using (var reader = cmd.ExecuteReader())
				{
					return reader.Read() ? reader.GetInt32(0) : 0;
				}
			}
		}
		public const string PurgeInterchangeQuery = @"
BEGIN TRAN;

BEGIN TRY
	DECLARE @InterchangePKs TABLE (EI_PK uniqueidentifier PRIMARY KEY, EI_SystemCreateTimeUtc datetime);

	INSERT INTO @InterchangePKs 
	SELECT TOP {0} EI_PK, EI_SystemCreateTimeUtc FROM dbo.EDIInterchange WITH (ROWLOCK, UPDLOCK) 
	LEFT JOIN dbo.EDIMessage ON EM_EI = EI_PK
	WHERE EM_EI IS NULL AND ({1}) ORDER BY EI_SystemCreateTimeUtc;

	DECLARE @DeletedInterchangePKs TABLE (EI_PK uniqueidentifier PRIMARY KEY);

	DELETE FROM dbo.EDIInterchange WITH (ROWLOCK)
	OUTPUT DELETED.EI_PK INTO @DeletedInterchangePKs
	WHERE EI_PK IN (SELECT EI_PK from @InterchangePKs);

	DELETE FROM dbo.StmNote WITH (ROWLOCK) WHERE ST_ParentID IN (SELECT EI_PK FROM @DeletedInterchangePKs);

	SELECT COUNT(*) FROM @InterchangePKs;
	SELECT TOP 1 EI_SystemCreateTimeUtc FROM @InterchangePKs;
END TRY
BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK;
	THROW;
END CATCH;

IF (@@TRANCOUNT > 0) COMMIT;
";

		public static int PurgeMessagesMarkedAsDiscarded(int recordsToPurge)
		{
			var filter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodes);
			filter.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Discarded);
			var sqlText = string.Format(CultureInfo.InvariantCulture, PurgeMessageQuery, filter.LiteralTextSqlFormatted);
			using (var cmd = Db.Connection.Command(sqlText, CommandTimeout))
			{
					cmd.AddParameter("@RecordsToPurge", SqlDbType.Int, recordsToPurge);
					using (var reader = cmd.ExecuteReader())
					{
						return reader.Read() ? reader.GetInt32(0) : 0;
					}
			}
		}

		public const string PurgeMessageQuery = @"
BEGIN TRAN;

BEGIN TRY
	DECLARE @MessagePKs TABLE (EM_PK uniqueidentifier PRIMARY KEY, EM_EI uniqueidentifier, EM_SystemCreateTimeUtc datetime);

	INSERT INTO @MessagePKs 
	SELECT TOP (@RecordsToPurge) EM_PK, EM_EI, EM_SystemCreateTimeUtc
	FROM dbo.EDIMessage WITH (ROWLOCK, UPDLOCK)
	WHERE {0}
	ORDER BY EM_SystemCreateTimeUtc;

	-- Clear the request message relation from any EDI messages that have FK to a message being purged
	UPDATE dbo.EDIMessage SET EM_EM_RequestMessage = NULL
	WHERE EM_EM_RequestMessage IN (SELECT EM_PK FROM @MessagePKs) AND EM_PK NOT IN (SELECT EM_PK FROM @MessagePKs)

	DELETE FROM dbo.GenPivot WITH (ROWLOCK)
	WHERE XX_Relation2ID IN (SELECT EM_PK FROM @MessagePKs) AND XX_RelationType = 'XEM';

	DELETE FROM dbo.EDIMessage WITH (ROWLOCK)
	WHERE EM_PK IN (SELECT EM_PK FROM @MessagePKs);

	DELETE FROM dbo.StmNote WITH (ROWLOCK)
	WHERE ST_ParentID IN (SELECT EM_PK FROM @MessagePKs);

	DELETE FROM dbo.EDIMessageLogPivot WITH (ROWLOCK)
	WHERE EML_EM IN (SELECT EM_PK FROM @MessagePKs);

	DECLARE @DeletedInterchangePKs TABLE (EI_PK uniqueidentifier PRIMARY KEY);
	
	DELETE FROM dbo.EDIInterchange WITH (ROWLOCK)
	OUTPUT DELETED.EI_PK
	INTO @DeletedInterchangePKs
	WHERE (EI_PK IN (SELECT EM_EI FROM @MessagePKs)) AND (NOT EXISTS (SELECT NULL FROM dbo.EDIMessage WHERE EM_EI = EI_PK));

	DELETE FROM dbo.StmNote WITH (ROWLOCK)
	WHERE ST_ParentID IN (SELECT EI_PK FROM @DeletedInterchangePKs);

	SELECT COUNT(*) FROM @MessagePKs;
	SELECT TOP 1 EM_SystemCreateTimeUtc FROM @MessagePKs;
END TRY
BEGIN CATCH
	IF (@@TRANCOUNT > 0) ROLLBACK;
	THROW;
END CATCH;

IF (@@TRANCOUNT > 0) COMMIT;
";
	}
}
