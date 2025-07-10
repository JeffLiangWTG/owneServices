namespace Enterprise.Customs.IT.NCTS.GUI
{
	partial class NctsGoodsItemsUserControl
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
				M2LinesTabPage.Enter -= M2LinesTabPage_Enter;
				ItemPreviousDocumentsTabPage.Enter -= ItemPreviousDocumentsTabPage_Enter;
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
			this.ItemTaxOrFeeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TaxOrFeeUserControl = new Enterprise.Customs.IT.NCTS.GUI.NctsTaxOrFeeUserControl();
			this.M2LinesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GroupedPreviousDocumentsUserControl = new Enterprise.Customs.IT.GUI.GroupedPreviousDocumentsUserControl();
			this.RemarksTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NctsGoodsItemRemarksUserControl = new Enterprise.Customs.IT.NCTS.GUI.NctsGoodsItemRemarksUserControl();
			this.GoodsItemsTabControl.SuspendLayout();
			this.ItemDetailsTabPage.SuspendLayout();
			this.ItemContainersTabPage.SuspendLayout();
			this.ItemPackagesTabPage.SuspendLayout();
			this.ItemPreviousDocumentsTabPage.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			this.ItemAdditionalInfosTabPage.SuspendLayout();
			this.ItemSecurityTabPage.SuspendLayout();
			this.GoodsItemLineDetailDynamicUserControl.SuspendLayout();
			this.GoodsItemsGridDynamicUserControl.SuspendLayout();
			this.NctsPreviousDocumentsDynamicUserControl.SuspendLayout();
			this.NctsPackagesDynamicUserControl.SuspendLayout();
			this.ItemSecurityDynamicUserControl.SuspendLayout();
			this.AdditionalInfosDynamicUserControl.SuspendLayout();
			this.SupportingDocumentsDynamicUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ItemTaxOrFeeTabPage.SuspendLayout();
			this.TaxOrFeeUserControl.SuspendLayout();
			this.M2LinesTabPage.SuspendLayout();
			this.GroupedPreviousDocumentsUserControl.SuspendLayout();
			this.RemarksTabPage.SuspendLayout();
			this.NctsGoodsItemRemarksUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// GoodsItemsSplitContainer.Panel2
			// 
			this.GoodsItemsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			// 
			// GoodsItemsTabControl
			// 
			this.GoodsItemsTabControl.Controls.Add(this.RemarksTabPage);
			this.GoodsItemsTabControl.Controls.Add(this.ItemTaxOrFeeTabPage);
			this.GoodsItemsTabControl.Controls.Add(this.M2LinesTabPage);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.M2LinesTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemTaxOrFeeTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemSecurityTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemPreviousDocumentsTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.RemarksTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemAdditionalInfosTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.SupportingDocumentsTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemContainersTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemPackagesTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemDetailsTabPage, 0);
			// 
			// ItemTaxOrFeeTabPage
			// 
			this.ItemTaxOrFeeTabPage.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("ee02aca6-9a76-4c56-a460-e1dfbe04a2c3", "[47] Tax or Fee");
			this.ItemTaxOrFeeTabPage.Controls.Add(this.TaxOrFeeUserControl);
			this.ItemTaxOrFeeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ItemTaxOrFeeTabPage.Name = "ItemTaxOrFeeTabPage";
			this.ItemTaxOrFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 462, true);
			this.ItemTaxOrFeeTabPage.TabIndex = 8;
			// 
			// TaxOrFeeUserControl
			// 
			this.TaxOrFeeUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxOrFeeUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)))));
			this.TaxOrFeeUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxOrFeeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxOrFeeUserControl.Name = "TaxOrFeeUserControl";
			this.TaxOrFeeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 462, true);
			this.TaxOrFeeUserControl.TabIndex = 0;
			// 
			// M2LinesTabPage
			// 
			this.M2LinesTabPage.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("e115bc45-c975-4152-8874-6e6a25cac057", "M2 Lines");
			this.M2LinesTabPage.Controls.Add(this.GroupedPreviousDocumentsUserControl);
			this.M2LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.M2LinesTabPage.Name = "M2LinesTabPage";
			this.M2LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 462, true);
			this.M2LinesTabPage.TabIndex = 9;
			// 
			// GroupedPreviousDocumentsUserControl
			// 
			this.GroupedPreviousDocumentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GroupedPreviousDocumentsUserControl, "GroupedPreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.IT.Business.GroupedPreviousDocumentCollection)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).GroupedPreviousDocuments)));
			this.GroupedPreviousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupedPreviousDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupedPreviousDocumentsUserControl.Name = "GroupedPreviousDocumentsUserControl";
			this.GroupedPreviousDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 462, true);
			this.GroupedPreviousDocumentsUserControl.TabIndex = 0;
			// 
			// RemarksTabPage
			// 
			this.RemarksTabPage.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("77D8832E-41B8-47A1-9F52-29083DDA7D14", "[44.15] Remarks");
			this.RemarksTabPage.Controls.Add(this.NctsGoodsItemRemarksUserControl);
			this.RemarksTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RemarksTabPage.Name = "RemarksTabPage";
			this.RemarksTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 462, true);
			this.RemarksTabPage.TabIndex = 8;
			// 
			// NctsGoodsItemRemarksUserControl
			//
			this.BindingSource.SetBindingMember(this.NctsGoodsItemRemarksUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)))));
			this.NctsGoodsItemRemarksUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NctsGoodsItemRemarksUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NctsGoodsItemRemarksUserControl.Name = "NctsGoodsItemRemarksUserControl";
			this.NctsGoodsItemRemarksUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 462, true);
			this.NctsGoodsItemRemarksUserControl.TabIndex = 0;
			this.NctsGoodsItemRemarksUserControl.TabStop = false;
			// 
			// NctsGoodsItemsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "NctsGoodsItemsUserControl";
			this.GoodsItemsTabControl.ResumeLayout(false);
			this.GoodsItemsTabControl.PerformLayout();
			this.ItemDetailsTabPage.ResumeLayout(false);
			this.ItemDetailsTabPage.PerformLayout();
			this.ItemContainersTabPage.ResumeLayout(false);
			this.ItemContainersTabPage.PerformLayout();
			this.ItemPackagesTabPage.ResumeLayout(false);
			this.ItemPackagesTabPage.PerformLayout();
			this.ItemPreviousDocumentsTabPage.ResumeLayout(false);
			this.ItemPreviousDocumentsTabPage.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			this.ItemAdditionalInfosTabPage.ResumeLayout(false);
			this.ItemAdditionalInfosTabPage.PerformLayout();
			this.ItemSecurityTabPage.ResumeLayout(false);
			this.ItemSecurityTabPage.PerformLayout();
			this.GoodsItemLineDetailDynamicUserControl.ResumeLayout(true);
			this.GoodsItemLineDetailDynamicUserControl.PerformLayout();
			this.GoodsItemsGridDynamicUserControl.ResumeLayout(true);
			this.GoodsItemsGridDynamicUserControl.PerformLayout();
			this.NctsPreviousDocumentsDynamicUserControl.ResumeLayout(true);
			this.NctsPreviousDocumentsDynamicUserControl.PerformLayout();
			this.NctsPackagesDynamicUserControl.ResumeLayout(true);
			this.NctsPackagesDynamicUserControl.PerformLayout();
			this.ItemSecurityDynamicUserControl.ResumeLayout(true);
			this.ItemSecurityDynamicUserControl.PerformLayout();
			this.AdditionalInfosDynamicUserControl.ResumeLayout(true);
			this.AdditionalInfosDynamicUserControl.PerformLayout();
			this.SupportingDocumentsDynamicUserControl.ResumeLayout(true);
			this.SupportingDocumentsDynamicUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ItemTaxOrFeeTabPage.ResumeLayout(false);
			this.ItemTaxOrFeeTabPage.PerformLayout();
			this.TaxOrFeeUserControl.ResumeLayout(true);
			this.TaxOrFeeUserControl.PerformLayout();
			this.M2LinesTabPage.ResumeLayout(false);
			this.M2LinesTabPage.PerformLayout();
			this.GroupedPreviousDocumentsUserControl.ResumeLayout(true);
			this.GroupedPreviousDocumentsUserControl.PerformLayout();
			this.RemarksTabPage.ResumeLayout(false);
			this.RemarksTabPage.PerformLayout();
			this.NctsGoodsItemRemarksUserControl.ResumeLayout(false);
			this.NctsGoodsItemRemarksUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZTabPage ItemTaxOrFeeTabPage;
		private NctsTaxOrFeeUserControl TaxOrFeeUserControl;
		protected ZArchitecture.GUI.ZTabPage M2LinesTabPage;
		private IT.GUI.GroupedPreviousDocumentsUserControl GroupedPreviousDocumentsUserControl;
		protected ZArchitecture.GUI.ZTabPage RemarksTabPage;
		private NCTS.GUI.NctsGoodsItemRemarksUserControl NctsGoodsItemRemarksUserControl;
	}
}
