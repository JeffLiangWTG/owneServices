using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(GlbWorkTimeSubscriber))]
	class GlbWorkTimeSubscriberTest : DataScienceAuditSubscriberTestBase<GlbWorkTimeSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new GlbWorkTimeSubscriber();
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
			The schema of the table GlbWorkTime required by GlbWorkTimeSubscriber has changed.
			The Data Science team will need to adjust their data pipelines before this change can be committed.
			Please contact the Data Science team.",
				() =>
				{
					AssertEquals(11, subscriber.ColumnInfos.Count);

					AssertEquals("GW_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("GW_IsValid", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("GW_DayOfWeek", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("GW_StartTime", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("GW_EndTime", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("GW_ParentTableCode", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("GW_ParentID", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("GW_SystemCreateTimeUtc", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("GW_SystemCreateUser", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("GW_SystemLastEditTimeUtc", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("GW_SystemLastEditUser", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
