namespace Enterprise.Customs.AsycudaCustoms.GUI.CommercialInvoice
{
	partial class InvoiceHeaderUserControl
	{
		#region Designer Generated

		void InitializeComponent()
		{
			this.ChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AsycudaCustoms.Business.JobDeclaration);
			// 
			// InvoiceHeaderUserControl
			// 
			this.Name = "InvoiceHeaderUserControl";
			this.ChargesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
