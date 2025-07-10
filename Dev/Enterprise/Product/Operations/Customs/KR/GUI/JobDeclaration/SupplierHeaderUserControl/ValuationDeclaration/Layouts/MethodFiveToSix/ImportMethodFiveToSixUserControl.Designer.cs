using CargoWise.Windows.UI;

namespace Enterprise.Customs.KR.GUI
{
	partial class ImportMethodFiveToSixUserControl
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
            this.AmountAgreedUponWithCustomsKRWCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.AdditionalCostFreightToArrivalPortCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.AdditionalCostFreightToDeparturePortCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.AdditionalCostInsuranceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.AdditionalCostTotalAdditionalAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceHeader);
			// 
			// AmountAgreedUponWithCustomsKRWCalcEdit
			//
			this.BindingSource.SetBindingMember(this.AmountAgreedUponWithCustomsKRWCalcEdit, "AmountAgreedUponWithCustomsKRW");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).AmountAgreedUponWithCustomsKRW)));
			this.AmountAgreedUponWithCustomsKRWCalcEdit.DecimalPlaces = 2;
            this.AmountAgreedUponWithCustomsKRWCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 31, true);
            this.AmountAgreedUponWithCustomsKRWCalcEdit.Name = "AmountAgreedUponWithCustomsKRWCalcEdit";
            this.AmountAgreedUponWithCustomsKRWCalcEdit.ReadOnly = true;
            this.AmountAgreedUponWithCustomsKRWCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 18, true);
            this.AmountAgreedUponWithCustomsKRWCalcEdit.TabIndex = 0;
            this.AmountAgreedUponWithCustomsKRWCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AmountAgreedUponWithCustomsKRWCalcEdit.TrackDisposedAccess = true;
			// 
			// FreightToArrivalPortCalcEdit
			//
			this.BindingSource.SetBindingMember(this.AdditionalCostFreightToArrivalPortCalcEdit, "AdditionalCostFreightToArrivalPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).AdditionalCostFreightToArrivalPort)));
			this.AdditionalCostFreightToArrivalPortCalcEdit.DecimalPlaces = 2;
            this.AdditionalCostFreightToArrivalPortCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 53, true);
            this.AdditionalCostFreightToArrivalPortCalcEdit.Name = "AdditionalCostFreightToArrivalPortCalcEdit";
			this.AdditionalCostFreightToArrivalPortCalcEdit.ReadOnly = true;
			this.AdditionalCostFreightToArrivalPortCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 18, true);
            this.AdditionalCostFreightToArrivalPortCalcEdit.TabIndex = 1;
            this.AdditionalCostFreightToArrivalPortCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AdditionalCostFreightToArrivalPortCalcEdit.TrackDisposedAccess = true;
			// 
			// FreightToDeparturePortCalcEdit
			//
			this.BindingSource.SetBindingMember(this.AdditionalCostFreightToDeparturePortCalcEdit, "AdditionalCostFreightToDeparturePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).AdditionalCostFreightToDeparturePort)));
			this.AdditionalCostFreightToDeparturePortCalcEdit.DecimalPlaces = 2;
            this.AdditionalCostFreightToDeparturePortCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 74, true);
            this.AdditionalCostFreightToDeparturePortCalcEdit.Name = "AdditionalCostFreightToDeparturePortCalcEdit";
			this.AdditionalCostFreightToDeparturePortCalcEdit.ReadOnly = true;
            this.AdditionalCostFreightToDeparturePortCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 18, true);
            this.AdditionalCostFreightToDeparturePortCalcEdit.TabIndex = 2;
            this.AdditionalCostFreightToDeparturePortCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AdditionalCostFreightToDeparturePortCalcEdit.TrackDisposedAccess = true;
			// 
			// InsuranceCalcEdit
			//
			this.BindingSource.SetBindingMember(this.AdditionalCostInsuranceCalcEdit, "AdditionalCostInsurance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).AdditionalCostInsurance)));
			this.AdditionalCostInsuranceCalcEdit.DecimalPlaces = 2;
            this.AdditionalCostInsuranceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 96, true);
            this.AdditionalCostInsuranceCalcEdit.Name = "AdditionalCostInsuranceCalcEdit";
			this.AdditionalCostInsuranceCalcEdit.ReadOnly = true;
            this.AdditionalCostInsuranceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 18, true);
            this.AdditionalCostInsuranceCalcEdit.TabIndex = 3;
            this.AdditionalCostInsuranceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AdditionalCostInsuranceCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalAdditionalAmountCalcEdit
			//
			this.BindingSource.SetBindingMember(this.AdditionalCostTotalAdditionalAmountCalcEdit, "AdditionalCostTotalAdditionalAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).AdditionalCostTotalAdditionalAmount)));
			this.AdditionalCostTotalAdditionalAmountCalcEdit.DecimalPlaces = 2;
            this.AdditionalCostTotalAdditionalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 118, true);
            this.AdditionalCostTotalAdditionalAmountCalcEdit.Name = "AdditionalCostTotalAdditionalAmountCalcEdit";
			this.AdditionalCostTotalAdditionalAmountCalcEdit.ReadOnly = true;
            this.AdditionalCostTotalAdditionalAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 18, true);
            this.AdditionalCostTotalAdditionalAmountCalcEdit.TabIndex = 4;
            this.AdditionalCostTotalAdditionalAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.AdditionalCostTotalAdditionalAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // ImportMethodFiveToSixUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.AdditionalCostTotalAdditionalAmountCalcEdit);
            this.Controls.Add(this.AdditionalCostInsuranceCalcEdit);
            this.Controls.Add(this.AdditionalCostFreightToDeparturePortCalcEdit);
            this.Controls.Add(this.AdditionalCostFreightToArrivalPortCalcEdit);
            this.Controls.Add(this.AmountAgreedUponWithCustomsKRWCalcEdit);
            this.Name = "ImportMethodFiveToSixUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 256, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZCalcEdit AmountAgreedUponWithCustomsKRWCalcEdit;
		internal ZArchitecture.ZCalcEdit AdditionalCostFreightToArrivalPortCalcEdit;
		internal ZArchitecture.ZCalcEdit AdditionalCostFreightToDeparturePortCalcEdit;
		internal ZArchitecture.ZCalcEdit AdditionalCostInsuranceCalcEdit;
		internal ZArchitecture.ZCalcEdit AdditionalCostTotalAdditionalAmountCalcEdit;
	}
}
