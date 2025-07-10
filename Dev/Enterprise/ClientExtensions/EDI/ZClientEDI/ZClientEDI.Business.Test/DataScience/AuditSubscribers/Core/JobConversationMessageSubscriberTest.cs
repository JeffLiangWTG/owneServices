using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(JobConversationMessageSubscriber))]
	class JobConversationMessageSubscriberTest : DataScienceAuditSubscriberTestBase<JobConversationMessageSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new JobConversationMessageSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			//var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(1, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
			@"
The schema of the table JobConversationMessage required by JobConversationMessageSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
			() =>
			{
				AssertEquals(15, subscriber.ColumnInfos.Count);

				AssertEquals("JCM_PK", subscriber.ColumnInfos[0].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

				AssertEquals("JCM_Body", subscriber.ColumnInfos[1].ColumnName);
				AssertEquals("nvarchar(max)", subscriber.ColumnInfos[1].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

				AssertEquals("JCM_IsBroadcast", subscriber.ColumnInfos[2].ColumnName);
				AssertEquals("bit", subscriber.ColumnInfos[2].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

				AssertEquals("JCM_IsInternal", subscriber.ColumnInfos[3].ColumnName);
				AssertEquals("bit", subscriber.ColumnInfos[3].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

				AssertEquals("JCM_IsLocal", subscriber.ColumnInfos[4].ColumnName);
				AssertEquals("bit", subscriber.ColumnInfos[4].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

				AssertEquals("JCM_IsSystem", subscriber.ColumnInfos[5].ColumnName);
				AssertEquals("bit", subscriber.ColumnInfos[5].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

				AssertEquals("JCM_JCC_Conversation", subscriber.ColumnInfos[6].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[6].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

				AssertEquals("JCM_JCP_Participant", subscriber.ColumnInfos[7].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[7].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

				AssertEquals("JCM_Language", subscriber.ColumnInfos[8].ColumnName);
				AssertEquals("varchar(7)", subscriber.ColumnInfos[8].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

				AssertEquals("JCM_PostedTimeUtc", subscriber.ColumnInfos[9].ColumnName);
				AssertEquals("datetime", subscriber.ColumnInfos[9].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

				AssertEquals("JCM_Score", subscriber.ColumnInfos[10].ColumnName);
				AssertEquals("smallint", subscriber.ColumnInfos[10].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

				AssertEquals("JCM_SystemCreateTimeUtc", subscriber.ColumnInfos[11].ColumnName);
				AssertEquals("smalldatetime", subscriber.ColumnInfos[11].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

				AssertEquals("JCM_SystemCreateUser", subscriber.ColumnInfos[12].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[12].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

				AssertEquals("JCM_SystemLastEditTimeUtc", subscriber.ColumnInfos[13].ColumnName);
				AssertEquals("smalldatetime", subscriber.ColumnInfos[13].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

				AssertEquals("JCM_SystemLastEditUser", subscriber.ColumnInfos[14].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[14].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[14].IsNullable);
			});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
