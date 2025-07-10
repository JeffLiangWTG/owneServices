namespace Enterprise.Accounting.GUI.JobInvoicing
{
	partial class ChangeTransactionDatesMessageBox
	{
		private System.ComponentModel.IContainer components = null;

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
			this.YesNoAllPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.YesButtonOnYesNoAllPanel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NoButtonOnYesNoAllPanel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.YesToAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NoToAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RevenueRecognitionDatesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.PostDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.InvoiceDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.YesButtonOnYesNoCancelPanel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NoButtonOnYesNoCancelPanel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButtonOnYesNoCancelPanel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.YesNoCancelPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.YesNoAllPanel.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.YesNoCancelPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 387, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 24, true);
			this.MainStatusBar.TabIndex = 3;
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.JobInvoicing.ChangeTransactionDatesBusinessObject);
			// 
			// YesNoAllPanel
			// 
			this.YesNoAllPanel.Controls.Add(this.YesButtonOnYesNoAllPanel);
			this.YesNoAllPanel.Controls.Add(this.NoButtonOnYesNoAllPanel);
			this.YesNoAllPanel.Controls.Add(this.YesToAllButton);
			this.YesNoAllPanel.Controls.Add(this.NoToAllButton);
			this.YesNoAllPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.YesNoAllPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 297, true);
			this.YesNoAllPanel.Name = "YesNoAllPanel";
			this.YesNoAllPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 34, true);
			this.YesNoAllPanel.TabIndex = 1;
			// 
			// YesButtonOnYesNoAllPanel
			// 
			this.YesButtonOnYesNoAllPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.YesButtonOnYesNoAllPanel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChangeTransactionDatesMessageBox|91ab293d-0934-4a08-851d-c7937003199a", "&Yes");
			this.YesButtonOnYesNoAllPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 7, true);
			this.YesButtonOnYesNoAllPanel.Name = "YesButtonOnYesNoAllPanel";
			this.YesButtonOnYesNoAllPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.YesButtonOnYesNoAllPanel.TabIndex = 0;
			this.YesButtonOnYesNoAllPanel.Click += new System.EventHandler(this.YesButton_Click);
			// 
			// NoButtonOnYesNoAllPanel
			// 
			this.NoButtonOnYesNoAllPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.NoButtonOnYesNoAllPanel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChangeTransactionDatesMessageBox|f3da57f0-2281-4f46-a122-5b03cba5194f", "&No");
			this.NoButtonOnYesNoAllPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 7, true);
			this.NoButtonOnYesNoAllPanel.Name = "NoButtonOnYesNoAllPanel";
			this.NoButtonOnYesNoAllPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.NoButtonOnYesNoAllPanel.TabIndex = 1;
			this.NoButtonOnYesNoAllPanel.Click += new System.EventHandler(this.NoButton_Click);
			// 
			// YesToAllButton
			// 
			this.YesToAllButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.YesToAllButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChangeTransactionDatesMessageBox|fe114c39-98cb-498f-a04f-1c431a3dbbf1", "Yes to All");
			this.YesToAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 7, true);
			this.YesToAllButton.Name = "YesToAllButton";
			this.YesToAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.YesToAllButton.TabIndex = 2;
			this.YesToAllButton.Click += new System.EventHandler(this.YesToAllButton_Click);
			// 
			// NoToAllButton
			// 
			this.NoToAllButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.NoToAllButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChangeTransactionDatesMessageBox|1db27ca6-a00d-4e56-9a2f-eb6a369f2073", "No to All");
			this.NoToAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(465, 7, true);
			this.NoToAllButton.Name = "NoToAllButton";
			this.NoToAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.NoToAllButton.TabIndex = 3;
			this.NoToAllButton.Click += new System.EventHandler(this.NoToAllButton_Click);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.RevenueRecognitionDatesTextBox);
			this.MainPanel.Controls.Add(this.zLabel2);
			this.MainPanel.Controls.Add(this.PostDateDateEdit);
			this.MainPanel.Controls.Add(this.InvoiceDateDateEdit);
			this.MainPanel.Controls.Add(this.zLabel1);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 297, true);
			this.MainPanel.TabIndex = 0;
			// 
			// RevenueRecognitionDatesTextBox
			// 
			this.BindingSource.SetBindingMember(this.RevenueRecognitionDatesTextBox, "RevenueRecognitionDates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ChangeTransactionDatesBusinessObject)(null)).RevenueRecognitionDates)));
			this.RevenueRecognitionDatesTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChangeTransactionDatesMessageBox|d3d3ca58-69a4-4324-9130-7017a3902019", "Revenue Recognition Dates");
			this.RevenueRecognitionDatesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 256, true);
			this.RevenueRecognitionDatesTextBox.Name = "RevenueRecognitionDatesTextBox";
			this.RevenueRecognitionDatesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 20, true);
			this.RevenueRecognitionDatesTextBox.TabIndex = 4;
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChangeTransactionDatesMessageBox|7cc6ce31-51f2-4374-8f46-9d7c5e12af3c", "", "Answering YES: Your invoice/s and credit note/s will be posted using the above \'Post Date\'.\r\nThe invoice document will display the above \'Invoice Date\' and the \'Due Date\' will also be calculated from that date.\r\n\r\nAnswering NO: Your invoice/s and credit note/s will be posted using today\'s date.\r\nThe invoice document will display the today\'s date as the \'Invoice Date\' and the \'Due Date\' will be calculated from today\'s date.\r\n\r\n\r\nNOTE: Your current registry setup allows you to do this.\r\nYou can disable this function by changing the \'Accounting -> Job Invoicing -> Back Date AR Invoices -> Back Date Invoice Configuration registry item, available through Maintain -> System -> Registry.");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 99, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 130, true);
			this.zLabel2.TabIndex = 3;
			// 
			// PostDateDateEdit
			// 
			this.PostDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateDateEdit, "PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.JobInvoicing.ChangeTransactionDatesBusinessObject)(null)).PostDate)));
			this.PostDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChangeTransactionDatesMessageBox|bcc36a3c-d2e1-474d-8b83-8ae0badea199", "Post Date");
			this.PostDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 64, true);
			this.PostDateDateEdit.Name = "PostDateDateEdit";
			this.PostDateDateEdit.TabIndex = 2;
			// 
			// InvoiceDateDateEdit
			// 
			this.InvoiceDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.InvoiceDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InvoiceDateDateEdit, "InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.JobInvoicing.ChangeTransactionDatesBusinessObject)(null)).InvoiceDate)));
			this.InvoiceDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChangeTransactionDatesMessageBox|4a4c4c8a-3f76-4a33-9341-2a93a2c61167", "Invoice Date");
			this.InvoiceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 38, true);
			this.InvoiceDateDateEdit.Name = "InvoiceDateDateEdit";
			this.InvoiceDateDateEdit.TabIndex = 1;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChangeTransactionDatesMessageBox|626e1483-e6cd-4617-a23d-685743176f9c", "", "Do you want the receivables invoice/s and credit note/s posted here to have an \'invoice date\' and \'post date\' of:                       .");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 13, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 13, true);
			this.zLabel1.TabIndex = 0;
			// 
			// YesButtonOnYesNoCancelPanel
			// 
			this.YesButtonOnYesNoCancelPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.YesButtonOnYesNoCancelPanel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChangeTransactionDatesMessageBox|ba998bba-aa6c-4d13-8745-8406d2e9dd70", "&Yes");
			this.YesButtonOnYesNoCancelPanel.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.YesButtonOnYesNoCancelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(265, 7, true);
			this.YesButtonOnYesNoCancelPanel.Name = "YesButtonOnYesNoCancelPanel";
			this.YesButtonOnYesNoCancelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.YesButtonOnYesNoCancelPanel.TabIndex = 0;
			// 
			// NoButtonOnYesNoCancelPanel
			// 
			this.NoButtonOnYesNoCancelPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.NoButtonOnYesNoCancelPanel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChangeTransactionDatesMessageBox|9382911f-a72b-4789-9904-6b7278f78e50", "&No");
			this.NoButtonOnYesNoCancelPanel.DialogResult = System.Windows.Forms.DialogResult.No;
			this.NoButtonOnYesNoCancelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 7, true);
			this.NoButtonOnYesNoCancelPanel.Name = "NoButtonOnYesNoCancelPanel";
			this.NoButtonOnYesNoCancelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.NoButtonOnYesNoCancelPanel.TabIndex = 1;
			// 
			// CancelButtonOnYesNoCancelPanel
			// 
			this.CancelButtonOnYesNoCancelPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.CancelButtonOnYesNoCancelPanel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChangeTransactionDatesMessageBox|289b6e69-77f6-450b-b4ed-fae2fa1f58a9", "&Cancel");
			this.CancelButtonOnYesNoCancelPanel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonOnYesNoCancelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 7, true);
			this.CancelButtonOnYesNoCancelPanel.Name = "CancelButtonOnYesNoCancelPanel";
			this.CancelButtonOnYesNoCancelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelButtonOnYesNoCancelPanel.TabIndex = 2;
			// 
			// YesNoCancelPanel
			// 
			this.YesNoCancelPanel.Controls.Add(this.YesButtonOnYesNoCancelPanel);
			this.YesNoCancelPanel.Controls.Add(this.NoButtonOnYesNoCancelPanel);
			this.YesNoCancelPanel.Controls.Add(this.CancelButtonOnYesNoCancelPanel);
			this.YesNoCancelPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.YesNoCancelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 331, true);
			this.YesNoCancelPanel.Name = "YesNoCancelPanel";
			this.YesNoCancelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 34, true);
			this.YesNoCancelPanel.TabIndex = 4;
			// 
			// ChangeTransactionDatesMessageBox
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 411, true);
			this.Controls.Add(this.YesNoCancelPanel);
			this.Controls.Add(this.YesNoAllPanel);
			this.Controls.Add(this.MainPanel);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.JobInvoicing.ChangeTransactionDatesBusinessObject);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "ChangeTransactionDatesMessageBox";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "ChangeTransactionDatesMessageBox";
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.YesNoAllPanel, 0);
			this.Controls.SetChildIndex(this.YesNoCancelPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.YesNoAllPanel.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.YesNoCancelPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZPanel YesNoAllPanel;
		protected Enterprise.ZArchitecture.GUI.ZButton YesButtonOnYesNoAllPanel;
		protected Enterprise.ZArchitecture.GUI.ZButton NoButtonOnYesNoAllPanel;
		protected Enterprise.ZArchitecture.GUI.ZButton YesToAllButton;
		protected Enterprise.ZArchitecture.GUI.ZButton NoToAllButton;
		private Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.ZLabel zLabel2;
		private Enterprise.ZArchitecture.GUI.ZDateEdit PostDateDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit InvoiceDateDateEdit;
		private Enterprise.ZArchitecture.ZTextBox RevenueRecognitionDatesTextBox;
		protected Enterprise.ZArchitecture.GUI.ZPanel YesNoCancelPanel;
		protected Enterprise.ZArchitecture.GUI.ZButton YesButtonOnYesNoCancelPanel;
		protected Enterprise.ZArchitecture.GUI.ZButton NoButtonOnYesNoCancelPanel;
		protected Enterprise.ZArchitecture.GUI.ZButton CancelButtonOnYesNoCancelPanel;
	}
}
