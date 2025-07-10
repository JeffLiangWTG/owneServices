using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents.Tests
{
	[TestedType(typeof(IncidentManagementGroupMessageSubscriber))]
	class IncidentManagementGroupMessageSubscriberTest : DataScienceAuditSubscriberTestBase<IncidentManagementGroupMessageSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new IncidentManagementGroupMessageSubscriber();
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
			The schema of the table IncidentMain required by IncidentManagementGroupMessageSubscriber has changed.
			The Data Science team will need to adjust their data pipelines before this change can be committed.
			Please contact the Data Science team.",
				() =>
				{
					AssertEquals(10, subscriber.ColumnInfos.Count);

					AssertEquals("IGM_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("IGM_ING_Group", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("IGM_IsPublished", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("IGM_Type", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("IGM_Message", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("nvarchar(max)", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("IGM_SystemCreateTimeUtc", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("IGM_SystemCreateUser", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("IGM_SystemLastEditTimeUtc", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("IGM_SystemLastEditUser", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("IGM_BroadcastDateUtc", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[9].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
