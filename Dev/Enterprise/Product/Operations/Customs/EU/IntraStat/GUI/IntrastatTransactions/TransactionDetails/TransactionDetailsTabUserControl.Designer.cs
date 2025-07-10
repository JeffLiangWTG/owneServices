namespace Enterprise.Customs.EU.Intrastat.GUI
{
	partial class TransactionDetailsTabUserControl
	{
		private void InitializeComponent()
		{
			this.DynamicOrganisationDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.DynamicTransactionDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.TransactionDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransactionDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader);
			// 
			// DynamicOrganisationDetailsPanel
			// 
			this.DynamicOrganisationDetailsPanel.AllowDrop = true;
			this.DynamicOrganisationDetailsPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.DynamicOrganisationDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicOrganisationDetailsPanel.Name = "DynamicOrganisationDetailsPanel";
			this.DynamicOrganisationDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 653, true);
			this.DynamicOrganisationDetailsPanel.TabIndex = 0;
			// 
			// DynamicTransactionDetailsPanel
			// 
			this.DynamicTransactionDetailsPanel.AllowDrop = true;
			this.DynamicTransactionDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicTransactionDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicTransactionDetailsPanel.Name = "DynamicTransactionDetailsPanel";
			this.DynamicTransactionDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 631, true);
			this.DynamicTransactionDetailsPanel.TabIndex = 0;
			// 
			// TransactionDetailsGroupBox
			// 
			this.TransactionDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left)));
			this.TransactionDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.Intrastat.GUI.Res.GetData("b2cdb9e8-d0a9-4265-9551-ac77483adaaa", "Transaction Details");
			this.TransactionDetailsGroupBox.Controls.Add(this.DynamicTransactionDetailsPanel);
			this.TransactionDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 0, true);
			this.TransactionDetailsGroupBox.Name = "TransactionDetailsGroupBox";
			this.TransactionDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 650, true);
			this.TransactionDetailsGroupBox.TabIndex = 1;
			this.TransactionDetailsGroupBox.TabStop = false;
			// 
			// TransactionDetailsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DynamicOrganisationDetailsPanel);
			this.Controls.Add(this.TransactionDetailsGroupBox);
			this.Name = "TransactionDetailsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1248, 653, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransactionDetailsGroupBox.ResumeLayout(false);
			this.TransactionDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicOrganisationDetailsPanel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox TransactionDetailsGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicTransactionDetailsPanel;
	}
}
