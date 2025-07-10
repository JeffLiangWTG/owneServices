using System.Collections.Generic;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity.Tests
{
	[TestedType(typeof(ProcessTasksSubscriber))]
	class ProcessTasksSubscriberTest : DataScienceAuditSubscriberTestBase<ProcessTasksSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new ProcessTasksSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new ProcessTasksSubscriber();

			// Assert
			AssertEquals("DPT", subscriber.Code);
			AssertEquals(true, subscriber.NotifyInsert);
			AssertEquals(true, subscriber.NotifyUpdate);
			AssertEquals(true, subscriber.NotifyDelete);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			// var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(2, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table ProcessTasks required by ProcessTasksSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(75, subscriber.ColumnInfos.Count);
					var i = 0;

					AssertEquals("P9_PK", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_ActualDate", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_ActualDateUpdateType", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_ActualDateUtc", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_ActualDuration", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_AndOr", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_CardNote", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("nvarchar(40)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_CascadedEventsContext", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(250)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_CompletedTimeUtc", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_Condition1", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_Condition2", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_Condition2Value", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_DelayDurationSeconds", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_Description", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("nvarchar(50)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_EstDuration", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_EstimatedDefaultedFrom", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_EstimatedDefaultFromPredecessor", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_EstimatedDefaultTimeDelta", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_EstimatedHandoverTimeUtc", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_EstimatedTimeToComplete", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_EstimateVariationFactor", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("decimal(5,2)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_ExceptionAddedUtc", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_ExceptionDurationHours", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_ExceptionEndDate", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("datetimeoffset(0)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_FH_ProcessHeader", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_G4_RequiredCapability", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_GB_TriggerBranch", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_GC", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_GE_TriggerDepartment", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_GG_AssignedGroup", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_GS_NKAssignedStaffMember", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_IsCalendarItem", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_IsInterruptable", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_IsPublished", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_IsResetBeingAppliedToThisTask", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_IsValid", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_LineTriggerType", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_MilestoneExceptionAdded", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_NonWorkHours", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_OA", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_OC", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_OriginalScheduledDateUtc", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_Outcome", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_ParentID", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_ParentTableCode", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_ParentTemplateID", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_RecalculateScheduledDate", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_ReferencedID", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_ReferencedTableCode", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_RespondToCascadedEvents", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_RL_NKExceptionLocation", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(5)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_RN_NKDestinationCountry", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(2)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_RN_NKOriginCountry", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(2)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_ScheduledDate", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_ScheduledDateUtc", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_SE_NKExceptionEvent", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_SE_NKMilestoneEvent", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_SE_NKTaskCompletionEvent", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_Sequence", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_Status", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_SuppressDuplicates", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_SuspendedAt", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_SuspendedAtUtc", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_SystemCreateTimeUtc", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_SystemCreateUser", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_SystemLastEditTimeUtc", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_SystemLastEditUser", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_TaskCannotBeDeleted", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_TaskID", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(20)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_TotalSuspendedDuration", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_TriggerCondition", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_TriggerContext", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_TriggerField", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(80)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_TriggerFiredCountdown", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("smallint", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);

					AssertEquals("P9_Type", subscriber.ColumnInfos[i].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[i].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[i++].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames
		{
			get
			{
				yield return ProcessTasksSchema.P9_MilestoneCompletionPivotKey.Name;
				yield return ProcessTasksSchema.P9_FC_CurrentComponent.Name;
				yield return ProcessTasksSchema.P9_FormFlowType.Name;
			}
		}
	}
}
