using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(GlbSecuritySubscriber))]
	class GlbSecuritySubscriberTest : DataScienceAuditSubscriberTestBase<GlbSecuritySubscriber>
	{
		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new GlbSecuritySubscriber();

			// Assert
			AssertEquals("DGU", subscriber.Code);
			AssertEquals(true, subscriber.NotifyInsert);
			AssertEquals(true, subscriber.NotifyUpdate);
			AssertEquals(true, subscriber.NotifyDelete);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbSecuritySubscriber();
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
The schema of the table GlbSecurity required by GlbSecuritySubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(14, subscriber.ColumnInfos.Count);

					AssertEquals("GU_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("GU_GB", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("GU_GC", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("GU_GE", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("GU_GG", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("GU_GS", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("GU_IsValid", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("GU_ItemGUID", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("GU_SecurityItemIsAllowed", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("GU_SecurityRight", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("varchar(320)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("GU_SystemCreateTimeUtc", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("GU_SystemCreateUser", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("GU_SystemLastEditTimeUtc", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("GU_SystemLastEditUser", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
