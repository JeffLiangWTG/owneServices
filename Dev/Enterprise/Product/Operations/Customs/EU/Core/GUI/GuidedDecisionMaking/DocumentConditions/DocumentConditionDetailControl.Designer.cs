using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class DocumentConditionDetailControl
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
			this.IsTickedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DateOfIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConditionCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DateOfIssueDateEdit.SuspendLayout();
			this.ConditionCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.GuidedDecisionMakingConditionDetail);
			// 
			// IsTickedCheckBox
			// 
			this.IsTickedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsTickedCheckBox, "IsTicked");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingConditionDetail)(null)).IsTicked)));
			this.IsTickedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 3, true);
			this.IsTickedCheckBox.Name = "IsTickedCheckBox";
			this.IsTickedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 12, true);
			this.IsTickedCheckBox.TabIndex = 0;
			this.IsTickedCheckBox.UseVisualStyleBackColor = true;
			// 
			// DateOfIssueDateEdit
			// 
			this.DateOfIssueDateEdit.AllowDrop = true;
			this.DateOfIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DateOfIssueDateEdit, "DateOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingConditionDetail)(null)).DateOfIssue)));
			this.DateOfIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(833, 2, true);
			this.DateOfIssueDateEdit.Name = "DateOfIssueDateEdit";
			this.DateOfIssueDateEdit.TabIndex = 3;
			// 
			// ReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceTextBox, "Reference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingConditionDetail)(null)).Reference)));
			this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(629, 1, true);
			this.ReferenceTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ReferenceTextBox.Name = "ReferenceTextBox";
			this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 17, true);
			this.ReferenceTextBox.TabIndex = 2;
			// 
			// ConditionCodeFindBox
			// 
			this.ConditionCodeFindBox.AllowDrop = true;
			this.ConditionCodeFindBox.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.BindingSource.SetBindingMember(this.ConditionCodeFindBox, "Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingConditionDetail)(null)).Code)));
			this.ConditionCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 1, true);
			this.ConditionCodeFindBox.Name = "ConditionCodeFindBox";
			this.ConditionCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ConditionCodeFindBox.ParentType = null;
			this.ConditionCodeFindBox.PreBoundMaxLength = 5;
			this.ConditionCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 17, true);
			this.ConditionCodeFindBox.TabIndex = 1;
			// 
			// DocumentConditionDetailControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConditionCodeFindBox);
			this.Controls.Add(this.ReferenceTextBox);
			this.Controls.Add(this.DateOfIssueDateEdit);
			this.Controls.Add(this.IsTickedCheckBox);
			this.Name = "DocumentConditionDetailControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DateOfIssueDateEdit.ResumeLayout(true);
			this.DateOfIssueDateEdit.PerformLayout();
			this.ConditionCodeFindBox.ResumeLayout(true);
			this.ConditionCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZCheckBox IsTickedCheckBox;
		private ZDateEdit DateOfIssueDateEdit;
		private ZTextBox ReferenceTextBox;
		private ZCodeFindBox ConditionCodeFindBox;
		
	}
}
