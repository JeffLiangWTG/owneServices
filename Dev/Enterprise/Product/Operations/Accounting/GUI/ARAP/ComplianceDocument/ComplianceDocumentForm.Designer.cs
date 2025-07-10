using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class ComplianceDocumentForm
	{


		#region Windows Form Designer generated code

		IContainer components = null;
		public ZTemplateTabControl MainTabControl;
		public ZTabPage ComplianceDocumentTabPage;
		public ZTabPage RelatedInvoicesTabPage;
		public ZWorkflowTabPage WorkflowTabPage;
		public ZLogsTabPage LogsTabPage;
		public ZPanel MainPanel;
		public ZPanel ButtonsPanel;
		public ComplianceDocumentUserControl ComplianceDocumentUserControl;
		public ZPanel PostingPanel;
		public ZPostingButtonsUserControl PostingButtonsUserControl;
		public ZPanel useControlPanel;
		public ZGrid RelatedInvoicesGrid;
		internal MenuItem ResetStatusToQueuedMenuItem;

		protected override void InitializeComponent()
		{
			this.components = new Container();
			this.MainTabControl = new ZTemplateTabControl();
			this.ComplianceDocumentTabPage = new ZTabPage();
			this.RelatedInvoicesTabPage = new ZTabPage();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.LogsTabPage = new ZLogsTabPage();
			this.ButtonsPanel = new ZPanel();
			this.PostingButtonsUserControl = new ZPostingButtonsUserControl();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 552, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AccComplianceDocumentHeader);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.ComplianceDocumentTabPage);
			this.MainTabControl.Controls.Add(this.RelatedInvoicesTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.LogsTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 515, true);
			this.MainTabControl.TabIndex = 1;
			// 
			// ComplianceDocumentTabPage
			// 
			this.ComplianceDocumentTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ComplianceDocumentTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6F08AF2A-6EFD-4CB5-A02A-4100A64688A4", "Compliance Document");
			this.ComplianceDocumentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ComplianceDocumentTabPage.Name = "ComplianceDocumentTabPage";
			this.ComplianceDocumentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 488, true);
			this.ComplianceDocumentTabPage.TabIndex = 0;
			this.ComplianceDocumentTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.ComplianceDocumentTabPage_InitializeTab));
			// 
			// RelatedInvoicesTabPage
			// 
			this.RelatedInvoicesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.RelatedInvoicesTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2DC36CF5-0366-4019-991E-7459464D320F", "Related Invoices");
			this.RelatedInvoicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelatedInvoicesTabPage.Name = "RelatedInvoicesTabPage";
			this.RelatedInvoicesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RelatedInvoicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 488, true);
			this.RelatedInvoicesTabPage.TabIndex = 1;
			this.RelatedInvoicesTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.RelatedInvoicesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).TransactionHeaders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccTransactionHeader)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).TransactionHeaders)).SyncRoot)).AH_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccTransactionHeader)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).TransactionHeaders)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((AccTransactionHeader)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).TransactionHeaders)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccTransactionHeader)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).TransactionHeaders)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccTransactionHeader)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).TransactionHeaders)).SyncRoot)).JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((AccTransactionHeader)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).TransactionHeaders)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((AccTransactionHeader)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).TransactionHeaders)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((AccTransactionHeader)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).TransactionHeaders)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((AccTransactionHeader)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).TransactionHeaders)).SyncRoot)).AH_FullyPaidDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((AccTransactionHeader)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).TransactionHeaders)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((AccTransactionHeader)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).TransactionHeaders)).SyncRoot)).AH_OSTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((AccTransactionHeader)(((System.Collections.IList)(((AccComplianceDocumentHeader)(null)).TransactionHeaders)).SyncRoot)).AH_GSTAmount)));
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 488, true);
			this.WorkflowTabPage.TabIndex = 2;
			this.WorkflowTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.WorkflowTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.LogsTabPage.ExcludeFromBindingOnSave = true;
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LogsTabPage.Name = "LogsTabPage";
			this.LogsTabPage.ShouldBeReadOnlyInViewMode = false;
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 488, true);
			this.LogsTabPage.TabIndex = 4;
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.BackColor = System.Drawing.SystemColors.Control;
			this.ButtonsPanel.Controls.Add(this.PostingButtonsUserControl);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPanel.ImeMode = System.Windows.Forms.ImeMode.On;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 515, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 37, true);
			this.ButtonsPanel.TabIndex = 6;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(611, 6, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 25, true);
			this.PostingButtonsUserControl.TabIndex = 6;
			// 
			// ComplianceDocumentForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3BADB0E9-E8AA-4DD3-B5F1-22554490063C", "Compliance Document");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 574, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.ButtonsPanel);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(AccComplianceDocumentHeader);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.AccComplianceDocumentHeader";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 613, true);
			this.Name = "ComplianceDocumentForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsPanel, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
