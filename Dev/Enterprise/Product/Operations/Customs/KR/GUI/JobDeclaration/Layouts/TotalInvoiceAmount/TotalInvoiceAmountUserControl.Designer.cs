using CargoWise.Windows.UI;

namespace Enterprise.Customs.KR.GUI
{
	partial class TotalInvoiceAmountUserControl
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
			this.TotalInvoiceAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InvoiceCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// TotalInvoiceAmountCalcEdit
			// 
			this.TotalInvoiceAmountCalcEdit.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.TotalInvoiceAmountCalcEdit, "TotalInvoiceAmountValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).TotalInvoiceAmountValue)));
			this.TotalInvoiceAmountCalcEdit.DecimalPlaces = 2;
			this.TotalInvoiceAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TotalInvoiceAmountCalcEdit.Name = "TotalInvoiceAmountCalcEdit";
			this.TotalInvoiceAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.TotalInvoiceAmountCalcEdit.TabIndex = 0;
			this.TotalInvoiceAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalInvoiceAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// InvoiceCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.InvoiceCurrencyTextBox, "Invoices.JZ_RX_NKInvoice_Currency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_RX_NKInvoice_Currency)));
			this.InvoiceCurrencyTextBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.InvoiceCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 0, true);
			this.InvoiceCurrencyTextBox.Name = "InvoiceCurrencyTextBox";
			this.InvoiceCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 20, true);
			this.InvoiceCurrencyTextBox.TabIndex = 1;
			this.InvoiceCurrencyTextBox.SetReadOnly(true);
			// 
			// TotalInvoiceAmountUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoiceCurrencyTextBox);
			this.Controls.Add(this.TotalInvoiceAmountCalcEdit);
			this.Name = "TotalInvoiceAmountUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZCalcEdit TotalInvoiceAmountCalcEdit;
		public ZArchitecture.ZTextBox InvoiceCurrencyTextBox;
	}
}
