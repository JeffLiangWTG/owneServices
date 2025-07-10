namespace Enterprise.UniversalCopy.GUI
{
	partial class UniversalCopyScheduleForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.moduleDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.copyObjectDescriptionTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.taskDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.copyTemplateDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEditWithFixedWidth();
			this.recurrenceControl = new Enterprise.MasterFiles.GUI.Scheduler.RecurrenceControl();
			this.activeCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.deleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.showButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.branchFindbox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.copyTemplateDropEdit.SuspendLayout();
			this.recurrenceControl.SuspendLayout();
			this.branchFindbox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(945, 369, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.branchFindbox);
			this.MainTabPage.Controls.Add(this.activeCheckbox);
			this.MainTabPage.Controls.Add(this.recurrenceControl);
			this.MainTabPage.Controls.Add(this.copyTemplateDropEdit);
			this.MainTabPage.Controls.Add(this.taskDescriptionTextBox);
			this.MainTabPage.Controls.Add(this.copyObjectDescriptionTextbox);
			this.MainTabPage.Controls.Add(this.moduleDescriptionTextBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(937, 342, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 328, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(945, 369, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.deleteButton);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.showButton);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 30, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = true;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(945, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.UniversalCopy.Business.StmUniversalCopyScheduleTask);
			// 
			// moduleDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.moduleDescriptionTextBox, "Parent.ModuleDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.StmUniversalCopyScheduleTask)(null)).Parent.ModuleDescription)));
			this.moduleDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.moduleDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 38, true);
			this.moduleDescriptionTextBox.Name = "moduleDescriptionTextBox";
			this.moduleDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.moduleDescriptionTextBox.TabIndex = 1;
			// 
			// copyObjectDescriptionTextbox
			// 
			this.BindingSource.SetBindingMember(this.copyObjectDescriptionTextbox, "Parent.CopyObjectDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.StmUniversalCopyScheduleTask)(null)).Parent.CopyObjectDescription)));
			this.copyObjectDescriptionTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.copyObjectDescriptionTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 64, true);
			this.copyObjectDescriptionTextbox.Name = "copyObjectDescriptionTextbox";
			this.copyObjectDescriptionTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.copyObjectDescriptionTextbox.TabIndex = 2;
			// 
			// taskDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.taskDescriptionTextBox, "S5_ScheduleDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.StmUniversalCopyScheduleTask)(null)).S5_ScheduleDescription)));
			this.taskDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.taskDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 12, true);
			this.taskDescriptionTextBox.Name = "taskDescriptionTextBox";
			this.taskDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.taskDescriptionTextBox.TabIndex = 0;
			// 
			// copyTemplateDropEdit
			// 
			this.copyTemplateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.copyTemplateDropEdit, "Parent.SUC_S9_CopyTemplate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.UniversalCopy.Business.StmUniversalCopyScheduleTask)(null)).Parent.SUC_S9_CopyTemplate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.StmUniversalCopyScheduleTask)(null)).Parent.Lookups.CopyTemplates)));
			this.copyTemplateDropEdit.BindToList = "Parent.Lookups.CopyTemplates";
			this.copyTemplateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 90, true);
			this.copyTemplateDropEdit.Name = "copyTemplateDropEdit";
			this.copyTemplateDropEdit.PreBoundMaxLength = 34;
			this.copyTemplateDropEdit.ShowDescriptionBox = false;
			this.copyTemplateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.copyTemplateDropEdit.TabIndex = 3;
			// 
			// recurrenceControl
			// 
			this.recurrenceControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.recurrenceControl, "Recurrence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Scheduler.Business.StmScheduleTaskRecurrence)(((Enterprise.UniversalCopy.Business.StmUniversalCopyScheduleTask)(null)).Recurrence)));
			this.recurrenceControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 133, true);
			this.recurrenceControl.Name = "recurrenceControl";
			this.recurrenceControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 189, true);
			this.recurrenceControl.TabIndex = 6;
			// 
			// activeCheckbox
			// 
			this.BindingSource.SetBindingMember(this.activeCheckbox, "S5_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.UniversalCopy.Business.StmUniversalCopyScheduleTask)(null)).S5_IsActive)));
			this.activeCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.activeCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(337, 12, true);
			this.activeCheckbox.Name = "activeCheckbox";
			this.activeCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.activeCheckbox.TabIndex = 7;
			this.activeCheckbox.UseVisualStyleBackColor = true;
			// 
			// deleteButton
			// 
			this.deleteButton.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("2bacb9b1-c19e-4c2d-bc78-f23f2b6f04fa", "Delete");
			this.deleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.deleteButton.TabIndex = 0;
			this.deleteButton.UseVisualStyleBackColor = false;
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// showButton
			// 
			this.showButton.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("4ded9fae-3575-46f8-a8ac-b2b3140a5e7f", "Show Parent Object");
			this.showButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 4, true);
			this.showButton.Name = "showButton";
			this.showButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 23, true);
			this.showButton.TabIndex = 1;
			this.showButton.UseVisualStyleBackColor = false;
			this.showButton.Click += new System.EventHandler(this.showButton_Click);
			// 
			// branchFindbox
			// 
			this.branchFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.branchFindbox, "S5_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.UniversalCopy.Business.StmUniversalCopyScheduleTask)(null)).S5_GB)));
			this.branchFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 12, true);
			this.branchFindbox.Name = "branchFindbox";
			this.branchFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.branchFindbox.TabIndex = 8;
			// 
			// UniversalCopyScheduleForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(945, 425, true);
			this.DataSourceType = typeof(Enterprise.UniversalCopy.Business.StmUniversalCopyScheduleTask);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(910, 450, true);
			this.Name = "UniversalCopyScheduleForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.ResumeLayout(false);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.copyTemplateDropEdit.ResumeLayout(true);
			this.copyTemplateDropEdit.PerformLayout();
			this.recurrenceControl.ResumeLayout(true);
			this.recurrenceControl.PerformLayout();
			this.branchFindbox.ResumeLayout(true);
			this.branchFindbox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox moduleDescriptionTextBox;
		private ZArchitecture.ZTextBox copyObjectDescriptionTextbox;
		private ZArchitecture.ZTextBox taskDescriptionTextBox;
		private ZArchitecture.GUI.ZGuidDropEditWithFixedWidth copyTemplateDropEdit;
		private MasterFiles.GUI.Scheduler.RecurrenceControl recurrenceControl;
		private ZArchitecture.GUI.ZCheckBox activeCheckbox;
		internal ZArchitecture.GUI.ZButton deleteButton;
		internal ZArchitecture.GUI.ZButton showButton;
		private ZArchitecture.GUI.ZGuidFindBox branchFindbox;

	}
}