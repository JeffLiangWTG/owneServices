using Enterprise.Accounting.ElectronicMessaging.TaxCore;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Samoa
{
	public class SamoaEInvoicingResponseObjectFactory : TaxCoreCountryEInvoicingResponseObjectFactory
	{
		protected override string CountryName => (NoResString)"Samoa"; // Country Name for internal use

		protected override ITaxCoreEInvoiceAuthorisationRecordCreator GetAuthorisationRecordCreator() => new SamoaEInvoiceAuthorisationRecordCreator();
	}
}
