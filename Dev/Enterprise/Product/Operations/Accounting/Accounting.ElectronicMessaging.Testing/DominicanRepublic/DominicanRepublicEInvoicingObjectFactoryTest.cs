using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.CountryFactory;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.DominicanRepublic.Testing
{
	[TestedType(typeof(DominicanRepublicEInvoicingObjectFactory))]
	class DominicanRepublicEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new DominicanRepublicEInvoicingObjectFactory();

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction => IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString MessageTypeAssignedInCountryObjectFactory => DominicanRepublicEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override Type GetExpectedAdditionalDataItemsProviderType() => typeof(DominicanRepublicEInvoicingAdditionalDataItemsProvider);
	}
}
