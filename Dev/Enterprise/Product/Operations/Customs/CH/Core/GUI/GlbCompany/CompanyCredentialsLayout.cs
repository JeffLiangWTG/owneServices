using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public class CompanyCredentialsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; } = CreateLayout();

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new MasterFiles.GUI.CompanyCredentialsLayoutBuilder<Business.GlbCompanyWrapper>();
		var cHBag = CompanyCredentialsControlBag.Instance;
		var commonBag = builder.CommonBag;
		builder.AddControlBag(cHBag);

		builder.AddColumn();
		builder.Add(cHBag.CompanyCredentialsDetailsUserControl, ControlWidthClass.LongControl);
		builder.Add(commonBag.ICS2CredentialUserControl, ControlWidthClass.LongControl);

		return builder.Build();
	}
}
