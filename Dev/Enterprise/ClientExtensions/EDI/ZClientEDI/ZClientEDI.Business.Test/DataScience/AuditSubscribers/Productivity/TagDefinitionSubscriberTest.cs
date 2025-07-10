using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity.Tests
{
	[TestedType(typeof(TagDefinitionSubscriber))]
	class TagDefinitionSubscriberTest : DataScienceAuditSubscriberTestBase<TagDefinitionSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new TagDefinitionSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new TagDefinitionSubscriber();

			// Assert
			AssertEquals("DTD", subscriber.Code);
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
The schema of the table TagDefinition required by TagDefinitionSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(12, subscriber.ColumnInfos.Count);

					AssertEquals("TGD_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("TGD_Code", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("TGD_Description", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("varchar(80)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("TGD_IsActive", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("TGD_IsExclusive", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("TGD_IsSystem", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("TGD_Scope", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("TGD_SystemCreateTimeUtc", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("TGD_SystemCreateUser", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("TGD_SystemLastEditTimeUtc", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("TGD_SystemLastEditUser", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("TGD_UsageScope", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
