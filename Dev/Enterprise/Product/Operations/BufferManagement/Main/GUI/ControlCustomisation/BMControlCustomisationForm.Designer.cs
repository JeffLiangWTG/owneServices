namespace Enterprise.BufferManagement.GUI
{
	partial class BMControlCustomisationForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CustomisedControlDetailsUserControl = new Enterprise.BufferManagement.GUI.CustomisedControlDetailsUserControl();
			this.ControlUsagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ControlUsagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ControlUsagesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ControlUsagesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ControlUsagesTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 544, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ControlUsagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.CustomisedControlDetailsUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 517, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 517, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 544, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMControlCustomisation);
			// 
			// CustomisedControlDetailsUserControl
			// 
			this.CustomisedControlDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomisedControlDetailsUserControl, ".");
			this.CustomisedControlDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomisedControlDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomisedControlDetailsUserControl.Name = "CustomisedControlDetailsUserControl";
			this.CustomisedControlDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 517, true);
			this.CustomisedControlDetailsUserControl.TabIndex = 0;
			// 
			// ControlUsagesTabPage
			// 
			this.ControlUsagesTabPage.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("b6041c0e-9c77-474d-b06c-3caf658bc752", "Layout Usages");
			this.ControlUsagesTabPage.Controls.Add(this.ControlUsagesGrid);
			this.ControlUsagesTabPage.LicenceCheckpoint = null;
			this.ControlUsagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ControlUsagesTabPage.Name = "ControlUsagesTabPage";
			this.ControlUsagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 517, true);
			this.ControlUsagesTabPage.TabIndex = 3;
			// 
			// ControlUsagesGrid
			// 
			this.ControlUsagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ControlUsagesGrid, "ControlUsages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMControlCustomisation)(null)).ControlUsages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMControlCustomisationLink)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMControlCustomisation)(null)).ControlUsages)).SyncRoot)).UsageDescription)));
			this.ControlUsagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "UsageDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(600);
			this.ControlUsagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ControlUsagesGrid.CopySelectedRowsAllowed = true;
			this.ControlUsagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ControlUsagesGrid.GridId = "f12044f0-4da4-408a-913a-c2db32e8f1fa";
			this.ControlUsagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ControlUsagesGrid.LayoutKey = "ControlUsagesGrid";
			this.ControlUsagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ControlUsagesGrid.Name = "ControlUsagesGrid";
			this.ControlUsagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 517, true);
			this.ControlUsagesGrid.TabIndex = 0;
			// 
			// BMControlCustomisationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 600, true);
			this.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMControlCustomisation);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 520, true);
			this.Name = "BMControlCustomisationForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ControlUsagesTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ControlUsagesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CustomisedControlDetailsUserControl CustomisedControlDetailsUserControl;
		private ZArchitecture.GUI.ZTabPage ControlUsagesTabPage;
		private ZArchitecture.ZGrid ControlUsagesGrid;
	}
}