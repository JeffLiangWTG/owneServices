using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class ClassAInvoiceForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.TransactionNumberTextBox = new ZArchitecture.ZTextBox();
			this.TransactionTypeTextBox = new ZArchitecture.ZTextBox();
			this.OrganizationTextBox = new ZArchitecture.ZTextBox();
			this.InvoiceDateDateEdit = new ZDateEdit();
			this.PostDateDateEdit = new ZDateEdit();
			this.ClassAInvoiceNumTextBox = new ZArchitecture.ZTextBox();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.ComplianceSubTypeDropEdit = new ZDropEdit();
			this.ComplianceDocDateEdit = new ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceDateDateEdit.SuspendLayout();
			this.PostDateDateEdit.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.ComplianceSubTypeDropEdit.SuspendLayout();
			this.ComplianceDocDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 258, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 7;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(379);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GovernmentInvoice);
			// 
			// TransactionNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransactionNumberTextBox, "GovtTaxInvoiceDisplay_TransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GovernmentInvoice)(null)).GovtTaxInvoiceDisplay_TransactionNum)));
			this.TransactionNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClassAInvoiceForm|2bdfedac-0c73-445a-8619-4c205bc16157", "Transaction Number", "Transaction Number", "");
			this.TransactionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 40, true);
			this.TransactionNumberTextBox.Name = "TransactionNumberTextBox";
			this.TransactionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
			this.TransactionNumberTextBox.TabIndex = 1;
			// 
			// TransactionTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransactionTypeTextBox, "GovtTaxInvoiceDisplay_TransactionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GovernmentInvoice)(null)).GovtTaxInvoiceDisplay_TransactionType)));
			this.TransactionTypeTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClassAInvoiceForm|a0a43f7e-8120-4052-9760-911ed0c07317", "Transaction Type", "Transaction Type", "");
			this.TransactionTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 16, true);
			this.TransactionTypeTextBox.Name = "TransactionTypeTextBox";
			this.TransactionTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
			this.TransactionTypeTextBox.TabIndex = 0;
			// 
			// OrganizationTextBox
			// 
			this.BindingSource.SetBindingMember(this.OrganizationTextBox, "GovtTaxInvoiceDisplay_OrganisationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GovernmentInvoice)(null)).GovtTaxInvoiceDisplay_OrganisationCode)));
			this.OrganizationTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClassAInvoiceForm|f4f68b2d-efe5-4ef9-ade4-5fc62ddf44b9", "Organization", "Organization", "");
			this.OrganizationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 112, true);
			this.OrganizationTextBox.Name = "OrganizationTextBox";
			this.OrganizationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
			this.OrganizationTextBox.TabIndex = 4;
			// 
			// InvoiceDateDateEdit
			// 
			this.InvoiceDateDateEdit.AllowDrop = true;
			this.InvoiceDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.InvoiceDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InvoiceDateDateEdit, "GovtTaxInvoiceDisplay_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((GovernmentInvoice)(null)).GovtTaxInvoiceDisplay_InvoiceDate)));
			this.InvoiceDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClassAInvoiceForm|0248e8d3-2345-4919-8b39-1011690e1f05", "Invoice Date");
			this.InvoiceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 64, true);
			this.InvoiceDateDateEdit.Name = "InvoiceDateDateEdit";
			this.InvoiceDateDateEdit.TabIndex = 2;
			// 
			// PostDateDateEdit
			// 
			this.PostDateDateEdit.AllowDrop = true;
			this.PostDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateDateEdit, "GovtTaxInvoiceDisplay_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((GovernmentInvoice)(null)).GovtTaxInvoiceDisplay_PostDate)));
			this.PostDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClassAInvoiceForm|e83c8839-29f6-4df9-ad55-0e92b49fabed", "Post Date", "Post Date", "");
			this.PostDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 88, true);
			this.PostDateDateEdit.Name = "PostDateDateEdit";
			this.PostDateDateEdit.TabIndex = 3;
			// 
			// ClassAInvoiceNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClassAInvoiceNumTextBox, "AH_TransactionReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GovernmentInvoice)(null)).AH_TransactionReference)));
			this.ClassAInvoiceNumTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClassAInvoiceForm|58667AE7-D9AC-4968-96CD-47F146217241", "Compliance Number", "Compliance Number", "");
			this.ClassAInvoiceNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 144, true);
			this.ClassAInvoiceNumTextBox.Name = "ClassAInvoiceNumTextBox";
			this.ClassAInvoiceNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 18, true);
			this.ClassAInvoiceNumTextBox.TabIndex = 5;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 220, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 6;
			// 
			// ComplianceSubTypeDropEdit
			// 
			this.ComplianceSubTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComplianceSubTypeDropEdit, "AH_ComplianceSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((GovernmentInvoice)(null)).AH_ComplianceSubType)));
			this.ComplianceSubTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClassAInvoiceForm|8C2C6F0E-D46C-4165-BDBF-3B244D2D0E1E", "Compliance Sub Type", "Compliance Sub Type", "");
			this.ComplianceSubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 170, true);
			this.ComplianceSubTypeDropEdit.Name = "ComplianceSubTypeDropEdit";
			this.ComplianceSubTypeDropEdit.PreBoundMaxLength = 3;
			this.ComplianceSubTypeDropEdit.ShowDescriptionBox = false;
			this.ComplianceSubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 18, true);
			this.ComplianceSubTypeDropEdit.TabIndex = 13;
			// 
			// ComplianceDocDateEdit
			// 
			this.ComplianceDocDateEdit.AllowDrop = true;
			this.ComplianceDocDateEdit.AutoCompleteMonthThreshold = 1;
			this.ComplianceDocDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ComplianceDocDateEdit, "AH_ComplianceDocumentDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((GovernmentInvoice)(null)).AH_ComplianceDocumentDate)));
			this.ComplianceDocDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClassAInvoiceForm|65eaf799-d19e-4e81-a45a-fd1d8f5cdb5e", "Compliance Doc Date", "Compliance Doc Date", "");
			this.ComplianceDocDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 198, true);
			this.ComplianceDocDateEdit.Name = "ComplianceDocDateEdit";
			this.ComplianceDocDateEdit.TabIndex = 14;
			// 
			// ClassAInvoiceForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ClassAInvoiceForm|AAF99143-5D30-47DE-AB21-0A5C71364D7C", "Compliance Sub Type / Number");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 282, true);
			this.Controls.Add(this.ComplianceDocDateEdit);
			this.Controls.Add(this.ComplianceSubTypeDropEdit);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.ClassAInvoiceNumTextBox);
			this.Controls.Add(this.PostDateDateEdit);
			this.Controls.Add(this.InvoiceDateDateEdit);
			this.Controls.Add(this.OrganizationTextBox);
			this.Controls.Add(this.TransactionTypeTextBox);
			this.Controls.Add(this.TransactionNumberTextBox);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(GovernmentInvoice);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.GovernmentInvoice";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 278, true);
			this.Name = "ClassAInvoiceForm";
			this.RememberFormSize = false;
			this.Controls.SetChildIndex(this.TransactionNumberTextBox, 0);
			this.Controls.SetChildIndex(this.TransactionTypeTextBox, 0);
			this.Controls.SetChildIndex(this.OrganizationTextBox, 0);
			this.Controls.SetChildIndex(this.InvoiceDateDateEdit, 0);
			this.Controls.SetChildIndex(this.PostDateDateEdit, 0);
			this.Controls.SetChildIndex(this.ClassAInvoiceNumTextBox, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.ComplianceSubTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.ComplianceDocDateEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceDateDateEdit.ResumeLayout(true);
			this.InvoiceDateDateEdit.PerformLayout();
			this.PostDateDateEdit.ResumeLayout(true);
			this.PostDateDateEdit.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ComplianceSubTypeDropEdit.ResumeLayout(true);
			this.ComplianceSubTypeDropEdit.PerformLayout();
			this.ComplianceDocDateEdit.ResumeLayout(true);
			this.ComplianceDocDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}