using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class CompanyCredentialsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MasterFiles.GUI.CompanyCredentialsLayoutBuilder<Business.GlbCompanyWrapper>();
			var iEBag = CompanyCredentialsControlBag.Instance;
			var commonBag = builder.CommonBag;

			builder.AddControlBag(iEBag);

			builder.AddColumn();
			builder.Add(commonBag.ICS2CredentialUserControl, ControlWidthClass.LongControl);
			builder.Add(iEBag.CompanyCredentialsDetailsUserControl, ControlWidthClass.LongControl);

			return builder.Build();
		}
	}
}
