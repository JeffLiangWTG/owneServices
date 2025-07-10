using System.Collections.Generic;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(GlbStaffHolidaySubscriber))]
	class GlbStaffHolidaySubscriberTest : DataScienceAuditSubscriberTestBase<GlbStaffHolidaySubscriber>
	{
		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new GlbStaffHolidaySubscriber();

			// Assert
			AssertEquals("DGO", subscriber.Code);
			AssertEquals(true, subscriber.NotifyInsert);
			AssertEquals(true, subscriber.NotifyUpdate);
			AssertEquals(true, subscriber.NotifyDelete);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbStaffHolidaySubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			//var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(2, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table GlbStaffHoliday required by GlbStaffHolidaySubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(20, subscriber.ColumnInfos.Count);

					AssertEquals("GA_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("GA_ApprovalStatus", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("GA_AvailabilityPercentage", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("tinyint", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("GA_DaysLeaveTaken", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("decimal(8,2)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("GA_EndTime", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("GA_IsValid", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("GA_IsWorkingAway", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("GA_OverrideLeaveTaken", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("GA_ParentTableCode", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("GA_WorkHolidayType", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("GA_GS", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("GA_ParentID", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("GA_RecordType", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("GA_StartTime", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("GA_SystemCreateTimeUtc", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("GA_SystemCreateUser", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("GA_SystemCreateBranch", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("GA_SystemCreateDepartment", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("GA_SystemLastEditTimeUtc", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[18].IsNullable);

					AssertEquals("GA_SystemLastEditUser", subscriber.ColumnInfos[19].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[19].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[19].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames
		{
			get
			{
				yield return "GA_LeaveComment";
			}
		}
	}
}
