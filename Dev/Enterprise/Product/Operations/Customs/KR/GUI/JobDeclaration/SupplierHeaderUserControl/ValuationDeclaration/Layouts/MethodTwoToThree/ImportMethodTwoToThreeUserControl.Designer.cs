namespace Enterprise.Customs.KR.GUI
{
	partial class ImportMethodTwoToThreeUserControl
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
            this.ReplacementAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
            this.ReplacementExchangeRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ReplacementAmountKRWCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.AdditionalAdjustmentQuantityDiscountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.AdditionalAdjustmentCommercialAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.AdditionalAdjustmentTransportationCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.AdditionalAdjustmentShippingPortCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.AdditionalAdjustmentInsuranceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.TotalAdditionalAdjustmentAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.DeductionAdjustmentQuantityDiscountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.DeductionAdjustmentCommercialAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.DeductionAdjustmentTransportationCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.DeductionAdjustmentShippingPortCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.DeductionAdjustmentInsuranceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.TotalDeductionAdjustmentAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ReplacementAmountCalcFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceHeader);
            // 
            // ReplacementAmountCalcFindBox
            // 
            this.ReplacementAmountCalcFindBox.AllowDrop = true;
            this.ReplacementAmountCalcFindBox.BindToAmount = "ReplaceAmount";
            this.ReplacementAmountCalcFindBox.BindToUnit = "ReplacementCurrency";
            this.ReplacementAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.ReplacementAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 21, true);
            this.ReplacementAmountCalcFindBox.Name = "ReplacementAmountCalcFindBox";
            this.ReplacementAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.ReplacementAmountCalcFindBox.TabIndex = 0;
            // 
            // ReplacementExchangeRateCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ReplacementExchangeRateCalcEdit, "JZ_InvoiceCurrExRate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_InvoiceCurrExRate)));
            this.ReplacementExchangeRateCalcEdit.DecimalPlaces = 4;
            this.ReplacementExchangeRateCalcEdit.Decimals = 4;
            this.ReplacementExchangeRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 23, true);
            this.ReplacementExchangeRateCalcEdit.Name = "ReplacementExchangeRateCalcEdit";
            this.ReplacementExchangeRateCalcEdit.ReadOnly = true;
            this.ReplacementExchangeRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
            this.ReplacementExchangeRateCalcEdit.TabIndex = 1;
            this.ReplacementExchangeRateCalcEdit.Text = "0.0000";
            this.ReplacementExchangeRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ReplacementExchangeRateCalcEdit.TrackDisposedAccess = true;
            // 
            // ReplacementAmountKRWCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ReplacementAmountKRWCalcEdit, "ReplacementAmountKRW");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).ReplacementAmountKRW)));
            this.ReplacementAmountKRWCalcEdit.DecimalPlaces = 2;
            this.ReplacementAmountKRWCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 50, true);
            this.ReplacementAmountKRWCalcEdit.Name = "ReplacementAmountKRWCalcEdit";
            this.ReplacementAmountKRWCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
            this.ReplacementAmountKRWCalcEdit.TabIndex = 2;
            this.ReplacementAmountKRWCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ReplacementAmountKRWCalcEdit.TrackDisposedAccess = true;
            // 
            // AdditionalAdjustmentQuantityDiscountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.AdditionalAdjustmentQuantityDiscountCalcEdit, "AdditionalAdjustmentQuantityDiscount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).AdditionalAdjustmentQuantityDiscount)));
            this.AdditionalAdjustmentQuantityDiscountCalcEdit.DecimalPlaces = 2;
            this.AdditionalAdjustmentQuantityDiscountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 96, true);
            this.AdditionalAdjustmentQuantityDiscountCalcEdit.Name = "AdditionalAdjustmentQuantityDiscountCalcEdit";
            this.AdditionalAdjustmentQuantityDiscountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
            this.AdditionalAdjustmentQuantityDiscountCalcEdit.TabIndex = 3;
            this.AdditionalAdjustmentQuantityDiscountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AdditionalAdjustmentQuantityDiscountCalcEdit.TrackDisposedAccess = true;
            // 
            // AdditionalAdjustmentCommercialAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.AdditionalAdjustmentCommercialAmountCalcEdit, "AdditionalAdjustmentCommercialAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).AdditionalAdjustmentCommercialAmount)));
            this.AdditionalAdjustmentCommercialAmountCalcEdit.DecimalPlaces = 2;
            this.AdditionalAdjustmentCommercialAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 96, true);
            this.AdditionalAdjustmentCommercialAmountCalcEdit.Name = "AdditionalAdjustmentCommercialAmountCalcEdit";
            this.AdditionalAdjustmentCommercialAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
            this.AdditionalAdjustmentCommercialAmountCalcEdit.TabIndex = 4;
            this.AdditionalAdjustmentCommercialAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AdditionalAdjustmentCommercialAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // AdditionalAdjustmentTransportationCostCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.AdditionalAdjustmentTransportationCostCalcEdit, "AdditionalAdjustmentTransportationCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).AdditionalAdjustmentTransportationCost)));
            this.AdditionalAdjustmentTransportationCostCalcEdit.DecimalPlaces = 2;
            this.AdditionalAdjustmentTransportationCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 129, true);
            this.AdditionalAdjustmentTransportationCostCalcEdit.Name = "AdditionalAdjustmentTransportationCostCalcEdit";
            this.AdditionalAdjustmentTransportationCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
            this.AdditionalAdjustmentTransportationCostCalcEdit.TabIndex = 5;
            this.AdditionalAdjustmentTransportationCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AdditionalAdjustmentTransportationCostCalcEdit.TrackDisposedAccess = true;
            // 
            // AdditionalAdjustmentShippingPortCostCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.AdditionalAdjustmentShippingPortCostCalcEdit, "AdditionalAdjustmentShippingPortCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).AdditionalAdjustmentShippingPortCost)));
            this.AdditionalAdjustmentShippingPortCostCalcEdit.DecimalPlaces = 2;
            this.AdditionalAdjustmentShippingPortCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 129, true);
            this.AdditionalAdjustmentShippingPortCostCalcEdit.Name = "AdditionalAdjustmentShippingPortCostCalcEdit";
            this.AdditionalAdjustmentShippingPortCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
            this.AdditionalAdjustmentShippingPortCostCalcEdit.TabIndex = 6;
            this.AdditionalAdjustmentShippingPortCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AdditionalAdjustmentShippingPortCostCalcEdit.TrackDisposedAccess = true;
            // 
            // AdditionalAdjustmentInsuranceCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.AdditionalAdjustmentInsuranceCalcEdit, "AdditionalAdjustmentInsurance");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).AdditionalAdjustmentInsurance)));
            this.AdditionalAdjustmentInsuranceCalcEdit.DecimalPlaces = 2;
            this.AdditionalAdjustmentInsuranceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 164, true);
            this.AdditionalAdjustmentInsuranceCalcEdit.Name = "AdditionalAdjustmentInsuranceCalcEdit";
            this.AdditionalAdjustmentInsuranceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
            this.AdditionalAdjustmentInsuranceCalcEdit.TabIndex = 7;
            this.AdditionalAdjustmentInsuranceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AdditionalAdjustmentInsuranceCalcEdit.TrackDisposedAccess = true;
            // 
            // TotalAdditionalAdjustmentAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.TotalAdditionalAdjustmentAmountCalcEdit, "TotalAdditionalAdjustmentAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).TotalAdditionalAdjustmentAmount)));
            this.TotalAdditionalAdjustmentAmountCalcEdit.DecimalPlaces = 2;
            this.TotalAdditionalAdjustmentAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 164, true);
            this.TotalAdditionalAdjustmentAmountCalcEdit.Name = "TotalAdditionalAdjustmentAmountCalcEdit";
            this.TotalAdditionalAdjustmentAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
            this.TotalAdditionalAdjustmentAmountCalcEdit.TabIndex = 8;
            this.TotalAdditionalAdjustmentAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TotalAdditionalAdjustmentAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // DeductionAdjustmentQuantityDiscountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.DeductionAdjustmentQuantityDiscountCalcEdit, "DeductionAdjustmentQuantityDiscount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).DeductionAdjustmentQuantityDiscount)));
            this.DeductionAdjustmentQuantityDiscountCalcEdit.DecimalPlaces = 2;
            this.DeductionAdjustmentQuantityDiscountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 213, true);
            this.DeductionAdjustmentQuantityDiscountCalcEdit.Name = "DeductionAdjustmentQuantityDiscountCalcEdit";
            this.DeductionAdjustmentQuantityDiscountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
            this.DeductionAdjustmentQuantityDiscountCalcEdit.TabIndex = 9;
            this.DeductionAdjustmentQuantityDiscountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.DeductionAdjustmentQuantityDiscountCalcEdit.TrackDisposedAccess = true;
            // 
            // DeductionAdjustmentCommercialAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.DeductionAdjustmentCommercialAmountCalcEdit, "DeductionAdjustmentCommercialAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).DeductionAdjustmentCommercialAmount)));
            this.DeductionAdjustmentCommercialAmountCalcEdit.DecimalPlaces = 2;
            this.DeductionAdjustmentCommercialAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 213, true);
            this.DeductionAdjustmentCommercialAmountCalcEdit.Name = "DeductionAdjustmentCommercialAmountCalcEdit";
            this.DeductionAdjustmentCommercialAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
            this.DeductionAdjustmentCommercialAmountCalcEdit.TabIndex = 10;
            this.DeductionAdjustmentCommercialAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.DeductionAdjustmentCommercialAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // DeductionAdjustmentTransportationCostCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.DeductionAdjustmentTransportationCostCalcEdit, "DeductionAdjustmentTransportationCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).DeductionAdjustmentTransportationCost)));
            this.DeductionAdjustmentTransportationCostCalcEdit.DecimalPlaces = 2;
            this.DeductionAdjustmentTransportationCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 249, true);
            this.DeductionAdjustmentTransportationCostCalcEdit.Name = "DeductionAdjustmentTransportationCostCalcEdit";
            this.DeductionAdjustmentTransportationCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
            this.DeductionAdjustmentTransportationCostCalcEdit.TabIndex = 11;
            this.DeductionAdjustmentTransportationCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.DeductionAdjustmentTransportationCostCalcEdit.TrackDisposedAccess = true;
            // 
            // DeductionAdjustmentShippingPortCostCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.DeductionAdjustmentShippingPortCostCalcEdit, "DeductionAdjustmentShippingPortCost");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).DeductionAdjustmentShippingPortCost)));
            this.DeductionAdjustmentShippingPortCostCalcEdit.DecimalPlaces = 2;
            this.DeductionAdjustmentShippingPortCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 249, true);
            this.DeductionAdjustmentShippingPortCostCalcEdit.Name = "DeductionAdjustmentShippingPortCostCalcEdit";
            this.DeductionAdjustmentShippingPortCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
            this.DeductionAdjustmentShippingPortCostCalcEdit.TabIndex = 12;
            this.DeductionAdjustmentShippingPortCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.DeductionAdjustmentShippingPortCostCalcEdit.TrackDisposedAccess = true;
            // 
            // DeductionAdjustmentInsuranceCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.DeductionAdjustmentInsuranceCalcEdit, "DeductionAdjustmentInsurance");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).DeductionAdjustmentInsurance)));
            this.DeductionAdjustmentInsuranceCalcEdit.DecimalPlaces = 2;
            this.DeductionAdjustmentInsuranceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 281, true);
            this.DeductionAdjustmentInsuranceCalcEdit.Name = "DeductionAdjustmentInsuranceCalcEdit";
            this.DeductionAdjustmentInsuranceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
            this.DeductionAdjustmentInsuranceCalcEdit.TabIndex = 13;
            this.DeductionAdjustmentInsuranceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.DeductionAdjustmentInsuranceCalcEdit.TrackDisposedAccess = true;
            // 
            // TotalDeductionAdjustmentAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.TotalDeductionAdjustmentAmountCalcEdit, "TotalDeductionAdjustmentAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).TotalDeductionAdjustmentAmount)));
            this.TotalDeductionAdjustmentAmountCalcEdit.DecimalPlaces = 2;
            this.TotalDeductionAdjustmentAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 281, true);
            this.TotalDeductionAdjustmentAmountCalcEdit.Name = "TotalDeductionAdjustmentAmountCalcEdit";
            this.TotalDeductionAdjustmentAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
            this.TotalDeductionAdjustmentAmountCalcEdit.TabIndex = 14;
            this.TotalDeductionAdjustmentAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TotalDeductionAdjustmentAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // ImportMethodTwoToThreeUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.TotalDeductionAdjustmentAmountCalcEdit);
            this.Controls.Add(this.DeductionAdjustmentInsuranceCalcEdit);
            this.Controls.Add(this.DeductionAdjustmentShippingPortCostCalcEdit);
            this.Controls.Add(this.DeductionAdjustmentTransportationCostCalcEdit);
            this.Controls.Add(this.DeductionAdjustmentCommercialAmountCalcEdit);
            this.Controls.Add(this.DeductionAdjustmentQuantityDiscountCalcEdit);
            this.Controls.Add(this.TotalAdditionalAdjustmentAmountCalcEdit);
            this.Controls.Add(this.AdditionalAdjustmentInsuranceCalcEdit);
            this.Controls.Add(this.AdditionalAdjustmentShippingPortCostCalcEdit);
            this.Controls.Add(this.AdditionalAdjustmentTransportationCostCalcEdit);
            this.Controls.Add(this.AdditionalAdjustmentCommercialAmountCalcEdit);
            this.Controls.Add(this.AdditionalAdjustmentQuantityDiscountCalcEdit);
            this.Controls.Add(this.ReplacementAmountKRWCalcEdit);
            this.Controls.Add(this.ReplacementExchangeRateCalcEdit);
            this.Controls.Add(this.ReplacementAmountCalcFindBox);
            this.Name = "ImportMethodTwoToThreeUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 361, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ReplacementAmountCalcFindBox.ResumeLayout(true);
            this.ReplacementAmountCalcFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZCalcFindBox ReplacementAmountCalcFindBox;
		public ZArchitecture.ZCalcEdit ReplacementExchangeRateCalcEdit;
		public ZArchitecture.ZCalcEdit ReplacementAmountKRWCalcEdit;

		public ZArchitecture.ZCalcEdit AdditionalAdjustmentQuantityDiscountCalcEdit;
		public ZArchitecture.ZCalcEdit AdditionalAdjustmentCommercialAmountCalcEdit;
		public ZArchitecture.ZCalcEdit AdditionalAdjustmentTransportationCostCalcEdit;
		public ZArchitecture.ZCalcEdit AdditionalAdjustmentShippingPortCostCalcEdit;
		public ZArchitecture.ZCalcEdit AdditionalAdjustmentInsuranceCalcEdit;
		public ZArchitecture.ZCalcEdit TotalAdditionalAdjustmentAmountCalcEdit;

		public ZArchitecture.ZCalcEdit DeductionAdjustmentQuantityDiscountCalcEdit;
		public ZArchitecture.ZCalcEdit DeductionAdjustmentCommercialAmountCalcEdit;
		public ZArchitecture.ZCalcEdit DeductionAdjustmentTransportationCostCalcEdit;
		public ZArchitecture.ZCalcEdit DeductionAdjustmentShippingPortCostCalcEdit;
		public ZArchitecture.ZCalcEdit DeductionAdjustmentInsuranceCalcEdit;
		public ZArchitecture.ZCalcEdit TotalDeductionAdjustmentAmountCalcEdit;
	}
}
