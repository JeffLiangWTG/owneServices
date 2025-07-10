using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.TaxCore;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Fiji.Testing
{
	public class FijiEInvoicingObjectFactoryTest : TestCaseWithFactory
	{
		public void TestFactoryObjectTypes()
		{
			var countryFactory = TaxCoreEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCodes.Fiji);
			AssertEquals("Country", CountryCodes.Fiji, countryFactory.CountryCode);
			AssertEquals("InvoicingSystemName", "Fiji electronic invoicing system", countryFactory.InvoicingSystemName);
			AssertEquals("TaxFileCode", OrgCusCode.CodeTypes.TaxFileCode, countryFactory.TaxFileCode);
			AssertEquals("TFNCode", CertificateTypePairList.Codes.TFN, countryFactory.TFNCode);
			AssertType<TaxCoreEInvoiceCreator>(countryFactory.GetInvoiceCreator());
		}
	}
}
