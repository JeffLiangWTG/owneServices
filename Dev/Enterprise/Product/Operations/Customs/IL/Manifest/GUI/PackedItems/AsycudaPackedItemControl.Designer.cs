namespace Enterprise.Customs.IL.Manifest.GUI
{
	partial class AsycudaPackedItemControl
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
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Splitter = new CargoWise.Windows.UI.KSplitter();
			this.PackedItemTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.PackedItemDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PackedItemDetailsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.PackPackedItemPivotTabPage = new ZArchitecture.GUI.ZTabPage();
			this.AdditionalInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalInformationLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PackedItemTabControl.SuspendLayout();
			this.PackedItemDetailsTabPage.SuspendLayout();
			this.AdditionalInfoTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Manifest.Business.AsycudaBill);
			// 
			// TopPanel
			// 
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 200, true);
			this.TopPanel.TabIndex = 0;
			// 
			// Splitter
			// 
			this.Splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Splitter.DoNotSaveSplitterLayout = false;
			this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 201, true);
			this.Splitter.Name = "Splitter";
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 3, true);
			this.Splitter.TabIndex = 1;
			this.Splitter.TabStop = false;
			// 
			// PackedItemTabControl
			// 
			this.PackedItemTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PackedItemTabControl.Controls.Add(this.PackedItemDetailsTabPage);
			this.PackedItemTabControl.Controls.Add(this.PackPackedItemPivotTabPage);
			this.PackedItemTabControl.Controls.Add(this.AdditionalInfoTabPage);
			this.PackedItemTabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PackedItemTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 204, true);
			this.PackedItemTabControl.Name = "PackedItemTabControl";
			this.PackedItemTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 200, true);
			this.PackedItemTabControl.TabIndex = 2;
			// 
			// PackedItemDetailsTabPage
			//
			this.PackedItemDetailsTabPage.CaptionResourceString = Enterprise.Customs.IL.Manifest.GUI.Res.GetData("1DBAC8CE-E6FD-4F51-8692-2F5F907D7549", "Packed Item Details");
			this.PackedItemDetailsTabPage.Controls.Add(this.PackedItemDetailsLayoutPanel);
			this.PackedItemDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.PackedItemDetailsTabPage.Name = "PackedItemDetailsTabPage";
			this.PackedItemDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackedItemDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 177, true);
			this.PackedItemDetailsTabPage.TabIndex = 1;
			// 
			// PackedItemDetailsLayoutPanel
			// 
			this.PackedItemDetailsLayoutPanel.AllowDrop = true;
			this.PackedItemDetailsLayoutPanel.AutoScroll = true;
			this.PackedItemDetailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackedItemDetailsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackedItemDetailsLayoutPanel.Name = "PackedItemDetailsLayoutPanel";
			this.PackedItemDetailsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 180, true);
			this.PackedItemDetailsLayoutPanel.TabIndex = 2;
			// 
			// AdditionalInfoTabPage
			// 
			this.AdditionalInfoTabPage.CaptionResourceString = Enterprise.Customs.IL.Manifest.GUI.Res.GetData("21911448-3ED9-4CE6-83A9-BAC4035CE730", "Additional Information");
			this.AdditionalInfoTabPage.Controls.Add(this.AdditionalInformationLayoutPanel);
			this.AdditionalInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.AdditionalInfoTabPage.Name = "AdditionalInfoTabPage";
			this.AdditionalInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1282, 257, true);
			this.AdditionalInfoTabPage.TabIndex = 3;
			// 
			// AdditionalInformationLayoutPanel
			// 
			this.AdditionalInformationLayoutPanel.AllowDrop = true;
			this.AdditionalInformationLayoutPanel.AutoScroll = true;
			this.AdditionalInformationLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInformationLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInformationLayoutPanel.Name = "AdditionalInformationLayoutPanel";
			this.AdditionalInformationLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1282, 257, true);
			this.AdditionalInformationLayoutPanel.TabIndex = 4;
			// 
			// PackPackedItemPivotTabPage
			// 
			this.PackPackedItemPivotTabPage.CaptionResourceString = Enterprise.Customs.IL.Manifest.GUI.Res.GetData("C973ECEC-E2A6-4C24-A952-7AB8E19349AB", "Packs");
			this.PackPackedItemPivotTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.PackPackedItemPivotTabPage.Name = "PackPackedItemPivotTabPage";
			this.PackPackedItemPivotTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackPackedItemPivotTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 177, true);
			this.PackPackedItemPivotTabPage.TabIndex = 2;
			this.PackPackedItemPivotTabPage.UseVisualStyleBackColor = true;
			// 
			// AsycudaPackedItemControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.Splitter);
			this.Controls.Add(this.PackedItemTabControl);
			this.Name = "AsycudaPackedItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 403, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackedItemTabControl.ResumeLayout(false);
			this.PackedItemTabControl.PerformLayout();
			this.AdditionalInfoTabPage.ResumeLayout(false);
			this.AdditionalInfoTabPage.PerformLayout();
			this.PackedItemDetailsTabPage.ResumeLayout(false);
			this.PackedItemDetailsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		private CargoWise.Windows.UI.KSplitter Splitter;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl PackedItemTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage PackedItemDetailsTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel PackedItemDetailsLayoutPanel;
		private ZArchitecture.GUI.ZTabPage PackPackedItemPivotTabPage;
		private ZArchitecture.GUI.ZTabPage AdditionalInfoTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel AdditionalInformationLayoutPanel;
	}
}
