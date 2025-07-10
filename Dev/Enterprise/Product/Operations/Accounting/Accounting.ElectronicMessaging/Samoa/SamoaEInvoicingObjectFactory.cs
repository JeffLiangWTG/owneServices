using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.TaxCore;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Samoa
{
	public class SamoaEInvoicingObjectFactory : TaxCoreCountryEInvoicingObjectFactory
	{
		protected override ZString InvoicingSystemName => (NoResString)"Samoa electronic invoicing system"; // E-Invoicing System Name for internal use

		protected override ZString CountryCode => CountryCodes.WesternSamoa;
	}
}
