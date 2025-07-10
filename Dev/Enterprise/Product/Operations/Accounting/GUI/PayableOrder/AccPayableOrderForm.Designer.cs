using System;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Accounting.GUI.PayableOrder
{
	partial class AccPayableOrderForm : ZForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage OrdersTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage NotesTabPage;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl OrderTabControl;
		private ZWorkflowTabPage WorkflowTabPage;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Order != null)
				{
					Order.ShowApprovalMessage -= new AccPayableOrderHeader.ApprovalMessageEventHandler(Order_ShowApprovalMessage);
				}
				if (SecurityOverrideProviderSource.Get(BusinessEntity) != null)
				{
					SecurityOverrideProviderSource.Get(BusinessEntity).Provider = null;
				}
			}
			
			if(components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.OrdersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OrdersUserControl = new Enterprise.Accounting.GUI.PayableOrder.AccPayableOrderUserControlDecider();
			this.NotesTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.OrderTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.OrderTabControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 487, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = 1001;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(768, 462, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 25, true);
			this.PostingButtonsUserControl.TabIndex = 2;
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OrdersForm|08709d13-cfb5-4b5c-bd00-b710ebbe8bcc", "Order");
			this.OrdersTabPage.Controls.Add(this.OrdersUserControl);
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OrdersTabPage.Name = "OrdersTabPage";
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 429, true);
			this.OrdersTabPage.TabIndex = 0;
			// 
			// OrdersUserControl
			// 
			this.OrdersUserControl.AllowDrop = true;
			this.OrdersUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrdersUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrdersUserControl.Name = "OrdersUserControl";
			this.OrdersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 429, true);
			this.OrdersUserControl.TabIndex = 0;
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NotesTabPage.Name = "NotesTabPage";
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 600, true);
			this.NotesTabPage.TabIndex = 4;
			// 
			// OrderTabControl
			// 
			this.OrderTabControl.Controls.Add(this.OrdersTabPage);
			this.OrderTabControl.Controls.Add(this.WorkflowTabPage);
			this.OrderTabControl.Controls.Add(this.NotesTabPage);
			this.OrderTabControl.Controls.Add(this.zLogsTabPage1);
			this.OrderTabControl.ItemSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 19, true);
			this.OrderTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrderTabControl.Name = "OrderTabControl";
			this.OrderTabControl.SelectedIndex = 0;
			this.OrderTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 456, true);
			this.OrderTabControl.TabIndex = 1;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OrdersForm|e32ad04f-ff4d-4489-a8cf-221f864f61cf", "Workflow");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 620, true);
			this.WorkflowTabPage.TabIndex = 7;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 620, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// AccPayableOrderForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 511, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.OrderTabControl);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader);
			this.Name = "AccPayableOrderForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.OrderTabControl, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.OrdersTabPage.ResumeLayout(false);
			this.OrdersTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.OrderTabControl.ResumeLayout(false);
			this.OrderTabControl.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

	}
}