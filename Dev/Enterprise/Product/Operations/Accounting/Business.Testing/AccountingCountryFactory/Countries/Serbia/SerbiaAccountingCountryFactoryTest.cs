using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class SerbiaAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestIEInvoicingEligibilityDecider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingEligibilityDecider>)?.Get();
			AssertNotNull(obj);
		}
		public void TestITaxIDMacroDataProvider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<ITaxIDMacroDataProvider>)?.Get();
			AssertNotNull(obj);
		}
		static IAccountingCountryFactory GetCountryFactoryForThisCountry()
			=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Serbia);
	}
}
