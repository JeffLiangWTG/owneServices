using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class CookIslandsTaxIDMacroDataTest : TestCaseWithFactory
	{
		public void TestITaxIDMacroDataProviderIsImplemented()
		{
			AssertNotNull("When CookIslands country factory implements IInstanceProvider<ITaxIDMacroDataProvider>", TaxIDMacroDataProvider);
			AssertNotNull("CookIslandsTaxIDMarcoData", TaxIDMacroDataProvider.Get());
		}

		public void TestGetTaxIDMacroData()
		{
			var taxIDMacroDataObject = TaxIDMacroDataProvider.Get().GetTaxIDMacroData();
			AssertEquals("OrgTaxRegistrationPrefix", "RMD#: ", taxIDMacroDataObject.OrgTaxRegistrationPrefix);
			AssertEquals("OrgTaxRegistrationCode", "VAT", taxIDMacroDataObject.OrgTaxRegistrationCode);
		}

		IInstanceProvider<ITaxIDMacroDataProvider> taxIDMacroDataProvider;

		IInstanceProvider<ITaxIDMacroDataProvider> TaxIDMacroDataProvider =>
			taxIDMacroDataProvider ?? (taxIDMacroDataProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.CookIslands) as IInstanceProvider<ITaxIDMacroDataProvider>);
	}
}
