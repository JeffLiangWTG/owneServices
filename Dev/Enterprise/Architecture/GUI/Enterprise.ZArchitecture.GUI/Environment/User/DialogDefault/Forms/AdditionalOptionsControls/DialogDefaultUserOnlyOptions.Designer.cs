using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.DialogDefault
{
	partial class DialogDefaultUserOnlyOptions
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
			this.keepShowingCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.applyToAllContextsCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.applyToAllContextsCheckbox);
			this.zGroupBox1.Controls.Add(this.keepShowingCheckbox);
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 72, true);
			// 
			// keepShowingCheckbox
			// 
			this.keepShowingCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.keepShowingCheckbox, "KeepShowingDialog");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Core.DialogDefault.DialogDefaultSaveOptions)(null)).KeepShowingDialog)));
			this.keepShowingCheckbox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("c9aae524-3eec-4d48-b450-b8aa9437fa12", "Keep Showing This Dialog");
			this.keepShowingCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.keepShowingCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 19, true);
			this.keepShowingCheckbox.Name = "keepShowingCheckbox";
			this.keepShowingCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 17, true);
			this.keepShowingCheckbox.TabIndex = 0;
			this.keepShowingCheckbox.UseVisualStyleBackColor = true;
			// 
			// applyToAllContextsCheckbox
			// 
			this.applyToAllContextsCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.applyToAllContextsCheckbox, "SaveForAllContexts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Core.DialogDefault.DialogDefaultSaveOptions)(null)).SaveForAllContexts)));
			this.applyToAllContextsCheckbox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("18a1e4f4-2cb4-4c28-89f8-6be7b5177c8b", "Apply to Similar Dialogs");
			this.applyToAllContextsCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.applyToAllContextsCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 42, true);
			this.applyToAllContextsCheckbox.Name = "applyToAllContextsCheckbox";
			this.applyToAllContextsCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.applyToAllContextsCheckbox.TabIndex = 1;
			this.applyToAllContextsCheckbox.UseVisualStyleBackColor = true;
			// 
			// DialogDefaultUserOnlyOptions
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 79, true);
			this.Name = "DialogDefaultUserOnlyOptions";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 79, true);
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZCheckBox keepShowingCheckbox;
		private ZCheckBox applyToAllContextsCheckbox;
	}
}
