namespace Enterprise.Customs.CA.GUI
{
	partial class LVSLineChargesUserControl
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
			this.ChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).BeginInit();
			this.ApportionedChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 200, true);
			this.ChargesGroupBox.TabIndex = 0;
			// 
			// ChargesGrid
			// 
			this.BindingSource.SetBindingMember(this.ChargesGrid, "Charges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).Charges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.InvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).Charges)).SyncRoot)).J7_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.InvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).Charges)).SyncRoot)).Lookups.ChargeTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.InvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).Charges)).SyncRoot)).ChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.InvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).Charges)).SyncRoot)).J7_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.InvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).Charges)).SyncRoot)).J7_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.InvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).Charges)).SyncRoot)).Lookups.Currencies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.InvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).Charges)).SyncRoot)).J7_IsDutiable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.InvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).Charges)).SyncRoot)).J7_IsGSTApplicable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.InvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).Charges)).SyncRoot)).NoOfDecimalsForPercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.InvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).Charges)).SyncRoot)).J7_Percentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.InvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).Charges)).SyncRoot)).J7_IsIncludedInITOT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.InvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).Charges)).SyncRoot)).J7_Calc_IsIncludedInInvoiceAmount)));
			this.ChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 181, true);
			// 
			// ApportionedChargesGroupBox
			// 
			this.ApportionedChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 200, true);
			this.ApportionedChargesGroupBox.TabIndex = 0;
			// 
			// ApportionedChargesGrid
			// 
			this.BindingSource.SetBindingMember(this.ApportionedChargesGrid, "ApportionedCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).ApportionedCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.InvoiceLineApportionCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).ApportionedCharges)).SyncRoot)).J7_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.InvoiceLineApportionCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).ApportionedCharges)).SyncRoot)).Lookups.ChargeTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.InvoiceLineApportionCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).ApportionedCharges)).SyncRoot)).ChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.InvoiceLineApportionCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).ApportionedCharges)).SyncRoot)).J7_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.InvoiceLineApportionCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).ApportionedCharges)).SyncRoot)).J7_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.InvoiceLineApportionCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).ApportionedCharges)).SyncRoot)).Lookups.Currencies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.InvoiceLineApportionCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).ApportionedCharges)).SyncRoot)).J7_IsDutiable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.InvoiceLineApportionCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).ApportionedCharges)).SyncRoot)).J7_IsGSTApplicable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.InvoiceLineApportionCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).ApportionedCharges)).SyncRoot)).J7_IsIncludedInITOT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.InvoiceLineApportionCharge)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).ApportionedCharges)).SyncRoot)).J7_FullOrPartialApportionment)));
			this.ApportionedChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 181, true);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.SplitContainer.Panel1MinSize = 200;
			this.SplitContainer.Panel2MinSize = 200;
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 200, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobComInvoiceLine);
			// 
			// LVSLineChargesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.CaptionRenderingEnabled = true;
			this.Name = "LVSLineChargesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 200, true);
			this.ChargesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).EndInit();
			this.ApportionedChargesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			this.SplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
