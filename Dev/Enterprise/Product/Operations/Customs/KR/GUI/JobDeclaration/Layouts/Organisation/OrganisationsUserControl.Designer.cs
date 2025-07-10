
namespace Enterprise.Customs.KR.GUI
{
	partial class OrganisationsUserControl
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
			this.IndustrialParkCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExpoterAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.FinalBondedWarehouseCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.StevedoreAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ManufacturerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ExporterGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SupplierAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ImporterAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.PayerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AuthorGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AuthorPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.ResponsiblePersonGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ResponsiblePersonPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IndustrialParkCodeCodeFindBox.SuspendLayout();
			this.ExpoterAddressControl.SuspendLayout();
			this.FinalBondedWarehouseCodeFindBox.SuspendLayout();
			this.StevedoreAddressControl.SuspendLayout();
			this.ManufacturerGuidFindBox.SuspendLayout();
			this.ExporterGuidFindBox.SuspendLayout();
			this.SupplierAddressControl.SuspendLayout();
			this.ImporterAddressControl.SuspendLayout();
			this.PayerGuidFindBox.SuspendLayout();
			this.AuthorGroupBox.SuspendLayout();
			this.ResponsiblePersonGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// IndustrialParkCodeCodeFindBox
			// 
			this.IndustrialParkCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IndustrialParkCodeCodeFindBox, "IndustrialParkCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).IndustrialParkCode)));
			this.IndustrialParkCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 83, true);
			this.IndustrialParkCodeCodeFindBox.Name = "IndustrialParkCodeCodeFindBox";
			this.IndustrialParkCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.IndustrialParkCodeCodeFindBox.ParentType = null;
			this.IndustrialParkCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.IndustrialParkCodeCodeFindBox.TabIndex = 2;
			// 
			// ExpoterAddressControl
			// 
			this.ExpoterAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExpoterAddressControl, "JE_OA_SellerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_OA_SellerAddress)));
			this.ExpoterAddressControl.BindToOrgList = "Lookups.Organisations";
			this.ExpoterAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 121, true);
			this.ExpoterAddressControl.Name = "ExpoterAddressControl";
			this.ExpoterAddressControl.PopupCaption = "";
			this.ExpoterAddressControl.ShowAddress = false;
			this.ExpoterAddressControl.ShowOrganisationName = true;
			this.ExpoterAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.ExpoterAddressControl.TabIndex = 3;
			// 
			// FinalBondedWarehouseCodeFindBox
			// 
			this.FinalBondedWarehouseCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FinalBondedWarehouseCodeFindBox, "FinalBondedWarehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FinalBondedWarehouse)));
			this.FinalBondedWarehouseCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 159, true);
			this.FinalBondedWarehouseCodeFindBox.Name = "FinalBondedWarehouseCodeFindBox";
			this.FinalBondedWarehouseCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FinalBondedWarehouseCodeFindBox.ParentType = null;
			this.FinalBondedWarehouseCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.FinalBondedWarehouseCodeFindBox.TabIndex = 4;
			// 
			// StevedoreAddressControl
			// 
			this.StevedoreAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StevedoreAddressControl, "StevedoreCompanyAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).StevedoreCompanyAddress)));
			this.StevedoreAddressControl.BindToOrgList = "Lookups.Organisations";
			this.StevedoreAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 276, true);
			this.StevedoreAddressControl.Name = "StevedoreAddressControl";
			this.StevedoreAddressControl.PopupCaption = "";
			this.StevedoreAddressControl.ShowAddress = false;
			this.StevedoreAddressControl.ShowOrganisationName = true;
			this.StevedoreAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.StevedoreAddressControl.TabIndex = 7;
			// 
			// ManufacturerGuidFindBox
			// 
			this.ManufacturerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerGuidFindBox, "JE_OH_Manufacturer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_OH_Manufacturer)));
			this.ManufacturerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 196, true);
			this.ManufacturerGuidFindBox.Name = "ManufacturerGuidFindBox";
			this.ManufacturerGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ManufacturerGuidFindBox.ParentType = null;
			this.ManufacturerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.ManufacturerGuidFindBox.TabIndex = 5;
			// 
			// ExporterGuidFindBox
			// 
			this.ExporterGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExporterGuidFindBox, "JE_OH_Exporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_OH_Exporter)));
			this.ExporterGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 237, true);
			this.ExporterGuidFindBox.Name = "ExporterGuidFindBox";
			this.ExporterGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ExporterGuidFindBox.ParentType = null;
			this.ExporterGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.ExporterGuidFindBox.TabIndex = 6;
			// 
			// SupplierAddressControl
			// 
			this.SupplierAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierAddressControl, "JE_OA_SupplierAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_OA_SupplierAddress)));
			this.SupplierAddressControl.BindToOrgList = "Lookups.Organisations";
			this.SupplierAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 47, true);
			this.SupplierAddressControl.Name = "SupplierAddressControl";
			this.SupplierAddressControl.PopupCaption = "";
			this.SupplierAddressControl.ShowAddress = false;
			this.SupplierAddressControl.ShowOrganisationName = true;
			this.SupplierAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.SupplierAddressControl.TabIndex = 1;
			// 
			// ImporterAddressControl
			// 
			this.ImporterAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterAddressControl, "JE_OA_ImporterAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_OA_ImporterAddress)));
			this.ImporterAddressControl.BindToOrgList = "Lookups.Organisations";
			this.ImporterAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 12, true);
			this.ImporterAddressControl.Name = "ImporterAddressControl";
			this.ImporterAddressControl.PopupCaption = "";
			this.ImporterAddressControl.ShowAddress = false;
			this.ImporterAddressControl.ShowOrganisationName = true;
			this.ImporterAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.ImporterAddressControl.TabIndex = 0;
			// 
			// PayerGuidFindBox
			// 
			this.PayerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PayerGuidFindBox, "JE_OH_DutyPayer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_OH_DutyPayer)));
			this.PayerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 315, true);
			this.PayerGuidFindBox.Name = "PayerGuidFindBox";
			this.PayerGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PayerGuidFindBox.ParentType = null;
			this.PayerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.PayerGuidFindBox.TabIndex = 8;
			// 
			// AuthorGroupBox
			// 
			this.AuthorGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("75b9dba3-e5cd-4f0c-98c0-574cbef396b9", "Author");
			this.AuthorGroupBox.Controls.Add(this.AuthorPanel);
			this.AuthorGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 353, true);
			this.AuthorGroupBox.Name = "AuthorGroupBox";
			this.AuthorGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 112, true);
			this.AuthorGroupBox.TabIndex = 9;
			this.AuthorGroupBox.TabStop = false;
			// 
			// AuthorPanel
			// 
			this.AuthorPanel.AllowDrop = true;
			this.AuthorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AuthorPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AuthorPanel.Name = "AuthorPanel";
			this.AuthorPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 93, true);
			this.AuthorPanel.TabIndex = 0;
			// 
			// ResponsiblePersonGroupBox
			// 
			this.ResponsiblePersonGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("9544c464-4180-4d9e-b452-bf6220f491b0", "Responsible Person");
			this.ResponsiblePersonGroupBox.Controls.Add(this.ResponsiblePersonPanel);
			this.ResponsiblePersonGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 527, true);
			this.ResponsiblePersonGroupBox.Name = "ResponsiblePersonGroupBox";
			this.ResponsiblePersonGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 112, true);
			this.ResponsiblePersonGroupBox.TabIndex = 10;
			this.ResponsiblePersonGroupBox.TabStop = false;
			// 
			// ResponsiblePersonPanel
			// 
			this.ResponsiblePersonPanel.AllowDrop = true;
			this.ResponsiblePersonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResponsiblePersonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ResponsiblePersonPanel.Name = "ResponsiblePersonPanel";
			this.ResponsiblePersonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 93, true);
			this.ResponsiblePersonPanel.TabIndex = 1;
			// 
			// OrganisationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ResponsiblePersonGroupBox);
			this.Controls.Add(this.AuthorGroupBox);
			this.Controls.Add(this.PayerGuidFindBox);
			this.Controls.Add(this.SupplierAddressControl);
			this.Controls.Add(this.ImporterAddressControl);
			this.Controls.Add(this.ExporterGuidFindBox);
			this.Controls.Add(this.ManufacturerGuidFindBox);
			this.Controls.Add(this.StevedoreAddressControl);
			this.Controls.Add(this.FinalBondedWarehouseCodeFindBox);
			this.Controls.Add(this.ExpoterAddressControl);
			this.Controls.Add(this.IndustrialParkCodeCodeFindBox);
			this.Name = "OrganisationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(558, 806, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IndustrialParkCodeCodeFindBox.ResumeLayout(true);
			this.IndustrialParkCodeCodeFindBox.PerformLayout();
			this.ExpoterAddressControl.ResumeLayout(true);
			this.ExpoterAddressControl.PerformLayout();
			this.FinalBondedWarehouseCodeFindBox.ResumeLayout(true);
			this.FinalBondedWarehouseCodeFindBox.PerformLayout();
			this.StevedoreAddressControl.ResumeLayout(true);
			this.StevedoreAddressControl.PerformLayout();
			this.ManufacturerGuidFindBox.ResumeLayout(true);
			this.ManufacturerGuidFindBox.PerformLayout();
			this.ExporterGuidFindBox.ResumeLayout(true);
			this.ExporterGuidFindBox.PerformLayout();
			this.SupplierAddressControl.ResumeLayout(true);
			this.SupplierAddressControl.PerformLayout();
			this.ImporterAddressControl.ResumeLayout(true);
			this.ImporterAddressControl.PerformLayout();
			this.PayerGuidFindBox.ResumeLayout(true);
			this.PayerGuidFindBox.PerformLayout();
			this.AuthorGroupBox.ResumeLayout(false);
			this.AuthorGroupBox.PerformLayout();
			this.ResponsiblePersonGroupBox.ResumeLayout(false);
			this.ResponsiblePersonGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public ZArchitecture.GUI.ZCodeFindBox IndustrialParkCodeCodeFindBox;
		public ZArchitecture.GUI.ZAddressControl ExpoterAddressControl;
		public ZArchitecture.GUI.ZCodeFindBox FinalBondedWarehouseCodeFindBox;
		public ZArchitecture.GUI.ZAddressControl StevedoreAddressControl;
        public ZArchitecture.GUI.ZAddressControl SupplierAddressControl;
        public ZArchitecture.GUI.ZAddressControl ImporterAddressControl;
		public ZArchitecture.GUI.ZGuidFindBox ManufacturerGuidFindBox;
		public ZArchitecture.GUI.ZGuidFindBox ExporterGuidFindBox;
		public ZArchitecture.GUI.ZGuidFindBox PayerGuidFindBox;
		public ZArchitecture.GUI.ZGroupBox AuthorGroupBox;
		public ZArchitecture.GUI.ZGroupBox ResponsiblePersonGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel ResponsiblePersonPanel;
		internal ZArchitecture.GUI.DynamicLayoutPanel AuthorPanel;
	}
}
