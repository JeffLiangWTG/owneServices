using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.TaxCore;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Fiji.Testing
{
	public class FijiEInvoicingResponseObjectFactory : TestCaseWithFactory
	{
		public void TestFactoryResponseObjectTypes()
		{
			var countryFactory = TaxCoreEInvoicingResponseObjectFactory.GetICountryEInvoicingResponseObjectFactory(CountryCodes.Fiji);
			AssertEquals("Country", "Fiji", countryFactory.CountryName);
			AssertEquals("InvoicingSystemName", "Fiji Invoice Response", countryFactory.ResponseMessageSubTypeCode);
			AssertEquals("InvoicingSystemName", "ResponseMessage", countryFactory.ResponseMessageContextTypeCode);
			AssertType<FijiEInvoiceAuthorisationRecordCreator>(countryFactory.GetAuthorisationRecordCreator());
			AssertType<TaxCoreEInvoiceChecker>(countryFactory.GetEInvoiceChecker());
			AssertType<TaxCoreEInvoiceResponseReader>(countryFactory.GetEInvoiceResponseReader());
		}
	}
}
