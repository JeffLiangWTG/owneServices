using System.Collections;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Business;
using CusVehicle = Enterprise.Customs.KR.Business.CusVehicle;

namespace Enterprise.Customs.KR.GUI
{
	partial class VehicleUserControl
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
			this.NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExhaustVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.VehicleIDNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ModelYearTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SeatCapacityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FirstRegistrationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CurrentRegistrationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ManufacturingCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FirstRegistrationDateEdit.SuspendLayout();
			this.CurrentRegistrationDateEdit.SuspendLayout();
			this.ManufacturingCountryCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// NameTextBox
			// 
			this.BindingSource.SetBindingMember(this.NameTextBox, "ModelName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).ModelName)));
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 19, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 18, true);
			this.NameTextBox.TabIndex = 0;
			// 
			// ExhaustVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExhaustVolumeCalcEdit, "EngineCapacity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).EngineCapacity)));
			this.ExhaustVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 45, true);
			this.ExhaustVolumeCalcEdit.Name = "ExhaustVolumeCalcEdit";
			this.ExhaustVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 18, true);
			this.ExhaustVolumeCalcEdit.TabIndex = 1;
			// 
			// VehicleIDNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.VehicleIDNumberTextBox, "VehicleIdentificationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).VehicleIdentificationNumber)));
			this.VehicleIDNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 19, true);
			this.VehicleIDNumberTextBox.Name = "VehicleIDNumberTextBox";
			this.VehicleIDNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 18, true);
			this.VehicleIDNumberTextBox.TabIndex = 2;
			// 
			// ModelYearTextBox
			// 
			this.BindingSource.SetBindingMember(this.ModelYearTextBox, "ModelYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).ModelYear)));
			this.ModelYearTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 45, true);
			this.ModelYearTextBox.Name = "ModelYearTextBox";
			this.ModelYearTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 18, true);
			this.ModelYearTextBox.TabIndex = 3;
			// 
			// SeatCapacityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SeatCapacityCalcEdit, "SeatingCapacity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZByte)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).SeatingCapacity)));
			this.SeatCapacityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 71, true);
			this.SeatCapacityCalcEdit.Name = "SeatCapacityCalcEdit";
			this.SeatCapacityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 18, true);
			this.SeatCapacityCalcEdit.TabIndex = 4;
			// 
			// FirstRegistrationDateEdit
			// 
			this.FirstRegistrationDateEdit.AllowDrop = true;
			this.FirstRegistrationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.FirstRegistrationDateEdit, "DateOfFirstRegistration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).DateOfFirstRegistration)));
			this.FirstRegistrationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 97, true);
			this.FirstRegistrationDateEdit.Name = "FirstRegistrationDateEdit";
			this.FirstRegistrationDateEdit.TabIndex = 5;
			// 
			// CurrentRegistrationDateEdit
			// 
			this.CurrentRegistrationDateEdit.AllowDrop = true;
			this.CurrentRegistrationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.CurrentRegistrationDateEdit, "DateOfCurrentRegistration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).DateOfCurrentRegistration)));
			this.CurrentRegistrationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(414, 97, true);
			this.CurrentRegistrationDateEdit.Name = "CurrentRegistrationDateEdit";
			this.CurrentRegistrationDateEdit.TabIndex = 6;
			// 
			// ManufacturingCountryCodeFindBox
			// 
			this.ManufacturingCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturingCountryCodeFindBox, "CountryOfManufacture");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CountryOfManufacture)));
			this.ManufacturingCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 71, true);
			this.ManufacturingCountryCodeFindBox.Name = "ManufacturingCountryCodeFindBox";
			this.ManufacturingCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ManufacturingCountryCodeFindBox.ParentType = null;
			this.ManufacturingCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 18, true);
			this.ManufacturingCountryCodeFindBox.TabIndex = 7;
			// 
			// VehicleUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ManufacturingCountryCodeFindBox);
			this.Controls.Add(this.CurrentRegistrationDateEdit);
			this.Controls.Add(this.FirstRegistrationDateEdit);
			this.Controls.Add(this.SeatCapacityCalcEdit);
			this.Controls.Add(this.ModelYearTextBox);
			this.Controls.Add(this.VehicleIDNumberTextBox);
			this.Controls.Add(this.ExhaustVolumeCalcEdit);
			this.Controls.Add(this.NameTextBox);
			this.Name = "VehicleUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 154, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FirstRegistrationDateEdit.ResumeLayout(true);
			this.FirstRegistrationDateEdit.PerformLayout();
			this.CurrentRegistrationDateEdit.ResumeLayout(true);
			this.CurrentRegistrationDateEdit.PerformLayout();
			this.ManufacturingCountryCodeFindBox.ResumeLayout(true);
			this.ManufacturingCountryCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox NameTextBox;
		internal ZArchitecture.ZCalcEdit ExhaustVolumeCalcEdit;
		internal ZArchitecture.ZTextBox VehicleIDNumberTextBox;
		internal ZArchitecture.ZTextBox ModelYearTextBox;
		internal ZArchitecture.ZCalcEdit SeatCapacityCalcEdit;
		internal ZArchitecture.GUI.ZDateEdit FirstRegistrationDateEdit;
		internal ZArchitecture.GUI.ZDateEdit CurrentRegistrationDateEdit;
		internal ZArchitecture.GUI.ZCodeFindBox ManufacturingCountryCodeFindBox;
	}
}
