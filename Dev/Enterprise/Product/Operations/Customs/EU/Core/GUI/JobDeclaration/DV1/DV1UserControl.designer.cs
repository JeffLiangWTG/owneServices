namespace Enterprise.Customs.EU.GUI
{
	partial class DV1UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.DV1GridUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.DV1TabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DV1DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DynamicDV1DetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
            // 
            // SplitContainer
            // 
            this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SplitContainer.Name = "SplitContainer";
            this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // SplitContainer.Panel1
            // 
             this.SplitContainer.Panel1.Controls.Add(this.DV1GridUserControl);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.DV1TabControl);
            this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1361, 502, true);
            this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(177);
            this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(324);
            this.SplitContainer.TabIndex = 0;
			// 
			// DV1GridUserControl
			// 
			this.DV1GridUserControl.AllowDrop = true;
			this.DV1GridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DV1GridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DV1GridUserControl.Name = "DV1GridUserControl";
			this.DV1GridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1337, 291, true);
			this.DV1GridUserControl.TabIndex = 1;
			// 
			// DV1TabControl
			// 
			this.DV1TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DV1TabControl.Controls.Add(this.DV1DetailsTabPage);
			this.DV1TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DV1TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DV1TabControl.Name = "DV1TabControl";
			this.DV1TabControl.SelectedIndex = 0;
			this.DV1TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1337, 432, true);
			this.DV1TabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DV1DetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("1D28C28B-DBEC-4A16-8EB0-D2304C11D23A", "Details");
			this.DV1DetailsTabPage.Controls.Add(this.DynamicDV1DetailsPanel);
			this.DV1DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DV1DetailsTabPage.Name = "DV1DetailsTabPage";
			this.DV1DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.DV1DetailsTabPage.TabIndex = 0;
			// 
			// DynamicDV1DetailsPanel
			// 
			this.DynamicDV1DetailsPanel.AllowDrop = true;
			this.DynamicDV1DetailsPanel.AutoScroll = true;
			this.DynamicDV1DetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicDV1DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DynamicDV1DetailsPanel.Name = "DynamicDV1DetailsPanel";
			this.DynamicDV1DetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicDV1DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.DynamicDV1DetailsPanel.TabIndex = 1;
			this.DynamicDV1DetailsPanel.CaptionRenderingEnabled = true;
			// 
			// DV1UserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.SplitContainer);
            this.Name = "DV1DetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1361, 502, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.DV1TabControl.ResumeLayout(false);
			this.DV1TabControl.PerformLayout();
			this.DV1DetailsTabPage.ResumeLayout(false);
			this.DV1DetailsTabPage.PerformLayout();
			this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer SplitContainer;
		ZArchitecture.GUI.ZDynamicControlCreationUserControl DV1GridUserControl;
		Enterprise.ZArchitecture.GUI.ZTabControl DV1TabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage DV1DetailsTabPage;
		Enterprise.ZArchitecture.GUI.DynamicLayoutPanel DynamicDV1DetailsPanel;
	}
}
