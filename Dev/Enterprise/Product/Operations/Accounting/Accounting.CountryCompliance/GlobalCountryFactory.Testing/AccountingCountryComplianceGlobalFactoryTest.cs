using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.CountryCompliance.GlobalCountryFactory.Testing
{
	public class AccountingCountryComplianceGlobalFactoryTest : TestCaseWithFactory
	{
		public void TestObjectFactoryCanProduceGlobalFactory()
		{
			AssertType(typeof(AccountingCountryComplianceGlobalFactory), ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>());
		}

		public void TestGlobalFactoryFromObjectFactoryIsNotSingleton()
		{
			var accountingCountryComplianceGlobalFactory = ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>();
			var accountingCountryComplianceGlobalFactory2 = ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>();
			AssertNotEquals(accountingCountryComplianceGlobalFactory, accountingCountryComplianceGlobalFactory2);
		}

		public void TestGetFeatureInterfaceReturnsNullForEmptyCountryCode()
		{
			AssertNull(GetGlobalFactory().GetFeatureInterface<IAccountingCountryComplianceGlobalFactory>(ZString.Empty));
		}

		public void TestGetFeatureInterfaceReturnsNullForInvalidCountryCode()
		{
			AssertNull(GetGlobalFactory().GetFeatureInterface<IAccountingCountryComplianceGlobalFactory>("InvalidCountryCode"));
		}

		public void TestGetFeatureInterfaceReturnsNullForNotImplementedFeatureInAllCountries()
		{
			foreach (var country in Factory.Load<RefCountry>(new ZQuery()))
			{
				AssertNull(GetGlobalFactory().GetFeatureInterface<IAccountingCountryComplianceGlobalFactory>(country.Code));
			}
		}

		IAccountingCountryComplianceGlobalFactory GetGlobalFactory() => new AccountingCountryComplianceGlobalFactory();
	}
}
