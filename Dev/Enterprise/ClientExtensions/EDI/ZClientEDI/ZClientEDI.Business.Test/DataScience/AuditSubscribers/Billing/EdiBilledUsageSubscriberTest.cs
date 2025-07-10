using System.Collections.Generic;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing.Tests
{
	[TestedType(typeof(EdiBilledUsageSubscriber))]
	class EdiBilledUsageSubscriberTest : DataScienceAuditSubscriberTestBase<EdiBilledUsageSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new EdiBilledUsageSubscriber();
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
The schema of the table EdiBilledUsage required by EdiBilledUsageSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(13, subscriber.ColumnInfos.Count);

					AssertEquals("BU9_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("BU9_LD", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("BU9_PeriodStart", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("date", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("BU9_UsageCode", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("BU9_UsageSubCode", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("varchar(50)", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("BU9_UnitCount", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("decimal(14,4)", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("BU9_L7", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("BU9_PriceCode", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("BU9_LocalAmountPostDiscount", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("money", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("BU9_LCC", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("BU9_LC", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("BU9_AH_Invoice", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("BU9_BillingModel", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames
		{
			get
			{
				yield return "BU9_UnitPrice";
				yield return "BU9_TransactionAmountPostDiscount";
			}
		}
	}
}
