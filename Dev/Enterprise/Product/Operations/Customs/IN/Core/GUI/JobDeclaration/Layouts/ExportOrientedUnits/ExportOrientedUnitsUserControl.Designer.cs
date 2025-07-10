namespace Enterprise.Customs.IN.GUI;

partial class ExportOrientedUnitsUserControl
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
            this.ExportOrientedUnitsDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
            this.CommissionerateTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.DivisionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RangeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SealNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.VerifiedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.SampleForwardedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ExaminationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ExaminingOfficerNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ExaminingOfficerDesignationTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SupervisingOfficerNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SupervisingOfficerDesignationTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ExportOrientedUnitsDocAddressControl.SuspendLayout();
            this.VerifiedDropEdit.SuspendLayout();
            this.SampleForwardedDropEdit.SuspendLayout();
            this.ExaminationDateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.JobDeclaration);
            // 
            // ExportOrientedUnitsDocAddressControl
            // 
            this.ExportOrientedUnitsDocAddressControl.AddressValidationProcessCmdKey = null;
            this.ExportOrientedUnitsDocAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ExportOrientedUnitsDocAddressControl, "ExportOrientedUnitsDocAddress");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).ExportOrientedUnitsDocAddress)));
            this.ExportOrientedUnitsDocAddressControl.BindToOrganisations = "Lookups+ExportOrientedUnitsCollection";
            this.ExportOrientedUnitsDocAddressControl.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("e5d6a723-972d-4b96-bece-039e4185d9f3", "Org.", "Organization", "EOU Organization", "");
            this.ExportOrientedUnitsDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
            this.ExportOrientedUnitsDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 27, true);
            this.ExportOrientedUnitsDocAddressControl.Name = "ExportOrientedUnitsDocAddressControl";
            this.ExportOrientedUnitsDocAddressControl.ReadOnly = false;
            this.ExportOrientedUnitsDocAddressControl.SingleLineNoGroupBoxPanelWidth = 320;
            this.ExportOrientedUnitsDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
            this.ExportOrientedUnitsDocAddressControl.TabIndex = 6;
            this.ExportOrientedUnitsDocAddressControl.ValidationJustForced = false;
            // 
            // CommissionerateTextBox
            // 
            this.BindingSource.SetBindingMember(this.CommissionerateTextBox, "JE_Commissionerate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_Commissionerate)));
            this.CommissionerateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CommissionerateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 238, true);
            this.CommissionerateTextBox.Name = "CommissionerateTextBox";
            this.CommissionerateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 31, true);
            this.CommissionerateTextBox.TabIndex = 12;
            // 
            // DivisionTextBox
            // 
            this.BindingSource.SetBindingMember(this.DivisionTextBox, "JE_Division");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_Division)));
            this.DivisionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.DivisionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 275, true);
            this.DivisionTextBox.Name = "DivisionTextBox";
            this.DivisionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 31, true);
            this.DivisionTextBox.TabIndex = 13;
            // 
            // RangeTextBox
            // 
            this.BindingSource.SetBindingMember(this.RangeTextBox, "JE_Range");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_Range)));
            this.RangeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.RangeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 312, true);
            this.RangeTextBox.Name = "RangeTextBox";
            this.RangeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 31, true);
            this.RangeTextBox.TabIndex = 14;
            // 
            // SealNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.SealNoTextBox, "JE_SealNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_SealNo)));
            this.SealNoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.SealNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 349, true);
            this.SealNoTextBox.Name = "SealNoTextBox";
            this.SealNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 31, true);
            this.SealNoTextBox.TabIndex = 15;
            // 
            // VerifiedDropEdit
            // 
            this.VerifiedDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VerifiedDropEdit, "JE_Verified");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_Verified)));
            this.VerifiedDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.VerifiedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 386, true);
            this.VerifiedDropEdit.Name = "VerifiedDropEdit";
            this.VerifiedDropEdit.PreBoundMaxLength = 2;
            this.VerifiedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 31, true);
            this.VerifiedDropEdit.TabIndex = 16;
            // 
            // SampleForwardedDropEdit
            // 
            this.SampleForwardedDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SampleForwardedDropEdit, "JE_SampleForwarded");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_SampleForwarded)));
            this.SampleForwardedDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.SampleForwardedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 423, true);
            this.SampleForwardedDropEdit.Name = "SampleForwardedDropEdit";
            this.SampleForwardedDropEdit.PreBoundMaxLength = 2;
            this.SampleForwardedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 31, true);
            this.SampleForwardedDropEdit.TabIndex = 17;
            // 
            // ExaminationDateEdit
            // 
            this.ExaminationDateEdit.AllowDrop = true;
            this.ExaminationDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ExaminationDateEdit, "JE_ExaminationDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_ExaminationDate)));
            this.ExaminationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 53, true);
            this.ExaminationDateEdit.Name = "ExaminationDateEdit";
            this.ExaminationDateEdit.TabIndex = 7;
            // 
            // ExaminingOfficerNameTextBox
            // 
            this.BindingSource.SetBindingMember(this.ExaminingOfficerNameTextBox, "JE_ExaminingOfficerName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_ExaminingOfficerName)));
            this.ExaminingOfficerNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ExaminingOfficerNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 90, true);
            this.ExaminingOfficerNameTextBox.Name = "ExaminingOfficerNameTextBox";
            this.ExaminingOfficerNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 31, true);
            this.ExaminingOfficerNameTextBox.TabIndex = 8;
            // 
            // ExaminingOfficerDesignationTextBox
            // 
            this.BindingSource.SetBindingMember(this.ExaminingOfficerDesignationTextBox, "JE_ExaminingOfficerDesignation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_ExaminingOfficerDesignation)));
            this.ExaminingOfficerDesignationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ExaminingOfficerDesignationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 127, true);
            this.ExaminingOfficerDesignationTextBox.Name = "ExaminingOfficerDesignationTextBox";
            this.ExaminingOfficerDesignationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 31, true);
            this.ExaminingOfficerDesignationTextBox.TabIndex = 9;
            // 
            // SupervisingOfficerNameTextBox
            // 
            this.BindingSource.SetBindingMember(this.SupervisingOfficerNameTextBox, "JE_SupervisingOfficerName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_SupervisingOfficerName)));
            this.SupervisingOfficerNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.SupervisingOfficerNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 164, true);
            this.SupervisingOfficerNameTextBox.Name = "SupervisingOfficerNameTextBox";
            this.SupervisingOfficerNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 31, true);
            this.SupervisingOfficerNameTextBox.TabIndex = 10;
            // 
            // SupervisingOfficerDesignationTextBox
            // 
            this.BindingSource.SetBindingMember(this.SupervisingOfficerDesignationTextBox, "JE_SupervisingOfficerDesignation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_SupervisingOfficerDesignation)));
            this.SupervisingOfficerDesignationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.SupervisingOfficerDesignationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 201, true);
            this.SupervisingOfficerDesignationTextBox.Name = "SupervisingOfficerDesignationTextBox";
            this.SupervisingOfficerDesignationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 31, true);
            this.SupervisingOfficerDesignationTextBox.TabIndex = 11;
            // 
            // ExportOrientedUnitsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ExportOrientedUnitsDocAddressControl);
            this.Controls.Add(this.ExaminationDateEdit);
            this.Controls.Add(this.ExaminingOfficerNameTextBox);
            this.Controls.Add(this.ExaminingOfficerDesignationTextBox);
            this.Controls.Add(this.SupervisingOfficerNameTextBox);
            this.Controls.Add(this.SupervisingOfficerDesignationTextBox);
            this.Controls.Add(this.CommissionerateTextBox);
            this.Controls.Add(this.DivisionTextBox);
            this.Controls.Add(this.RangeTextBox);
            this.Controls.Add(this.SealNoTextBox);
            this.Controls.Add(this.VerifiedDropEdit);
            this.Controls.Add(this.SampleForwardedDropEdit);
            this.Name = "ExportOrientedUnitsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(595, 277, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ExportOrientedUnitsDocAddressControl.ResumeLayout(true);
            this.ExportOrientedUnitsDocAddressControl.PerformLayout();
            this.VerifiedDropEdit.ResumeLayout(true);
            this.VerifiedDropEdit.PerformLayout();
            this.SampleForwardedDropEdit.ResumeLayout(true);
            this.SampleForwardedDropEdit.PerformLayout();
            this.ExaminationDateEdit.ResumeLayout(true);
            this.ExaminationDateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

	}

	#endregion

	public MasterFiles.GUI.ZDocAddressControl ExportOrientedUnitsDocAddressControl;
	public ZArchitecture.ZTextBox CommissionerateTextBox;
	public ZArchitecture.ZTextBox DivisionTextBox;
	public ZArchitecture.ZTextBox RangeTextBox;
	public ZArchitecture.ZTextBox SealNoTextBox;
	public ZArchitecture.GUI.ZDropEdit VerifiedDropEdit;
	public ZArchitecture.GUI.ZDropEdit SampleForwardedDropEdit;
	public ZArchitecture.GUI.ZDateEdit ExaminationDateEdit;
	public ZArchitecture.ZTextBox ExaminingOfficerNameTextBox;
	public ZArchitecture.ZTextBox ExaminingOfficerDesignationTextBox;
	public ZArchitecture.ZTextBox SupervisingOfficerNameTextBox;
	public ZArchitecture.ZTextBox SupervisingOfficerDesignationTextBox;
}
