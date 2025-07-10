using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILManifestRegistryOptionsTest : TestCaseWithFactory
	{
		public void TestList()
		{
			var list = new ILManifestRegistryOptions();
			AssertEquals(4, list.Count);
			AssertEquals("All", list.GetDescriptionFromCode("ALL"));
			AssertEquals("Export", list.GetDescriptionFromCode("EXPORT"));
			AssertEquals("Import", list.GetDescriptionFromCode("IMPORT"));
			AssertEquals("None", list.GetDescriptionFromCode("NONE"));
		}
	}
}
