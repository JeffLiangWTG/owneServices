using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class DetailsUserControl
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
      this.DutyReductionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.SpecificUseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
      this.DutyReductionCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
      this.InstalmentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
      this.RemarkLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
      this.RemarkTextBox = new Enterprise.ZArchitecture.ZTextBox();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.DutyReductionTypeDropEdit.SuspendLayout();
      this.DutyReductionCodeFindBox.SuspendLayout();
      this.InstalmentCodeFindBox.SuspendLayout();
      this.RemarkLongTextControl.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceLine);
      // 
      // DutyReductionTypeDropEdit
      // 
      this.DutyReductionTypeDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.DutyReductionTypeDropEdit, "DutyReductionClassificationCode");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).DutyReductionClassificationCode)));
      this.DutyReductionTypeDropEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("162df6c7-f478-4a40-83e0-ae33a2fd02d5", "Duty Reduction Type");
      this.DutyReductionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 3, true);
      this.DutyReductionTypeDropEdit.Name = "DutyReductionTypeDropEdit";
      this.DutyReductionTypeDropEdit.PreBoundMaxLength = 1;
      this.DutyReductionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 17, true);
      this.DutyReductionTypeDropEdit.TabIndex = 0;
      // 
      // SpecificUseCheckBox
      // 
      this.BindingSource.SetBindingMember(this.SpecificUseCheckBox, "JI_IsSpecificUseCode");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_IsSpecificUseCode)));
      this.SpecificUseCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
      this.SpecificUseCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
      this.SpecificUseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 21, true);
      this.SpecificUseCheckBox.Name = "SpecificUseCheckBox";
      this.SpecificUseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
      this.SpecificUseCheckBox.TabIndex = 2;
      this.SpecificUseCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.SpecificUseCheckBox.UseVisualStyleBackColor = true;
      // 
      // DutyReductionCodeFindBox
      // 
      this.DutyReductionCodeFindBox.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.DutyReductionCodeFindBox, "JI_SecondaryPreference");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_SecondaryPreference)));
      this.DutyReductionCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 47, true);
      this.DutyReductionCodeFindBox.Name = "DutyReductionCodeFindBox";
      this.DutyReductionCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
      this.DutyReductionCodeFindBox.ParentType = null;
      this.DutyReductionCodeFindBox.PreBoundMaxLength = 12;
      this.DutyReductionCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 17, true);
      this.DutyReductionCodeFindBox.TabIndex = 0;
      // 
      // InstalmentCodeFindBox
      // 
      this.InstalmentCodeFindBox.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.InstalmentCodeFindBox, "JI_InstallmentCode");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_InstallmentCode)));
      this.InstalmentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 47, true);
      this.InstalmentCodeFindBox.Name = "InstalmentCodeFindBox";
      this.InstalmentCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
      this.InstalmentCodeFindBox.ParentType = null;
      this.InstalmentCodeFindBox.PreBoundMaxLength = 12;
      this.InstalmentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 17, true);
      this.InstalmentCodeFindBox.TabIndex = 3;
      // 
      // RemarkLongTextControl
      // 
      this.RemarkLongTextControl.AllowDrop = true;
      this.RemarkLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
      this.RemarkLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 87, true);
      this.RemarkLongTextControl.Name = "RemarkLongTextControl";
      this.RemarkLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 20, true);
      this.RemarkLongTextControl.TabIndex = 4;
      // 
      // RemarkTextBox
      // 
      this.BindingSource.SetBindingMember(this.RemarkTextBox, "AdditionalInformationContent");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).AdditionalInformationContent)));
      this.RemarkTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 128, true);
      this.RemarkTextBox.Multiline = true;
      this.RemarkTextBox.Name = "RemarkTextBox";
      this.RemarkTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 47, true);
      this.RemarkTextBox.TabIndex = 5;
      // 
      // DetailsUserControl
      // 
      this.CaptionRenderingEnabled = true;
      this.Controls.Add(this.RemarkTextBox);
      this.Controls.Add(this.RemarkLongTextControl);
      this.Controls.Add(this.InstalmentCodeFindBox);
      this.Controls.Add(this.DutyReductionTypeDropEdit);
      this.Controls.Add(this.SpecificUseCheckBox);
      this.Controls.Add(this.DutyReductionCodeFindBox);
      this.Name = "DetailsUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 201, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.DutyReductionTypeDropEdit.ResumeLayout(true);
      this.DutyReductionTypeDropEdit.PerformLayout();
      this.DutyReductionCodeFindBox.ResumeLayout(true);
      this.DutyReductionCodeFindBox.PerformLayout();
      this.InstalmentCodeFindBox.ResumeLayout(true);
      this.InstalmentCodeFindBox.PerformLayout();
      this.RemarkLongTextControl.ResumeLayout(true);
      this.RemarkLongTextControl.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		internal ZDropEdit DutyReductionTypeDropEdit;
		internal ZCheckBox SpecificUseCheckBox;
		internal ZCodeFindBox DutyReductionCodeFindBox;
		internal ZCodeFindBox InstalmentCodeFindBox;
		internal Customs.GUI.LongTextControl RemarkLongTextControl;
		internal ZArchitecture.ZTextBox RemarkTextBox;
	}
}
