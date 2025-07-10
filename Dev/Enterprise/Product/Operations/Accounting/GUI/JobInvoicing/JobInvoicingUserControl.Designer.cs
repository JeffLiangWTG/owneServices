using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobInvoicingUserControl
	{


		#region Component Designer generated code

		JobProfitLossControl jobProfitLossControl1;
		ZTemplateTabControl JobInvoicingTabControl;
		ZTabPage InvoicingTabPage;
		public ZTabPage ARInvoicesTabPage;
		JobChargeUserControl jobChargeUserControl1;
		JobInvoicePrintingControl jobInvoicePrintingControl1;
		public ZTabPage ProfitLossTabPage;
		private ZPanel ARJobInvoicingPrintingSecurityPanel;
		private ZLabel ARInvoicePrintingSecurityLabel;
		private ZPanel JobProfitLossSecurityPanel;
		private ZLabel JobProfitLossSecurityLabel;
		private ZPanel JobInvoicingSecurityPanel;
		public ZTabPage APInvoicesTabPage;
		private ZPanel APJobInvoicingPrintingSecurityPanel;
		private ZLabel APInvoicePrintingSecurityLabel;
		private APInvoicePrintingUserControl apInvoicePrintingUserControl1;
		private APDraftInvoicePrintingUserControl draftInvoiceListUserControl1;
		public ZTabPage CreditStatusTabPage;
		private ZLabel JobInvoicingSecurityLabel;
		private ZPanel jobCreditStatusSecurityPanel;
		private ZLabel jobCreditStatusSecurityLabel;
		private CreditStatusControl creditStatusControl1;
		internal ZTabPage CashAdvanceRequestsTabPage;
		private CashAdvanceRequestUserControl cashAdvanceRequestUserControl1;
		private System.ComponentModel.IContainer components;
		private Enterprise.ZArchitecture.GUI.ZGroupBox APInvoiceGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DraftInvoiceGroupBox;
		public CargoWise.Windows.UI.KSplitContainer APInvoicePrintingSplitContainer;

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.JobInvoicingTabControl = new ZTemplateTabControl();
			this.InvoicingTabPage = new ZTabPage();
			this.jobChargeUserControl1 = new JobChargeUserControl();
			this.JobInvoicingSecurityPanel = new ZPanel();
			this.JobInvoicingSecurityLabel = new ZLabel();
			this.ProfitLossTabPage = new ZTabPage();
			this.jobProfitLossControl1 = new JobProfitLossControl();
			this.JobProfitLossSecurityPanel = new ZPanel();
			this.JobProfitLossSecurityLabel = new ZLabel();
			this.ARInvoicesTabPage = new ZTabPage();
			this.jobInvoicePrintingControl1 = new JobInvoicePrintingControl();
			this.ARJobInvoicingPrintingSecurityPanel = new ZPanel();
			this.ARInvoicePrintingSecurityLabel = new ZLabel();
			this.APInvoicesTabPage = new ZTabPage();
			this.apInvoicePrintingUserControl1 = new APInvoicePrintingUserControl();
			this.draftInvoiceListUserControl1 = new APDraftInvoicePrintingUserControl();
			this.APJobInvoicingPrintingSecurityPanel = new ZPanel();
			this.APInvoicePrintingSecurityLabel = new ZLabel();
			this.CreditStatusTabPage = new ZTabPage();
			this.jobCreditStatusSecurityPanel = new ZPanel();
			this.creditStatusControl1 = new CreditStatusControl();
			this.jobCreditStatusSecurityLabel = new ZLabel();
			this.CashAdvanceRequestsTabPage = new ZTabPage();
			this.cashAdvanceRequestUserControl1 = new CashAdvanceRequestUserControl();
			this.APInvoiceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DraftInvoiceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.APInvoicePrintingSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JobInvoicingTabControl.SuspendLayout();
			this.InvoicingTabPage.SuspendLayout();
			this.jobChargeUserControl1.SuspendLayout();
			this.JobInvoicingSecurityPanel.SuspendLayout();
			this.ProfitLossTabPage.SuspendLayout();
			this.jobProfitLossControl1.SuspendLayout();
			this.JobProfitLossSecurityPanel.SuspendLayout();
			this.ARInvoicesTabPage.SuspendLayout();
			this.jobInvoicePrintingControl1.SuspendLayout();
			this.ARJobInvoicingPrintingSecurityPanel.SuspendLayout();
			this.APInvoicesTabPage.SuspendLayout();
			this.apInvoicePrintingUserControl1.SuspendLayout();
			this.draftInvoiceListUserControl1.SuspendLayout();
			this.APJobInvoicingPrintingSecurityPanel.SuspendLayout();
			this.CreditStatusTabPage.SuspendLayout();
			this.jobCreditStatusSecurityPanel.SuspendLayout();
			this.creditStatusControl1.SuspendLayout();
			this.CashAdvanceRequestsTabPage.SuspendLayout();
			this.cashAdvanceRequestUserControl1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.APInvoicePrintingSplitContainer)).BeginInit();
			this.APInvoiceGroupBox.SuspendLayout();
			this.DraftInvoiceGroupBox.SuspendLayout();
			this.APInvoicePrintingSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.JobInvoicing.Job);
			// 
			// JobInvoicingTabControl
			// 
			this.JobInvoicingTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.JobInvoicingTabControl.Controls.Add(this.InvoicingTabPage);
			this.JobInvoicingTabControl.Controls.Add(this.ProfitLossTabPage);
			this.JobInvoicingTabControl.Controls.Add(this.ARInvoicesTabPage);
			this.JobInvoicingTabControl.Controls.Add(this.APInvoicesTabPage);
			this.JobInvoicingTabControl.Controls.Add(this.CreditStatusTabPage);
			this.JobInvoicingTabControl.Controls.Add(this.CashAdvanceRequestsTabPage);
			this.JobInvoicingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobInvoicingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobInvoicingTabControl.Name = "JobInvoicingTabControl";
			this.JobInvoicingTabControl.SelectedIndex = 0;
			this.JobInvoicingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 432, true);
			this.JobInvoicingTabControl.TabIndex = 0;
			this.JobInvoicingTabControl.Selected += new TabControlEventHandler(this.JobInvoicingTabControl_Selected);
			// 
			// InvoicingTabPage
			// 
			this.InvoicingTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicingUserControl|f761a12f-8c7a-4161-b73b-d58690bf963a", "Invoicing");
			this.InvoicingTabPage.Controls.Add(this.jobChargeUserControl1);
			this.InvoicingTabPage.Controls.Add(this.JobInvoicingSecurityPanel);
			this.InvoicingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InvoicingTabPage.Name = "InvoicingTabPage";
			this.InvoicingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1119, 405, true);
			this.InvoicingTabPage.TabIndex = 0;
			// 
			// jobChargeUserControl1
			// 
			this.jobChargeUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jobChargeUserControl1, ".");
			this.jobChargeUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.jobChargeUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.jobChargeUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 405, true);
			this.jobChargeUserControl1.Name = "jobChargeUserControl1";
			this.jobChargeUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 405, true);
			this.jobChargeUserControl1.TabIndex = 0;
			// 
			// JobInvoicingSecurityPanel
			// 
			this.JobInvoicingSecurityPanel.Controls.Add(this.JobInvoicingSecurityLabel);
			this.JobInvoicingSecurityPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobInvoicingSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobInvoicingSecurityPanel.Name = "JobInvoicingSecurityPanel";
			this.JobInvoicingSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1119, 405, true);
			this.JobInvoicingSecurityPanel.TabIndex = 1;
			// 
			// JobInvoicingSecurityLabel
			// 
			this.JobInvoicingSecurityLabel.AutoSize = true;
			this.JobInvoicingSecurityLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.JobInvoicingSecurityLabel.IsFontBold = true;
			this.JobInvoicingSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 200, true);
			this.JobInvoicingSecurityLabel.Name = "JobInvoicingSecurityLabel";
			this.JobInvoicingSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 13, true);
			this.JobInvoicingSecurityLabel.TabIndex = 0;
			this.JobInvoicingSecurityLabel.Text = "InvoicingSecurityLabel";
			this.JobInvoicingSecurityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ProfitLossTabPage
			// 
			this.ProfitLossTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicingUserControl|ecb11a1e-3e81-4c94-8a8a-161a38e4ef0d", "Profit and Loss");
			this.ProfitLossTabPage.Controls.Add(this.jobProfitLossControl1);
			this.ProfitLossTabPage.Controls.Add(this.JobProfitLossSecurityPanel);
			this.ProfitLossTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ProfitLossTabPage.Name = "ProfitLossTabPage";
			this.ProfitLossTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1119, 405, true);
			this.ProfitLossTabPage.TabIndex = 3;
			// 
			// jobProfitLossControl1
			// 
			this.jobProfitLossControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jobProfitLossControl1, "ProfitLoss");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Integration.IJobProfitLoss)(((Integration.IJobProfitLoss)(((System.Collections.IList)(((Business.JobInvoicing.Job)(null)).ProfitLoss)).SyncRoot)))));
			this.jobProfitLossControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.jobProfitLossControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.jobProfitLossControl1.Name = "jobProfitLossControl1";
			this.jobProfitLossControl1.PluginSecurity = null;
			this.jobProfitLossControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1119, 405, true);
			this.jobProfitLossControl1.TabIndex = 0;
			// 
			// JobProfitLossSecurityPanel
			// 
			this.JobProfitLossSecurityPanel.Controls.Add(this.JobProfitLossSecurityLabel);
			this.JobProfitLossSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobProfitLossSecurityPanel.Name = "JobProfitLossSecurityPanel";
			this.JobProfitLossSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 573, true);
			this.JobProfitLossSecurityPanel.TabIndex = 1;
			// 
			// JobProfitLossSecurityLabel
			// 
			this.JobProfitLossSecurityLabel.AutoSize = true;
			this.JobProfitLossSecurityLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.JobProfitLossSecurityLabel.IsFontBold = true;
			this.JobProfitLossSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 200, true);
			this.JobProfitLossSecurityLabel.Name = "JobProfitLossSecurityLabel";
			this.JobProfitLossSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 13, true);
			this.JobProfitLossSecurityLabel.TabIndex = 0;
			this.JobProfitLossSecurityLabel.Text = "ProfitLossSecurityLabel";
			this.JobProfitLossSecurityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ARInvoicesTabPage
			// 
			this.ARInvoicesTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicingUserControl|b3b8f22e-7649-4dbb-9d32-8b41ff964fb9", "AR Invoices");
			this.ARInvoicesTabPage.Controls.Add(this.jobInvoicePrintingControl1);
			this.ARInvoicesTabPage.Controls.Add(this.ARJobInvoicingPrintingSecurityPanel);
			this.ARInvoicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ARInvoicesTabPage.Name = "ARInvoicesTabPage";
			this.ARInvoicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1119, 405, true);
			this.ARInvoicesTabPage.TabIndex = 1;
			// 
			// jobInvoicePrintingControl1
			// 
			this.jobInvoicePrintingControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jobInvoicePrintingControl1, "PrintingFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.JobInvoicing.JobARInvoicePrintingFilter)(((Business.JobInvoicing.Job)(null)).PrintingFilter)));
			this.jobInvoicePrintingControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.jobInvoicePrintingControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.jobInvoicePrintingControl1.Name = "jobInvoicePrintingControl1";
			this.jobInvoicePrintingControl1.PluginSecurity = null;
			this.jobInvoicePrintingControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1119, 405, true);
			this.jobInvoicePrintingControl1.TabIndex = 0;
			// 
			// ARJobInvoicingPrintingSecurityPanel
			// 
			this.ARJobInvoicingPrintingSecurityPanel.Controls.Add(this.ARInvoicePrintingSecurityLabel);
			this.ARJobInvoicingPrintingSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.ARJobInvoicingPrintingSecurityPanel.Name = "ARJobInvoicingPrintingSecurityPanel";
			this.ARJobInvoicingPrintingSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 573, true);
			this.ARJobInvoicingPrintingSecurityPanel.TabIndex = 1;
			// 
			// ARInvoicePrintingSecurityLabel
			// 
			this.ARInvoicePrintingSecurityLabel.AutoSize = true;
			this.ARInvoicePrintingSecurityLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ARInvoicePrintingSecurityLabel.IsFontBold = true;
			this.ARInvoicePrintingSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 194, true);
			this.ARInvoicePrintingSecurityLabel.Name = "ARInvoicePrintingSecurityLabel";
			this.ARInvoicePrintingSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 13, true);
			this.ARInvoicePrintingSecurityLabel.TabIndex = 0;
			this.ARInvoicePrintingSecurityLabel.Text = "InvoicePrintingSecurityLabel";
			this.ARInvoicePrintingSecurityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// APInvoicesTabPage
			// 
			this.APInvoicesTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicingUserControl|d9a27141-5db8-4b06-83ee-45e712722bd9", "AP Invoices");
			this.APInvoicesTabPage.Controls.Add(this.APInvoicePrintingSplitContainer);
			this.APInvoicesTabPage.Controls.Add(this.APJobInvoicingPrintingSecurityPanel);
			this.APInvoicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.APInvoicesTabPage.Name = "APInvoicesTabPage";
			this.APInvoicesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.APInvoicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1119, 405, true);
			this.APInvoicesTabPage.TabIndex = 4;
			// 
			// apInvoicePrintingUserControl1
			// 
			this.apInvoicePrintingUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.apInvoicePrintingUserControl1, "APPrintingFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.JobInvoicing.JobAPInvoicePrintingFilter)(((Business.JobInvoicing.Job)(null)).APPrintingFilter)));
			this.apInvoicePrintingUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.apInvoicePrintingUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.apInvoicePrintingUserControl1.Name = "apInvoicePrintingUserControl1";
			this.apInvoicePrintingUserControl1.PluginSecurity = null;
			this.apInvoicePrintingUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1113, 399, true);
			this.apInvoicePrintingUserControl1.TabIndex = 0;
			// 
			// draftInvoicePrintingUserControl1
			// 
			this.draftInvoiceListUserControl1.AllowDrop = true;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.draftInvoiceListUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.draftInvoiceListUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.draftInvoiceListUserControl1.Name = "draftInvoicePrintingUserControl1";
			this.draftInvoiceListUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1113, 399, true);
			this.draftInvoiceListUserControl1.TabIndex = 1;
			// 
			// APJobInvoicingPrintingSecurityPanel
			// 
			this.APJobInvoicingPrintingSecurityPanel.Controls.Add(this.APInvoicePrintingSecurityLabel);
			this.APJobInvoicingPrintingSecurityPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.APJobInvoicingPrintingSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.APJobInvoicingPrintingSecurityPanel.Name = "APJobInvoicingPrintingSecurityPanel";
			this.APJobInvoicingPrintingSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1113, 399, true);
			this.APJobInvoicingPrintingSecurityPanel.TabIndex = 2;
			// 
			// APInvoicePrintingSecurityLabel
			// 
			this.APInvoicePrintingSecurityLabel.AutoSize = true;
			this.APInvoicePrintingSecurityLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.APInvoicePrintingSecurityLabel.IsFontBold = true;
			this.APInvoicePrintingSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 194, true);
			this.APInvoicePrintingSecurityLabel.Name = "APInvoicePrintingSecurityLabel";
			this.APInvoicePrintingSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.APInvoicePrintingSecurityLabel.TabIndex = 0;
			this.APInvoicePrintingSecurityLabel.Text = "zLabel2";
			this.APInvoicePrintingSecurityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// CreditStatusTabPage
			// 
			this.CreditStatusTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.CreditStatusTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobInvoicingUserControl|db9bb341-555b-47d5-a03b-dedc04380f95", "Credit Status");
			this.CreditStatusTabPage.Controls.Add(this.jobCreditStatusSecurityPanel);
			this.CreditStatusTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CreditStatusTabPage.Name = "CreditStatusTabPage";
			this.CreditStatusTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CreditStatusTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1119, 405, true);
			this.CreditStatusTabPage.TabIndex = 5;
			// 
			// jobCreditStatusSecurityPanel
			// 
			this.jobCreditStatusSecurityPanel.Controls.Add(this.creditStatusControl1);
			this.jobCreditStatusSecurityPanel.Controls.Add(this.jobCreditStatusSecurityLabel);
			this.jobCreditStatusSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.jobCreditStatusSecurityPanel.Name = "jobCreditStatusSecurityPanel";
			this.jobCreditStatusSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 378, true);
			this.jobCreditStatusSecurityPanel.TabIndex = 4;
			// 
			// creditStatusControl1
			// 
			this.creditStatusControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.creditStatusControl1, "CreditStatusBizObject");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CreditStatus.CreditStatusBusinessObject)(((Business.JobInvoicing.Job)(null)).CreditStatusBizObject)));
			this.creditStatusControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.creditStatusControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.creditStatusControl1.Name = "creditStatusControl1";
			this.creditStatusControl1.PluginSecurity = null;
			this.creditStatusControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 378, true);
			this.creditStatusControl1.TabIndex = 8;
			this.creditStatusControl1.VisibleChanged += new EventHandler(this.creditStatusControl1_VisibleChanged);
			// 
			// jobCreditStatusSecurityLabel
			// 
			this.jobCreditStatusSecurityLabel.AutoSize = true;
			this.jobCreditStatusSecurityLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.jobCreditStatusSecurityLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.jobCreditStatusSecurityLabel, false);
			this.jobCreditStatusSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 200, true);
			this.jobCreditStatusSecurityLabel.Name = "jobCreditStatusSecurityLabel";
			this.jobCreditStatusSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 13, true);
			this.jobCreditStatusSecurityLabel.TabIndex = 6;
			this.jobCreditStatusSecurityLabel.Text = "Credit Status Security Label";
			this.jobCreditStatusSecurityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// CashAdvanceRequestsTabPage
			// 
			this.CashAdvanceRequestsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a8a88cad-efc8-4bc7-8cc8-9ecb976d4f1d", "Advance Payments");
			this.CashAdvanceRequestsTabPage.Controls.Add(this.cashAdvanceRequestUserControl1);
			this.CashAdvanceRequestsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CashAdvanceRequestsTabPage.Name = "CashAdvanceRequestsTabPage";
			this.CashAdvanceRequestsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CashAdvanceRequestsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1119, 405, true);
			this.CashAdvanceRequestsTabPage.TabIndex = 6;
			this.CashAdvanceRequestsTabPage.UseVisualStyleBackColor = true;
			// 
			// cashAdvanceRequestUserControl1
			// 
			this.cashAdvanceRequestUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cashAdvanceRequestUserControl1, ".");
			this.cashAdvanceRequestUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cashAdvanceRequestUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cashAdvanceRequestUserControl1.Name = "cashAdvanceRequestUserControl1";
			this.cashAdvanceRequestUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1113, 399, true);
			this.cashAdvanceRequestUserControl1.TabIndex = 0;
			// 
			// APInvoiceGroupBox
			// 
			this.APInvoiceGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("907A8F7D-9E7A-4DB5-82E7-F5D1285A8AC6", "AP Invoices");
			this.APInvoiceGroupBox.Controls.Add(this.apInvoicePrintingUserControl1);
			this.APInvoiceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.APInvoiceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.APInvoiceGroupBox.Name = "APInvoiceGroupBox";
			this.APInvoiceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1113, 199, true);
			this.APInvoiceGroupBox.TabIndex = 0;
			this.APInvoiceGroupBox.TabStop = false;
			// 
			// DraftInvoiceGroupBox
			// 
			this.DraftInvoiceGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("C413CB00-5023-4670-82C5-ED865427D332", "Draft Invoices");
			this.DraftInvoiceGroupBox.Controls.Add(this.draftInvoiceListUserControl1);
			this.DraftInvoiceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DraftInvoiceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DraftInvoiceGroupBox.Name = "DraftInvoiceGroupBox";
			this.DraftInvoiceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1113, 199, true);
			this.DraftInvoiceGroupBox.TabIndex = 0;
			this.DraftInvoiceGroupBox.TabStop = false;
			// 
			// splitContainer1
			// 
			this.APInvoicePrintingSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.APInvoicePrintingSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.APInvoicePrintingSplitContainer.Panel1.Controls.Add(APInvoiceGroupBox);
			this.APInvoicePrintingSplitContainer.Panel2.Controls.Add(DraftInvoiceGroupBox);
			this.APInvoicePrintingSplitContainer.Name = "splitContainer1";
			this.APInvoicePrintingSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.APInvoicePrintingSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1113, 399);
			this.APInvoicePrintingSplitContainer.SplitterDistance = 199;
			this.APInvoicePrintingSplitContainer.TabIndex = 0;
			// 
			// JobInvoicingUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.JobInvoicingTabControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 432, true);
			this.Name = "JobInvoicingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 432, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.JobInvoicingTabControl.ResumeLayout(false);
			this.JobInvoicingTabControl.PerformLayout();
			this.InvoicingTabPage.ResumeLayout(false);
			this.InvoicingTabPage.PerformLayout();
			this.jobChargeUserControl1.ResumeLayout(true);
			this.jobChargeUserControl1.PerformLayout();
			this.JobInvoicingSecurityPanel.ResumeLayout(false);
			this.JobInvoicingSecurityPanel.PerformLayout();
			this.ProfitLossTabPage.ResumeLayout(false);
			this.ProfitLossTabPage.PerformLayout();
			this.jobProfitLossControl1.ResumeLayout(true);
			this.jobProfitLossControl1.PerformLayout();
			this.JobProfitLossSecurityPanel.ResumeLayout(false);
			this.JobProfitLossSecurityPanel.PerformLayout();
			this.ARInvoicesTabPage.ResumeLayout(false);
			this.ARInvoicesTabPage.PerformLayout();
			this.jobInvoicePrintingControl1.ResumeLayout(true);
			this.jobInvoicePrintingControl1.PerformLayout();
			this.ARJobInvoicingPrintingSecurityPanel.ResumeLayout(false);
			this.ARJobInvoicingPrintingSecurityPanel.PerformLayout();
			this.APInvoicesTabPage.ResumeLayout(false);
			this.APInvoicesTabPage.PerformLayout();
			this.apInvoicePrintingUserControl1.ResumeLayout(true);
			this.apInvoicePrintingUserControl1.PerformLayout();
			this.draftInvoiceListUserControl1.ResumeLayout(true);
			this.draftInvoiceListUserControl1.PerformLayout();
			this.APInvoicePrintingSplitContainer.ResumeLayout(false);
			this.APInvoicePrintingSplitContainer.PerformLayout();
			this.APJobInvoicingPrintingSecurityPanel.ResumeLayout(false);
			this.APJobInvoicingPrintingSecurityPanel.PerformLayout();
			this.CreditStatusTabPage.ResumeLayout(false);
			this.CreditStatusTabPage.PerformLayout();
			this.jobCreditStatusSecurityPanel.ResumeLayout(false);
			this.jobCreditStatusSecurityPanel.PerformLayout();
			this.creditStatusControl1.ResumeLayout(true);
			this.creditStatusControl1.PerformLayout();
			this.CashAdvanceRequestsTabPage.ResumeLayout(false);
			this.CashAdvanceRequestsTabPage.PerformLayout();
			this.cashAdvanceRequestUserControl1.ResumeLayout(true);
			this.cashAdvanceRequestUserControl1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.APInvoicePrintingSplitContainer)).EndInit();
			this.APInvoiceGroupBox.ResumeLayout(false);
			this.APInvoiceGroupBox.PerformLayout();
			this.DraftInvoiceGroupBox.ResumeLayout(false);
			this.DraftInvoiceGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
