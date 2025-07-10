namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class PriceItemBulkCopyForm
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
		protected override void InitializeComponent()
		{
			this.PriceCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelFormButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PriceDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InfoLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 185, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 24, true);
			// 
			// PriceCodeTextBox
			// 
			this.PriceCodeTextBox.AllowDrop = true;
			this.PriceCodeTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("d46962bc-b9e2-4efc-bc8f-333ffac32a39", "New Price Code:");
			this.PriceCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 70, true);
			this.PriceCodeTextBox.Name = "PriceCodeTextBox";
			this.PriceCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 17, true);
			this.PriceCodeTextBox.TabIndex = 1;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ffd540c9-39b8-4790-93d5-a250fb13fb82", "OK");
			this.OKButton.IsCaptionOverridden = false;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 148, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 3;
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelFormButton
			// 
			this.CancelFormButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("1e3ee077-8877-4f2f-9673-1b76d6a29156", "Cancel");
			this.CancelFormButton.IsCaptionOverridden = false;
			this.CancelFormButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 148, true);
			this.CancelFormButton.Name = "CancelFormButton";
			this.CancelFormButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelFormButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelFormButton.TabIndex = 4;
			this.CancelFormButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelFormButton.ToolTipCaption = null;
			this.CancelFormButton.UseVisualStyleBackColor = true;
			this.CancelFormButton.Click += new System.EventHandler(this.CancelFormButton_Click);
			// 
			// PriceDescTextBox
			// 
			this.PriceDescTextBox.AllowDrop = true;
			this.PriceDescTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("82ca9f89-689e-4412-bc6a-a75c2d6d25ec", "New Description:");
			this.PriceDescTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PriceDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 102, true);
			this.PriceDescTextBox.Name = "PriceDescTextBox";
			this.PriceDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 17, true);
			this.PriceDescTextBox.TabIndex = 2;
			// 
			// InfoLabel
			// 
			this.InfoLabel.AllowDrop = true;
			this.InfoLabel.AutoSize = true;
			this.InfoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.InfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 14, true);
			this.InfoLabel.Name = "InfoLabel";
			this.InfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 40, true);
			this.InfoLabel.TabIndex = 0;
			this.InfoLabel.Text = "Create new records by copying selected records.\r\nPrices in other currencies are a" +
    "lso copied.\r\nNew record will have the order bumped by one.\r\n";
			// 
			// PriceItemBulkCopyForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("22bdfc58-403c-4417-bbe9-50db8b66bb36", "Bulk Copy");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 209, true);
			this.Controls.Add(this.CancelFormButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.PriceDescTextBox);
			this.Controls.Add(this.InfoLabel);
			this.Controls.Add(this.PriceCodeTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 247, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 247, true);
			this.Name = "PriceItemBulkCopyForm";
			this.Controls.SetChildIndex(this.PriceCodeTextBox, 0);
			this.Controls.SetChildIndex(this.InfoLabel, 0);
			this.Controls.SetChildIndex(this.PriceDescTextBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelFormButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		
		private Enterprise.ZArchitecture.ZTextBox PriceCodeTextBox;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZButton CancelFormButton;
		private ZArchitecture.ZTextBox PriceDescTextBox;
		private ZArchitecture.ZLabel InfoLabel;
	}
}