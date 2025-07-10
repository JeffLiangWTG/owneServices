using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.TaxCore;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Samoa.Testing
{
	public class SamoaEInvoicingResponseObjectFactory : TestCaseWithFactory
	{
		public void TestFactoryResponseObjectTypes()
		{
			var countryFactory = TaxCoreEInvoicingResponseObjectFactory.GetICountryEInvoicingResponseObjectFactory(CountryCodes.WesternSamoa);
			AssertEquals("Country", "Samoa", countryFactory.CountryName);
			AssertEquals("InvoicingSystemName", "Samoa Invoice Response", countryFactory.ResponseMessageSubTypeCode);
			AssertEquals("InvoicingSystemName", "ResponseMessage", countryFactory.ResponseMessageContextTypeCode);
			AssertType<SamoaEInvoiceAuthorisationRecordCreator>(countryFactory.GetAuthorisationRecordCreator());
			AssertType<TaxCoreEInvoiceChecker>(countryFactory.GetEInvoiceChecker());
			AssertType<TaxCoreEInvoiceResponseReader>(countryFactory.GetEInvoiceResponseReader());
		}
	}
}
