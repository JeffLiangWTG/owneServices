namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class UnloadingRemarksUserControl
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
			this.components = new System.ComponentModel.Container();
			this.UnloadingRemarksTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.HeaderDifferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GoodsItemDifferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnloadingRemarksTabControl.SuspendLayout();
			this.HeaderDifferencesTabPage.SuspendLayout();
			this.GoodsItemDifferencesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// UnloadingRemarksTabControl
			// 
			this.UnloadingRemarksTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.UnloadingRemarksTabControl.Controls.Add(this.HeaderDifferencesTabPage);
			this.UnloadingRemarksTabControl.Controls.Add(this.GoodsItemDifferencesTabPage);
			this.UnloadingRemarksTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingRemarksTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnloadingRemarksTabControl.Name = "UnloadingRemarksTabControl";
			this.UnloadingRemarksTabControl.SelectedIndex = 0;
			this.UnloadingRemarksTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1131, 756, true);
			this.UnloadingRemarksTabControl.TabIndex = 0;
			// 
			// HeaderDifferencesTabPage
			// 
			this.HeaderDifferencesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.HeaderDifferencesTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("33b7a24b-8b8d-4803-be51-a65f1b02c608", "Unloading Differences");
			this.HeaderDifferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HeaderDifferencesTabPage.Name = "HeaderDifferencesTabPage";
			this.HeaderDifferencesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HeaderDifferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 729, true);
			this.HeaderDifferencesTabPage.TabIndex = 1;
			this.HeaderDifferencesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.HeaderDifferencesTabPage_InitializeTab));
			// 
			// GoodsItemDifferencesTabPage
			//
			this.GoodsItemDifferencesTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("ea567217-cc27-45e9-8faa-0f6799286b9f", "Goods Item Differences");
			this.GoodsItemDifferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GoodsItemDifferencesTabPage.Name = "GoodsItemDifferencesTabPage";
			this.GoodsItemDifferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 729, true);
			this.GoodsItemDifferencesTabPage.TabIndex = 2;
			this.GoodsItemDifferencesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ItemDifferencesTabPage_InitializeTab));
			// 
			// UnloadingRemarksUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnloadingRemarksTabControl);
			this.Name = "UnloadingRemarksUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1131, 756, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnloadingRemarksTabControl.ResumeLayout(false);
			this.UnloadingRemarksTabControl.PerformLayout();
			this.HeaderDifferencesTabPage.ResumeLayout(false);
			this.HeaderDifferencesTabPage.PerformLayout();
			this.GoodsItemDifferencesTabPage.ResumeLayout(false);
			this.GoodsItemDifferencesTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl UnloadingRemarksTabControl;
		private ZArchitecture.GUI.ZTabPage HeaderDifferencesTabPage;
		private ZArchitecture.GUI.ZTabPage GoodsItemDifferencesTabPage;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl UnloadingHeaderDifferencesDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl UnloadingItemDifferencesDynamicUserControl;
	}
}

