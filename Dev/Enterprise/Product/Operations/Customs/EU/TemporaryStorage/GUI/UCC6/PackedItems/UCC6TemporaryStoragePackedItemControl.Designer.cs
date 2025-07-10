using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class UCC6TemporaryStoragePackedItemControl
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
			this.PackPackedItemPivotTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalInformationLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.SupplyChainActorTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupplyChainActorTabUserControl = new Enterprise.Customs.EU.TemporaryStorage.GUI.SupplyChainActorTabUserControl();
			this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupportingDocumentsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.PreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PreviousDocumentsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PackedItemTabControl.SuspendLayout();
			this.PackedItemDetailsTabPage.SuspendLayout();
			this.AdditionalInfoTabPage.SuspendLayout();
			this.SupplyChainActorTabPage.SuspendLayout();
			this.SupplyChainActorTabUserControl.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			this.SupportingDocumentsLayoutPanel.SuspendLayout();
			this.PreviousDocumentsTabPage.SuspendLayout();
			this.PreviousDocumentsLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// TopPanel
			// 
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 243, true);
			this.TopPanel.TabIndex = 0;
			// 
			// Splitter
			// 
			this.Splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Splitter.DoNotSaveSplitterLayout = false;
			this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 243, true);
			this.Splitter.Name = "Splitter";
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 5, true);
			this.Splitter.TabIndex = 1;
			this.Splitter.TabStop = false;
			// 
			// PackedItemTabControl
			// 
			this.PackedItemTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PackedItemTabControl.Controls.Add(this.PackedItemDetailsTabPage);
			this.PackedItemTabControl.Controls.Add(this.PackPackedItemPivotTabPage);
			this.PackedItemTabControl.Controls.Add(this.SupportingDocumentsTabPage);
			this.PackedItemTabControl.Controls.Add(this.PreviousDocumentsTabPage);
			this.PackedItemTabControl.Controls.Add(this.AdditionalInfoTabPage);
			this.PackedItemTabControl.Controls.Add(this.SupplyChainActorTabPage);
			this.PackedItemTabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PackedItemTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 253, true);
			this.PackedItemTabControl.Name = "PackedItemTabControl";
			this.PackedItemTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 280, true);
			this.PackedItemTabControl.TabIndex = 2;
			// 
			// PackedItemDetailsTabPage
			// 
			this.PackedItemDetailsTabPage.CaptionResourceString = Res.GetData("ac144f23-2361-4386-8245-8c22ef86cfa0", "Packed Item Details");
			this.PackedItemDetailsTabPage.Controls.Add(this.PackedItemDetailsLayoutPanel);
			this.PackedItemDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PackedItemDetailsTabPage.Name = "PackedItemDetailsTabPage";
			this.PackedItemDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 304, true);
			this.PackedItemDetailsTabPage.TabIndex = 1;
			// 
			// PackedItemDetailsLayoutPanel
			// 
			this.PackedItemDetailsLayoutPanel.AllowDrop = true;
			this.PackedItemDetailsLayoutPanel.AutoScroll = true;
			this.PackedItemDetailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackedItemDetailsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackedItemDetailsLayoutPanel.Name = "PackedItemDetailsLayoutPanel";
			this.PackedItemDetailsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 304, true);
			this.PackedItemDetailsLayoutPanel.TabIndex = 2;
			// 
			// PackPackedItemPivotTabPage
			// 
			this.PackPackedItemPivotTabPage.CaptionResourceString = Res.GetData("a21ce1ea-8914-4632-b599-255f717197ef", "Packs");
			this.PackPackedItemPivotTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PackPackedItemPivotTabPage.Name = "PackPackedItemPivotTabPage";
			this.PackPackedItemPivotTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackPackedItemPivotTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 304, true);
			this.PackPackedItemPivotTabPage.TabIndex = 2;
			this.PackPackedItemPivotTabPage.UseVisualStyleBackColor = true;
			//
			// SupplyChainActorTabPage
			// 
			this.SupplyChainActorTabPage.CaptionResourceString = Res.GetData("22272E0C-0BFD-460C-8475-9DD29524887D", "Add. Supply Chain Actors");
			this.SupplyChainActorTabPage.Controls.Add(this.SupplyChainActorTabUserControl);
			this.SupplyChainActorTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.SupplyChainActorTabPage.Name = "SupplyChainActorTabPage";
			this.SupplyChainActorTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SupplyChainActorTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 366, true);
			this.SupplyChainActorTabPage.TabIndex = 3;
			this.SupplyChainActorTabPage.UseVisualStyleBackColor = true;
			// 
			// SupplyChainActorTabUserControl
			// 
			this.SupplyChainActorTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplyChainActorTabUserControl, ".");
			this.SupplyChainActorTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupplyChainActorTabUserControl.Name = "SupplyChainActorTabUserControl";
			this.SupplyChainActorTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 295, true);
			this.SupplyChainActorTabUserControl.TabIndex = 0;
            // 
            // AdditionalInfoTabPage
            // 
            this.AdditionalInfoTabPage.CaptionResourceString = Res.GetData("88d2bce2-d8d4-46d0-ae9b-2af8230c2121", "Additional Information");
			this.AdditionalInfoTabPage.Controls.Add(this.AdditionalInformationLayoutPanel);
			this.AdditionalInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalInfoTabPage.Name = "AdditionalInfoTabPage";
			this.AdditionalInfoTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 304, true);
			this.AdditionalInfoTabPage.TabIndex = 2;
			// 
			// AdditionalInformationLayoutPanel
			// 
			this.AdditionalInformationLayoutPanel.AllowDrop = true;
			this.AdditionalInformationLayoutPanel.AutoScroll = true;
			this.AdditionalInformationLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInformationLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalInformationLayoutPanel.Name = "AdditionalInformationLayoutPanel";
			this.AdditionalInformationLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1275, 298, true);
			this.AdditionalInformationLayoutPanel.TabIndex = 2;
			// 
			// PreviousDocumentsTabPage
			// 
			this.PreviousDocumentsTabPage.CaptionResourceString = Res.GetData("5605a51f-6758-447b-8dbc-042c6bbbd851", "Previous Documents");
			this.PreviousDocumentsTabPage.Controls.Add(this.PreviousDocumentsLayoutPanel);
			this.PreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.PreviousDocumentsTabPage.Name = "PreviousDocumentsTabPage";
			this.PreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 366, true);
			this.PreviousDocumentsTabPage.TabIndex = 0;
			// 
			// PreviousDocumentsLayoutPanel
			// 
			this.PreviousDocumentsLayoutPanel.AllowDrop = true;
			this.PreviousDocumentsLayoutPanel.AutoScroll = true;
			this.PreviousDocumentsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousDocumentsLayoutPanel.Name = "PreviousDocumentsLayoutPanel";
			this.PreviousDocumentsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 366, true);
			this.PreviousDocumentsLayoutPanel.TabIndex = 3;
			// 
			// SupportingDocumentsTabPage
			// 
			this.SupportingDocumentsTabPage.CaptionResourceString = Res.GetData("68C51D9C-3226-47D1-804B-760C6A165BC1", "Supporting Documents");
			this.SupportingDocumentsTabPage.Controls.Add(this.SupportingDocumentsLayoutPanel);
			this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
			this.SupportingDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 304, true);
			this.SupportingDocumentsTabPage.TabIndex = 2;
			// 
			// SupportingDocumentsLayoutPanel
			// 
			this.SupportingDocumentsLayoutPanel.AllowDrop = true;
			this.SupportingDocumentsLayoutPanel.AutoScroll = true;
			this.SupportingDocumentsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SupportingDocumentsLayoutPanel.Name = "SupportingDocumentsLayoutPanel";
			this.SupportingDocumentsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1275, 298, true);
			this.SupportingDocumentsLayoutPanel.TabIndex = 3;

			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.Splitter);
			this.Controls.Add(this.PackedItemTabControl);
			this.Name = "UCC6TemporaryStoragePackedItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 584, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackedItemTabControl.ResumeLayout(false);
			this.PackedItemTabControl.PerformLayout();
			this.PackedItemDetailsTabPage.ResumeLayout(false);
			this.PackedItemDetailsTabPage.PerformLayout();
			this.AdditionalInfoTabPage.ResumeLayout(false);
			this.AdditionalInfoTabPage.PerformLayout();
			this.SupplyChainActorTabPage.ResumeLayout(false);
			this.SupplyChainActorTabPage.PerformLayout();
			this.SupplyChainActorTabUserControl.ResumeLayout(true);
			this.SupplyChainActorTabUserControl.PerformLayout();
			this.PreviousDocumentsTabPage.ResumeLayout(false);
			this.PreviousDocumentsTabPage.PerformLayout();
			this.PreviousDocumentsLayoutPanel.ResumeLayout(false);
			this.PreviousDocumentsLayoutPanel.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			this.SupportingDocumentsLayoutPanel.ResumeLayout(false);
			this.SupportingDocumentsLayoutPanel.PerformLayout();
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		private CargoWise.Windows.UI.KSplitter Splitter;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl PackedItemTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage PackedItemDetailsTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel PackedItemDetailsLayoutPanel;
		protected ZArchitecture.GUI.ZTabPage AdditionalInfoTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel AdditionalInformationLayoutPanel;
		private ZArchitecture.GUI.ZTabPage PackPackedItemPivotTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage SupplyChainActorTabPage;
		SupplyChainActorTabUserControl SupplyChainActorTabUserControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel SupportingDocumentsLayoutPanel;
		protected ZArchitecture.GUI.ZTabPage PreviousDocumentsTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel PreviousDocumentsLayoutPanel;
	}
}
