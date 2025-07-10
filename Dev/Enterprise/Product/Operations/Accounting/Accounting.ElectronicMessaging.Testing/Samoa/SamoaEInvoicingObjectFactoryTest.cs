using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.TaxCore;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Samoa.Testing
{
	public class SamoaEInvoicingObjectFactoryTest : TestCaseWithFactory
	{
		public void TestFactoryObjectTypes()
		{
			var countryFactory = TaxCoreEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCodes.WesternSamoa);
			AssertEquals("Country", CountryCodes.WesternSamoa, countryFactory.CountryCode);
			AssertEquals("InvoicingSystemName", "Samoa electronic invoicing system", countryFactory.InvoicingSystemName);
			AssertEquals("TaxFileCode", OrgCusCode.CodeTypes.TaxFileCode, countryFactory.TaxFileCode);
			AssertEquals("TFNCode", CertificateTypePairList.Codes.TFN, countryFactory.TFNCode);
			AssertType<TaxCoreEInvoiceCreator>(countryFactory.GetInvoiceCreator());
		}
	}
}
