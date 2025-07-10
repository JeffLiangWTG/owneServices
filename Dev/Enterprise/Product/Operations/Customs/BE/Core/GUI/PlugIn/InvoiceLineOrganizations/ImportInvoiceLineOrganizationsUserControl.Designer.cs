namespace Enterprise.Customs.BE.GUI.PlugIn
{
	partial class ImportInvoiceLineOrganizationsUserControl
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
			this.BuyerDocAddressControl.SuspendLayout();
			this.SellerDocAddressControl.SuspendLayout();
			this.ConsignorAddressControl.SuspendLayout();
			this.ConsigneeAddressControl.SuspendLayout();
			this.SupplyChainActorReferencesUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BuyerDocAddressControl
			// 
			this.BuyerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 43, true);
			// 
			// SellerDocAddressControl
			// 
			this.SellerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 68, true);
			// 
			// ConsignorAddressControl
			// 
			this.ConsignorAddressControl.BindToOrgList = "FilteredInvoiceLines.Lookups+ExporterList";
			this.ConsignorAddressControl.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("08939440-edc9-4382-85dd-61d6754791de", "Consignor");
			// 
			// ConsigneeAddressControl
			// 
			this.ConsigneeAddressControl.Visible = false;
			// 
			// ImportInvoiceLineOrganizationsUserControl
			// 
			this.Name = "ImportInvoiceLineOrganizationsUserControl";
			this.BuyerDocAddressControl.ResumeLayout(true);
			this.BuyerDocAddressControl.PerformLayout();
			this.SellerDocAddressControl.ResumeLayout(true);
			this.SellerDocAddressControl.PerformLayout();
			this.ConsignorAddressControl.ResumeLayout(true);
			this.ConsignorAddressControl.PerformLayout();
			this.ConsigneeAddressControl.ResumeLayout(true);
			this.ConsigneeAddressControl.PerformLayout();
			this.SupplyChainActorReferencesUserControl.ResumeLayout(true);
			this.SupplyChainActorReferencesUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
