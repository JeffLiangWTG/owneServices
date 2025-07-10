
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class SEDDetailsUserControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SEDDetailsUserControl));
			this.GoodsOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.COODeterminationRuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.COOIssueStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.COOLabelLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ManufacturerIPCCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ManufacturerUnipassIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ManufacturerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ManufacturerGuidFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.BuyerIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BuyerGuidFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.SupplierGuidFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.SupplierUnipassIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsOriginCodeFindBox.SuspendLayout();
			this.COODeterminationRuleDropEdit.SuspendLayout();
			this.COOIssueStatusDropEdit.SuspendLayout();
			this.COOLabelLocationDropEdit.SuspendLayout();
			this.ManufacturerIPCCodeFindBox.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.ManufacturerGuidFindBox.SuspendLayout();
			this.BuyerGuidFindBox.SuspendLayout();
			this.SupplierGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// GoodsOriginCodeFindBox
			// 
			this.GoodsOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginCodeFindBox, "Invoices.JZ_RN_NKDefaultOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_RN_NKDefaultOrigin)));
			this.GoodsOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 2, true);
			this.GoodsOriginCodeFindBox.Name = "GoodsOriginCodeFindBox";
			this.GoodsOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GoodsOriginCodeFindBox.ParentType = null;
			this.GoodsOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 19, true);
			this.GoodsOriginCodeFindBox.TabIndex = 16;
			// 
			// COODeterminationRuleDropEdit
			// 
			this.COODeterminationRuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.COODeterminationRuleDropEdit, "Invoices.CriteriaForDeterminingCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).CriteriaForDeterminingCountryOfOrigin)));
			this.COODeterminationRuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 50, true);
			this.COODeterminationRuleDropEdit.Name = "COODeterminationRuleDropEdit";
			this.COODeterminationRuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 19, true);
			this.COODeterminationRuleDropEdit.TabIndex = 18;
			// 
			// COOIssueStatusDropEdit
			// 
			this.COOIssueStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.COOIssueStatusDropEdit, "Invoices.CertificateOfOriginIssueStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).CertificateOfOriginIssueStatus)));
			this.COOIssueStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 26, true);
			this.COOIssueStatusDropEdit.Name = "COOIssueStatusDropEdit";
			this.COOIssueStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 19, true);
			this.COOIssueStatusDropEdit.TabIndex = 17;
			// 
			// COOLabelLocationDropEdit
			// 
			this.COOLabelLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.COOLabelLocationDropEdit, "Invoices.JZ_COOLabelLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_COOLabelLocation)));
			this.COOLabelLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 74, true);
			this.COOLabelLocationDropEdit.Name = "COOLabelLocationDropEdit";
			this.COOLabelLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 19, true);
			this.COOLabelLocationDropEdit.TabIndex = 19;
			// 
			// ManufacturerIPCCodeFindBox
			// 
			this.ManufacturerIPCCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerIPCCodeFindBox, "Invoices.ManufacturerIPCCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ManufacturerIPCCode)));
			this.ManufacturerIPCCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 166, true);
			this.ManufacturerIPCCodeFindBox.Name = "ManufacturerIPCCodeFindBox";
			this.ManufacturerIPCCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ManufacturerIPCCodeFindBox.ParentType = null;
			this.ManufacturerIPCCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 19, true);
			this.ManufacturerIPCCodeFindBox.TabIndex = 22;
			// 
			// ManufacturerUnipassIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.ManufacturerUnipassIDTextBox, "Invoices.ManufacturerUnipassID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).ManufacturerUnipassID)));
			this.ManufacturerUnipassIDTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("ManufacturerUnipassIDTextBox.CaptionResourceString")));
			this.ManufacturerUnipassIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 144, true);
			this.ManufacturerUnipassIDTextBox.Name = "ManufacturerUnipassIDTextBox";
			this.ManufacturerUnipassIDTextBox.ReadOnly = true;
			this.ManufacturerUnipassIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 19, true);
			this.ManufacturerUnipassIDTextBox.TabIndex = 21;
			// 
			// ManufacturerAddressControl
			// 
			this.ManufacturerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerAddressControl, "Invoices.JZ_OA_ManufacturerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OA_ManufacturerAddress)));
			this.ManufacturerAddressControl.BindToOrgList = "Lookups.Organisations";
			this.ManufacturerAddressControl.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("4652a942-1a58-422e-bcf4-93a749ff09e5", "Code");
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 97, true);
			this.ManufacturerAddressControl.Name = "ManufacturerAddressControl";
			this.ManufacturerAddressControl.PopupCaption = "";
			this.ManufacturerAddressControl.ShowAddress = false;
			this.ManufacturerAddressControl.ShowOrganisationName = true;
			this.ManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 19, true);
			this.ManufacturerAddressControl.TabIndex = 20;
			// 
			// ManufacturerGuidFindBox
			// 
			this.ManufacturerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerGuidFindBox, "Invoices.JZ_OH_Manufacturer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OH_Manufacturer)));
			this.ManufacturerGuidFindBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("52fa382c-e363-4ab0-97b1-e2c813afb4fd", "Code");
			this.ManufacturerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 121, true);
			this.ManufacturerGuidFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.ManufacturerGuidFindBox.Name = "ManufacturerGuidFindBox";
			this.ManufacturerGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ManufacturerGuidFindBox.ParentType = null;
			this.ManufacturerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 19, true);
			this.ManufacturerGuidFindBox.TabIndex = 23;
			// 
			// BuyerIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.BuyerIDTextBox, "Invoices.BuyerID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).BuyerID)));
			this.BuyerIDTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("BuyerIDTextBox.CaptionResourceString")));
			this.BuyerIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 211, true);
			this.BuyerIDTextBox.Name = "BuyerIDTextBox";
			this.BuyerIDTextBox.ReadOnly = true;
			this.BuyerIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 19, true);
			this.BuyerIDTextBox.TabIndex = 25;
			// 
			// BuyerGuidFindBox
			// 
			this.BuyerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerGuidFindBox, "Invoices.JZ_OH_Buyer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OH_Buyer)));
			this.BuyerGuidFindBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("52fa382c-e363-4ab0-97b1-e2c813afb4fd", "Code");
			this.BuyerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 189, true);
			this.BuyerGuidFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.BuyerGuidFindBox.Name = "BuyerGuidFindBox";
			this.BuyerGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BuyerGuidFindBox.ParentType = null;
			this.BuyerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 19, true);
			this.BuyerGuidFindBox.TabIndex = 24;
			// 
			// SupplierGuidFindBox
			// 
			this.SupplierGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierGuidFindBox, "Invoices.JZ_OH_Supplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OH_Supplier)));
			this.SupplierGuidFindBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("52fa382c-e363-4ab0-97b1-e2c813afb4fd", "Code");
			this.SupplierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 233, true);
			this.SupplierGuidFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.SupplierGuidFindBox.Name = "SupplierGuidFindBox";
			this.SupplierGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SupplierGuidFindBox.ParentType = null;
			this.SupplierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 19, true);
			this.SupplierGuidFindBox.TabIndex = 27;
			// 
			// SupplierUnipassIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.SupplierUnipassIDTextBox, "Invoices.SupplierUnipassID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).SupplierUnipassID)));
			this.SupplierUnipassIDTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("SupplierUnipassIDTextBox.CaptionResourceString")));
			this.SupplierUnipassIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 257, true);
			this.SupplierUnipassIDTextBox.Name = "SupplierUnipassIDTextBox";
			this.SupplierUnipassIDTextBox.ReadOnly = true;
			this.SupplierUnipassIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 19, true);
			this.SupplierUnipassIDTextBox.TabIndex = 26;
			// 
			// SEDDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupplierGuidFindBox);
			this.Controls.Add(this.SupplierUnipassIDTextBox);
			this.Controls.Add(this.BuyerIDTextBox);
			this.Controls.Add(this.BuyerGuidFindBox);
			this.Controls.Add(this.ManufacturerGuidFindBox);
			this.Controls.Add(this.ManufacturerIPCCodeFindBox);
			this.Controls.Add(this.ManufacturerUnipassIDTextBox);
			this.Controls.Add(this.ManufacturerAddressControl);
			this.Controls.Add(this.GoodsOriginCodeFindBox);
			this.Controls.Add(this.COODeterminationRuleDropEdit);
			this.Controls.Add(this.COOIssueStatusDropEdit);
			this.Controls.Add(this.COOLabelLocationDropEdit);
			this.Name = "SEDDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 285, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsOriginCodeFindBox.ResumeLayout(true);
			this.GoodsOriginCodeFindBox.PerformLayout();
			this.COODeterminationRuleDropEdit.ResumeLayout(true);
			this.COODeterminationRuleDropEdit.PerformLayout();
			this.COOIssueStatusDropEdit.ResumeLayout(true);
			this.COOIssueStatusDropEdit.PerformLayout();
			this.COOLabelLocationDropEdit.ResumeLayout(true);
			this.COOLabelLocationDropEdit.PerformLayout();
			this.ManufacturerIPCCodeFindBox.ResumeLayout(true);
			this.ManufacturerIPCCodeFindBox.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.ManufacturerGuidFindBox.ResumeLayout(true);
			this.ManufacturerGuidFindBox.PerformLayout();
			this.BuyerGuidFindBox.ResumeLayout(true);
			this.BuyerGuidFindBox.PerformLayout();
			this.SupplierGuidFindBox.ResumeLayout(true);
			this.SupplierGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZCodeFindBox GoodsOriginCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit COODeterminationRuleDropEdit;
		internal ZArchitecture.GUI.ZDropEdit COOIssueStatusDropEdit;
		internal ZArchitecture.GUI.ZDropEdit COOLabelLocationDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox ManufacturerIPCCodeFindBox;
		internal ZArchitecture.ZTextBox ManufacturerUnipassIDTextBox;
		internal ZArchitecture.GUI.ZAddressControl ManufacturerAddressControl;
		internal ZOrganisationFindBox ManufacturerGuidFindBox;
		internal ZArchitecture.ZTextBox BuyerIDTextBox;
		internal ZOrganisationFindBox BuyerGuidFindBox;
		internal ZOrganisationFindBox SupplierGuidFindBox;
		internal ZArchitecture.ZTextBox SupplierUnipassIDTextBox;
	}
}
