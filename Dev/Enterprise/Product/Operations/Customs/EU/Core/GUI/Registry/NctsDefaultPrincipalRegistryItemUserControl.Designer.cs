namespace Enterprise.Customs.EU.GUI.Registry
{
	partial class NctsDefaultPrincipalRegistryItemUserControl
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
			this.LeaveBlankCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PrincipalGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PrincipalGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Registry.NctsDefaultPrincipal);
			// 
			// LeaveBlankCheckBox
			// 
			this.BindingSource.SetBindingMember(this.LeaveBlankCheckBox, "LeaveBlank");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Registry.NctsDefaultPrincipal)(null)).LeaveBlank)));
			this.LeaveBlankCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LeaveBlankCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 12, true);
			this.LeaveBlankCheckBox.Name = "LeaveBlankCheckBox";
			this.LeaveBlankCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 24, true);
			this.LeaveBlankCheckBox.TabIndex = 0;
			// 
			// PrincipalGuidFindBox
			// 
			this.PrincipalGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PrincipalGuidFindBox, "Principal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Registry.NctsDefaultPrincipal)(null)).Principal)));
			this.PrincipalGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 42, true);
			this.PrincipalGuidFindBox.Name = "PrincipalGuidFindBox";
			this.PrincipalGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PrincipalGuidFindBox.ParentType = null;
			this.PrincipalGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.PrincipalGuidFindBox.TabIndex = 1;
			// 
			// NctsDefaultPrincipalRegistryItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LeaveBlankCheckBox);
			this.Controls.Add(this.PrincipalGuidFindBox);
			this.Name = "NctsDefaultPrincipalRegistryItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 85, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PrincipalGuidFindBox.ResumeLayout(true);
			this.PrincipalGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZCheckBox LeaveBlankCheckBox;
		ZArchitecture.GUI.ZGuidFindBox PrincipalGuidFindBox;
	}
}
