using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Colombia.Testing
{
	[TestedType(typeof(ColombiaEInvoicingObjectFactory))]
	class ColombiaEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new ColombiaEInvoicingObjectFactory();

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction => IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString MessageTypeAssignedInCountryObjectFactory => ColombiaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting => new PopulateOptionalXUTFieldsSetting(populateAttachedDocuments: false, populateShipments: true, populateAuthorizationDetails: true);

		protected override Type GetExpectedAdditionalDataItemsProviderType() => typeof(ColombiaEInvoicingAdditionalDataItemsProvider);
	}
}
