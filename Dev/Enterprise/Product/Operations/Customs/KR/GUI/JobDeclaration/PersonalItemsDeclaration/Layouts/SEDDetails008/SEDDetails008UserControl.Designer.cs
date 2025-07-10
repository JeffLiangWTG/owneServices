namespace Enterprise.Customs.KR.GUI
{
	partial class SEDDetails008UserControl
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
            this.StayPeriodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.DepartmentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.WeaponDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DrugDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.AnimalsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.EndangeredItemsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CounterfeitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CommercialUseItemsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ExcessTimeLimitItemsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PornographyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.ServiceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.BranchCodeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.HasItemsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.StayPeriodDropEdit.SuspendLayout();
            this.CustomsOfficeCodeFindBox.SuspendLayout();
            this.DepartmentCodeFindBox.SuspendLayout();
            this.WeaponDropEdit.SuspendLayout();
            this.DrugDropEdit.SuspendLayout();
            this.AnimalsDropEdit.SuspendLayout();
            this.EndangeredItemsDropEdit.SuspendLayout();
            this.CounterfeitDropEdit.SuspendLayout();
            this.CommercialUseItemsDropEdit.SuspendLayout();
            this.ExcessTimeLimitItemsDropEdit.SuspendLayout();
            this.PornographyDropEdit.SuspendLayout();
            this.BrokerCodeFindBox.SuspendLayout();
            this.ServiceCodeFindBox.SuspendLayout();
            this.BranchCodeGuidFindBox.SuspendLayout();
            this.HasItemsDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // StayPeriodDropEdit
            // 
            this.StayPeriodDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.StayPeriodDropEdit, "JE_MessageSubType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_MessageSubType)));
            this.StayPeriodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 13, true);
            this.StayPeriodDropEdit.Name = "StayPeriodDropEdit";
            this.StayPeriodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
            this.StayPeriodDropEdit.TabIndex = 0;
            // 
            // CustomsOfficeCodeFindBox
            // 
            this.CustomsOfficeCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "JE_CustomsOffice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_CustomsOffice)));
            this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 39, true);
            this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
            this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CustomsOfficeCodeFindBox.ParentType = null;
            this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
            this.CustomsOfficeCodeFindBox.TabIndex = 1;
            // 
            // DepartmentCodeFindBox
            // 
            this.DepartmentCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DepartmentCodeFindBox, "JE_CustomsDivision");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_CustomsDivision)));
            this.DepartmentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 65, true);
            this.DepartmentCodeFindBox.Name = "DepartmentCodeFindBox";
            this.DepartmentCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.DepartmentCodeFindBox.ParentType = null;
            this.DepartmentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
            this.DepartmentCodeFindBox.TabIndex = 2;
            // 
            // WeaponDropEdit
            // 
            this.WeaponDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.WeaponDropEdit, "PIDWeapon");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).PIDWeapon)));
            this.WeaponDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 91, true);
            this.WeaponDropEdit.Name = "WeaponDropEdit";
            this.WeaponDropEdit.PreBoundMaxLength = 1;
            this.WeaponDropEdit.ShowDescriptionBox = false;
            this.WeaponDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
            this.WeaponDropEdit.TabIndex = 3;
            // 
            // DrugDropEdit
            // 
            this.DrugDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DrugDropEdit, "PIDDrug");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).PIDDrug)));
            this.DrugDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 117, true);
            this.DrugDropEdit.Name = "DrugDropEdit";
            this.DrugDropEdit.PreBoundMaxLength = 1;
            this.DrugDropEdit.ShowDescriptionBox = false;
            this.DrugDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
            this.DrugDropEdit.TabIndex = 5;
            // 
            // AnimalsDropEdit
            // 
            this.AnimalsDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AnimalsDropEdit, "PIDAnimal");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).PIDAnimal)));
            this.AnimalsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 143, true);
            this.AnimalsDropEdit.Name = "AnimalsDropEdit";
            this.AnimalsDropEdit.PreBoundMaxLength = 1;
            this.AnimalsDropEdit.ShowDescriptionBox = false;
            this.AnimalsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
            this.AnimalsDropEdit.TabIndex = 6;
            // 
            // EndangeredItemsDropEdit
            // 
            this.EndangeredItemsDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.EndangeredItemsDropEdit, "PIDEndangeredItems");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).PIDEndangeredItems)));
            this.EndangeredItemsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 169, true);
            this.EndangeredItemsDropEdit.Name = "EndangeredItemsDropEdit";
            this.EndangeredItemsDropEdit.PreBoundMaxLength = 1;
            this.EndangeredItemsDropEdit.ShowDescriptionBox = false;
            this.EndangeredItemsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
            this.EndangeredItemsDropEdit.TabIndex = 7;
            // 
            // CounterfeitDropEdit
            // 
            this.CounterfeitDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CounterfeitDropEdit, "PIDCounterfeit");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).PIDCounterfeit)));
            this.CounterfeitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 195, true);
            this.CounterfeitDropEdit.Name = "CounterfeitDropEdit";
            this.CounterfeitDropEdit.PreBoundMaxLength = 1;
            this.CounterfeitDropEdit.ShowDescriptionBox = false;
            this.CounterfeitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
            this.CounterfeitDropEdit.TabIndex = 8;
            // 
            // CommercialUseItemsDropEdit
            // 
            this.CommercialUseItemsDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CommercialUseItemsDropEdit, "PIDCommercialUseItems");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).PIDCommercialUseItems)));
            this.CommercialUseItemsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 221, true);
            this.CommercialUseItemsDropEdit.Name = "CommercialUseItemsDropEdit";
            this.CommercialUseItemsDropEdit.PreBoundMaxLength = 1;
            this.CommercialUseItemsDropEdit.ShowDescriptionBox = false;
            this.CommercialUseItemsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
            this.CommercialUseItemsDropEdit.TabIndex = 9;
            // 
            // ExcessTimeLimitItemsDropEdit
            // 
            this.ExcessTimeLimitItemsDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ExcessTimeLimitItemsDropEdit, "PIDExcessTimeLimitItems");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).PIDExcessTimeLimitItems)));
            this.ExcessTimeLimitItemsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 247, true);
            this.ExcessTimeLimitItemsDropEdit.Name = "ExcessTimeLimitItemsDropEdit";
            this.ExcessTimeLimitItemsDropEdit.PreBoundMaxLength = 1;
            this.ExcessTimeLimitItemsDropEdit.ShowDescriptionBox = false;
            this.ExcessTimeLimitItemsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
            this.ExcessTimeLimitItemsDropEdit.TabIndex = 10;
            // 
            // PornographyDropEdit
            // 
            this.PornographyDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PornographyDropEdit, "PIDPornography");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).PIDPornography)));
            this.PornographyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 273, true);
            this.PornographyDropEdit.Name = "PornographyDropEdit";
            this.PornographyDropEdit.PreBoundMaxLength = 1;
            this.PornographyDropEdit.ShowDescriptionBox = false;
            this.PornographyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
            this.PornographyDropEdit.TabIndex = 11;
            // 
            // BrokerCodeFindBox
            // 
            this.BrokerCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "JE_GS_NKCusAgent");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_GS_NKCusAgent)));
            this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 325, true);
            this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
            this.BrokerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.BrokerCodeFindBox.ParentType = null;
            this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
            this.BrokerCodeFindBox.TabIndex = 13;
            // 
            // ServiceCodeFindBox
            // 
            this.ServiceCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ServiceCodeFindBox, "JE_RS_NKServiceLevel");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_RS_NKServiceLevel)));
            this.ServiceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 351, true);
            this.ServiceCodeFindBox.Name = "ServiceCodeFindBox";
            this.ServiceCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ServiceCodeFindBox.ParentType = null;
            this.ServiceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
            this.ServiceCodeFindBox.TabIndex = 14;
            // 
            // BranchCodeGuidFindBox
            // 
            this.BranchCodeGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BranchCodeGuidFindBox, "JE_GB");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_GB)));
            this.BranchCodeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 299, true);
            this.BranchCodeGuidFindBox.Name = "BranchCodeGuidFindBox";
            this.BranchCodeGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.BranchCodeGuidFindBox.ParentType = null;
            this.BranchCodeGuidFindBox.PreBoundMaxLength = 3;
            this.BranchCodeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
            this.BranchCodeGuidFindBox.TabIndex = 12;
            // 
            // HasItemsDropEdit
            // 
            this.HasItemsDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.HasItemsDropEdit, "PIDHasItems");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).PIDHasItems)));
            this.HasItemsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 235, true);
            this.HasItemsDropEdit.Name = "HasItemsDropEdit";
            this.HasItemsDropEdit.PreBoundMaxLength = 1;
            this.HasItemsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
            this.HasItemsDropEdit.TabIndex = 4;
            // 
            // StayPeriodUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.HasItemsDropEdit);
            this.Controls.Add(this.BranchCodeGuidFindBox);
            this.Controls.Add(this.ServiceCodeFindBox);
            this.Controls.Add(this.BrokerCodeFindBox);
            this.Controls.Add(this.PornographyDropEdit);
            this.Controls.Add(this.ExcessTimeLimitItemsDropEdit);
            this.Controls.Add(this.CommercialUseItemsDropEdit);
            this.Controls.Add(this.CounterfeitDropEdit);
            this.Controls.Add(this.EndangeredItemsDropEdit);
            this.Controls.Add(this.AnimalsDropEdit);
            this.Controls.Add(this.DrugDropEdit);
            this.Controls.Add(this.WeaponDropEdit);
            this.Controls.Add(this.DepartmentCodeFindBox);
            this.Controls.Add(this.CustomsOfficeCodeFindBox);
            this.Controls.Add(this.StayPeriodDropEdit);
            this.Name = "StayPeriodUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 383, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.StayPeriodDropEdit.ResumeLayout(true);
            this.StayPeriodDropEdit.PerformLayout();
            this.CustomsOfficeCodeFindBox.ResumeLayout(true);
            this.CustomsOfficeCodeFindBox.PerformLayout();
            this.DepartmentCodeFindBox.ResumeLayout(true);
            this.DepartmentCodeFindBox.PerformLayout();
            this.WeaponDropEdit.ResumeLayout(true);
            this.WeaponDropEdit.PerformLayout();
            this.DrugDropEdit.ResumeLayout(true);
            this.DrugDropEdit.PerformLayout();
            this.AnimalsDropEdit.ResumeLayout(true);
            this.AnimalsDropEdit.PerformLayout();
            this.EndangeredItemsDropEdit.ResumeLayout(true);
            this.EndangeredItemsDropEdit.PerformLayout();
            this.CounterfeitDropEdit.ResumeLayout(true);
            this.CounterfeitDropEdit.PerformLayout();
            this.CommercialUseItemsDropEdit.ResumeLayout(true);
            this.CommercialUseItemsDropEdit.PerformLayout();
            this.ExcessTimeLimitItemsDropEdit.ResumeLayout(true);
            this.ExcessTimeLimitItemsDropEdit.PerformLayout();
            this.PornographyDropEdit.ResumeLayout(true);
            this.PornographyDropEdit.PerformLayout();
            this.BrokerCodeFindBox.ResumeLayout(true);
            this.BrokerCodeFindBox.PerformLayout();
            this.ServiceCodeFindBox.ResumeLayout(true);
            this.ServiceCodeFindBox.PerformLayout();
            this.BranchCodeGuidFindBox.ResumeLayout(true);
            this.BranchCodeGuidFindBox.PerformLayout();
            this.HasItemsDropEdit.ResumeLayout(true);
            this.HasItemsDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit StayPeriodDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox DepartmentCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit WeaponDropEdit;
		private ZArchitecture.GUI.ZDropEdit DrugDropEdit;
		private ZArchitecture.GUI.ZDropEdit AnimalsDropEdit;
		private ZArchitecture.GUI.ZDropEdit EndangeredItemsDropEdit;
		private ZArchitecture.GUI.ZDropEdit CounterfeitDropEdit;
		private ZArchitecture.GUI.ZDropEdit CommercialUseItemsDropEdit;
		private ZArchitecture.GUI.ZDropEdit ExcessTimeLimitItemsDropEdit;
		private ZArchitecture.GUI.ZDropEdit PornographyDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox ServiceCodeFindBox;
		private ZArchitecture.GUI.ZGuidFindBox BranchCodeGuidFindBox;
		private ZArchitecture.GUI.ZDropEdit HasItemsDropEdit;
	}
}
