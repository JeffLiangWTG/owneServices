using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(GlbEmploymentHistorySubscriber))]
	class GlbEmploymentHistorySubscriberTest : DataScienceAuditSubscriberTestBase<GlbEmploymentHistorySubscriber>
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
The schema of the table GlbEmploymentHistory required by GlbEmploymentHistorySubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(21, subscriber.ColumnInfos.Count);

					AssertEquals("GEH_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("GEH_AutoEffectiveEndDate", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("datetimeoffset(0)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("GEH_CompanyName", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("nvarchar(100)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("GEH_DepartureComments", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("nvarchar(max)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("GEH_DepartureReason", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("GEH_EffectiveDate", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("datetimeoffset(0)", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("GEH_EmploymentType", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("GEH_GCR_ChangeRequest", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("GEH_GS_Staff", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("GEH_HJ_JobRole", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("GEH_IsApproved", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("GEH_IsInternalPosition", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("GEH_IsPromotion", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("GEH_JobDescription", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("nvarchar(512)", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("GEH_JobFamily", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("GEH_JobTitle", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("nvarchar(80)", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("GEH_SystemCreateTimeUtc", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("GEH_SystemCreateUser", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("GEH_SystemLastEditTimeUtc", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[18].IsNullable);

					AssertEquals("GEH_SystemLastEditUser", subscriber.ColumnInfos[19].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[19].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[19].IsNullable);

					AssertEquals("GEH_WorksOutsideBranch", subscriber.ColumnInfos[20].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[20].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[20].IsNullable);
				}
			);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbEmploymentHistorySubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
