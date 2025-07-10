using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	sealed partial class UCC6TemporaryStorageBillControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
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
			this.LineDetailTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.BillDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BillDetailsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.PreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PreviousDocumentsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupportingDocumentsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.AdditionalInformationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalInformationLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.PacksTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PacksLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.UCC6TemporaryStoragePackagesControl = new Enterprise.Customs.EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackagesControl();
			this.BillPartiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BillPartiesLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.PackedItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PackedItemsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.SupplyChainActorTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupplyChainActorTabUserControl = new Enterprise.Customs.EU.TemporaryStorage.GUI.SupplyChainActorTabUserControl();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LineDetailTabControl.SuspendLayout();
			this.BillDetailsTabPage.SuspendLayout();
			this.PreviousDocumentsTabPage.SuspendLayout();
			this.PreviousDocumentsLayoutPanel.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			this.SupportingDocumentsLayoutPanel.SuspendLayout();
			this.AdditionalInformationTabPage.SuspendLayout();
			this.PacksTabPage.SuspendLayout();
			this.PacksLayoutPanel.SuspendLayout();
			this.UCC6TemporaryStoragePackagesControl.SuspendLayout();
			this.BillPartiesTabPage.SuspendLayout();
			this.PackedItemsTabPage.SuspendLayout();
			this.PackedItemsLayoutPanel.SuspendLayout();
			this.SupplyChainActorTabPage.SuspendLayout();
			this.SupplyChainActorTabUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// LineDetailTabControl
			//
			this.LineDetailTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.LineDetailTabControl.Controls.Add(this.BillDetailsTabPage);
			this.LineDetailTabControl.Controls.Add(this.PacksTabPage);
			this.LineDetailTabControl.Controls.Add(this.PackedItemsTabPage);
			this.LineDetailTabControl.Controls.Add(this.BillPartiesTabPage);
			this.LineDetailTabControl.Controls.Add(this.SupportingDocumentsTabPage);
			this.LineDetailTabControl.Controls.Add(this.PreviousDocumentsTabPage);
			this.LineDetailTabControl.Controls.Add(this.AdditionalInformationTabPage);
			this.LineDetailTabControl.Controls.Add(this.SupplyChainActorTabPage);
			this.LineDetailTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LineDetailTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LineDetailTabControl.Name = "LineDetailTabControl";
			this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 389, true);
			this.LineDetailTabControl.TabIndex = 0;
			// 
			// BillDetailsTabPage
			// 
			this.BillDetailsTabPage.CaptionResourceString = Res.GetData("c374c955-c544-4bc7-bc13-2998641e81a4", "Bill Details");
			this.BillDetailsTabPage.Controls.Add(this.BillDetailsLayoutPanel);
			this.BillDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.BillDetailsTabPage.Name = "BillDetailsTabPage";
			this.BillDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 366, true);
			this.BillDetailsTabPage.TabIndex = 1;
			// 
			// BillDetailsLayoutPanel
			// 
			this.BillDetailsLayoutPanel.AllowDrop = true;
			this.BillDetailsLayoutPanel.AutoScroll = true;
			this.BillDetailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillDetailsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillDetailsLayoutPanel.Name = "BillDetailsLayoutPanel";
			this.BillDetailsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 366, true);
			this.BillDetailsLayoutPanel.TabIndex = 2;
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
			this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
			this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 366, true);
			this.SupportingDocumentsTabPage.TabIndex = 0;
			// 
			// SupportingDocumentsLayoutPanel
			// 
			this.SupportingDocumentsLayoutPanel.AllowDrop = true;
			this.SupportingDocumentsLayoutPanel.AutoScroll = true;
			this.SupportingDocumentsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsLayoutPanel.Name = "SupportingDocumentsLayoutPanel";
			this.SupportingDocumentsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 366, true);
			this.SupportingDocumentsLayoutPanel.TabIndex = 3;
			// 
			// AdditionalInformationTabPage
			// 
			this.AdditionalInformationTabPage.CaptionResourceString = Res.GetData("b100a147-5849-42d6-86de-48f552e8b6df", "Additional Information");
			this.AdditionalInformationTabPage.Controls.Add(this.AdditionalInformationLayoutPanel);
			this.AdditionalInformationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.AdditionalInformationTabPage.Name = "AdditionalInformationTabPage";
			this.AdditionalInformationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalInformationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 366, true);
			this.AdditionalInformationTabPage.TabIndex = 2;
			this.AdditionalInformationTabPage.UseVisualStyleBackColor = true;
			// 
			// BillPartiesTabPage
			// 
			this.BillPartiesTabPage.CaptionResourceString = Res.GetData("03A0015D-230A-4E13-A8BA-44D4875A2570", "Bill Parties");
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
			// AdditionalInformationLayoutPanel
			//
			this.AdditionalInformationLayoutPanel.AllowDrop = true;
			this.AdditionalInformationLayoutPanel.AutoScroll = true;
			this.AdditionalInformationLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInformationLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalInformationLayoutPanel.Name = "AdditionalInformationLayoutPanel";
			this.AdditionalInformationLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1275, 360, true);
			this.AdditionalInformationLayoutPanel.TabIndex = 2;
			// 
			// PacksTabPage
			// 
			this.PacksTabPage.CaptionResourceString = Res.GetData("D9FC2C66-F979-405E-A992-60D82601AD2C", "Packs");
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
			this.PacksLayoutPanel.Controls.Add(this.UCC6TemporaryStoragePackagesControl);
			this.PacksLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PacksLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PacksLayoutPanel.Name = "PacksLayoutPanel";
			this.PacksLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1275, 360, true);
			this.PacksLayoutPanel.TabIndex = 2;
			// 
			// UCC6TemporaryStoragePackagesControl
			// 
			this.UCC6TemporaryStoragePackagesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UCC6TemporaryStoragePackagesControl, ".");
			this.UCC6TemporaryStoragePackagesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UCC6TemporaryStoragePackagesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UCC6TemporaryStoragePackagesControl.Name = "UCC6TemporaryStoragePackagesControl";
			this.UCC6TemporaryStoragePackagesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1275, 360, true);
			this.UCC6TemporaryStoragePackagesControl.TabIndex = 3;
			// 
			// PackedItemsTabPage
			// 
			this.PackedItemsTabPage.CaptionResourceString = Res.GetData("C5DD9E49-DDE0-4C62-B4E8-644178FFF06E", "Items");
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
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 584, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.LineDetailTabControl);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(370);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(183);
			this.SplitContainer.SplitterWidth = 3;
			this.SplitContainer.TabIndex = 21;
			// 
			// UCC6TemporaryStorageBillControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "UCC6TemporaryStorageBillControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1289, 584, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LineDetailTabControl.ResumeLayout(false);
			this.LineDetailTabControl.PerformLayout();
			this.BillDetailsTabPage.ResumeLayout(false);
			this.BillDetailsTabPage.PerformLayout();
			this.PreviousDocumentsTabPage.ResumeLayout(false);
			this.PreviousDocumentsTabPage.PerformLayout();
			this.PreviousDocumentsLayoutPanel.ResumeLayout(false);
			this.PreviousDocumentsLayoutPanel.PerformLayout();
			this.BillPartiesTabPage.ResumeLayout(false);
			this.BillPartiesTabPage.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			this.SupportingDocumentsLayoutPanel.ResumeLayout(false);
			this.SupportingDocumentsLayoutPanel.PerformLayout();
			this.AdditionalInformationTabPage.ResumeLayout(false);
			this.AdditionalInformationTabPage.PerformLayout();
			this.PacksTabPage.ResumeLayout(false);
			this.PacksTabPage.PerformLayout();
			this.PacksLayoutPanel.ResumeLayout(false);
			this.PacksLayoutPanel.PerformLayout();
			this.UCC6TemporaryStoragePackagesControl.ResumeLayout(true);
			this.UCC6TemporaryStoragePackagesControl.PerformLayout();
			this.PackedItemsTabPage.ResumeLayout(false);
			this.PackedItemsTabPage.PerformLayout();
			this.PackedItemsLayoutPanel.ResumeLayout(false);
			this.PackedItemsLayoutPanel.PerformLayout();
			this.SupplyChainActorTabPage.ResumeLayout(false);
			this.SupplyChainActorTabPage.PerformLayout();
			this.SupplyChainActorTabUserControl.ResumeLayout(true);
			this.SupplyChainActorTabUserControl.PerformLayout();
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl LineDetailTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage BillDetailsTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel BillDetailsLayoutPanel;
		private UCC6TemporaryStoragePackagesControl UCC6TemporaryStoragePackagesControl;
		private ZArchitecture.GUI.DynamicLayoutPanel SupportingDocumentsLayoutPanel;
		private ZArchitecture.GUI.ZTabPage AdditionalInformationTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel AdditionalInformationLayoutPanel;
		private ZArchitecture.GUI.ZTabPage PreviousDocumentsTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel PreviousDocumentsLayoutPanel;
		private Enterprise.ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
		private ZArchitecture.GUI.ZTabPage PacksTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel PacksLayoutPanel;
		private Enterprise.ZArchitecture.GUI.ZTabPage BillPartiesTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel BillPartiesLayoutPanel;
		private ZArchitecture.GUI.ZTabPage PackedItemsTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel PackedItemsLayoutPanel;
		Enterprise.ZArchitecture.GUI.ZTabPage SupplyChainActorTabPage;
		SupplyChainActorTabUserControl SupplyChainActorTabUserControl;
	}
}
