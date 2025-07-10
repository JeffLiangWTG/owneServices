using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class KyrgyzstanTaxIDMacroDataTest : TestCaseWithFactory
	{
		public void TestGetTaxIDMacroData()
		{
			var taxIdMacroObject = GetTaxIDMacroDataProvider().GetTaxIDMacroData();
			CombineAssertions(() =>
			{
				AssertEquals("OrgTaxRegistrationPrefix", "TIN #: ", taxIdMacroObject.OrgTaxRegistrationPrefix);
				AssertEquals("OrgTaxRegistrationCode", KyrgyzstanOrgCusCodeInfo.OrgCusCodes.TIN, taxIdMacroObject.OrgTaxRegistrationCode);
			});
		}

		static ITaxIDMacroDataProvider GetTaxIDMacroDataProvider()
			=> (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Kyrgyzstan) as IInstanceProvider<ITaxIDMacroDataProvider>).Get();
	}
}
