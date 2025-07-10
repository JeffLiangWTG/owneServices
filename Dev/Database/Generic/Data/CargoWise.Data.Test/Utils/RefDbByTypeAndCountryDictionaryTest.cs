using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class RefDbByTypeAndCountryDictionaryTest : TestCase
	{
		public void TestGetAndSet()
		{
			var refDbNameDictionary = new RefDbByTypeAndCountryDictionary();

			AssertNull("EntUS not yet in the dictionary => should be null", refDbNameDictionary.GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "US"));
			refDbNameDictionary.SetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "US", "DummyEntUsDb");
			AssertEquals("EntUS name", "DummyEntUsDb", refDbNameDictionary.GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "US"));

			AssertNull("CmrAU not yet in the dictionary => should be null", refDbNameDictionary.GetReferenceDatabaseName(RefDbTypeEnum.Customs, "AU"));
			refDbNameDictionary.SetReferenceDatabaseName(RefDbTypeEnum.Customs, "AU", "DummyCmrAUDb");
			AssertEquals("CmrAU name", "DummyCmrAUDb", refDbNameDictionary.GetReferenceDatabaseName(RefDbTypeEnum.Customs, "AU"));

			refDbNameDictionary.SetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "US", "AnotherName");
			AssertEquals("EntUS name [after change]", "AnotherName", refDbNameDictionary.GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, "US"));
		}
	}
}
