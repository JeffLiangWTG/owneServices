using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCargoOutturnBillsForm
	{
		new void InitializeComponent()
		{
			this.underbondControl = new Enterprise.Customs.AU.AirCargo.GUI.AirCargoOutturnBillsControl();
			this.workflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.workflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 480, true);
			this.MainTabControl.Controls.SetChildIndex(this.workflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.underbondControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 453, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1001);
			// 
			// UnderbondControl
			// 
			this.underbondControl.CurrentUnderbond = null;
			this.underbondControl.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.underbondControl.DataSourceTypeName = "Enterprise.Customs.Business.CusUnderbondUnionCollectionParentCollection";
			this.underbondControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.underbondControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.underbondControl.Name = "UnderbondControl";
			this.underbondControl.OutturnDisabled = false;
			this.underbondControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 453, true);
			this.underbondControl.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.workflowTabPage.CheckForNotifications = true;
			this.workflowTabPage.ExcludeFromBindingOnSave = true;
			this.workflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.workflowTabPage.Name = "WorkflowTabPage";
			this.workflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 453, true);
			this.workflowTabPage.TabIndex = 3;
			// 
			// AirCargoOutturnBillsForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 586, true);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusUnderbond";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 570, true);
			this.Name = "AirCargoOutturnBillsForm";
			this.Text = "AirCargoStandAloneUnderbond";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}

		private AirCargoOutturnBillsControl underbondControl;
		private ZWorkflowTabPage workflowTabPage;
	}
}
