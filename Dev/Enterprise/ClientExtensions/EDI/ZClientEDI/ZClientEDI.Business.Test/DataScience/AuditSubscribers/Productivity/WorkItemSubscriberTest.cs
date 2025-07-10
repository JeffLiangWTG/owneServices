using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity.Tests
{
	[TestedType(typeof(WorkItemSubscriber))]
	class WorkItemSubscriberTest : DataScienceAuditSubscriberTestBase<WorkItemSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new WorkItemSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new WorkItemSubscriber();

			// Assert
			AssertEquals("DWI", subscriber.Code);
			AssertEquals(true, subscriber.NotifyInsert);
			AssertEquals(true, subscriber.NotifyUpdate);
			AssertEquals(true, subscriber.NotifyDelete);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			// var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(4, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table WorkItem required by WorkItemSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(23, subscriber.ColumnInfos.Count);

					AssertEquals("WKI_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("WKI_ActivitySubtype", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("WKI_ActivityType", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("WKI_DateOfChange", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("WKI_GB_AssignedBranch", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("WKI_GC_AssignedCompany", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("WKI_GE_AssignedDepartment", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("WKI_P9_DefectCausedByTask", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("WKI_P9_DefectFirstMissedInTask", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("WKI_PortOrCountry", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("varchar(5)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("WKI_Priority", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("WKI_Risk", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("WKI_Status", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("WKI_Summary", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("nvarchar(80)", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("WKI_SystemCreateBranch", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("WKI_SystemCreateDepartment", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("WKI_SystemCreateTimeUtc", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("WKI_SystemCreateUser", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("WKI_SystemLastEditTimeUtc", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[18].IsNullable);

					AssertEquals("WKI_SystemLastEditUser", subscriber.ColumnInfos[19].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[19].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[19].IsNullable);

					AssertEquals("WKI_WorkItemArea", subscriber.ColumnInfos[20].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[20].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[20].IsNullable);

					AssertEquals("WKI_WorkItemNumber", subscriber.ColumnInfos[21].ColumnName);
					AssertEquals("varchar(20)", subscriber.ColumnInfos[21].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[21].IsNullable);

					AssertEquals("WKI_WorkItemType", subscriber.ColumnInfos[22].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[22].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[22].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
