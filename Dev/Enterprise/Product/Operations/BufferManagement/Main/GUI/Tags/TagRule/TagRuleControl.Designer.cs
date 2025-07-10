using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	partial class TagRuleControl
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
			this.nameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.actionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FilterStripsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IndexSearchCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.filterCustomisationControl = new Enterprise.BufferManagement.GUI.TagRuleFilterStripWrapperControl();
			this.activeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TagLinkTemplateControl = new Enterprise.BufferManagement.GUI.TagLinkTemplateControl();
			this.IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.detailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.durationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.lastRunTimeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.branchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.departmentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.actionDropEdit.SuspendLayout();
			this.FilterStripsGroupBox.SuspendLayout();
			this.TagLinkTemplateControl.SuspendLayout();
			this.detailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.TagRule);
			// 
			// nameTextBox
			// 
			this.nameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.nameTextBox, "TGR_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.TagRule)(null)).TGR_Name)));
			this.nameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.nameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 19, true);
			this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.nameTextBox.TabIndex = 0;
			// 
			// actionDropEdit
			// 
			this.actionDropEdit.AllowDrop = true;
			this.actionDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.actionDropEdit, "TGR_ActionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.TagRule)(null)).TGR_ActionType)));
			this.actionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 45, true);
			this.actionDropEdit.Name = "actionDropEdit";
			this.actionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.actionDropEdit.TabIndex = 3;
			// 
			// FilterStripsGroupBox
			// 
			this.FilterStripsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FilterStripsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("f4fbbb5e-d5f1-47b9-8dbf-bd9498e7da75", "Filters");
			this.FilterStripsGroupBox.Controls.Add(this.IndexSearchCheckBox);
			this.FilterStripsGroupBox.Controls.Add(this.filterCustomisationControl);
			this.FilterStripsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 250, true);
			this.FilterStripsGroupBox.Name = "FilterStripsGroupBox";
			this.FilterStripsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 199, true);
			this.FilterStripsGroupBox.TabIndex = 5;
			this.FilterStripsGroupBox.TabStop = false;
			// 
			// IndexSearchCheckBox
			//
			this.BindingSource.SetBindingMember(this.IndexSearchCheckBox, "Filter.S9_IsIndexSearch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.TagRule)(null)).Filter.S9_IsIndexSearch)));
			this.IndexSearchCheckBox.AutoSize = true;
			this.IndexSearchCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IndexSearchCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 22, true);
			this.IndexSearchCheckBox.Name = "IndexSearchCheckBox";
			this.IndexSearchCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 20, true);
			this.IndexSearchCheckBox.TabIndex = 6;
			this.IndexSearchCheckBox.Text = Enterprise.BufferManagement.GUI.Res.GetString("68113234-4d0a-4e32-b797-e2a8bcd533d9", "Index search");
			this.IndexSearchCheckBox.UseVisualStyleBackColor = true;
			this.IndexSearchCheckBox.CheckedChanged += new System.EventHandler(this.IndexSearchCheckBox_CheckedChanged);
			// 
			// filterCustomisationControl
			// 
			this.filterCustomisationControl.AllowDrop = true;
			this.filterCustomisationControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.filterCustomisationControl, "Filter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.StmModuleFilter)(((Enterprise.BufferManagement.Business.TagRule)(null)).Filter)));
			this.filterCustomisationControl.FilterControlIdentifier = this.filterCustomisationControl.ControlIdentifier;
			this.filterCustomisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 50, true);
			this.filterCustomisationControl.Name = "filterCustomisationControl";
			this.filterCustomisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 180, true);
			this.filterCustomisationControl.TabIndex = 9;
			filterCustomisationControl.IsPreviewAllowed = true;
			// 
			// activeCheckBox
			// 
			this.activeCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.activeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.activeCheckBox, "TGR_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.TagRule)(null)).TGR_IsActive)));
			this.activeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.activeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 48, true);
			this.activeCheckBox.Name = "activeCheckBox";
			this.activeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.activeCheckBox.TabIndex = 7;
			this.activeCheckBox.UseVisualStyleBackColor = true;
			// 
			// TagLinkTemplateControl
			// 
			this.TagLinkTemplateControl.AllowDrop = true;
			this.TagLinkTemplateControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TagLinkTemplateControl, "TagTemplate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.Business.TagLinkTemplate)(((Enterprise.BufferManagement.Business.TagRule)(null)).TagTemplate)));
			this.TagLinkTemplateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 109, true);
			this.TagLinkTemplateControl.Name = "TagLinkTemplateControl";
			this.TagLinkTemplateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 133, true);
			this.TagLinkTemplateControl.TabIndex = 4;
			// 
			// IsSystemCheckBox
			// 
			this.IsSystemCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.IsSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSystemCheckBox, "TGR_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.TagRule)(null)).TGR_IsSystem)));
			this.IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 47, true);
			this.IsSystemCheckBox.Name = "IsSystemCheckBox";
			this.IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			this.IsSystemCheckBox.TabIndex = 8;
			this.IsSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// detailsGroupBox
			// 
			this.detailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.detailsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("0cb3dcdb-0e08-47dd-8b80-6ac2ef9caa03", "Details");
			this.detailsGroupBox.Controls.Add(this.durationTextBox);
			this.detailsGroupBox.Controls.Add(this.lastRunTimeTextBox);
			this.detailsGroupBox.Controls.Add(this.nameTextBox);
			this.detailsGroupBox.Controls.Add(this.actionDropEdit);
			this.detailsGroupBox.Controls.Add(this.activeCheckBox);
			this.detailsGroupBox.Controls.Add(this.IsSystemCheckBox);
			this.detailsGroupBox.Controls.Add(this.branchGuidFindBox);
			this.detailsGroupBox.Controls.Add(this.departmentGuidFindBox);
			this.detailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.detailsGroupBox.Name = "detailsGroupBox";
			this.detailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 100, true);
			this.detailsGroupBox.TabIndex = 11;
			this.detailsGroupBox.TabStop = false;
			// 
			// durationTextBox
			// 
			this.durationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.durationTextBox, "LastRunDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.TagRule)(null)).LastRunDuration)));
			this.durationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(517, 19, true);
			this.durationTextBox.Name = "durationTextBox";
			this.durationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.durationTextBox.TabIndex = 2;
			this.durationTextBox.TabStop = false;
			// 
			// lastRunTimeTextBox
			// 
			this.lastRunTimeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.lastRunTimeTextBox, "LastRunStartTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.TagRule)(null)).LastRunStartTimeLocal)));
			this.lastRunTimeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(343, 19, true);
			this.lastRunTimeTextBox.Name = "lastRunTimeTextBox";
			this.lastRunTimeTextBox.ReadOnly = true;
			this.lastRunTimeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.lastRunTimeTextBox.TabIndex = 1;
			this.lastRunTimeTextBox.TabStop = false;
			// 
			// branchGuidFindBox
			// 
			this.branchGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.branchGuidFindBox, "TGR_GB_Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.TagRule)(null)).TGR_GB_Branch)));
			this.branchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 72, true);
			this.branchGuidFindBox.Name = "branchGuidFindBox";
			this.branchGuidFindBox.ShowDescriptionBox = false;
			this.branchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.branchGuidFindBox.TabIndex = 10;
			this.branchGuidFindBox.TabStop = true;
			// 
			// departmentGuidFindBox
			// 
			//this.departmentGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.departmentGuidFindBox, "TGR_GE_Department");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.TagRule)(null)).TGR_GE_Department)));
			this.departmentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 72, true);
			this.departmentGuidFindBox.Name = "departmentGuidFindBox";
			this.departmentGuidFindBox.ShowDescriptionBox = false;
			this.departmentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.departmentGuidFindBox.TabIndex = 12;
			this.departmentGuidFindBox.TabStop = true;
			// 
			// TagRuleControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.detailsGroupBox);
			this.Controls.Add(this.FilterStripsGroupBox);
			this.Controls.Add(this.TagLinkTemplateControl);
			this.Name = "TagRuleControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(582, 452, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.actionDropEdit.ResumeLayout(true);
			this.actionDropEdit.PerformLayout();
			this.FilterStripsGroupBox.ResumeLayout(false);
			this.FilterStripsGroupBox.PerformLayout();
			this.TagLinkTemplateControl.ResumeLayout(true);
			this.TagLinkTemplateControl.PerformLayout();
			this.detailsGroupBox.ResumeLayout(false);
			this.detailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox nameTextBox;
		private ZArchitecture.GUI.ZDropEdit actionDropEdit;
		private TagLinkTemplateControl TagLinkTemplateControl;
		private ZArchitecture.GUI.ZGroupBox FilterStripsGroupBox;
		private ZCheckBox IndexSearchCheckBox;
		private TagRuleFilterStripWrapperControl filterCustomisationControl;
		private ZArchitecture.GUI.ZCheckBox activeCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsSystemCheckBox;
		private ZArchitecture.GUI.ZGroupBox detailsGroupBox;
		private ZArchitecture.ZTextBox lastRunTimeTextBox;
		private ZArchitecture.ZTextBox durationTextBox;
		private ZArchitecture.GUI.ZGuidFindBox branchGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox departmentGuidFindBox;
	}
}
