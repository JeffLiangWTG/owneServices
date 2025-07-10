namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class OverrideInvoiceDetailsForm
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
			this.UpperPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InvoicesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UpperLabelRegistryMessage = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.UpperPanel.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).BeginInit();
			this.InvoicesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 159, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.OverrideTransactionDescriptionHelper);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Controls.Add(this.ContinueButton);
			this.BottomPanel.Controls.Add(this.CloseButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 123, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 36, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 6, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			// 
			// ContinueButton
			// 
			this.ContinueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ContinueButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bd2c15e3-d976-49ee-ac2a-19d20cac72cf", "Continue");
			this.ContinueButton.IsCaptionOverridden = false;
			this.ContinueButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 6, true);
			this.ContinueButton.Name = "ContinueButton";
			this.ContinueButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ContinueButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.ContinueButton.TabIndex = 0;
			this.ContinueButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ContinueButton.ToolTipCaption = null;
			this.ContinueButton.Click += new System.EventHandler(this.ContinueButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0a01216b-b732-4faa-a0c5-2a760a7a2947", "Cancel");
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(465, 6, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.CloseButton.TabIndex = 0;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// UpperPanel
			// 
			this.UpperPanel.Controls.Add(this.UpperLabelRegistryMessage);
			this.UpperPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.UpperPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UpperPanel.Name = "UpperPanel";
			this.UpperPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 36, true);
			this.UpperPanel.TabIndex = 1;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.InvoicesGrid);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 36, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 87, true);
			this.MainPanel.TabIndex = 0;
			// 
			// InvoicesGrid
			// 
			this.InvoicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InvoicesGrid, "WrappedObjects");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.OverrideTransactionDescriptionHelper)(null)).WrappedObjects)));
			this.InvoicesGrid.CaptionVisible = false;
			this.InvoicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoicesGrid.GridId = "EE801F09-F86B-4301-9F44-7D1AE6E737B7";
			this.InvoicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoicesGrid.LayoutKey = "InvoicesGrid";
			this.InvoicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoicesGrid.Name = "InvoicesGrid";
			this.InvoicesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.InvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 87, true);
			this.InvoicesGrid.TabIndex = 0;
			// 
			// UpperLabelRegistryMessage
			// 
			this.UpperLabelRegistryMessage.AutoSize = true;
			this.UpperLabelRegistryMessage.Name = "UpperLabelRegistryMessage";
			this.UpperLabelRegistryMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 9, true);
			this.UpperLabelRegistryMessage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("78EB71D1-33F2-4FA0-A4B2-47E309182746", "Label");
			this.UpperLabelRegistryMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 13, true);
			this.UpperLabelRegistryMessage.TabIndex = 1;
			// 
			// OverrideInvoiceDetailsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 183, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.UpperPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.OverrideTransactionDescriptionHelper);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 200, true);
			this.Name = "OverrideInvoiceDetailsForm";
			this.Text = "OverrideTransactionDescriptionForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.UpperPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.UpperPanel.ResumeLayout(false);
			this.UpperPanel.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).EndInit();
			this.InvoicesGrid.ResumeLayout(false);
			this.InvoicesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel BottomPanel;
		protected ZArchitecture.GUI.ZPanel UpperPanel;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZArchitecture.GUI.ZPanel MainPanel;
		protected ZArchitecture.ZGrid InvoicesGrid;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected Enterprise.ZArchitecture.GUI.ZButton ContinueButton;
		protected Enterprise.ZArchitecture.ZLabel UpperLabelRegistryMessage;
		
	}
}
