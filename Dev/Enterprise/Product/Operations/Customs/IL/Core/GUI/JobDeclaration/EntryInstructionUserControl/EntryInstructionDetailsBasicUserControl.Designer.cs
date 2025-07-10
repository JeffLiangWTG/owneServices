using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	partial class EntryInstructionDetailsBasicUserControl
	{
		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FromWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DateForDutyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FormattedProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AutonomyRegionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ToWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.PackagesQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FromWarehouseAddressControl.SuspendLayout();
			this.DateForDutyDateEdit.SuspendLayout();
			this.FormattedProcedureDropEdit.SuspendLayout();
			this.AutonomyRegionTypeDropEdit.SuspendLayout();
			this.ToWarehouseAddressControl.SuspendLayout();
			this.PackagesQtyCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Business.CusEntryInstruction);
			// 
			// FromWarehouseAddressControl
			// 
			this.FromWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWarehouseAddressControl, "CEI_OA_Warehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(null)).CEI_OA_Warehouse)));
			this.FromWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.FromWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 108, true);
			this.FromWarehouseAddressControl.Name = "FromWarehouseAddressControl";
			this.FromWarehouseAddressControl.PopupCaption = "";
			this.FromWarehouseAddressControl.ShowAddress = false;
			this.FromWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.FromWarehouseAddressControl.TabIndex = 0;
			// 
			// DateForDutyDateEdit
			// 
			this.DateForDutyDateEdit.AllowDrop = true;
			this.DateForDutyDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DateForDutyDateEdit, "CEI_DateForDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(null)).CEI_DateForDuty)));
			this.DateForDutyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 21, true);
			this.DateForDutyDateEdit.Name = "DateForDutyDateEdit";
			this.DateForDutyDateEdit.TabIndex = 4;
			// 
			// FormattedProcedureDropEdit
			// 
			this.FormattedProcedureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FormattedProcedureDropEdit, "CEI_FormattedProcedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(null)).CEI_FormattedProcedure)));
			this.FormattedProcedureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 73, true);
			this.FormattedProcedureDropEdit.Name = "FormattedProcedureDropEdit";
			this.FormattedProcedureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.FormattedProcedureDropEdit.TabIndex = 5;
			// 
			// AutonomyRegionTypeDropEdit
			// 
			this.AutonomyRegionTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AutonomyRegionTypeDropEdit, "CEI_AutonomyRegionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(null)).CEI_AutonomyRegionType)));
			this.AutonomyRegionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 188, true);
			this.AutonomyRegionTypeDropEdit.Name = "AutonomyRegionTypeDropEdit";
			this.AutonomyRegionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.AutonomyRegionTypeDropEdit.TabIndex = 8;
			// 
			// ToWarehouseAddressControl
			// 
			this.ToWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWarehouseAddressControl, "CEI_OA_Warehouse2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(null)).CEI_OA_Warehouse2)));
			this.ToWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.ToWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 146, true);
			this.ToWarehouseAddressControl.Name = "ToWarehouseAddressControl";
			this.ToWarehouseAddressControl.PopupCaption = "";
			this.ToWarehouseAddressControl.ShowAddress = false;
			this.ToWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ToWarehouseAddressControl.TabIndex = 9;
			// 
			// PackagesQtyCalcDropEdit
			// 
			this.PackagesQtyCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackagesQtyCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(null)).CEI_NumberOfPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.CusEntryInstruction)(null)).CEI_CustomsPackType)));
			this.PackagesQtyCalcDropEdit.BindToAmount = "CEI_NumberOfPackages";
			this.PackagesQtyCalcDropEdit.BindToUnit = "CEI_CustomsPackType";
			this.PackagesQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 225, true);
            this.PackagesQtyCalcDropEdit.Name = "PackagesQtyCalcDropEdit";
            this.PackagesQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
            this.PackagesQtyCalcDropEdit.TabIndex = 10;
			// 
			// EntryInstructionDetailsBasicUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PackagesQtyCalcDropEdit);
			this.Controls.Add(this.ToWarehouseAddressControl);
			this.Controls.Add(this.FromWarehouseAddressControl);
			this.Controls.Add(this.AutonomyRegionTypeDropEdit);
			this.Controls.Add(this.FormattedProcedureDropEdit);
			this.Controls.Add(this.DateForDutyDateEdit);
			this.Name = "EntryInstructionDetailsBasicUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 378, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FromWarehouseAddressControl.ResumeLayout(true);
			this.FromWarehouseAddressControl.PerformLayout();
			this.DateForDutyDateEdit.ResumeLayout(true);
			this.DateForDutyDateEdit.PerformLayout();
			this.FormattedProcedureDropEdit.ResumeLayout(true);
			this.FormattedProcedureDropEdit.PerformLayout();
			this.AutonomyRegionTypeDropEdit.ResumeLayout(true);
			this.AutonomyRegionTypeDropEdit.PerformLayout();
			this.ToWarehouseAddressControl.ResumeLayout(true);
			this.ToWarehouseAddressControl.PerformLayout();
			this.PackagesQtyCalcDropEdit.ResumeLayout(true);
			this.PackagesQtyCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZAddressControl FromWarehouseAddressControl;
		internal ZDateEdit DateForDutyDateEdit;
		internal ZDropEdit FormattedProcedureDropEdit;
		internal ZDropEdit AutonomyRegionTypeDropEdit;
		internal ZAddressControl ToWarehouseAddressControl;
		internal ZCalcDropEdit PackagesQtyCalcDropEdit;
	}
}
