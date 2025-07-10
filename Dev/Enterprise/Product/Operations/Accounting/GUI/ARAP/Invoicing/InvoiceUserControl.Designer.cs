using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.TaxFramework.GUI;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class InvoiceUserControl
	{


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.HeaderGroupBox = new ZGroupBox();
			this.HeaderGroupBoxOuterPanel = new ZPanel();
			this.AddressWithContactControl = new MasterFiles.GUI.Organisation.UserControls.Address.ZAddressWithContactControl();
			this.ApportionChargesButton = new ZButton();
			this.InvoiceCollapsibleTableLayoutPanel = new CollapsibleTableLayoutPanel();
			this.zPanel2 = new ZPanel();
			this.ComplianceSequenceTextBox = new ZTextBox();
			this.AH_DocReceivedDateEdit = new ZDateEdit();
			this.AH_TransactionNumTextBox = new ZTextBox();
			this.AH_InvoiceDateEdit = new ZDateEdit();
			this.AH_PostDateEdit = new ZDateEdit();
			this.zPanel3 = new ZPanel();
			this.AH_ComplianceSubTypeDropEdit = new ZDropEdit();
			this.InvoiceRemittanceReferenceTextBoxForAP = new ZTextBox();
			this.InvoiceRemittanceReferenceTextBoxForAR = new ZTextBox();
			this.ExchangeRateControl = new ZExchangeRateControl();
			this.UseJobExRateCheckBox = new ZCheckBox();
			this.OverrideJobExRateCheckBox = new ZCheckBox();
			this.zPanel4 = new ZPanel();
			this.AH_GB_TaxBranchFindBox = new ZGuidFindBox();
			this.AH_ConsolidatedInvoiceRefTextBox = new ZTextBox();
			this.zPanel5 = new ZPanel();
			this.CashBasisVATIndicatorCheckbox = new ZCheckBox();
			this.zPanel6 = new ZPanel();
			this.InvoiceTotalValidationCheckBox = new ZCheckBox();
			this.ValidInvoiceTotalCalcEdit = new ZCalcEdit();
			this.ExpectedTaxCalcEdit = new ZCalcEdit();
			this.ExpectedExclTaxCalcEdit = new ZCalcEdit();
			this.zPanel7 = new ZPanel();
			this.zDropEditPlaceOfSupply = new ZDropEdit();
			this.AH_InvoiceTermDropEdit = new ZDropEdit();
			this.AH_InvoiceTermDaysCalcEdit = new ZCalcEdit();
			this.ExtendDropEdit = new ZDropEdit();
			this.zPanel8 = new ZPanel();
			this.GSTInclusiveAmountsCheckBox = new ZCheckBox();
			this.PaymentRequestedDateEdit = new ZDateEdit();
			this.PaymentCriticalityDropEdit = new ZDropEdit();
			this.zPanel9 = new ZPanel();
			this.TransactionGuidFindBox = new ZGuidFindBox();
			this.AH_OriginalTransactionNumTextBox = new ZTextBox();
			this.AH_OriginalInvoiceDateEdit = new ZDateEdit();
			this.ReasonCodeDropEdit = new ZDropEdit();
			this.zPanel10 = new ZPanel();
			this.AH_TransactionReferenceTextBox = new ZTextBox();
			this.IsDisbursementInvoiceCheckBox = new ZCheckBox();
			this.AH_ChequeOrReferenceTextBox = new ZTextBox();
			this.AH_DueDateEdit = new ZDateEdit();
			this.zPanel11 = new ZPanel();
			this.AH_DescTextbox = new ZTextBox();
			this.AH_NumberOfSupportingDocumentsCalcEdit = new ZCalcEdit();
			this.SourceReferenceTextBox = new ZTextBox();
			this.GovernmentAllocatedIDTextBox = new ZTextBox();
			this.zPanel12 = new ZPanel();
			this.AH_OriginalReferenceStartDateEdit = new ZDateEdit();
			this.AH_OriginalReferenceEndDateEdit = new ZDateEdit();
			this.ReasonDescriptionTextBox = new ZTextBox();
			this.BulkChargeImportButton = new ZButton();
			this.LineDetailsGroupbox = new ZGroupBox();
			this.LineSummaryOnlyPanel = new ZPanel();
			this.InvoiceLineHidingMessageLabel = new ZLabel();
			this.LineAndJobSummaryPanel = new ZPanel();
			this.TabControl = new ZTabControl();
			this.LineSummaryTabPage = new ZTabPage();
			this.JobSummaryTabPage = new ZTabPage();
			this.TaxSummaryTabPage = new ZTabPage();
			this.TaxTransactionSummaryTabPage = new ZTabPage();
			this.ReversalDropEdit = new ZDropEdit();
			this.ChangeSizeMenuItem = new ZMenuItem();
			this.HeaderGroupBoxOuterContextMenu = new ContextMenu();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HeaderGroupBox.SuspendLayout();
			this.HeaderGroupBoxOuterPanel.SuspendLayout();
			this.AddressWithContactControl.SuspendLayout();
			this.InvoiceCollapsibleTableLayoutPanel.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.AH_DocReceivedDateEdit.SuspendLayout();
			this.AH_InvoiceDateEdit.SuspendLayout();
			this.AH_PostDateEdit.SuspendLayout();
			this.zPanel3.SuspendLayout();
			this.AH_ComplianceSubTypeDropEdit.SuspendLayout();
			this.ExchangeRateControl.SuspendLayout();
			this.zPanel4.SuspendLayout();
			this.AH_GB_TaxBranchFindBox.SuspendLayout();
			this.zPanel5.SuspendLayout();
			this.zPanel6.SuspendLayout();
			this.zPanel7.SuspendLayout();
			this.zDropEditPlaceOfSupply.SuspendLayout();
			this.AH_InvoiceTermDropEdit.SuspendLayout();
			this.ExtendDropEdit.SuspendLayout();
			this.zPanel8.SuspendLayout();
			this.PaymentRequestedDateEdit.SuspendLayout();
			this.PaymentCriticalityDropEdit.SuspendLayout();
			this.zPanel9.SuspendLayout();
			this.TransactionGuidFindBox.SuspendLayout();
			this.AH_OriginalInvoiceDateEdit.SuspendLayout();
			this.ReasonCodeDropEdit.SuspendLayout();
			this.zPanel10.SuspendLayout();
			this.AH_DueDateEdit.SuspendLayout();
			this.zPanel11.SuspendLayout();
			this.zPanel12.SuspendLayout();
			this.AH_OriginalReferenceStartDateEdit.SuspendLayout();
			this.AH_OriginalReferenceEndDateEdit.SuspendLayout();
			this.LineSummaryOnlyPanel.SuspendLayout();
			this.LineAndJobSummaryPanel.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.ReversalDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(InvoicingBase);
			// 
			// HeaderGroupBox
			// 
			this.HeaderGroupBox.AutoSize = true;
			this.HeaderGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.HeaderGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|15965f2c-d599-4090-9fdb-5f8ef8babbbc", "{0} Summary");
			this.HeaderGroupBox.Controls.Add(this.AddressWithContactControl);
			this.HeaderGroupBox.Controls.Add(this.ApportionChargesButton);
			this.HeaderGroupBox.Controls.Add(this.InvoiceCollapsibleTableLayoutPanel);
			this.HeaderGroupBox.Controls.Add(this.BulkChargeImportButton);
			this.HeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.HeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderGroupBox.Name = "HeaderGroupBox";
			this.HeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 346, true);
			this.HeaderGroupBox.TabIndex = 0;
			this.HeaderGroupBox.TabStop = false;
			//
			// HeaderGroupBoxOuterPanel
			//
			this.HeaderGroupBoxOuterPanel.AutoSize = true;
			this.HeaderGroupBoxOuterPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.HeaderGroupBoxOuterPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.HeaderGroupBoxOuterPanel.Controls.Add(this.HeaderGroupBox);
			this.HeaderGroupBoxOuterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderGroupBoxOuterPanel.Name = "HeaderGroupBoxOuterPanel";
			this.HeaderGroupBoxOuterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 346, true);
			this.HeaderGroupBoxOuterPanel.TabIndex = 0;
			this.HeaderGroupBoxOuterPanel.TabStop = false;
			this.HeaderGroupBoxOuterPanel.AutoScroll = true;
			this.HeaderGroupBoxOuterPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1236, 60, true);
			ChangeSizeMenuItem.Caption = MinimizeInvoiceHeaderMenuItemCaption;
			ChangeSizeMenuItem.Click += ChangeSizeMenuItem_Click;
			HeaderGroupBoxOuterContextMenu.MenuItems.Add(ChangeSizeMenuItem);
			this.HeaderGroupBoxOuterPanel.ContextMenu = HeaderGroupBoxOuterContextMenu;
			// 
			// AddressWithContactControl
			// 
			this.AddressWithContactControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressWithContactControl, "OrganisationAddressWithContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZAddressWithContact)(((InvoicingBase)(null)).OrganisationAddressWithContact)));
			this.AddressWithContactControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|3986d730-36cc-4aab-9a13-0daa8a009efb", "Debtor Information");
			this.AddressWithContactControl.ContactInfoTabVisible = true;
			this.AddressWithContactControl.InvoiceContactTabCaption = Enterprise.Accounting.GUI.Res.GetData("ZAddressWithContactControl|BD031E76-4B11-4997-9453-5D182BCA64CA", "Invoice Contact");
			this.AddressWithContactControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.AddressWithContactControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AddressWithContactControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AddressWithContactControl.Name = "AddressWithContactControl";
			this.AddressWithContactControl.OnlyStopOnDebtor = true;
			this.AddressWithContactControl.PopupCaption = "";
			this.AddressWithContactControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AddressWithContactControl.TabIndex = 0;
			// 
			// ApportionChargesButton
			// 
			this.ApportionChargesButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|7bd2446e-38a6-4110-a69f-5ef6599c5943", "Apportion To Consols");
			this.ApportionChargesButton.IsCaptionOverridden = false;
			this.ApportionChargesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 175, true);
			this.ApportionChargesButton.Name = "ApportionChargesButton";
			this.ApportionChargesButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ApportionChargesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.ApportionChargesButton.TabIndex = 41;
			this.ApportionChargesButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ApportionChargesButton.ToolTipCaption = null;
			this.ApportionChargesButton.UseVisualStyleBackColor = true;
			// 
			// InvoiceCollapsibleTableLayoutPanel
			// 
			this.InvoiceCollapsibleTableLayoutPanel.AutoSize = true;
			this.InvoiceCollapsibleTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.InvoiceCollapsibleTableLayoutPanel.ColumnCount = 1;
			this.InvoiceCollapsibleTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel2, 0, 0);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel3, 0, 2);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel4, 0, 4);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel5, 0, 5);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel6, 0, 6);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel7, 0, 7);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel8, 0, 8);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel9, 0, 9);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel10, 0, 1);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel11, 0, 3);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel12, 0, 10);
			this.InvoiceCollapsibleTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 19, true);
			this.InvoiceCollapsibleTableLayoutPanel.Name = "InvoiceCollapsibleTableLayoutPanel";
			this.InvoiceCollapsibleTableLayoutPanel.RowCount = 11;
			this.InvoiceCollapsibleTableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(28)));
			this.InvoiceCollapsibleTableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(28)));
			this.InvoiceCollapsibleTableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(28)));
			this.InvoiceCollapsibleTableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(28)));
			this.InvoiceCollapsibleTableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(28)));
			this.InvoiceCollapsibleTableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(28)));
			this.InvoiceCollapsibleTableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(28)));
			this.InvoiceCollapsibleTableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(28)));
			this.InvoiceCollapsibleTableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(28)));
			this.InvoiceCollapsibleTableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(28)));
			this.InvoiceCollapsibleTableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(28)));
			this.InvoiceCollapsibleTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 308, true);
			this.InvoiceCollapsibleTableLayoutPanel.TabIndex = 30;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.ComplianceSequenceTextBox);
			this.zPanel2.Controls.Add(this.AH_DocReceivedDateEdit);
			this.zPanel2.Controls.Add(this.AH_TransactionNumTextBox);
			this.zPanel2.Controls.Add(this.AH_InvoiceDateEdit);
			this.zPanel2.Controls.Add(this.AH_PostDateEdit);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.zPanel2.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 26, true);
			this.zPanel2.TabIndex = 0;
			// 
			// ComplianceSequenceTextBox
			// 
			this.ComplianceSequenceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d2c3d8ef-2865-432e-9b0c-3696c75b12de", "Compliance Sequence");
			this.ComplianceSequenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(828, 2, true);
			this.ComplianceSequenceTextBox.Name = "ComplianceSequenceTextBox";
			this.ComplianceSequenceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ComplianceSequenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.ComplianceSequenceTextBox.TabIndex = 4;
			// 
			// AH_DocReceivedDateEdit
			// 
			this.AH_DocReceivedDateEdit.AllowDrop = true;
			this.AH_DocReceivedDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_DocReceivedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_DocReceivedDateEdit, "AH_DocumentReceivedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoicingBase)(null)).AH_DocumentReceivedDate)));
			this.AH_DocReceivedDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|60B5C46A-986F-4CC3-99A3-076EE072F1AE", "Doc Rec Date", "Document Received Date");
			this.AH_DocReceivedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(285, 2, true);
			this.AH_DocReceivedDateEdit.Name = "AH_DocReceivedDateEdit";
			this.AH_DocReceivedDateEdit.TabIndex = 1;
			// 
			// AH_TransactionNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_TransactionNumTextBox, "AH_TransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(null)).AH_TransactionNum)));
			this.AH_TransactionNumTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|d490b3a6-58c2-490a-a5b2-f39ad9884b91", "{0} Num.", "{0} Number", "System generated Transaction number.");
			this.AH_TransactionNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 2, true);
			this.AH_TransactionNumTextBox.Name = "AH_TransactionNumTextBox";
			this.AH_TransactionNumTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AH_TransactionNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 20, true);
			this.AH_TransactionNumTextBox.TabIndex = 3;
			// 
			// AH_InvoiceDateEdit
			// 
			this.AH_InvoiceDateEdit.AllowDrop = true;
			this.AH_InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_InvoiceDateEdit.AutoCompleteYear = true;
			this.AH_InvoiceDateEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AH_InvoiceDateEdit, "AH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoicingBase)(null)).AH_InvoiceDate)));
			this.AH_InvoiceDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|86b4a7c3-0c10-4664-8b9a-5088c4407677", "{0} Date", "{0} Date", "The date of this bank currency adjustment.");
			this.AH_InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 2, true);
			this.AH_InvoiceDateEdit.Name = "AH_InvoiceDateEdit";
			this.AH_InvoiceDateEdit.TabIndex = 0;
			// 
			// AH_PostDateEdit
			// 
			this.AH_PostDateEdit.AllowDrop = true;
			this.AH_PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_PostDateEdit, "AH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoicingBase)(null)).AH_PostDate)));
			this.AH_PostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|92c4e1fa-afad-4b26-ad75-4fa2f9d72197", "Post Date", "Posted Date", "System generated Transaction Date.");
			this.AH_PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(428, 2, true);
			this.AH_PostDateEdit.Name = "AH_PostDateEdit";
			this.AH_PostDateEdit.TabIndex = 2;
			// 
			// zPanel3
			// 
			this.zPanel3.Controls.Add(this.AH_ComplianceSubTypeDropEdit);
			this.zPanel3.Controls.Add(this.InvoiceRemittanceReferenceTextBoxForAP);
			this.zPanel3.Controls.Add(this.InvoiceRemittanceReferenceTextBoxForAR);
			this.zPanel3.Controls.Add(this.ExchangeRateControl);
			this.zPanel3.Controls.Add(this.UseJobExRateCheckBox);
			this.zPanel3.Controls.Add(this.OverrideJobExRateCheckBox);
			this.zPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 57, true);
			this.zPanel3.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel3.Name = "zPanel3";
			this.zPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 26, true);
			this.zPanel3.TabIndex = 2;
			// 
			// AH_ComplianceSubTypeDropEdit
			// 
			this.AH_ComplianceSubTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AH_ComplianceSubTypeDropEdit, "AH_ComplianceSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoicingBase)(null)).AH_ComplianceSubType)));
			this.AH_ComplianceSubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(828, 4, true);
			this.AH_ComplianceSubTypeDropEdit.Name = "AH_ComplianceSubTypeDropEdit";
			this.AH_ComplianceSubTypeDropEdit.PreBoundMaxLength = 3;
			this.AH_ComplianceSubTypeDropEdit.ShouldResizeByMaxLength = true;
			this.AH_ComplianceSubTypeDropEdit.ShowDescriptionBox = false;
			this.AH_ComplianceSubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.AH_ComplianceSubTypeDropEdit.TabIndex = 5;
			// 
			// InvoiceRemittanceReferenceTextBoxForAP
			// 
			this.BindingSource.SetBindingMember(this.InvoiceRemittanceReferenceTextBoxForAP, "InvoiceRemittanceReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(null)).InvoiceRemittanceReference)));
			this.InvoiceRemittanceReferenceTextBoxForAP.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|d13dd012-9cc0-4169-80f1-ef12e4b991ae", "Inv. Remittance Ref.", "Inv. Remittance Ref.", "");
			this.InvoiceRemittanceReferenceTextBoxForAP.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(571, 2, true);
			this.InvoiceRemittanceReferenceTextBoxForAP.Name = "InvoiceRemittanceReferenceTextBoxForAP";
			this.InvoiceRemittanceReferenceTextBoxForAP.ShouldEscapeAllSpecialCharacters = false;
			this.InvoiceRemittanceReferenceTextBoxForAP.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.InvoiceRemittanceReferenceTextBoxForAP.TabIndex = 4;
			// 
			// InvoiceRemittanceReferenceTextBoxForAR
			// 
			this.BindingSource.SetBindingMember(this.InvoiceRemittanceReferenceTextBoxForAR, "InvoiceRemittanceReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(null)).InvoiceRemittanceReference)));
			this.InvoiceRemittanceReferenceTextBoxForAR.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|d13dd012-9cc0-4169-80f1-ef12e4b991ae", "Inv. Remittance Ref.", "Inv. Remittance Ref.", "");
			this.InvoiceRemittanceReferenceTextBoxForAR.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(541, 2, true);
			this.InvoiceRemittanceReferenceTextBoxForAR.Name = "InvoiceRemittanceReferenceTextBoxForAR";
			this.InvoiceRemittanceReferenceTextBoxForAR.ShouldEscapeAllSpecialCharacters = false;
			this.InvoiceRemittanceReferenceTextBoxForAR.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.InvoiceRemittanceReferenceTextBoxForAR.TabIndex = 3;
			// 
			// ExchangeRateControl
			// 
			this.ExchangeRateControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExchangeRateControl, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((InvoicingBase)(null)).ExchangeRate)));
			this.ExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|a5b6343c-720a-4134-b901-1466cf503235", "Exchange Rate", "Exchange Rate", "");
			this.ExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 2, true);
			this.ExchangeRateControl.Name = "ExchangeRateControl";
			this.ExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.ExchangeRateControl.TabIndex = 0;
			// 
			// OverrideJobExRateCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideJobExRateCheckBox, "AH_OverrideExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((InvoicingBase)(null)).AH_OverrideExchangeRate)));
			this.OverrideJobExRateCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|89f6b096-47fd-4cbe-99a4-6f5c6a658cf4", "Override", "Override Line Ex. Rate", "Select this check box to override the exchange rate enforced by the 'AP Invoice Posting Exchange Rate Option' defined in the Registry.");
			this.OverrideJobExRateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideJobExRateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 3, true);
			this.OverrideJobExRateCheckBox.Name = "OverrideJobExRateCheckBox";
			this.OverrideJobExRateCheckBox.TabIndex = 1;
			this.OverrideJobExRateCheckBox.AutoSize = true;
			this.OverrideJobExRateCheckBox.BringToFront();
			// 
			// UseJobExRateCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UseJobExRateCheckBox, "AH_PostedToEFT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((InvoicingBase)(null)).AH_PostedToEFT)));
			this.UseJobExRateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseJobExRateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 3, true);
			this.UseJobExRateCheckBox.Name = "UseJobExRateCheckBox";
			this.UseJobExRateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.UseJobExRateCheckBox.TabIndex = 2;
			this.UseJobExRateCheckBox.AutoSize = true;
			// 
			// zPanel4
			// 
			this.zPanel4.Controls.Add(this.AH_GB_TaxBranchFindBox);
			this.zPanel4.Controls.Add(this.AH_ConsolidatedInvoiceRefTextBox);
			this.zPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 113, true);
			this.zPanel4.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel4.Name = "zPanel4";
			this.zPanel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 26, true);
			this.zPanel4.TabIndex = 4;
			// 
			// AH_GB_TaxBranchFindBox
			// 
			this.AH_GB_TaxBranchFindBox.AllowDrop = true;
			this.AH_GB_TaxBranchFindBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AH_GB_TaxBranchFindBox, "AH_GB_TaxBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingBase)(null)).AH_GB_TaxBranch)));
			this.AH_GB_TaxBranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 2, true);
			this.AH_GB_TaxBranchFindBox.Name = "AH_GB_TaxBranchFindBox";
			this.AH_GB_TaxBranchFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AH_GB_TaxBranchFindBox.ParentType = null;
			this.AH_GB_TaxBranchFindBox.PreBoundMaxLength = 5;
			this.AH_GB_TaxBranchFindBox.ShowDescriptionBox = false;
			this.AH_GB_TaxBranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.AH_GB_TaxBranchFindBox.TabIndex = 0;
			// 
			// AH_ConsolidatedInvoiceRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_ConsolidatedInvoiceRefTextBox, "AH_ConsolidatedInvoiceRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(null)).AH_ConsolidatedInvoiceRef)));
			this.AH_ConsolidatedInvoiceRefTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|61fd9c29-0362-418e-b243-dc4fe5c8fd3f", "Internal Ref");
			this.AH_ConsolidatedInvoiceRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(571, 2, true);
			this.AH_ConsolidatedInvoiceRefTextBox.Name = "AH_ConsolidatedInvoiceRefTextBox";
			this.AH_ConsolidatedInvoiceRefTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AH_ConsolidatedInvoiceRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.AH_ConsolidatedInvoiceRefTextBox.TabIndex = 1;
			this.AH_ConsolidatedInvoiceRefTextBox.TabStop = false;
			// 
			// zPanel5
			// 
			this.zPanel5.Controls.Add(this.CashBasisVATIndicatorCheckbox);
			this.zPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 141, true);
			this.zPanel5.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel5.Name = "zPanel5";
			this.zPanel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 26, true);
			this.zPanel5.TabIndex = 5;
			// 
			// CashBasisVATIndicatorCheckbox
			// 
			this.BindingSource.SetBindingMember(this.CashBasisVATIndicatorCheckbox, "AH_CashBasisGSTIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((InvoicingBase)(null)).AH_CashBasisGSTIndicator)));
			this.CashBasisVATIndicatorCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CashBasisVATIndicatorCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 4, true);
			this.CashBasisVATIndicatorCheckbox.Name = "CashBasisVATIndicatorCheckbox";
			this.CashBasisVATIndicatorCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 16, true);
			this.CashBasisVATIndicatorCheckbox.TabIndex = 0;
			// 
			// zPanel6
			// 
			this.zPanel6.Controls.Add(this.ReversalDropEdit);
			this.zPanel6.Controls.Add(this.InvoiceTotalValidationCheckBox);
			this.zPanel6.Controls.Add(this.ValidInvoiceTotalCalcEdit);
			this.zPanel6.Controls.Add(this.ExpectedTaxCalcEdit);
			this.zPanel6.Controls.Add(this.ExpectedExclTaxCalcEdit);
			this.zPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 169, true);
			this.zPanel6.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel6.Name = "zPanel6";
			this.zPanel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 26, true);
			this.zPanel6.TabIndex = 6;
			// 
			// InvoiceTotalValidationCheckBox
			// 
			this.InvoiceTotalValidationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.InvoiceTotalValidationCheckBox, "ValidateExpectedInvoiceTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((InvoicingBase)(null)).ValidateExpectedInvoiceTotal)));
			this.InvoiceTotalValidationCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|2257454f-0941-4a8f-b3a9-2b5bda1bfa86", "Expected Total", "Expected Total", "");
			this.InvoiceTotalValidationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.InvoiceTotalValidationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 6, true);
			this.InvoiceTotalValidationCheckBox.Name = "InvoiceTotalValidationCheckBox";
			this.InvoiceTotalValidationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 17, true);
			this.InvoiceTotalValidationCheckBox.TabIndex = 0;
			// 
			// ValidInvoiceTotalCalcEdit
			// 
			this.ValidInvoiceTotalCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ValidInvoiceTotalCalcEdit, "ExpectedInvoiceTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingBase)(null)).ExpectedInvoiceTotal)));
			this.ValidInvoiceTotalCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|ae5d1512-ab23-4302-8bfe-9e42dbcebd10", "Incl. Tax");
			this.ValidInvoiceTotalCalcEdit.DecimalPlaces = 2;
			this.ValidInvoiceTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 3, true);
			this.ValidInvoiceTotalCalcEdit.Name = "ValidInvoiceTotalCalcEdit";
			this.ValidInvoiceTotalCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.ValidInvoiceTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.ValidInvoiceTotalCalcEdit.TabIndex = 1;
			this.ValidInvoiceTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ExpectedTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExpectedTaxCalcEdit, "ExpectedInvoiceTaxTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingBase)(null)).ExpectedInvoiceTaxTotal)));
			this.ExpectedTaxCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|7f7cc6c0-cb5a-4f8a-8c95-45c4f2c90c2d", "Tax");
			this.ExpectedTaxCalcEdit.DecimalPlaces = 2;
			this.ExpectedTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 3, true);
			this.ExpectedTaxCalcEdit.Name = "ExpectedTaxCalcEdit";
			this.ExpectedTaxCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.ExpectedTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ExpectedTaxCalcEdit.TabIndex = 2;
			this.ExpectedTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ExpectedExclTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExpectedExclTaxCalcEdit, "ExpectedInvoiceExclTaxTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingBase)(null)).ExpectedInvoiceExclTaxTotal)));
			this.ExpectedExclTaxCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|6d3236d8-57a5-47b1-be6a-3a6c87c8adff", "Excl. Tax");
			this.ExpectedExclTaxCalcEdit.DecimalPlaces = 2;
			this.ExpectedExclTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(597, 3, true);
			this.ExpectedExclTaxCalcEdit.Name = "ExpectedExclTaxCalcEdit";
			this.ExpectedExclTaxCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.ExpectedExclTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ExpectedExclTaxCalcEdit.TabIndex = 3;
			this.ExpectedExclTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zPanel7
			// 
			this.zPanel7.Controls.Add(this.zDropEditPlaceOfSupply);
			this.zPanel7.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.zPanel7.Controls.Add(this.AH_InvoiceTermDropEdit);
			this.zPanel7.Controls.Add(this.AH_InvoiceTermDaysCalcEdit);
			this.zPanel7.Controls.Add(this.ExtendDropEdit);
			this.zPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 197, true);
			this.zPanel7.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel7.Name = "zPanel7";
			this.zPanel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 26, true);
			this.zPanel7.TabIndex = 7;
			// 
			// zDropEditPlaceOfSupply
			// 
			this.zDropEditPlaceOfSupply.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEditPlaceOfSupply, "AH_PlaceOfSupply");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoicingBase)(null)).AH_PlaceOfSupply)));
			this.zDropEditPlaceOfSupply.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 3, true);
			this.zDropEditPlaceOfSupply.Name = "zDropEditPlaceOfSupply";
			this.zDropEditPlaceOfSupply.PreBoundMaxLength = 3;
			this.zDropEditPlaceOfSupply.ShouldResizeByMaxLength = true;
			this.zDropEditPlaceOfSupply.ShowDescriptionBox = false;
			this.zDropEditPlaceOfSupply.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.zDropEditPlaceOfSupply.TabIndex = 1;
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_NumberOfSupportingDocumentsCalcEdit, "AH_NumberOfSupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingBase)(null)).AH_NumberOfSupportingDocuments)));
			this.AH_NumberOfSupportingDocumentsCalcEdit.CaptionResourceString = null;
			this.AH_NumberOfSupportingDocumentsCalcEdit.DecimalPlaces = 2;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 3, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 3;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AH_InvoiceTermDropEdit
			// 
			this.AH_InvoiceTermDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AH_InvoiceTermDropEdit, "AH_InvoiceTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoicingBase)(null)).AH_InvoiceTerm)));
			this.AH_InvoiceTermDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|841a2cec-a8b9-4589-84ce-fc1780da075e", "Terms", "Terms", "");
			this.AH_InvoiceTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 3, true);
			this.AH_InvoiceTermDropEdit.Name = "AH_InvoiceTermDropEdit";
			this.AH_InvoiceTermDropEdit.PreBoundMaxLength = 3;
			this.AH_InvoiceTermDropEdit.ShouldResizeByMaxLength = true;
			this.AH_InvoiceTermDropEdit.ShowDescriptionBox = false;
			this.AH_InvoiceTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.AH_InvoiceTermDropEdit.TabIndex = 2;
			// 
			// AH_InvoiceTermDaysCalcEdit
			// 
			this.AH_InvoiceTermDaysCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_InvoiceTermDaysCalcEdit, "AH_InvoiceTermDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingBase)(null)).AH_InvoiceTermDays)));
			this.AH_InvoiceTermDaysCalcEdit.CaptionResourceString = null;
			this.AH_InvoiceTermDaysCalcEdit.DecimalPlaces = 2;
			this.AH_InvoiceTermDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(665, 3, true);
			this.AH_InvoiceTermDaysCalcEdit.Name = "AH_InvoiceTermDaysCalcEdit";
			this.AH_InvoiceTermDaysCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.AH_InvoiceTermDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.AH_InvoiceTermDaysCalcEdit.TabIndex = 3;
			this.AH_InvoiceTermDaysCalcEdit.Text = "0";
			this.AH_InvoiceTermDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ExtendDropEdit
			// 
			this.ExtendDropEdit.AllowDrop = true;
			this.ExtendDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9a873a11-5a6b-472a-a623-47887c92d1b4", " ");
			this.ExtendDropEdit.Enabled = false;
			this.ExtendDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(828, 3, true);
			this.ExtendDropEdit.Name = "ExtendDropEdit";
			this.ExtendDropEdit.PreBoundMaxLength = 3;
			this.ExtendDropEdit.ShouldResizeByMaxLength = true;
			this.ExtendDropEdit.ShowDescriptionBox = false;
			this.ExtendDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.ExtendDropEdit.TabIndex = 4;
			this.ExtendDropEdit.Visible = false;
			// 
			// zPanel8
			// 
			this.zPanel8.Controls.Add(this.GSTInclusiveAmountsCheckBox);
			this.zPanel8.Controls.Add(this.PaymentRequestedDateEdit);
			this.zPanel8.Controls.Add(this.PaymentCriticalityDropEdit);
			this.zPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 225, true);
			this.zPanel8.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel8.Name = "zPanel8";
			this.zPanel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 26, true);
			this.zPanel8.TabIndex = 8;
			// 
			// GSTInclusiveAmountsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.GSTInclusiveAmountsCheckBox, "GSTInclusiveAmounts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((InvoicingBase)(null)).GSTInclusiveAmounts)));
			this.GSTInclusiveAmountsCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|27dc6bba-b47a-42c7-a86e-acc75b4da4f4", "Tax Inclusive Amounts", "Tax Inclusive Amounts", "");
			this.GSTInclusiveAmountsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GSTInclusiveAmountsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(551, 5, true);
			this.GSTInclusiveAmountsCheckBox.Name = "GSTInclusiveAmountsCheckBox";
			this.GSTInclusiveAmountsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 16, true);
			this.GSTInclusiveAmountsCheckBox.TabIndex = 2;
			// 
			// PaymentRequestedDateEdit
			// 
			this.PaymentRequestedDateEdit.AllowDrop = true;
			this.PaymentRequestedDateEdit.AutoCompleteMonthThreshold = 1;
			this.PaymentRequestedDateEdit.AutoCompleteYear = true;
			this.PaymentRequestedDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|b077cfb3-2b6e-4bb1-ac46-aa0223856e0f", "Payment Requested");
			this.PaymentRequestedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 2, true);
			this.PaymentRequestedDateEdit.Name = "PaymentRequestedDateEdit";
			this.PaymentRequestedDateEdit.TabIndex = 0;
			// 
			// PaymentCriticalityDropEdit
			// 
			this.PaymentCriticalityDropEdit.AllowDrop = true;
			this.PaymentCriticalityDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|75cbb9d6-c329-42ba-8610-7bbc6cabc62d", "Payment Criticality");
			this.PaymentCriticalityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 2, true);
			this.PaymentCriticalityDropEdit.Name = "PaymentCriticalityDropEdit";
			this.PaymentCriticalityDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentCriticalityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
			this.PaymentCriticalityDropEdit.TabIndex = 1;
			// 
			// zPanel9
			// 
			this.zPanel9.Controls.Add(this.TransactionGuidFindBox);
			this.zPanel9.Controls.Add(this.AH_OriginalTransactionNumTextBox);
			this.zPanel9.Controls.Add(this.AH_OriginalInvoiceDateEdit);
			this.zPanel9.Controls.Add(this.ReasonCodeDropEdit);
			this.zPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 253, true);
			this.zPanel9.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel9.Name = "zPanel9";
			this.zPanel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 26, true);
			this.zPanel9.TabIndex = 9;
			// 
			// TransactionGuidFindBox
			// 
			this.TransactionGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransactionGuidFindBox, "OriginalTransactionReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingBase)(null)).OriginalTransactionReference)));
			this.TransactionGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|1ad9577c-ebb4-4d24-b529-f7f8ceb0a0dc", "Original Reference");
			this.TransactionGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 3, true);
			this.TransactionGuidFindBox.Name = "TransactionGuidFindBox";
			this.TransactionGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransactionGuidFindBox.ParentType = null;
			this.TransactionGuidFindBox.PopupCaption = null;
			this.TransactionGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.TransactionGuidFindBox.TabIndex = 0;
			// 
			// AH_OriginalTransactionNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_OriginalTransactionNumTextBox, "AH_OriginalTransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(null)).AH_OriginalTransactionNum)));
			this.AH_OriginalTransactionNumTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|BD968050-53F2-4DA2-9348-D336FCC92DE8", "Original Invoice Num.");
			this.AH_OriginalTransactionNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 3, true);
			this.AH_OriginalTransactionNumTextBox.Name = "AH_OriginalTransactionNumTextBox";
			this.AH_OriginalTransactionNumTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AH_OriginalTransactionNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 20, true);
			this.AH_OriginalTransactionNumTextBox.TabIndex = 1;
			// 
			// AH_OriginalInvoiceDateEdit
			// 
			this.AH_OriginalInvoiceDateEdit.AllowDrop = true;
			this.AH_OriginalInvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_OriginalInvoiceDateEdit.AutoCompleteYear = true;
			this.AH_OriginalInvoiceDateEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AH_OriginalInvoiceDateEdit, "AH_OriginalInvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoicingBase)(null)).AH_OriginalInvoiceDate)));
			this.AH_OriginalInvoiceDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|DDEA3EDD-E4CD-4629-A025-E5CEEE29B199", "Date");
			this.AH_OriginalInvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(612, 3, true);
			this.AH_OriginalInvoiceDateEdit.Name = "AH_OriginalInvoiceDateEdit";
			this.AH_OriginalInvoiceDateEdit.TabIndex = 2;
			// 
			// ReasonCodeDropEdit
			// 
			this.ReasonCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReasonCodeDropEdit, "ReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoicingBase)(null)).ReasonCode)));
			this.ReasonCodeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("807bf8e6-74a3-4d93-815c-153e4453fae4", "Reason Code", "Reason Code", "");
			this.ReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(800, 3, true);
			this.ReasonCodeDropEdit.Name = "ReasonCodeDropEdit";
			this.ReasonCodeDropEdit.PreBoundMaxLength = 3;
			this.ReasonCodeDropEdit.ShouldResizeByMaxLength = true;
			this.ReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.ReasonCodeDropEdit.TabIndex = 6;
			// 
			// zPanel10
			// 
			this.zPanel10.Controls.Add(this.AH_TransactionReferenceTextBox);
			this.zPanel10.Controls.Add(this.IsDisbursementInvoiceCheckBox);
			this.zPanel10.Controls.Add(this.AH_ChequeOrReferenceTextBox);
			this.zPanel10.Controls.Add(this.AH_DueDateEdit);
			this.zPanel10.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 29, true);
			this.zPanel10.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel10.Name = "zPanel10";
			this.zPanel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 26, true);
			this.zPanel10.TabIndex = 1;
			// 
			// AH_TransactionReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_TransactionReferenceTextBox, "AH_TransactionReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(null)).AH_TransactionReference)));
			this.AH_TransactionReferenceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8c697d99-8d69-4960-9646-b25d7946fb62", "Compliance Number");
			this.AH_TransactionReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(828, 2, true);
			this.AH_TransactionReferenceTextBox.Name = "AH_TransactionReferenceTextBox";
			this.AH_TransactionReferenceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AH_TransactionReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.AH_TransactionReferenceTextBox.TabIndex = 3;
			// 
			// IsDisbursementInvoiceCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsDisbursementInvoiceCheckBox, "IsDisbursementOrFinal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((InvoicingBase)(null)).IsDisbursementOrFinal)));
			this.IsDisbursementInvoiceCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|6a29e99e-6011-4b70-af7f-1c37963799a6", "Is Disbursement Invoice", "Is Disbursement Invoice", "");
			this.IsDisbursementInvoiceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsDisbursementInvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 3, true);
			this.IsDisbursementInvoiceCheckBox.Name = "IsDisbursementInvoiceCheckBox";
			this.IsDisbursementInvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 18, true);
			this.IsDisbursementInvoiceCheckBox.TabIndex = 1;
			// 
			// AH_ChequeOrReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.AH_ChequeOrReferenceTextBox, "AH_ChequeOrReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(null)).AH_ChequeOrReference)));
			this.AH_ChequeOrReferenceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f77f849f-aaa4-44ed-8a8a-f9d520eeecc5", "Sup. Cost Ref.", "Supplier Cost Reference", "");
			this.AH_ChequeOrReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(571, 2, true);
			this.AH_ChequeOrReferenceTextBox.Name = "AH_ChequeOrReferenceTextBox";
			this.AH_ChequeOrReferenceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AH_ChequeOrReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.AH_ChequeOrReferenceTextBox.TabIndex = 2;
			// 
			// AH_DueDateEdit
			// 
			this.AH_DueDateEdit.AllowDrop = true;
			this.AH_DueDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_DueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_DueDateEdit, "AH_DueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoicingBase)(null)).AH_DueDate)));
			this.AH_DueDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|692ae3ed-b322-4dc1-b9b9-4795c82a04af", "Due Date", "System generated Transaction Date.");
			this.AH_DueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 2, true);
			this.AH_DueDateEdit.Name = "AH_DueDateEdit";
			this.AH_DueDateEdit.TabIndex = 0;
			// 
			// zPanel11
			// 
			this.zPanel11.Controls.Add(this.AH_DescTextbox);
			this.zPanel11.Controls.Add(this.SourceReferenceTextBox);
			this.zPanel11.Controls.Add(this.GovernmentAllocatedIDTextBox);
			this.zPanel11.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 85, true);
			this.zPanel11.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel11.Name = "zPanel11";
			this.zPanel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 26, true);
			this.zPanel11.TabIndex = 3;
			// 
			// AH_DescTextbox
			// 
			this.AH_DescTextbox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_DescTextbox, "AH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(null)).AH_Desc)));
			this.AH_DescTextbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|8d6c3dcb-5bb7-4511-95dd-28a6d55ae6ab", "Desc.", "Description", "A short description of this currency adjustment.");
			this.AH_DescTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AH_DescTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 2, true);
			this.AH_DescTextbox.Name = "AH_DescTextbox";
			this.AH_DescTextbox.ShouldEscapeAllSpecialCharacters = false;
			this.AH_DescTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 20, true);
			this.AH_DescTextbox.TabIndex = 0;
			// 
			// SourceReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.SourceReferenceTextBox, "SourceReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(null)).SourceReference)));
			this.SourceReferenceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("21b9febd-fc6e-40c5-bceb-91bf11e2fc93", "Source Ref.", "Source Reference", "");
			this.SourceReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(828, 4, true);
			this.SourceReferenceTextBox.Name = "SourceReferenceTextBox";
			this.SourceReferenceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.SourceReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.SourceReferenceTextBox.TabIndex = 2;
			this.SourceReferenceTextBox.TabStop = false;
			// 
			// GovernmentAllocatedIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.GovernmentAllocatedIDTextBox, "AH_GovernmentAllocatedID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(null)).AH_GovernmentAllocatedID)));
			this.GovernmentAllocatedIDTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bafa9ddf-8ecc-4ae3-9de9-509c45a496bb", "Govt. ID", "Government ID", "Government Allocated ID");
			this.GovernmentAllocatedIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(571, 2, true);
			this.GovernmentAllocatedIDTextBox.Name = "GovernmentAllocatedIDTextBox";
			this.GovernmentAllocatedIDTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.GovernmentAllocatedIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.GovernmentAllocatedIDTextBox.TabIndex = 2;
			this.GovernmentAllocatedIDTextBox.TabStop = false;
			this.GovernmentAllocatedIDTextBox.Visible = false;
			// 
			// zPanel12
			// 
			this.zPanel12.Controls.Add(this.AH_OriginalReferenceStartDateEdit);
			this.zPanel12.Controls.Add(this.AH_OriginalReferenceEndDateEdit);
			this.zPanel12.Controls.Add(this.ReasonDescriptionTextBox);
			this.zPanel12.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 281, true);
			this.zPanel12.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel12.Name = "zPanel12";
			this.zPanel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 26, true);
			this.zPanel12.TabIndex = 10;
			// 
			// AH_OriginalReferenceStartDateEdit
			// 
			this.AH_OriginalReferenceStartDateEdit.AllowDrop = true;
			this.AH_OriginalReferenceStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_OriginalReferenceStartDateEdit.AutoCompleteYear = true;
			this.AH_OriginalReferenceStartDateEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AH_OriginalReferenceStartDateEdit, "AH_OriginalReferenceStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoicingBase)(null)).AH_OriginalReferenceStartDate)));
			this.AH_OriginalReferenceStartDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|d46730aa-4c99-4762-a9a3-6b8ee859818b", "Reference Date From");
			this.AH_OriginalReferenceStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 2, true);
			this.AH_OriginalReferenceStartDateEdit.Name = "AH_OriginalReferenceStartDateEdit";
			this.AH_OriginalReferenceStartDateEdit.TabIndex = 0;
			// 
			// AH_OriginalReferenceEndDateEdit
			// 
			this.AH_OriginalReferenceEndDateEdit.AllowDrop = true;
			this.AH_OriginalReferenceEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_OriginalReferenceEndDateEdit.AutoCompleteYear = true;
			this.AH_OriginalReferenceEndDateEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AH_OriginalReferenceEndDateEdit, "AH_OriginalReferenceEndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoicingBase)(null)).AH_OriginalReferenceEndDate)));
			this.AH_OriginalReferenceEndDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|76efff44-e88c-485b-9bdf-fa9def7267cd", "Reference Date To");
			this.AH_OriginalReferenceEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 3, true);
			this.AH_OriginalReferenceEndDateEdit.Name = "AH_OriginalReferenceEndDateEdit";
			this.AH_OriginalReferenceEndDateEdit.TabIndex = 1;
			// 
			// ReasonDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReasonDescriptionTextBox, "ReasonDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(null)).ReasonDescription)));
			this.ReasonDescriptionTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("91e0dcd7-4c4f-4a31-9185-772de527bc78", "Reason Desc.", "Reason Description", "");
			this.ReasonDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(800, 3, true);
			this.ReasonDescriptionTextBox.Name = "ReasonDescriptionTextBox";
			this.ReasonDescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ReasonDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 20, true);
			this.ReasonDescriptionTextBox.TabIndex = 5;
			this.ReasonDescriptionTextBox.TabStop = false;
			// 
			// BulkChargeImportButton
			// 
			this.BulkChargeImportButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|46bf18f7-7126-45b4-ae86-b50ae73e89ba", "Bulk Charge Import");
			this.BulkChargeImportButton.IsCaptionOverridden = false;
			this.BulkChargeImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 175, true);
			this.BulkChargeImportButton.Name = "BulkChargeImportButton";
			this.BulkChargeImportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.BulkChargeImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.BulkChargeImportButton.TabIndex = 40;
			this.BulkChargeImportButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.BulkChargeImportButton.ToolTipCaption = null;
			this.BulkChargeImportButton.UseVisualStyleBackColor = true;
			// 
			// LineDetailsGroupbox
			// 
			this.LineDetailsGroupbox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.LineDetailsGroupbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|db75dc12-a65f-4315-9168-1677d56f1d7c", "Line Summary");
			this.LineDetailsGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			this.LineDetailsGroupbox.Name = "LineDetailsGroupbox";
			this.LineDetailsGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 172, true);
			this.LineDetailsGroupbox.TabIndex = 2;
			this.LineDetailsGroupbox.TabStop = false;
			// 
			// LineSummaryOnlyPanel
			// 
			this.LineSummaryOnlyPanel.Controls.Add(this.InvoiceLineHidingMessageLabel);
			this.LineSummaryOnlyPanel.Controls.Add(this.LineDetailsGroupbox);
			this.LineSummaryOnlyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LineSummaryOnlyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 346, true);
			this.LineSummaryOnlyPanel.Name = "LineSummaryOnlyPanel";
			this.LineSummaryOnlyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 196, true);
			this.LineSummaryOnlyPanel.TabIndex = 3;
			// 
			// InvoiceLineHidingMessageLabel
			// 
			this.InvoiceLineHidingMessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5c89e4be-adc7-4ab1-bcb1-ade5d3e16fea", "Note: Charges posted to branch / dept outside login permission are not listed.");
			this.InvoiceLineHidingMessageLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.InvoiceLineHidingMessageLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.InvoiceLineHidingMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceLineHidingMessageLabel.Name = "InvoiceLineHidingMessageLabel";
			this.InvoiceLineHidingMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 20, true);
			this.InvoiceLineHidingMessageLabel.TabIndex = 0;
			this.InvoiceLineHidingMessageLabel.Visible = false;
			// 
			// LineAndJobSummaryPanel
			// 
			this.LineAndJobSummaryPanel.Controls.Add(this.TabControl);
			this.LineAndJobSummaryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LineAndJobSummaryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 346, true);
			this.LineAndJobSummaryPanel.Name = "LineAndJobSummaryPanel";
			this.LineAndJobSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 196, true);
			this.LineAndJobSummaryPanel.TabIndex = 1;
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.LineSummaryTabPage);
			this.TabControl.Controls.Add(this.JobSummaryTabPage);
			this.TabControl.Controls.Add(this.TaxSummaryTabPage);
			this.TabControl.Controls.Add(this.TaxTransactionSummaryTabPage);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 172, true);
			this.TabControl.TabIndex = 0;
			this.TabControl.SelectedIndexChanged += new EventHandler(this.TabControl_SelectedIndexChanged);
			// 
			// LineSummaryTabPage
			// 
			this.LineSummaryTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.LineSummaryTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|87e78ddc-7274-4106-ad4e-c5d1acadfd14", "Line Summary");
			this.LineSummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LineSummaryTabPage.Name = "LineSummaryTabPage";
			this.LineSummaryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LineSummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1232, 145, true);
			this.LineSummaryTabPage.TabIndex = 0;
			this.LineSummaryTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.LineSummaryTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).GenericCharge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ComplianceDocumentSupportingReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ComplianceSupportingDocumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ChargeTypeWithOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_JH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_GB_TaxBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ExchangeRate.Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).Lookups.TransactionCurrencies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_ExchangeRate_Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ExchangeRate.Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_OSExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_AT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDate)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_TaxDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ComplianceDocumentDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_OSTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_AW)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_OSWHTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_LocalWHTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_OverseasTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_LocalTotalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_LocalExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_LocalTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_IsFinalCharge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ConsolIDFromApportionedCharge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_PreventInvoicePrintGrouping)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).CreateComplianceDocumentRecordOnPosting)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_OSExtraTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_LocalExtraTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_OSGSTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).GSTInclusiveAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_LocalGSTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_AG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).JobLocalReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_A9_VATClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).TaxReportingBasisHumanReadableName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_Calc_InputGSTVATRecoverablePercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_OSTaxAmount_Recoverable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_OSTaxAmount_NotRecoverable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_LocalTaxAmount_NotRecoverable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_LocalTaxAmount_Recoverable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ComplianceDocumentReportingPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).JobLocalClient)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).JobOverseasAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_Calc_FirstSubClassParentId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_Calc_FirstSubClassParent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_Calc_SecondSubClassParentId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_Calc_SecondSubClassParent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ComplianceDocumentOrganization)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).JobConsolXMLData)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ImportedChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ImportedChargeCodeXmlCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).Job.JH_GS_NKRepOps)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_GovtChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).BranchName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).DepartmentDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ComplianceSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ComplianceDocumentNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ComplianceDocumentVATRegistrationNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).ComplianceSupportingDocumentNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_Calc_RelatedJobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).TaxBranchName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_PlaceOfSupply)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodApportionmentMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDate)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodStartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDate)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodEndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodClearingGLAccountPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AL_SupplyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AlternateGLAccountNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).AlternateGLAccountDescription)));
			// 
			// JobSummaryTabPage
			// 
			this.JobSummaryTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.JobSummaryTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|911f788a-5ab7-4e00-bd5a-0db99ae00e63", "Job Summary");
			this.JobSummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.JobSummaryTabPage.Name = "JobSummaryTabPage";
			this.JobSummaryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.JobSummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1232, 145, true);
			this.JobSummaryTabPage.TabIndex = 1;
			this.JobSummaryTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.JobSummaryTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceDependentJobs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoiceDependentJob)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceDependentJobs)).SyncRoot)).JH_JobNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoiceDependentJob)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceDependentJobs)).SyncRoot)).JH_HouseBillNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoiceDependentJob)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceDependentJobs)).SyncRoot)).JH_RelatedInvoiceLinesCostAmountInInvoiceCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoiceDependentJob)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceDependentJobs)).SyncRoot)).JH_RelatedInvoiceLinesCostTaxAmountInInvoiceCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoiceDependentJob)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceDependentJobs)).SyncRoot)).JH_RelatedInvoiceLinesTotalCostInInvoiceCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoiceDependentJob)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceDependentJobs)).SyncRoot)).JH_InvoiceCurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoiceDependentJob)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceDependentJobs)).SyncRoot)).JH_RelatedInvoiceLinesCostAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoiceDependentJob)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceDependentJobs)).SyncRoot)).JH_RelatedInvoiceLinesCostTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoiceDependentJob)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceDependentJobs)).SyncRoot)).JH_RelatedInvoiceLinesTotalCost)));
			// 
			// TaxSummaryTabPage
			// 
			this.TaxSummaryTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.TaxSummaryTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|b77dcff5-f812-4057-907f-0c4bea480240", "{0} Tax Summary");
			this.TaxSummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TaxSummaryTabPage.Name = "TaxSummaryTabPage";
			this.TaxSummaryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TaxSummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1232, 145, true);
			this.TaxSummaryTabPage.TabIndex = 2;
			this.TaxSummaryTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.TaxSummaryTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceLineTaxSummaries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineTaxSummary)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceLineTaxSummaries)).SyncRoot)).TaxID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineTaxSummary)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceLineTaxSummaries)).SyncRoot)).Message)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineTaxSummary)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceLineTaxSummaries)).SyncRoot)).LocalExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineTaxSummary)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceLineTaxSummaries)).SyncRoot)).LocalTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineTaxSummary)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceLineTaxSummaries)).SyncRoot)).LocalTotalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingLineTaxSummary)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceLineTaxSummaries)).SyncRoot)).TaxRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingLineTaxSummary)(((System.Collections.IList)(((InvoicingBase)(null)).InvoiceLineTaxSummaries)).SyncRoot)).TaxIDDescription)));
			// 
			// TaxTransactionSummaryTabPage
			// 
			this.TaxTransactionSummaryTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.TaxTransactionSummaryTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("124DF2BC-A047-4CF4-A24A-6ED24C2333D9", "Tax Transaction Summary");
			this.TaxTransactionSummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TaxTransactionSummaryTabPage.Name = "TaxTransactionSummaryTabPage";
			this.TaxTransactionSummaryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TaxTransactionSummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1232, 145, true);
			this.TaxTransactionSummaryTabPage.TabIndex = 3;
			this.TaxTransactionSummaryTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.TaxTransactionSummaryTabPage_InitializeTab));
			// 
			// ReversalDropEdit
			// 
			this.ReversalDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReversalDropEdit, "ReversalStatusCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoicingBase)(null)).ReversalStatusCode)));
			this.ReversalDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(828, 3, true);
			this.ReversalDropEdit.Name = "ReversalDropEdit";
			this.ReversalDropEdit.PreBoundMaxLength = 3;
			this.ReversalDropEdit.ShouldResizeByMaxLength = false;
			this.ReversalDropEdit.ShowDescriptionBox = false;
			this.ReversalDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.ReversalDropEdit.TabIndex = 5;
			this.ReversalDropEdit.Visible = false;
			// 
			// InvoiceUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LineAndJobSummaryPanel);
			this.Controls.Add(this.LineSummaryOnlyPanel);
			this.Controls.Add(this.HeaderGroupBoxOuterPanel);
			this.Name = "InvoiceUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 542, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.HeaderGroupBoxOuterPanel.ResumeLayout(true);
			this.HeaderGroupBoxOuterPanel.PerformLayout();
			this.HeaderGroupBox.ResumeLayout(false);
			this.HeaderGroupBox.PerformLayout();
			this.AddressWithContactControl.ResumeLayout(true);
			this.AddressWithContactControl.PerformLayout();
			this.InvoiceCollapsibleTableLayoutPanel.ResumeLayout(false);
			this.InvoiceCollapsibleTableLayoutPanel.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.AH_DocReceivedDateEdit.ResumeLayout(true);
			this.AH_DocReceivedDateEdit.PerformLayout();
			this.AH_InvoiceDateEdit.ResumeLayout(true);
			this.AH_InvoiceDateEdit.PerformLayout();
			this.AH_PostDateEdit.ResumeLayout(true);
			this.AH_PostDateEdit.PerformLayout();
			this.zPanel3.ResumeLayout(false);
			this.zPanel3.PerformLayout();
			this.AH_ComplianceSubTypeDropEdit.ResumeLayout(true);
			this.AH_ComplianceSubTypeDropEdit.PerformLayout();
			this.ExchangeRateControl.ResumeLayout(true);
			this.ExchangeRateControl.PerformLayout();
			this.zPanel4.ResumeLayout(false);
			this.zPanel4.PerformLayout();
			this.AH_GB_TaxBranchFindBox.ResumeLayout(true);
			this.AH_GB_TaxBranchFindBox.PerformLayout();
			this.zPanel5.ResumeLayout(false);
			this.zPanel5.PerformLayout();
			this.zPanel6.ResumeLayout(false);
			this.zPanel6.PerformLayout();
			this.zPanel7.ResumeLayout(false);
			this.zPanel7.PerformLayout();
			this.zDropEditPlaceOfSupply.ResumeLayout(true);
			this.zDropEditPlaceOfSupply.PerformLayout();
			this.AH_InvoiceTermDropEdit.ResumeLayout(true);
			this.AH_InvoiceTermDropEdit.PerformLayout();
			this.ExtendDropEdit.ResumeLayout(true);
			this.ExtendDropEdit.PerformLayout();
			this.zPanel8.ResumeLayout(false);
			this.zPanel8.PerformLayout();
			this.PaymentRequestedDateEdit.ResumeLayout(true);
			this.PaymentRequestedDateEdit.PerformLayout();
			this.PaymentCriticalityDropEdit.ResumeLayout(true);
			this.PaymentCriticalityDropEdit.PerformLayout();
			this.zPanel9.ResumeLayout(false);
			this.zPanel9.PerformLayout();
			this.TransactionGuidFindBox.ResumeLayout(true);
			this.TransactionGuidFindBox.PerformLayout();
			this.AH_OriginalInvoiceDateEdit.ResumeLayout(true);
			this.AH_OriginalInvoiceDateEdit.PerformLayout();
			this.ReasonCodeDropEdit.ResumeLayout(true);
			this.ReasonCodeDropEdit.PerformLayout();
			this.zPanel10.ResumeLayout(false);
			this.zPanel10.PerformLayout();
			this.AH_DueDateEdit.ResumeLayout(true);
			this.AH_DueDateEdit.PerformLayout();
			this.zPanel11.ResumeLayout(false);
			this.zPanel11.PerformLayout();
			this.zPanel12.ResumeLayout(false);
			this.zPanel12.PerformLayout();
			this.AH_OriginalReferenceStartDateEdit.ResumeLayout(true);
			this.AH_OriginalReferenceStartDateEdit.PerformLayout();
			this.AH_OriginalReferenceEndDateEdit.ResumeLayout(true);
			this.AH_OriginalReferenceEndDateEdit.PerformLayout();
			this.LineSummaryOnlyPanel.ResumeLayout(false);
			this.LineSummaryOnlyPanel.PerformLayout();
			this.LineAndJobSummaryPanel.ResumeLayout(false);
			this.LineAndJobSummaryPanel.PerformLayout();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.ReversalDropEdit.ResumeLayout(true);
			this.ReversalDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		[SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		[SuppressMessage("Microsoft.Maintainability", "CA1505: Avoid unmaintainable code")]
		void LineSummaryTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new ZMultiLineTextBoxColumnInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new ZGuidFindBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo7 = new ZGuidFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZCalcEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZCheckBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo8 = new ZGuidFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo9 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo17 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo18 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo19 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo20 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo21 = new ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo10 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo11 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo12 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo13 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo14 = new ZGuidFindBoxColumnStyleInfo();
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new ZMultiLineTextBoxColumnInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZDateEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo15 = new ZGuidFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new ZTextBoxColumnStyleInfo();
			this.TransactionLinesGrid = new ZGrid();
			this.zPanel1 = new ZPanel();
			this.LineSummaryTabPage.SuspendLayout();
			((ISupportInitialize)(this.TransactionLinesGrid)).BeginInit();
			this.TransactionLinesGrid.SuspendLayout();
			this.LineSummaryTabPage.Controls.Add(this.TransactionLinesGrid);
			this.LineSummaryTabPage.Controls.Add(this.zPanel1);
			// 
			// TransactionLinesGrid
			// 
			this.TransactionLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TransactionLinesGrid, "FilteredLines");
			this.TransactionLinesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|5c93fcf1-cb5c-4367-9882-538fe55b844a", "Charges");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GenericCharge";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GenericCharge;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|0A6ECD35-5162-4305-8D04-D465D3F6C4A1", "Supporting Reason");
			zDropEditColumnStyleInfo1.ColumnName = "ComplianceDocumentSupportingReason";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|359C013A-5EA5-4172-8141-18845C54ECD4", "Supporting Doc Type");
			zDropEditColumnStyleInfo2.ColumnName = "ComplianceSupportingDocumentType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e9c7911d-de60-4576-8d3c-34398247d93b", "Charge Type");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeTypeWithOverride";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|e4015467-d22e-4989-9d0e-056ed30ad19a", "Job");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AL_JH";
			zGuidFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.JobHeader;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.ColumnName = "AL_Desc";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "AL_GB";
			zGuidFindBoxColumnStyleInfo3.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AL_GB_TaxBranch";
			zGuidFindBoxColumnStyleInfo4.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|453353ac-7c0b-4fab-80eb-801b942baa10", "Dept");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "AL_GE";
			zGuidFindBoxColumnStyleInfo5.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbDepartment;
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups.TransactionCurrencies";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|b0604db1-4369-495a-833b-4412d03edd27", "Cur.", "Currency", "Line Currency.");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ExchangeRate+Currency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "AL_ExchangeRate_Decimals";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|ee92b697-de09-4d0a-b511-b748f3271a7f", "Exch.", "Ex. Rate", "Exchange Rate", "Line Exchange Rate.");
			zCalcEditColumnStyleInfo1.ColumnName = "ExchangeRate+Rate";
			zCalcEditColumnStyleInfo1.Decimals = 4;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "Decimals";
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|c3ae0ffd-0d36-4d0a-a5ae-e75f1c133271", "Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "AL_OSExTaxAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo6.ColumnName = "AL_AT";
			zGuidFindBoxColumnStyleInfo6.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccTaxRate;
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.ColumnName = "AL_TaxDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|DDF38652-ED45-48A8-B558-A8E4482B748D", "Document Date");
			zDateEditColumnStyleInfo2.ColumnName = "ComplianceDocumentDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = "Decimals";
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|7892c3c5-1f82-495e-a7cf-06f552f1f868", "Tax");
			zCalcEditColumnStyleInfo3.ColumnName = "AL_OSTaxAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|24e37055-d4e2-4934-96ce-8db9c08f41ca", "WHT");
			zGuidFindBoxColumnStyleInfo7.ColumnName = "AL_AW";
			zGuidFindBoxColumnStyleInfo7.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccWithholding;
			zGuidFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|6503105e-7756-4796-8099-2970219801f7", "WHT Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "AL_OSWHTAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|AC588BDA-F190-44A9-AEB7-03D66601CA4E", "Local WHT");
			zCalcEditColumnStyleInfo5.ColumnName = "AL_LocalWHTAmount";
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = "Decimals";
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|1726de33-be53-4978-8b8a-f9c33e8e9cc4", "Total", "Line Total (in Invoice Currency).");
			zCalcEditColumnStyleInfo6.ColumnName = "AL_OverseasTotal";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|d914f53f-22a9-49c8-9494-304c9e86481e", "Local Total");
			zCalcEditColumnStyleInfo7.ColumnName = "AL_LocalTotalAmount";
			zCalcEditColumnStyleInfo7.IsReadOnly = true;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|3f62739f-d7d2-40f8-b78d-2406e8e753df", "Local Amount");
			zCalcEditColumnStyleInfo8.ColumnName = "AL_LocalExTaxAmount";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|ba6df7fb-5858-4549-a9e4-e2bfa64058de", "Local Tax");
			zCalcEditColumnStyleInfo9.ColumnName = "AL_LocalTaxAmount";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|229ba9af-7904-48a2-b8de-886c94a690f7", "Final");
			zCheckBoxColumnStyleInfo1.ColumnName = "AL_IsFinalCharge";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|31ac054a-2726-446d-89ea-5b4bb176ea2e", "Consol #");
			zTextBoxColumnStyleInfo2.ColumnName = "ConsolIDFromApportionedCharge";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "AL_PreventInvoicePrintGrouping";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|2435E4CB-34A9-4DD3-82CF-E86129B4B7DC", "Create Compliance Document Record On Posting");
			zCheckBoxColumnStyleInfo3.ColumnName = "CreateComplianceDocumentRecordOnPosting";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "AL_OSExtraTaxAmount";
			zCalcEditColumnStyleInfo10.IsReadOnly = true;
			zCalcEditColumnStyleInfo10.IsVisible = false;
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.ColumnName = "AL_LocalExtraTaxAmount";
			zCalcEditColumnStyleInfo11.IsReadOnly = true;
			zCalcEditColumnStyleInfo11.IsVisible = false;
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.ColumnName = "AL_OSGSTAmount";
			zCalcEditColumnStyleInfo12.IsReadOnly = true;
			zCalcEditColumnStyleInfo12.IsVisible = false;
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|8fbf2ce2-b172-4f7b-8255-a75475fcf7d5", "Tax Inclusive Amount");
			zCalcEditColumnStyleInfo13.ColumnName = "GSTInclusiveAmount";
			zCalcEditColumnStyleInfo13.IsVisible = false;
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.ColumnName = "AL_LocalGSTAmount";
			zCalcEditColumnStyleInfo14.IsReadOnly = true;
			zCalcEditColumnStyleInfo14.IsVisible = false;
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo8.ColumnName = "AL_AG";
			zGuidFindBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|f4ef2cbb-3c44-428b-a60f-09681d0e0ffa", "Sequence");
			zCalcEditColumnStyleInfo15.ColumnName = "AL_Sequence";
			zCalcEditColumnStyleInfo15.Decimals = 0;
			zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|9c889a6c-4568-4fcd-948d-331be7798008", "Job Local Ref", "Job Local Reference", "");
			zTextBoxColumnStyleInfo3.ColumnName = "JobLocalReference";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo9.ColumnName = "AL_A9_VATClass";
			zGuidFindBoxColumnStyleInfo9.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccInvMsg;
			zGuidFindBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b85a6a58-9072-46e4-9dac-96b807a58bc0", "Tax Basis", "Tax Reporting Basis", "");
			zTextBoxColumnStyleInfo4.ColumnName = "TaxReportingBasisHumanReadableName";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo16.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo16.ColumnName = "AL_Calc_InputGSTVATRecoverablePercentage";
			zCalcEditColumnStyleInfo16.IsVisible = false;
			zCalcEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo17.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo17.ColumnName = "AL_OSTaxAmount_Recoverable";
			zCalcEditColumnStyleInfo17.IsVisible = false;
			zCalcEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo18.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo18.ColumnName = "AL_OSTaxAmount_NotRecoverable";
			zCalcEditColumnStyleInfo18.IsVisible = false;
			zCalcEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo19.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo19.ColumnName = "AL_LocalTaxAmount_NotRecoverable";
			zCalcEditColumnStyleInfo19.IsVisible = false;
			zCalcEditColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo20.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo20.ColumnName = "AL_LocalTaxAmount_Recoverable";
			zCalcEditColumnStyleInfo20.IsVisible = false;
			zCalcEditColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo21.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo21.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|1DC4444D-BBE4-495E-B3F3-BF3FC9106F40", "Reporting Period");
			zCalcEditColumnStyleInfo21.ColumnName = "ComplianceDocumentReportingPeriod";
			zCalcEditColumnStyleInfo21.ShowGroupSeparators = false;
			zCalcEditColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("da8d6aec-4dbd-48a4-b3fc-9b9972531816", "Local Client", "Job Local Client", "");
			zGuidFindBoxColumnStyleInfo10.ColumnName = "JobLocalClient";
			zGuidFindBoxColumnStyleInfo10.IsVisible = false;
			zGuidFindBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("76eb4bc6-9867-482f-99e9-397ddcb4051b", "Overseas Agent", "Job Overseas Agent", "");
			zGuidFindBoxColumnStyleInfo11.ColumnName = "JobOverseasAgent";
			zGuidFindBoxColumnStyleInfo11.IsVisible = false;
			zGuidFindBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo12.ColumnName = "AL_Calc_FirstSubClassParentId";
			zGuidFindBoxColumnStyleInfo12.IsVisible = false;
			zGuidFindBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "AL_Calc_FirstSubClassParent";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo13.ColumnName = "AL_Calc_SecondSubClassParentId";
			zGuidFindBoxColumnStyleInfo13.IsVisible = false;
			zGuidFindBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "AL_Calc_SecondSubClassParent";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|43B6E0C4-ED2D-4806-A46B-56BB3C94E3D0", "Organization");
			zGuidFindBoxColumnStyleInfo14.ColumnName = "ComplianceDocumentOrganization";
			zGuidFindBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zMultiLineTextBoxColumnInfo2.ColumnName = "JobConsolXMLData";
			zMultiLineTextBoxColumnInfo2.IsSortable = false;
			zMultiLineTextBoxColumnInfo2.IsVisible = false;
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "ImportedChargeCode";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo8.ColumnName = "ImportedChargeCodeXmlCode";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("58589ed2-0174-4b21-a132-afa33d2a687d", "Job Operator");
			zTextBoxColumnStyleInfo9.ColumnName = "Job+JH_GS_NKRepOps";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9f02f1c2-15da-4fa3-9e2a-6bf87d806581", "Government Charge Code");
			zTextBoxColumnStyleInfo10.ColumnName = "AL_GovtChargeCode";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|b846424e-f4c0-486d-a4d2-de122fd9b708", "Branch Name");
			zTextBoxColumnStyleInfo11.ColumnName = "BranchName";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|c60005b6-6597-4fcb-86c4-224cb7c575f9", "Department Description");
			zTextBoxColumnStyleInfo12.ColumnName = "DepartmentDescription";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|580F0F1F-503D-455D-82A0-617BA8792771", "Compliance Sub Type");
			zDropEditColumnStyleInfo3.ColumnName = "ComplianceSubType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("61c4fe54-3453-413b-9a31-8fadf09481a9", "Compliance Document Number");
			zTextBoxColumnStyleInfo13.ColumnName = "ComplianceDocumentNumber";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f620d0da-87cb-4e69-beca-3a1888abc8c3", "VAT Registration Number");
			zTextBoxColumnStyleInfo14.ColumnName = "ComplianceDocumentVATRegistrationNum";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8e0f97c2-b57a-43ad-bc0a-05418dafc5ac", "Supporting Doc Number");
			zTextBoxColumnStyleInfo15.ColumnName = "ComplianceSupportingDocumentNumber";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fb662cef-26db-44fe-be4c-33bd4971b768", "Related Job Number");
			zTextBoxColumnStyleInfo16.ColumnName = "AL_Calc_RelatedJobNumber";
			zTextBoxColumnStyleInfo16.IsReadOnly = true;
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|2bde49e0-b3ae-40ba-a1ef-642fef3a4db1", "Tax Branch Name");
			zTextBoxColumnStyleInfo17.ColumnName = "TaxBranchName";
			zTextBoxColumnStyleInfo17.IsReadOnly = true;
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "AL_PlaceOfSupply";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|dc7789fc-7612-4637-8109-0d6d4c54a39e", "Apportionment Method");
			zDropEditColumnStyleInfo5.ColumnName = "PeriodApportionmentMethod";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|54fde1c0-9a8f-4bac-83ea-ef7dd30f89ce", "Period Start");
			zDateEditColumnStyleInfo3.ColumnName = "PeriodStartDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|15aa23a6-6974-4252-9275-f5a723796ece", "Period End");
			zDateEditColumnStyleInfo4.ColumnName = "PeriodEndDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|674708eb-4cfa-4ae5-b358-ba95ec1e6cfe", "Clearing Account");
			zGuidFindBoxColumnStyleInfo15.ColumnName = "PeriodClearingGLAccountPK";
			zGuidFindBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|FFEE99AA-BA2C-440B-9551-BBA3428E88E8", "Supply Type");
			zDropEditColumnStyleInfo6.ColumnName = "AL_SupplyType";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7FE3DCD8-628C-4279-849A-90CA674B2607", "Alternate Account");
			zTextBoxColumnStyleInfo18.ColumnName = "AlternateGLAccountNumber";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9F07B1DC-42F5-401F-AD31-D10911E0E638", "Alternate Account Name");
			zTextBoxColumnStyleInfo19.ColumnName = "AlternateGLAccountDescription";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TransactionLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TransactionLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.TransactionLinesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.TransactionLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.TransactionLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TransactionLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo7);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.TransactionLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TransactionLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.TransactionLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo8);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo9);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo18);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo19);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo20);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo21);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo10);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo11);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo12);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo13);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo14);
			this.TransactionLinesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.TransactionLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.TransactionLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.TransactionLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.TransactionLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.TransactionLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo15);
			this.TransactionLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.TransactionLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.TransactionLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransactionLinesGrid.GridId = "0D7B402A-E683-41EB-B208-7EF5B34024C5";
			this.TransactionLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransactionLinesGrid.LayoutKey = "TransactionLinesGrid";
			this.TransactionLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TransactionLinesGrid.Name = "TransactionLinesGrid";
			this.TransactionLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 139, true);
			this.TransactionLinesGrid.TabIndex = 1;
			// 
			// zPanel1
			// 
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 45, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 100, true);
			this.zPanel1.TabIndex = 0;
			this.zPanel1.Visible = false;
			this.LineSummaryTabPage.PerformLayout();
			((ISupportInitialize)(this.TransactionLinesGrid)).EndInit();
			this.TransactionLinesGrid.ResumeLayout(false);
			this.TransactionLinesGrid.PerformLayout();
			this.LineSummaryTabPage.ResumeLayout(true);
		}

		void JobSummaryTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo22 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo23 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo24 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo25 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo26 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo27 = new ZCalcEditColumnStyleInfo();
			this.JobSummaryGrid = new ZGrid();
			this.JobSummaryTabPage.SuspendLayout();
			((ISupportInitialize)(this.JobSummaryGrid)).BeginInit();
			this.JobSummaryGrid.SuspendLayout();
			this.JobSummaryTabPage.Controls.Add(this.JobSummaryGrid);
			// 
			// JobSummaryGrid
			// 
			this.JobSummaryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.JobSummaryGrid, "InvoiceDependentJobs");
			this.JobSummaryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|fecf833e-2977-44d0-8a87-b3e6b32d3043", "Job #");
			zTextBoxColumnStyleInfo18.ColumnName = "JH_JobNum";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|87cca5ab-fa36-4e50-9479-a117a9997935", "House Bill #");
			zTextBoxColumnStyleInfo19.ColumnName = "JH_HouseBillNo";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo22.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo22.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|aef654d5-0ac2-4aee-a0e4-a7d597fcab14", "Amount");
			zCalcEditColumnStyleInfo22.ColumnName = "JH_RelatedInvoiceLinesCostAmountInInvoiceCurrency";
			zCalcEditColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo23.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo23.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|b5228634-92ba-44af-b976-9b7f57268cd4", "Tax");
			zCalcEditColumnStyleInfo23.ColumnName = "JH_RelatedInvoiceLinesCostTaxAmountInInvoiceCurrency";
			zCalcEditColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo24.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo24.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|37b98c5b-8592-42a2-be26-f29265648d33", "Total Amount", "Invoice Currency Total Amount", "");
			zCalcEditColumnStyleInfo24.ColumnName = "JH_RelatedInvoiceLinesTotalCostInInvoiceCurrency";
			zCalcEditColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|c1c5ab2c-3146-49b0-abb7-e79d2edd42cb", "Invoice Currency");
			zTextBoxColumnStyleInfo20.ColumnName = "JH_InvoiceCurrencyCode";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo25.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo25.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|65d0757d-4632-442a-a17e-aeaa4b04f83c", "Local Amount");
			zCalcEditColumnStyleInfo25.ColumnName = "JH_RelatedInvoiceLinesCostAmount";
			zCalcEditColumnStyleInfo25.IsVisible = false;
			zCalcEditColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo26.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo26.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|6a7fe18a-3a53-497e-b1fe-d148bd17ee87", "Local Tax");
			zCalcEditColumnStyleInfo26.ColumnName = "JH_RelatedInvoiceLinesCostTaxAmount";
			zCalcEditColumnStyleInfo26.IsVisible = false;
			zCalcEditColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo27.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo27.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|ee985a88-d0b8-4cbe-9756-40e1182d09a9", "Local Total Amount");
			zCalcEditColumnStyleInfo27.ColumnName = "JH_RelatedInvoiceLinesTotalCost";
			zCalcEditColumnStyleInfo27.IsVisible = false;
			zCalcEditColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.JobSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.JobSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.JobSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo22);
			this.JobSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo23);
			this.JobSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo24);
			this.JobSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.JobSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo25);
			this.JobSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo26);
			this.JobSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo27);
			this.JobSummaryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobSummaryGrid.GridId = "2bada939-40f7-4714-9b0d-636850708c02";
			this.JobSummaryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobSummaryGrid.LayoutKey = "JobSummaryGrid";
			this.JobSummaryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.JobSummaryGrid.Name = "JobSummaryGrid";
			this.JobSummaryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 139, true);
			this.JobSummaryGrid.TabIndex = 0;
			this.JobSummaryTabPage.PerformLayout();
			((ISupportInitialize)(this.JobSummaryGrid)).EndInit();
			this.JobSummaryGrid.ResumeLayout(false);
			this.JobSummaryGrid.PerformLayout();
			this.JobSummaryTabPage.ResumeLayout(true);
		}

		void TaxSummaryTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo28 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo29 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo30 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo31 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new ZTextBoxColumnStyleInfo();
			this.TaxSummaryGrid = new ZGrid();
			this.TaxSummaryTabPage.SuspendLayout();
			((ISupportInitialize)(this.TaxSummaryGrid)).BeginInit();
			this.TaxSummaryGrid.SuspendLayout();
			this.TaxSummaryTabPage.Controls.Add(this.TaxSummaryGrid);
			// 
			// TaxSummaryGrid
			// 
			this.TaxSummaryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TaxSummaryGrid, "InvoiceLineTaxSummaries");
			this.TaxSummaryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|aca3ee31-afe1-43e2-a674-fcdafb732be2", "Tax ID");
			zTextBoxColumnStyleInfo21.ColumnName = "TaxID";
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|d273e8c4-066d-4352-9ee3-f4b6129dacb6", "Tax Msg.");
			zTextBoxColumnStyleInfo22.ColumnName = "Message";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo28.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo28.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|3f62739f-d7d2-40f8-b78d-2406e8e753df", "Local Amount");
			zCalcEditColumnStyleInfo28.ColumnName = "LocalExTaxAmount";
			zCalcEditColumnStyleInfo28.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo29.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo29.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|ba6df7fb-5858-4549-a9e4-e2bfa64058de", "Local Tax");
			zCalcEditColumnStyleInfo29.ColumnName = "LocalTaxAmount";
			zCalcEditColumnStyleInfo29.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo30.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo30.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|d914f53f-22a9-49c8-9494-304c9e86481e", "Local Total");
			zCalcEditColumnStyleInfo30.ColumnName = "LocalTotalAmount";
			zCalcEditColumnStyleInfo30.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo31.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo31.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|e3687688-f7b0-4238-bf09-bcc5e54ee712", "Tax Rate");
			zCalcEditColumnStyleInfo31.ColumnName = "TaxRate";
			zCalcEditColumnStyleInfo31.IsVisible = false;
			zCalcEditColumnStyleInfo31.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo23.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceUserControl|584af291-72c8-481e-8c9b-5284c64e647f", "Tax ID Description");
			zTextBoxColumnStyleInfo23.ColumnName = "TaxIDDescription";
			zTextBoxColumnStyleInfo23.IsVisible = false;
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TaxSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.TaxSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.TaxSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo28);
			this.TaxSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo29);
			this.TaxSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo30);
			this.TaxSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo31);
			this.TaxSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.TaxSummaryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxSummaryGrid.GridId = "20ee445a-366f-4373-b90c-61eece2fc11d";
			this.TaxSummaryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TaxSummaryGrid.LayoutKey = "TaxSummaryGrid";
			this.TaxSummaryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TaxSummaryGrid.Name = "TaxSummaryGrid";
			this.TaxSummaryGrid.ReadOnly = true;
			this.TaxSummaryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 139, true);
			this.TaxSummaryGrid.TabIndex = 0;
			this.TaxSummaryTabPage.PerformLayout();
			((ISupportInitialize)(this.TaxSummaryGrid)).EndInit();
			this.TaxSummaryGrid.ResumeLayout(false);
			this.TaxSummaryGrid.PerformLayout();
			this.TaxSummaryTabPage.ResumeLayout(true);
		}

		void TaxTransactionSummaryTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.InvoiceTaxTransactionsControl = new InvoiceOtherTaxesControl();
			this.OtherTaxesBottomPanel = new ZPanel();
			this.RemoveTaxTransactionsButton = new ZButton();
			this.TaxTransactionSummaryTabPage.SuspendLayout();
			this.InvoiceTaxTransactionsControl.SuspendLayout();
			this.OtherTaxesBottomPanel.SuspendLayout();
			this.TaxTransactionSummaryTabPage.Controls.Add(this.InvoiceTaxTransactionsControl);
			this.TaxTransactionSummaryTabPage.Controls.Add(this.OtherTaxesBottomPanel);
			// 
			// InvoiceTaxTransactionsControl
			// 
			this.InvoiceTaxTransactionsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceTaxTransactionsControl, ".");
			this.InvoiceTaxTransactionsControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2EE053FB-DA3C-4209-9976-BAC734BB1EBB", "Invoice Tax Transactions Control");
			this.InvoiceTaxTransactionsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceTaxTransactionsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InvoiceTaxTransactionsControl.Name = "InvoiceTaxTransactionsControl";
			this.InvoiceTaxTransactionsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 117, true);
			this.InvoiceTaxTransactionsControl.TabIndex = 0;
			// 
			// OtherTaxesBottomPanel
			// 
			this.OtherTaxesBottomPanel.Controls.Add(this.RemoveTaxTransactionsButton);
			this.OtherTaxesBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.OtherTaxesBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 120, true);
			this.OtherTaxesBottomPanel.Name = "OtherTaxesBottomPanel";
			this.OtherTaxesBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 22, true);
			this.OtherTaxesBottomPanel.TabIndex = 1;
			// 
			// RemoveTaxTransactionsButton
			// 
			this.RemoveTaxTransactionsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5C70FC6C-44F2-4B45-BCD0-A5A421091941", "Edit Invoice && Lines", "Remove Tax Transactions and Edit Invoice & Lines");
			this.RemoveTaxTransactionsButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.RemoveTaxTransactionsButton.IsCaptionOverridden = false;
			this.RemoveTaxTransactionsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1101, 0, true);
			this.RemoveTaxTransactionsButton.Name = "RemoveTaxTransactionsButton";
			this.RemoveTaxTransactionsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RemoveTaxTransactionsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 22, true);
			this.RemoveTaxTransactionsButton.TabIndex = 0;
			this.RemoveTaxTransactionsButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RemoveTaxTransactionsButton.ToolTipCaption = null;
			this.RemoveTaxTransactionsButton.UseVisualStyleBackColor = true;
			this.RemoveTaxTransactionsButton.Click += new EventHandler(this.RemoveTaxTransactionsButton_Click);
			this.TaxTransactionSummaryTabPage.PerformLayout();
			this.InvoiceTaxTransactionsControl.ResumeLayout(true);
			this.InvoiceTaxTransactionsControl.PerformLayout();
			this.OtherTaxesBottomPanel.ResumeLayout(false);
			this.OtherTaxesBottomPanel.PerformLayout();
			this.TaxTransactionSummaryTabPage.ResumeLayout(true);
		}

		#endregion

	}
}
