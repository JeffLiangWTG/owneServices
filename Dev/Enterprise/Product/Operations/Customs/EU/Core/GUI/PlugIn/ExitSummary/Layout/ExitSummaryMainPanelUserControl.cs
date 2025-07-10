using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class ExitSummaryMainPanelUserControl : ZUserControl
	{
		public ExitSummaryMainPanelUserControl()
		{
			InitializeComponent();
		}

		public void SetExitSummaryMainPanelLayout(IPanelLayoutProvider layout)
		{
			DynamicExitSummaryPanel.UpdateLayout(layout);
		}
	}
}
