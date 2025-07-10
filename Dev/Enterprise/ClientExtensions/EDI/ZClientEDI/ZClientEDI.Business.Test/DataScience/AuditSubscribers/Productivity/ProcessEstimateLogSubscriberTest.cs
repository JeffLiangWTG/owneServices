using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity.Tests
{
	[TestedType(typeof(ProcessEstimateLogSubscriber))]
	class ProcessEstimateLogSubscriberTest : DataScienceAuditSubscriberTestBase<ProcessEstimateLogSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new GlbGroupSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new ProcessEstimateLogSubscriber();

			// Assert
			AssertEquals("DPE", subscriber.Code);
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
			AssertEquals(1, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table ProcessEstimateLog required by ProcessEstimateLogSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(15, subscriber.ColumnInfos.Count);

					AssertEquals("P9E_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("P9E_GS_NKUser", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("P9E_HasWorkStarted", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("P9E_LogDateTime", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("datetimeoffset(7)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("P9E_NewHighEstimateMinutes", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("P9E_NewLowEstimateMinutes", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("P9E_ParentId", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("P9E_ParentTableCode", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("P9E_PreviousHighEstimateMinutes", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("P9E_PreviousLowEstimateMinutes", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("P9E_SystemCreateTimeUtc", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("P9E_SystemCreateUser", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("P9E_SystemLastEditTimeUtc", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("P9E_SystemLastEditUser", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("P9E_WasWorkPreviouslyStarted", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[14].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
