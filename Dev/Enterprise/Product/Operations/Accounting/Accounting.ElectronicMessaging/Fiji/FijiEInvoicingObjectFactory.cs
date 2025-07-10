using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.TaxCore;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Fiji
{
	public class FijiEInvoicingObjectFactory : TaxCoreCountryEInvoicingObjectFactory
	{
		protected override ZString InvoicingSystemName => (NoResString)"Fiji electronic invoicing system"; // E-Invoicing System name for internal use

		protected override ZString CountryCode => CountryCodes.Fiji;
	}
}
