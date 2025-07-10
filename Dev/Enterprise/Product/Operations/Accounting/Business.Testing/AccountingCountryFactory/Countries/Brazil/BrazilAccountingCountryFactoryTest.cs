using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class BrazilAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestITaxFrameworkThresholdMethodProvider()
		{
			var provider = (GetCountryFactoryForThisCountry() as IInstanceProvider<ITaxFrameworkThresholdMethodProvider>)?.Get();
			AssertNotNull(provider);
		}

		public void TestIEInvoicingPreEligibilityProvider()
		{
			var provider = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingPreEligibilityProvider>)?.Get();
			AssertNotNull(provider);
		}

		static IAccountingCountryFactory GetCountryFactoryForThisCountry()
			=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Brazil);
	}
}
