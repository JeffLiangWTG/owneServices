using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Vietnam.Testing
{
	public class VietnamAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestIEInvoicingPreEligibilityProvider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingPreEligibilityProvider>)?.Get();
			AssertNotNull(obj);
		}

		static IAccountingCountryFactory GetCountryFactoryForThisCountry()
			=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.VietNam);
	}
}
