using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Core.Forms;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Organisation.UserControls.Address;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class ComplianceDocumentUserControl
	{


		#region Component Designer generated code

		ZGroupBox HeaderGroupBox;
		ZTextBox DocumentNumTextBox;
		ZCheckBox CustomRelatedCheckBox;
		ZPanel LineSummaryGridPanel;
		public ZGrid TransactionLinesGrid;
		public ZGrid JobSummaryGrid;
		ZCalcEdit ReportingPeriodZCalcEdit;
		CollapsibleTableLayoutPanel InvoiceCollapsibleTableLayoutPanel;
		ZPanel zPanel1;
		ZPanel zPanel3;
		ZPanel zPanel5;
		ZPanel zPanel6;
		ZPanel zPanel7;
		ZPanel specialVoidingPanel;
		ZPanel zPanel2;
		ZPanel zPanel4;
		public ZAddressWithContactControl AddressWithContactControl;
		ZTextBox RegistrationNoTextBox;
		ZTextBox SupportingDocNumTextBox;
		ZDropEdit SupportingDocTypeDropEdit;
		ZCalcEdit PrintCountCalcEdit;
		ZGuidFindBox ComplianceBookGuidFindBox;
		ZTextBox DescTextbox;
		ZTextBox InternalReferencebox;
		ZCheckBox SpecialVoidingCheckBox;
		ZTextBox VoidingReasonbox;
		ZTextBox ApprovalNumberbox;
		ZDropEdit ComplianceSubTypeDropEdit;
		ZDateEdit DocumentDateEdit;
		ZDropEdit SupportingReasonDropEdit;
		ZDropEdit DocumentStatusDropEdit;
		public ZGroupBox zGroupBox1;
		ZDropEdit OrgCategoryDropEdit;
		ZPanel TotalPanel;
		ZGroupBox TotalGroupBox;
		ZCalcFindBox AmountCalcEdit;
		ZCalcFindBox TaxAmountCalcEdit;
		ZCalcFindBox TotalAmountCalcEdit;
		ZTabControl TabControl;
		public ZTabPage ComplianceDocumentLineSummaryTabPage;
		public ZTabPage TransactionLineSummaryTabPage;
		public ZGrid ComplianceDocumentLineGrid;
		public ZGrid TransactionLineGrid;
		public ZPostingButtonsUserControl PostingButtonsUserControl;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.HeaderGroupBox = new ZGroupBox();
			this.AddressWithContactControl = new ZAddressWithContactControl();
			this.InvoiceCollapsibleTableLayoutPanel = new CollapsibleTableLayoutPanel();
			this.zPanel1 = new ZPanel();
			this.OrgCategoryDropEdit = new ZDropEdit();
			this.RegistrationNoTextBox = new ZTextBox();
			this.zPanel3 = new ZPanel();
			this.ComplianceBookGuidFindBox = new ZGuidFindBox();
			this.DocumentDateEdit = new ZDateEdit();
			this.zPanel5 = new ZPanel();
			this.SupportingDocNumTextBox = new ZTextBox();
			this.SupportingDocTypeDropEdit = new ZDropEdit();
			this.PrintCountCalcEdit = new ZCalcEdit();
			this.zPanel6 = new ZPanel();
			this.zPanel7 = new ZPanel();
			this.specialVoidingPanel = new ZPanel();
			this.SpecialVoidingCheckBox = new ZCheckBox();
			this.VoidingReasonbox = new ZTextBox();
			this.ApprovalNumberbox = new ZTextBox();
			this.DocumentStatusDropEdit = new ZDropEdit();
			this.DescTextbox = new ZTextBox();
			this.InternalReferencebox = new ZTextBox();
			this.zPanel2 = new ZPanel();
			this.ComplianceSubTypeDropEdit = new ZDropEdit();
			this.CustomRelatedCheckBox = new ZCheckBox();
			this.DocumentNumTextBox = new ZTextBox();
			this.zPanel4 = new ZPanel();
			this.ReportingPeriodZCalcEdit = new ZCalcEdit();
			this.SupportingReasonDropEdit = new ZDropEdit();
			this.LineSummaryGridPanel = new ZPanel();
			this.TabControl = new ZTabControl();
			this.ComplianceDocumentLineSummaryTabPage = new ZTabPage();
			this.TransactionLineSummaryTabPage = new ZTabPage();
			this.TotalPanel = new ZPanel();
			this.TotalGroupBox = new ZGroupBox();
			this.AmountCalcEdit = new ZCalcFindBox();
			this.TaxAmountCalcEdit = new ZCalcFindBox();
			this.TotalAmountCalcEdit = new ZCalcFindBox();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HeaderGroupBox.SuspendLayout();
			this.AddressWithContactControl.SuspendLayout();
			this.InvoiceCollapsibleTableLayoutPanel.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.OrgCategoryDropEdit.SuspendLayout();
			this.zPanel3.SuspendLayout();
			this.ComplianceBookGuidFindBox.SuspendLayout();
			this.DocumentDateEdit.SuspendLayout();
			this.zPanel5.SuspendLayout();
			this.SupportingDocTypeDropEdit.SuspendLayout();
			this.zPanel6.SuspendLayout();
			this.DocumentStatusDropEdit.SuspendLayout();
			this.zPanel7.SuspendLayout();
			this.InternalReferencebox.SuspendLayout();
			this.specialVoidingPanel.SuspendLayout();
			this.SpecialVoidingCheckBox.SuspendLayout();
			this.VoidingReasonbox.SuspendLayout();
			this.ApprovalNumberbox.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.ComplianceSubTypeDropEdit.SuspendLayout();
			this.zPanel4.SuspendLayout();
			this.SupportingReasonDropEdit.SuspendLayout();
			this.LineSummaryGridPanel.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.TotalPanel.SuspendLayout();
			this.TotalGroupBox.SuspendLayout();
			this.AmountCalcEdit.SuspendLayout();
			this.TaxAmountCalcEdit.SuspendLayout();
			this.TotalAmountCalcEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(AccComplianceDocumentHeader);
			//
			// HeaderGroupBox
			//
			this.HeaderGroupBox.AutoSize = true;
			this.HeaderGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.HeaderGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ComplianceDocumentUserControl|3F8C5F7D-7908-4743-9CA4-69698D08962B", "Header Summary");
			this.HeaderGroupBox.Controls.Add(this.AddressWithContactControl);
			this.HeaderGroupBox.Controls.Add(this.InvoiceCollapsibleTableLayoutPanel);
			this.HeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.HeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderGroupBox.Name = "HeaderGroupBox";
			this.HeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 206, true);
			this.HeaderGroupBox.TabIndex = 0;
			this.HeaderGroupBox.TabStop = false;
			//
			// AddressWithContactControl
			//
			this.AddressWithContactControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressWithContactControl, "OrganisationAddressWithContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZAddressWithContact)(((AccComplianceDocumentHeader)(null)).OrganisationAddressWithContact)));
			this.AddressWithContactControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ComplianceDocumentUserControl|1F06D940-9766-416E-8ED1-09894B8E6A0A", "Debtor Information");
			this.AddressWithContactControl.ContactInfoTabVisible = true;
			this.AddressWithContactControl.InvoiceContactTabCaption = Enterprise.Accounting.GUI.Res.GetData("ZAddressWithContactControl|4D7CE633-04FA-4541-89C7-0BBF16109300", "Invoice Contact");
			this.AddressWithContactControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.AddressWithContactControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AddressWithContactControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AddressWithContactControl.Name = "OrganisationAddressWithContact";
			this.AddressWithContactControl.OnlyStopOnDebtor = true;
			this.AddressWithContactControl.PopupCaption = "";
			this.AddressWithContactControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AddressWithContactControl.TabIndex = 0;
			//
			// InvoiceCollapsibleTableLayoutPanel
			//
			this.InvoiceCollapsibleTableLayoutPanel.AutoSize = true;
			this.InvoiceCollapsibleTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.InvoiceCollapsibleTableLayoutPanel.ColumnCount = 1;
			this.InvoiceCollapsibleTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel1, 0, 0);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel3, 0, 2);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel5, 0, 4);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel6, 0, 5);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel7, 0, 6);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.specialVoidingPanel, 0, 7);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel2, 0, 1);
			this.InvoiceCollapsibleTableLayoutPanel.Controls.Add(this.zPanel4, 0, 3);
			this.InvoiceCollapsibleTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 19, true);
			this.InvoiceCollapsibleTableLayoutPanel.Name = "InvoiceCollapsibleTableLayoutPanel";
			this.InvoiceCollapsibleTableLayoutPanel.RowCount = 6;
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
			this.InvoiceCollapsibleTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 168, true);
			this.InvoiceCollapsibleTableLayoutPanel.TabIndex = 30;
			this.InvoiceCollapsibleTableLayoutPanel.VisibleChanged += new EventHandler(this.InvoiceCollapsibleTableLayoutPanel_VisibleChanged);
			//
			// zPanel1
			//
			this.zPanel1.Controls.Add(this.OrgCategoryDropEdit);
			this.zPanel1.Controls.Add(this.RegistrationNoTextBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.zPanel1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 26, true);
			this.zPanel1.TabIndex = 0;
			//
			// OrgCategoryDropEdit
			//
			this.OrgCategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrgCategoryDropEdit, "OrgHeaderCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccComplianceDocumentHeader)(null)).OrgHeaderCategory)));
			this.OrgCategoryDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7a8b646c-76af-4536-a6d2-7b6113029dcb", "Organization Category");
			this.OrgCategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 3, true);
			this.OrgCategoryDropEdit.Name = "OrgHeaderCategory";
			this.OrgCategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.OrgCategoryDropEdit.TabIndex = 0;
			//
			// RegistrationNoTextBox
			//
			this.BindingSource.SetBindingMember(this.RegistrationNoTextBox, "VATRegistrationNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccComplianceDocumentHeader)(null)).VATRegistrationNum)));
			this.RegistrationNoTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("613DC84E-E578-424D-A751-13F87BD71E74", "VAT Registration No.");
			this.RegistrationNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 3, true);
			this.RegistrationNoTextBox.Name = "VATRegistrationNum";
			this.RegistrationNoTextBox.ReadOnly = true;
			this.RegistrationNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 20, true);
			this.RegistrationNoTextBox.TabIndex = 1;
			//
			// zPanel3
			//
			this.zPanel3.Controls.Add(this.ComplianceBookGuidFindBox);
			this.zPanel3.Controls.Add(this.DocumentDateEdit);
			this.zPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 57, true);
			this.zPanel3.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel3.Name = "zPanel3";
			this.zPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 26, true);
			this.zPanel3.TabIndex = 2;
			//
			// ComplianceBookGuidFindBox
			//
			this.ComplianceBookGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComplianceBookGuidFindBox, "ADH_XD_ComplianceBook");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((AccComplianceDocumentHeader)(null)).ADH_XD_ComplianceBook)));
			this.ComplianceBookGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("279a6179-3610-4ded-bcea-47432d350e54", "Compliance Book");
			this.ComplianceBookGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ComplianceBookGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 3, true);
			this.ComplianceBookGuidFindBox.Name = "ADH_XD_ComplianceBook";
			this.ComplianceBookGuidFindBox.ShouldResize = true;
			this.ComplianceBookGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.ComplianceBookGuidFindBox.TabIndex = 5;
			//
			// DocumentDateEdit
			//
			this.DocumentDateEdit.AllowDrop = true;
			this.DocumentDateEdit.AutoCompleteMonthThreshold = 1;
			this.DocumentDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DocumentDateEdit, "ADH_DocumentDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccComplianceDocumentHeader)(null)).ADH_DocumentDate)));
			this.DocumentDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("B9EC0F89-1C8B-4BE1-8B97-63F393F543C1", "Document Date");
			this.DocumentDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 3, true);
			this.DocumentDateEdit.Name = "ADH_DocumentDate";
			this.DocumentDateEdit.TabIndex = 6;
			//
			// zPanel5
			//
			this.zPanel5.Controls.Add(this.SupportingDocNumTextBox);
			this.zPanel5.Controls.Add(this.SupportingDocTypeDropEdit);
			this.zPanel5.Controls.Add(this.PrintCountCalcEdit);
			this.zPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 113, true);
			this.zPanel5.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel5.Name = "zPanel5";
			this.zPanel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 26, true);
			this.zPanel5.TabIndex = 4;
			//
			// SupportingDocNumTextBox
			//
			this.SupportingDocNumTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SupportingDocNumTextBox, "ADH_SupportingDocumentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccComplianceDocumentHeader)(null)).ADH_SupportingDocumentNumber)));
			this.SupportingDocNumTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("15356566-936f-469f-8733-2df2246e9214", "Supporting Doc Number");
			this.SupportingDocNumTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SupportingDocNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 3, true);
			this.SupportingDocNumTextBox.Name = "ADH_SupportingDocumentNumber";
			this.SupportingDocNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 20, true);
			this.SupportingDocNumTextBox.TabIndex = 11;
			//
			// SupportingDocTypeDropEdit
			//
			this.SupportingDocTypeDropEdit.AllowDrop = true;
			this.SupportingDocTypeDropEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SupportingDocTypeDropEdit, "ADH_SupportingDocumentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccComplianceDocumentHeader)(null)).ADH_SupportingDocumentType)));
			this.SupportingDocTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4bc21b04-e7f9-4c30-bc47-c6e1e3d25aac", "Supporting Doc Type");
			this.SupportingDocTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SupportingDocTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 3, true);
			this.SupportingDocTypeDropEdit.Name = "ADH_SupportingDocumentType";
			this.SupportingDocTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.SupportingDocTypeDropEdit.TabIndex = 9;
			//
			// PrintCountCalcEdit
			//
			this.PrintCountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PrintCountCalcEdit, "ADH_PrintCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((AccComplianceDocumentHeader)(null)).ADH_PrintCount)));
			this.PrintCountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("A09F81E8-F3B8-48C5-8AA9-3E8AC535B487", "Print Count");
			this.PrintCountCalcEdit.DecimalPlaces = 0;
			this.PrintCountCalcEdit.Decimals = 0;
			this.PrintCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 2, true);
			this.PrintCountCalcEdit.Name = "ADH_PrintCount";
			this.PrintCountCalcEdit.ReadOnly = true;
			this.PrintCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.PrintCountCalcEdit.TabIndex = 10;
			this.PrintCountCalcEdit.Text = "0";
			this.PrintCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zPanel6
			//
			this.zPanel6.Controls.Add(this.DocumentStatusDropEdit);
			this.zPanel6.Controls.Add(this.DescTextbox);
			this.zPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 141, true);
			this.zPanel6.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel6.Name = "zPanel6";
			this.zPanel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 26, true);
			this.zPanel6.TabIndex = 5;
			//
			// DocumentStatusDropEdit
			//
			this.DocumentStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocumentStatusDropEdit, "ADH_DocumentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccComplianceDocumentHeader)(null)).ADH_DocumentStatus)));
			this.DocumentStatusDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("96537D0C-2F30-4C7E-89C3-83D406B5B188", "Document Status");
			this.DocumentStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 3, true);
			this.DocumentStatusDropEdit.Name = "ADH_DocumentStatus";
			this.DocumentStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 20, true);
			this.DocumentStatusDropEdit.TabIndex = 14;
			this.DocumentStatusDropEdit.ReadOnly = true;
			this.DocumentStatusDropEdit.TabStop = false;
			//
			// DescTextbox
			//
			this.DescTextbox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DescTextbox, "ADH_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccComplianceDocumentHeader)(null)).ADH_Description)));
			this.DescTextbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4a1095f9-b622-4050-9bae-78ce757de922", "Description");
			this.DescTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 3, true);
			this.DescTextbox.Name = "ADH_Description";
			this.DescTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.DescTextbox.TabIndex = 13;
			//
			// zPanel7
			//
			this.zPanel7.Controls.Add(this.InternalReferencebox);
			this.zPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 169, true);
			this.zPanel7.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel7.Name = "zPanel7";
			this.zPanel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 26, true);
			this.zPanel7.TabIndex = 6;
			//
			// InternalReferencebox
			//
			this.BindingSource.SetBindingMember(this.InternalReferencebox, "ADH_InternalReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccComplianceDocumentHeader)(null)).ADH_InternalReference)));
			this.InternalReferencebox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cc209013-2639-43a8-a006-e0e23ebc1926", "Internal Reference");
			this.InternalReferencebox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 3, true);
			this.InternalReferencebox.Name = "ADH_InternalReference";
			this.InternalReferencebox.ReadOnly = true;
			this.InternalReferencebox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 20, true);
			this.InternalReferencebox.TabIndex = 15;
			//
			// zPanel8
			//
			this.specialVoidingPanel.Controls.Add(this.SpecialVoidingCheckBox);
			this.specialVoidingPanel.Controls.Add(this.VoidingReasonbox);
			this.specialVoidingPanel.Controls.Add(this.ApprovalNumberbox);
			this.specialVoidingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.specialVoidingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 197, true);
			this.specialVoidingPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.specialVoidingPanel.Name = "specialVoidingPanel";
			this.specialVoidingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 26, true);
			this.specialVoidingPanel.Visible = false;
			this.specialVoidingPanel.TabIndex = 7;
			//
			// SpecialVoidingCheckBox
			//
			this.BindingSource.SetBindingMember(this.SpecialVoidingCheckBox, "IsSpecialVoiding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((AccComplianceDocumentHeader)(null)).IsSpecialVoiding)));
			this.SpecialVoidingCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ComplianceDocumentUserControl|44E16D39-0B0A-48EE-A253-AD8A76423109", "Special Voiding");
			this.SpecialVoidingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SpecialVoidingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 4, true);
			this.SpecialVoidingCheckBox.Name = "IsSpecialVoiding";
			this.SpecialVoidingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 18, true);
			this.SpecialVoidingCheckBox.TabIndex = 16;
			//
			// VoidingReasonbox
			//
			this.BindingSource.SetBindingMember(this.VoidingReasonbox, "ADH_VoidingReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccComplianceDocumentHeader)(null)).ADH_VoidingReason)));
			this.VoidingReasonbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("C5368C0C-F0AA-4AA7-97A7-EF5F3DFEA7B3", "Voiding Reason");
			this.VoidingReasonbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 3, true);
			this.VoidingReasonbox.Name = "ADH_VoidingReason";
			this.VoidingReasonbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 20, true);
			this.VoidingReasonbox.TabIndex = 17;
			//
			// ApprovalNumberbox
			//
			this.BindingSource.SetBindingMember(this.ApprovalNumberbox, "ADH_ApprovalNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccComplianceDocumentHeader)(null)).ADH_ApprovalNumber)));
			this.ApprovalNumberbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("FFC04C2E-9F63-4B39-BB9B-0FA564CA2EBC", "Approval Number");
			this.ApprovalNumberbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 3, true);
			this.ApprovalNumberbox.Name = "ADH_ApprovalNumber";
			this.ApprovalNumberbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 20, true);
			this.ApprovalNumberbox.TabIndex = 18;
			//
			// zPanel2
			//
			this.zPanel2.Controls.Add(this.ComplianceSubTypeDropEdit);
			this.zPanel2.Controls.Add(this.CustomRelatedCheckBox);
			this.zPanel2.Controls.Add(this.DocumentNumTextBox);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 29, true);
			this.zPanel2.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 26, true);
			this.zPanel2.TabIndex = 1;
			//
			// ComplianceSubTypeDropEdit
			//
			this.ComplianceSubTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComplianceSubTypeDropEdit, "ADH_ComplianceSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccComplianceDocumentHeader)(null)).ADH_ComplianceSubType)));
			this.ComplianceSubTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9756B780-9CB2-4DF7-8325-F604C531F31B", "Compliance Sub Type");
			this.ComplianceSubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 3, true);
			this.ComplianceSubTypeDropEdit.Name = "ADH_ComplianceSubType";
			this.ComplianceSubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.ComplianceSubTypeDropEdit.TabIndex = 2;
			//
			// CustomRelatedCheckBox
			//
			this.BindingSource.SetBindingMember(this.CustomRelatedCheckBox, "ADH_CustomRelated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((AccComplianceDocumentHeader)(null)).ADH_CustomRelated)));
			this.CustomRelatedCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ComplianceDocumentUserControl|92BBA3D2-C708-4DA6-B50F-0074C25F97F1", "Via Customs");
			this.CustomRelatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CustomRelatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 4, true);
			this.CustomRelatedCheckBox.Name = "ADH_CustomRelated";
			this.CustomRelatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 18, true);
			this.CustomRelatedCheckBox.TabIndex = 3;
			//
			// DocumentNumTextBox
			//
			this.BindingSource.SetBindingMember(this.DocumentNumTextBox, "ADH_DocumentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccComplianceDocumentHeader)(null)).ADH_DocumentNumber)));
			this.DocumentNumTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ComplianceDocumentUserControl|d490b3a6-58c2-490a-a5b2-f39ad9884b91", "Document Number");
			this.DocumentNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 3, true);
			this.DocumentNumTextBox.Name = "ADH_DocumentNumber";
			this.DocumentNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 20, true);
			this.DocumentNumTextBox.TabIndex = 4;
			//
			// zPanel4
			//
			this.zPanel4.Controls.Add(this.ReportingPeriodZCalcEdit);
			this.zPanel4.Controls.Add(this.SupportingReasonDropEdit);
			this.zPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 85, true);
			this.zPanel4.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.zPanel4.Name = "zPanel4";
			this.zPanel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 26, true);
			this.zPanel4.TabIndex = 3;
			//
			// ReportingPeriodZCalcEdit
			//
			this.BindingSource.SetBindingMember(this.ReportingPeriodZCalcEdit, "ADH_ReportingPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((AccComplianceDocumentHeader)(null)).ADH_ReportingPeriod)));
			this.ReportingPeriodZCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ComplianceDocumentUserControl|B51B34B7-854A-4C79-B8C3-50113CEC7834", "Reporting Period");
			this.ReportingPeriodZCalcEdit.DecimalPlaces = 0;
			this.ReportingPeriodZCalcEdit.Decimals = 0;
			this.ReportingPeriodZCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 3, true);
			this.ReportingPeriodZCalcEdit.Name = "ADH_ReportingPeriod";
			this.ReportingPeriodZCalcEdit.ShowGroupSeparators = false;
			this.ReportingPeriodZCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 20, true);
			this.ReportingPeriodZCalcEdit.TabIndex = 8;
			this.ReportingPeriodZCalcEdit.TabStop = false;
			this.ReportingPeriodZCalcEdit.Text = "0";
			this.ReportingPeriodZCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// SupportingReasonDropEdit
			//
			this.SupportingReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupportingReasonDropEdit, "ADH_SupportingReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccComplianceDocumentHeader)(null)).ADH_SupportingReason)));
			this.SupportingReasonDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("C39B03BE-99FF-4908-A538-DF2C36321D10", "Supporting Reason");
			this.SupportingReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 3, true);
			this.SupportingReasonDropEdit.Name = "ADH_SupportingReason";
			this.SupportingReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.SupportingReasonDropEdit.TabIndex = 7;
			//
			// LineSummaryGridPanel
			//
			this.LineSummaryGridPanel.Controls.Add(this.TabControl);
			this.LineSummaryGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LineSummaryGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 206, true);
			this.LineSummaryGridPanel.Name = "LineSummaryGridPanel";
			this.LineSummaryGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 304, true);
			this.LineSummaryGridPanel.TabIndex = 3;
			//
			// TabControl
			//
			this.TabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.ComplianceDocumentLineSummaryTabPage);
			this.TabControl.Controls.Add(this.TransactionLineSummaryTabPage);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 304, true);
			this.TabControl.TabIndex = 1;
			//
			// LineSummaryTabPage
			//
			this.ComplianceDocumentLineSummaryTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ComplianceDocumentLineSummaryTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ac1d3e1f-f923-4d02-8874-4da0afe2d4df", "Line Summary");
			this.ComplianceDocumentLineSummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ComplianceDocumentLineSummaryTabPage.Name = "LineSummaryTabPage";
			this.ComplianceDocumentLineSummaryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ComplianceDocumentLineSummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 277, true);
			this.ComplianceDocumentLineSummaryTabPage.TabIndex = 0;
			this.ComplianceDocumentLineSummaryTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.LineSummaryTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).ComplianceDocumentLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccComplianceDocumentLine)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).ComplianceDocumentLines)).SyncRoot)).Charge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccComplianceDocumentLine)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).ComplianceDocumentLines)).SyncRoot)).TaxID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((AccComplianceDocumentLine)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).ComplianceDocumentLines)).SyncRoot)).PostingGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((AccComplianceDocumentLine)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).ComplianceDocumentLines)).SyncRoot)).Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((AccComplianceDocumentLine)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).ComplianceDocumentLines)).SyncRoot)).TaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((AccComplianceDocumentLine)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).ComplianceDocumentLines)).SyncRoot)).TotalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccComplianceDocumentLine)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).ComplianceDocumentLines)).SyncRoot)).TaxMessage)));
			//
			// TransactionLineSummaryTabPage
			//
			this.TransactionLineSummaryTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.TransactionLineSummaryTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e5be6ec5-4d10-45fe-bda3-24df865802ca", "Transaction Line Summary");
			this.TransactionLineSummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TransactionLineSummaryTabPage.Name = "TransactionLineSummaryTabPage";
			this.TransactionLineSummaryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TransactionLineSummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 277, true);
			this.TransactionLineSummaryTabPage.TabIndex = 1;
			this.TransactionLineSummaryTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.TransactionLineSummaryTabPage_InitializeTab));
			//
			// TotalPanel
			//
			this.TotalPanel.Controls.Add(this.TotalGroupBox);
			this.TotalPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TotalPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 510, true);
			this.TotalPanel.Name = "TotalPanel";
			this.TotalPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 97, true);
			this.TotalPanel.TabIndex = 4;
			//
			// TotalGroupBox
			//
			this.TotalGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("61811294-b056-48bf-a9d7-1471afb8da12", "Total");
			this.TotalGroupBox.Controls.Add(this.AmountCalcEdit);
			this.TotalGroupBox.Controls.Add(this.TaxAmountCalcEdit);
			this.TotalGroupBox.Controls.Add(this.TotalAmountCalcEdit);
			this.TotalGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TotalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TotalGroupBox.Name = "TotalGroupBox";
			this.TotalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 97, true);
			this.TotalGroupBox.TabIndex = 2;
			this.TotalGroupBox.TabStop = false;
			//
			// AmountCalcEdit
			//
			this.AmountCalcEdit.AllowDrop = true;
			this.AmountCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AmountCalcEdit.BindToAmount = "Amount";
			this.AmountCalcEdit.BindToDecimalPlaces = "ADH_Calc_RXDecimals";
			this.AmountCalcEdit.BindToUnit = "ADH_Readonly_RXCode";
			this.AmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4A0CF1AA-F65A-4584-BFA3-AA5B7E8B8C6F", "Amount");
			this.AmountCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(851, 15, true);
			this.AmountCalcEdit.Name = "AmountCalcEdit";
			this.AmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.AmountCalcEdit.TabIndex = 0;
			//
			// TaxAmountCalcEdit
			//
			this.TaxAmountCalcEdit.AllowDrop = true;
			this.TaxAmountCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.TaxAmountCalcEdit.BindToAmount = "TaxAmount";
			this.TaxAmountCalcEdit.BindToDecimalPlaces = "ADH_Calc_RXDecimals";
			this.TaxAmountCalcEdit.BindToUnit = "ADH_Readonly_RXCode";
			this.TaxAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("71508023-BF7B-4995-B8AF-0E7EF4F9D3EE", "Tax Amount");
			this.TaxAmountCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.TaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(851, 41, true);
			this.TaxAmountCalcEdit.Name = "TaxAmountCalcEdit";
			this.TaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.TaxAmountCalcEdit.TabIndex = 1;
			//
			// TotalAmountCalcEdit
			//
			this.TotalAmountCalcEdit.AllowDrop = true;
			this.TotalAmountCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.TotalAmountCalcEdit.BindToAmount = "TotalAmount";
			this.TotalAmountCalcEdit.BindToDecimalPlaces = "ADH_Calc_RXDecimals";
			this.TotalAmountCalcEdit.BindToUnit = "ADH_Readonly_RXCode";
			this.TotalAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("42EF451C-B2D6-4C20-8516-ED25399A79DD", "Total Amount");
			this.TotalAmountCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.TotalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(851, 67, true);
			this.TotalAmountCalcEdit.Name = "TotalAmountCalcEdit";
			this.TotalAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.TotalAmountCalcEdit.TabIndex = 2;
			//
			// ComplianceDocumentUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LineSummaryGridPanel);
			this.Controls.Add(this.TotalPanel);
			this.Controls.Add(this.HeaderGroupBox);
			this.Name = "ComplianceDocumentUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 607, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.HeaderGroupBox.ResumeLayout(false);
			this.HeaderGroupBox.PerformLayout();
			this.AddressWithContactControl.ResumeLayout(true);
			this.AddressWithContactControl.PerformLayout();
			this.InvoiceCollapsibleTableLayoutPanel.ResumeLayout(false);
			this.InvoiceCollapsibleTableLayoutPanel.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.OrgCategoryDropEdit.ResumeLayout(true);
			this.OrgCategoryDropEdit.PerformLayout();
			this.zPanel3.ResumeLayout(false);
			this.zPanel3.PerformLayout();
			this.ComplianceBookGuidFindBox.ResumeLayout(true);
			this.ComplianceBookGuidFindBox.PerformLayout();
			this.DocumentDateEdit.ResumeLayout(true);
			this.DocumentDateEdit.PerformLayout();
			this.zPanel5.ResumeLayout(false);
			this.zPanel5.PerformLayout();
			this.SupportingDocTypeDropEdit.ResumeLayout(true);
			this.SupportingDocTypeDropEdit.PerformLayout();
			this.zPanel6.ResumeLayout(false);
			this.zPanel6.PerformLayout();
			this.DocumentStatusDropEdit.ResumeLayout(true);
			this.DocumentStatusDropEdit.PerformLayout();
			this.zPanel7.ResumeLayout(false);
			this.zPanel7.PerformLayout();
			this.InternalReferencebox.ResumeLayout(true);
			this.InternalReferencebox.PerformLayout();
			this.specialVoidingPanel.ResumeLayout(false);
			this.specialVoidingPanel.PerformLayout();
			this.VoidingReasonbox.ResumeLayout(true);
			this.VoidingReasonbox.PerformLayout();
			this.ApprovalNumberbox.ResumeLayout(true);
			this.ApprovalNumberbox.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.ComplianceSubTypeDropEdit.ResumeLayout(true);
			this.ComplianceSubTypeDropEdit.PerformLayout();
			this.zPanel4.ResumeLayout(false);
			this.zPanel4.PerformLayout();
			this.SupportingReasonDropEdit.ResumeLayout(true);
			this.SupportingReasonDropEdit.PerformLayout();
			this.LineSummaryGridPanel.ResumeLayout(false);
			this.LineSummaryGridPanel.PerformLayout();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.TotalPanel.ResumeLayout(false);
			this.TotalPanel.PerformLayout();
			this.TotalGroupBox.ResumeLayout(false);
			this.TotalGroupBox.PerformLayout();
			this.AmountCalcEdit.ResumeLayout(true);
			this.AmountCalcEdit.PerformLayout();
			this.TaxAmountCalcEdit.ResumeLayout(true);
			this.TaxAmountCalcEdit.PerformLayout();
			this.TotalAmountCalcEdit.ResumeLayout(true);
			this.TotalAmountCalcEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}