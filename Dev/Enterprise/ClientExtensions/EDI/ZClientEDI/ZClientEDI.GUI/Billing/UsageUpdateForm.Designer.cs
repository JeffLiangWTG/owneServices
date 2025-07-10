namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class UsageUpdateForm
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
			this.UsageCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PriceCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.helpLabel = new Enterprise.ZArchitecture.ZLabel();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.keyRefIndexTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.periodTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.updateConsolidationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 228, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 5, true);
			this.MainStatusBar.Visible = false;
			// 
			// UsageCodeTextBox
			// 
			this.UsageCodeTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("36f8860f-cf06-49f5-91fa-e36654d941ba", "Usage Code");
			this.UsageCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 74, true);
			this.UsageCodeTextBox.Name = "UsageCodeTextBox";
			this.UsageCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 20, true);
			this.UsageCodeTextBox.TabIndex = 3;
			// 
			// okButton
			// 
			this.okButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("bca4c972-f210-4514-bb7f-7472e483ca3b", "OK");
			this.okButton.IsCaptionOverridden = false;
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 190, true);
			this.okButton.Name = "okButton";
			this.okButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 23, true);
			this.okButton.TabIndex = 7;
			this.okButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.okButton.ToolTipCaption = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// PriceCodeTextBox
			// 
			this.PriceCodeTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("3daaecd7-a352-477e-abd7-de17f7e3978a", "Price Code (Optional)");
			this.PriceCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 101, true);
			this.PriceCodeTextBox.Name = "PriceCodeTextBox";
			this.PriceCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 17, true);
			this.PriceCodeTextBox.TabIndex = 4;
			this.PriceCodeTextBox.TextChanged += new System.EventHandler(this.PriceCodeTextBox_TextChanged);
			// 
			// helpLabel
			// 
			this.helpLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.helpLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.helpLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 10, true);
			this.helpLabel.Name = "helpLabel";
			this.helpLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 29, true);
			this.helpLabel.TabIndex = 1;
			this.helpLabel.Text = "Update Chargeable Usage from raw data.";
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("caab3ae4-a3be-46ea-a058-a53e73c11ab9", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.IsCaptionOverridden = false;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(198, 190, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 23, true);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// keyRefIndexTextBox
			// 
			this.keyRefIndexTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("e85c60b8-5f2c-4bb8-9b76-f564d24db51d", "Key Ref Index");
			this.keyRefIndexTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 129, true);
			this.keyRefIndexTextBox.Name = "keyRefIndexTextBox";
			this.keyRefIndexTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 17, true);
			this.keyRefIndexTextBox.TabIndex = 5;
			this.keyRefIndexTextBox.Text = "0";
			// 
			// periodTextBox
			// 
			this.periodTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("f53a65da-7ae8-4a26-9904-ddd9a530c64b", "Period (YYYYMM)");
			this.periodTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 47, true);
			this.periodTextBox.Name = "periodTextBox";
			this.periodTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 17, true);
			this.periodTextBox.TabIndex = 2;
			// 
			// updateConsolidationCheckBox
			// 
			this.updateConsolidationCheckBox.AutoSize = true;
			this.updateConsolidationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.updateConsolidationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 160, true);
			this.updateConsolidationCheckBox.Name = "updateConsolidationCheckBox";
			this.updateConsolidationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 17, true);
			this.updateConsolidationCheckBox.TabIndex = 6;
			this.updateConsolidationCheckBox.Text = "Update database consolidation";
			this.updateConsolidationCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.updateConsolidationCheckBox.UseVisualStyleBackColor = true;
			// 
			// UsageUpdateForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 233, true);
			this.Controls.Add(this.updateConsolidationCheckBox);
			this.Controls.Add(this.periodTextBox);
			this.Controls.Add(this.keyRefIndexTextBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.helpLabel);
			this.Controls.Add(this.PriceCodeTextBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.UsageCodeTextBox);
			this.Name = "UsageUpdateForm";
			this.Text = "Update Usage";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.UsageCodeTextBox, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.PriceCodeTextBox, 0);
			this.Controls.SetChildIndex(this.helpLabel, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.keyRefIndexTextBox, 0);
			this.Controls.SetChildIndex(this.periodTextBox, 0);
			this.Controls.SetChildIndex(this.updateConsolidationCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public ZArchitecture.ZTextBox UsageCodeTextBox;
		private ZArchitecture.GUI.ZButton okButton;
		public ZArchitecture.ZTextBox PriceCodeTextBox;
		private ZArchitecture.ZLabel helpLabel;
		private ZArchitecture.GUI.ZButton cancelButton;
		public ZArchitecture.ZTextBox keyRefIndexTextBox;
		public ZArchitecture.ZTextBox periodTextBox;
		private ZArchitecture.GUI.ZCheckBox updateConsolidationCheckBox;
	}
}
