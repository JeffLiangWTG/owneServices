using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
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
