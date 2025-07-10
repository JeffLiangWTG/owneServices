using System;

namespace Enterprise.DbUpgrader.Shared
{
	public static class WorkflowDbTestHelper
	{
		public static InsertStatement GetProcessTemplateTriggerInsertSql(Guid pk, Guid templatePK, string description, string eventCode, int sequence)
		{
			return new InsertStatement(@"
				INSERT dbo.ProcessTemplateTrigger (
					P9T_PK, P9T_P0_Template, P9T_Description, P9T_SE_NKTriggerEvent, P9T_SystemCreateTimeUtc, P9T_SystemLastEditTimeUtc, P9T_Sequence
				) VALUES
				", FormattableString.Invariant($@"
				(
					'{pk}', '{templatePK}', '{description}', '{eventCode}', GetUtcDate(), GetUtcDate(), {sequence}
				)
				"));
		}

		public static InsertStatement GetTriggerInsertSql(Guid pk, Guid parentID, string parentTableCode, string description, string eventCode)
		{
			return new InsertStatement(@"
				INSERT dbo.ProcessTasks (
					P9_PK, P9_ParentID, P9_ParentTableCode, P9_Type, P9_Description, P9_SE_NKMilestoneEvent
				) VALUES
				", FormattableString.Invariant($@"
				(
					'{pk}', '{parentID}', '{parentTableCode}', 'TRG', '{description}', '{eventCode}'
				)
				"));
		}

		public static InsertStatement GetCompletionTriggerActionInsertSql(Guid pk, Guid triggerPK, string triggerActionType)
		{
			return new InsertStatement(@"
				INSERT dbo.ProcessTaskNotification (
					PQ_PK, PQ_P9, PQ_TriggerType
				) VALUES
				", FormattableString.Invariant($@"
				(
					'{pk}', '{triggerPK}', '{triggerActionType}'
				)
				"));
		}

		public static InsertStatement GetUniversalCompletionTriggerActionInsertSql(Guid pk, Guid universalTriggerPK, string triggerActionType)
		{
			return new InsertStatement(@"
				INSERT dbo.ProcessTaskNotification (
					PQ_PK, PQ_P9T_Trigger, PQ_TriggerType
				) VALUES
				", FormattableString.Invariant($@"
				(
					'{pk}', '{universalTriggerPK}', '{triggerActionType}'
				)
				"));
		}
	}
}
