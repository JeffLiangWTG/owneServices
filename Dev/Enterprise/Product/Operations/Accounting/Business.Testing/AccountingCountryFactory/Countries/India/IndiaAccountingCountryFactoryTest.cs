using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class IndiaAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestIComplianceNumberResetStatus()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IComplianceNumberResetStatus>)?.Get();
			AssertNotNull(obj);
		}

		public void TestIEInvoicingEligibilityDecider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingEligibilityDecider>)?.Get();
			AssertNotNull(obj);
		}

		public void TestIEInvoicingPreEligibilityProvider()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<IEInvoicingPreEligibilityProvider>)?.Get();
			AssertNotNull(obj);
		}

		public void TestITaxFrameworkConfigurationDefaults()
		{
			var obj = (GetCountryFactoryForThisCountry() as IInstanceProvider<ITaxFrameworkConfigurationDefaults>)?.Get();
			AssertNotNull(obj);
		}

		static IAccountingCountryFactory GetCountryFactoryForThisCountry()
			=> ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.India);
	}
}
