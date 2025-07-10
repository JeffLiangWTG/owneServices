namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class OverrideReceiptPaymentDetailsForm
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
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.ContinueButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ReceiptPaymentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReceiptPaymentsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 201, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.ReceiptPayment.OverrideReceiptPaymentCashFlowCategoryHelper);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Controls.Add(this.ContinueButton);
			this.BottomPanel.Controls.Add(this.CloseButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 165, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 36, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(291, 6, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			// 
			// ContinueButton
			// 
			this.ContinueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ContinueButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3EF96998-1CC0-4B88-826C-A7B4CF310486", "Continue");
			this.ContinueButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 6, true);
			this.ContinueButton.Name = "ContinueButton";
			this.ContinueButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.ContinueButton.TabIndex = 0;
			this.ContinueButton.Click += new System.EventHandler(this.ContinueButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0B0BCBB7-DDEE-4554-A679-35EA868F3626", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(465, 6, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.CloseButton.TabIndex = 0;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.ReceiptPaymentsGrid);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 165, true);
			this.MainPanel.TabIndex = 0;
			// 
			// ReceiptPaymentsGrid
			// 
			this.ReceiptPaymentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReceiptPaymentsGrid, "WrappedObjects");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.ReceiptPayment.OverrideReceiptPaymentCashFlowCategoryHelper)(null)).WrappedObjects)));
			this.ReceiptPaymentsGrid.CaptionVisible = false;
			this.ReceiptPaymentsGrid.CopySelectedRowsAllowed = true;
			this.ReceiptPaymentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReceiptPaymentsGrid.GridId = "DADDB5AF-9BBE-4FC4-9F13-76A6EF26E99B";
			this.ReceiptPaymentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReceiptPaymentsGrid.LayoutKey = "ReceiptPaymentsGrid";
			this.ReceiptPaymentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReceiptPaymentsGrid.Name = "ReceiptPaymentsGrid";
			this.ReceiptPaymentsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ReceiptPaymentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 165, true);
			this.ReceiptPaymentsGrid.TabIndex = 0;
			// 
			// OverrideReceiptPaymentDetailsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 225, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.OverrideTransactionDescriptionHelper);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 200, true);
			this.Name = "OverrideReceiptPaymentDetailsForm";
			this.Text = "OverrideReceiptPaymentDetailsForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ReceiptPaymentsGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private ZArchitecture.GUI.ZPanel BottomPanel;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZArchitecture.GUI.ZPanel MainPanel;
		protected ZArchitecture.ZGrid ReceiptPaymentsGrid;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected Enterprise.ZArchitecture.GUI.ZButton ContinueButton;
	}
}