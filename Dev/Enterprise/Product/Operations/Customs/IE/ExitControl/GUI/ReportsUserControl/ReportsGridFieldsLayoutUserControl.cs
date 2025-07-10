using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public partial class ReportsGridFieldsLayoutUserControl : EU.ExitControl.GUI.ReportsGridFieldsLayoutUserControl
	{
		public ReportsGridFieldsLayoutUserControl()
		{
			InitializeComponent();
		}

		protected override IPanelLayoutProvider GetLayout() => new ReportsGridFieldsLayout();
	}
}
