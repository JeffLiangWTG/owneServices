using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Panama.Testing
{
	[TestedType(typeof(PanamaEInvoicingObjectFactory))]
	class PanamaEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new PanamaEInvoicingObjectFactory();

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction => IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString MessageTypeAssignedInCountryObjectFactory => EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override Type GetExpectedBatchCreatorType() => typeof(EInvoicingBatchCreatorForPanama);

		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting => new PopulateOptionalXUTFieldsSetting(populateAttachedDocuments: false, populateShipments: true, populateAuthorizationDetails: true);

		protected override bool? ExpectedSupportsWaitForOriginalTransactionForAmending => true;

		protected override Type GetExpectedAdditionalDataItemsProviderType() => typeof(PanamaEInvoicingAdditionalDataItemsProvider);
	}
}
