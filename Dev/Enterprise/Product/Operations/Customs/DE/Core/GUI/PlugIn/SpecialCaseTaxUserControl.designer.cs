namespace Enterprise.Customs.DE.GUI.PlugIn
{
	partial class SpecialCaseTaxUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.TaxJLT_TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TaxJLT_MethodOfCalculationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TaxJLT_RateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.TaxSplitContainer)).BeginInit();
			this.TaxSplitContainer.Panel1.SuspendLayout();
			this.TaxSplitContainer.Panel2.SuspendLayout();
			this.TaxSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaxGrid)).BeginInit();
			this.TaxGrid.SuspendLayout();
			this.TaxGroupBox.SuspendLayout();
			this.TaxMethodOfPaymentDropEdit.SuspendLayout();
			this.TaxBaseQuantityUQDropEdit.SuspendLayout();
			this.TaxRateGroupBox.SuspendLayout();
			this.TaxRateDutyDropEdit.SuspendLayout();
			this.TaxTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TaxJLT_TypeDropEdit.SuspendLayout();
			this.TaxJLT_MethodOfCalculationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// TaxSplitContainer
			// 
			// 
			// TaxGrid
			// 
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JLT_Type";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JLT_MethodOfCalculation";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JLT_Rate";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TaxGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.TaxGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.TaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			// 
			// TaxGroupBox
			// 
			this.TaxGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("5f82530a-a009-4d0a-bbad-28cd499a0fd8", "Special Cases");
			this.TaxGroupBox.Controls.Add(this.TaxJLT_TypeDropEdit);
			this.TaxGroupBox.Controls.Add(this.TaxJLT_MethodOfCalculationDropEdit);
			this.TaxGroupBox.Controls.Add(this.TaxJLT_RateCalcEdit);
			this.TaxGroupBox.Controls.SetChildIndex(this.TaxJLT_RateCalcEdit, 0);
			this.TaxGroupBox.Controls.SetChildIndex(this.TaxJLT_MethodOfCalculationDropEdit, 0);
			this.TaxGroupBox.Controls.SetChildIndex(this.TaxJLT_TypeDropEdit, 0);
			this.TaxGroupBox.Controls.SetChildIndex(this.TaxTypeDropEdit, 0);
			this.TaxGroupBox.Controls.SetChildIndex(this.TaxBaseQtyCalcEdit, 0);
			this.TaxGroupBox.Controls.SetChildIndex(this.TaxBaseAmountCalcEdit, 0);
			this.TaxGroupBox.Controls.SetChildIndex(this.TaxAmountCalcEdit, 0);
			this.TaxGroupBox.Controls.SetChildIndex(this.TaxRateGroupBox, 0);
			this.TaxGroupBox.Controls.SetChildIndex(this.TaxBaseQuantityUQDropEdit, 0);
			this.TaxGroupBox.Controls.SetChildIndex(this.TaxMethodOfPaymentDropEdit, 0);
			// 
			// TaxMethodOfPaymentDropEdit
			// 
			this.TaxMethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 109, true);
			this.TaxMethodOfPaymentDropEdit.TabIndex = 4;
			this.TaxMethodOfPaymentDropEdit.Visible = false;
			// 
			// TaxBaseQuantityUQDropEdit
			// 
			this.TaxBaseQuantityUQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 97, true);
			this.TaxBaseQuantityUQDropEdit.TabIndex = 3;
			this.TaxBaseQuantityUQDropEdit.Visible = false;
			// 
			// TaxRateGroupBox
			// 
			this.TaxRateGroupBox.TabIndex = 9;
			this.TaxRateGroupBox.Visible = false;
			// 
			// TaxRateDutyDropEdit
			// 
			this.TaxRateDutyDropEdit.TabIndex = 0;
			// 
			// TaxAmountCalcEdit
			// 
			this.TaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 149, true);
			this.TaxAmountCalcEdit.Visible = false;
			// 
			// TaxBaseAmountCalcEdit
			// 
			this.TaxBaseAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 149, true);
			this.TaxBaseAmountCalcEdit.TabIndex = 8;
			this.TaxBaseAmountCalcEdit.Visible = false;
			// 
			// TaxBaseQtyCalcEdit
			// 
			this.TaxBaseQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 123, true);
			this.TaxBaseQtyCalcEdit.TabIndex = 6;
			this.TaxBaseQtyCalcEdit.Visible = false;
			// 
			// TaxTypeDropEdit
			// 
			this.TaxTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 135, true);
			this.TaxTypeDropEdit.TabIndex = 5;
			this.TaxTypeDropEdit.Visible = false;
			// 
			// TaxJLT_TypeDropEdit
			// 
			this.TaxJLT_TypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxJLT_TypeDropEdit, "FilteredInvoiceLines.Taxes.JLT_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Taxes)).SyncRoot)).JLT_Type)));
			this.TaxJLT_TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 19, true);
			this.TaxJLT_TypeDropEdit.Name = "TaxJLT_TypeDropEdit";
			this.TaxJLT_TypeDropEdit.PreBoundMaxLength = 2;
			this.TaxJLT_TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.TaxJLT_TypeDropEdit.TabIndex = 0;
			// 
			// TaxJLT_MethodOfCalculationDropEdit
			// 
			this.TaxJLT_MethodOfCalculationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxJLT_MethodOfCalculationDropEdit, "FilteredInvoiceLines.Taxes.JLT_MethodOfCalculation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Taxes)).SyncRoot)).JLT_MethodOfCalculation)));
			this.TaxJLT_MethodOfCalculationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 45, true);
			this.TaxJLT_MethodOfCalculationDropEdit.Name = "TaxJLT_MethodOfCalculationDropEdit";
			this.TaxJLT_MethodOfCalculationDropEdit.PreBoundMaxLength = 2;
			this.TaxJLT_MethodOfCalculationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.TaxJLT_MethodOfCalculationDropEdit.TabIndex = 1;
			// 
			// TaxJLT_RateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TaxJLT_RateCalcEdit, "FilteredInvoiceLines.Taxes.JLT_Rate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLineTax)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Taxes)).SyncRoot)).JLT_Rate)));
			this.TaxJLT_RateCalcEdit.CaptionResourceString = null;
			this.TaxJLT_RateCalcEdit.DecimalPlaces = 5;
			this.TaxJLT_RateCalcEdit.Decimals = 5;
			this.TaxJLT_RateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 71, true);
			this.TaxJLT_RateCalcEdit.Name = "TaxJLT_RateCalcEdit";
			this.TaxJLT_RateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.TaxJLT_RateCalcEdit.TabIndex = 2;
			this.TaxJLT_RateCalcEdit.Text = "0.00000";
			this.TaxJLT_RateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SpecialCaseTaxUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "SpecialCaseTaxUserControl";
			this.TaxSplitContainer.Panel1.ResumeLayout(false);
			this.TaxSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TaxSplitContainer)).EndInit();
			this.TaxSplitContainer.ResumeLayout(false);
			this.TaxSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaxGrid)).EndInit();
			this.TaxGrid.ResumeLayout(false);
			this.TaxGrid.PerformLayout();
			this.TaxGroupBox.ResumeLayout(false);
			this.TaxGroupBox.PerformLayout();
			this.TaxMethodOfPaymentDropEdit.ResumeLayout(true);
			this.TaxMethodOfPaymentDropEdit.PerformLayout();
			this.TaxBaseQuantityUQDropEdit.ResumeLayout(true);
			this.TaxBaseQuantityUQDropEdit.PerformLayout();
			this.TaxRateGroupBox.ResumeLayout(false);
			this.TaxRateGroupBox.PerformLayout();
			this.TaxRateDutyDropEdit.ResumeLayout(true);
			this.TaxRateDutyDropEdit.PerformLayout();
			this.TaxTypeDropEdit.ResumeLayout(true);
			this.TaxTypeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TaxJLT_TypeDropEdit.ResumeLayout(true);
			this.TaxJLT_TypeDropEdit.PerformLayout();
			this.TaxJLT_MethodOfCalculationDropEdit.ResumeLayout(true);
			this.TaxJLT_MethodOfCalculationDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected ZArchitecture.GUI.ZDropEdit TaxJLT_TypeDropEdit;
		protected ZArchitecture.GUI.ZDropEdit TaxJLT_MethodOfCalculationDropEdit;
		protected ZArchitecture.ZCalcEdit TaxJLT_RateCalcEdit;
		#endregion
	}
}
