using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing.Tests
{
	[TestedType(typeof(ClientInvoiceDeliverySubscriber))]
	class ClientInvoiceDeliverySubscriberTest : DataScienceAuditSubscriberTestBase<ClientInvoiceDeliverySubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new ClientInvoiceDeliverySubscriber();
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
The schema of the table ClientInvoiceDelivery required by ClientInvoiceDeliverySubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(13, subscriber.ColumnInfos.Count);

					AssertEquals("L9_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("L9_LC", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("L9_ServerCode", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("L9_SystemCode", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("L9_OH_InvoiceTo", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("L9_IsBilled", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("char(1)", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("L9_Note", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(200)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("L9_GroupBy", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("L9_GB_InvoicingBranch", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("L9_AT_TaxId", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("L9_RX_NKInvoiceCurrency", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("L9_AC_SalesTaxChargeCode", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("L9_UseParentPrices", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();
	}
}
