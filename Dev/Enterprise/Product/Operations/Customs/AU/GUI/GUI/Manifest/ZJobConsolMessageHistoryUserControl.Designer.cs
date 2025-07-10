namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class ZManifestMessageHistoryUserControl
	{
		private void InitializeComponent()
		{
			this.contingencyCANLabel = new Enterprise.ZArchitecture.ZLabel();
			this.contingencyCANTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.depotPremiseIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.depotPremiseIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TopPanel.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			this.MessageTextGroupBox.SuspendLayout();
			this.HistoryPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessageTextPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.depotPremiseIDTextBox);
			this.TopPanel.Controls.Add(this.depotPremiseIDLabel);
			this.TopPanel.Controls.Add(this.contingencyCANTextBox);
			this.TopPanel.Controls.Add(this.contingencyCANLabel);
			this.TopPanel.TabIndex = 0;
			this.TopPanel.Controls.SetChildIndex(this.contingencyCANLabel, 0);
			this.TopPanel.Controls.SetChildIndex(this.contingencyCANTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.CustomsEntryNumberTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.E2_MessageStatusBoundTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.CRNLabel, 0);
			this.TopPanel.Controls.SetChildIndex(this.StatusLabel, 0);
			this.TopPanel.Controls.SetChildIndex(this.depotPremiseIDLabel, 0);
			this.TopPanel.Controls.SetChildIndex(this.depotPremiseIDTextBox, 0);
			// 
			// CRNLabel
			// 
			this.CRNLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 23, true);
			this.CRNLabel.TabIndex = 0;
			// 
			// StatusLabel
			// 
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 9, true);
			this.StatusLabel.TabIndex = 6;
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.TabIndex = 0;
			// 
			// TheSplitter
			// 
			this.TheSplitter.TabIndex = 1;
			// 
			// E2_MessageStatusBoundTextBox
			// 
			this.E2_MessageStatusBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.E2_MessageStatusBoundTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
			this.E2_MessageStatusBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(677, 9, true);
			this.E2_MessageStatusBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 20, true);
			this.E2_MessageStatusBoundTextBox.TabIndex = 7;
			// 
			// CustomsEntryNumberTextBox
			// 
			this.CustomsEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 9, true);
			this.CustomsEntryNumberTextBox.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.AUCustomsManifestStatus);
			// 
			// ContingencyCANLabel
			// 
			this.contingencyCANLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 8, true);
			this.contingencyCANLabel.Name = "ContingencyCANLabel";
			this.contingencyCANLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.contingencyCANLabel.TabIndex = 2;
			this.contingencyCANLabel.Text = "Contingency CRN:";
			// 
			// ContingencyCANTextBox
			// 
			this.BindingSource.SetBindingMember(this.contingencyCANTextBox, "ContingencyCAN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUCustomsManifestStatus)(null)).ContingencyCAN)));
			this.contingencyCANTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 9, true);
			this.contingencyCANTextBox.Name = "ContingencyCANTextBox";
			this.contingencyCANTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.contingencyCANTextBox.TabIndex = 3;
			// 
			// DepotPremiseIDLabel
			// 
			this.depotPremiseIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 8, true);
			this.depotPremiseIDLabel.Name = "DepotPremiseIDLabel";
			this.depotPremiseIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 23, true);
			this.depotPremiseIDLabel.TabIndex = 4;
			this.depotPremiseIDLabel.Text = "Depot PremiseID:";
			// 
			// DepotPremiseIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.depotPremiseIDTextBox, "PremisesID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUCustomsManifestStatus)(null)).PremisesID)));
			this.depotPremiseIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 9, true);
			this.depotPremiseIDTextBox.Name = "DepotPremiseIDTextBox";
			this.depotPremiseIDTextBox.ReadOnly = true;
			this.depotPremiseIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.depotPremiseIDTextBox.TabIndex = 5;
			// 
			// ZManifestMessageHistoryUserControl
			// 
			this.Name = "ZManifestMessageHistoryUserControl";
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.HistoryGroupBox.ResumeLayout(false);
			this.MessageTextGroupBox.ResumeLayout(false);
			this.MessageTextGroupBox.PerformLayout();
			this.HistoryPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessageTextPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		private ZArchitecture.ZLabel contingencyCANLabel;
		private ZArchitecture.ZTextBox contingencyCANTextBox;
		private ZArchitecture.ZLabel depotPremiseIDLabel;
		private ZArchitecture.ZTextBox depotPremiseIDTextBox;
	}
}
