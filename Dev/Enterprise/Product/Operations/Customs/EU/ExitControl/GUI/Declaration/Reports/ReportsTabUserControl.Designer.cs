namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ReportsTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ReportsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ReportTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ReportItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportsSplitContainer)).BeginInit();
			this.ReportsSplitContainer.Panel2.SuspendLayout();
			this.ReportsSplitContainer.SuspendLayout();
			this.ReportTabControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ExitControlBase.Business.ICusExitReportCollection<Enterprise.Customs.EU.ExitControl.Business.CusExitReport>);
			// 
			// ReportsSplitContainer
			// 
			this.ReportsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportsSplitContainer.Name = "ReportsSplitContainer";
			this.ReportsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.ReportsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1139, 449, true);
			this.ReportsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(205);
			// 
			// ReportsSplitContainer.Panel2
			// 
			this.ReportsSplitContainer.Panel2.Controls.Add(this.ReportTabControl);
			this.ReportsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(205);
			this.ReportsSplitContainer.TabIndex = 0;
			// 
			// ReportTabControl
			// 
			this.ReportTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ReportTabControl.Controls.Add(this.ReportItemsTabPage);
			this.ReportTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportTabControl.Name = "ReportTabControl";
			this.ReportTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1139, 240, true);
			this.ReportTabControl.TabIndex = 0;
			// 
			// ReportItemsTabPage
			// 
			this.ReportItemsTabPage.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("538D036F-7349-4497-A139-AA793EF405E3", "Items");
			this.ReportItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ReportItemsTabPage.Name = "ReportItemsTabPage";
			this.ReportItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1131, 213, true);
			this.ReportItemsTabPage.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("a140f373-17fe-4b56-8948-39d3f9096433", "Messages");
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1130, 252, true);
			this.MessagesTabPage.TabIndex = 1;
			this.MessagesTabPage.UseVisualStyleBackColor = true;
			// 
			// ReportsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReportsSplitContainer);
			this.Name = "ReportsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1139, 449, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReportsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ReportsSplitContainer)).EndInit();
			this.ReportsSplitContainer.ResumeLayout(false);
			this.ReportsSplitContainer.PerformLayout();
			this.ReportTabControl.ResumeLayout(false);
			this.ReportTabControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer ReportsSplitContainer;
		internal ZArchitecture.GUI.ZTabControl ReportTabControl;
		internal ZArchitecture.GUI.ZTabPage ReportItemsTabPage;
		private System.ComponentModel.IContainer components;
		internal ZArchitecture.GUI.ZTabPage MessagesTabPage;
	}
}

