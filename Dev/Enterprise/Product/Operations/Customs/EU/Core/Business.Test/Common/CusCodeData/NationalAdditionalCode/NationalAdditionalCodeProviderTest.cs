using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	class NationalAdditionalCodeProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetByCountryCode()
		{
			NUnit.Framework.Assert.That(NationalAdditionalCodeProvider.GetByCountryCode(ZString.Empty), NUnit.Framework.Is.EqualTo(default(NationalAdditionalCodeProvider)), "Should be null when country code is empty - should be [null]");

			var nationalAdditionalCodeProvider = NationalAdditionalCodeProvider.GetByCountryCode("EUN");
			NUnit.Framework.Assert.That(nationalAdditionalCodeProvider, NUnit.Framework.Is.Not.EqualTo(default(NationalAdditionalCodeProvider)), "Should be not null when country parameter is not empty - should not be [null]");
			CombineAssertions("", () =>
			{
				NUnit.Framework.Assert.That(nationalAdditionalCodeProvider, NUnit.Framework.Is.TypeOf<NationalAdditionalCodeProvider>(), "Provider type");
				NUnit.Framework.Assert.That(nationalAdditionalCodeProvider.CountryCode, NUnit.Framework.Is.EqualTo("EUN").Using(CustomComparers.TypeComparison), "Country Code");
			});
		}

		[ExpectNoExceptions]
		public void TestGetByNationalAdditionalCodeSupporter()
		{
			NUnit.Framework.Assert.That(NationalAdditionalCodeProvider.GetByNationalAdditionalCodeSupporter(null), NUnit.Framework.Is.EqualTo(default(NationalAdditionalCodeProvider)), "Should be null when code supporter is null - should be [null]");

			var nationalAdditionalCodeProvider = NationalAdditionalCodeProvider.GetByNationalAdditionalCodeSupporter(nationalAdditionalCodeSupporterForTest);
			NUnit.Framework.Assert.That(nationalAdditionalCodeProvider, NUnit.Framework.Is.Not.EqualTo(default(NationalAdditionalCodeProvider)), "Should be not null when country parameter is not null - should not be [null]");
			CombineAssertions("Checking NationalAdditionalCodeProvider", () =>
			{
				NUnit.Framework.Assert.That(nationalAdditionalCodeProvider, NUnit.Framework.Is.TypeOf<NationalAdditionalCodeProvider>(), "NationalAdditionalCodeProvider type");
				NUnit.Framework.Assert.That(nationalAdditionalCodeProvider.CountryCode, NUnit.Framework.Is.EqualTo("EUN").Using(CustomComparers.TypeComparison), "Country Code");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nationalAdditionalCodeSupporterForTest = new NationalAdditionalCodeSupporterForTest(Factory);
		}
		NationalAdditionalCodeSupporterForTest nationalAdditionalCodeSupporterForTest;
	}

	class NationalAdditionalCodeSupporterForTest : INationalAdditionalCodeSupporter
	{
		public NationalAdditionalCodeSupporterForTest(BusinessObjectFactory factory)
		{
			Factory = factory;
		}
		BusinessObjectFactory Factory { get; }
		public TariffView Tariff => Factory.New<TariffView>();

		public IZZRateSelectionCriteria RateSelectionCriteria => null;

		public CodeDescriptionPairList CachedListOfAdditionalCodeDescriptions => null;

		public ZString GetCountryCodeForCodeProvider() => "EUN";

		public NationalAdditionalCodeCollection NationalAdditionalCodes => null;

		public void OnCodesChanged()
		{
		}
	}
}
