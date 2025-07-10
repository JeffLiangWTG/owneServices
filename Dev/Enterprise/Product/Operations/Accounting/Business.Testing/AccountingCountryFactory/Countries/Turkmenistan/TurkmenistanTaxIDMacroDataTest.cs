using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class TurkmenistanTaxIDMacroDataTest : TestCaseWithFactory
	{
		public void TestGetTaxIDMacroData()
		{
			var taxIdMacroObject = GetTaxIDMacroDataProvider().GetTaxIDMacroData();
			CombineAssertions(() =>
			{
				AssertEquals("OrgTaxRegistrationPrefix", "VAT #: ", taxIdMacroObject.OrgTaxRegistrationPrefix);
				AssertEquals("OrgTaxRegistrationCode", OrgCusCode.CodeTypes.VATCode, taxIdMacroObject.OrgTaxRegistrationCode);
			});
		}

		static ITaxIDMacroDataProvider GetTaxIDMacroDataProvider()
			=> (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Turkmenistan) as IInstanceProvider<ITaxIDMacroDataProvider>).Get();
	}
}
