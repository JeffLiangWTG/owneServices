using System;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	[TestedType(typeof(UruguayEInvoicingObjectFactory))]
	class UruguayEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new UruguayEInvoicingObjectFactory();

		protected override Type GetExpectedTransactionBatchToPayloadWriterType() => typeof(CFEXmlWriter);

		protected override Type GetExpectedBatchCreatorType() => typeof(EInvoicingBatchCreatorForUruguay);

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction =>  IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override bool? ExpectedSupportsWaitForOriginalTransactionForAmending => true;

		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting => new PopulateOptionalXUTFieldsSetting(populateAttachedDocuments: false, populateShipments: true);

		protected override Type GetExpectedAdditionalDataItemsProviderType() => typeof(UruguayEInvoicingAdditionalDataItemsProvider);
	}
}
