using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class ZimbabweTaxIDMacroDataTest : TestCaseWithFactory
	{
		public void TestITaxIDMacroDataProviderIsImplemented()
		{
			var taxIDMacroDataProvider = (GetCountryFactoryForThisCountry() as IInstanceProvider<ITaxIDMacroDataProvider>)?.Get();
			AssertNotNull(taxIDMacroDataProvider);
		}

		public void TestGetTaxIDMacroData()
		{
			var taxIDMacroDataProvider = (GetCountryFactoryForThisCountry() as IInstanceProvider<ITaxIDMacroDataProvider>)?.Get();
			var taxIDMacroDataObject = taxIDMacroDataProvider.GetTaxIDMacroData();
			AssertEquals("OrgTaxRegistrationPrefix", "VAT #: ", taxIDMacroDataObject.OrgTaxRegistrationPrefix);
			AssertEquals("OrgTaxRegistrationCode", "VAT", taxIDMacroDataObject.OrgTaxRegistrationCode);
			AssertEquals("ExtraOrgTaxRegistrationPrefix", "TIN #: ", taxIDMacroDataObject.ExtraOrgTaxRegistrationPrefix);
			AssertEquals("ExtraOrgTaxRegistrationCode", "TIN", taxIDMacroDataObject.ExtraOrgTaxRegistrationCode);
		}

		static IAccountingCountryFactory GetCountryFactoryForThisCountry()
	=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Zimbabwe);
	}
}
