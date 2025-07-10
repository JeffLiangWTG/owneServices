using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class BaseApplicationBusinessProviderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestIsReciprocalRates_DeclarationIsNull_ZZRefCusConfigurationExists_ReciprocalIsYes()
		{
			ZZRefCusConfiguration.New(GlbCompany.CurrentCompany).ZZC_IsReciprocalExchangeRate = IsReciprocalExchangeRateList.Codes.Yes;
			Factory.Save();
			AssertEquals(true, new BaseApplicationBusinessProviderForTest().IsReciprocalRates(null));
		}

		public void TestIsReciprocalRates_DeclarationIsNull_ZZRefCusConfigurationExists_ReciprocalIsNo()
		{
			ZZRefCusConfiguration.New(GlbCompany.CurrentCompany).ZZC_IsReciprocalExchangeRate = IsReciprocalExchangeRateList.Codes.No;
			Factory.Save();
			AssertEquals(false, new BaseApplicationBusinessProviderForTest().IsReciprocalRates(null));
		}

		public void TestIsReciprocalRates_DeclarationIsNull_ZZRefCusConfigurationExists_ReciprocalIsBlank()
		{
			ZZRefCusConfiguration.New(GlbCompany.CurrentCompany).ZZC_IsReciprocalExchangeRate = ZString.Empty;
			Factory.Save();
			var provider = new BaseApplicationBusinessProviderForTest();
			CombineAssertions(() =>
			{
				provider.IsReciprocalRatesCoreExposed = true;
				AssertEquals("IsReciprocalRatesCore = true", true, provider.IsReciprocalRates(null));
				provider.IsReciprocalRatesCoreExposed = false;
				AssertEquals("IsReciprocalRatesCore = false", false, provider.IsReciprocalRates(null));
			});
		}

		public void TestIsReciprocalRates_DeclarationIsNull_ZZRefCusConfigurationDoesNotExist()
		{
			var provider = new BaseApplicationBusinessProviderForTest();
			CombineAssertions(() =>
			{
				provider.IsReciprocalRatesCoreExposed = true;
				AssertEquals("IsReciprocalRatesCore = true", true, provider.IsReciprocalRates(null));
				provider.IsReciprocalRatesCoreExposed = false;
				AssertEquals("IsReciprocalRatesCore = false", false, provider.IsReciprocalRates(null));
			});
		}

		public void TestIsReciprocalRates_DeclarationIsNotNull_ZZRefCusConfigurationExists_ReciprocalIsYes()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			ZZRefCusConfiguration.New(company).ZZC_IsReciprocalExchangeRate = IsReciprocalExchangeRateList.Codes.Yes;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GC = company.PK;
			AssertEquals(true, new BaseApplicationBusinessProviderForTest().IsReciprocalRates(declaration));
		}

		public void TestIsReciprocalRates_DeclarationIsNotNull_ZZRefCusConfigurationExists_ReciprocalIsNo()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			ZZRefCusConfiguration.New(company).ZZC_IsReciprocalExchangeRate = IsReciprocalExchangeRateList.Codes.No;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GC = company.PK;
			AssertEquals(false, new BaseApplicationBusinessProviderForTest().IsReciprocalRates(declaration));
		}

		public void TestIsReciprocalRates_DeclarationIsNotNull_ZZRefCusConfigurationExists_ReciprocalIsBlank()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			ZZRefCusConfiguration.New(company).ZZC_IsReciprocalExchangeRate = ZString.Empty;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GC = company.PK;
			var provider = new BaseApplicationBusinessProviderForTest();
			CombineAssertions(() =>
			{
				provider.IsReciprocalRatesCoreExposed = true;
				AssertEquals("IsReciprocalRatesCore = true", true, provider.IsReciprocalRates(declaration));
				provider.IsReciprocalRatesCoreExposed = false;
				AssertEquals("IsReciprocalRatesCore = false", false, provider.IsReciprocalRates(declaration));
			});
		}

		public void TestIsReciprocalRates_DeclarationIsNotNull_ZZRefCusConfigurationDoesNotExist()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GC = company.PK;
			var provider = new BaseApplicationBusinessProviderForTest();
			CombineAssertions(() =>
			{
				provider.IsReciprocalRatesCoreExposed = true;
				AssertEquals("IsReciprocalRatesCore = true", true, provider.IsReciprocalRates(declaration));
				provider.IsReciprocalRatesCoreExposed = false;
				AssertEquals("IsReciprocalRatesCore = false", false, provider.IsReciprocalRates(declaration));
			});
		}
	}
}
