namespace Enterprise.Customs.MY.GUI
{
	public partial class InvoiceHeaderUserControl : Customs.GUI.InvoiceHeaderUserControl
	{
		void InitializeComponent()
		{
			this.ChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ShipmentTypeGroupBox
			// 
			this.ShipmentTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 44, true);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceNumberTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_IncoTermDropDownEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceAmountCurrencyControl, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.GrossWeightCalcDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceCurrExRateCalcEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.JobDeclaration);
			// 
			// InvoiceHeaderUserControl
			// 
			this.Name = "InvoiceHeaderUserControl";
			this.ChargesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.ShipmentTypeGroupBox.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
