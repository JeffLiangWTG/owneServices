using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class ItalyAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestIEInvoicingPreEligibilityProvider()
		{
			var provider = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingPreEligibilityProvider>);
			AssertNotNull("IInstanceProvider<IEInvoicingPreEligibilityProvider> is implemented in Italy", provider);
			AssertType<ItalyEInvoicingPreEligibilityProvider>(provider.Get());
		}

		static IAccountingCountryFactory GetCountryFactoryForThisCountry()
			=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Italy);
	}
}
