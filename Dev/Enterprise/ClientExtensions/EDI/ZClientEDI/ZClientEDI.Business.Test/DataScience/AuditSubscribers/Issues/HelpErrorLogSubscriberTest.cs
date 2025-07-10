using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Issues.Tests
{
	[TestedType(typeof(HelpErrorLogSubscriber))]
	class HelpErrorLogSubscriberTest : DataScienceAuditSubscriberTestBase<HelpErrorLogSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new HelpErrorLogSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(1, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table HelpErrorLog required by HelpErrorLogSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(21, subscriber.ColumnInfos.Count);

					AssertEquals("HE_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("HE_ExceptionMessage", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("nvarchar(512)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("HE_ExceptionSource", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("varchar(128)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("HE_ExceptionType", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("varchar(128)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("HE_FailCount", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("HE_FirstEXEVersionDate", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("HE_FirstProcessed", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("HE_FirstReported", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("HE_FirstVersionNumber", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("varchar(32)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("HE_FixedCount", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("tinyint", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("HE_FixedDate", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("HE_IsClientVisible", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("HE_IssueNumber", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("varchar(20)", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("HE_LastEXEVersionDate", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("HE_LastReported", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("HE_LastVersionNumber", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("varchar(32)", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("HE_LogType", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("HE_SystemCreateTimeUtc", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("HE_SystemCreateUser", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[18].IsNullable);

					AssertEquals("HE_SystemLastEditTimeUtc", subscriber.ColumnInfos[19].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[19].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[19].IsNullable);

					AssertEquals("HE_SystemLastEditUser", subscriber.ColumnInfos[20].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[20].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[20].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
