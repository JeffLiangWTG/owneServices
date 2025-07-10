using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing.Tests
{
	[TestedType(typeof(ClientLicencePriceItemSubscriber))]
	class ClientLicencePriceItemSubscriberTest : DataScienceAuditSubscriberTestBase<ClientLicencePriceItemSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new ClientLicencePriceItemSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			// var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(3, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table ClientLicencePriceItem required by ClientLicencePriceItemSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(28, subscriber.ColumnInfos.Count);

					AssertEquals("L7_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("L7_L6", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("L7_Code", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("L7_Order", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("smallint", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("L7_FeeType", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("L7_Price", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("decimal(18,6)", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("L7_ParentCode", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("L7_WebParentCode", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("L7_LicenceUnits", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("decimal(18,6)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("L7_UnitBreak", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("int", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("L7_RX_NKCurrency", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("L7_Ref4", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("varchar(50)", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("L7_Description", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("nvarchar(250)", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("L7_PGM_DiscountGroupCode", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("varchar(20)", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("L7_ChargeCode", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("varchar(10)", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("L7_DepositChargeCode", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("varchar(10)", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("L7_ChargeBasis", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("nvarchar(100)", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("L7_UnitBreakParentCode", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("L7_Language", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("varchar(7)", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[18].IsNullable);

					AssertEquals("L7_ExchangeRateGroupCode", subscriber.ColumnInfos[19].ColumnName);
					AssertEquals("varchar(5)", subscriber.ColumnInfos[19].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[19].IsNullable);

					AssertEquals("L7_IsVolumeAdjustmentEligible", subscriber.ColumnInfos[20].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[20].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[20].IsNullable);

					AssertEquals("L7_Category", subscriber.ColumnInfos[21].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[21].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[21].IsNullable);

					AssertEquals("L7_ParentCategory", subscriber.ColumnInfos[22].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[22].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[22].IsNullable);

					AssertEquals("L7_ProductAvailability", subscriber.ColumnInfos[23].ColumnName);
					AssertEquals("varchar(1)", subscriber.ColumnInfos[23].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[23].IsNullable);

					AssertEquals("L7_ProductDisplayCategory", subscriber.ColumnInfos[24].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[24].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[24].IsNullable);

					AssertEquals("L7_CountryTierCode", subscriber.ColumnInfos[25].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[25].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[25].IsNullable);

					AssertEquals("L7_DisbursementDirection", subscriber.ColumnInfos[26].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[26].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[26].IsNullable);

					AssertEquals("L7_RN_NKDisbursementCountry", subscriber.ColumnInfos[27].ColumnName);
					AssertEquals("varchar(2)", subscriber.ColumnInfos[27].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[27].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
