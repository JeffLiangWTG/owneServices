using Enterprise.MasterFiles.Integration.Customs.AsycudaCustoms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public class GlbCompanyWrapperProvider : MasterFiles.GUI.GlbCompanyWrapperProvider, IAsycudaGlbCompanyWrapperProvider
	{
		public override IPanelLayoutProvider GetNewCompanyCredentialsLayout() => new CompanyConfigurationLayout();

		public override string PluginTextOverride => Res.GetString("F394AFE8-A24A-4AC5-9CEC-E7D90C519805", "Customs Configuration");
	}
}
