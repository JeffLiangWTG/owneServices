using Enterprise.Customs.EU.Intrastat.Business;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	partial class TransactionLineDetailsTabUserControl
	{
		private void InitializeComponent()
		{
			this.TransactionLinesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TransactionLineTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.TransactionLineDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DynamicTransactionLineDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TransactionLinesSplitContainer)).BeginInit();
			this.TransactionLinesSplitContainer.Panel1.SuspendLayout();
			this.TransactionLinesSplitContainer.Panel2.SuspendLayout();
			this.TransactionLinesSplitContainer.SuspendLayout();
			this.TransactionLineTabControl.SuspendLayout();
			this.TransactionLineDetailsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Intrastat.Business.ICusIntrastatLineCollection<Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine>);
			// 
			// TransactionLinesSplitContainer
			// 
			this.TransactionLinesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransactionLinesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransactionLinesSplitContainer.Name = "TransactionLinesSplitContainer";
			this.TransactionLinesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.TransactionLinesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 490, true);
			// 
			// TransactionLinesSplitContainer.Panel1
			// 
			this.TransactionLinesSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(175);
			// 
			// TransactionLinesSplitContainer.Panel2
			// 
			this.TransactionLinesSplitContainer.Panel2.Controls.Add(this.TransactionLineTabControl);
			this.TransactionLinesSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(175);
			this.TransactionLinesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(490);
			this.TransactionLinesSplitContainer.SplitterWidth = 5;
			this.TransactionLinesSplitContainer.TabIndex = 0;
			// 
			// TransactionLineTabControl
			// 
			this.TransactionLineTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TransactionLineTabControl.Controls.Add(this.TransactionLineDetailsTabPage);
			this.TransactionLineTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransactionLineTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransactionLineTabControl.Name = "TransactionLineTabControl";
			this.TransactionLineTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 326, true);
			this.TransactionLineTabControl.TabIndex = 0;
			// 
			// TransactionLineDetailsTabPage
			// 
			this.TransactionLineDetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.Intrastat.GUI.Res.GetData("13d88695-bd5b-4b1c-80c8-d691cee99b1e", "Details");
			this.TransactionLineDetailsTabPage.Controls.Add(this.DynamicTransactionLineDetailsPanel);
			this.TransactionLineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TransactionLineDetailsTabPage.Name = "TransactionLineDetailsTabPage";
			this.TransactionLineDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TransactionLineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 292, true);
			this.TransactionLineDetailsTabPage.TabIndex = 0;
			// 
			// DynamicTransactionLineDetailsPanel
			// 
			this.DynamicTransactionLineDetailsPanel.AllowDrop = true;
			this.DynamicTransactionLineDetailsPanel.AutoScroll = true;
			this.DynamicTransactionLineDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicTransactionLineDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DynamicTransactionLineDetailsPanel.Name = "DynamicTransactionLineDetailsPanel";
			this.DynamicTransactionLineDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 286, true);
			this.DynamicTransactionLineDetailsPanel.TabIndex = 0;
			// 
			// Phase5TransactionLinesTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransactionLinesSplitContainer);
			this.Name = "Phase5TransactionLinesTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 490, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransactionLinesSplitContainer.Panel1.ResumeLayout(false);
			this.TransactionLinesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TransactionLinesSplitContainer)).EndInit();
			this.TransactionLinesSplitContainer.ResumeLayout(false);
			this.TransactionLinesSplitContainer.PerformLayout();
			this.TransactionLineTabControl.ResumeLayout(false);
			this.TransactionLineTabControl.PerformLayout();
			this.TransactionLineDetailsTabPage.ResumeLayout(false);
			this.TransactionLineDetailsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal CargoWise.Windows.UI.KSplitContainer TransactionLinesSplitContainer;
		public ZArchitecture.GUI.ZTabControl TransactionLineTabControl;
		internal ZArchitecture.GUI.ZTabPage TransactionLineDetailsTabPage;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicTransactionLineDetailsPanel;
	}
}
