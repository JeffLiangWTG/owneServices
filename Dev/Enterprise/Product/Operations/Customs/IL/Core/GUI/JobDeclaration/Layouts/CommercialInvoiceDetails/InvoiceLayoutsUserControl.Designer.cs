using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.GUI
{
	partial class InvoiceLayoutsUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.InvoiceDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SequenceTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IncoTermsWithCountryCodeUserControl = new Enterprise.Customs.IL.GUI.IncoTermsWithCountryCodeUserControl();
			this.SupplierCodeFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.PreferenceAgreementDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentTermsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceDateDateEdit.SuspendLayout();
			this.IncoTermsWithCountryCodeUserControl.SuspendLayout();
			this.SupplierCodeFindBox.SuspendLayout();
			this.PreferenceAgreementDropEdit.SuspendLayout();
			this.PaymentTermsDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Business.JobComInvoiceHeader);
			// 
			// InvoiceDateDateEdit
			// 
			this.InvoiceDateDateEdit.AllowDrop = true;
			this.InvoiceDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.InvoiceDateDateEdit, "JZ_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Business.JobComInvoiceHeader)(null)).JZ_InvoiceDate)));
			this.InvoiceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceDateDateEdit.Name = "InvoiceDateDateEdit";
			this.InvoiceDateDateEdit.TabIndex = 0;
			// 
			// SequenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.SequenceTextBox, "JZ_InvoiceDisplaySequence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IL.Business.JobComInvoiceHeader)(null)).JZ_InvoiceDisplaySequence)));
			this.SequenceTextBox.DecimalPlaces = 2;
			this.SequenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 21, true);
			this.SequenceTextBox.Name = "SequenceTextBox";
			this.SequenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.SequenceTextBox.TabIndex = 1;
			this.SequenceTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.SequenceTextBox.TrackDisposedAccess = true;
			// 
			// IncoTermsWithCountryCodeUserControl
			// 
			this.IncoTermsWithCountryCodeUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncoTermsWithCountryCodeUserControl, ".");
			this.IncoTermsWithCountryCodeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 75, true);
			this.IncoTermsWithCountryCodeUserControl.Name = "IncoTermsWithCountryCodeUserControl";
			this.IncoTermsWithCountryCodeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 22, true);
			this.IncoTermsWithCountryCodeUserControl.TabIndex = 2;
			// 
			// SupplierCodeFindBox
			// 
			this.SupplierCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierCodeFindBox, "JZ_OH_Supplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IL.Business.JobComInvoiceHeader)(null)).JZ_OH_Supplier)));
			this.SupplierCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 38, true);
			this.SupplierCodeFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.SupplierCodeFindBox.Name = "SupplierCodeFindBox";
			this.SupplierCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SupplierCodeFindBox.ParentType = null;
			this.SupplierCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.SupplierCodeFindBox.TabIndex = 2;
			// 
			// PreferenceAgreementDropEdit
			// 
			this.PreferenceAgreementDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreferenceAgreementDropEdit, "JZ_PreferenceDocumentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Business.JobComInvoiceHeader)(null)).JZ_PreferenceDocumentType)));
			this.PreferenceAgreementDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 56, true);
			this.PreferenceAgreementDropEdit.Name = "PreferenceAgreementDropEdit";
			this.PreferenceAgreementDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.PreferenceAgreementDropEdit.TabIndex = 3;
			// 
			// PaymentTermsDropEdit
			// 
			this.PaymentTermsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTermsDropEdit, "JZ_PaymentTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Business.JobComInvoiceHeader)(null)).JZ_PaymentTerms)));
			this.PaymentTermsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 96, true);
			this.PaymentTermsDropEdit.Name = "PaymentTermsDropEdit";
			this.PaymentTermsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.PaymentTermsDropEdit.TabIndex = 4;
			// 
			// InvoiceLayoutsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoiceDateDateEdit);
			this.Controls.Add(this.SequenceTextBox);
			this.Controls.Add(this.IncoTermsWithCountryCodeUserControl);
			this.Controls.Add(this.SupplierCodeFindBox);
			this.Controls.Add(this.PreferenceAgreementDropEdit);
			this.Controls.Add(this.PaymentTermsDropEdit);
			this.Name = "InvoiceLayoutsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 120, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceDateDateEdit.ResumeLayout(true);
			this.InvoiceDateDateEdit.PerformLayout();
			this.IncoTermsWithCountryCodeUserControl.ResumeLayout(true);
			this.IncoTermsWithCountryCodeUserControl.PerformLayout();
			this.SupplierCodeFindBox.ResumeLayout(true);
			this.SupplierCodeFindBox.PerformLayout();
			this.PreferenceAgreementDropEdit.ResumeLayout(true);
			this.PreferenceAgreementDropEdit.PerformLayout();
			this.PaymentTermsDropEdit.ResumeLayout(true);
			this.PaymentTermsDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZDateEdit InvoiceDateDateEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit SequenceTextBox;
		internal Enterprise.MasterFiles.GUI.ZOrganisationFindBox SupplierCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit PreferenceAgreementDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit PaymentTermsDropEdit;
		internal IncoTermsWithCountryCodeUserControl IncoTermsWithCountryCodeUserControl;
	}
}
