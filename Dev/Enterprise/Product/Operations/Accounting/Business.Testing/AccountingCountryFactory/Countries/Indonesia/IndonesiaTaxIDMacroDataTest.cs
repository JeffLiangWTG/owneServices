using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class IndonesiaTaxIDMacroDataTest : TestCaseWithFactory
	{
		public void TestITaxIDMacroDataProviderIsImplemented()
		{
			var taxIDMacroDataProvider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Indonesia) as IInstanceProvider<ITaxIDMacroDataProvider>)?.Get();

			AssertNotNull(taxIDMacroDataProvider);
		}

		public void TestGetTaxIDMacroData()
		{
			var taxIDMacroDataProvider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Indonesia) as IInstanceProvider<ITaxIDMacroDataProvider>)?.Get();
			var taxIDMacroDataObject = taxIDMacroDataProvider.GetTaxIDMacroData();
			AssertEquals("OrgTaxRegistrationPrefix", "NPWP #: ", taxIDMacroDataObject.OrgTaxRegistrationPrefix);
			AssertEquals("OrgTaxRegistrationCode", "PPN", taxIDMacroDataObject.OrgTaxRegistrationCode);
			AssertEquals("ExtraOrgTaxRegistrationPrefix", "NITKU #: ", taxIDMacroDataObject.ExtraOrgTaxRegistrationPrefix);
			AssertEquals("ExtraOrgTaxRegistrationCode", "NIT", taxIDMacroDataObject.ExtraOrgTaxRegistrationCode);
		}
	}
}
