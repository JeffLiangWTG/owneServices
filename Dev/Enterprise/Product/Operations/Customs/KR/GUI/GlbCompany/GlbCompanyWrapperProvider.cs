using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class GlbCompanyWrapperProvider : MasterFiles.GUI.GlbCompanyWrapperProvider, MasterFiles.Integration.Customs.KR.IKRGlbCompanyWrapperProvider
	{
		public override IPanelLayoutProvider GetNewCompanyCredentialsLayout() => new CompanyCredentialsLayout();
	}
}
