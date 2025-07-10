using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	class GhanaAccountingCountryFactoryTest : TestCaseWithFactory
	{
		public void TestISurchargeApplicationConfigurationUsesTaxIdIsImplemented()
		{
			var countryFactory = GetCountryFactory();
			Assert(typeof(ISurchargeApplicationConfigurationUsesTaxId).IsAssignableFrom(countryFactory.GetType()));
		}

		IAccountingCountryFactory GetCountryFactory() => ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCodes.Ghana);
	}
}
