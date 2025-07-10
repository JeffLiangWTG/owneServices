namespace Enterprise.Accounting.GUI.JobInvoicing
{
	partial class ReversalDatesForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.MessagePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ReversedInvoicesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.YesNoCancelPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.YesButtonOnYesNoCancelPanel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NoButtonOnYesNoCancelPanel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButtonOnYesNoCancelPanel = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessagePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReversedInvoicesGrid)).BeginInit();
			this.GridPanel.SuspendLayout();
			this.YesNoCancelPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 380, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(927, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Transaction.TransactionHeaderCollection);
			// 
			// MessagePanel
			// 
			this.MessagePanel.Controls.Add(this.ReversedInvoicesGrid);
			this.MessagePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 182, true);
			this.MessagePanel.Name = "MessagePanel";
			this.MessagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(927, 166, true);
			this.MessagePanel.TabIndex = 1;
			// 
			// ReversedInvoicesGrid
			// 
			this.ReversedInvoicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReversedInvoicesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(null)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(null)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(null)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(null)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(null)).UnmatchDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(null)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(null)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(null)).AH_OSTotalAmount)));
			this.ReversedInvoicesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = null;
			zTextBoxColumnStyleInfo1.ColumnName = "AH_TransactionType";
			zOrganisationFindBoxColumnStyleInfo1.Caption = null;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = null;
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = null;
			zDateEditColumnStyleInfo1.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = null;
			zDateEditColumnStyleInfo2.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Caption = null;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c2051599-1243-41e7-bab9-5e4a462670cb", "Unmatch Date");
			zDateEditColumnStyleInfo3.ColumnName = "UnmatchDate";
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = null;
			zTextBoxColumnStyleInfo3.ColumnName = "AH_RX_NKTransactionCurrency";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AH_OSTotalAmount";
			this.ReversedInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReversedInvoicesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ReversedInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ReversedInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ReversedInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ReversedInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReversedInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ReversedInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ReversedInvoicesGrid.CopySelectedRowsAllowed = true;
			this.ReversedInvoicesGrid.DataSource = this.BindingSource;
			this.ReversedInvoicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReversedInvoicesGrid.GridId = "0bda40ed-8013-4ce3-817b-03d7f2bf3406";
			this.ReversedInvoicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReversedInvoicesGrid.LayoutKey = "ReversedInvoicesGrid";
			this.ReversedInvoicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReversedInvoicesGrid.Name = "ReversedInvoicesGrid";
			this.ReversedInvoicesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ReversedInvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(927, 166, true);
			this.ReversedInvoicesGrid.TabIndex = 0;
			// 
			// GridPanel
			// 
			this.GridPanel.Controls.Add(this.MessageLabel);
			this.GridPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.GridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GridPanel.Name = "GridPanel";
			this.GridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(927, 182, true);
			this.GridPanel.TabIndex = 2;
			// 
			// MessageLabel
			// 
			this.MessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("70ae1806-6fbd-44c0-9451-90eb09f041f8", "Do you want the receivables invoice/s and credit note/s reversed here to have the following invoice dates and post dates?\r\n\r\nAnswering YES: Your invoice/s and credit note/s will be reversed using the Post Dates below.\r\nThe invoice document will display the below Invoice Date and the Due Date will also be calculated from that date.\r\n\r\nAnswering NO: Your invoice/s and credit note/s will be reversed using today\'s date.\r\nThe invoice document will display today\'s date as the Invoice Date and the Due Date will be calculated from today\'s date.\r\n\r\nNOTE: Your current registry setup allows you to do this.\r\nYou can disable this function by changing the \'Accounting -> Job Invoicing -> Back Date AR Invoices -> Back Date Invoice Configuration\' registry item, available through Maintain -> System -> Registry.");
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 13, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 166, true);
			this.MessageLabel.TabIndex = 0;
			// 
			// YesNoCancelPanel
			// 
			this.YesNoCancelPanel.Controls.Add(this.YesButtonOnYesNoCancelPanel);
			this.YesNoCancelPanel.Controls.Add(this.NoButtonOnYesNoCancelPanel);
			this.YesNoCancelPanel.Controls.Add(this.CancelButtonOnYesNoCancelPanel);
			this.YesNoCancelPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.YesNoCancelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 348, true);
			this.YesNoCancelPanel.Name = "YesNoCancelPanel";
			this.YesNoCancelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(927, 26, true);
			this.YesNoCancelPanel.TabIndex = 5;
			// 
			// YesButtonOnYesNoCancelPanel
			// 
			this.YesButtonOnYesNoCancelPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.YesButtonOnYesNoCancelPanel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f7cfc663-0cef-4a84-b0ce-ca43061f2ee5", "Yes");
			this.YesButtonOnYesNoCancelPanel.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.YesButtonOnYesNoCancelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 2, true);
			this.YesButtonOnYesNoCancelPanel.Name = "YesButtonOnYesNoCancelPanel";
			this.YesButtonOnYesNoCancelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.YesButtonOnYesNoCancelPanel.TabIndex = 0;
			this.YesButtonOnYesNoCancelPanel.Click += new System.EventHandler(this.YesButtonOnYesNoCancelPanel_Click);
			// 
			// NoButtonOnYesNoCancelPanel
			// 
			this.NoButtonOnYesNoCancelPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.NoButtonOnYesNoCancelPanel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6c5d689a-fa23-44c3-a0dd-291cc9427aba", "No");
			this.NoButtonOnYesNoCancelPanel.DialogResult = System.Windows.Forms.DialogResult.No;
			this.NoButtonOnYesNoCancelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 2, true);
			this.NoButtonOnYesNoCancelPanel.Name = "NoButtonOnYesNoCancelPanel";
			this.NoButtonOnYesNoCancelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.NoButtonOnYesNoCancelPanel.TabIndex = 1;
			this.NoButtonOnYesNoCancelPanel.Click += new System.EventHandler(this.NoButtonOnYesNoCancelPanel_Click);
			// 
			// CancelButtonOnYesNoCancelPanel
			// 
			this.CancelButtonOnYesNoCancelPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.CancelButtonOnYesNoCancelPanel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("898a5aa6-c38f-4e83-8171-64d6748b6f1c", "Cancel");
			this.CancelButtonOnYesNoCancelPanel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonOnYesNoCancelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(506, 2, true);
			this.CancelButtonOnYesNoCancelPanel.Name = "CancelButtonOnYesNoCancelPanel";
			this.CancelButtonOnYesNoCancelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelButtonOnYesNoCancelPanel.TabIndex = 2;
			this.CancelButtonOnYesNoCancelPanel.Click += new System.EventHandler(this.CancelButtonOnYesNoCancelPanel_Click);
			// 
			// ReversalDatesForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(927, 404, true);
			this.Controls.Add(this.YesNoCancelPanel);
			this.Controls.Add(this.MessagePanel);
			this.Controls.Add(this.GridPanel);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Transaction.TransactionHeaderCollection);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "ReversalDatesForm";
			this.Text = "ReversalDatesForm";
			this.Controls.SetChildIndex(this.GridPanel, 0);
			this.Controls.SetChildIndex(this.MessagePanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.YesNoCancelPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagePanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ReversedInvoicesGrid)).EndInit();
			this.GridPanel.ResumeLayout(false);
			this.YesNoCancelPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZPanel MessagePanel;
		private ZArchitecture.GUI.ZPanel GridPanel;
		protected ZArchitecture.GUI.ZPanel YesNoCancelPanel;
		public ZArchitecture.GUI.ZButton YesButtonOnYesNoCancelPanel;
		public ZArchitecture.GUI.ZButton NoButtonOnYesNoCancelPanel;
		public ZArchitecture.GUI.ZButton CancelButtonOnYesNoCancelPanel;
		private ZArchitecture.ZGrid ReversedInvoicesGrid;
		private ZArchitecture.ZLabel MessageLabel;
	}
}