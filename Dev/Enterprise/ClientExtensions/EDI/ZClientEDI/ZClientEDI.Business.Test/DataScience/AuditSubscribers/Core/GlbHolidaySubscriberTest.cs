using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(GlbHolidaySubscriber))]
	class GlbHolidaySubscriberTest : DataScienceAuditSubscriberTestBase<GlbHolidaySubscriber>
	{
		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new GlbHolidaySubscriber();

			// Assert
			AssertEquals("DGH", subscriber.Code);
			AssertEquals(true, subscriber.NotifyInsert);
			AssertEquals(true, subscriber.NotifyUpdate);
			AssertEquals(true, subscriber.NotifyDelete);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbHolidaySubscriber();
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
The schema of the table GlbHoliday required by GlbHolidaySubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(16, subscriber.ColumnInfos.Count);

					AssertEquals("GH_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("GH_IsValid", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("GH_IsWorkingDay", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("GH_RecurrDay", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("GH_RecurrMonth", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("GH_Date", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("GH_HolidayName", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(35)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("GH_IsActive", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("GH_ParentID", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("GH_ParentTableCode", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("GH_Recurring", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("GH_RecurrType", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("GH_SystemCreateTimeUtc", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("GH_SystemCreateUser", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("GH_SystemLastEditTimeUtc", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("GH_SystemLastEditUser", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
