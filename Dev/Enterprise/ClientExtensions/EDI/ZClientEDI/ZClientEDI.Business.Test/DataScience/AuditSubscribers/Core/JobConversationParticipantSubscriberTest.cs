using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(JobConversationParticipantSubscriber))]
	class JobConversationParticipantSubscriberTest : DataScienceAuditSubscriberTestBase<JobConversationParticipantSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new JobConversationParticipantSubscriber();
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
The schema of the table JobConversationParticipant required by JobConversationParticipantSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
			() =>
			{
				AssertEquals(11, subscriber.ColumnInfos.Count);

				AssertEquals("JCP_PK", subscriber.ColumnInfos[0].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

				AssertEquals("JCP_EmailAddress", subscriber.ColumnInfos[1].ColumnName);
				AssertEquals("nvarchar(254)", subscriber.ColumnInfos[1].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

				AssertEquals("JCP_IsSubscribed", subscriber.ColumnInfos[2].ColumnName);
				AssertEquals("bit", subscriber.ColumnInfos[2].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

				AssertEquals("JCP_JCC_Conversation", subscriber.ColumnInfos[3].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[3].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

				AssertEquals("JCP_ParticipantID", subscriber.ColumnInfos[4].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[4].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[4].IsNullable);

				AssertEquals("JCP_ParticipantTableCode", subscriber.ColumnInfos[5].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[5].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

				AssertEquals("JCP_Relation", subscriber.ColumnInfos[6].ColumnName);
				AssertEquals("nvarchar(30)", subscriber.ColumnInfos[6].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

				AssertEquals("JCP_SystemCreateTimeUtc", subscriber.ColumnInfos[7].ColumnName);
				AssertEquals("smalldatetime", subscriber.ColumnInfos[7].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

				AssertEquals("JCP_SystemCreateUser", subscriber.ColumnInfos[8].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

				AssertEquals("JCP_SystemLastEditTimeUtc", subscriber.ColumnInfos[9].ColumnName);
				AssertEquals("smalldatetime", subscriber.ColumnInfos[9].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[9].IsNullable);

				AssertEquals("JCP_SystemLastEditUser", subscriber.ColumnInfos[10].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);
			});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
