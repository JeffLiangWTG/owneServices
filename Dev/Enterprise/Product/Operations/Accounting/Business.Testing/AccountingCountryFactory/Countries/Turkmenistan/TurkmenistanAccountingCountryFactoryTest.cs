using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class TurkmenistanAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestITaxIDMacroDataProvider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<ITaxIDMacroDataProvider>)?.Get();
			AssertNotNull(obj);
		}

		static IAccountingCountryFactory GetCountryFactoryForThisCountry()
			=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Turkmenistan);
	}
}
