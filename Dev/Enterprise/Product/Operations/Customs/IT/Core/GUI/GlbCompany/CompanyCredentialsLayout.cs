using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class CompanyCredentialsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; } = CreateLayout();

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new MasterFiles.GUI.CompanyCredentialsLayoutBuilder<Business.GlbCompanyWrapper>();
		var iTBag = CompanyCredentialsControlBag.Instance;
		var commonBag = builder.CommonBag;
		builder.AddControlBag(iTBag);

		builder.AddColumn();
		builder.Add(iTBag.CompanyCredentialsDetailsUserControl, ControlWidthClass.LongControl);
		builder.Add(commonBag.ICS2CredentialUserControl, ControlWidthClass.LongControl);

		return builder.Build();
	}
}
