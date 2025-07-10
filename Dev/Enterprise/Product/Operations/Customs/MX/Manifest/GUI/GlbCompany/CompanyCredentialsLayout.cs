using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.Manifest.GUI
{
	public class CompanyCredentialsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MasterFiles.GUI.CompanyCredentialsLayoutBuilder<Business.GlbCompanyWrapper>();
			var mXBag = CompanyCredentialsControlBag.Instance;
			var commonBag = builder.CommonBag;
			builder.AddControlBag(mXBag);

			builder.AddColumn();
			builder.Add(mXBag.CredentialsGroupBox, ControlWidthClass.LongControl);
			builder.Add(commonBag.ICS2CredentialUserControl, ControlWidthClass.LongControl);

			return builder.Build();
		}
	}
}
