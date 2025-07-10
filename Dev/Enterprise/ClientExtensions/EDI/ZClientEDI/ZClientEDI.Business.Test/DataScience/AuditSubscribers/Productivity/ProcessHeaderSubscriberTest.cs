using System.Collections.Generic;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity.Tests
{
	[TestedType(typeof(ProcessHeaderSubscriber))]
	class ProcessHeaderSubscriberTest : DataScienceAuditSubscriberTestBase<ProcessHeaderSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new ProcessHeaderSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new ProcessHeaderSubscriber();

			// Assert
			AssertEquals("DPH", subscriber.Code);
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
			AssertEquals(11, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table ProcessHeader required by ProcessHeaderSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(54, subscriber.ColumnInfos.Count);

					AssertEquals("FH_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("FH_AgreedDeliveryDate", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("FH_AgreedDeliveryDateDefaultHoursOffset", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("FH_AgreedDeliveryDateDefaultsFrom", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("FH_AllowTaskAutoAssignment", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("FH_BufferPenetrationPercentWhenCompleted", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("decimal(10,3)", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("FH_Category", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("FH_CompletionStatement", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("nvarchar(512)", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("FH_DateAcceptability", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("FH_DeadlineType", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("FH_DoNotStartBeforeDate", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("FH_EarliestStartDateDefaultsFrom", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("FH_EarliestStartDefaultHoursOffset", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("FH_FC_CurrentComponent", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("FH_FH_ParentHeader", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("FH_GG_ReleaseGroup", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("FH_IsActive", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("FH_IsCriticalHandover", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("FH_IsReleasableUnitParent", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[18].IsNullable);

					AssertEquals("FH_IsReleaseGroupSetByTemplate", subscriber.ColumnInfos[19].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[19].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[19].IsNullable);

					AssertEquals("FH_IsStandby", subscriber.ColumnInfos[20].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[20].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[20].IsNullable);

					AssertEquals("FH_JobCode", subscriber.ColumnInfos[21].ColumnName);
					AssertEquals("varchar(50)", subscriber.ColumnInfos[21].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[21].IsNullable);

					AssertEquals("FH_JobDescription", subscriber.ColumnInfos[22].ColumnName);
					AssertEquals("nvarchar(200)", subscriber.ColumnInfos[22].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[22].IsNullable);

					AssertEquals("FH_LastTransferType", subscriber.ColumnInfos[23].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[23].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[23].IsNullable);

					AssertEquals("FH_P0_Template", subscriber.ColumnInfos[24].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[24].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[24].IsNullable);

					AssertEquals("FH_ParentId", subscriber.ColumnInfos[25].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[25].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[25].IsNullable);

					AssertEquals("FH_ParentTableCode", subscriber.ColumnInfos[26].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[26].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[26].IsNullable);

					AssertEquals("FH_ParentTemplateId", subscriber.ColumnInfos[27].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[27].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[27].IsNullable);

					AssertEquals("FH_PlannedDurationInMinutes", subscriber.ColumnInfos[28].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[28].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[28].IsNullable);

					AssertEquals("FH_ReleaseDateTime", subscriber.ColumnInfos[29].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[29].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[29].IsNullable);

					AssertEquals("FH_RemainingMinutesToComplete", subscriber.ColumnInfos[30].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[30].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[30].IsNullable);

					AssertEquals("FH_StaggeredReleaseDelayExpiry", subscriber.ColumnInfos[31].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[31].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[31].IsNullable);

					AssertEquals("FH_Status", subscriber.ColumnInfos[32].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[32].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[32].IsNullable);

					AssertEquals("FH_SystemCreateTimeUtc", subscriber.ColumnInfos[33].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[33].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[33].IsNullable);

					AssertEquals("FH_SystemCreateUser", subscriber.ColumnInfos[34].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[34].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[34].IsNullable);

					AssertEquals("FH_SystemLastEditTimeUtc", subscriber.ColumnInfos[35].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[35].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[35].IsNullable);

					AssertEquals("FH_SystemLastEditUser", subscriber.ColumnInfos[36].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[36].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[36].IsNullable);

					AssertEquals("FH_TaskLowestOpenSequenceNumber", subscriber.ColumnInfos[37].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[37].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[37].IsNullable);

					AssertEquals("FH_TaskPenetrationResetDateTimeUtc", subscriber.ColumnInfos[38].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[38].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[38].IsNullable);

					AssertEquals("FH_TimeDelayFactor", subscriber.ColumnInfos[39].ColumnName);
					AssertEquals("decimal(2,1)", subscriber.ColumnInfos[39].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[39].IsNullable);

					AssertEquals("FH_TimeDelayMinutes", subscriber.ColumnInfos[40].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[40].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[40].IsNullable);

					AssertEquals("FH_VoteUpDownAmount", subscriber.ColumnInfos[41].ColumnName);
					AssertEquals("smallint", subscriber.ColumnInfos[41].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[41].IsNullable);

					AssertEquals("FH_WorkflowType", subscriber.ColumnInfos[42].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[42].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[42].IsNullable);

					AssertEquals("FH_IsApproved", subscriber.ColumnInfos[43].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[43].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[43].IsNullable);

					AssertEquals("FH_EffectiveNudge", subscriber.ColumnInfos[44].ColumnName);
					AssertEquals("decimal(22,4)", subscriber.ColumnInfos[44].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[44].IsNullable);

					AssertEquals("FH_FC_DedicatedBuffer", subscriber.ColumnInfos[45].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[45].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[45].IsNullable);

					AssertEquals("FH_GB_Branch", subscriber.ColumnInfos[46].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[46].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[46].IsNullable);

					AssertEquals("FH_GE_Department", subscriber.ColumnInfos[47].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[47].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[47].IsNullable);

					AssertEquals("FH_EffectiveAgreedDeliveryDateUtc", subscriber.ColumnInfos[48].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[48].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[48].IsNullable);

					AssertEquals("FH_ReleaseSequenceSortDateUtc", subscriber.ColumnInfos[49].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[49].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[49].IsNullable);

					AssertEquals("FH_LatestAcceptableReleaseDateUtc", subscriber.ColumnInfos[50].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[50].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[50].IsNullable);

					AssertEquals("FH_GB_EffectiveBranch", subscriber.ColumnInfos[51].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[51].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[51].IsNullable);

					AssertEquals("FH_GE_EffectiveDepartment", subscriber.ColumnInfos[52].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[52].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[52].IsNullable);

					AssertEquals("FH_BMT_BufferTimespan", subscriber.ColumnInfos[53].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[53].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[53].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames
		{
			get
			{
				yield return ProcessHeaderSchema.FH_MilestoneCompletionPivotKey.Name;
			}
		}
	}
}
