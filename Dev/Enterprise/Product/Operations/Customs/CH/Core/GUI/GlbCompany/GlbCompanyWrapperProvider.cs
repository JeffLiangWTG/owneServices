using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public class GlbCompanyWrapperProvider : MasterFiles.GUI.GlbCompanyWrapperProvider, MasterFiles.Integration.Customs.CH.ICHGlbCompanyWrapperProvider
{
	public override IPanelLayoutProvider GetNewCompanyCredentialsLayout() => new CompanyCredentialsLayout();
}
