using System.Collections.Generic;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(LicenceEnterpriseSubscriber))]
	class LicenceEnterpriseSubscriberTest : DataScienceAuditSubscriberTestBase<LicenceEnterpriseSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new LicenceEnterpriseSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			//var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);
			//AssertNullOrEmpty(generatedCode);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(2, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
			@"
The schema of the table LicenceEnterprise required by LicenceEnterpriseSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
			() =>
			{
				AssertEquals(9, subscriber.ColumnInfos.Count);

				AssertEquals("LE_PK", subscriber.ColumnInfos[0].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

				AssertEquals("LE_EnterpriseCode", subscriber.ColumnInfos[1].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[1].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

				AssertEquals("LE_OH", subscriber.ColumnInfos[2].ColumnName);
				AssertEquals("uniqueidentifier", subscriber.ColumnInfos[2].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

				AssertEquals("LE_IsInternal", subscriber.ColumnInfos[3].ColumnName);
				AssertEquals("bit", subscriber.ColumnInfos[3].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

				AssertEquals("LE_EnterpriseID", subscriber.ColumnInfos[4].ColumnName);
				AssertEquals("varchar(12)", subscriber.ColumnInfos[4].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

				AssertEquals("LE_SystemCreateTimeUtc", subscriber.ColumnInfos[5].ColumnName);
				AssertEquals("smalldatetime", subscriber.ColumnInfos[5].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[5].IsNullable);

				AssertEquals("LE_SystemCreateUser", subscriber.ColumnInfos[6].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

				AssertEquals("LE_SystemLastEditTimeUtc", subscriber.ColumnInfos[7].ColumnName);
				AssertEquals("smalldatetime", subscriber.ColumnInfos[7].SqlType);
				AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

				AssertEquals("LE_SystemLastEditUser", subscriber.ColumnInfos[8].ColumnName);
				AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
				AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);
			});
		}

		protected override IEnumerable<string> IgnoredColumnNames
		{
			get
			{
				yield return "LE_AuthorityUri";
				yield return "LE_OidcClientID";
				yield return "LE_DomainHint";
				yield return "LE_TokenAuthenticationEnabled";
			}
		}
	}
}
