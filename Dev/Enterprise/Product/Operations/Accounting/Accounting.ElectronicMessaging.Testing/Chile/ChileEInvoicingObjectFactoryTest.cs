using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Chile.Testing
{
	[TestedType(typeof(ChileEInvoicingObjectFactory))]
	class ChileEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new ChileEInvoicingObjectFactory();

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction => IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString MessageTypeAssignedInCountryObjectFactory => ChileEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting => new PopulateOptionalXUTFieldsSetting(populateAttachedDocuments: false, populateShipments: true, populateAuthorizationDetails: true);

		protected override Type GetExpectedAdditionalDataItemsProviderType() => typeof(ChileEInvoicingAdditionalDataItemsProvider);

		protected override Type GetExpectedBatchCreatorType() => typeof(EInvoicingBatchCreatorForChile);

		protected override bool? ExpectedSupportsWaitForOriginalTransactionForAmending => true;
	}
}
