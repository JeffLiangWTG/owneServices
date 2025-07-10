using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	[TestedType(typeof(GlbCompanyWrapperProvider))]
	sealed class GlbCompanyWrapperProviderTest : MasterFiles.GUI.Testing.GlbCompanyWrapperProviderTest<GlbCompanyWrapperProvider>
	{
		public void TestPluginTextOverride()
		{
			AssertEquals("Customs Configuration", new GlbCompanyWrapperProvider().PluginTextOverride);
		}

		public void TestGetNewCompanyCredentialsLayout()
		{
			var provider = new GlbCompanyWrapperProvider();
			AssertType<CompanyConfigurationLayout>(provider.GetNewCompanyCredentialsLayout());
		}
	}
}
