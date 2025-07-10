using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(GlbCompanyWrapperProvider))]
	sealed class GlbCompanyWrapperProviderTest : MasterFiles.GUI.Testing.GlbCompanyWrapperProviderTest<GlbCompanyWrapperProvider>
	{
		public void TestGetNewCompanyCredentialsLayout()
		{
			var provider = new GlbCompanyWrapperProvider();
			AssertType<CompanyCredentialsLayout>(provider.GetNewCompanyCredentialsLayout());
		}
	}
}
