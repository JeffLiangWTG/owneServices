namespace Enterprise.Customs.KR.GUI
{
	partial class ValuationDeclarationTemplateDetailsUserControl
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
			this.ValuationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DepartmentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PONoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PODateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ServiceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ValuationCodeDropEdit.SuspendLayout();
			this.CustomsOfficeCodeFindBox.SuspendLayout();
			this.DepartmentCodeFindBox.SuspendLayout();
			this.PODateEdit.SuspendLayout();
			this.ServiceCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// ValuationCodeDropEdit
			// 
			this.ValuationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationCodeDropEdit, "Invoices.JZ_ValuationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ValuationCode)));
			this.ValuationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 8, true);
			this.ValuationCodeDropEdit.Name = "ValuationCodeDropEdit";
			this.ValuationCodeDropEdit.PreBoundMaxLength = 2;
			this.ValuationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ValuationCodeDropEdit.TabIndex = 0;
			// 
			// CustomsOfficeCodeFindBox
			// 
			this.CustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "JE_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_CustomsOffice)));
			this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 29, true);
			this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
			this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsOfficeCodeFindBox.ParentType = null;
			this.CustomsOfficeCodeFindBox.PreBoundMaxLength = 3;
			this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CustomsOfficeCodeFindBox.TabIndex = 1;
			// 
			// DepartmentCodeFindBox
			// 
			this.DepartmentCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartmentCodeFindBox, "JE_CustomsDivision");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_CustomsDivision)));
			this.DepartmentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 51, true);
			this.DepartmentCodeFindBox.Name = "DepartmentCodeFindBox";
			this.DepartmentCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DepartmentCodeFindBox.ParentType = null;
			this.DepartmentCodeFindBox.PreBoundMaxLength = 2;
			this.DepartmentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.DepartmentCodeFindBox.TabIndex = 2;
			// 
			// PONoTextBox
			// 
			this.BindingSource.SetBindingMember(this.PONoTextBox, "Invoices.PurchaseOrderNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).PurchaseOrderNumber)));
			this.PONoTextBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("48f6ff5b-1841-4810-87b6-179267465fed", "P/O No.");
			this.PONoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 77, true);
			this.PONoTextBox.Name = "PONoTextBox";
			this.PONoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.PONoTextBox.TabIndex = 5;
			// 
			// PODateEdit
			// 
			this.PODateEdit.AllowDrop = true;
			this.PODateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PODateEdit, "Invoices.PurchaseOrderDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).PurchaseOrderDate)));
			this.PODateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 103, true);
			this.PODateEdit.Name = "PODateEdit";
			this.PODateEdit.TabIndex = 6;
			// 
			// ServiceCodeFindBox
			// 
			this.ServiceCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceCodeFindBox, "JE_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_RS_NKServiceLevel)));
			this.ServiceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 129, true);
			this.ServiceCodeFindBox.Name = "ServiceCodeFindBox";
			this.ServiceCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ServiceCodeFindBox.ParentType = null;
			this.ServiceCodeFindBox.PreBoundMaxLength = 3;
			this.ServiceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ServiceCodeFindBox.TabIndex = 7;
			// 
			// ValuationDeclarationTemplateDetailsUserControl
			// 
			this.Controls.Add(this.ServiceCodeFindBox);
			this.Controls.Add(this.PODateEdit);
			this.Controls.Add(this.PONoTextBox);
			this.Controls.Add(this.DepartmentCodeFindBox);
			this.Controls.Add(this.CustomsOfficeCodeFindBox);
			this.Controls.Add(this.ValuationCodeDropEdit);
			this.Name = "ValuationDeclarationTemplateDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 153, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ValuationCodeDropEdit.ResumeLayout(true);
			this.ValuationCodeDropEdit.PerformLayout();
			this.CustomsOfficeCodeFindBox.ResumeLayout(true);
			this.CustomsOfficeCodeFindBox.PerformLayout();
			this.DepartmentCodeFindBox.ResumeLayout(true);
			this.DepartmentCodeFindBox.PerformLayout();
			this.PODateEdit.ResumeLayout(true);
			this.PODateEdit.PerformLayout();
			this.ServiceCodeFindBox.ResumeLayout(true);
			this.ServiceCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit ValuationCodeDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox DepartmentCodeFindBox;
		private ZArchitecture.ZTextBox PONoTextBox;
		private ZArchitecture.GUI.ZDateEdit PODateEdit;
		private ZArchitecture.GUI.ZCodeFindBox ServiceCodeFindBox;
	}
}
