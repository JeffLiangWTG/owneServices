using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents.Tests
{
	[TestedType(typeof(IncidentDiagnosticCriteriaPivotSubscriber))]
	class IncidentDiagnosticCriteriaPivotSubscriberTest : DataScienceAuditSubscriberTestBase<IncidentDiagnosticCriteriaPivotSubscriber>
	{
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
The schema of the table IncidentDiagnosticCriteriaPivot required by IncidentDiagnosticCriteriaPivotSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(9, subscriber.ColumnInfos.Count);

					AssertEquals("IMV_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("IMV_IMD_DiagnosticCriteria", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("IMV_ParentID", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("IMV_ParentTableCode", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("IMV_Status", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("IMV_SystemCreateTimeUtc", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("IMV_SystemCreateUser", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("IMV_SystemLastEditTimeUtc", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("IMV_SystemLastEditUser", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);
				});
		}

		public override void TestCustomFilter()
		{
			var subscriber = SubscriberUnderTest;
			AssertNull(subscriber.CustomFilter);
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
