
namespace Enterprise.Customs.KR.GUI
{
	partial class StatementLineUserControl
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
			this.DutyAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LiquorTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AgricultureTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TransportationTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EducationTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InterestCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SpecialConsumptionTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.VATCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeclarationPenaltyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusStatementHeader);
			// 
			// DutyAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DutyAmountCalcEdit, "FirstLine+DutyAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).FirstLine.DutyAmount)));
			this.DutyAmountCalcEdit.DecimalPlaces = 2;
			this.DutyAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 21, true);
			this.DutyAmountCalcEdit.Name = "DutyAmountCalcEdit";
			this.DutyAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
			this.DutyAmountCalcEdit.TabIndex = 24;
			this.DutyAmountCalcEdit.TabStop = false;
			this.DutyAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DutyAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// LiquorTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LiquorTaxCalcEdit, "FirstLine+LiquorTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).FirstLine.LiquorTax)));
			this.LiquorTaxCalcEdit.DecimalPlaces = 2;
			this.LiquorTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 47, true);
			this.LiquorTaxCalcEdit.Name = "LiquorTaxCalcEdit";
			this.LiquorTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
			this.LiquorTaxCalcEdit.TabIndex = 25;
			this.LiquorTaxCalcEdit.TabStop = false;
			this.LiquorTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.LiquorTaxCalcEdit.TrackDisposedAccess = true;
			// 
			// AgricultureTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AgricultureTaxCalcEdit, "FirstLine+AgricultureTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).FirstLine.AgricultureTax)));
			this.AgricultureTaxCalcEdit.DecimalPlaces = 2;
			this.AgricultureTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 73, true);
			this.AgricultureTaxCalcEdit.Name = "AgricultureTaxCalcEdit";
			this.AgricultureTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
			this.AgricultureTaxCalcEdit.TabIndex = 26;
			this.AgricultureTaxCalcEdit.TabStop = false;
			this.AgricultureTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.AgricultureTaxCalcEdit.TrackDisposedAccess = true;
			// 
			// TransportationTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TransportationTaxCalcEdit, "FirstLine+TransportationTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).FirstLine.TransportationTax)));
			this.TransportationTaxCalcEdit.DecimalPlaces = 2;
			this.TransportationTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 99, true);
			this.TransportationTaxCalcEdit.Name = "TransportationTaxCalcEdit";
			this.TransportationTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
			this.TransportationTaxCalcEdit.TabIndex = 27;
			this.TransportationTaxCalcEdit.TabStop = false;
			this.TransportationTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TransportationTaxCalcEdit.TrackDisposedAccess = true;
			// 
			// EducationTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.EducationTaxCalcEdit, "FirstLine+EducationTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).FirstLine.EducationTax)));
			this.EducationTaxCalcEdit.DecimalPlaces = 2;
			this.EducationTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 125, true);
			this.EducationTaxCalcEdit.Name = "EducationTaxCalcEdit";
			this.EducationTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
			this.EducationTaxCalcEdit.TabIndex = 28;
			this.EducationTaxCalcEdit.TabStop = false;
			this.EducationTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.EducationTaxCalcEdit.TrackDisposedAccess = true;
			// 
			// InterestCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InterestCalcEdit, "FirstLine+PenaltyAndInterest");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).FirstLine.PenaltyAndInterest)));
			this.InterestCalcEdit.DecimalPlaces = 2;
			this.InterestCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 151, true);
			this.InterestCalcEdit.Name = "InterestCalcEdit";
			this.InterestCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
			this.InterestCalcEdit.TabIndex = 29;
			this.InterestCalcEdit.TabStop = false;
			this.InterestCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.InterestCalcEdit.TrackDisposedAccess = true;
			// 
			// SpecialConsumptionTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SpecialConsumptionTaxCalcEdit, "FirstLine+SpecialConsumptionTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).FirstLine.SpecialConsumptionTax)));
			this.SpecialConsumptionTaxCalcEdit.DecimalPlaces = 2;
			this.SpecialConsumptionTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 177, true);
			this.SpecialConsumptionTaxCalcEdit.Name = "SpecialConsumptionTaxCalcEdit";
			this.SpecialConsumptionTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
			this.SpecialConsumptionTaxCalcEdit.TabIndex = 30;
			this.SpecialConsumptionTaxCalcEdit.TabStop = false;
			this.SpecialConsumptionTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.SpecialConsumptionTaxCalcEdit.TrackDisposedAccess = true;
			// 
			// VATCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.VATCalcEdit, "FirstLine+VAT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).FirstLine.VAT)));
			this.VATCalcEdit.DecimalPlaces = 2;
			this.VATCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 203, true);
			this.VATCalcEdit.Name = "VATCalcEdit";
			this.VATCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
			this.VATCalcEdit.TabIndex = 31;
			this.VATCalcEdit.TabStop = false;
			this.VATCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.VATCalcEdit.TrackDisposedAccess = true;
			// 
			// DeclarationPenaltyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeclarationPenaltyCalcEdit, "FirstLine+PenaltyForLatePayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusStatementHeader)(null)).FirstLine.PenaltyForLatePayment)));
			this.DeclarationPenaltyCalcEdit.DecimalPlaces = 2;
			this.DeclarationPenaltyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 229, true);
			this.DeclarationPenaltyCalcEdit.Name = "DeclarationPenaltyCalcEdit";
			this.DeclarationPenaltyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
			this.DeclarationPenaltyCalcEdit.TabIndex = 32;
			this.DeclarationPenaltyCalcEdit.TabStop = false;
			this.DeclarationPenaltyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DeclarationPenaltyCalcEdit.TrackDisposedAccess = true;
			// 
			// StatementLineUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.DeclarationPenaltyCalcEdit);
			this.Controls.Add(this.VATCalcEdit);
			this.Controls.Add(this.SpecialConsumptionTaxCalcEdit);
			this.Controls.Add(this.InterestCalcEdit);
			this.Controls.Add(this.EducationTaxCalcEdit);
			this.Controls.Add(this.TransportationTaxCalcEdit);
			this.Controls.Add(this.AgricultureTaxCalcEdit);
			this.Controls.Add(this.LiquorTaxCalcEdit);
			this.Controls.Add(this.DutyAmountCalcEdit);
			this.Name = "StatementLineUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 303, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.ZCalcEdit DutyAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit LiquorTaxCalcEdit;
		internal ZArchitecture.ZCalcEdit AgricultureTaxCalcEdit;
		internal ZArchitecture.ZCalcEdit TransportationTaxCalcEdit;
		internal ZArchitecture.ZCalcEdit EducationTaxCalcEdit;
		internal ZArchitecture.ZCalcEdit InterestCalcEdit;
		internal ZArchitecture.ZCalcEdit SpecialConsumptionTaxCalcEdit;
		internal ZArchitecture.ZCalcEdit VATCalcEdit;
		internal ZArchitecture.ZCalcEdit DeclarationPenaltyCalcEdit;
	}
}
