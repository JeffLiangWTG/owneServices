using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class SwazilandTaxIDMacroDataTest : TestCaseWithFactory
	{
		public void TestITaxIDMacroDataProviderIsImplemented()
		{
			var taxIDMacroDataProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Swaziland) as IInstanceProvider<ITaxIDMacroDataProvider>;
			AssertNotNull("When Swaziland country factory implements IInstanceProvider<ITaxIDMacroDataProvider>", taxIDMacroDataProvider);
			AssertNotNull("SwazilandTaxIDMacroData", taxIDMacroDataProvider.Get());
		}

		public void TestGetTaxIDMacroData()
		{
			var taxIDMacroDataObject = TaxIDMacroDataProvider.Get().GetTaxIDMacroData();
			AssertEquals("OrgTaxRegistrationPrefix", "TIN #: ", taxIDMacroDataObject.OrgTaxRegistrationPrefix);
			AssertEquals("OrgTaxRegistrationCode", "VAT", taxIDMacroDataObject.OrgTaxRegistrationCode);
		}

		IInstanceProvider<ITaxIDMacroDataProvider> taxIDMacroDataProvider;

		IInstanceProvider<ITaxIDMacroDataProvider> TaxIDMacroDataProvider =>
			taxIDMacroDataProvider ?? (taxIDMacroDataProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Swaziland) as IInstanceProvider<ITaxIDMacroDataProvider>);
	}
}
