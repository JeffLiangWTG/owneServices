using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.Manifest.GUI
{
	public class GlbCompanyWrapperProvider : MasterFiles.GUI.GlbCompanyWrapperProvider, MasterFiles.Integration.Customs.MX.IMXGlbCompanyWrapperProvider
	{
		public override IPanelLayoutProvider GetNewCompanyCredentialsLayout() => new CompanyCredentialsLayout();
	}
}
