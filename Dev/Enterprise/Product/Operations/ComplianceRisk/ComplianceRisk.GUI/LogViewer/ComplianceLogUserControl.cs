using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI
{
	public partial class ComplianceLogUserControl : ZUserControl
	{
		public ComplianceLogUserControl()
		{
			InitializeComponent();
		}

		public ComplianceLogUserControl(ZUserControl dpsLogsUserControl, bool showRemovedCommoditiesTabPage = true) : this()
		{
			removedCommoditiesTabPage.TabVisible = showRemovedCommoditiesTabPage;
			AddDpsLogsTabPageIfNeeded(dpsLogsUserControl);
		}

		void AddDpsLogsTabPageIfNeeded(ZUserControl dpsLogsUserControl)
		{
			var dpsLogsTabPage = new ZTabPage();
			dpsLogsTabPage.ExcludeFromBindingOnSave = true;
			dpsLogsTabPage.Name = "dpsLogsTabPage";
			dpsLogsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			dpsLogsTabPage.TabIndex = 2;
			dpsLogsTabPage.Text = Res.GetString("7d17a343-6816-49b9-99d3-9713dc6484d8", "Denied Party Screening Logs");
			dpsLogsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			dpsLogsUserControl.TabIndex = 0;
			dpsLogsTabPage.Controls.Add(dpsLogsUserControl);

			complianceLogControl.Controls.Add(dpsLogsTabPage);
		}
	}
}
