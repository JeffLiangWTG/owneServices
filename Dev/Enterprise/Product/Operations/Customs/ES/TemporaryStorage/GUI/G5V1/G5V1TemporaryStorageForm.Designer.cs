namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class G5V1TemporaryStorageForm
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
			this.BillPartiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BillPartiesLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.PacksTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PacksLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.PackedItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PackedItemsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BillPartiesTabPage.SuspendLayout();
			this.PacksTabPage.SuspendLayout();
			this.PacksLayoutPanel.SuspendLayout();
			this.PackedItemsTabPage.SuspendLayout();
			this.PackedItemsLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// BillPartiesTabPage
			// 
			this.BillPartiesTabPage.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("D653D64B-BFD3-468A-A785-93CEF1AC5D6A", "Organizations");
			this.BillPartiesTabPage.Controls.Add(this.BillPartiesLayoutPanel);
			this.BillPartiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.BillPartiesTabPage.Name = "BillPartiesTabPage";
			this.BillPartiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1283, 369, true);
			this.BillPartiesTabPage.TabIndex = 1;
			this.BillPartiesTabPage.UseVisualStyleBackColor = true;
			// 
			// BillPartiesLayoutPanel
			// 
			this.BillPartiesLayoutPanel.AllowDrop = true;
			this.BillPartiesLayoutPanel.AutoScroll = true;
			this.BillPartiesLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillPartiesLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillPartiesLayoutPanel.Name = "BillPartiesLayoutPanel";
			this.BillPartiesLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2562, 747, true);
			this.BillPartiesLayoutPanel.TabIndex = 2;
			// 
			// PacksTabPage
			// 
			this.PacksTabPage.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("4C204B18-D660-4948-A746-791FD8DCBA01", "Packs");
			this.PacksTabPage.Controls.Add(this.PacksLayoutPanel);
			this.PacksTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.PacksTabPage.Name = "PacksTabPage";
			this.PacksTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PacksTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 366, true);
			this.PacksTabPage.TabIndex = 3;
			this.PacksTabPage.UseVisualStyleBackColor = true;
			// 
			// PacksLayoutPanel
			// 
			this.PacksLayoutPanel.AllowDrop = true;
			this.PacksLayoutPanel.AutoScroll = true;
			this.PacksLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PacksLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PacksLayoutPanel.Name = "PacksLayoutPanel";
			this.PacksLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1275, 360, true);
			this.PacksLayoutPanel.TabIndex = 2;
			// 
			// PackedItemsTabPage
			// 
			this.PackedItemsTabPage.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("655A3DCB-ACA7-4A8D-845C-BC74F9068BCC", "Items");
			this.PackedItemsTabPage.Controls.Add(this.PackedItemsLayoutPanel);
			this.PackedItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.PackedItemsTabPage.Name = "PackedItemsTabPage";
			this.PackedItemsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackedItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 366, true);
			this.PackedItemsTabPage.TabIndex = 3;
			this.PackedItemsTabPage.UseVisualStyleBackColor = true;
			// 
			// PackedItemsLayoutPanel
			// 
			this.PackedItemsLayoutPanel.AllowDrop = true;
			this.PackedItemsLayoutPanel.AutoScroll = true;
			this.PackedItemsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackedItemsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PackedItemsLayoutPanel.Name = "PackedItemsLayoutPanel";
			this.PackedItemsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1275, 360, true);
			this.PackedItemsLayoutPanel.TabIndex = 2;
			// 
			// G5V1TemporaryStorageForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 656, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 695, true);
			this.Name = "G5V1TemporaryStorageForm";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BillPartiesTabPage.ResumeLayout(false);
			this.BillPartiesTabPage.PerformLayout();
			this.BillPartiesLayoutPanel.ResumeLayout(false);
			this.BillPartiesLayoutPanel.PerformLayout();
			this.PacksTabPage.ResumeLayout(false);
			this.PacksTabPage.PerformLayout();
			this.PacksLayoutPanel.ResumeLayout(false);
			this.PacksLayoutPanel.PerformLayout();
			this.PackedItemsTabPage.ResumeLayout(false);
			this.PackedItemsTabPage.PerformLayout();
			this.PackedItemsLayoutPanel.ResumeLayout(false);
			this.PackedItemsLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTabPage BillPartiesTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel BillPartiesLayoutPanel;
		private ZArchitecture.GUI.ZTabPage PacksTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel PacksLayoutPanel;
		private ZArchitecture.GUI.ZTabPage PackedItemsTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel PackedItemsLayoutPanel;
	}
}
