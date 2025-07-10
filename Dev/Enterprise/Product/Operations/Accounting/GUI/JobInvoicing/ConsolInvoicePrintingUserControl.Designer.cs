using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	partial class ConsolInvoicePrintingUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code


		public ZPanel ARJobInvoicingPrintingSecurityPanel;
		public ZLabel ARInvoicePrintingSecurityLabel;

		public ZPanel APJobInvoicingPrintingSecurityPanel;
		public ZLabel APInvoicePrintingSecurityLabel;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.InvoicePrintingTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ARInvoicesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.APInvoicesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.JobInvoicePrintingControl = new Enterprise.Accounting.GUI.JobInvoicing.JobInvoicePrintingControl();
			this.APInvoicePrintingUserControl = new Enterprise.Accounting.GUI.JobInvoicing.APInvoicePrintingUserControl();
			this.APDraftInvoicePrintingUserControl = new APDraftInvoicePrintingUserControl();

			this.ARJobInvoicingPrintingSecurityPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ARInvoicePrintingSecurityLabel = new Enterprise.ZArchitecture.ZLabel();

			this.APJobInvoicingPrintingSecurityPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.APInvoicePrintingSecurityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.APInvoiceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DraftInvoiceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.APInvoicePrintingSplitContainer = new CargoWise.Windows.UI.KSplitContainer();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoicePrintingTabControl.SuspendLayout();
			this.APDraftInvoicePrintingUserControl.SuspendLayout();
			this.ARInvoicesTabPage.SuspendLayout();
			this.APInvoicesTabPage.SuspendLayout();
			this.APInvoiceGroupBox.SuspendLayout();
			this.DraftInvoiceGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.APInvoicePrintingSplitContainer)).BeginInit();
			this.APInvoicePrintingSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.GUI.JobInvoicing.InvoicingPluginToConsol.JobARAPInvoicePrintingAggregator);
			// 
			// InvoicePrintingTabControl
			// 
			this.InvoicePrintingTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.InvoicePrintingTabControl.Controls.Add(this.ARInvoicesTabPage);
			this.InvoicePrintingTabControl.Controls.Add(this.APInvoicesTabPage);
			this.InvoicePrintingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoicePrintingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoicePrintingTabControl.Name = "InvoicePrintingTabControl";
			this.InvoicePrintingTabControl.SelectedIndex = 0;
			this.InvoicePrintingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 372, true);
			this.InvoicePrintingTabControl.TabIndex = 0;

			// 
			// ARInvoicePrintingSecurityLabel
			// 
			this.ARInvoicePrintingSecurityLabel.AutoSize = true;
			this.ARInvoicePrintingSecurityLabel.IsFontBold = true;
			this.ARInvoicePrintingSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 194, true);
			this.ARInvoicePrintingSecurityLabel.Name = "ARInvoicePrintingSecurityLabel";
			this.ARInvoicePrintingSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 13, true);
			this.ARInvoicePrintingSecurityLabel.TabIndex = 0;
			this.ARInvoicePrintingSecurityLabel.Text = "InvoicePrintingSecurityLabel";
			this.ARInvoicePrintingSecurityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ARJobInvoicingPrintingSecurityPanel
			// 
			this.ARJobInvoicingPrintingSecurityPanel.Controls.Add(this.ARInvoicePrintingSecurityLabel);
			this.ARJobInvoicingPrintingSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ARJobInvoicingPrintingSecurityPanel.Name = "ARJobInvoicingPrintingSecurityPanel";
			this.ARJobInvoicingPrintingSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 339, true);
			this.ARJobInvoicingPrintingSecurityPanel.TabIndex = 1;
			// 
			// ARInvoicesTabPage
			// 
			this.ARInvoicesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ARInvoicesTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolInvoicePrintingUserControl|9b9f4d28-8744-4836-8a54-f91f753ae8f4", "AR Invoices");
			this.ARInvoicesTabPage.Controls.Add(this.JobInvoicePrintingControl);
			this.ARInvoicesTabPage.Controls.Add(this.ARJobInvoicingPrintingSecurityPanel);
			this.ARInvoicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ARInvoicesTabPage.Name = "ARInvoicesTabPage";
			this.ARInvoicesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ARInvoicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 345, true);
			this.ARInvoicesTabPage.TabIndex = 0;

			// 
			// APInvoicePrintingSecurityLabel
			// 
			this.APInvoicePrintingSecurityLabel.AutoSize = true;
			this.APInvoicePrintingSecurityLabel.IsFontBold = true;
			this.APInvoicePrintingSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 194, true);
			this.APInvoicePrintingSecurityLabel.Name = "APInvoicePrintingSecurityLabel";
			this.APInvoicePrintingSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.APInvoicePrintingSecurityLabel.TabIndex = 0;
			this.APInvoicePrintingSecurityLabel.Text = "zLabel2";
			this.APInvoicePrintingSecurityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// APJobInvoicingPrintingSecurityPanel
			// 
			this.APJobInvoicingPrintingSecurityPanel.Controls.Add(this.APInvoicePrintingSecurityLabel);
			this.APJobInvoicingPrintingSecurityPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.APJobInvoicingPrintingSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.APJobInvoicingPrintingSecurityPanel.Name = "APJobInvoicingPrintingSecurityPanel";
			this.APJobInvoicingPrintingSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 339, true);
			this.APJobInvoicingPrintingSecurityPanel.TabIndex = 2;
			// 
			// APInvoicesTabPage
			// 
			this.APInvoicesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.APInvoicesTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolInvoicePrintingUserControl|248525fb-86a0-44be-a13f-98428bcd9a4f", "AP Invoices");
			this.APInvoicesTabPage.Controls.Add(this.APInvoicePrintingSplitContainer);
			this.APInvoicesTabPage.Controls.Add(this.APJobInvoicingPrintingSecurityPanel);
			this.APInvoicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.APInvoicesTabPage.Name = "APInvoicesTabPage";
			this.APInvoicesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.APInvoicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 345, true);
			this.APInvoicesTabPage.TabIndex = 1;
			// 
			// JobInvoicePrintingControl
			// 
			this.BindingSource.SetBindingMember(this.JobInvoicePrintingControl, "PrintingFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Accounting.Business.JobInvoicing.JobARInvoicePrintingFilter)(((Enterprise.Accounting.GUI.JobInvoicing.InvoicingPluginToConsol.JobARAPInvoicePrintingAggregator)(null)).PrintingFilter)));
			this.JobInvoicePrintingControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobInvoicePrintingControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.JobInvoicePrintingControl.Name = "JobInvoicePrintingControl";
			this.JobInvoicePrintingControl.PluginSecurity = null;
			this.JobInvoicePrintingControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 339, true);
			this.JobInvoicePrintingControl.TabIndex = 0;
			// 
			// APInvoicePrintingUserControl
			// 
			this.BindingSource.SetBindingMember(this.APInvoicePrintingUserControl, "APPrintingFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(((Enterprise.Accounting.GUI.JobInvoicing.InvoicingPluginToConsol.JobARAPInvoicePrintingAggregator)(null)).APPrintingFilter)));
			this.APInvoicePrintingUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.APInvoicePrintingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.APInvoicePrintingUserControl.Name = "APInvoicePrintingUserControl";
			this.APInvoicePrintingUserControl.PluginSecurity = null;
			this.APInvoicePrintingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 169, true);
			this.APInvoicePrintingUserControl.TabIndex = 0;
			// 
			// DraftInvoiceListUserControl
			// 
			this.APDraftInvoicePrintingUserControl.AllowDrop = true;
			this.APDraftInvoicePrintingUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.APDraftInvoicePrintingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.APDraftInvoicePrintingUserControl.Name = "DraftInvoiceListUserControl";
			this.APDraftInvoicePrintingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 169, true);
			this.APDraftInvoicePrintingUserControl.TabIndex = 0;
			// 
			// APInvoiceGroupBox
			// 
			this.APInvoiceGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("907A8F7D-9E7A-4DB5-82E7-F5D1285A8AC6", "AP Invoices");
			this.APInvoiceGroupBox.Controls.Add(this.APInvoicePrintingUserControl);
			this.APInvoiceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.APInvoiceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.APInvoiceGroupBox.Name = "APInvoiceGroupBox";
			this.APInvoiceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 169, true);
			this.APInvoiceGroupBox.TabIndex = 0;
			this.APInvoiceGroupBox.TabStop = false;
			// 
			// DraftInvoiceGroupBox
			// 
			this.DraftInvoiceGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("C413CB00-5023-4670-82C5-ED865427D332", "Draft Invoices");
			this.DraftInvoiceGroupBox.Controls.Add(this.APDraftInvoicePrintingUserControl);
			this.DraftInvoiceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DraftInvoiceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DraftInvoiceGroupBox.Name = "DraftInvoiceGroupBox";
			this.DraftInvoiceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 169, true);
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
			this.APInvoicePrintingSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 339);
			this.APInvoicePrintingSplitContainer.SplitterDistance = 199;
			this.APInvoicePrintingSplitContainer.TabIndex = 0;
			// 
			// ConsolInvoicePrintingUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoicePrintingTabControl);
			this.Name = "ConsolInvoicePrintingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 372, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoicePrintingTabControl.ResumeLayout(false);
			this.ARInvoicesTabPage.ResumeLayout(false);
			this.APInvoicesTabPage.ResumeLayout(false);
			this.APDraftInvoicePrintingUserControl.ResumeLayout(false);
			this.APDraftInvoicePrintingUserControl.PerformLayout();
			this.APInvoicePrintingSplitContainer.ResumeLayout(false);
			this.APInvoicePrintingSplitContainer.PerformLayout();
			this.APInvoiceGroupBox.ResumeLayout(false);
			this.APInvoiceGroupBox.PerformLayout();
			this.DraftInvoiceGroupBox.ResumeLayout(false);
			this.DraftInvoiceGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.APInvoicePrintingSplitContainer)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTabControl InvoicePrintingTabControl;
		public Enterprise.ZArchitecture.GUI.ZTabPage ARInvoicesTabPage;
		public Enterprise.ZArchitecture.GUI.ZTabPage APInvoicesTabPage;
		public JobInvoicePrintingControl JobInvoicePrintingControl;
		public APInvoicePrintingUserControl APInvoicePrintingUserControl;
		public APDraftInvoicePrintingUserControl APDraftInvoicePrintingUserControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox APInvoiceGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DraftInvoiceGroupBox;
		public CargoWise.Windows.UI.KSplitContainer APInvoicePrintingSplitContainer;
	}
}
