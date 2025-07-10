using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity.Tests
{
	[TestedType(typeof(GlbGroupSubscriber))]
	class GlbGroupSubscriberTest : DataScienceAuditSubscriberTestBase<GlbGroupSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new GlbGroupSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestAuditSubscriberParameters()
		{
			// Arrange / Act
			var subscriber = new GlbGroupSubscriber();

			// Assert
			AssertEquals("DGG", subscriber.Code);
			AssertEquals(true, subscriber.NotifyInsert);
			AssertEquals(true, subscriber.NotifyUpdate);
			AssertEquals(true, subscriber.NotifyDelete);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			//var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);
			//AssertNullOrEmpty(generatedCode);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(3, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
			@"
The schema of the table GlbGroup required by GlbGroupSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
			() =>
			{
				AssertEquals(21, subscriber.ColumnInfos.Count);

				AssertEquals("GG_PK", subscriber.ColumnInfos[0].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

				AssertEquals("GG_ActiveDirectoryObjectGuid", subscriber.ColumnInfos[1].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[1].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[1].IsNullable);

				AssertEquals("GG_Category", subscriber.ColumnInfos[2].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[2].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

				AssertEquals("GG_Code", subscriber.ColumnInfos[3].ColumnName);
				AssertEquals("varchar(15)", subscriber.ColumnInfos[3].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

				AssertEquals("GG_Desc", subscriber.ColumnInfos[4].ColumnName);
				AssertEquals("nvarchar(64)", subscriber.ColumnInfos[4].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

				AssertEquals("GG_DomainName", subscriber.ColumnInfos[5].ColumnName);
				AssertEquals("nvarchar(255)", subscriber.ColumnInfos[5].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

				AssertEquals("GG_ExternalId", subscriber.ColumnInfos[6].ColumnName);
				AssertEquals("nvarchar(254)", subscriber.ColumnInfos[6].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

				AssertEquals("GG_GC", subscriber.ColumnInfos[7].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[7].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

				AssertEquals("GG_GG_ParentGroup", subscriber.ColumnInfos[8].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[8].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[8].IsNullable);

				AssertEquals("GG_IsActive", subscriber.ColumnInfos[9].ColumnName);
				AssertEquals("bit", subscriber.ColumnInfos[9].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

				AssertEquals("GG_IsSales", subscriber.ColumnInfos[10].ColumnName);
				AssertEquals("bit", subscriber.ColumnInfos[10].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

				AssertEquals("GG_IsSecurityEnabled", subscriber.ColumnInfos[11].ColumnName);
				AssertEquals("bit", subscriber.ColumnInfos[11].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

				AssertEquals("GG_IsSystemDefined", subscriber.ColumnInfos[12].ColumnName);
				AssertEquals("bit", subscriber.ColumnInfos[12].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

				AssertEquals("GG_IsValid", subscriber.ColumnInfos[13].ColumnName);
				AssertEquals("bit", subscriber.ColumnInfos[13].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

				AssertEquals("GG_SystemCreateBranch", subscriber.ColumnInfos[14].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[14].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[14].IsNullable);

				AssertEquals("GG_SystemCreateDepartment", subscriber.ColumnInfos[15].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[15].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

				AssertEquals("GG_SystemCreateTimeUtc", subscriber.ColumnInfos[16].ColumnName);
				AssertEquals("datetime", subscriber.ColumnInfos[16].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[16].IsNullable);

				AssertEquals("GG_SystemCreateUser", subscriber.ColumnInfos[17].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[17].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[17].IsNullable);

				AssertEquals("GG_SystemLastEditTimeUtc", subscriber.ColumnInfos[18].ColumnName);
				AssertEquals("datetime", subscriber.ColumnInfos[18].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[18].IsNullable);

				AssertEquals("GG_SystemLastEditUser", subscriber.ColumnInfos[19].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[19].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[19].IsNullable);

				AssertEquals("GG_Type", subscriber.ColumnInfos[20].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[20].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[20].IsNullable);
			});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
