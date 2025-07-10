using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents.Tests
{
	[TestedType(typeof(IncidentMetricsSubscriber))]
	class IncidentMetricsSubscriberTest : DataScienceAuditSubscriberTestBase<IncidentMetricsSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new IncidentMetricsSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			// var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(3, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table IncidentMetrics required by IncidentMetricsSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(11, subscriber.ColumnInfos.Count);

					AssertEquals("IME_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("IME_CalculatedMetric", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("IME_EndTimeUtc", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("IME_IncidentNumber", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("varchar(20)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("IME_MetricCode", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("IME_MetricCount", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("IME_StartTimeUtc", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("IME_SystemCreateTimeUtc", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("IME_SystemCreateUser", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("IME_SystemLastEditTimeUtc", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("IME_SystemLastEditUser", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
