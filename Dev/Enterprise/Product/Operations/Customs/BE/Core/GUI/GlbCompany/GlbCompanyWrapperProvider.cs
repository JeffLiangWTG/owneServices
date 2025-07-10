using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public class GlbCompanyWrapperProvider : MasterFiles.GUI.GlbCompanyWrapperProvider, MasterFiles.Integration.Customs.BE.IBEGlbCompanyWrapperProvider
{
	public override IPanelLayoutProvider GetNewCompanyCredentialsLayout() => new CompanyCredentialsLayout();
}
