using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class GLLocalAccCountryFilterProviderTest : TestCaseWithFactory
	{
		public void TestFilter()
		{
			CodeLookupField master = new CodeLookupField(Factory);
			CodeLookupField detail = new CodeLookupField(Factory);
			master.Value = Core.Constants.CountryCodes.HongKong;

			GLLocalAccCountryFilterProvider provider = new GLLocalAccCountryFilterProvider(master, detail);
			AssertEquals(new ZQuery(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, Core.Constants.CountryCodes.HongKong), provider.Filter);
		}
	}
}
