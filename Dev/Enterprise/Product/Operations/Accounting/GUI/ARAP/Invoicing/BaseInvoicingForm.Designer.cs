using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.InvoicingApproval;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI
{
	public partial class BaseInvoicingForm
	{

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new Container();
			this.MainTabControl = new ZTemplateTabControl();
			this.InvoiceDetailsTabPage = new ZTabPage();
			this.RelatedInvoicesTabPage = new ZTabPage();
			this.sourceXmlTabPage = new ZTabPage();
			this.zWorkflowTabPage1 = new ZWorkflowTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			this.ButtonsPanel = new ZPanel();
			this.ButtonsLayoutPanel = new KTableLayoutPanel();
			this.PostingButtonsUserControl = new ZPostingButtonsUserControl();
			this.CalculateTaxTransactionsButton = new ZButton();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.ButtonsLayoutPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 627, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 22, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1249);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(InvoicingBase);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.InvoiceDetailsTabPage);
			this.MainTabControl.Controls.Add(this.RelatedInvoicesTabPage);
			this.MainTabControl.Controls.Add(this.sourceXmlTabPage);
			this.MainTabControl.Controls.Add(this.zWorkflowTabPage1);
			this.MainTabControl.Controls.Add(this.zEventTabPage1);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 649, true);
			this.MainTabControl.TabIndex = 1;
			this.zWorkflowTabPage1.RunWhenBindingOrFirstShown(new EventHandler(this.zWorkflowTabPage1_InitializeTab));
			// 
			// InvoiceDetailsTabPage
			// 
			this.InvoiceDetailsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|e594bf95-ae92-467d-9a81-891a6255a78b", "XXXXXXX Details");
			this.InvoiceDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InvoiceDetailsTabPage.Name = "InvoiceDetailsTabPage";
			this.InvoiceDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 622, true);
			this.InvoiceDetailsTabPage.TabIndex = 0;
			this.InvoiceDetailsTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.InvoiceDetailsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).LineCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccTransactionLines)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).LineCharges)).SyncRoot)).JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((AccTransactionLines)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).LineCharges)).SyncRoot)).AL_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccTransactionLines)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).LineCharges)).SyncRoot)).AL_LineType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((AccTransactionLines)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).LineCharges)).SyncRoot)).AL_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((AccTransactionLines)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).LineCharges)).SyncRoot)).AL_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((AccTransactionLines)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).LineCharges)).SyncRoot)).AL_LineAmount)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransactionHeaderWithLines)(((InvoicingBase)(null)))));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodApportionmentLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PeriodApportionmentLine)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodApportionmentLines)).SyncRoot)).Period)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((PeriodApportionmentLine)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodApportionmentLines)).SyncRoot)).PeriodStart)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((PeriodApportionmentLine)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodApportionmentLines)).SyncRoot)).PeriodEnd)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PeriodApportionmentLine)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodApportionmentLines)).SyncRoot)).NumberOfDaysInPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PeriodApportionmentLine)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodApportionmentLines)).SyncRoot)).CurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PeriodApportionmentLine)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodApportionmentLines)).SyncRoot)).ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PeriodApportionmentLine)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodApportionmentLines)).SyncRoot)).OSAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PeriodApportionmentLine)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodApportionmentLines)).SyncRoot)).LocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PeriodApportionmentLine)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodApportionmentLines)).SyncRoot)).OSTaxNotRecoverable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PeriodApportionmentLine)(((System.Collections.IList)(((InvoicingLineBase)(((System.Collections.IList)(((InvoicingBase)(null)).FilteredLines)).SyncRoot)).PeriodApportionmentLines)).SyncRoot)).LocalTaxNotRecoverable)));
			// 
			// RelatedInvoicesTabPage
			// 
			this.RelatedInvoicesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.RelatedInvoicesTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|35323b85-ef74-4500-acf5-c1fc23fd6bff", "Related Invoices");
			this.RelatedInvoicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelatedInvoicesTabPage.Name = "RelatedInvoicesTabPage";
			this.RelatedInvoicesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RelatedInvoicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 622, true);
			this.RelatedInvoicesTabPage.TabIndex = 2;
			this.RelatedInvoicesTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.RelatedInvoicesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).AH_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).AH_FullyPaidDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).AH_OSTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).AH_OSTax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).OSOutstandingAmountMatching)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).AH_LocalTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingBase)(((System.Collections.IList)(((InvoicingBase)(null)).RelatedInvoices)).SyncRoot)).AH_GSTAmount)));
			// 
			// sourceXmlTabPage
			// 
			this.sourceXmlTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1a8750b8-f233-4dd1-93db-171c1b098982", "Imported XML");
			this.sourceXmlTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.sourceXmlTabPage.Name = "sourceXmlTabPage";
			this.sourceXmlTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.sourceXmlTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 622, true);
			this.sourceXmlTabPage.TabIndex = 4;
			this.sourceXmlTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.sourceXmlTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((UniversalTransactionWrapper)(((InvoicingBase)(null)).AllocationApprovalRequest.PostingDetails.UniversalTransaction)));
			// 
			// zWorkflowTabPage1
			// 
			this.zWorkflowTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zWorkflowTabPage1.Name = "zWorkflowTabPage1";
			this.zWorkflowTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.zWorkflowTabPage1.TabIndex = 3;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 622, true);
			this.zEventTabPage1.TabIndex = 1;
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.BackColor = System.Drawing.SystemColors.Control;
			this.ButtonsPanel.Controls.Add(this.ButtonsLayoutPanel);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPanel.ImeMode = System.Windows.Forms.ImeMode.On;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 649, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 37, true);
			this.ButtonsPanel.TabIndex = 6;
			// 
			// ButtonsLayoutPanel
			// 
			this.ButtonsLayoutPanel.AutoSize = true;
			this.ButtonsLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ButtonsLayoutPanel.ColumnCount = 2;
			this.ButtonsLayoutPanel.ColumnStyles.Add(new ColumnStyle(System.Windows.Forms.SizeType.Percent, 36.25F));
			this.ButtonsLayoutPanel.ColumnStyles.Add(new ColumnStyle(System.Windows.Forms.SizeType.Percent, 63.75F));
			this.ButtonsLayoutPanel.Controls.Add(this.PostingButtonsUserControl, 1, 0);
			this.ButtonsLayoutPanel.Controls.Add(this.CalculateTaxTransactionsButton, 0, 0);
			this.ButtonsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.ButtonsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(950, 0, true);
			this.ButtonsLayoutPanel.Name = "ButtonsLayoutPanel";
			this.ButtonsLayoutPanel.RowCount = 1;
			this.ButtonsLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ButtonsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 37, true);
			this.ButtonsLayoutPanel.TabIndex = 9;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 3, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 31, true);
			this.PostingButtonsUserControl.TabIndex = 7;
			// 
			// CalculateTaxTransactionsButton
			// 
			this.CalculateTaxTransactionsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("55E5C210-95A7-41DE-9678-50505C65409B", "Calculate Tax Transactions");
			this.CalculateTaxTransactionsButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.CalculateTaxTransactionsButton.IsCaptionOverridden = false;
			this.CalculateTaxTransactionsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CalculateTaxTransactionsButton.Name = "CalculateTaxTransactionsButton";
			this.CalculateTaxTransactionsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CalculateTaxTransactionsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 24, true);
			this.CalculateTaxTransactionsButton.TabIndex = 6;
			this.CalculateTaxTransactionsButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CalculateTaxTransactionsButton.ToolTipCaption = null;
			this.CalculateTaxTransactionsButton.UseVisualStyleBackColor = true;
			this.CalculateTaxTransactionsButton.Click += new EventHandler(this.CalculateTaxTransactionsButton_Click);
			// 
			// BaseInvoicingForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|99f5fa03-e202-43b6-9048-96195f31957f", "Base Invoice Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1350, 686, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.ButtonsPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(InvoicingBase);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase";
			this.IsPostOnly = true;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 725, true);
			this.Name = "BaseInvoicingForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.ButtonsPanel, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.ButtonsLayoutPanel.ResumeLayout(false);
			this.ButtonsLayoutPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1505: Avoid unmaintainable code", Justification = "Auto generated code")]
		void InvoiceDetailsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ChargeDetailsPanel = new ZPanel();
			this.AH_LocalExtraTaxAmountCalcEdit = new ZCalcFindBox();
			this.AH_OSExtraTaxAmountCalcEdit = new ZCalcFindBox();
			this.AH_LocalWHTAmountCalcEdit = new ZCalcFindBox();
			this.AH_OSWHTAmountCalcEdit = new ZCalcFindBox();
			this.AH_LocalTotalAmountCalcEdit = new ZCalcFindBox();
			this.AH_LocalTaxAmountCalcEdit = new ZCalcFindBox();
			this.AH_OSTotalAmountCalcEdit = new ZCalcFindBox();
			this.AH_OSTaxAmountCalcEdit = new ZCalcFindBox();
			this.MainPanel = new ZPanel();
			this.InvoiceDetails = new InvoiceUserControl();
			this.ReceiptPaymentOuterPanel = new ZPanel();
			this.ChargesAndApportionmentsTabControl = new ZTabControl();
			this.LineChargesTabPage = new ZTabPage();
			this.SubAccountsTabPage = new ZTabPage();
			this.SubAccountsControl = new SubAccountsControl();
			this.PeriodApportionmentTabPage = new ZTabPage();
			this.AH_OSSubTotalAmountCalcEdit = new ZCalcFindBox();
			this.AH_LocalSubTotalAmountCalcEdit = new ZCalcFindBox();
			this.AH_OSOtherTaxesAmountCalcEdit = new ZCalcFindBox();
			this.AH_LocalOtherTaxesAmountCalcEdit = new ZCalcFindBox();
			this.TaxAmountsPanel = new ZPanel();
			this.SubTotalAmountsPanel = new ZPanel();
			this.OtherTaxesAmountsPanel = new ZPanel();
			this.TotalInvoiceAmountsPanel = new ZPanel();
			this.InvoiceTotalsCollapsibleLayoutPanel = new CollapsibleTableLayoutPanel();
			this.OSSubTotalExTaxAmountCalcEdit = new ZCalcFindBox();
			this.LocalSubTotalExTaxAmountCalcEdit = new ZCalcFindBox();
			this.SubTotalExTaxPanel = new ZPanel();
			this.WHTPanel = new ZPanel();
			this.ExtraTaxPanel = new ZPanel();
			this.TotalAndUnallocatedTabControl = new ZTabControl();
			this.TotalTabPage = new ZTabPage();
			this.UnallocatedTabPage = new ZTabPage();
			this.OSExpectedTotalExTaxCalcEdit = new ZCalcFindBox();
			this.OSInvoiceTotalExTaxCalcEdit = new ZCalcFindBox();
			this.OSUnallocatedTotalExTaxCalcEdit = new ZCalcFindBox();
			this.ExcludingTaxPanel = new ZPanel();
			this.TaxTotalPanel = new ZPanel();
			this.UnallocatedCaptionPanel = new ZPanel();
			this.ExpectedTotalCaption = new ZLabel();
			this.InvoiceTotalCaption = new ZLabel();
			this.UnallocatedCaption = new ZLabel();
			this.UnallocatedFlowLayoutPanel = new KFlowLayoutPanel();
			this.OSInvoiceTotalTaxCalcEdit = new ZCalcFindBox();
			this.OSExpectedTotalTaxCalcEdit = new ZCalcFindBox();
			this.OSUnallocatedTotalTaxCalcEdit = new ZCalcFindBox();
			this.IncludingTaxTotalPanel = new ZPanel();
			this.OSInvoiceTotalIncTaxCalcEdit = new ZCalcFindBox();
			this.OSExpectedTotalIncTaxCalcEdit = new ZCalcFindBox();
			this.OSUnallocatedTotalIncTaxCalcEdit = new ZCalcFindBox();
			this.InvoiceDetailsTabPage.SuspendLayout();
			this.ChargeDetailsPanel.SuspendLayout();
			this.AH_LocalExtraTaxAmountCalcEdit.SuspendLayout();
			this.AH_OSExtraTaxAmountCalcEdit.SuspendLayout();
			this.AH_LocalWHTAmountCalcEdit.SuspendLayout();
			this.AH_OSWHTAmountCalcEdit.SuspendLayout();
			this.AH_LocalTotalAmountCalcEdit.SuspendLayout();
			this.AH_LocalTaxAmountCalcEdit.SuspendLayout();
			this.AH_OSTotalAmountCalcEdit.SuspendLayout();
			this.AH_OSTaxAmountCalcEdit.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.InvoiceDetails.SuspendLayout();
			this.SubAccountsTabPage.SuspendLayout();
			this.SubAccountsControl.SuspendLayout();
			this.ChargesAndApportionmentsTabControl.SuspendLayout();
			this.AH_OSSubTotalAmountCalcEdit.SuspendLayout();
			this.AH_LocalSubTotalAmountCalcEdit.SuspendLayout();
			this.AH_OSOtherTaxesAmountCalcEdit.SuspendLayout();
			this.AH_LocalOtherTaxesAmountCalcEdit.SuspendLayout();
			this.TaxAmountsPanel.SuspendLayout();
			this.SubTotalAmountsPanel.SuspendLayout();
			this.OtherTaxesAmountsPanel.SuspendLayout();
			this.TotalInvoiceAmountsPanel.SuspendLayout();
			this.InvoiceTotalsCollapsibleLayoutPanel.SuspendLayout();
			this.OSSubTotalExTaxAmountCalcEdit.SuspendLayout();
			this.LocalSubTotalExTaxAmountCalcEdit.SuspendLayout();
			this.SubTotalExTaxPanel.SuspendLayout();
			this.WHTPanel.SuspendLayout();
			this.ExtraTaxPanel.SuspendLayout();
			this.TotalAndUnallocatedTabControl.SuspendLayout();
			this.TotalTabPage.SuspendLayout();
			this.UnallocatedTabPage.SuspendLayout();
			this.OSExpectedTotalExTaxCalcEdit.SuspendLayout();
			this.OSInvoiceTotalExTaxCalcEdit.SuspendLayout();
			this.OSUnallocatedTotalExTaxCalcEdit.SuspendLayout();
			this.ExcludingTaxPanel.SuspendLayout();
			this.TaxTotalPanel.SuspendLayout();
			this.UnallocatedCaptionPanel.SuspendLayout();
			this.ExpectedTotalCaption.SuspendLayout();
			this.InvoiceTotalCaption.SuspendLayout();
			this.UnallocatedCaption.SuspendLayout();
			this.OSInvoiceTotalTaxCalcEdit.SuspendLayout();
			this.OSExpectedTotalTaxCalcEdit.SuspendLayout();
			this.OSUnallocatedTotalTaxCalcEdit.SuspendLayout();
			this.IncludingTaxTotalPanel.SuspendLayout();
			this.OSInvoiceTotalIncTaxCalcEdit.SuspendLayout();
			this.OSExpectedTotalIncTaxCalcEdit.SuspendLayout();
			this.OSUnallocatedTotalIncTaxCalcEdit.SuspendLayout();
			this.InvoiceDetailsTabPage.Controls.Add(this.MainPanel);
			this.InvoiceDetailsTabPage.Controls.Add(this.ReceiptPaymentOuterPanel);
			// 
			// ChargeDetailsPanel
			// 
			this.ChargeDetailsPanel.AutoSize = true;
			this.ChargeDetailsPanel.Controls.Add(this.TotalAndUnallocatedTabControl);
			this.ChargeDetailsPanel.Controls.Add(this.ChargesAndApportionmentsTabControl);
			this.ChargeDetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ChargeDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 401, true);
			this.ChargeDetailsPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 126, true);
			this.ChargeDetailsPanel.Name = "ChargeDetailsPanel";
			this.ChargeDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 221, true);
			this.ChargeDetailsPanel.TabIndex = 18;
			// 
			// AH_LocalExtraTaxAmountCalcEdit
			// 
			this.AH_LocalExtraTaxAmountCalcEdit.AllowDrop = true;
			this.AH_LocalExtraTaxAmountCalcEdit.BindToAmount = "AH_LocalExtraTaxAmount";
			this.AH_LocalExtraTaxAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.AH_LocalExtraTaxAmountCalcEdit.BindToUnit = "AH_Calc_LocalRX";
			this.AH_LocalExtraTaxAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceForm|D2F2E755-29A5-4B07-AF8B-6F00BADDB172", "{0} in Local Currency");
			this.AH_LocalExtraTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 4, true);
			this.AH_LocalExtraTaxAmountCalcEdit.Name = "AH_LocalExtraTaxAmountCalcEdit";
			this.AH_LocalExtraTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AH_LocalExtraTaxAmountCalcEdit.TabIndex = 34;
			// 
			// AH_OSExtraTaxAmountCalcEdit
			// 
			this.AH_OSExtraTaxAmountCalcEdit.AllowDrop = true;
			this.AH_OSExtraTaxAmountCalcEdit.BindToAmount = "AH_OSExtraTaxAmount";
			this.AH_OSExtraTaxAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.AH_OSExtraTaxAmountCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.AH_OSExtraTaxAmountCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_OSExtraTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 3, true);
			this.AH_OSExtraTaxAmountCalcEdit.Name = "AH_OSExtraTaxAmountCalcEdit";
			this.AH_OSExtraTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AH_OSExtraTaxAmountCalcEdit.TabIndex = 33;
			// 
			// AH_LocalWHTAmountCalcEdit
			// 
			this.AH_LocalWHTAmountCalcEdit.AllowDrop = true;
			this.AH_LocalWHTAmountCalcEdit.BindToAmount = "AH_LocalWHTAmount";
			this.AH_LocalWHTAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.AH_LocalWHTAmountCalcEdit.BindToUnit = "AH_Calc_LocalRX";
			this.AH_LocalWHTAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceForm|efa8ad9a-be76-4233-a2a4-2f7b2c67fca7", "Total Withholding Tax Amount in local currency");
			this.AH_LocalWHTAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 3, true);
			this.AH_LocalWHTAmountCalcEdit.Name = "AH_LocalWHTAmountCalcEdit";
			this.AH_LocalWHTAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AH_LocalWHTAmountCalcEdit.TabIndex = 32;
			// 
			// AH_OSWHTAmountCalcEdit
			// 
			this.AH_OSWHTAmountCalcEdit.AllowDrop = true;
			this.AH_OSWHTAmountCalcEdit.BindToAmount = "AH_OSWHTAmount";
			this.AH_OSWHTAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.AH_OSWHTAmountCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.AH_OSWHTAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|565bea32-777e-4ab7-9ad2-497f6c10f12a", "WHT Amount:");
			this.AH_OSWHTAmountCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_OSWHTAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 3, true);
			this.AH_OSWHTAmountCalcEdit.Name = "AH_OSWHTAmountCalcEdit";
			this.AH_OSWHTAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AH_OSWHTAmountCalcEdit.TabIndex = 31;
			// 
			// AH_LocalTotalAmountCalcEdit
			// 
			this.AH_LocalTotalAmountCalcEdit.AllowDrop = true;
			this.AH_LocalTotalAmountCalcEdit.BindToAmount = "AH_LocalTotalAmount";
			this.AH_LocalTotalAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.AH_LocalTotalAmountCalcEdit.BindToUnit = "AH_Calc_LocalRX";
			this.AH_LocalTotalAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceForm|976dbca4-569b-4903-842e-b0f610d3d84f", "Total Invoice in Local Currency");
			this.AH_LocalTotalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 3, true);
			this.AH_LocalTotalAmountCalcEdit.Name = "AH_LocalTotalAmountCalcEdit";
			this.AH_LocalTotalAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AH_LocalTotalAmountCalcEdit.TabIndex = 27;
			// 
			// AH_LocalTaxAmountCalcEdit
			// 
			this.AH_LocalTaxAmountCalcEdit.AllowDrop = true;
			this.AH_LocalTaxAmountCalcEdit.BindToAmount = "AH_LocalTaxAmount";
			this.AH_LocalTaxAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.AH_LocalTaxAmountCalcEdit.BindToUnit = "AH_Calc_LocalRX";
			this.AH_LocalTaxAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceForm|64366edc-8ac6-4c33-b2ed-7abfc2402008", "{0} in Local Currency");
			this.AH_LocalTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 3, true);
			this.AH_LocalTaxAmountCalcEdit.Name = "AH_LocalTaxAmountCalcEdit";
			this.AH_LocalTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AH_LocalTaxAmountCalcEdit.TabIndex = 24;
			// 
			// AH_OSTotalAmountCalcEdit
			// 
			this.AH_OSTotalAmountCalcEdit.AllowDrop = true;
			this.AH_OSTotalAmountCalcEdit.BindToAmount = "AH_OSTotalAmount";
			this.AH_OSTotalAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.AH_OSTotalAmountCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.AH_OSTotalAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("dc3a9640-37aa-4c2b-8f37-60b7b6663298", "Total Invoice");
			this.AH_OSTotalAmountCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_OSTotalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 3, true);
			this.AH_OSTotalAmountCalcEdit.Name = "AH_OSTotalAmountCalcEdit";
			this.AH_OSTotalAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AH_OSTotalAmountCalcEdit.TabIndex = 23;
			// 
			// AH_OSTaxAmountCalcEdit
			// 
			this.AH_OSTaxAmountCalcEdit.AllowDrop = true;
			this.AH_OSTaxAmountCalcEdit.BindToAmount = "AH_OSTaxAmount";
			this.AH_OSTaxAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.AH_OSTaxAmountCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.AH_OSTaxAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8a2e7c0b-86fc-4d7c-9cb8-47e3b018018d", "{0}");
			this.AH_OSTaxAmountCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_OSTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 3, true);
			this.AH_OSTaxAmountCalcEdit.Name = "AH_OSTaxAmountCalcEdit";
			this.AH_OSTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AH_OSTaxAmountCalcEdit.TabIndex = 20;
			// 
			// OSSubTotalExTaxAmountCalcEdit
			// 
			this.OSSubTotalExTaxAmountCalcEdit.AllowDrop = true;
			this.OSSubTotalExTaxAmountCalcEdit.BindToAmount = "AH_OSExTaxAmount";
			this.OSSubTotalExTaxAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.OSSubTotalExTaxAmountCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.OSSubTotalExTaxAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9ad4a120-5bec-4c51-894d-8be6f0306079", "Sub Total Excl. Tax");
			this.OSSubTotalExTaxAmountCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSSubTotalExTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 3, true);
			this.OSSubTotalExTaxAmountCalcEdit.Name = "OSSubTotalExTaxAmountCalcEdit";
			this.OSSubTotalExTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.OSSubTotalExTaxAmountCalcEdit.TabIndex = 20;
			// 
			// LocalSubTotalExTaxAmountCalcEdit
			// 
			this.LocalSubTotalExTaxAmountCalcEdit.AllowDrop = true;
			this.LocalSubTotalExTaxAmountCalcEdit.BindToAmount = "AH_LocalExTaxAmount";
			this.LocalSubTotalExTaxAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.LocalSubTotalExTaxAmountCalcEdit.BindToUnit = "AH_Calc_LocalRX";
			this.LocalSubTotalExTaxAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b9c94fd0-2d41-4b37-887d-6b2f2fb17a81", "Sub Total Excl. Tax in Local Currency");
			this.LocalSubTotalExTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 3, true);
			this.LocalSubTotalExTaxAmountCalcEdit.Name = "LocalSubTotalExTaxAmountCalcEdit";
			this.LocalSubTotalExTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.LocalSubTotalExTaxAmountCalcEdit.TabIndex = 24;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.InvoiceDetails);
			this.MainPanel.Controls.Add(this.ChargeDetailsPanel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 622, true);
			this.MainPanel.TabIndex = 5;
			// 
			// InvoiceDetails
			// 
			this.InvoiceDetails.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceDetails, ".");
			this.InvoiceDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceDetails.Name = "InvoiceDetails";
			this.InvoiceDetails.ReadOnly = false;
			this.InvoiceDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 401, true);
			this.InvoiceDetails.TabIndex = 0;
			this.InvoiceDetails.Load += InvoiceDetails_Load;
			// 
			// ReceiptPaymentOuterPanel
			// 
			this.ReceiptPaymentOuterPanel.AutoSize = true;
			this.ReceiptPaymentOuterPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ReceiptPaymentOuterPanel.BackColor = System.Drawing.SystemColors.Control;
			this.ReceiptPaymentOuterPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ReceiptPaymentOuterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 622, true);
			this.ReceiptPaymentOuterPanel.Name = "ReceiptPaymentOuterPanel";
			this.ReceiptPaymentOuterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 0, true);
			this.ReceiptPaymentOuterPanel.TabIndex = 4;
			this.ReceiptPaymentOuterPanel.Visible = false;
			// 
			// ChargesAndApportionmentsTabControl
			// 
			this.ChargesAndApportionmentsTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ChargesAndApportionmentsTabControl.Controls.Add(this.LineChargesTabPage);
			this.ChargesAndApportionmentsTabControl.Controls.Add(this.PeriodApportionmentTabPage);
			this.ChargesAndApportionmentsTabControl.Controls.Add(this.SubAccountsTabPage);
			this.ChargesAndApportionmentsTabControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.ChargesAndApportionmentsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargesAndApportionmentsTabControl.Name = "ChargesAndApportionmentsTabControl";
			this.ChargesAndApportionmentsTabControl.SelectedIndex = 0;
			this.ChargesAndApportionmentsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 221, true);
			this.ChargesAndApportionmentsTabControl.TabIndex = 17;
			// 
			// LineChargesTabPage
			// 
			this.LineChargesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.LineChargesTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|8A69DFCC-B1A6-45FD-8D09-905E8A03941B", "Line Charges");
			this.LineChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LineChargesTabPage.Name = "LineChargesTabPage";
			this.LineChargesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 194, true);
			this.LineChargesTabPage.TabIndex = 1;
			this.LineChargesTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.LineChargesTabPage_InitializeTab));
			// 
			// SubAccountsTabPage
			//
			this.SubAccountsTabPage.Controls.Add(SubAccountsControl);
			this.SubAccountsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.SubAccountsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|b9eb9eac-9f72-421f-a2ff-c5c5fed1dbcc", "Sub Accounts");
			this.SubAccountsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SubAccountsTabPage.Name = "SubAccountsTabPage";
			this.SubAccountsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SubAccountsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 194, true);
			this.SubAccountsTabPage.TabIndex = 3;
			// 
			// SubAccountsControl
			// 
			this.SubAccountsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubAccountsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SubAccountsControl.Name = "SubAccountsControl";
			// 
			// PeriodApportionmentTabPage
			// 
			this.PeriodApportionmentTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.PeriodApportionmentTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|C15509BA-EAD6-4FCF-B7C6-321F358A6A2E", "Period Apportionment");
			this.PeriodApportionmentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PeriodApportionmentTabPage.Name = "PeriodApportionmentTabPage";
			this.PeriodApportionmentTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PeriodApportionmentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 194, true);
			this.PeriodApportionmentTabPage.TabIndex = 2;
			this.PeriodApportionmentTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.PeriodApportionmentTabPage_InitializeTab));
			// 
			// TotalAndUnallocatedTabControl
			// 
			this.TotalAndUnallocatedTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TotalAndUnallocatedTabControl.Controls.Add(this.TotalTabPage);
			this.TotalAndUnallocatedTabControl.Controls.Add(this.UnallocatedTabPage);
			this.TotalAndUnallocatedTabControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.TotalAndUnallocatedTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(650, 0, true);
			this.TotalAndUnallocatedTabControl.Name = "TotalAndUnallocatedTabControl";
			this.TotalAndUnallocatedTabControl.SelectedIndex = 0;
			this.TotalAndUnallocatedTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 135, true);
			this.TotalAndUnallocatedTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 135, true);
			this.TotalAndUnallocatedTabControl.TabIndex = 18;
			// 
			// TotalTabPage
			//
			this.TotalTabPage.Controls.Add(this.ExtraTaxPanel);
			this.TotalTabPage.Controls.Add(this.WHTPanel);
			this.TotalTabPage.Controls.Add(this.InvoiceTotalsCollapsibleLayoutPanel);
			this.TotalTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.TotalTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|0e9fcb57-52be-4bc3-8556-9e2fbb8dcb4e", "Totals");
			this.TotalTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TotalTabPage.Name = "TotalTabPage";
			this.TotalTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TotalTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 221, true);
			this.TotalTabPage.TabIndex = 0;
			// 
			// UnallocatedTabPage
			//
			this.UnallocatedTabPage.Controls.Add(this.UnallocatedFlowLayoutPanel);
			this.UnallocatedTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.UnallocatedTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|5991aaf8-40c4-4a9d-98c6-2b088f9f4fb6", "Unallocated");
			this.UnallocatedTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.UnallocatedTabPage.Name = "UnallocatedTabPage";
			this.UnallocatedTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UnallocatedTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 221, true);
			this.UnallocatedTabPage.TabIndex = 1;
			this.UnallocatedTabPage.TabVisible = false;
			// 
			// UnallocatedFlowLayoutPanel
			// 
			this.UnallocatedFlowLayoutPanel.FlowDirection = FlowDirection.TopDown;
			this.UnallocatedFlowLayoutPanel.Controls.Add(this.UnallocatedCaptionPanel);
			this.UnallocatedFlowLayoutPanel.Controls.Add(this.ExcludingTaxPanel);
			this.UnallocatedFlowLayoutPanel.Controls.Add(this.TaxTotalPanel);
			this.UnallocatedFlowLayoutPanel.Controls.Add(this.IncludingTaxTotalPanel);
			this.UnallocatedFlowLayoutPanel.Name = "UnallocatedFlowLayoutPanel";
			this.UnallocatedFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// OSExpectedTotalExTaxCalcEdit
			// 
			this.OSExpectedTotalExTaxCalcEdit.AllowDrop = true;
			this.OSExpectedTotalExTaxCalcEdit.BindToAmount = "ExpectedInvoiceExclTaxTotal";
			this.OSExpectedTotalExTaxCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.OSExpectedTotalExTaxCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.OSExpectedTotalExTaxCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("44e1eccf-1d60-4d49-8840-12f9d59f9183", "Excluding Tax");
			this.OSExpectedTotalExTaxCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSExpectedTotalExTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 3, true);
			this.OSExpectedTotalExTaxCalcEdit.Name = "OSExpectedTotalExTaxCalcEdit";
			this.OSExpectedTotalExTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.OSExpectedTotalExTaxCalcEdit.TabIndex = 21;
			// 
			// OSInvoiceTotalExTaxCalcEdit
			// 
			this.OSInvoiceTotalExTaxCalcEdit.AllowDrop = true;
			this.OSInvoiceTotalExTaxCalcEdit.BindToAmount = "AH_OSExTaxAmount";
			this.OSInvoiceTotalExTaxCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.OSInvoiceTotalExTaxCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.OSInvoiceTotalExTaxCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSInvoiceTotalExTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 3, true);
			this.OSInvoiceTotalExTaxCalcEdit.Name = "OSInvoiceTotalExTaxCalcEdit";
			this.OSInvoiceTotalExTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.OSInvoiceTotalExTaxCalcEdit.TabIndex = 22;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OSInvoiceTotalExTaxCalcEdit, false);
			// 
			// OSUnallocatedTotalExTaxCalcEdit
			// 
			this.OSUnallocatedTotalExTaxCalcEdit.AllowDrop = true;
			this.OSUnallocatedTotalExTaxCalcEdit.BindToAmount = "UnallocatedInvoiceExclTaxTotal";
			this.OSUnallocatedTotalExTaxCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.OSUnallocatedTotalExTaxCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.OSUnallocatedTotalExTaxCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSUnallocatedTotalExTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 3, true);
			this.OSUnallocatedTotalExTaxCalcEdit.Name = "OSUnallocatedTotalExTaxCalcEdit";
			this.OSUnallocatedTotalExTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.OSUnallocatedTotalExTaxCalcEdit.TabIndex = 23;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OSUnallocatedTotalExTaxCalcEdit, false);
			// 
			// UnallocatedCaptionPanel
			//
			this.UnallocatedCaptionPanel.AutoSize = true;
			this.UnallocatedCaptionPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.UnallocatedCaptionPanel.Controls.Add(this.ExpectedTotalCaption);
			this.UnallocatedCaptionPanel.Controls.Add(this.InvoiceTotalCaption);
			this.UnallocatedCaptionPanel.Controls.Add(this.UnallocatedCaption);
			this.UnallocatedCaptionPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.UnallocatedCaptionPanel.Name = "UnallocatedCaptionPanel";
			this.UnallocatedCaptionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 20, true);
			this.UnallocatedCaptionPanel.TabIndex = 25;
			// 
			// ExpectedTotalCaption
			//
			this.ExpectedTotalCaption.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6e333a41-981e-4b01-9a26-c0bb5b88758d", "Expected Total");
			this.ExpectedTotalCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 0, true);
			this.ExpectedTotalCaption.Name = "ExpectedTotalCaption";
			this.ExpectedTotalCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ExpectedTotalCaption.TabIndex = 25;
			// 
			// InvoiceTotalCaption
			//
			this.InvoiceTotalCaption.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("aad324ca-736e-4ca5-83b7-0d9e7d9721af", "Invoice Total");
			this.InvoiceTotalCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 0, true);
			this.InvoiceTotalCaption.Name = "InvoiceTotalCaption";
			this.InvoiceTotalCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.InvoiceTotalCaption.TabIndex = 25;
			// 
			// UnallocatedCaption
			//
			this.UnallocatedCaption.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c7e8876b-0b0a-44cc-a946-8241c5a1af86", "Unallocated");
			this.UnallocatedCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 0, true);
			this.UnallocatedCaption.Name = "UnallocatedCaption";
			this.UnallocatedCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.UnallocatedCaption.TabIndex = 25;
			// 
			// ExcludingTaxPanel
			//
			this.ExcludingTaxPanel.AutoSize = true;
			this.ExcludingTaxPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ExcludingTaxPanel.Controls.Add(this.OSInvoiceTotalExTaxCalcEdit);
			this.ExcludingTaxPanel.Controls.Add(this.OSExpectedTotalExTaxCalcEdit);
			this.ExcludingTaxPanel.Controls.Add(this.OSUnallocatedTotalExTaxCalcEdit);
			this.ExcludingTaxPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ExcludingTaxPanel.Name = "ExcludingTaxPanel";
			this.ExcludingTaxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 23, true);
			this.ExcludingTaxPanel.TabIndex = 25;
			// 
			// TaxTotalPanel
			//
			this.TaxTotalPanel.AutoSize = true;
			this.TaxTotalPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TaxTotalPanel.Controls.Add(this.OSInvoiceTotalTaxCalcEdit);
			this.TaxTotalPanel.Controls.Add(this.OSExpectedTotalTaxCalcEdit);
			this.TaxTotalPanel.Controls.Add(this.OSUnallocatedTotalTaxCalcEdit);
			this.TaxTotalPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TaxTotalPanel.Name = "TaxTotalPanel";
			this.TaxTotalPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 23, true);
			this.TaxTotalPanel.TabIndex = 26;
			// 
			// OSInvoiceTotalTaxCalcEdit
			// 
			this.OSInvoiceTotalTaxCalcEdit.AllowDrop = true;
			this.OSInvoiceTotalTaxCalcEdit.BindToAmount = "AH_OSTaxAmount";
			this.OSInvoiceTotalTaxCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.OSInvoiceTotalTaxCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.OSInvoiceTotalTaxCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSInvoiceTotalTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 3, true);
			this.OSInvoiceTotalTaxCalcEdit.Name = "OSInvoiceTotalTaxCalcEdit";
			this.OSInvoiceTotalTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.OSInvoiceTotalTaxCalcEdit.TabIndex = 22;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OSInvoiceTotalTaxCalcEdit, false);
			// 
			// OSExpectedTotalTaxCalcEdit
			// 
			this.OSExpectedTotalTaxCalcEdit.AllowDrop = true;
			this.OSExpectedTotalTaxCalcEdit.BindToAmount = "ExpectedInvoiceTaxTotal";
			this.OSExpectedTotalTaxCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.OSExpectedTotalTaxCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.OSExpectedTotalTaxCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("103f5ed4-d91e-40ca-8dce-5be9c8b45bb1", "Tax");
			this.OSExpectedTotalTaxCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSExpectedTotalTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 3, true);
			this.OSExpectedTotalTaxCalcEdit.Name = "OSExpectedTotalTaxCalcEdit";
			this.OSExpectedTotalTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.OSExpectedTotalTaxCalcEdit.TabIndex = 21;
			// 
			// OSUnallocatedTotalTaxCalcEdit
			// 
			this.OSUnallocatedTotalTaxCalcEdit.AllowDrop = true;
			this.OSUnallocatedTotalTaxCalcEdit.BindToAmount = "UnallocatedInvoiceTaxTotal";
			this.OSUnallocatedTotalTaxCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.OSUnallocatedTotalTaxCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.OSUnallocatedTotalTaxCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSUnallocatedTotalTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 3, true);
			this.OSUnallocatedTotalTaxCalcEdit.Name = "OSUnallocatedTotalTaxCalcEdit";
			this.OSUnallocatedTotalTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.OSUnallocatedTotalTaxCalcEdit.TabIndex = 23;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OSUnallocatedTotalTaxCalcEdit, false);
			// 
			// IncludingTaxTotalPanel
			//
			this.IncludingTaxTotalPanel.AutoSize = true;
			this.IncludingTaxTotalPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.IncludingTaxTotalPanel.Controls.Add(this.OSInvoiceTotalIncTaxCalcEdit);
			this.IncludingTaxTotalPanel.Controls.Add(this.OSExpectedTotalIncTaxCalcEdit);
			this.IncludingTaxTotalPanel.Controls.Add(this.OSUnallocatedTotalIncTaxCalcEdit);
			this.IncludingTaxTotalPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.IncludingTaxTotalPanel.Name = "IncludingTaxTotalPanel";
			this.IncludingTaxTotalPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 23, true);
			this.IncludingTaxTotalPanel.TabIndex = 27;
			// 
			// OSInvoiceTotalIncTaxCalcEdit
			// 
			this.OSInvoiceTotalIncTaxCalcEdit.AllowDrop = true;
			this.OSInvoiceTotalIncTaxCalcEdit.BindToAmount = "AH_OSTotalAmount";
			this.OSInvoiceTotalIncTaxCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.OSInvoiceTotalIncTaxCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.OSInvoiceTotalIncTaxCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSInvoiceTotalIncTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 3, true);
			this.OSInvoiceTotalIncTaxCalcEdit.Name = "OSInvoiceTotalIncTaxCalcEdit";
			this.OSInvoiceTotalIncTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.OSInvoiceTotalIncTaxCalcEdit.TabIndex = 22;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OSInvoiceTotalIncTaxCalcEdit, false);
			// 
			// OSExpectedTotalIncTaxCalcEdit
			// 
			this.OSExpectedTotalIncTaxCalcEdit.AllowDrop = true;
			this.OSExpectedTotalIncTaxCalcEdit.BindToAmount = "ExpectedInvoiceTotal";
			this.OSExpectedTotalIncTaxCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.OSExpectedTotalIncTaxCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.OSExpectedTotalIncTaxCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4143cdec-1483-4a30-8911-01201d5efd29", "Including Tax");
			this.OSExpectedTotalIncTaxCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSExpectedTotalIncTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 3, true);
			this.OSExpectedTotalIncTaxCalcEdit.Name = "OSExpectedTotalIncTaxCalcEdit";
			this.OSExpectedTotalIncTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.OSExpectedTotalIncTaxCalcEdit.TabIndex = 21;
			// 
			// OSUnallocatedTotalIncTaxCalcEdit
			// 
			this.OSUnallocatedTotalIncTaxCalcEdit.AllowDrop = true;
			this.OSUnallocatedTotalIncTaxCalcEdit.BindToAmount = "UnallocatedInvoiceTotal";
			this.OSUnallocatedTotalIncTaxCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.OSUnallocatedTotalIncTaxCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.OSUnallocatedTotalIncTaxCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSUnallocatedTotalIncTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 3, true);
			this.OSUnallocatedTotalIncTaxCalcEdit.Name = "OSUnallocatedTotalIncTaxCalcEdit";
			this.OSUnallocatedTotalIncTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.OSUnallocatedTotalIncTaxCalcEdit.TabIndex = 23;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OSUnallocatedTotalIncTaxCalcEdit, false);
			// 
			// AH_OSSubTotalAmountCalcEdit
			// 
			this.AH_OSSubTotalAmountCalcEdit.AllowDrop = true;
			this.AH_OSSubTotalAmountCalcEdit.BindToAmount = "AH_OSSubTotalAmountWithGSTOnly";
			this.AH_OSSubTotalAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.AH_OSSubTotalAmountCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.AH_OSSubTotalAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fae95072-fdc3-4c98-b480-384c8da6361b", "Sub Total Invoice");
			this.AH_OSSubTotalAmountCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_OSSubTotalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 3, true);
			this.AH_OSSubTotalAmountCalcEdit.Name = "AH_OSSubTotalAmountCalcEdit";
			this.AH_OSSubTotalAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AH_OSSubTotalAmountCalcEdit.TabIndex = 21;
			// 
			// AH_LocalSubTotalAmountCalcEdit
			// 
			this.AH_LocalSubTotalAmountCalcEdit.AllowDrop = true;
			this.AH_LocalSubTotalAmountCalcEdit.BindToAmount = "AH_LocalSubTotalAmountWithGSTOnly";
			this.AH_LocalSubTotalAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.AH_LocalSubTotalAmountCalcEdit.BindToUnit = "AH_Calc_LocalRX";
			this.AH_LocalSubTotalAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9b6d7e82-4111-441f-84a7-4bc12b051454", "Sub Total Invoice in Local Currency");
			this.AH_LocalSubTotalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 3, true);
			this.AH_LocalSubTotalAmountCalcEdit.Name = "AH_LocalSubTotalAmountCalcEdit";
			this.AH_LocalSubTotalAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AH_LocalSubTotalAmountCalcEdit.TabIndex = 25;
			// 
			// AH_OSOtherTaxesAmountCalcEdit
			// 
			this.AH_OSOtherTaxesAmountCalcEdit.AllowDrop = true;
			this.AH_OSOtherTaxesAmountCalcEdit.BindToAmount = "AH_OSTaxAmountOtherTaxes_ForDisplay";
			this.AH_OSOtherTaxesAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.AH_OSOtherTaxesAmountCalcEdit.BindToUnit = "AH_Readonly_RXCode";
			this.AH_OSOtherTaxesAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("E3913E9E-33FB-413A-9BD2-6B8D92D9AF8F", "Tax Transactions");
			this.AH_OSOtherTaxesAmountCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_OSOtherTaxesAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 3, true);
			this.AH_OSOtherTaxesAmountCalcEdit.Name = "AH_OSOtherTaxesAmountCalcEdit";
			this.AH_OSOtherTaxesAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AH_OSOtherTaxesAmountCalcEdit.TabIndex = 22;
			// 
			// AH_LocalOtherTaxesAmountCalcEdit
			// 
			this.AH_LocalOtherTaxesAmountCalcEdit.AllowDrop = true;
			this.AH_LocalOtherTaxesAmountCalcEdit.BindToAmount = "AH_LocalTaxAmountOtherTaxes_ForDisplay";
			this.AH_LocalOtherTaxesAmountCalcEdit.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.AH_LocalOtherTaxesAmountCalcEdit.BindToUnit = "AH_Calc_LocalRX";
			this.AH_LocalOtherTaxesAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("92CC77BF-5F91-468F-A381-1689D3553C11", "Tax Transactions in Local Currency");
			this.AH_LocalOtherTaxesAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 3, true);
			this.AH_LocalOtherTaxesAmountCalcEdit.Name = "AH_LocalOtherTaxesAmountCalcEdit";
			this.AH_LocalOtherTaxesAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AH_LocalOtherTaxesAmountCalcEdit.TabIndex = 26;
			// 
			// SubTotalExTaxPanel
			// 
			this.SubTotalExTaxPanel.Controls.Add(this.OSSubTotalExTaxAmountCalcEdit);
			this.SubTotalExTaxPanel.Controls.Add(this.LocalSubTotalExTaxAmountCalcEdit);
			this.SubTotalExTaxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubTotalExTaxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SubTotalExTaxPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.SubTotalExTaxPanel.Name = "SubTotalExTaxPanel";
			this.SubTotalExTaxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 26, true);
			this.SubTotalExTaxPanel.TabIndex = 27;
			// 
			// TaxAmountsPanel
			// 
			this.TaxAmountsPanel.Controls.Add(this.AH_OSTaxAmountCalcEdit);
			this.TaxAmountsPanel.Controls.Add(this.AH_LocalTaxAmountCalcEdit);
			this.TaxAmountsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxAmountsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.TaxAmountsPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TaxAmountsPanel.Name = "TaxAmountsPanel";
			this.TaxAmountsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 26, true);
			this.TaxAmountsPanel.TabIndex = 28;
			// 
			// SubTotalAmountsPanel
			// 
			this.SubTotalAmountsPanel.Controls.Add(this.AH_OSSubTotalAmountCalcEdit);
			this.SubTotalAmountsPanel.Controls.Add(this.AH_LocalSubTotalAmountCalcEdit);
			this.SubTotalAmountsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubTotalAmountsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 52, true);
			this.SubTotalAmountsPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.SubTotalAmountsPanel.Name = "SubTotalAmountsPanel";
			this.SubTotalAmountsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 26, true);
			this.SubTotalAmountsPanel.TabIndex = 29;
			// 
			// OtherTaxesAmountsPanel
			// 
			this.OtherTaxesAmountsPanel.Controls.Add(this.AH_LocalOtherTaxesAmountCalcEdit);
			this.OtherTaxesAmountsPanel.Controls.Add(this.AH_OSOtherTaxesAmountCalcEdit);
			this.OtherTaxesAmountsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherTaxesAmountsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 78, true);
			this.OtherTaxesAmountsPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.OtherTaxesAmountsPanel.Name = "OtherTaxesAmountsPanel";
			this.OtherTaxesAmountsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 26, true);
			this.OtherTaxesAmountsPanel.TabIndex = 30;
			// 
			// TotalInvoiceAmountsPanel
			// 
			this.TotalInvoiceAmountsPanel.Controls.Add(this.AH_LocalTotalAmountCalcEdit);
			this.TotalInvoiceAmountsPanel.Controls.Add(this.AH_OSTotalAmountCalcEdit);
			this.TotalInvoiceAmountsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TotalInvoiceAmountsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 104, true);
			this.TotalInvoiceAmountsPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TotalInvoiceAmountsPanel.Name = "TotalInvoiceAmountsPanel";
			this.TotalInvoiceAmountsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 26, true);
			this.TotalInvoiceAmountsPanel.TabIndex = 31;
			// 
			// InvoiceTotalsCollapsibleLayoutPanel
			// 
			this.InvoiceTotalsCollapsibleLayoutPanel.AutoSize = true;
			this.InvoiceTotalsCollapsibleLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.InvoiceTotalsCollapsibleLayoutPanel.ColumnCount = 1;
			this.InvoiceTotalsCollapsibleLayoutPanel.ColumnStyles.Add(new ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.InvoiceTotalsCollapsibleLayoutPanel.Controls.Add(this.SubTotalExTaxPanel, 0, 0);
			this.InvoiceTotalsCollapsibleLayoutPanel.Controls.Add(this.TaxAmountsPanel, 0, 1);
			this.InvoiceTotalsCollapsibleLayoutPanel.Controls.Add(this.SubTotalAmountsPanel, 0, 2);
			this.InvoiceTotalsCollapsibleLayoutPanel.Controls.Add(this.OtherTaxesAmountsPanel, 0, 3);
			this.InvoiceTotalsCollapsibleLayoutPanel.Controls.Add(this.TotalInvoiceAmountsPanel, 0, 4);
			this.InvoiceTotalsCollapsibleLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.InvoiceTotalsCollapsibleLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InvoiceTotalsCollapsibleLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.InvoiceTotalsCollapsibleLayoutPanel.Name = "InvoiceTotalsCollapsibleLayoutPanel";
			this.InvoiceTotalsCollapsibleLayoutPanel.RowCount = 5;
			this.InvoiceTotalsCollapsibleLayoutPanel.RowStyles.Add(new RowStyle());
			this.InvoiceTotalsCollapsibleLayoutPanel.RowStyles.Add(new RowStyle());
			this.InvoiceTotalsCollapsibleLayoutPanel.RowStyles.Add(new RowStyle());
			this.InvoiceTotalsCollapsibleLayoutPanel.RowStyles.Add(new RowStyle());
			this.InvoiceTotalsCollapsibleLayoutPanel.RowStyles.Add(new RowStyle());
			this.InvoiceTotalsCollapsibleLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.InvoiceTotalsCollapsibleLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.InvoiceTotalsCollapsibleLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.InvoiceTotalsCollapsibleLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 130, true);
			this.InvoiceTotalsCollapsibleLayoutPanel.TabIndex = 19;
			// 
			// WHTPanel
			// 
			this.WHTPanel.Controls.Add(this.AH_LocalWHTAmountCalcEdit);
			this.WHTPanel.Controls.Add(this.AH_OSWHTAmountCalcEdit);
			this.WHTPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.WHTPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 133, true);
			this.WHTPanel.Name = "WHTPanel";
			this.WHTPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 26, true);
			this.WHTPanel.TabIndex = 20;
			// 
			// ExtraTaxPanel
			// 
			this.ExtraTaxPanel.Controls.Add(this.AH_LocalExtraTaxAmountCalcEdit);
			this.ExtraTaxPanel.Controls.Add(this.AH_OSExtraTaxAmountCalcEdit);
			this.ExtraTaxPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ExtraTaxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 159, true);
			this.ExtraTaxPanel.Name = "ExtraTaxPanel";
			this.ExtraTaxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 26, true);
			this.ExtraTaxPanel.TabIndex = 21;
			this.InvoiceDetailsTabPage.PerformLayout();
			this.ChargeDetailsPanel.ResumeLayout(false);
			this.ChargeDetailsPanel.PerformLayout();
			this.AH_LocalExtraTaxAmountCalcEdit.ResumeLayout(true);
			this.AH_LocalExtraTaxAmountCalcEdit.PerformLayout();
			this.AH_OSExtraTaxAmountCalcEdit.ResumeLayout(true);
			this.AH_OSExtraTaxAmountCalcEdit.PerformLayout();
			this.AH_LocalWHTAmountCalcEdit.ResumeLayout(true);
			this.AH_LocalWHTAmountCalcEdit.PerformLayout();
			this.AH_OSWHTAmountCalcEdit.ResumeLayout(true);
			this.AH_OSWHTAmountCalcEdit.PerformLayout();
			this.AH_LocalTotalAmountCalcEdit.ResumeLayout(true);
			this.AH_LocalTotalAmountCalcEdit.PerformLayout();
			this.AH_LocalTaxAmountCalcEdit.ResumeLayout(true);
			this.AH_LocalTaxAmountCalcEdit.PerformLayout();
			this.AH_OSTotalAmountCalcEdit.ResumeLayout(true);
			this.AH_OSTotalAmountCalcEdit.PerformLayout();
			this.AH_OSTaxAmountCalcEdit.ResumeLayout(true);
			this.AH_OSTaxAmountCalcEdit.PerformLayout();
			this.OSSubTotalExTaxAmountCalcEdit.ResumeLayout(true);
			this.OSSubTotalExTaxAmountCalcEdit.PerformLayout();
			this.LocalSubTotalExTaxAmountCalcEdit.ResumeLayout(true);
			this.LocalSubTotalExTaxAmountCalcEdit.PerformLayout();
			this.SubTotalExTaxPanel.ResumeLayout(false);
			this.SubTotalExTaxPanel.PerformLayout();
			this.WHTPanel.ResumeLayout(false);
			this.WHTPanel.PerformLayout();
			this.TotalAndUnallocatedTabControl.ResumeLayout(false);
			this.TotalAndUnallocatedTabControl.PerformLayout();
			this.TotalTabPage.PerformLayout();
			this.TotalTabPage.ResumeLayout(true);
			this.UnallocatedTabPage.PerformLayout();
			this.UnallocatedTabPage.ResumeLayout(true);
			this.OSExpectedTotalExTaxCalcEdit.ResumeLayout(true);
			this.OSExpectedTotalExTaxCalcEdit.PerformLayout();
			this.OSInvoiceTotalExTaxCalcEdit.ResumeLayout(true);
			this.OSInvoiceTotalExTaxCalcEdit.PerformLayout();
			this.OSUnallocatedTotalExTaxCalcEdit.ResumeLayout(true);
			this.OSUnallocatedTotalExTaxCalcEdit.PerformLayout();
			this.ExcludingTaxPanel.ResumeLayout(false);
			this.ExcludingTaxPanel.PerformLayout();
			this.TaxTotalPanel.ResumeLayout(false);
			this.TaxTotalPanel.PerformLayout();
			this.OSInvoiceTotalTaxCalcEdit.ResumeLayout(true);
			this.OSInvoiceTotalTaxCalcEdit.PerformLayout();
			this.OSExpectedTotalTaxCalcEdit.ResumeLayout(true);
			this.OSExpectedTotalTaxCalcEdit.PerformLayout();
			this.OSUnallocatedTotalTaxCalcEdit.ResumeLayout(true);
			this.OSUnallocatedTotalTaxCalcEdit.PerformLayout();
			this.IncludingTaxTotalPanel.ResumeLayout(false);
			this.IncludingTaxTotalPanel.PerformLayout();
			this.OSInvoiceTotalIncTaxCalcEdit.ResumeLayout(true);
			this.OSInvoiceTotalIncTaxCalcEdit.PerformLayout();
			this.OSExpectedTotalIncTaxCalcEdit.ResumeLayout(true);
			this.OSExpectedTotalIncTaxCalcEdit.PerformLayout();
			this.OSUnallocatedTotalIncTaxCalcEdit.ResumeLayout(true);
			this.OSUnallocatedTotalIncTaxCalcEdit.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.ExtraTaxPanel.ResumeLayout(false);
			this.ExtraTaxPanel.PerformLayout();
			this.InvoiceDetails.ResumeLayout(true);
			this.InvoiceDetails.PerformLayout();
			this.ChargesAndApportionmentsTabControl.ResumeLayout(false);
			this.ChargesAndApportionmentsTabControl.PerformLayout();
			this.AH_OSSubTotalAmountCalcEdit.ResumeLayout(true);
			this.AH_OSSubTotalAmountCalcEdit.PerformLayout();
			this.AH_LocalSubTotalAmountCalcEdit.ResumeLayout(true);
			this.AH_LocalSubTotalAmountCalcEdit.PerformLayout();
			this.AH_OSOtherTaxesAmountCalcEdit.ResumeLayout(true);
			this.AH_OSOtherTaxesAmountCalcEdit.PerformLayout();
			this.AH_LocalOtherTaxesAmountCalcEdit.ResumeLayout(true);
			this.AH_LocalOtherTaxesAmountCalcEdit.PerformLayout();
			this.TaxAmountsPanel.ResumeLayout(false);
			this.TaxAmountsPanel.PerformLayout();
			this.SubTotalAmountsPanel.ResumeLayout(false);
			this.SubTotalAmountsPanel.PerformLayout();
			this.OtherTaxesAmountsPanel.ResumeLayout(false);
			this.OtherTaxesAmountsPanel.PerformLayout();
			this.TotalInvoiceAmountsPanel.ResumeLayout(false);
			this.TotalInvoiceAmountsPanel.PerformLayout();
			this.UnallocatedCaptionPanel.ResumeLayout(false);
			this.UnallocatedCaptionPanel.PerformLayout();
			this.ExpectedTotalCaption.ResumeLayout(false);
			this.ExpectedTotalCaption.PerformLayout();
			this.InvoiceTotalCaption.ResumeLayout(false);
			this.InvoiceTotalCaption.PerformLayout();
			this.UnallocatedCaption.ResumeLayout(false);
			this.UnallocatedCaption.PerformLayout();
			this.InvoiceTotalsCollapsibleLayoutPanel.ResumeLayout(false);
			this.InvoiceTotalsCollapsibleLayoutPanel.PerformLayout();
			this.InvoiceDetailsTabPage.ResumeLayout(true);
			this.SubAccountsTabPage.ResumeLayout(false);
			this.SubAccountsTabPage.PerformLayout();
			this.SubAccountsControl.ResumeLayout(true);
			this.SubAccountsControl.PerformLayout();
		}

		void InvoiceDetails_Load(object sender, EventArgs e)
		{
			var amendStatusCodeInstanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.Country.Code) as IInstanceProvider<IAmendStatusCodeProvider>;
			var amendStatusCodeProvider = amendStatusCodeInstanceProvider?.Get();
			if (amendStatusCodeProvider?.IsSupportAmendStatusCode((BusinessObject)BusinessEntity) ?? false)
			{
				InvoiceDetails.InitExtendField(extendDropEdit =>
				{
					extendDropEdit.Enabled = true;
					extendDropEdit.Visible = true;
					extendDropEdit.CaptionResourceString = Res.GetData("5A9CAD88-B318-4AB9-966C-7D9B98864B0D", "Amend Status Code");
					BindingSource.SetBindingMember(extendDropEdit, "AH_Calc_AmendStatusCode");
				});
			}

			InvoiceDetails.ReversalDropEdit.Visible = InvoiceFormPresentationProvider.GetIsReversalStatusCodeVisible((InvoicingBase)BusinessEntity);
		}

		void LineChargesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			this.LineChargesGrid = new ZGrid();
			this.RestrictedLineChargesLabel = new ZLabel();
			this.LineChargesTabPage.SuspendLayout();
			((ISupportInitialize)(this.LineChargesGrid)).BeginInit();
			this.LineChargesGrid.SuspendLayout();
			this.LineChargesTabPage.Controls.Add(this.LineChargesGrid);
			this.LineChargesTabPage.Controls.Add(this.RestrictedLineChargesLabel);
			// 
			// LineChargesGrid
			// 
			this.LineChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LineChargesGrid, "FilteredLines.LineCharges");
			this.LineChargesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|a0c0807d-b4d6-4243-8148-603df8f9f722", "Job");
			zTextBoxColumnStyleInfo1.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|8c15263a-c160-40cb-bef1-dd318c128e46", "Charge");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AL_AC";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|fbad634e-be02-4434-844f-8605636212f4", "Type");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "AL_LineType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AL_GB";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|6f74515e-9c5e-415d-be4a-2520ac4158c6", "Dept");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "AL_GE";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AL_LineAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.LineChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LineChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.LineChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LineChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.LineChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.LineChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LineChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LineChargesGrid.GridId = "60A02B72-8737-47CE-8869-0CDEE67238B6";
			this.LineChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LineChargesGrid.LayoutKey = "LineChargesGrid";
			this.LineChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LineChargesGrid.Name = "LineChargesGrid";
			this.LineChargesGrid.ReadOnly = true;
			this.LineChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 93, true);
			this.LineChargesGrid.TabIndex = 0;
			// 
			// RestrictedLineChargesLabel
			// 
			this.RestrictedLineChargesLabel.AutoSize = true;
			this.RestrictedLineChargesLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RestrictedLineChargesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.RestrictedLineChargesLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 93, true);
			this.RestrictedLineChargesLabel.Name = "RestrictedLineChargesLabel";
			this.RestrictedLineChargesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.RestrictedLineChargesLabel.TabIndex = 1;
			this.RestrictedLineChargesLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.RestrictedLineChargesLabel.Visible = false;
			this.LineChargesTabPage.PerformLayout();
			((ISupportInitialize)(this.LineChargesGrid)).EndInit();
			this.LineChargesGrid.ResumeLayout(false);
			this.LineChargesGrid.PerformLayout();
			this.LineChargesTabPage.ResumeLayout(true);
		}

		void PeriodApportionmentTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new ZCalcEditColumnStyleInfo();
			this.PeriodApportionmentGrid = new ZGrid();
			this.PeriodApportionmentTabPage.SuspendLayout();
			((ISupportInitialize)(this.PeriodApportionmentGrid)).BeginInit();
			this.PeriodApportionmentGrid.SuspendLayout();
			this.PeriodApportionmentTabPage.Controls.Add(this.PeriodApportionmentGrid);
			// 
			// PeriodApportionmentGrid
			// 
			this.PeriodApportionmentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PeriodApportionmentGrid, "FilteredLines.PeriodApportionmentLines");
			this.PeriodApportionmentGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "Period";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.ShowGroupSeparators = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.ColumnName = "PeriodStart";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo2.ColumnName = "PeriodEnd";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "NumberOfDaysInPeriod";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.ColumnName = "CurrencyCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "ExchangeRate";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "OSAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "LocalAmount";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = "";
			zCalcEditColumnStyleInfo12.ColumnName = "OSTaxNotRecoverable";
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = "";
			zCalcEditColumnStyleInfo13.ColumnName = "LocalTaxNotRecoverable";
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PeriodApportionmentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PeriodApportionmentGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PeriodApportionmentGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.PeriodApportionmentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PeriodApportionmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PeriodApportionmentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PeriodApportionmentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.PeriodApportionmentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.PeriodApportionmentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.PeriodApportionmentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);

			this.PeriodApportionmentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PeriodApportionmentGrid.GridId = "ba1d2c83-e418-45e9-97e5-4565f8f3af4c";
			this.PeriodApportionmentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PeriodApportionmentGrid.LayoutKey = "PeriodApportionmentGrid";
			this.PeriodApportionmentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PeriodApportionmentGrid.Name = "PeriodApportionmentGrid";
			this.PeriodApportionmentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 93, true);
			this.PeriodApportionmentGrid.TabIndex = 2;
			this.PeriodApportionmentTabPage.PerformLayout();
			((ISupportInitialize)(this.PeriodApportionmentGrid)).EndInit();
			this.PeriodApportionmentGrid.ResumeLayout(false);
			this.PeriodApportionmentGrid.PerformLayout();
			this.PeriodApportionmentTabPage.ResumeLayout(true);
		}

		void RelatedInvoicesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new ZOrganisationFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new ZDateEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new ZCalcEditColumnStyleInfo();
			this.RelatedInvoicesGrid = new ZGrid();
			this.RelatedInvoicesTabPage.SuspendLayout();
			((ISupportInitialize)(this.RelatedInvoicesGrid)).BeginInit();
			this.RelatedInvoicesGrid.SuspendLayout();
			this.RelatedInvoicesTabPage.Controls.Add(this.RelatedInvoicesGrid);
			// 
			// RelatedInvoicesGrid
			// 
			this.RelatedInvoicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RelatedInvoicesGrid, "RelatedInvoices");
			this.RelatedInvoicesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.ColumnName = "AH_Ledger";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|ca837312-0bac-44e2-aea3-d1f277e69620", "Transaction Number");
			zTextBoxColumnStyleInfo6.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|9a96d7b3-c0c8-4ce5-8d0c-10523892cb3e", "Job");
			zTextBoxColumnStyleInfo7.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo5.ColumnName = "AH_DueDate";
			zDateEditColumnStyleInfo5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo6.ColumnName = "AH_FullyPaidDate";
			zDateEditColumnStyleInfo6.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AH_RX_NKTransactionCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|d074a5cc-5e66-493e-9c8c-c35cc2929b7c", "Trans. Amount");
			zCalcEditColumnStyleInfo7.ColumnName = "AH_OSTotal";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|58152cdf-e0d3-41c8-a952-9d650a3b0a40", "Tax Amount");
			zCalcEditColumnStyleInfo8.ColumnName = "AH_OSTax";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|650efe83-c3f3-4fd3-bae8-8ee5616f1aae", "Outstanding Amount");
			zCalcEditColumnStyleInfo9.ColumnName = "OSOutstandingAmountMatching";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|b1221a5b-0dd7-4fe7-b4c2-394532238751", "Local Amount");
			zCalcEditColumnStyleInfo10.ColumnName = "AH_LocalTotal";
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BaseInvoicingForm|de37b870-0cef-4617-b310-898d0c2a41af", "Local Tax Amount");
			zCalcEditColumnStyleInfo11.ColumnName = "AH_GSTAmount";
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.RelatedInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.RelatedInvoicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedInvoicesGrid.GridId = "8e61d6da-e5b6-44cf-b497-d09d2cc51c4a";
			this.RelatedInvoicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedInvoicesGrid.LayoutKey = "RelatedInvoicesGrid";
			this.RelatedInvoicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RelatedInvoicesGrid.Name = "RelatedInvoicesGrid";
			this.RelatedInvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 616, true);
			this.RelatedInvoicesGrid.TabIndex = 0;
			this.RelatedInvoicesTabPage.PerformLayout();
			((ISupportInitialize)(this.RelatedInvoicesGrid)).EndInit();
			this.RelatedInvoicesGrid.ResumeLayout(false);
			this.RelatedInvoicesGrid.PerformLayout();
			this.RelatedInvoicesTabPage.ResumeLayout(true);
		}

		void sourceXmlTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.importedInvoiceXMLControl = new ImportedInvoiceXMLControl();
			this.sourceXmlTabPage.SuspendLayout();
			this.importedInvoiceXMLControl.SuspendLayout();
			this.sourceXmlTabPage.Controls.Add(this.importedInvoiceXMLControl);
			// 
			// importedInvoiceXMLControl
			// 
			this.importedInvoiceXMLControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.importedInvoiceXMLControl, "AllocationApprovalRequest.PostingDetails.UniversalTransaction");
			this.importedInvoiceXMLControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.importedInvoiceXMLControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.importedInvoiceXMLControl.Name = "importedInvoiceXMLControl";
			this.importedInvoiceXMLControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 616, true);
			this.importedInvoiceXMLControl.TabIndex = 0;
			this.sourceXmlTabPage.PerformLayout();
			this.importedInvoiceXMLControl.ResumeLayout(true);
			this.importedInvoiceXMLControl.PerformLayout();
			this.sourceXmlTabPage.ResumeLayout(true);
		}

		void zWorkflowTabPage1_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.zWorkflowTabPage1.SuspendLayout();
			this.zWorkflowTabPage1.ResumeLayout(false);
			this.zWorkflowTabPage1.PerformLayout();
		}

		#endregion

	}
}
