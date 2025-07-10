namespace Enterprise.Customs.KR.GUI
{
	partial class CarnetCustomsAreaDetailsUserControl
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
            this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.BondedAreaCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.DepartmentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CustomsOfficeCodeFindBox.SuspendLayout();
            this.BondedAreaCodeFindBox.SuspendLayout();
            this.DepartmentCodeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // CustomsOfficeCodeFindBox
            // 
            this.CustomsOfficeCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "JE_CustomsOffice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_CustomsOffice)));
            this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 16, true);
            this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
            this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CustomsOfficeCodeFindBox.ParentType = null;
            this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
            this.CustomsOfficeCodeFindBox.TabIndex = 0;
            // 
            // BondedAreaCodeFindBox
            // 
            this.BondedAreaCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BondedAreaCodeFindBox, "JE_LocationOtherInformation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_LocationOtherInformation)));
            this.BondedAreaCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 42, true);
            this.BondedAreaCodeFindBox.Name = "BondedAreaCodeFindBox";
            this.BondedAreaCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.BondedAreaCodeFindBox.ParentType = null;
            this.BondedAreaCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 17, true);
            this.BondedAreaCodeFindBox.TabIndex = 2;
            // 
            // DepartmentCodeFindBox
            // 
            this.DepartmentCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DepartmentCodeFindBox, "JE_CustomsDivision");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_CustomsDivision)));
            this.DepartmentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 16, true);
            this.DepartmentCodeFindBox.Name = "DepartmentCodeFindBox";
            this.DepartmentCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.DepartmentCodeFindBox.ParentType = null;
            this.DepartmentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 17, true);
            this.DepartmentCodeFindBox.TabIndex = 3;
            // 
            // CarnetSEDDetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.DepartmentCodeFindBox);
            this.Controls.Add(this.BondedAreaCodeFindBox);
            this.Controls.Add(this.CustomsOfficeCodeFindBox);
            this.Name = "CarnetSEDDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 77, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CustomsOfficeCodeFindBox.ResumeLayout(true);
            this.CustomsOfficeCodeFindBox.PerformLayout();
            this.BondedAreaCodeFindBox.ResumeLayout(true);
            this.BondedAreaCodeFindBox.PerformLayout();
            this.DepartmentCodeFindBox.ResumeLayout(true);
            this.DepartmentCodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox BondedAreaCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox DepartmentCodeFindBox;
	}
}
