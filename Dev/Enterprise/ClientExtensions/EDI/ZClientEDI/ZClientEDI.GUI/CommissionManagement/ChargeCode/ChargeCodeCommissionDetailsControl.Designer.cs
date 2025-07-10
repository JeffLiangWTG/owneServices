namespace Enterprise.Client.EDI.CommissionManagement.GUI
{
	partial class ChargeCodeCommissionDetailsControl
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
			this.AC_IsCommissionableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AC_DefaultCommissionProductDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AC_DefaultCommissionServiceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AC_DefaultCommissionSubModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DefaultCommissionItemGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.defaultCommissionItemLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AC_DefaultCommissionProductDropEdit.SuspendLayout();
			this.AC_DefaultCommissionServiceDropEdit.SuspendLayout();
			this.AC_DefaultCommissionSubModuleDropEdit.SuspendLayout();
			this.DefaultCommissionItemGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccChargeCode);
			// 
			// AC_IsCommissionableCheckBox
			// 
			this.AC_IsCommissionableCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AC_IsCommissionableCheckBox, "AC_IsCommissionable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_IsCommissionable)));
			this.AC_IsCommissionableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AC_IsCommissionableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.AC_IsCommissionableCheckBox.Name = "AC_IsCommissionableCheckBox";
			this.AC_IsCommissionableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 15, true);
			this.AC_IsCommissionableCheckBox.TabIndex = 0;
			this.AC_IsCommissionableCheckBox.UseVisualStyleBackColor = true;
			// 
			// AC_DefaultCommissionProductDropEdit
			// 
			this.AC_DefaultCommissionProductDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AC_DefaultCommissionProductDropEdit, "AC_DefaultCommissionProduct");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_DefaultCommissionProduct)));
			this.AC_DefaultCommissionProductDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 50, true);
			this.AC_DefaultCommissionProductDropEdit.Name = "AC_DefaultCommissionProductDropEdit";
			this.AC_DefaultCommissionProductDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.AC_DefaultCommissionProductDropEdit.TabIndex = 1;
			// 
			// AC_DefaultCommissionServiceDropEdit
			// 
			this.AC_DefaultCommissionServiceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AC_DefaultCommissionServiceDropEdit, "AC_DefaultCommissionService");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_DefaultCommissionService)));
			this.AC_DefaultCommissionServiceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 73, true);
			this.AC_DefaultCommissionServiceDropEdit.Name = "AC_DefaultCommissionServiceDropEdit";
			this.AC_DefaultCommissionServiceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.AC_DefaultCommissionServiceDropEdit.TabIndex = 2;
			// 
			// AC_DefaultCommissionSubModuleDropEdit
			// 
			this.AC_DefaultCommissionSubModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AC_DefaultCommissionSubModuleDropEdit, "AC_DefaultCommissionSubModule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccChargeCode)(null)).AC_DefaultCommissionSubModule)));
			this.AC_DefaultCommissionSubModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 96, true);
			this.AC_DefaultCommissionSubModuleDropEdit.Name = "AC_DefaultCommissionSubModuleDropEdit";
			this.AC_DefaultCommissionSubModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.AC_DefaultCommissionSubModuleDropEdit.TabIndex = 3;
			// 
			// DefaultCommissionItemGroupBox
			// 
			this.DefaultCommissionItemGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("69bc9280-28d4-4215-a797-ae35d6b9c4e6", "Default Commission Item");
			this.DefaultCommissionItemGroupBox.Controls.Add(this.defaultCommissionItemLabel);
			this.DefaultCommissionItemGroupBox.Controls.Add(this.AC_DefaultCommissionProductDropEdit);
			this.DefaultCommissionItemGroupBox.Controls.Add(this.AC_DefaultCommissionSubModuleDropEdit);
			this.DefaultCommissionItemGroupBox.Controls.Add(this.AC_DefaultCommissionServiceDropEdit);
			this.DefaultCommissionItemGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 27, true);
			this.DefaultCommissionItemGroupBox.Name = "DefaultCommissionItemGroupBox";
			this.DefaultCommissionItemGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 161, true);
			this.DefaultCommissionItemGroupBox.TabIndex = 4;
			this.DefaultCommissionItemGroupBox.TabStop = false;
			// 
			// defaultCommissionItemLabel
			// 
			this.defaultCommissionItemLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("c48297c7-0910-4b15-b285-15efa3f71fd3", "When this charge code is used on a non-job related transaction, the commissionable amount will belong to the below.");
			this.defaultCommissionItemLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 14, true);
			this.defaultCommissionItemLabel.Name = "defaultCommissionItemLabel";
			this.defaultCommissionItemLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(374, 35, true);
			this.defaultCommissionItemLabel.TabIndex = 4;
			// 
			// ChargeCodeCommissionDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DefaultCommissionItemGroupBox);
			this.Controls.Add(this.AC_IsCommissionableCheckBox);
			this.Name = "ChargeCodeCommissionDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 188, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AC_DefaultCommissionProductDropEdit.ResumeLayout(true);
			this.AC_DefaultCommissionProductDropEdit.PerformLayout();
			this.AC_DefaultCommissionServiceDropEdit.ResumeLayout(true);
			this.AC_DefaultCommissionServiceDropEdit.PerformLayout();
			this.AC_DefaultCommissionSubModuleDropEdit.ResumeLayout(true);
			this.AC_DefaultCommissionSubModuleDropEdit.PerformLayout();
			this.DefaultCommissionItemGroupBox.ResumeLayout(false);
			this.DefaultCommissionItemGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCheckBox AC_IsCommissionableCheckBox;
		private ZArchitecture.GUI.ZDropEdit AC_DefaultCommissionProductDropEdit;
		private ZArchitecture.GUI.ZDropEdit AC_DefaultCommissionServiceDropEdit;
		private ZArchitecture.GUI.ZDropEdit AC_DefaultCommissionSubModuleDropEdit;
		private ZArchitecture.GUI.ZGroupBox DefaultCommissionItemGroupBox;
		private ZArchitecture.ZLabel defaultCommissionItemLabel;
	}
}
