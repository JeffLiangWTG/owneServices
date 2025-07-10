using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public class CompanyConfigurationLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MasterFiles.GUI.CompanyCredentialsLayoutBuilder<Business.GlbCompanyWrapper>();
			var bWBag = CompanyConfigurationControlBag.Instance;
			var commonBag = builder.CommonBag;
			builder.AddControlBag(bWBag);

			builder.AddColumn();
			builder.Add(commonBag.ICS2CredentialUserControl, ControlWidthClass.Auto);
			builder.Add(bWBag.CustomsConfigurationGroupBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
