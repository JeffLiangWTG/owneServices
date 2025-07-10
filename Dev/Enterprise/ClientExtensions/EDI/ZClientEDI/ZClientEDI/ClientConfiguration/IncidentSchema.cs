using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;

namespace Enterprise.Client.EDI
{
	public static class IncidentSchema
	{
		#region EdiIncidentConversationMessageQueue

		/// <summary>
		/// A trigger on JobConversationMessage adds new IncidentRequest messages into this table
		/// for later processing.
		/// </summary>
		public static DatabaseObjectCreateScript EdiIncidentConversationMessageQueue
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiIncidentConversationMessageQueue", @"
CREATE TABLE dbo.EdiIncidentConversationMessageQueue
(
	JCQ_JCM uniqueidentifier not null,
	JCQ_PostedTimeUtc datetime not null
);

ALTER TABLE [EdiIncidentConversationMessageQueue]
    SET (LOCK_ESCALATION = DISABLE);

CREATE CLUSTERED INDEX [NR_RC__JCQ_PostedTimeUtc] ON [EdiIncidentConversationMessageQueue] ([JCQ_PostedTimeUtc] ASC) WITH (ALLOW_PAGE_LOCKS = OFF)

", "DROP TABLE EdiIncidentConversationMessageQueue");
			}
		}

		#endregion

		#region EdiLegacyMessage

		/// <summary>
		/// JobConversationMessages created by the legacy eRequest system have a matching record in this table.
		/// Allows us to distinguish messages from the new web portal from the legacy messages.
		/// </summary>
		public static DatabaseObjectCreateScript EdiLegacyConversationMessage
		{
			get
			{
				return new DatabaseObjectCreateScript("EdiLegacyConversationMessage", @"
CREATE TABLE dbo.EdiLegacyConversationMessage
(
	ELC_PK uniqueidentifier not null,
	ELC_JCM_Message uniqueidentifier not null,

	CONSTRAINT PK_EdiLegacyConversationMessage PRIMARY KEY NONCLUSTERED ([ELC_PK] ASC) WITH (ALLOW_PAGE_LOCKS = OFF),
	CONSTRAINT EdiLegacyConversationMessage_ELC_JCM_FK2_JobConversationMessage_RRR_120N FOREIGN KEY (ELC_JCM_Message) REFERENCES JobConversationMessage (JCM_PK),
);

ALTER TABLE [EdiLegacyConversationMessage]
    SET (LOCK_ESCALATION = DISABLE);

CREATE UNIQUE CLUSTERED INDEX [NR_UC__ELC_JCM_Message] ON [EdiLegacyConversationMessage] ([ELC_JCM_Message] ASC)
WITH (ALLOW_PAGE_LOCKS = OFF, IGNORE_DUP_KEY = OFF)
;
", "DROP TABLE EdiLegacyConversationMessage");
			}
		}

		#endregion

		#region Trigger JobConversationMessage

		public static DatabaseViewAndRoutineCreateScript TriggerJobConversationMessageScript()
		{
			return new DatabaseViewAndRoutineCreateScript("TG_Edi_JobConversationMessage_Insert", @"
CREATE TRIGGER TG_Edi_JobConversationMessage_Insert
ON JobConversationMessage
FOR INSERT
AS
SET NOCOUNT ON
insert dbo.EdiIncidentConversationMessageQueue(JCQ_JCM, JCQ_PostedTimeUtc)
select JCM_PK, JCM_PostedTimeUtc
from inserted
join dbo.JobConversation on JCM_JCC_Conversation = JCC_PK
join dbo.JobConversationParticipant on JCM_JCP_Participant = JCP_PK
join dbo.IncidentRequest on JCC_ParentID = INC_PK
where JCC_ParentTableCode = 'INC'
	and JCP_ParticipantTableCode = 'OC'
	and JCM_IsLocal = 0
	and JCM_IsSystem = 0

", "DROP TRIGGER TG_Edi_JobConversationMessage_Insert", DbRoutineType.SqlTriggerTypeDesc);
		}

		#endregion

		#region EdiViewIncidentStatusChange

		public static DatabaseViewAndRoutineCreateScript EdiViewIncidentStatusChangeScript()
		{
			return new DatabaseIndexedViewCreateScript("EdiViewIncidentStatusChange",
@"CREATE VIEW EdiViewIncidentStatusChange
WITH SCHEMABINDING
AS
SELECT IM_PK FROM dbo.IncidentRequest JOIN dbo.IncidentMain on INC_PK = IM_INC_Request WHERE INC_Status != IM_RequestStatus;
",
@"CREATE UNIQUE CLUSTERED INDEX NR_UC__EdiViewIncidentStatusChange
    ON dbo.EdiViewIncidentStatusChange(IM_PK) WITH (ALLOW_PAGE_LOCKS = OFF);
", "DROP VIEW EdiViewIncidentStatusChange", DbRoutineType.SqlViewTypeDesc);
		}

		#endregion
	}
}
