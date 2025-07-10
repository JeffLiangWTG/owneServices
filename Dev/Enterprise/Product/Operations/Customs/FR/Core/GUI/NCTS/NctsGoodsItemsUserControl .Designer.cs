namespace Enterprise.Customs.FR.GUI.NCTS
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
			this.TaxOrFeeUserControl = new Enterprise.Customs.FR.GUI.NCTS.NctsTaxOrFeeUserControl();
			((System.ComponentModel.ISupportInitialize)(this.GoodsItemsSplitContainer)).BeginInit();
			this.GoodsItemsSplitContainer.Panel1.SuspendLayout();
			this.GoodsItemsSplitContainer.Panel2.SuspendLayout();
			this.GoodsItemsSplitContainer.SuspendLayout();
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
			this.NctsPackagesDynamicUserControl.SuspendLayout();
			this.ItemSecurityDynamicUserControl.SuspendLayout();
			this.AdditionalInfosDynamicUserControl.SuspendLayout();
			this.SupportingDocumentsDynamicUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ItemTaxOrFeeTabPage.SuspendLayout();
			this.TaxOrFeeUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// GoodsItemsSplitContainer
			// 
			// 
			// GoodsItemsTabControl
			// 
			this.GoodsItemsTabControl.Controls.Add(this.ItemTaxOrFeeTabPage);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemSecurityTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemPreviousDocumentsTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemAdditionalInfosTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.SupportingDocumentsTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemContainersTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemPackagesTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemDetailsTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemTaxOrFeeTabPage, 0);
			// 
			// NctsPreviousDocumentsDynamicUserControl
			// 
			this.NctsPreviousDocumentsDynamicUserControl.UserControlType = typeof(Enterprise.Customs.FR.GUI.NCTS.NctsPreviousDocumentsUserControl);
			// 
			// ItemTaxOrFeeTabPage
			// 
			this.ItemTaxOrFeeTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("3F03C680-9C7B-416E-B879-CF7EEE4D836B", "[47] Tax or Fee");
			this.ItemTaxOrFeeTabPage.Controls.Add(this.TaxOrFeeUserControl);
			this.ItemTaxOrFeeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ItemTaxOrFeeTabPage.Name = "ItemTaxOrFeeTabPage";
			this.ItemTaxOrFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.ItemTaxOrFeeTabPage.TabIndex = 8;
			// 
			// TaxOrFeeUserControl
			// 
			this.TaxOrFeeUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxOrFeeUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.FR.Business.NCTS.NctsDepartureCargoDesc)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)))));
			this.TaxOrFeeUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxOrFeeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxOrFeeUserControl.Name = "TaxOrFeeUserControl";
			this.TaxOrFeeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.TaxOrFeeUserControl.TabIndex = 0;
			// 
			// NctsGoodsItemsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "NctsGoodsItemsUserControl";
			this.GoodsItemsSplitContainer.Panel1.ResumeLayout(false);
			this.GoodsItemsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.GoodsItemsSplitContainer)).EndInit();
			this.GoodsItemsSplitContainer.ResumeLayout(false);
			this.GoodsItemsSplitContainer.PerformLayout();
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
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected ZArchitecture.GUI.ZTabPage ItemTaxOrFeeTabPage;
		private NctsTaxOrFeeUserControl TaxOrFeeUserControl;
		#endregion
	}
}
