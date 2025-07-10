using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class CompanyCredentialsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MasterFiles.GUI.CompanyCredentialsLayoutBuilder<Business.GlbCompanyWrapper>();
			var kRBag = CompanyCredentialsControlBag.Instance;
			var commonBag = builder.CommonBag;
			builder.AddControlBag(kRBag);

			builder.AddColumn();
			builder.Add(kRBag.UnipassCertificateGroupBox, ControlWidthClass.LongControl);
			builder.Add(commonBag.ICS2CredentialUserControl, ControlWidthClass.LongControl);

			return builder.Build();
		}
	}
}
