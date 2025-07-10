using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public partial class ReportsUcc6GridFieldsLayoutUserControl : ReportsGridFieldsLayoutUserControl
{
	public ReportsUcc6GridFieldsLayoutUserControl()
	{
		InitializeComponent();
	}

	protected override IPanelLayoutProvider GetLayout() => new ReportsUcc6GridFieldsLayout();
}
