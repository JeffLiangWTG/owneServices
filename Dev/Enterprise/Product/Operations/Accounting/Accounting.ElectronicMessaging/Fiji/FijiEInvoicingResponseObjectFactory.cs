using Enterprise.Accounting.ElectronicMessaging.TaxCore;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.Fiji
{
	public class FijiEInvoicingResponseObjectFactory : TaxCoreCountryEInvoicingResponseObjectFactory
	{
		protected override string CountryName => (NoResString)"Fiji"; // Country name for internal use

		protected override ITaxCoreEInvoiceAuthorisationRecordCreator GetAuthorisationRecordCreator() => new FijiEInvoiceAuthorisationRecordCreator();
	}
}
