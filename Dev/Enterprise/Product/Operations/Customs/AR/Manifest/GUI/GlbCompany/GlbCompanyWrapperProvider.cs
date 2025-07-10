using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AR.Manifest.GUI
{
	public class GlbCompanyWrapperProvider : MasterFiles.GUI.GlbCompanyWrapperProvider, MasterFiles.Integration.Customs.AR.IARGlbCompanyWrapperProvider
	{
		public override IPanelLayoutProvider GetNewCompanyCredentialsLayout() => new CompanyCredentialsLayout();
	}
}
