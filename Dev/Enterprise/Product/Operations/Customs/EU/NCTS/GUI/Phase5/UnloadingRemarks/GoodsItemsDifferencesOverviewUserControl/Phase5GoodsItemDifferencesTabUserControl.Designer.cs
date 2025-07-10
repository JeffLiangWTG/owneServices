using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5GoodsItemDifferencesTabUserControl
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
			this.PackagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NctsPackagePanelUserControl = new Enterprise.Customs.EU.NCTS.GUI.NctsPackagePanelUserControl();
			this.ContainersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NctsContainerGridUserControl = new Enterprise.Customs.EU.NCTS.GUI.NctsContainerGridUserControl();
			this.GoodsItemDifferencesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.GoodsItemDifferencesGridUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5GoodsItemDifferencesGridUserControl();
			this.GoodsItemDifferencesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ItemDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GoodsItemDifferencesDetailsDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.GoodsItemDifferencesDetailsColumnDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.ItemPackagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ItemSupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl = new Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl();
			this.ItemAdditionalDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.Phase5GoodItemsAdditionalDocumentPanelUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5GoodItemsAdditionalDocumentPanelUserControl();
			this.ItemPreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GoodsItemPreviousDocumentsPanelUserControl = new Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentPreviousDocumentsPanelUserControl();
			this.LiabilityCalculationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LiabilityCalculationDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PackagesGroupBox.SuspendLayout();
			this.NctsPackagePanelUserControl.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			this.NctsContainerGridUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GoodsItemDifferencesSplitContainer)).BeginInit();
			this.GoodsItemDifferencesSplitContainer.Panel1.SuspendLayout();
			this.GoodsItemDifferencesSplitContainer.Panel2.SuspendLayout();
			this.GoodsItemDifferencesSplitContainer.SuspendLayout();
			this.GoodsItemDifferencesGridUserControl.SuspendLayout();
			this.GoodsItemDifferencesTabControl.SuspendLayout();
			this.ItemDetailsTabPage.SuspendLayout();
			this.ItemPackagesTabPage.SuspendLayout();
			this.ItemSupportingDocumentsTabPage.SuspendLayout();
			this.ItemPreviousDocumentsTabPage.SuspendLayout();
			this.HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl.SuspendLayout();
			this.ItemAdditionalDocumentsTabPage.SuspendLayout();
			this.Phase5GoodItemsAdditionalDocumentPanelUserControl.SuspendLayout();
			this.GoodsItemPreviousDocumentsPanelUserControl.SuspendLayout();
			this.LiabilityCalculationTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.INctsArrivalCargoDescCollection<Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc>);
			// 
			// PackagesGroupBox
			// 
			this.PackagesGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("ECE066D1-8C89-48A0-8145-7189B2C9DEA0", "Packages");
			this.PackagesGroupBox.Controls.Add(this.NctsPackagePanelUserControl);
			this.PackagesGroupBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.PackagesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PackagesGroupBox.Name = "PackagesGroupBox";
			this.PackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 439, true);
			this.PackagesGroupBox.TabIndex = 0;
			this.PackagesGroupBox.TabStop = false;
			// 
			// NctsPackagePanelUserControl
			// 
			this.NctsPackagePanelUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NctsPackagePanelUserControl, "Packages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).Packages)).SyncRoot)))));
			this.NctsPackagePanelUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NctsPackagePanelUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
			this.NctsPackagePanelUserControl.Name = "NctsPackagePanelUserControl";
			this.NctsPackagePanelUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(806, 409, true);
			this.NctsPackagePanelUserControl.TabIndex = 0;
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("4F242261-3DE6-4E42-BB24-34D3DE2FFE29", "Containers");
			this.ContainersGroupBox.Controls.Add(this.NctsContainerGridUserControl);
			this.ContainersGroupBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.ContainersGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.ContainersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(815, 3, true);
			this.ContainersGroupBox.Name = "ContainersGroupBox";
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 439, true);
			this.ContainersGroupBox.TabIndex = 0;
			this.ContainersGroupBox.TabStop = false;
			// 
			// NctsContainerGridUserControl
			// 
			this.NctsContainerGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NctsContainerGridUserControl, "Packages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).Packages)).SyncRoot)))));
			this.NctsContainerGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NctsContainerGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
			this.NctsContainerGridUserControl.Name = "NctsContainerGridUserControl";
			this.NctsContainerGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 409, true);
			this.NctsContainerGridUserControl.TabIndex = 0;
			// 
			// GoodsItemDifferencesSplitContainer
			// 
			this.GoodsItemDifferencesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemDifferencesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemDifferencesSplitContainer.Name = "GoodsItemDifferencesSplitContainer";
			this.GoodsItemDifferencesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// GoodsItemDifferencesSplitContainer.Panel1
			// 
			this.GoodsItemDifferencesSplitContainer.Panel1.Controls.Add(this.GoodsItemDifferencesGridUserControl);
			// 
			// GoodsItemDifferencesSplitContainer.Panel2
			// 
			this.GoodsItemDifferencesSplitContainer.Panel2.Controls.Add(this.GoodsItemDifferencesTabControl);
			this.GoodsItemDifferencesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 747, true);
			this.GoodsItemDifferencesSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(300);
			this.GoodsItemDifferencesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(260);
			this.GoodsItemDifferencesSplitContainer.TabIndex = 0;
			// 
			// GoodsItemDifferencesGridUserControl
			// 
			this.GoodsItemDifferencesGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsItemDifferencesGridUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)))));
			this.GoodsItemDifferencesGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemDifferencesGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemDifferencesGridUserControl.Name = "GoodsItemDifferencesGridUserControl";
			this.GoodsItemDifferencesGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 260, true);
			this.GoodsItemDifferencesGridUserControl.TabIndex = 0;
			// 
			// GoodsItemDifferencesTabControl
			// 
			this.GoodsItemDifferencesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.GoodsItemDifferencesTabControl.Controls.Add(this.ItemDetailsTabPage);
			this.GoodsItemDifferencesTabControl.Controls.Add(this.ItemPackagesTabPage);
			this.GoodsItemDifferencesTabControl.Controls.Add(this.ItemSupportingDocumentsTabPage);
			this.GoodsItemDifferencesTabControl.Controls.Add(this.ItemAdditionalDocumentsTabPage);
			this.GoodsItemDifferencesTabControl.Controls.Add(this.ItemPreviousDocumentsTabPage);
			this.GoodsItemDifferencesTabControl.Controls.Add(this.LiabilityCalculationTabPage);
			this.GoodsItemDifferencesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemDifferencesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemDifferencesTabControl.Name = "GoodsItemDifferencesTabControl";
			this.GoodsItemDifferencesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 483, true);
			this.GoodsItemDifferencesTabControl.TabIndex = 0;
			// 
			// ItemDetailsTabPage
			// 
			this.ItemDetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("ED101825-EC0B-431D-A670-A2C02BACB800", "Item Details");
			this.ItemDetailsTabPage.Controls.Add(this.GoodsItemDifferencesDetailsDynamicLayoutPanel);
			this.ItemDetailsTabPage.Controls.Add(this.GoodsItemDifferencesDetailsColumnDynamicLayoutPanel);
			this.ItemDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
			this.ItemDetailsTabPage.Name = "ItemDetailsTabPage";
			this.ItemDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 445, true);
			this.ItemDetailsTabPage.TabIndex = 1;
			// 
			// GoodsItemDifferencesDetailsDynamicLayoutPanel
			// 
			this.GoodsItemDifferencesDetailsDynamicLayoutPanel.AllowDrop = true;
			this.GoodsItemDifferencesDetailsDynamicLayoutPanel.AutoSize = true;
			this.GoodsItemDifferencesDetailsDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemDifferencesDetailsDynamicLayoutPanel.Name = "GoodsItemDifferencesDetailsDynamicLayoutPanel";
			this.GoodsItemDifferencesDetailsDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 50, true);
			this.GoodsItemDifferencesDetailsDynamicLayoutPanel.TabIndex = 1;
			// 
			// GoodsItemDifferencesDetailsColumnDynamicLayoutPanel
			// 
			this.GoodsItemDifferencesDetailsColumnDynamicLayoutPanel.AllowDrop = true;
			this.GoodsItemDifferencesDetailsColumnDynamicLayoutPanel.AutoSize = true;
			this.GoodsItemDifferencesDetailsColumnDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.GoodsItemDifferencesDetailsColumnDynamicLayoutPanel.Name = "GoodsItemDifferencesDetailsColumnDynamicLayoutPanel";
			this.GoodsItemDifferencesDetailsColumnDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 150, true);
			this.GoodsItemDifferencesDetailsColumnDynamicLayoutPanel.TabIndex = 1;
			// 
			// ItemPackagesTabPage
			// 
			this.ItemPackagesTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("48E6D412-FEAB-4CE4-9517-5B836B8462FA", "Packages && Containers");
			this.ItemPackagesTabPage.Controls.Add(this.PackagesGroupBox);
			this.ItemPackagesTabPage.Controls.Add(this.ContainersGroupBox);
			this.ItemPackagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
			this.ItemPackagesTabPage.Name = "ItemPackagesTabPage";
			this.ItemPackagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ItemPackagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 445, true);
			this.ItemPackagesTabPage.TabIndex = 0;
			this.ItemPackagesTabPage.UseVisualStyleBackColor = true;
			// 
			// ItemSupportingDocumentsTabPage
			// 
			this.ItemSupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("D0EE2966-C8FB-4E5C-A301-B25510B6DC0F", "Supporting Documents");
			this.ItemSupportingDocumentsTabPage.Controls.Add(this.HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl);
			this.ItemSupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
			this.ItemSupportingDocumentsTabPage.Name = "ItemSupportingDocumentsTabPage";
			this.ItemSupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 445, true);
			this.ItemSupportingDocumentsTabPage.TabIndex = 1;
			// 
			// HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl
			// 
			this.HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl, "SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocumentCollection<Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument>)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).SupportingDocuments)));
			this.HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl.Name = "HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl";
			this.HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 445, true);
			this.HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl.TabIndex = 0;
			// 
			// ItemAdditionalDocumentsTabPage
			// 
			this.ItemAdditionalDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("81BFCB80-EF72-4145-B31E-A2AE8A7236C6", "Additional Documents");
			this.ItemAdditionalDocumentsTabPage.Controls.Add(this.Phase5GoodItemsAdditionalDocumentPanelUserControl);
			this.ItemAdditionalDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
			this.ItemAdditionalDocumentsTabPage.Name = "ItemAdditionalDocumentsTabPage";
			this.ItemAdditionalDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 445, true);
			this.ItemAdditionalDocumentsTabPage.TabIndex = 1;
			// 
			// Phase5GoodItemsAdditionalDocumentPanelUserControl
			// 
			this.Phase5GoodItemsAdditionalDocumentPanelUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Phase5GoodItemsAdditionalDocumentPanelUserControl, "AdditionalInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).AdditionalInfos)));
			this.Phase5GoodItemsAdditionalDocumentPanelUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Phase5GoodItemsAdditionalDocumentPanelUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Phase5GoodItemsAdditionalDocumentPanelUserControl.Name = "Phase5GoodItemsAdditionalDocumentPanelUserControl";
			this.Phase5GoodItemsAdditionalDocumentPanelUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 445, true);
			this.Phase5GoodItemsAdditionalDocumentPanelUserControl.TabIndex = 0;
			// 
			// ItemPreviousDocumentsTabPage
			// 
			this.ItemPreviousDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("ACF882E2-F66E-47FB-AA5D-B91BC3AF86C6", "Previous Documents");
			this.ItemPreviousDocumentsTabPage.Controls.Add(this.GoodsItemPreviousDocumentsPanelUserControl);
			this.ItemPreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
			this.ItemPreviousDocumentsTabPage.Name = "ItemPreviousDocumentsTabPage";
			this.ItemPreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 445, true);
			this.ItemPreviousDocumentsTabPage.TabIndex = 2;
			// 
			// Phase5GoodsItemPreviousDocumentsGridUserControl
			// 
			this.GoodsItemPreviousDocumentsPanelUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsItemPreviousDocumentsPanelUserControl, "PreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).PreviousDocuments)));
			this.GoodsItemPreviousDocumentsPanelUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemPreviousDocumentsPanelUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemPreviousDocumentsPanelUserControl.Name = "GoodsItemPreviousDocumentsPanelUserControl";
			this.GoodsItemPreviousDocumentsPanelUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 445, true);
			this.GoodsItemPreviousDocumentsPanelUserControl.TabIndex = 0;
			// 
			// LibailityCalculationTabPage
			// 
			this.LiabilityCalculationTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("29089DF2-A18E-4B66-A559-357A689E1DFB", "Liability Calculation");
			this.LiabilityCalculationTabPage.Controls.Add(this.LiabilityCalculationDynamicLayoutPanel);
			this.LiabilityCalculationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
			this.LiabilityCalculationTabPage.Name = "LiabilityCalculationTabPage";
			this.LiabilityCalculationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 445, true);
			this.LiabilityCalculationTabPage.TabIndex = 3;
			// 
			// LiabilityCalculationDynamicLayoutPanel
			// 
			this.LiabilityCalculationDynamicLayoutPanel.AllowDrop = true;
			this.LiabilityCalculationDynamicLayoutPanel.AutoSize = true;
			this.LiabilityCalculationDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LiabilityCalculationDynamicLayoutPanel.Name = "LiabilityCalculationDynamicLayoutPanel";
			this.LiabilityCalculationDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 50, true);
			this.LiabilityCalculationDynamicLayoutPanel.TabIndex = 3;
			// 
			// Phase5GoodsItemDifferencesTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GoodsItemDifferencesSplitContainer);
			this.Name = "Phase5GoodsItemDifferencesTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 747, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackagesGroupBox.ResumeLayout(false);
			this.PackagesGroupBox.PerformLayout();
			this.NctsPackagePanelUserControl.ResumeLayout(true);
			this.NctsPackagePanelUserControl.PerformLayout();
			this.ContainersGroupBox.ResumeLayout(false);
			this.ContainersGroupBox.PerformLayout();
			this.NctsContainerGridUserControl.ResumeLayout(true);
			this.NctsContainerGridUserControl.PerformLayout();
			this.GoodsItemDifferencesSplitContainer.Panel1.ResumeLayout(false);
			this.GoodsItemDifferencesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.GoodsItemDifferencesSplitContainer)).EndInit();
			this.GoodsItemDifferencesSplitContainer.ResumeLayout(false);
			this.GoodsItemDifferencesSplitContainer.PerformLayout();
			this.GoodsItemDifferencesGridUserControl.ResumeLayout(true);
			this.GoodsItemDifferencesGridUserControl.PerformLayout();
			this.GoodsItemDifferencesTabControl.ResumeLayout(false);
			this.GoodsItemDifferencesTabControl.PerformLayout();
			this.ItemDetailsTabPage.ResumeLayout(false);
			this.ItemDetailsTabPage.PerformLayout();
			this.ItemPackagesTabPage.ResumeLayout(false);
			this.ItemPackagesTabPage.PerformLayout();
			this.ItemSupportingDocumentsTabPage.ResumeLayout(false);
			this.ItemSupportingDocumentsTabPage.PerformLayout();
			this.HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl.ResumeLayout(true);
			this.HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl.PerformLayout();
			this.ItemAdditionalDocumentsTabPage.ResumeLayout(false);
			this.ItemAdditionalDocumentsTabPage.PerformLayout();
			this.Phase5GoodItemsAdditionalDocumentPanelUserControl.ResumeLayout(true);
			this.Phase5GoodItemsAdditionalDocumentPanelUserControl.PerformLayout();
			this.ItemPreviousDocumentsTabPage.ResumeLayout(false);
			this.ItemPreviousDocumentsTabPage.PerformLayout();
			this.LiabilityCalculationTabPage.ResumeLayout(false);
			this.LiabilityCalculationTabPage.PerformLayout();
			this.GoodsItemPreviousDocumentsPanelUserControl.ResumeLayout(true);
			this.GoodsItemPreviousDocumentsPanelUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer GoodsItemDifferencesSplitContainer;
		internal Phase5GoodsItemDifferencesGridUserControl GoodsItemDifferencesGridUserControl;
		internal NctsPackagePanelUserControl NctsPackagePanelUserControl;
		internal Phase5GoodItemsAdditionalDocumentPanelUserControl Phase5GoodItemsAdditionalDocumentPanelUserControl;
		internal ZArchitecture.GUI.ZTabControl GoodsItemDifferencesTabControl;
		internal ZArchitecture.GUI.ZTabPage ItemDetailsTabPage;
		internal ZArchitecture.GUI.ZTabPage ItemSupportingDocumentsTabPage;
		internal ZArchitecture.GUI.ZTabPage ItemAdditionalDocumentsTabPage;
		internal ZArchitecture.GUI.ZTabPage ItemPreviousDocumentsTabPage;
		internal HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl HouseConsignmentGoodItemsSupportingDocumentsPanelUserControl;
		internal ZArchitecture.GUI.ZTabPage ItemPackagesTabPage;
		internal ZArchitecture.GUI.DynamicLayoutPanel GoodsItemDifferencesDetailsDynamicLayoutPanel;
		internal ZArchitecture.GUI.DynamicLayoutPanel GoodsItemDifferencesDetailsColumnDynamicLayoutPanel;
		internal ZArchitecture.GUI.ZGroupBox PackagesGroupBox;
		internal ZArchitecture.GUI.ZGroupBox ContainersGroupBox;
		internal NctsContainerGridUserControl NctsContainerGridUserControl;
		internal HouseConsignmentPreviousDocumentsPanelUserControl GoodsItemPreviousDocumentsPanelUserControl;
		internal ZArchitecture.GUI.ZTabPage LiabilityCalculationTabPage;
		internal ZArchitecture.GUI.DynamicLayoutPanel LiabilityCalculationDynamicLayoutPanel;
	}
}
