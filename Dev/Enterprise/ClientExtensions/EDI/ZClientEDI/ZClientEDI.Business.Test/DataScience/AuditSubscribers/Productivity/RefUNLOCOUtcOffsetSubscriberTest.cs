using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity.Tests
{
	[TestedType(typeof(RefUNLOCOUtcOffsetSubscriber))]
	class RefUNLOCOUtcOffsetSubscriberTest : DataScienceAuditSubscriberTestBase<RefUNLOCOUtcOffsetSubscriber>
	{
		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new RefUNLOCOUtcOffsetSubscriber();

			// Assert
			AssertEquals("DRU", subscriber.Code);
			AssertEquals(true, subscriber.NotifyInsert);
			AssertEquals(true, subscriber.NotifyUpdate);
			AssertEquals(true, subscriber.NotifyDelete);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new RefUNLOCOUtcOffsetSubscriber();
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
The schema of the table RefUNLOCOUtcOffset required by RefUNLOCOUtcOffsetSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(9, subscriber.ColumnInfos.Count);

					AssertEquals("RLO_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("RLO_EndTimeUtc", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("RLO_OffsetMinutesFromUtc", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("smallint", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("RLO_RL_NKCode", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("varchar(5)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("RLO_StartTimeUtc", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("RLO_SystemCreateTimeUtc", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("RLO_SystemCreateUser", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("RLO_SystemLastEditTimeUtc", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("RLO_SystemLastEditUser", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
