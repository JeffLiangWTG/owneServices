using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	partial class ImportEntryInstructionLayoutTemplate
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DutyDrawbackDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContentInspectionResultDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BeforePermitApplicationReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BondedLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BondedLocationNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SpecialDeclarationOfficeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsOfficeForSpecialDeclarationsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsOfficeDepartmentForSpecialDeclarationsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarationCargoTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SpecialDeclarationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DutyDrawbackDropEdit.SuspendLayout();
			this.ContentInspectionResultDropEdit.SuspendLayout();
			this.BeforePermitApplicationReasonDropEdit.SuspendLayout();
			this.BondedLocationCodeFindBox.SuspendLayout();
			this.SpecialDeclarationOfficeGroupBox.SuspendLayout();
			this.CustomsOfficeForSpecialDeclarationsDropEdit.SuspendLayout();
			this.CustomsOfficeDepartmentForSpecialDeclarationsDropEdit.SuspendLayout();
			this.DeclarationCargoTypeDropEdit.SuspendLayout();
			this.SpecialDeclarationTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.CusEntryInstruction);
			// 
			// DutyDrawbackDropEdit
			// 
			this.DutyDrawbackDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DutyDrawbackDropEdit, "CEI_DutyDrawback");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_DutyDrawback)));
			this.DutyDrawbackDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 81, true);
			this.DutyDrawbackDropEdit.Name = "DutyDrawbackDropEdit";
			this.DutyDrawbackDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.DutyDrawbackDropEdit.TabIndex = 3;
			// 
			// ContentInspectionResultDropEdit
			// 
			this.ContentInspectionResultDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContentInspectionResultDropEdit, "CEI_ContentInspectionResult");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_ContentInspectionResult)));
			this.ContentInspectionResultDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 81, true);
			this.ContentInspectionResultDropEdit.Name = "ContentInspectionResultDropEdit";
			this.ContentInspectionResultDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.ContentInspectionResultDropEdit.TabIndex = 8;
			// 
			// BeforePermitApplicationReasonDropEdit
			// 
			this.BeforePermitApplicationReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BeforePermitApplicationReasonDropEdit, "CEI_BeforePermitApplicationReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_BeforePermitApplicationReason)));
			this.BeforePermitApplicationReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 107, true);
			this.BeforePermitApplicationReasonDropEdit.Name = "BeforePermitApplicationReasonDropEdit";
			this.BeforePermitApplicationReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.BeforePermitApplicationReasonDropEdit.TabIndex = 9;
			// 
			// BondedLocationCodeFindBox
			// 
			this.BondedLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondedLocationCodeFindBox, "CEI_BondedLocationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_BondedLocationCode)));
			this.BondedLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 159, true);
			this.BondedLocationCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.BondedLocationCodeFindBox.Name = "BondedLocationCodeFindBox";
			this.BondedLocationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BondedLocationCodeFindBox.ParentType = null;
			this.BondedLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 16, true);
			this.BondedLocationCodeFindBox.TabIndex = 12;
			// 
			// BondedLocationNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.BondedLocationNameTextBox, "CEI_BondedLocationName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_BondedLocationName)));
			this.BondedLocationNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 133, true);
			this.BondedLocationNameTextBox.Name = "BondedLocationNameTextBox";
			this.BondedLocationNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.BondedLocationNameTextBox.TabIndex = 13;
			// 
			// SpecialDeclarationOfficeGroupBox
			// 
			this.SpecialDeclarationOfficeGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("398E76EB-5C62-46DA-9BC1-8FFB02CC1D88", "Special Declaration Office");
			this.SpecialDeclarationOfficeGroupBox.Controls.Add(this.CustomsOfficeForSpecialDeclarationsDropEdit);
			this.SpecialDeclarationOfficeGroupBox.Controls.Add(this.CustomsOfficeDepartmentForSpecialDeclarationsDropEdit);
			this.SpecialDeclarationOfficeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 216, true);
			this.SpecialDeclarationOfficeGroupBox.Name = "SpecialDeclarationOfficeGroupBox";
			this.SpecialDeclarationOfficeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 70, true);
			this.SpecialDeclarationOfficeGroupBox.TabIndex = 11;
			this.SpecialDeclarationOfficeGroupBox.TabStop = false;
			// 
			// CustomsOfficeForSpecialDeclarationsDropEdit
			// 
			this.CustomsOfficeForSpecialDeclarationsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeForSpecialDeclarationsDropEdit, "CEI_CustomsOfficeForSpecialDeclarations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_CustomsOfficeForSpecialDeclarations)));
			this.CustomsOfficeForSpecialDeclarationsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 20, true);
			this.CustomsOfficeForSpecialDeclarationsDropEdit.Name = "CustomsOfficeForSpecialDeclarationsDropEdit";
			this.CustomsOfficeForSpecialDeclarationsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 16, true);
			this.CustomsOfficeForSpecialDeclarationsDropEdit.TabIndex = 1;
			// 
			// CustomsOfficeDepartmentForSpecialDeclarationsDropEdit
			// 
			this.CustomsOfficeDepartmentForSpecialDeclarationsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeDepartmentForSpecialDeclarationsDropEdit, "CEI_CustomsOfficeDepartmentForSpecialDeclarations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_CustomsOfficeDepartmentForSpecialDeclarations)));
			this.CustomsOfficeDepartmentForSpecialDeclarationsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 42, true);
			this.CustomsOfficeDepartmentForSpecialDeclarationsDropEdit.Name = "CustomsOfficeDepartmentForSpecialDeclarationsDropEdit";
			this.CustomsOfficeDepartmentForSpecialDeclarationsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 16, true);
			this.CustomsOfficeDepartmentForSpecialDeclarationsDropEdit.TabIndex = 2;
			// 
			// DeclarationCargoTypeDropEdit
			// 
			this.DeclarationCargoTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationCargoTypeDropEdit, "CEI_DeclarationCargoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_DeclarationCargoType)));
			this.DeclarationCargoTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 159, true);
			this.DeclarationCargoTypeDropEdit.Name = "DeclarationCargoTypeDropEdit";
			this.DeclarationCargoTypeDropEdit.PreBoundMaxLength = 1;
			this.DeclarationCargoTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.DeclarationCargoTypeDropEdit.TabIndex = 4;
			// 
			// SpecialDeclarationTypeDropEdit
			// 
			this.SpecialDeclarationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecialDeclarationTypeDropEdit, "CEI_SubStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_SubStyle)));
			this.SpecialDeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 185, true);
			this.SpecialDeclarationTypeDropEdit.Name = "SpecialDeclarationTypeDropEdit";
			this.SpecialDeclarationTypeDropEdit.PreBoundMaxLength = 1;
			this.SpecialDeclarationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.SpecialDeclarationTypeDropEdit.TabIndex = 14;
			// 
			// ImportEntryInstructionLayoutTemplate
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SpecialDeclarationTypeDropEdit);
			this.Controls.Add(this.DutyDrawbackDropEdit);
			this.Controls.Add(this.ContentInspectionResultDropEdit);
			this.Controls.Add(this.BeforePermitApplicationReasonDropEdit);
			this.Controls.Add(this.SpecialDeclarationOfficeGroupBox);
			this.Controls.Add(this.BondedLocationCodeFindBox);
			this.Controls.Add(this.BondedLocationNameTextBox);
			this.Controls.Add(this.DeclarationCargoTypeDropEdit);
			this.Name = "ImportEntryInstructionLayoutTemplate";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 302, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DutyDrawbackDropEdit.ResumeLayout(true);
			this.DutyDrawbackDropEdit.PerformLayout();
			this.ContentInspectionResultDropEdit.ResumeLayout(true);
			this.ContentInspectionResultDropEdit.PerformLayout();
			this.BeforePermitApplicationReasonDropEdit.ResumeLayout(true);
			this.BeforePermitApplicationReasonDropEdit.PerformLayout();
			this.BondedLocationCodeFindBox.ResumeLayout(true);
			this.BondedLocationCodeFindBox.PerformLayout();
			this.SpecialDeclarationOfficeGroupBox.ResumeLayout(false);
			this.SpecialDeclarationOfficeGroupBox.PerformLayout();
			this.CustomsOfficeForSpecialDeclarationsDropEdit.ResumeLayout(true);
			this.CustomsOfficeForSpecialDeclarationsDropEdit.PerformLayout();
			this.CustomsOfficeDepartmentForSpecialDeclarationsDropEdit.ResumeLayout(true);
			this.CustomsOfficeDepartmentForSpecialDeclarationsDropEdit.PerformLayout();
			this.DeclarationCargoTypeDropEdit.ResumeLayout(true);
			this.DeclarationCargoTypeDropEdit.PerformLayout();
			this.SpecialDeclarationTypeDropEdit.ResumeLayout(true);
			this.SpecialDeclarationTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZDropEdit DutyDrawbackDropEdit;
		ZDropEdit ContentInspectionResultDropEdit;
		ZDropEdit BeforePermitApplicationReasonDropEdit;
		ZGroupBox SpecialDeclarationOfficeGroupBox;
		ZDropEdit CustomsOfficeForSpecialDeclarationsDropEdit;
		ZDropEdit CustomsOfficeDepartmentForSpecialDeclarationsDropEdit;
		ZCodeFindBox BondedLocationCodeFindBox;
		ZTextBox BondedLocationNameTextBox;
		ZDropEdit DeclarationCargoTypeDropEdit;
		ZDropEdit SpecialDeclarationTypeDropEdit;
		#endregion
	}
}
