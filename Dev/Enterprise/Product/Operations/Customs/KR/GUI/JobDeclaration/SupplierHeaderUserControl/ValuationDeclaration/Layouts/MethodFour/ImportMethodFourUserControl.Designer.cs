namespace Enterprise.Customs.KR.GUI
{
	partial class ImportMethodFourUserControl
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
			this.SalesOfHighestQuantityAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.DeductionCostCustomsReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SalesOfHighestQuantityExchangeRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SalesOfHighestQuantityAmountKRWCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeductionCostConsignmentSalesFeeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeductionCostGeneralCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeductionCostCostRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeductionCostTransportationCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeductionCosInsuranceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeductionCostUnloadCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeductionCostOtherTransportationCostsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PercentageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeductionCostAdditionalCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeductionCosTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeductionCostTotalDeductionAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeductionCostCostRateCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SalesOfHighestQuantityAmountCalcFindBox.SuspendLayout();
			this.DeductionCostCostRateCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceHeader);
			// 
			// SalesOfHighestQuantityAmountCalcFindBox
			// 
			this.SalesOfHighestQuantityAmountCalcFindBox.AllowDrop = true;
			this.SalesOfHighestQuantityAmountCalcFindBox.BindToAmount = "DeductionCostAmount";
			this.SalesOfHighestQuantityAmountCalcFindBox.BindToUnit = "ReplacementCurrency";
			this.SalesOfHighestQuantityAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.SalesOfHighestQuantityAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 17, true);
			this.SalesOfHighestQuantityAmountCalcFindBox.Name = "SalesOfHighestQuantityAmountCalcFindBox";
			this.SalesOfHighestQuantityAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.SalesOfHighestQuantityAmountCalcFindBox.TabIndex = 0;
			// 
			// DeductionCostCustomsReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeductionCostCustomsReferenceNumberTextBox, "CustomsReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).CustomsReferenceNumber)));
			this.DeductionCostCustomsReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 101, true);
			this.DeductionCostCustomsReferenceNumberTextBox.Name = "DeductionCostCustomsReferenceNumberTextBox";
			this.DeductionCostCustomsReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 18, true);
			this.DeductionCostCustomsReferenceNumberTextBox.TabIndex = 1;
			// 
			// SalesOfHighestQuantityExchangeRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SalesOfHighestQuantityExchangeRateCalcEdit, "JZ_InvoiceCurrExRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_InvoiceCurrExRate)));
			this.SalesOfHighestQuantityExchangeRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 17, true);
			this.SalesOfHighestQuantityExchangeRateCalcEdit.Name = "SalesOfHighestQuantityExchangeRateCalcEdit";
			this.SalesOfHighestQuantityExchangeRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.SalesOfHighestQuantityExchangeRateCalcEdit.TabIndex = 2;
			this.SalesOfHighestQuantityExchangeRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.SalesOfHighestQuantityExchangeRateCalcEdit.TrackDisposedAccess = true;
			// 
			// SalesOfHighestQuantityAmountKRWCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SalesOfHighestQuantityAmountKRWCalcEdit, "ReplacementAmountKRW");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).ReplacementAmountKRW)));
			this.SalesOfHighestQuantityAmountKRWCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 50, true);
			this.SalesOfHighestQuantityAmountKRWCalcEdit.Name = "SalesOfHighestQuantityAmountKRWCalcEdit";
			this.SalesOfHighestQuantityAmountKRWCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.SalesOfHighestQuantityAmountKRWCalcEdit.TabIndex = 3;
			this.SalesOfHighestQuantityAmountKRWCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.SalesOfHighestQuantityAmountKRWCalcEdit.TrackDisposedAccess = true;
			// 
			// DeductionCostConsignmentSalesFeeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeductionCostConsignmentSalesFeeCalcEdit, "ConsignmentSalesFee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).ConsignmentSalesFee)));
			this.DeductionCostConsignmentSalesFeeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 101, true);
			this.DeductionCostConsignmentSalesFeeCalcEdit.Name = "DeductionCostConsignmentSalesFeeCalcEdit";
			this.DeductionCostConsignmentSalesFeeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.DeductionCostConsignmentSalesFeeCalcEdit.TabIndex = 4;
			this.DeductionCostConsignmentSalesFeeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DeductionCostConsignmentSalesFeeCalcEdit.TrackDisposedAccess = true;
			// 
			// DeductionCostGeneralCostCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeductionCostGeneralCostCalcEdit, "GeneralCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).GeneralCost)));
			this.DeductionCostGeneralCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 129, true);
			this.DeductionCostGeneralCostCalcEdit.Name = "DeductionCostGeneralCostCalcEdit";
			this.DeductionCostGeneralCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.DeductionCostGeneralCostCalcEdit.TabIndex = 5;
			this.DeductionCostGeneralCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DeductionCostGeneralCostCalcEdit.TrackDisposedAccess = true;
			// 
			// DeductionCostCostRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeductionCostCostRateCalcEdit, "JZ_DeductionRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_DeductionRate)));
			this.DeductionCostCostRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 158, true);
			this.DeductionCostCostRateCalcEdit.Name = "DeductionCostCostRateCalcEdit";
			this.DeductionCostCostRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.DeductionCostCostRateCalcEdit.TabIndex = 7;
			this.DeductionCostCostRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DeductionCostCostRateCalcEdit.TrackDisposedAccess = true;
			// 
			// DeductionCostTransportationCostCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeductionCostTransportationCostCalcEdit, "DeductionTransportationCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).DeductionTransportationCost)));
			this.DeductionCostTransportationCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 158, true);
			this.DeductionCostTransportationCostCalcEdit.Name = "DeductionCostTransportationCostCalcEdit";
			this.DeductionCostTransportationCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.DeductionCostTransportationCostCalcEdit.TabIndex = 8;
			this.DeductionCostTransportationCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DeductionCostTransportationCostCalcEdit.TrackDisposedAccess = true;
			// 
			// DeductionCosInsuranceCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeductionCosInsuranceCalcEdit, "DeductionInsurance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).DeductionInsurance)));
			this.DeductionCosInsuranceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 188, true);
			this.DeductionCosInsuranceCalcEdit.Name = "DeductionCosInsuranceCalcEdit";
			this.DeductionCosInsuranceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.DeductionCosInsuranceCalcEdit.TabIndex = 9;
			this.DeductionCosInsuranceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DeductionCosInsuranceCalcEdit.TrackDisposedAccess = true;
			// 
			// DeductionCostUnloadCostCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeductionCostUnloadCostCalcEdit, "DeductionUnloadCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).DeductionUnloadCost)));
			this.DeductionCostUnloadCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 188, true);
			this.DeductionCostUnloadCostCalcEdit.Name = "DeductionCostUnloadCostCalcEdit";
			this.DeductionCostUnloadCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.DeductionCostUnloadCostCalcEdit.TabIndex = 10;
			this.DeductionCostUnloadCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DeductionCostUnloadCostCalcEdit.TrackDisposedAccess = true;
			// 
			// DeductionCostOtherTransportationCostsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeductionCostOtherTransportationCostsCalcEdit, "OtherTransportationCosts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).OtherTransportationCosts)));
			this.DeductionCostOtherTransportationCostsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 222, true);
			this.DeductionCostOtherTransportationCostsCalcEdit.Name = "DeductionCostOtherTransportationCostsCalcEdit";
			this.DeductionCostOtherTransportationCostsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.DeductionCostOtherTransportationCostsCalcEdit.TabIndex = 11;
			this.DeductionCostOtherTransportationCostsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DeductionCostOtherTransportationCostsCalcEdit.TrackDisposedAccess = true;
			// 
			// PercentageLabel
			// 
			this.PercentageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PercentageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 158, true);
			this.PercentageLabel.Name = "PercentageLabel";
			this.PercentageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 23, true);
			this.PercentageLabel.TabIndex = 12;
			this.PercentageLabel.Text = "%";
			this.PercentageLabel.UseMnemonic = false;
			// 
			// DeductionCostAdditionalCostCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeductionCostAdditionalCostCalcEdit, "AdditionalCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).AdditionalCost)));
			this.DeductionCostAdditionalCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 222, true);
			this.DeductionCostAdditionalCostCalcEdit.Name = "DeductionCostAdditionalCostCalcEdit";
			this.DeductionCostAdditionalCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.DeductionCostAdditionalCostCalcEdit.TabIndex = 13;
			this.DeductionCostAdditionalCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DeductionCostAdditionalCostCalcEdit.TrackDisposedAccess = true;
			// 
			// DeductionCosTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeductionCosTaxCalcEdit, "Tax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).Tax)));
			this.DeductionCosTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 255, true);
			this.DeductionCosTaxCalcEdit.Name = "DeductionCosTaxCalcEdit";
			this.DeductionCosTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.DeductionCosTaxCalcEdit.TabIndex = 14;
			this.DeductionCosTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DeductionCosTaxCalcEdit.TrackDisposedAccess = true;
			// 
			// DeductionCostTotalDeductionAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeductionCostTotalDeductionAmountCalcEdit, "DeductionCostTotalDeductionAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).DeductionCostTotalDeductionAmount)));
			this.DeductionCostTotalDeductionAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 255, true);
			this.DeductionCostTotalDeductionAmountCalcEdit.Name = "DeductionCostTotalDeductionAmountCalcEdit";
			this.DeductionCostTotalDeductionAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.DeductionCostTotalDeductionAmountCalcEdit.TabIndex = 15;
			this.DeductionCostTotalDeductionAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DeductionCostTotalDeductionAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// DeductionCostCostRateCodeDropEdit
			// 
			this.DeductionCostCostRateCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeductionCostCostRateCodeDropEdit, "JZ_DeductionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_DeductionType)));
			this.DeductionCostCostRateCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 123, true);
			this.DeductionCostCostRateCodeDropEdit.Name = "DeductionCostCostRateCodeDropEdit";
			this.DeductionCostCostRateCodeDropEdit.PreBoundMaxLength = 1;
			this.DeductionCostCostRateCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 18, true);
			this.DeductionCostCostRateCodeDropEdit.TabIndex = 16;
			// 
			// ImportMethodFourUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.DeductionCostCostRateCodeDropEdit);
			this.Controls.Add(this.DeductionCostTotalDeductionAmountCalcEdit);
			this.Controls.Add(this.DeductionCosTaxCalcEdit);
			this.Controls.Add(this.DeductionCostAdditionalCostCalcEdit);
			this.Controls.Add(this.PercentageLabel);
			this.Controls.Add(this.DeductionCostOtherTransportationCostsCalcEdit);
			this.Controls.Add(this.DeductionCostUnloadCostCalcEdit);
			this.Controls.Add(this.DeductionCosInsuranceCalcEdit);
			this.Controls.Add(this.DeductionCostTransportationCostCalcEdit);
			this.Controls.Add(this.DeductionCostCostRateCalcEdit);
			this.Controls.Add(this.DeductionCostGeneralCostCalcEdit);
			this.Controls.Add(this.DeductionCostConsignmentSalesFeeCalcEdit);
			this.Controls.Add(this.SalesOfHighestQuantityAmountKRWCalcEdit);
			this.Controls.Add(this.SalesOfHighestQuantityExchangeRateCalcEdit);
			this.Controls.Add(this.DeductionCostCustomsReferenceNumberTextBox);
			this.Controls.Add(this.SalesOfHighestQuantityAmountCalcFindBox);
			this.Name = "ImportMethodFourUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 303, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SalesOfHighestQuantityAmountCalcFindBox.ResumeLayout(true);
			this.SalesOfHighestQuantityAmountCalcFindBox.PerformLayout();
			this.DeductionCostCostRateCodeDropEdit.ResumeLayout(true);
			this.DeductionCostCostRateCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCalcFindBox SalesOfHighestQuantityAmountCalcFindBox;
		internal ZArchitecture.ZTextBox DeductionCostCustomsReferenceNumberTextBox;
		internal ZArchitecture.ZCalcEdit SalesOfHighestQuantityExchangeRateCalcEdit;
		internal ZArchitecture.ZCalcEdit SalesOfHighestQuantityAmountKRWCalcEdit;
		internal ZArchitecture.ZCalcEdit DeductionCostConsignmentSalesFeeCalcEdit;
		internal ZArchitecture.ZCalcEdit DeductionCostGeneralCostCalcEdit;
		internal ZArchitecture.ZCalcEdit DeductionCostCostRateCalcEdit;
		internal ZArchitecture.ZCalcEdit DeductionCostTransportationCostCalcEdit;
		internal ZArchitecture.ZCalcEdit DeductionCosInsuranceCalcEdit;
		internal ZArchitecture.ZCalcEdit DeductionCostUnloadCostCalcEdit;
		internal ZArchitecture.ZCalcEdit DeductionCostOtherTransportationCostsCalcEdit;
		internal ZArchitecture.ZLabel PercentageLabel;
		internal ZArchitecture.ZCalcEdit DeductionCostAdditionalCostCalcEdit;
		internal ZArchitecture.ZCalcEdit DeductionCosTaxCalcEdit;
		internal ZArchitecture.ZCalcEdit DeductionCostTotalDeductionAmountCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit DeductionCostCostRateCodeDropEdit;
	}
}
