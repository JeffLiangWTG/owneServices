using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity.Tests
{
	[TestedType(typeof(BMComponentSubscriber))]
	class BMComponentSubscriberTest : DataScienceAuditSubscriberTestBase<BMComponentSubscriber>
	{
		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new BMComponentSubscriber();

			// Assert
			AssertEquals("DBC", subscriber.Code);
			AssertEquals(true, subscriber.NotifyInsert);
			AssertEquals(true, subscriber.NotifyUpdate);
			AssertEquals(true, subscriber.NotifyDelete);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new BMComponentSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			//var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(1, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table BMComponent required by BMComponentSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(19, subscriber.ColumnInfos.Count);

					AssertEquals("FC_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("FC_AutoAssignTasksAge", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("FC_BufferLoadLimitPercent", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("tinyint", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("FC_BufferTimeCapacityConstraintThresholdMultiple", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("decimal(3,1)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("FC_BufferTimespanInMinutes", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("FC_DisplaySequence", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("FC_IsActive", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("FC_NonCCRTemporaryOverloadLimitMultiplier", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("decimal(3,1)", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("FC_OffsetInMinutes", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("FC_SystemCreateUser", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("FC_Type", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("FC_FC_ParentComponent", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("FC_FS_System", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("FC_GB_AgingBranch", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("FC_GE_AgingDepartment", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("FC_Name", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("varchar(100)", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("FC_SystemCreateTimeUtc", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("FC_SystemLastEditTimeUtc", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("FC_SystemLastEditUser", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[18].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
