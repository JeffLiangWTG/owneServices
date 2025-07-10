using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity.Tests
{
	[TestedType(typeof(GlbCapabilitySubscriber))]
	class GlbCapabilitySubscriberTest : DataScienceAuditSubscriberTestBase<GlbCapabilitySubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new GlbCapabilitySubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new GlbCapabilitySubscriber();

			// Assert
			AssertEquals("DGC", subscriber.Code);
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
			AssertEquals(1, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table GlbCapability required by GlbCapabilitySubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(12, subscriber.ColumnInfos.Count);

					AssertEquals("G4_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("G4_AllowTaskAutoAssignment", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("G4_AutoAssignTasksAge", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("G4_CapacityScope", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("G4_Code", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("G4_Description", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("varchar(50)", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("G4_IsActive", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("G4_MembershipRequirements", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("varchar(512)", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("G4_SystemCreateTimeUtc", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("G4_SystemCreateUser", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("G4_SystemLastEditTimeUtc", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("G4_SystemLastEditUser", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
