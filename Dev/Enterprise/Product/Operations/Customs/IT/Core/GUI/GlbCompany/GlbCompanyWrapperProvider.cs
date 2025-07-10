using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class GlbCompanyWrapperProvider : MasterFiles.GUI.GlbCompanyWrapperProvider, MasterFiles.Integration.CustomsIntegration.IT.IGlbCompanyWrapperProvider
{
	public override IPanelLayoutProvider GetNewCompanyCredentialsLayout() => new CompanyCredentialsLayout();
}
