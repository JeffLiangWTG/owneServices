using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class GambiaTaxIDMacroDataTest : TestCaseWithFactory
	{
		public void TestITaxIDMacroDataProviderIsImplemented()
		{
			var taxIDMacroDataProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Gambia) as IInstanceProvider<ITaxIDMacroDataProvider>;
			AssertNotNull("When Gambia countryfactory implements IInstanceProvider<ITaxIDMacroDataProvider>", taxIDMacroDataProvider);
			AssertNotNull("GambiaTaxIDMacroData", taxIDMacroDataProvider.Get());
		}

		public void TestGetTaxIDMacroData()
		{
			var taxIDMacroDataObject = TaxIDMacroDataProvider.Get().GetTaxIDMacroData();
			AssertEquals("OrgTaxRegistrationPrefix", "VAT #: ", taxIDMacroDataObject.OrgTaxRegistrationPrefix);
			AssertEquals("OrgTaxRegistrationCode", "VAT", taxIDMacroDataObject.OrgTaxRegistrationCode);
		}

		IInstanceProvider<ITaxIDMacroDataProvider> TaxIDMacroDataProvider => taxIDMacroDataProvider ??
			(taxIDMacroDataProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Gambia) as IInstanceProvider<ITaxIDMacroDataProvider>);
		IInstanceProvider<ITaxIDMacroDataProvider> taxIDMacroDataProvider;
	}
}
