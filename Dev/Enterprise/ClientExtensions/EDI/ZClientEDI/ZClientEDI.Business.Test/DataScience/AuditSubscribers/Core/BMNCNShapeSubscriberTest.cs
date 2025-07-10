using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(BMNCNShapeSubscriber))]
	class BMNCNShapeSubscriberTest : DataScienceAuditSubscriberTestBase<BMNCNShapeSubscriber>
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
The schema of the table BMNCNShape required by BMNCNShapeSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(16, subscriber.ColumnInfos.Count);

					AssertEquals("BNS_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("BNS_BNS_ParentShape", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("BNS_BNS_RootShape", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("BNS_BNT_Style", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("BNS_GS_NKApprovedBy", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("BNS_IsValid", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("BNS_JobType", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("BNS_Name", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("nvarchar(256)", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("BNS_RelatedEntityID", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("BNS_RelatedEntityTableCode", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("BNS_ShapeType", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("BNS_Status", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("BNS_SystemCreateTimeUtc", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("BNS_SystemCreateUser", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("BNS_SystemLastEditTimeUtc", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("BNS_SystemLastEditUser", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);
				}
			);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new BMNCNShapeSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
