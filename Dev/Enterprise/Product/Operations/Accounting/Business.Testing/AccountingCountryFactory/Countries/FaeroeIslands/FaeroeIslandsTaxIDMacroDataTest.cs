using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class FaeroeIslandsTaxIDMacroDataTest : TestCaseWithFactory
	{
		public void TestITaxIDMacroDataProviderIsImplemented()
		{
			AssertNotNull("When FaeroeIslands countryfactory implements IInstanceProvider<ITaxIDMacroDataProvider>", TaxIDMacroDataProvider);
			AssertNotNull("FaeroeIslandsTaxIDMacroData", TaxIDMacroDataProvider.Get());
		}

		public void TestGetTaxIDMacroDataReturnsCorrectValue()
		{
			var taxIDMacroDataObject = TaxIDMacroDataProvider.Get().GetTaxIDMacroData();
			AssertEquals("OrgTaxRegistrationPrefix", "MVG #: ", taxIDMacroDataObject.OrgTaxRegistrationPrefix);
			AssertEquals("OrgTaxRegistrationCode", "MVG", taxIDMacroDataObject.OrgTaxRegistrationCode);
		}

		IInstanceProvider<ITaxIDMacroDataProvider> TaxIDMacroDataProvider =>
			(ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.FaeroeIslands) as IInstanceProvider<ITaxIDMacroDataProvider>);
	}
}
