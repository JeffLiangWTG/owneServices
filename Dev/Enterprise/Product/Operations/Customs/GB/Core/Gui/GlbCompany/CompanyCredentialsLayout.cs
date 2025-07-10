using Enterprise.Customs.GB.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public class CompanyCredentialsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new MasterFiles.GUI.CompanyCredentialsLayoutBuilder<GBGlbCompanyWrapper>();
			var gBBag = CompanyCredentialsControlBag.Instance;
			var commonBag = builder.CommonBag;
			builder.AddControlBag(gBBag);

			builder.AddColumn();
			builder.Add(commonBag.ICS2CredentialUserControl, ControlWidthClass.Auto);
			builder.Add(gBBag.CredentialsDetailsUserControl, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
