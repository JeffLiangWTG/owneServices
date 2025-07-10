using System.Collections.Generic;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing.Tests
{
	[TestedType(typeof(ClientLicencePriceHeaderSubscriber))]
	class ClientLicencePriceHeaderSubscriberTest : DataScienceAuditSubscriberTestBase<ClientLicencePriceHeaderSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new ClientLicencePriceHeaderSubscriber();
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
The schema of the table ClientLicencePriceHeader required by ClientLicencePriceHeaderSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(19, subscriber.ColumnInfos.Count);

					AssertEquals("L6_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("L6_LC", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("L6_RX_NKCurrency", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("L6_ValidFrom", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("L6_ValidTo", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("L6_SystemCreateTimeUtc", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("L6_SystemCreateUser", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("L6_LicenceEdition", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("L6_SystemCode", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("L6_RN_NKCountry", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("varchar(2)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("L6_IsStandard", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("char(1)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("L6_LicenceUnitRate", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("decimal(18,9)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("L6_DiscountCode", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("varchar(20)", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("L6_UseStandardDiscount", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("L6_LiveMonthsUntilTestDbBilling", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("L6_TestDbPriceCode", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("L6_HasExchangeRates", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("L6_Rounding", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("L6_PricelistVersion", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("varchar(50)", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[18].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames
		{
			get
			{
				yield return "L6_IsDisbursementBundle";
			}
		}
	}
}
