using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity.Tests
{
	[TestedType(typeof(ProcessHeaderLinkSubscriber))]
	class ProcessHeaderLinkSubscriberTest : DataScienceAuditSubscriberTestBase<ProcessHeaderLinkSubscriber>
	{
		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new ProcessHeaderLinkSubscriber();

			// Assert
			AssertEquals("DPL", subscriber.Code);
			AssertEquals(true, subscriber.NotifyInsert);
			AssertEquals(true, subscriber.NotifyUpdate);
			AssertEquals(true, subscriber.NotifyDelete);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new ProcessHeaderLinkSubscriber();
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
The schema of the table ProcessHeaderLink required by ProcessHeaderLinkSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(13, subscriber.ColumnInfos.Count);

					AssertEquals("FP_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("FP_FH_HeaderFrom", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("FP_FH_HeaderTo", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("FP_IsActive", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("FP_LinkType", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("FP_SynchroniseBufferPenetration", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("FP_TimeDelayFactor", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("decimal(2,1)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("FP_TimeDelayMinutes", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("FP_SystemCreateTimeUtc", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("FP_SystemCreateUser", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("FP_SystemLastEditTimeUtc", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("FP_SystemLastEditUser", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("FP_IsValid", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
