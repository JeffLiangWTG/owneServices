using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public partial class ReportsUcc6GridUserControl : ZUserControl, IReportsGridUserControl
{
	public ReportsUcc6GridUserControl()
	{
		InitializeComponent();
		LoadFieldsControl();
	}

	ZGrid IReportsGridUserControl.ReportsGrid => ReportsGrid;

	public ReportsUcc6GridFieldsLayoutUserControl ReportsGridFieldsLayoutUserControl { get; private set; }

	void LoadFieldsControl()
	{
		if (ReportsGridFieldsLayoutUserControl is null)
		{
			ReportsGridFieldsLayoutUserControl = new ReportsUcc6GridFieldsLayoutUserControl();
			ReportsGridFieldsLayoutUserControl.Dock = DockStyle.Fill;
			ReportsGridFieldsLayoutUserControl.CaptionRenderingEnabled = true;
			BindingSource.SetBindingMember(ReportsGridFieldsLayoutUserControl, ".");
			BottomGroupBox.Controls.Add(ReportsGridFieldsLayoutUserControl);
		}
	}
}
