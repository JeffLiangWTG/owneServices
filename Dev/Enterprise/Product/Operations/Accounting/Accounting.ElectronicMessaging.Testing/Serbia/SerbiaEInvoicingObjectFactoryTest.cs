using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Serbia.Testing
{
	[TestedType(typeof(SerbiaEInvoicingObjectFactory))]
	sealed class SerbiaEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction => IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override CountryEInvoicingObjectFactory GetTestCountryFactory()
			=> (CountryEInvoicingObjectFactory)GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCodes.Serbia);

		protected override ZString MessageTypeAssignedInCountryObjectFactory => SerbiaEInvoiceAPICommandList.Codes.GenerateInvoiceSubmission;
	}
}
