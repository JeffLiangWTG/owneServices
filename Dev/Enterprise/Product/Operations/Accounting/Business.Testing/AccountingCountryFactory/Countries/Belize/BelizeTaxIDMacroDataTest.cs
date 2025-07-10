using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class BelizeTaxIDMacroDataTest : TestCaseWithFactory
	{
		public void TestITaxIDMacroDataProviderIsImplemented()
		{
			var taxIDMacroDataProvider = GetTaxIDMacroDataProvider();
			AssertNotNull("When Belize countryfactory implements IInstanceProvider<ITaxIDMacroDataProvider>", taxIDMacroDataProvider);
			AssertNotNull("BelizeTaxIDMacroData", taxIDMacroDataProvider);
		}

		public void TestGetTaxIDMacroData()
		{
			var taxIdMacroObject = GetTaxIDMacroDataProvider().GetTaxIDMacroData();
			CombineAssertions(() =>
			{
				AssertEquals("OrgTaxRegistrationPrefix", "TIN #: ", taxIdMacroObject.OrgTaxRegistrationPrefix);
				AssertEquals("OrgTaxRegistrationCode", "GST", taxIdMacroObject.OrgTaxRegistrationCode);
			});
		}

		static ITaxIDMacroDataProvider GetTaxIDMacroDataProvider()
			=> (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Belize) as IInstanceProvider<ITaxIDMacroDataProvider>).Get();
	}
}
