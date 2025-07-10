using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.CostaRica.Testing
{
	[TestedType(typeof(CostaRicaEInvoicingObjectFactory))]
	class CostaRicaEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new CostaRicaEInvoicingObjectFactory();

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction => IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString MessageTypeAssignedInCountryObjectFactory => CostaRicaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
	}
}
