namespace Enterprise.Customs.EU.Intrastat.GUI.Transactions
{
	public partial class IntrastatTransactionForm
	{
		new void InitializeComponent()
		{
			this.TransactionDetailsTabUserControl = new Enterprise.Customs.EU.Intrastat.GUI.TransactionDetailsTabUserControl();
			this.TransactionLineDetailsTabUserControl = new Enterprise.Customs.EU.Intrastat.GUI.TransactionLineDetailsTabUserControl();
			this.LinesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransactionDetailsTabUserControl.SuspendLayout();
			this.TransactionLineDetailsTabUserControl.SuspendLayout();
			this.LinesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.LinesTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1351, 630, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LinesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.TransactionDetailsTabUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1343, 603, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1343, 603, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1343, 603, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1351, 630, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1351, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader);
			// 
			// TransactionDetailsTabUserControl
			// 
			this.TransactionDetailsTabUserControl.AllowDrop = true;
			this.TransactionDetailsTabUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.TransactionDetailsTabUserControl, ".");
			this.TransactionDetailsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransactionDetailsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransactionDetailsTabUserControl.Name = "TransactionDetailsTabUserControl";
			this.TransactionDetailsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1343, 603, true);
			this.TransactionDetailsTabUserControl.TabIndex = 0;
			// 
			// TransactionLineDetailsTabUserControl
			// 
			this.TransactionLineDetailsTabUserControl.AllowDrop = true;
			this.TransactionLineDetailsTabUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.TransactionLineDetailsTabUserControl, "CusIntrastatLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Intrastat.Business.ICusIntrastatLineCollection<Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine>)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader)(null)).CusIntrastatLines)));
			this.TransactionLineDetailsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransactionLineDetailsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TransactionLineDetailsTabUserControl.Name = "TransactionLineDetailsTabUserControl";
			this.TransactionLineDetailsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1337, 597, true);
			this.TransactionLineDetailsTabUserControl.TabIndex = 0;
			// 
			// LinesTabPage
			// 
			this.LinesTabPage.Controls.Add(this.TransactionLineDetailsTabUserControl);
			this.LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LinesTabPage.Name = "LinesTabPage";
			this.LinesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1343, 603, true);
			this.LinesTabPage.TabIndex = 3;
			this.LinesTabPage.UseVisualStyleBackColor = true;
			// 
			// IntrastatTransactionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1351, 686, true);
			this.DataSourceType = typeof(Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 725, true);
			this.Name = "IntrastatTransactionForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransactionDetailsTabUserControl.ResumeLayout(true);
			this.TransactionDetailsTabUserControl.PerformLayout();
			this.TransactionLineDetailsTabUserControl.ResumeLayout(true);
			this.TransactionLineDetailsTabUserControl.PerformLayout();
			this.LinesTabPage.ResumeLayout(false);
			this.LinesTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal EU.Intrastat.GUI.TransactionDetailsTabUserControl TransactionDetailsTabUserControl;
		internal ZArchitecture.GUI.ZTabPage LinesTabPage;
		internal TransactionLineDetailsTabUserControl TransactionLineDetailsTabUserControl;
	}
}
