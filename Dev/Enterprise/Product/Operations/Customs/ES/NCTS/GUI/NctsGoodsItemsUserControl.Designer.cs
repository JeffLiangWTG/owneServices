namespace Enterprise.Customs.ES.NCTS.GUI
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
			this.SuspendLayout();
			// 
			// GoodsItemsTabControl
			// 
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemSecurityTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemPreviousDocumentsTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemAdditionalInfosTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.SupportingDocumentsTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemContainersTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemPackagesTabPage, 0);
			this.GoodsItemsTabControl.Controls.SetChildIndex(this.ItemDetailsTabPage, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.INctsDepartureCargoDescCollection<Enterprise.Customs.ES.NCTS.Business.NctsDepartureCargoDesc>);
			// 
			// NctsGoodsItemsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
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
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
