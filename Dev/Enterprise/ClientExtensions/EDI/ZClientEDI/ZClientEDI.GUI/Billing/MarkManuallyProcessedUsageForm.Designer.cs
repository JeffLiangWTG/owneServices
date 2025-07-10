namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class MarkManuallyProcessedUsageForm
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
			this.StartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.markButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OrgTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.helpLabel = new Enterprise.ZArchitecture.ZLabel();
			this.unmarkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StartDateEdit.SuspendLayout();
			this.EndDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 200, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 5, true);
			this.MainStatusBar.Visible = false;
			// 
			// StartDateEdit
			// 
			this.StartDateEdit.AllowDrop = true;
			this.StartDateEdit.AutoCompleteMonthThreshold = 1;
			this.StartDateEdit.AutoCompleteYear = true;
			this.StartDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("614de477-3f5c-42c4-9234-6f782f15815a", "Date From");
			this.StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 68, true);
			this.StartDateEdit.Name = "StartDateEdit";
			this.StartDateEdit.TabIndex = 1;
			// 
			// CodeTextBox
			// 
			this.CodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CodeTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("dbc367dc-458d-4985-9d16-44df7b3ec12a", "Usage Codes");
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 96, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 17, true);
			this.CodeTextBox.TabIndex = 2;
			// 
			// markButton
			// 
			this.markButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("f531590e-0b88-4799-88d7-928ee06a0f23", "Mark");
			this.markButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 158, true);
			this.markButton.Name = "markButton";
			this.markButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.markButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 23, true);
			this.markButton.TabIndex = 5;
			this.markButton.ToolTipCaption = null;
			this.markButton.UseVisualStyleBackColor = true;
			this.markButton.Click += new System.EventHandler(this.MarkButton_Click);
			// 
			// EndDateEdit
			// 
			this.EndDateEdit.AllowDrop = true;
			this.EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.EndDateEdit.AutoCompleteYear = true;
			this.EndDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("3ee15b7c-a7c0-4738-a7c7-02d68be313d0", "Date To");
			this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 68, true);
			this.EndDateEdit.Name = "EndDateEdit";
			this.EndDateEdit.TabIndex = 6;
			// 
			// OrgTextBox
			// 
			this.OrgTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OrgTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("4146e6f7-ebb5-4d3e-82e7-f3c30417fd86", "Organization Codes");
			this.OrgTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 122, true);
			this.OrgTextBox.Name = "OrgTextBox";
			this.OrgTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 17, true);
			this.OrgTextBox.TabIndex = 7;
			// 
			// helpLabel
			// 
			this.helpLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.helpLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 10, true);
			this.helpLabel.Name = "helpLabel";
			this.helpLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 44, true);
			this.helpLabel.TabIndex = 33;
			this.helpLabel.Text = "Marks as manually processed, non-invoiced usage, to exclude it from billing. Matc" +
    "hes by usage code and organization code. Organization code is optional. Separate" +
    " multiple codes with a comma.";
			// 
			// unmarkButton
			// 
			this.unmarkButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("d34226aa-8102-434e-a9ff-82a1eb311cbf", "Un-mark");
			this.unmarkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 158, true);
			this.unmarkButton.Name = "unmarkButton";
			this.unmarkButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.unmarkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 23, true);
			this.unmarkButton.TabIndex = 34;
			this.unmarkButton.ToolTipCaption = null;
			this.unmarkButton.UseVisualStyleBackColor = true;
			this.unmarkButton.Click += new System.EventHandler(this.UnmarkButton_Click);
			// 
			// MarkManuallyProcessedUsageForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 205, true);
			this.Controls.Add(this.unmarkButton);
			this.Controls.Add(this.helpLabel);
			this.Controls.Add(this.OrgTextBox);
			this.Controls.Add(this.EndDateEdit);
			this.Controls.Add(this.markButton);
			this.Controls.Add(this.CodeTextBox);
			this.Controls.Add(this.StartDateEdit);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 240, true);
			this.Name = "MarkManuallyProcessedUsageForm";
			this.Text = "Mark Manually Processed Usage";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.StartDateEdit, 0);
			this.Controls.SetChildIndex(this.CodeTextBox, 0);
			this.Controls.SetChildIndex(this.markButton, 0);
			this.Controls.SetChildIndex(this.EndDateEdit, 0);
			this.Controls.SetChildIndex(this.OrgTextBox, 0);
			this.Controls.SetChildIndex(this.helpLabel, 0);
			this.Controls.SetChildIndex(this.unmarkButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StartDateEdit.ResumeLayout(true);
			this.StartDateEdit.PerformLayout();
			this.EndDateEdit.ResumeLayout(true);
			this.EndDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZDateEdit StartDateEdit;
		public ZArchitecture.ZTextBox CodeTextBox;
		private ZArchitecture.GUI.ZButton markButton;
		public ZArchitecture.GUI.ZDateEdit EndDateEdit;
		public ZArchitecture.ZTextBox OrgTextBox;
		private ZArchitecture.ZLabel helpLabel;
		private ZArchitecture.GUI.ZButton unmarkButton;
	}
}