using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.TaxCore.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Fiji.Testing
{
	public class GlobalElectronicInvoiceBuilderForFijiTest : GlobalElectronicInvoiceBuilderForTaxCoreTest
	{
		protected override ZString CountryCode => CountryCodes.Fiji;

		protected override ZString InvoiceSystemName => "Fiji electronic invoicing system";
	}
}
