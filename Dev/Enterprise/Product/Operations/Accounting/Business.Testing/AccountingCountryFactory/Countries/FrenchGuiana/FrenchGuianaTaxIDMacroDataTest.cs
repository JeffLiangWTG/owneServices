using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class FrenchGuianaTaxIDMacroDataTest : TestCaseWithFactory
	{
		public void TestITaxIDMacroDataProviderIsImplemented()
		{
			var taxIDMacroDataProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.FrenchGuyana) as IInstanceProvider<ITaxIDMacroDataProvider>;
			AssertNotNull("When FrenchGuiana countryfactory implements IInstanceProvider<ITaxIDMacroDataProvider>", taxIDMacroDataProvider);
			AssertNotNull("FrenchGuianaTaxIDMacroData", taxIDMacroDataProvider.Get());
		}

		public void TestGetTaxIDMacroData()
		{
			var taxIDMacroDataObject = TaxIDMacroDataProvider.Get().GetTaxIDMacroData();
			AssertEquals("OrgTaxRegistrationPrefix", "TVA #: ", taxIDMacroDataObject.OrgTaxRegistrationPrefix);
			AssertEquals("OrgTaxRegistrationCode", "TVA", taxIDMacroDataObject.OrgTaxRegistrationCode);
		}

		IInstanceProvider<ITaxIDMacroDataProvider> TaxIDMacroDataProvider => taxIDMacroDataProvider ??
			(taxIDMacroDataProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.FrenchGuyana) as IInstanceProvider<ITaxIDMacroDataProvider>);
		IInstanceProvider<ITaxIDMacroDataProvider> taxIDMacroDataProvider;
	}
}
