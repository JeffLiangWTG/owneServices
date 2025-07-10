using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	partial class ECREntryInstructionLayoutTemplate
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

		private void InitializeComponent()
		{
			this.CusEntryInstructionNSITextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ECRNotesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ECRCargoTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SpecialCargoCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsVolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ECRCargoTypeDropEdit.SuspendLayout();
			this.SpecialCargoCodeFindBox.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.CustomsVolumeCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.CusEntryInstruction);
			// 
			// CusEntryInstructionNSITextBox
			// 
			this.BindingSource.SetBindingMember(this.CusEntryInstructionNSITextBox, "NSI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).NSI)));
			this.CusEntryInstructionNSITextBox.Name = "CusEntryInstructionNSITextBox";
			this.CusEntryInstructionNSITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			// 
			// ECRNotesTextBox
			// 
			this.BindingSource.SetBindingMember(this.ECRNotesTextBox, "JP_ECRNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JP_ECRNotes)));
			this.ECRNotesTextBox.Multiline = true;
			this.ECRNotesTextBox.Name = "ECRNotesTextBox";
			this.ECRNotesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 68, true);
			// 
			// ECRCargoTypeDropEdit
			// 
			this.ECRCargoTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ECRCargoTypeDropEdit, "CEI_ECRCargoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_ECRCargoType)));
			this.ECRCargoTypeDropEdit.Name = "ECRCargoTypeDropEdit";
			this.ECRCargoTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			// 
			// SpecialCargoCodeFindBox
			// 
			this.SpecialCargoCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecialCargoCodeFindBox, "CEI_SpecialCargoCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_SpecialCargoCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.SpecialCargoCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.SpecialCargoCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.SpecialCargoCodeFindBox.Name = "SpecialCargoCodeFindBox";
			this.SpecialCargoCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SpecialCargoCodeFindBox.ParentType = null;
			this.SpecialCargoCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_VolumeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).Lookups.VolumeUnitList)));
			this.VolumeCalcDropEdit.BindToAmount = "CEI_Volume";
			this.VolumeCalcDropEdit.BindToList = "Lookups.VolumeUnitList";
			this.VolumeCalcDropEdit.BindToUnit = "CEI_VolumeUnit";
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 31, true);
			this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 5;
			this.VolumeCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CustomsVolumeCalcDropEdit
			// 
			this.CustomsVolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_CustomsVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_CustomsVolumeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).Lookups.VolumeUnitList)));
			this.CustomsVolumeCalcDropEdit.BindToAmount = "CEI_CustomsVolume";
			this.CustomsVolumeCalcDropEdit.BindToList = "Lookups.CustomsVolumeUnitList";
			this.CustomsVolumeCalcDropEdit.BindToUnit = "CEI_CustomsVolumeUnit";
			this.CustomsVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 57, true);
			this.CustomsVolumeCalcDropEdit.Name = "CustomsVolumeCalcDropEdit";
			this.CustomsVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.CustomsVolumeCalcDropEdit.TabIndex = 6;
			this.CustomsVolumeCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// ECREntryInstructionLayoutTemplate
			//
			this.Controls.Add(this.ECRCargoTypeDropEdit);
			this.Controls.Add(this.ECRNotesTextBox);
			this.Controls.Add(this.SpecialCargoCodeFindBox);
			this.Controls.Add(this.CustomsVolumeCalcDropEdit);
			this.Controls.Add(this.VolumeCalcDropEdit);
			this.Controls.Add(this.CusEntryInstructionNSITextBox);
			this.Name = "ECREntryInstructionLayoutTemplate";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 140, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CusEntryInstructionNSITextBox.ResumeLayout(true);
			this.CusEntryInstructionNSITextBox.PerformLayout();
			this.ECRCargoTypeDropEdit.ResumeLayout(true);
			this.ECRCargoTypeDropEdit.PerformLayout();
			this.SpecialCargoCodeFindBox.ResumeLayout(true);
			this.SpecialCargoCodeFindBox.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.CustomsVolumeCalcDropEdit.ResumeLayout(true);
			this.CustomsVolumeCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZTextBox ECRNotesTextBox;
		ZTextBox CusEntryInstructionNSITextBox;
		ZDropEdit ECRCargoTypeDropEdit;
		ZCalcDropEdit VolumeCalcDropEdit;
		ZCalcDropEdit CustomsVolumeCalcDropEdit;
		ZCodeFindBox SpecialCargoCodeFindBox;
	}
}
