using Enterprise.Customs.BE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public class CompanyCredentialsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; } = CreateLayout();

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new MasterFiles.GUI.CompanyCredentialsLayoutBuilder<BEGlbCompanyWrapper>();
		var beBag = CompanyCredentialsControlBag.Instance;
		builder.AddControlBag(beBag);

		builder.AddColumn();
		builder.Add(beBag.UserIDTextBox, ControlWidthClass.Auto);
		builder.Add(beBag.CurrentPasswordTextBox, ControlWidthClass.Auto);

		return builder.Build();
	}
}
