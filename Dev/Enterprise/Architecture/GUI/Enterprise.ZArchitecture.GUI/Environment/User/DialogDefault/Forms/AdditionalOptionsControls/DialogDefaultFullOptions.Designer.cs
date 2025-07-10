using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.DialogDefault
{
	partial class DialogDefaultFullOptions
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private IContainer components = null; // SuppressCodeSmell Reason = Designer requires this variable

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
		void InitializeComponent()
		{
			this.defaultsLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.allowPersonalRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.overridePersonalRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.saveForMeAsWellCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.keepShowingDialogCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.saveForAllContextsCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.defaultsLevelDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.saveForAllContextsCheckbox);
			this.zGroupBox1.Controls.Add(this.keepShowingDialogCheckBox);
			this.zGroupBox1.Controls.Add(this.saveForMeAsWellCheckbox);
			this.zGroupBox1.Controls.Add(this.overridePersonalRadioButton);
			this.zGroupBox1.Controls.Add(this.allowPersonalRadioButton);
			this.zGroupBox1.Controls.Add(this.defaultsLevelDropEdit);
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 115, true);
			// 
			// defaultsLevelDropEdit
			// 
			this.defaultsLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.defaultsLevelDropEdit, "Level");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Core.DialogDefault.DialogDefaultSaveOptions)(null)).Level)));
			this.defaultsLevelDropEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("9c91f918-0ebd-403f-83fa-df0a436b861c", "Save for");
			this.defaultsLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 19, true);
			this.defaultsLevelDropEdit.Name = "defaultsLevelDropEdit";
			this.defaultsLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.defaultsLevelDropEdit.TabIndex = 0;
			// 
			// allowPersonalRadioButton
			// 
			this.allowPersonalRadioButton.AutoCheck = false;
			this.allowPersonalRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.allowPersonalRadioButton, "AllowPersonal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Core.DialogDefault.DialogDefaultSaveOptions)(null)).AllowPersonal)));
			this.allowPersonalRadioButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("82edaebf-67e7-43e9-9fe3-733a5bf370b2", "Allow Personal Defaults");
			this.allowPersonalRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.allowPersonalRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 46, true);
			this.allowPersonalRadioButton.Name = "allowPersonalRadioButton";
			this.allowPersonalRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.allowPersonalRadioButton.TabIndex = 1;
			this.allowPersonalRadioButton.TabStop = true;
			this.allowPersonalRadioButton.UseVisualStyleBackColor = true;
			// 
			// overridePersonalRadioButton
			// 
			this.overridePersonalRadioButton.AutoCheck = false;
			this.overridePersonalRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.overridePersonalRadioButton, "OverridePersonal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Core.DialogDefault.DialogDefaultSaveOptions)(null)).OverridePersonal)));
			this.overridePersonalRadioButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("5a291795-a28d-4a71-b5f3-218380aabc7f", "Override Personal Defaults");
			this.overridePersonalRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.overridePersonalRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 69, true);
			this.overridePersonalRadioButton.Name = "overridePersonalRadioButton";
			this.overridePersonalRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 17, true);
			this.overridePersonalRadioButton.TabIndex = 2;
			this.overridePersonalRadioButton.TabStop = true;
			this.overridePersonalRadioButton.UseVisualStyleBackColor = true;
			// 
			// saveForMeAsWellCheckbox
			// 
			this.saveForMeAsWellCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.saveForMeAsWellCheckbox, "SaveForMeAsWell");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Core.DialogDefault.DialogDefaultSaveOptions)(null)).SaveForMeAsWell)));
			this.saveForMeAsWellCheckbox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("24a1920a-d9f5-414f-97f6-36861c527fa8", "Save for Me As Well");
			this.saveForMeAsWellCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.saveForMeAsWellCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 69, true);
			this.saveForMeAsWellCheckbox.Name = "saveForMeAsWellCheckbox";
			this.saveForMeAsWellCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 17, true);
			this.saveForMeAsWellCheckbox.TabIndex = 3;
			this.saveForMeAsWellCheckbox.UseVisualStyleBackColor = true;
			// 
			// keepShowingDialogCheckBox
			// 
			this.keepShowingDialogCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.keepShowingDialogCheckBox, "KeepShowingDialog");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Core.DialogDefault.DialogDefaultSaveOptions)(null)).KeepShowingDialog)));
			this.keepShowingDialogCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("58f124f3-3f9f-45c5-8e74-839792dbe409", "Keep Showing This Dialog");
			this.keepShowingDialogCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.keepShowingDialogCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 46, true);
			this.keepShowingDialogCheckBox.Name = "keepShowingDialogCheckBox";
			this.keepShowingDialogCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 17, true);
			this.keepShowingDialogCheckBox.TabIndex = 4;
			this.keepShowingDialogCheckBox.UseVisualStyleBackColor = true;
			// 
			// saveForAllContextsCheckbox
			// 
			this.saveForAllContextsCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.saveForAllContextsCheckbox, "SaveForAllContexts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Core.DialogDefault.DialogDefaultSaveOptions)(null)).SaveForAllContexts)));
			this.saveForAllContextsCheckbox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("f05612fa-622d-4ebf-bec7-6995cb2b7a75", "Apply to Similar Dialogs");
			this.saveForAllContextsCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.saveForAllContextsCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 92, true);
			this.saveForAllContextsCheckbox.Name = "saveForAllContextsCheckbox";
			this.saveForAllContextsCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.saveForAllContextsCheckbox.TabIndex = 5;
			this.saveForAllContextsCheckbox.UseVisualStyleBackColor = true;
			// 
			// DialogDefaultFullOptions
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 122, true);
			this.Name = "DialogDefaultFullOptions";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 122, true);
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.defaultsLevelDropEdit.ResumeLayout(true);
			this.defaultsLevelDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZCheckBox saveForAllContextsCheckbox;
		private ZCheckBox keepShowingDialogCheckBox;
		private ZCheckBox saveForMeAsWellCheckbox;
		private ZRadioButton overridePersonalRadioButton;
		private ZRadioButton allowPersonalRadioButton;
		private ZDropEdit defaultsLevelDropEdit;
	}
}
