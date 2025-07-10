using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentDifferencesTabUserControl
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
			this.HouseDetailsDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.HouseConsignmentDifferencesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.HouseConsignmentDifferencesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.HouseDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GoodsItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.Phase5GoodsItemDifferencesTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5GoodsItemDifferencesTabUserControl();
			this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseConsignmentSupportingDocumentsPanelUserControl = new Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentDifferencesSupportingDocumentPanelUserControl();
			this.AdditionalDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseConsignmentAdditionalDocumentsPanelUserControl = new Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentAdditionalDocumentsPanelUserControl();
			this.PreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseConsignmentPreviousDocumentsPanelUserControl = new Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentPreviousDocumentsPanelUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HouseConsignmentDifferencesSplitContainer)).BeginInit();
			this.HouseConsignmentDifferencesSplitContainer.Panel2.SuspendLayout();
			this.HouseConsignmentDifferencesSplitContainer.SuspendLayout();
			this.HouseConsignmentDifferencesTabControl.SuspendLayout();
			this.HouseDetailsTabPage.SuspendLayout();
			this.GoodsItemsTabPage.SuspendLayout();
			this.Phase5GoodsItemDifferencesTabUserControl.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			this.HouseConsignmentSupportingDocumentsPanelUserControl.SuspendLayout();
			this.AdditionalDocumentsTabPage.SuspendLayout();
			this.HouseConsignmentAdditionalDocumentsPanelUserControl.SuspendLayout();
			this.PreviousDocumentsTabPage.SuspendLayout();
			this.HouseConsignmentPreviousDocumentsPanelUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.INctsBillCollection<Enterprise.Customs.EU.NCTS.Business.NctsBill>);
			// 
			// HouseDetailsDynamicLayoutPanel
			// 
			this.HouseDetailsDynamicLayoutPanel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseDetailsDynamicLayoutPanel, ".");
			this.HouseDetailsDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseDetailsDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HouseDetailsDynamicLayoutPanel.Name = "HouseDetailsDynamicLayoutPanel";
			this.HouseDetailsDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 616, true);
			this.HouseDetailsDynamicLayoutPanel.TabIndex = 0;
			// 
			// HouseConsignmentDifferencesSplitContainer
			// 
			this.HouseConsignmentDifferencesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentDifferencesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseConsignmentDifferencesSplitContainer.Name = "HouseConsignmentDifferencesSplitContainer";
			this.HouseConsignmentDifferencesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// HouseConsignmentDifferencesSplitContainer.Panel2
			// 
			this.HouseConsignmentDifferencesSplitContainer.Panel2.Controls.Add(this.HouseConsignmentDifferencesTabControl);
			this.HouseConsignmentDifferencesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 747, true);
			this.HouseConsignmentDifferencesSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(450);
			this.HouseConsignmentDifferencesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(89);
			this.HouseConsignmentDifferencesSplitContainer.SplitterWidth = 9;
			this.HouseConsignmentDifferencesSplitContainer.TabIndex = 0;
			// 
			// HouseConsignmentDifferencesTabControl
			// 
			this.HouseConsignmentDifferencesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.HouseConsignmentDifferencesTabControl.Controls.Add(this.HouseDetailsTabPage);
			this.HouseConsignmentDifferencesTabControl.Controls.Add(this.GoodsItemsTabPage);
			this.HouseConsignmentDifferencesTabControl.Controls.Add(this.SupportingDocumentsTabPage);
			this.HouseConsignmentDifferencesTabControl.Controls.Add(this.AdditionalDocumentsTabPage);
			this.HouseConsignmentDifferencesTabControl.Controls.Add(this.PreviousDocumentsTabPage);
			this.HouseConsignmentDifferencesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentDifferencesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseConsignmentDifferencesTabControl.Name = "HouseConsignmentDifferencesTabControl";
			this.HouseConsignmentDifferencesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 649, true);
			this.HouseConsignmentDifferencesTabControl.TabIndex = 0;
			// 
			// HouseDetailsTabPage
			// 
			this.HouseDetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("33A64BD5-1805-419F-B6B2-C9818B918C24", "House Details");
			this.HouseDetailsTabPage.Controls.Add(this.HouseDetailsDynamicLayoutPanel);
			this.HouseDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseDetailsTabPage.Name = "HouseDetailsTabPage";
			this.HouseDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HouseDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 622, true);
			this.HouseDetailsTabPage.TabIndex = 0;
			this.HouseDetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// GoodsItemsTabPage
			// 
			this.GoodsItemsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("b143fe2c-29e8-451e-b1d2-9ad4580f76d0", "Goods Items");
			this.GoodsItemsTabPage.Controls.Add(this.Phase5GoodsItemDifferencesTabUserControl);
			this.GoodsItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GoodsItemsTabPage.Name = "GoodsItemsTabPage";
			this.GoodsItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 622, true);
			this.GoodsItemsTabPage.TabIndex = 1;
			// 
			// Phase5GoodsItemDifferencesTabUserControl
			// 
			this.Phase5GoodsItemDifferencesTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Phase5GoodsItemDifferencesTabUserControl, "ArrivalGoodsItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.INctsArrivalCargoDescCollection<Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc>)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).ArrivalGoodsItems)));
			this.Phase5GoodsItemDifferencesTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Phase5GoodsItemDifferencesTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Phase5GoodsItemDifferencesTabUserControl.Name = "Phase5GoodsItemDifferencesTabUserControl";
			this.Phase5GoodsItemDifferencesTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 622, true);
			this.Phase5GoodsItemDifferencesTabUserControl.TabIndex = 0;
			// 
			// SupportingDocumentsTabPage
			// 
			this.SupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("E0E096B8-D12E-4D85-99C4-75393462B9FE", "Supporting Documents");
			this.SupportingDocumentsTabPage.Controls.Add(this.HouseConsignmentSupportingDocumentsPanelUserControl);
			this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
			this.SupportingDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 622, true);
			this.SupportingDocumentsTabPage.TabIndex = 0;
			this.SupportingDocumentsTabPage.UseVisualStyleBackColor = true;
			// 
			// HouseConsignmentSupportingDocumentsPanelUserControl
			// 
			this.HouseConsignmentSupportingDocumentsPanelUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseConsignmentSupportingDocumentsPanelUserControl, "SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocumentCollection<Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument>)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).SupportingDocuments)));
			this.HouseConsignmentSupportingDocumentsPanelUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentSupportingDocumentsPanelUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HouseConsignmentSupportingDocumentsPanelUserControl.Name = "HouseConsignmentSupportingDocumentsPanelUserControl";
			this.HouseConsignmentSupportingDocumentsPanelUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 616, true);
			this.HouseConsignmentSupportingDocumentsPanelUserControl.TabIndex = 0;
			// 
			// AdditionalDocumentsTabPage
			// 
			this.AdditionalDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("8ED7F5A9-01E1-49E7-8D39-3EC6DCEDB8D2", "Additional Documents");
			this.AdditionalDocumentsTabPage.Controls.Add(this.HouseConsignmentAdditionalDocumentsPanelUserControl);
			this.AdditionalDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalDocumentsTabPage.Name = "AdditionalDocumentsTabPage";
			this.AdditionalDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 622, true);
			this.AdditionalDocumentsTabPage.TabIndex = 0;
			this.AdditionalDocumentsTabPage.UseVisualStyleBackColor = true;
			// 
			// HouseConsignmentAdditionalDocumentsPanelUserControl
			// 
			this.HouseConsignmentAdditionalDocumentsPanelUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseConsignmentAdditionalDocumentsPanelUserControl, "AdditionalDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).AdditionalDocuments)));
			this.HouseConsignmentAdditionalDocumentsPanelUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentAdditionalDocumentsPanelUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HouseConsignmentAdditionalDocumentsPanelUserControl.Name = "HouseConsignmentAdditionalDocumentsPanelUserControl";
			this.HouseConsignmentAdditionalDocumentsPanelUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 616, true);
			this.HouseConsignmentAdditionalDocumentsPanelUserControl.TabIndex = 0;
			// 
			// PreviousDocumentsTabPage
			// 
			this.PreviousDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("3E2ED30D-1979-499C-BE3C-BAC044898799", "Previous Documents");
			this.PreviousDocumentsTabPage.Controls.Add(this.HouseConsignmentPreviousDocumentsPanelUserControl);
			this.PreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PreviousDocumentsTabPage.Name = "PreviousDocumentsTabPage";
			this.PreviousDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 622, true);
			this.PreviousDocumentsTabPage.TabIndex = 0;
			this.PreviousDocumentsTabPage.UseVisualStyleBackColor = true;
			// 
			// HouseConsignmentPreviousDocumentsPanelUserControl
			// 
			this.HouseConsignmentPreviousDocumentsPanelUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseConsignmentPreviousDocumentsPanelUserControl, "PreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsPreviousDocumentCollection<Enterprise.Customs.EU.NCTS.Business.NctsPreviousDocument>)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).PreviousDocuments)));
			this.HouseConsignmentPreviousDocumentsPanelUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentPreviousDocumentsPanelUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HouseConsignmentPreviousDocumentsPanelUserControl.Name = "HouseConsignmentPreviousDocumentsPanelUserControl";
			this.HouseConsignmentPreviousDocumentsPanelUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 616, true);
			this.HouseConsignmentPreviousDocumentsPanelUserControl.TabIndex = 0;
			// 
			// HouseConsignmentDifferencesTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HouseConsignmentDifferencesSplitContainer);
			this.Name = "HouseConsignmentDifferencesTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 747, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HouseConsignmentDifferencesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.HouseConsignmentDifferencesSplitContainer)).EndInit();
			this.HouseConsignmentDifferencesSplitContainer.ResumeLayout(false);
			this.HouseConsignmentDifferencesSplitContainer.PerformLayout();
			this.HouseConsignmentDifferencesTabControl.ResumeLayout(false);
			this.HouseConsignmentDifferencesTabControl.PerformLayout();
			this.HouseDetailsTabPage.ResumeLayout(false);
			this.HouseDetailsTabPage.PerformLayout();
			this.GoodsItemsTabPage.ResumeLayout(false);
			this.GoodsItemsTabPage.PerformLayout();
			this.Phase5GoodsItemDifferencesTabUserControl.ResumeLayout(true);
			this.Phase5GoodsItemDifferencesTabUserControl.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			this.HouseConsignmentSupportingDocumentsPanelUserControl.ResumeLayout(true);
			this.HouseConsignmentSupportingDocumentsPanelUserControl.PerformLayout();
			this.AdditionalDocumentsTabPage.ResumeLayout(false);
			this.AdditionalDocumentsTabPage.PerformLayout();
			this.HouseConsignmentAdditionalDocumentsPanelUserControl.ResumeLayout(true);
			this.HouseConsignmentAdditionalDocumentsPanelUserControl.PerformLayout();
			this.PreviousDocumentsTabPage.ResumeLayout(false);
			this.PreviousDocumentsTabPage.PerformLayout();
			this.HouseConsignmentPreviousDocumentsPanelUserControl.ResumeLayout(true);
			this.HouseConsignmentPreviousDocumentsPanelUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer HouseConsignmentDifferencesSplitContainer;
		internal ZArchitecture.GUI.ZTabControl HouseConsignmentDifferencesTabControl;
		internal ZArchitecture.GUI.ZTabPage GoodsItemsTabPage;
		internal Phase5GoodsItemDifferencesTabUserControl Phase5GoodsItemDifferencesTabUserControl;
		internal ZArchitecture.GUI.ZTabPage HouseDetailsTabPage;
		internal ZArchitecture.GUI.DynamicLayoutPanel HouseDetailsDynamicLayoutPanel;
		internal ZArchitecture.GUI.ZTabPage AdditionalDocumentsTabPage;
		internal ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
		internal ZArchitecture.GUI.ZTabPage PreviousDocumentsTabPage;
		internal HouseConsignmentAdditionalDocumentsPanelUserControl HouseConsignmentAdditionalDocumentsPanelUserControl;
		internal HouseConsignmentPreviousDocumentsPanelUserControl HouseConsignmentPreviousDocumentsPanelUserControl;
		internal HouseConsignmentDifferencesSupportingDocumentPanelUserControl HouseConsignmentSupportingDocumentsPanelUserControl;
	}
}
