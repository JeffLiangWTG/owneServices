using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class SurinameTaxIDMacroDataTest : TestCaseWithFactory
	{
		public void TestITaxIDMacroDataProviderIsImplemented()
		{
			var taxIDMacroDataProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Suriname) as IInstanceProvider<ITaxIDMacroDataProvider>;
			AssertNotNull("When Suriname countryfactory implements IInstanceProvider<ITaxIDMacroDataProvider>", taxIDMacroDataProvider);
			AssertNotNull("SurinameTaxIDMacroData", taxIDMacroDataProvider.Get());
		}

		public void TestGetTaxIDMacroData()
		{
			var taxIDMacroDataObject = TaxIDMacroDataProvider.Get().GetTaxIDMacroData();
			AssertEquals("OrgTaxRegistrationPrefix", "FIN #: ", taxIDMacroDataObject.OrgTaxRegistrationPrefix);
			AssertEquals("OrgTaxRegistrationCode", "BTW", taxIDMacroDataObject.OrgTaxRegistrationCode);
		}

		IInstanceProvider<ITaxIDMacroDataProvider> TaxIDMacroDataProvider => taxIDMacroDataProvider ??
			(taxIDMacroDataProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Suriname) as IInstanceProvider<ITaxIDMacroDataProvider>);
		IInstanceProvider<ITaxIDMacroDataProvider> taxIDMacroDataProvider;
	}
}
