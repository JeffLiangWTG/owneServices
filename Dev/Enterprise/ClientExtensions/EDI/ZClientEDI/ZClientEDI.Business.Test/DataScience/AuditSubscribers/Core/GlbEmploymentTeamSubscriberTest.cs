using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(GlbEmploymentTeamSubscriber))]
	class GlbEmploymentTeamSubscriberTest : DataScienceAuditSubscriberTestBase<GlbEmploymentTeamSubscriber>
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
The schema of the table GlbEmploymentTeam required by GlbEmploymentTeamSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(11, subscriber.ColumnInfos.Count);

					AssertEquals("GET_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("GET_AutoEffectiveEndDate", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("datetimeoffset(0)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("GET_EffectiveDate", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("datetimeoffset(0)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("GET_GCR_ChangeRequest", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("GET_GST_NKTeamCode", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("GET_GS_Staff", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("GET_IsApproved", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("GET_SystemCreateTimeUtc", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("GET_SystemCreateUser", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("GET_SystemLastEditTimeUtc", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("GET_SystemLastEditUser", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);
				}
			);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbEmploymentTeamSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
