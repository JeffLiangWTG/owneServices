using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	sealed class AdditionalReferenceNumberTypesParametersTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCountry()
		{
			NUnit.Framework.Assert.That(new AdditionalReferenceNumberTypesParameters("Country").CountryCode, Is.EqualTo("Country").Using(CustomComparers.TypeComparison), "Property Country should be equal to the expected value");
		}

		[ExpectNoExceptions]
		public void TestKey()
		{
			var parent = Factory.New<DummyBusinessObject>();
			var additionalReferenceNumberTypesParameters = new AdditionalReferenceNumberTypesParameters("IL");
			additionalReferenceNumberTypesParameters.Parent = parent;
			additionalReferenceNumberTypesParameters.DischargeCountryCode = "CN";
			NUnit.Framework.Assert.That(additionalReferenceNumberTypesParameters.Key, Is.EqualTo("IL_CargoWise.EntityFramework.Testing.DummyBusinessObject_CN").Using(CustomComparers.TypeComparison), "Property Key should be equal to the expected value");
		}
	}
}
