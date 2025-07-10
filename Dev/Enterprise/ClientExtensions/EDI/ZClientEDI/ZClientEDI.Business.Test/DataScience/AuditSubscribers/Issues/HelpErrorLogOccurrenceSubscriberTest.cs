using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Issues.Tests
{
	[TestedType(typeof(HelpErrorLogOccurrenceSubscriber))]
	class HelpErrorLogOccurrenceSubscriberTest : DataScienceAuditSubscriberTestBase<HelpErrorLogOccurrenceSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new HelpErrorLogOccurrenceSubscriber();
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
The schema of the table HelpErrorLogOccurrence required by HelpErrorLogOccurrenceSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(13, subscriber.ColumnInfos.Count);

					AssertEquals("HO_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("HO_Company", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("varchar(50)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("HO_ExceptionDateTime", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("HO_ExceptionID", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("varchar(19)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("HO_EXEDateTime", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("HO_HE", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("HO_HL", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("HO_LCC", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("HO_LD", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("HO_Sequence", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("HO_ServerName", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varchar(64)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("HO_SessionID", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("HO_VersionNumber", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("varchar(32)", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
