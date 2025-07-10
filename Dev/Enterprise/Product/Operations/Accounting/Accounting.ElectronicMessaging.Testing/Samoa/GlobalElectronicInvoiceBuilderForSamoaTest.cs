using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.TaxCore.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Samoa.Testing
{
	public class GlobalElectronicInvoiceBuilderForSamoaTest : GlobalElectronicInvoiceBuilderForTaxCoreTest
	{
		protected override ZString CountryCode => CountryCodes.WesternSamoa;

		protected override ZString InvoiceSystemName => "Samoa electronic invoicing system";
	}
}
