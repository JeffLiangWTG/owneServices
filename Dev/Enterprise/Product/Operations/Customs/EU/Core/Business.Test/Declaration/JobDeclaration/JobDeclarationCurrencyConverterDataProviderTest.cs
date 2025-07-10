using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class JobDeclarationCurrencyConverterDataProviderTest : TestCaseWithFactory
	{
		[TestDate(2021, 03, 19)]
		public void TestICurrencyConverterDataProviderMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var jobDeclarationCurrencyConverterDataProvider = new JobDeclarationCurrencyConverterDataProvider(declaration);
			CombineAssertions(() =>
			{
				var provider = (ICurrencyConverterDataProvider)jobDeclarationCurrencyConverterDataProvider;
				AssertEquals(nameof(provider.DateOfValuation), declaration.DateOfValuation, provider.DateOfValuation);
				AssertEquals(nameof(provider.RateType), ZArchitecture.Core.ExchangeRateType.Customs, provider.RateType);
				AssertEquals(nameof(provider.MaximumDaysToFallback), Customs.Business.BaseJobDeclaration.CurrencyConverterMaximumDaysToFallBack, provider.MaximumDaysToFallback);
				AssertEquals(nameof(provider.Company), declaration.Company, provider.Company);
				AssertEquals(nameof(provider.LocalCurrencyCodeOverride), declaration.LocalCurrencyCode, provider.LocalCurrencyCodeOverride);
				AssertEquals(nameof(provider.IsReciprocalOverride), declaration.IsReciprocalRates, provider.IsReciprocalOverride);
			});

			jobDeclarationCurrencyConverterDataProvider = new JobDeclarationCurrencyConverterDataProvider(null);
			CombineAssertions(() =>
			{
				var provider = (ICurrencyConverterDataProvider)jobDeclarationCurrencyConverterDataProvider;
				AssertEquals(nameof(provider.DateOfValuation), ZDateTime.Today, provider.DateOfValuation);
				AssertEquals(nameof(provider.RateType), ZArchitecture.Core.ExchangeRateType.Customs, provider.RateType);
				AssertEquals(nameof(provider.MaximumDaysToFallback), Customs.Business.BaseJobDeclaration.CurrencyConverterMaximumDaysToFallBack, provider.MaximumDaysToFallback);
				AssertEquals(nameof(provider.Company), GlbCompany.CurrentCompany.PK, provider.Company.PK);
				AssertEquals(nameof(provider.LocalCurrencyCodeOverride), Core.Constants.CurrencyCodes.Italy, provider.LocalCurrencyCodeOverride);
				AssertEquals(nameof(provider.IsReciprocalOverride), GlbCompany.CurrentCompany.GC_IsReciprocal, provider.IsReciprocalOverride);
			});
		}
	}
}
