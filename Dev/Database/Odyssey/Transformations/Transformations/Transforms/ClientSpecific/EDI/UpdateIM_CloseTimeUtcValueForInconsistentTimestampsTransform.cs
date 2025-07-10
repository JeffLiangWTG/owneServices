using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	public class UpdateIM_CloseTimeUtcValueForInconsistentTimestampsTransform : DataTransformation
	{
		public override string UserDescription => "Update IM_CloseTimeUtc Value for inconsistent timestamps";

		const string tableName = "IncidentMain";
		const string LastProcessedPropertyName = "UpdateIM_CloseTimeUtcValueForInconsistentTimestamps_LowWatermark";

		protected virtual int BatchSize => 1000;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (!DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, tableName))
			{
				manager?.ShowInfoMessage($"No {tableName} table");
				return;
			}
			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, tableName);	
			var operation = new GuidChunkingOperation(manager, BatchSize, rowCount, ProcessChunk, LastProcessedPropertyName, token);
			operation.DoChunking();
		}

		void ProcessChunk(Guid fromPK, Guid toPK)
		{
			using (var transactionManager = Db.Connection.BeginTransactionWithManager())
			{
				Db.Connection.ExecuteScalar(
					UpdateSQL(),
					cmd =>
					{
						cmd.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
						cmd.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);
					});
				transactionManager.CommitTransaction();
			}
		}

		static string UpdateSQL()
		{
			return $@"
WITH IncidentCloseDateInfo AS (
    -- Get CloseDate from StmALog
    -- Reference to UpdateIM_CloseTimeUtcValueTransform
    SELECT
        IM_PK AS IncidentPK,
        MAX(COALESCE(SL_EventTimeUtc, SL_PostedTimeUtc)) AS CloseDate
    FROM dbo.IncidentMain WITH (FORCESEEK, INDEX(PK_UX__IM_PK))
    INNER JOIN dbo.StmALog WITH(INDEX(NR_RX__SL_Parent_SL_SE_NKEvent_SL_EventTime)) 
        ON SL_Parent = IM_PK 
        AND SL_SE_NKEvent = 'ICL' 
        AND SL_IsEstimate = 'N' 
        AND SL_IsCancelled = 'N'
        AND IM_PK BETWEEN @FromPK AND @ToPK
    GROUP BY IM_PK
), JobConversationCloseDate AS (
    -- Get the CloseDate from the JobConversationMessage
    SELECT 
        JC.JCC_ParentID AS IncidentPK,
        MAX(JCM.JCM_PostedTimeUtc) AS CloseDate
    FROM dbo.JobConversationMessage JCM WITH (FORCESEEK, INDEX(FK_RC__JCM_JCC_Conversation_JCM_PostedTimeUtc))
    INNER JOIN dbo.JobConversation JC 
        ON JCM.JCM_JCC_Conversation = JC.JCC_PK
    WHERE JCM.JCM_Body LIKE 'Closed As%'
      AND JC.JCC_ParentTableCode = 'INC'
      AND JC.JCC_ParentID BETWEEN @FromPK AND @ToPK
    GROUP BY JC.JCC_ParentID
), FinalCloseDate AS (
    -- Combines both CloseDate sources, giving priority to IncidentCloseDateInfo
    SELECT 
        COALESCE(i.IncidentPK, j.IncidentPK) AS IncidentPK,
        COALESCE(i.CloseDate, j.CloseDate) AS CloseDate
    FROM IncidentCloseDateInfo i
    FULL OUTER JOIN JobConversationCloseDate j 
        ON i.IncidentPK = j.IncidentPK
)
UPDATE dbo.IncidentMain
SET 
    IM_CloseTimeUtc = FinalCloseDate.CloseDate,
    IM_SystemLastEditTimeUtc = GETUTCDATE(),
    IM_SystemLastEditUser = '~BP'
FROM dbo.IncidentMain WITH (FORCESEEK, INDEX(PK_UX__IM_PK))
JOIN FinalCloseDate ON dbo.IncidentMain.IM_PK = FinalCloseDate.IncidentPK
WHERE
    IM_IncidentType = 'INC'
    AND (
        (
            (IM_CloseTimeUtc >= '2022-12-31' OR IM_Status <> 'CLS')  
            AND IM_CloseTimeUtc < IM_SystemCreateTimeUtc
        ) 
        OR (IM_Status = 'CLS' AND IM_CloseTimeUtc IS NULL)
    );
";
		}
	}
}
