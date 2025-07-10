using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(SupplementaryCodeProvider))]
	sealed class SupplementaryCodeProviderTest : BaseSupplementaryCodeProviderTest<SupplementaryCodeProvider, SupplementaryCode>
	{
		[ExpectNoExceptions]
		public void TestGetByCountryCode() => CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(SupplementaryCodeProvider.GetByCountryCode(ZString.Empty), NUnit.Framework.Is.TypeOf<SupplementaryCodeProvider>(), "When Code is empty");
			NUnit.Framework.Assert.That(SupplementaryCodeProvider.GetByCountryCode("AA"), NUnit.Framework.Is.TypeOf<SupplementaryCodeProvider>(), "When No Entry Exists for Country");
			NUnit.Framework.Assert.That(SupplementaryCodeProvider.GetByCountryCode(Constants.CountryCodes.Germany), NUnit.Framework.Is.Not.EqualTo(default(BaseSupplementaryCodeProvider)), "When Registry Exists For Country - should not be [null]");
		});

		protected override ZString CountryCodeForSupplementaryCodeProvider => "EUN";

		protected override ZShort ExpectedAdditionalSupplementaryCodesStartingOrder => 3;

		protected override ZShort ExpectedNumberOfAdditionalSupplementaryCodes => 8;

		protected override Type ExpectedGetNewValidationReturnType => typeof(SupplementaryCodeValidation);

		protected override Type ExpectedGetNewLookupsReturnType => typeof(BaseSupplementaryCodeLookups);

		protected override SupplementaryCode GetSupplementaryCode() => Factory.New<SupplementaryCode>();

		protected override SupplementaryCodeProvider CreateSupplementaryCodeProvider() => new (CountryCodeForSupplementaryCodeProvider);
	}
}
