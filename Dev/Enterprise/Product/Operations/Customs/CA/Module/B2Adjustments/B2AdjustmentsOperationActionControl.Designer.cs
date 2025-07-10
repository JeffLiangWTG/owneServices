namespace Enterprise.Customs.CA.Module
{
	partial class B2AdjustmentsOperationActionControl
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
			this.LastPortDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OverrideExistingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ForceUpdateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LastPortDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Module.B2AdjustmentsOperationalActionMethodApplicator);
			// 
			// LastPortDateEdit
			// 
			this.LastPortDateEdit.AllowDrop = true;
			this.LastPortDateEdit.AutoCompleteMonthThreshold = 1;
			this.LastPortDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LastPortDateEdit, "SubmissionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Module.B2AdjustmentsOperationalActionMethodApplicator)(null)).SubmissionDate)));
			this.LastPortDateEdit.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("20fd44b7-96db-463e-b682-6271b00dfc6a", "Date Submitted");
			this.LastPortDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 12, true);
			this.LastPortDateEdit.Name = "LastPortDateEdit";
			this.LastPortDateEdit.TabIndex = 1;
			// 
			// OverrideExistingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideExistingCheckBox, "OverrideExisting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Module.B2AdjustmentsOperationalActionMethodApplicator)(null)).OverrideExisting)));
			this.OverrideExistingCheckBox.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("97dd18ef-1661-4cf5-9adc-334592611013", "Override Existing Submitted Date if Present?");
			this.OverrideExistingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideExistingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 38, true);
			this.OverrideExistingCheckBox.Name = "OverrideExistingCheckBox";
			this.OverrideExistingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 16, true);
			this.OverrideExistingCheckBox.TabIndex = 2;
			this.OverrideExistingCheckBox.UseVisualStyleBackColor = true;
			// 
			// ForceUpdateCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ForceUpdateCheckBox, "ForceUpdate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Module.B2AdjustmentsOperationalActionMethodApplicator)(null)).ForceUpdate)));
			this.ForceUpdateCheckBox.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("50c37a5d-c7f1-438a-b05f-b552e8da135f", "Update Date Submitted even if earlier than job created date?");
			this.ForceUpdateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ForceUpdateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 60, true);
			this.ForceUpdateCheckBox.Name = "ForceUpdateCheckBox";
			this.ForceUpdateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 16, true);
			this.ForceUpdateCheckBox.TabIndex = 3;
			this.ForceUpdateCheckBox.UseVisualStyleBackColor = true;
			// 
			// B2AdjustmentsOperationActionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ForceUpdateCheckBox);
			this.Controls.Add(this.LastPortDateEdit);
			this.Controls.Add(this.OverrideExistingCheckBox);
			this.Name = "B2AdjustmentsOperationActionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 88, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LastPortDateEdit.ResumeLayout(true);
			this.LastPortDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		Enterprise.ZArchitecture.GUI.ZCheckBox OverrideExistingCheckBox;
		private ZArchitecture.GUI.ZDateEdit LastPortDateEdit;
		private ZArchitecture.GUI.ZCheckBox ForceUpdateCheckBox;
	}
}
