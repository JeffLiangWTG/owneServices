using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class CapeVerdeTaxIDMacroDataTest : TestCaseWithFactory
	{
		public void TestITaxIDMacroDataProviderIsImplemented()
		{
			AssertNotNull("When CapeVerde country factory implements IInstanceProvider<ITaxIDMacroDataProvider>", TaxIDMacroDataProvider);
			AssertNotNull("CapeVerdeTaxIDMarcoData", TaxIDMacroDataProvider.Get());
		}

		public void TestGetTaxIDMacroData()
		{
			var taxIDMacroDataObject = TaxIDMacroDataProvider.Get().GetTaxIDMacroData();
			AssertEquals("OrgTaxRegistrationPrefix", "NIF#: ", taxIDMacroDataObject.OrgTaxRegistrationPrefix);
			AssertEquals("OrgTaxRegistrationCode", "IVA", taxIDMacroDataObject.OrgTaxRegistrationCode);
		}

		IInstanceProvider<ITaxIDMacroDataProvider> taxIDMacroDataProvider;

		IInstanceProvider<ITaxIDMacroDataProvider> TaxIDMacroDataProvider =>
			taxIDMacroDataProvider ?? (taxIDMacroDataProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.CapeVerde) as IInstanceProvider<ITaxIDMacroDataProvider>);
	}
}
