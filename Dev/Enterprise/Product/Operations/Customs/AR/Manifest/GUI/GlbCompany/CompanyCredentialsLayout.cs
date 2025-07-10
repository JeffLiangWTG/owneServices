using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AR.Manifest.GUI
{
	public class CompanyCredentialsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MasterFiles.GUI.CompanyCredentialsLayoutBuilder<Business.GlbCompanyWrapper>();
			var aRBag = CompanyCredentialsControlBag.Instance;
			var commonBag = builder.CommonBag;

			builder.AddControlBag(aRBag);

			builder.AddColumn();
			builder.Add(commonBag.ICS2CredentialUserControl, ControlWidthClass.LongControl);
			builder.Add(aRBag.CompanyCredentialsDetailsUserControl, ControlWidthClass.LongControl);

			return builder.Build();
		}
	}
}
