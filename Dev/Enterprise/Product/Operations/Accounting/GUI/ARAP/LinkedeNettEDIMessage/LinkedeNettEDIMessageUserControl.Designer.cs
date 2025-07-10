using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class LinkedeNettEDIMessageUserControl
	{
void InitializeComponent()
		{
			this.TabControl = new ZTabControl();
			this.MessageSummaryTabPage = new ZTabPage();
			this.OrganisationNameTextBox = new ZArchitecture.ZTextBox();
			this.OrganisationCodeTextBox = new ZArchitecture.ZTextBox();
			this.CurrencyTextBox = new ZArchitecture.ZTextBox();
			this.OSInvoiceAmtInclTaxTextBox = new ZArchitecture.ZTextBox();
			this.LocalInvoiceAmtInclTaxTextBox = new ZArchitecture.ZTextBox();
			this.PostDateTextBox = new ZArchitecture.ZTextBox();
			this.InvoiceDateTextBox = new ZArchitecture.ZTextBox();
			this.ChequeOrReferenceTextBox = new ZArchitecture.ZTextBox();
			this.JobInvoiceNoTextBox = new ZArchitecture.ZTextBox();
			this.TransactionNumberTextBox = new ZArchitecture.ZTextBox();
			this.TransactionTypeTextBox = new ZArchitecture.ZTextBox();
			this.LedgerTextBox = new ZArchitecture.ZTextBox();
			this.MessageTextTabPage = new ZTabPage();
			this.MessageTextTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.MessageSummaryTabPage.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Messaging.Business.EDIMessage);
			// 
			// TabControl
			// 
			this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.MessageSummaryTabPage);
			this.TabControl.Controls.Add(this.MessageTextTabPage);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 197, true);
			this.TabControl.TabIndex = 2;
			// 
			// MessageSummaryTabPage
			// 
			this.MessageSummaryTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.MessageSummaryTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("LinkedeNettEDIMessageUserControl|3baf1e94-38e8-4564-950b-9f597a91d3d5", "Message Summary");
			this.MessageSummaryTabPage.Controls.Add(this.OrganisationNameTextBox);
			this.MessageSummaryTabPage.Controls.Add(this.OrganisationCodeTextBox);
			this.MessageSummaryTabPage.Controls.Add(this.CurrencyTextBox);
			this.MessageSummaryTabPage.Controls.Add(this.OSInvoiceAmtInclTaxTextBox);
			this.MessageSummaryTabPage.Controls.Add(this.LocalInvoiceAmtInclTaxTextBox);
			this.MessageSummaryTabPage.Controls.Add(this.PostDateTextBox);
			this.MessageSummaryTabPage.Controls.Add(this.InvoiceDateTextBox);
			this.MessageSummaryTabPage.Controls.Add(this.ChequeOrReferenceTextBox);
			this.MessageSummaryTabPage.Controls.Add(this.JobInvoiceNoTextBox);
			this.MessageSummaryTabPage.Controls.Add(this.TransactionNumberTextBox);
			this.MessageSummaryTabPage.Controls.Add(this.TransactionTypeTextBox);
			this.MessageSummaryTabPage.Controls.Add(this.LedgerTextBox);
			this.MessageSummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageSummaryTabPage.Name = "MessageSummaryTabPage";
			this.MessageSummaryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageSummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 170, true);
			this.MessageSummaryTabPage.TabIndex = 0;
			// 
			// OrganisationNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.OrganisationNameTextBox, "EM_OrganisationName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.EDIMessage)(null)).EM_OrganisationName)));
			this.OrganisationNameTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("LinkedeNettEDIMessageUserControl|00d127d2-0cf7-4d5b-82c5-62f3c9874218", "Organization Name");
			this.OrganisationNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 133, true);
			this.OrganisationNameTextBox.Name = "OrganisationNameTextBox";
			this.OrganisationNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.OrganisationNameTextBox.TabIndex = 11;
			// 
			// OrganisationCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.OrganisationCodeTextBox, "EM_OrganisationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.EDIMessage)(null)).EM_OrganisationCode)));
			this.OrganisationCodeTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("LinkedeNettEDIMessageUserControl|281e27ce-757e-4485-a954-43da1636a2d4", "Organization Code");
			this.OrganisationCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 107, true);
			this.OrganisationCodeTextBox.Name = "OrganisationCodeTextBox";
			this.OrganisationCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.OrganisationCodeTextBox.TabIndex = 10;
			// 
			// CurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.CurrencyTextBox, "EM_Currency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.EDIMessage)(null)).EM_Currency)));
			this.CurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("LinkedeNettEDIMessageUserControl|c4b5b799-a2f2-4b02-b203-b543b45a1d4e", "Currency");
			this.CurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 81, true);
			this.CurrencyTextBox.Name = "CurrencyTextBox";
			this.CurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.CurrencyTextBox.TabIndex = 9;
			// 
			// OSInvoiceAmtInclTaxTextBox
			// 
			this.BindingSource.SetBindingMember(this.OSInvoiceAmtInclTaxTextBox, "EM_OSInvoiceAmtInclTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.EDIMessage)(null)).EM_OSInvoiceAmtInclTax)));
			this.OSInvoiceAmtInclTaxTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("LinkedeNettEDIMessageUserControl|39969a53-1a37-4b37-b4ad-ff008065c2c5", "OS Amount Inc Tax");
			this.OSInvoiceAmtInclTaxTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 55, true);
			this.OSInvoiceAmtInclTaxTextBox.Name = "OSInvoiceAmtInclTaxTextBox";
			this.OSInvoiceAmtInclTaxTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.OSInvoiceAmtInclTaxTextBox.TabIndex = 8;
			// 
			// LocalInvoiceAmtInclTaxTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalInvoiceAmtInclTaxTextBox, "EM_LocalInvoiceAmtInclTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.EDIMessage)(null)).EM_LocalInvoiceAmtInclTax)));
			this.LocalInvoiceAmtInclTaxTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("LinkedeNettEDIMessageUserControl|a6631f61-3e28-4227-82cb-4b2d899f4e95", "Local Amount Inc Tax");
			this.LocalInvoiceAmtInclTaxTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 29, true);
			this.LocalInvoiceAmtInclTaxTextBox.Name = "LocalInvoiceAmtInclTaxTextBox";
			this.LocalInvoiceAmtInclTaxTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.LocalInvoiceAmtInclTaxTextBox.TabIndex = 7;
			// 
			// PostDateTextBox
			// 
			this.BindingSource.SetBindingMember(this.PostDateTextBox, "EM_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.EDIMessage)(null)).EM_PostDate)));
			this.PostDateTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("LinkedeNettEDIMessageUserControl|be6224a1-ed03-41ad-bf99-8f0a68b09e86", "Post Date");
			this.PostDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 3, true);
			this.PostDateTextBox.Name = "PostDateTextBox";
			this.PostDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.PostDateTextBox.TabIndex = 6;
			// 
			// InvoiceDateTextBox
			// 
			this.BindingSource.SetBindingMember(this.InvoiceDateTextBox, "EM_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.EDIMessage)(null)).EM_InvoiceDate)));
			this.InvoiceDateTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("LinkedeNettEDIMessageUserControl|46211b03-bbbc-4879-ae29-18cfa38c3497", "Invoice Date");
			this.InvoiceDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 137, true);
			this.InvoiceDateTextBox.Name = "InvoiceDateTextBox";
			this.InvoiceDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.InvoiceDateTextBox.TabIndex = 5;
			// 
			// ChequeOrReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChequeOrReferenceTextBox, "EM_ChequeOrReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.EDIMessage)(null)).EM_ChequeOrReference)));
			this.ChequeOrReferenceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("LinkedeNettEDIMessageUserControl|a055c216-3504-4502-9901-40c41fac447b", "Check or Reference");
			this.ChequeOrReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 111, true);
			this.ChequeOrReferenceTextBox.Name = "ChequeOrReferenceTextBox";
			this.ChequeOrReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.ChequeOrReferenceTextBox.TabIndex = 4;
			// 
			// JobInvoiceNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.JobInvoiceNoTextBox, "EM_JobInvoiceNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.EDIMessage)(null)).EM_JobInvoiceNo)));
			this.JobInvoiceNoTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("LinkedeNettEDIMessageUserControl|12c15a81-d6ab-4852-8ca6-9ab6a5f19e22", "Job Number");
			this.JobInvoiceNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 85, true);
			this.JobInvoiceNoTextBox.Name = "JobInvoiceNoTextBox";
			this.JobInvoiceNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JobInvoiceNoTextBox.TabIndex = 3;
			// 
			// TransactionNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransactionNumberTextBox, "EM_TransactionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.EDIMessage)(null)).EM_TransactionNumber)));
			this.TransactionNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("LinkedeNettEDIMessageUserControl|42782fce-ecf5-434c-acab-e78d740ed101", "Transaction Number");
			this.TransactionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 59, true);
			this.TransactionNumberTextBox.Name = "TransactionNumberTextBox";
			this.TransactionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.TransactionNumberTextBox.TabIndex = 2;
			// 
			// TransactionTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransactionTypeTextBox, "EM_TransactionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.EDIMessage)(null)).EM_TransactionType)));
			this.TransactionTypeTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("LinkedeNettEDIMessageUserControl|d5f25eee-b244-4992-8f10-ee15a0699e94", "Transaction Type");
			this.TransactionTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 32, true);
			this.TransactionTypeTextBox.Name = "TransactionTypeTextBox";
			this.TransactionTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.TransactionTypeTextBox.TabIndex = 1;
			// 
			// LedgerTextBox
			// 
			this.BindingSource.SetBindingMember(this.LedgerTextBox, "EM_Ledger");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.EDIMessage)(null)).EM_Ledger)));
			this.LedgerTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("LinkedeNettEDIMessageUserControl|2c0d3d77-1dbc-457f-ac8a-f3faf7c7215e", "Ledger");
			this.LedgerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 6, true);
			this.LedgerTextBox.Name = "LedgerTextBox";
			this.LedgerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.LedgerTextBox.TabIndex = 0;
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.MessageTextTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("LinkedeNettEDIMessageUserControl|36b9f8ad-9c19-497c-95cf-fcd7c1f48055", "Message Text");
			this.MessageTextTabPage.Controls.Add(this.MessageTextTextBox);
			this.MessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTextTabPage.Name = "MessageTextTabPage";
			this.MessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 170, true);
			this.MessageTextTabPage.TabIndex = 1;
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "EM_MessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Messaging.Business.EDIMessage)(null)).EM_MessageText)));
			this.MessageTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 164, true);
			this.MessageTextTextBox.TabIndex = 1;
			// 
			// LinkedeNettEDIMessageUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TabControl);
			this.Name = "LinkedeNettEDIMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 197, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.MessageSummaryTabPage.ResumeLayout(false);
			this.MessageSummaryTabPage.PerformLayout();
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.ResumeLayout(false);
		}

	}
}