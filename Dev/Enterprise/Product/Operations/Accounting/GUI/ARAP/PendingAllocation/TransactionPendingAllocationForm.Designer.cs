using System.ComponentModel;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	partial class TransactionPendingAllocationForm
	{
		IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.TransactionDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.sourceXmlTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.workflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.NotesTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.postingButtonsUserControl.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 486, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 21, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation);
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.postingButtonsUserControl);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 452, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 34, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(625, 6, true);
			this.postingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.TabIndex = 0;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.TransactionDetailsTabPage);
			this.MainTabControl.Controls.Add(this.sourceXmlTabPage);
			this.MainTabControl.Controls.Add(this.workflowTabPage);
			this.MainTabControl.Controls.Add(this.NotesTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 452, true);
			this.MainTabControl.TabIndex = 0;
			this.workflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.workflowTabPage_InitializeTab));

			// NotesTabPage
			//
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 11, true);
			this.NotesTabPage.Name = "NotesTabPage";
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 416, true);
			this.NotesTabPage.TabIndex = 1;
			this.NotesTabPage.ReadOnly = true;
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));

			// 
			// TransactionDetailsTabPage
			// 
			this.TransactionDetailsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionPendingAllocationForm|3c013408-5081-4eb4-8f20-5fa3e529e64d", "Details");
			this.TransactionDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.TransactionDetailsTabPage.Name = "TransactionDetailsTabPage";
			this.TransactionDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 429, true);
			this.TransactionDetailsTabPage.TabIndex = 0;
			this.TransactionDetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.TransactionDetailsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).DisplayInvoiceContactOverride)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).DisplayInvoiceAddressOverride)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).AH_NumberOfSupportingDocuments)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).AH_InvoiceDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).AH_GB)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).AH_GE)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).AH_OH)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZExchangeRate)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).ExchangeRate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).AH_DueDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).AH_TransactionNum)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).AH_PostDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).AH_Desc)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).AH_PlaceOfSupply)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).AH_DocumentReceivedDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).AH_GB_TaxBranch)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).AH_GovernmentAllocatedID)));
			// 
			// sourceXmlTabPage
			// 
			this.sourceXmlTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1c5439f5-b687-4ef3-87f3-f1d1b1892b56", "Imported XML");
			this.sourceXmlTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.sourceXmlTabPage.Name = "sourceXmlTabPage";
			this.sourceXmlTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.sourceXmlTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 429, true);
			this.sourceXmlTabPage.TabIndex = 2;
			this.sourceXmlTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.sourceXmlTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(null)).TransactionApprovalRequest.PostingDetails.UniversalTransaction)));
			// 
			// workflowTabPage
			// 
			this.workflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.workflowTabPage.Name = "workflowTabPage";
			this.workflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 450, true);
			this.workflowTabPage.TabIndex = 1;
			// 
			// TransactionPendingAllocationForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionPendingAllocationForm|b66088c1-bd5d-4553-af5d-53537896479b", "Unallocated Transaction");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 506, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 470, true);
			this.Name = "TransactionPendingAllocationForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.postingButtonsUserControl.ResumeLayout(true);
			this.postingButtonsUserControl.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void TransactionDetailsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.OverrideContactDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.OverrideAddressGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.AH_NumberOfSupportingDocumentsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AH_InvoiceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BranchFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TaxBranchFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.departmentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.creditorOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.ExchangeRateControl = new Enterprise.ZArchitecture.GUI.ZExchangeRateControl();
			this.taxAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.localTaxAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.localExTaxAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.exTaxAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.AH_DueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AH_TransactionNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AH_PostDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AH_DescTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.PlaceOfSupplyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AH_DocReceivedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AH_GovernmentAllocatedIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransactionDetailsTabPage.SuspendLayout();
			this.OverrideContactDropEdit.SuspendLayout();
			this.OverrideAddressGuidDropEdit.SuspendLayout();
			this.AH_InvoiceDateEdit.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.TaxBranchFindBox.SuspendLayout();
			this.departmentGuidFindBox.SuspendLayout();
			this.creditorOrganisationFindBox.SuspendLayout();
			this.ExchangeRateControl.SuspendLayout();
			this.taxAmountCalcFindBox.SuspendLayout();
			this.localTaxAmountCalcFindBox.SuspendLayout();
			this.localExTaxAmountCalcFindBox.SuspendLayout();
			this.exTaxAmountCalcFindBox.SuspendLayout();
			this.AH_DueDateEdit.SuspendLayout();
			this.AH_PostDateEdit.SuspendLayout();
			this.PlaceOfSupplyDropEdit.SuspendLayout();
			this.AH_DocReceivedDateEdit.SuspendLayout();
			this.TransactionDetailsTabPage.Controls.Add(this.AH_GovernmentAllocatedIDTextBox);
			this.TransactionDetailsTabPage.Controls.Add(this.AH_DocReceivedDateEdit);
			this.TransactionDetailsTabPage.Controls.Add(this.PlaceOfSupplyDropEdit);
			this.TransactionDetailsTabPage.Controls.Add(this.OverrideContactDropEdit);
			this.TransactionDetailsTabPage.Controls.Add(this.OverrideAddressGuidDropEdit);
			this.TransactionDetailsTabPage.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.TransactionDetailsTabPage.Controls.Add(this.AH_InvoiceDateEdit);
			this.TransactionDetailsTabPage.Controls.Add(this.BranchFindBox);
			this.TransactionDetailsTabPage.Controls.Add(this.TaxBranchFindBox);
			this.TransactionDetailsTabPage.Controls.Add(this.departmentGuidFindBox);
			this.TransactionDetailsTabPage.Controls.Add(this.creditorOrganisationFindBox);
			this.TransactionDetailsTabPage.Controls.Add(this.ExchangeRateControl);
			this.TransactionDetailsTabPage.Controls.Add(this.taxAmountCalcFindBox);
			this.TransactionDetailsTabPage.Controls.Add(this.localTaxAmountCalcFindBox);
			this.TransactionDetailsTabPage.Controls.Add(this.localExTaxAmountCalcFindBox);
			this.TransactionDetailsTabPage.Controls.Add(this.exTaxAmountCalcFindBox);
			this.TransactionDetailsTabPage.Controls.Add(this.AH_DueDateEdit);
			this.TransactionDetailsTabPage.Controls.Add(this.AH_TransactionNumTextBox);
			this.TransactionDetailsTabPage.Controls.Add(this.AH_PostDateEdit);
			this.TransactionDetailsTabPage.Controls.Add(this.AH_DescTextbox);
			// 
			// OverrideContactDropEdit
			// 
			this.OverrideContactDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OverrideContactDropEdit, "DisplayInvoiceContactOverride");
			this.OverrideContactDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b095d1cf-15d2-4b0f-80bd-2504043c6961", "Contact");
			this.OverrideContactDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 259, true);
			this.OverrideContactDropEdit.Name = "OverrideContactDropEdit";
			this.OverrideContactDropEdit.ShouldResizeByMaxLength = true;
			this.OverrideContactDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 17, true);
			this.OverrideContactDropEdit.TabIndex = 16;
			// 
			// OverrideAddressGuidDropEdit
			// 
			this.OverrideAddressGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OverrideAddressGuidDropEdit, "DisplayInvoiceAddressOverride");
			this.OverrideAddressGuidDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3efac85d-a7ad-47b9-ab17-2eb054cb1432", "Address");
			this.OverrideAddressGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 236, true);
			this.OverrideAddressGuidDropEdit.Name = "OverrideAddressGuidDropEdit";
			this.OverrideAddressGuidDropEdit.ShouldResizeByMaxLength = true;
			this.OverrideAddressGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 17, true);
			this.OverrideAddressGuidDropEdit.TabIndex = 15;
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_NumberOfSupportingDocumentsCalcEdit, "AH_NumberOfSupportingDocuments");
			this.AH_NumberOfSupportingDocumentsCalcEdit.CaptionResourceString = null;
			this.AH_NumberOfSupportingDocumentsCalcEdit.DecimalPlaces = 2;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 29, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 17, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 4;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AH_InvoiceDateEdit
			// 
			this.AH_InvoiceDateEdit.AllowDrop = true;
			this.AH_InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_InvoiceDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_InvoiceDateEdit, "AH_InvoiceDate");
			this.AH_InvoiceDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionPendingAllocationForm|c6d7dd07-cf12-4d31-96a6-ed12460131fa", "Transaction Date", "Transaction Date", "");
			this.AH_InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 6, true);
			this.AH_InvoiceDateEdit.Name = "AH_InvoiceDateEdit";
			this.AH_InvoiceDateEdit.TabIndex = 0;
			// 
			// AH_DocReceivedDateEdit
			// 
			this.AH_DocReceivedDateEdit.AllowDrop = true;
			this.AH_DocReceivedDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_DocReceivedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_DocReceivedDateEdit, "AH_DocumentReceivedDate");
			this.AH_DocReceivedDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionPendingAllocationForm|9E6CFFE7-158E-490D-A7FB-17219C5B0F12", "Doc Rec Date", "Document Received Date");
			this.AH_DocReceivedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(265, 6, true);
			this.AH_DocReceivedDateEdit.Name = "zDateEdit1";
			this.AH_DocReceivedDateEdit.TabIndex = 1;
			// 
			// BranchFindBox
			// 
			this.BranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchFindBox, "AH_GB");
			this.BranchFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 190, true);
			this.BranchFindBox.Name = "BranchFindBox";
			this.BranchFindBox.ShouldResize = true;
			this.BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 17, true);
			this.BranchFindBox.TabIndex = 13;
			// 
			// TaxBranchFindBox
			// 
			this.TaxBranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxBranchFindBox, "AH_GB_TaxBranch");
			this.TaxBranchFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.TaxBranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 190, true);
			this.TaxBranchFindBox.Name = "TaxBranchFindBox";
			this.TaxBranchFindBox.ShouldResize = true;
			this.TaxBranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 17, true);
			this.TaxBranchFindBox.TabIndex = 13;
			// 
			// departmentGuidFindBox
			// 
			this.departmentGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.departmentGuidFindBox, "AH_GE");
			this.departmentGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.departmentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 213, true);
			this.departmentGuidFindBox.Name = "departmentGuidFindBox";
			this.departmentGuidFindBox.ShouldResize = true;
			this.departmentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 17, true);
			this.departmentGuidFindBox.TabIndex = 14;
			// 
			// creditorOrganisationFindBox
			// 
			this.creditorOrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.creditorOrganisationFindBox, "AH_OH");
			this.creditorOrganisationFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionPendingAllocationForm|8c184636-c4f0-4a87-861e-74aec177b349", "Creditor", "Creditor", "Creditor", "");
			this.creditorOrganisationFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.creditorOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 52, true);
			this.creditorOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.creditorOrganisationFindBox.Name = "creditorOrganisationFindBox";
			this.creditorOrganisationFindBox.ShouldResize = true;
			this.creditorOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(451, 17, true);
			this.creditorOrganisationFindBox.TabIndex = 5;
			// 
			// ExchangeRateControl
			// 
			this.ExchangeRateControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExchangeRateControl, "ExchangeRate");
			this.ExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionPendingAllocationForm|62e41f20-2cdd-4463-84f4-cd1af0bb5036", "Currency");
			this.ExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 98, true);
			this.ExchangeRateControl.Name = "ExchangeRateControl";
			this.ExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.ExchangeRateControl.TabIndex = 7;
			// 
			// taxAmountCalcFindBox
			// 
			this.taxAmountCalcFindBox.AllowDrop = true;
			this.taxAmountCalcFindBox.BindToAmount = "AH_OSTaxAmount";
			this.taxAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.taxAmountCalcFindBox.BindToUnit = "AH_Readonly_RXCode";
			this.taxAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionPendingAllocationForm|7e403c7e-2057-44de-a6af-6188f2f8e151", "Tax Amount");
			this.taxAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.taxAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 144, true);
			this.taxAmountCalcFindBox.Name = "taxAmountCalcFindBox";
			this.taxAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.taxAmountCalcFindBox.TabIndex = 10;
			// 
			// localTaxAmountCalcFindBox
			// 
			this.localTaxAmountCalcFindBox.AllowDrop = true;
			this.localTaxAmountCalcFindBox.BindToAmount = "AH_LocalTaxAmount";
			this.localTaxAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.localTaxAmountCalcFindBox.BindToUnit = "AH_Calc_LocalRX";
			this.localTaxAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionPendingAllocationForm|caf2c21b-87a1-45e6-8d2b-4a4177b23d99", "Local Tax Amount", "Local Tax Amount", "Local Tax Amount", "");
			this.localTaxAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 144, true);
			this.localTaxAmountCalcFindBox.Name = "localTaxAmountCalcFindBox";
			this.localTaxAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.localTaxAmountCalcFindBox.TabIndex = 11;
			// 
			// localExTaxAmountCalcFindBox
			// 
			this.localExTaxAmountCalcFindBox.AllowDrop = true;
			this.localExTaxAmountCalcFindBox.BindToAmount = "AH_LocalExTaxAmount";
			this.localExTaxAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.localExTaxAmountCalcFindBox.BindToUnit = "AH_Calc_LocalRX";
			this.localExTaxAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionPendingAllocationForm|92fdf1d8-16fd-4eee-b437-13779a3c6142", "Local Amount Excl. Tax", "Local Amount Excl. Tax", "Local Amount Excluding Tax.");
			this.localExTaxAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 121, true);
			this.localExTaxAmountCalcFindBox.Name = "localExTaxAmountCalcFindBox";
			this.localExTaxAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.localExTaxAmountCalcFindBox.TabIndex = 9;
			// 
			// exTaxAmountCalcFindBox
			// 
			this.exTaxAmountCalcFindBox.AllowDrop = true;
			this.exTaxAmountCalcFindBox.BindToAmount = "AH_OSExTaxAmount";
			this.exTaxAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.exTaxAmountCalcFindBox.BindToUnit = "AH_Readonly_RXCode";
			this.exTaxAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionPendingAllocationForm|e0cb7b8b-a553-4aab-ba61-bb9f662c713f", "Amount Excl. Tax", "Amount Excluding Tax.");
			this.exTaxAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.exTaxAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 121, true);
			this.exTaxAmountCalcFindBox.Name = "exTaxAmountCalcFindBox";
			this.exTaxAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.exTaxAmountCalcFindBox.TabIndex = 8;
			// 
			// AH_DueDateEdit
			// 
			this.AH_DueDateEdit.AllowDrop = true;
			this.AH_DueDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_DueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_DueDateEdit, "AH_DueDate");
			this.AH_DueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 75, true);
			this.AH_DueDateEdit.Name = "AH_DueDateEdit";
			this.AH_DueDateEdit.TabIndex = 6;
			// 
			// AH_TransactionNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_TransactionNumTextBox, "AH_TransactionNum");
			this.AH_TransactionNumTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionPendingAllocationForm|045fecca-fcdb-4545-b2a0-65f9b9010fb3", "Transaction Number");
			this.AH_TransactionNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 6, true);
			this.AH_TransactionNumTextBox.Name = "AH_TransactionNumTextBox";
			this.AH_TransactionNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 17, true);
			this.AH_TransactionNumTextBox.TabIndex = 2;
			// 
			// AH_PostDateEdit
			// 
			this.AH_PostDateEdit.AllowDrop = true;
			this.AH_PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_PostDateEdit, "AH_PostDate");
			this.AH_PostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionPendingAllocationForm|689ca2eb-78aa-44f5-a2c5-685309305710", "Post Date");
			this.AH_PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 29, true);
			this.AH_PostDateEdit.Name = "AH_PostDateEdit";
			this.AH_PostDateEdit.TabIndex = 3;
			// 
			// AH_DescTextbox
			// 
			this.AH_DescTextbox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_DescTextbox, "AH_Desc");
			this.AH_DescTextbox.CaptionResourceString = null;
			this.AH_DescTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AH_DescTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 167, true);
			this.AH_DescTextbox.Name = "AH_DescTextbox";
			this.AH_DescTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 17, true);
			this.AH_DescTextbox.TabIndex = 12;
			// 
			// PlaceOfSupplyDropEdit
			// 
			this.PlaceOfSupplyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfSupplyDropEdit, "AH_PlaceOfSupply");
			this.PlaceOfSupplyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(337, 75, true);
			this.PlaceOfSupplyDropEdit.Name = "PlaceOfSupplyDropEdit";
			this.PlaceOfSupplyDropEdit.ShouldResizeByMaxLength = true;
			this.PlaceOfSupplyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 17, true);
			this.PlaceOfSupplyDropEdit.TabIndex = 17;
			// 
			// AH_GovernmentAllocatedIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_GovernmentAllocatedIDTextBox, "AH_GovernmentAllocatedID");
			this.AH_GovernmentAllocatedIDTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionPendingAllocationForm|bb305f82-1dba-4f00-9e08-b69108993e1f", "Govt. ID");
			this.AH_GovernmentAllocatedIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(265, 29, true);
			this.AH_GovernmentAllocatedIDTextBox.Name = "AH_GovernmentAllocatedIDTextBox";
			this.AH_GovernmentAllocatedIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 17, true);
			this.AH_GovernmentAllocatedIDTextBox.TabIndex = 18;
			this.TransactionDetailsTabPage.PerformLayout();
			this.OverrideContactDropEdit.ResumeLayout(true);
			this.OverrideContactDropEdit.PerformLayout();
			this.OverrideAddressGuidDropEdit.ResumeLayout(true);
			this.OverrideAddressGuidDropEdit.PerformLayout();
			this.AH_InvoiceDateEdit.ResumeLayout(true);
			this.AH_InvoiceDateEdit.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.TaxBranchFindBox.ResumeLayout(true);
			this.TaxBranchFindBox.PerformLayout();
			this.departmentGuidFindBox.ResumeLayout(true);
			this.departmentGuidFindBox.PerformLayout();
			this.creditorOrganisationFindBox.ResumeLayout(true);
			this.creditorOrganisationFindBox.PerformLayout();
			this.ExchangeRateControl.ResumeLayout(true);
			this.ExchangeRateControl.PerformLayout();
			this.taxAmountCalcFindBox.ResumeLayout(true);
			this.taxAmountCalcFindBox.PerformLayout();
			this.localTaxAmountCalcFindBox.ResumeLayout(true);
			this.localTaxAmountCalcFindBox.PerformLayout();
			this.localExTaxAmountCalcFindBox.ResumeLayout(true);
			this.localExTaxAmountCalcFindBox.PerformLayout();
			this.exTaxAmountCalcFindBox.ResumeLayout(true);
			this.exTaxAmountCalcFindBox.PerformLayout();
			this.AH_DueDateEdit.ResumeLayout(true);
			this.AH_DueDateEdit.PerformLayout();
			this.AH_PostDateEdit.ResumeLayout(true);
			this.AH_PostDateEdit.PerformLayout();
			this.PlaceOfSupplyDropEdit.ResumeLayout(true);
			this.PlaceOfSupplyDropEdit.PerformLayout();
			this.AH_DocReceivedDateEdit.ResumeLayout(true);
			this.AH_DocReceivedDateEdit.PerformLayout();
			this.TransactionDetailsTabPage.ResumeLayout(true);

		}

		void sourceXmlTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.importedInvoiceXMLControl = new Enterprise.Accounting.GUI.ImportedInvoiceXMLControl();
			this.sourceXmlTabPage.SuspendLayout();
			this.importedInvoiceXMLControl.SuspendLayout();
			this.sourceXmlTabPage.Controls.Add(this.importedInvoiceXMLControl);
			// 
			// importedInvoiceXMLControl
			// 
			this.importedInvoiceXMLControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.importedInvoiceXMLControl, "TransactionApprovalRequest.PostingDetails.UniversalTransaction");
			this.importedInvoiceXMLControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.importedInvoiceXMLControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.importedInvoiceXMLControl.Name = "importedInvoiceXMLControl";
			this.importedInvoiceXMLControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(856, 422, true);
			this.importedInvoiceXMLControl.TabIndex = 0;
			this.sourceXmlTabPage.PerformLayout();
			this.importedInvoiceXMLControl.ResumeLayout(true);
			this.importedInvoiceXMLControl.PerformLayout();
			this.sourceXmlTabPage.ResumeLayout(true);

		}

		void workflowTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.workflowTabPage.SuspendLayout();
			this.workflowTabPage.ResumeLayout(false);
			this.workflowTabPage.PerformLayout();

		}

		void NotesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);
		}
		#endregion

		private ZPanel bottomPanel;
		private ZPostingButtonsUserControl postingButtonsUserControl;
		public ZTemplateTabControl MainTabControl;
		public ZTabPage TransactionDetailsTabPage;
		private ZGuidDropEdit OverrideContactDropEdit;
		private ZGuidDropEdit OverrideAddressGuidDropEdit;
		private ZCalcEdit AH_NumberOfSupportingDocumentsCalcEdit;
		private ZDateEdit AH_InvoiceDateEdit;
		private ZGuidFindBox BranchFindBox;
		private ZGuidFindBox departmentGuidFindBox;
		private ZOrganisationFindBox creditorOrganisationFindBox;
		private ZExchangeRateControl ExchangeRateControl;
		private ZCalcFindBox taxAmountCalcFindBox;
		private ZCalcFindBox localTaxAmountCalcFindBox;
		private ZCalcFindBox localExTaxAmountCalcFindBox;
		private ZCalcFindBox exTaxAmountCalcFindBox;
		private ZDateEdit AH_DueDateEdit;
		private ZTextBox AH_TransactionNumTextBox;
		private ZDateEdit AH_PostDateEdit;
		private ZTextBox AH_DescTextbox;
		private ZTabPage sourceXmlTabPage;
		private ImportedInvoiceXMLControl importedInvoiceXMLControl;
		private ZWorkflowTabPage workflowTabPage;
		private ZDropEdit PlaceOfSupplyDropEdit;
		private ZDateEdit AH_DocReceivedDateEdit;
		private ZGuidFindBox TaxBranchFindBox;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage NotesTabPage;
		private ZTextBox AH_GovernmentAllocatedIDTextBox;
	}
}
