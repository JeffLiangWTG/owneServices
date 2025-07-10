

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	partial class RecommendEnableAutoJRJInfoControl
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		void InitializeComponent()
		{
			this.LearnMoreButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.IgnoreButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TopLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// LearnMoreButton
			// 
			this.LearnMoreButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("25EF5DB1-8A72-4270-A2E5-4A92C797D732", "Learn more");
			this.LearnMoreButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.LearnMoreButton.IsCaptionOverridden = false;
			this.LearnMoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 119, true);
			this.LearnMoreButton.Name = "LearnMoreButton";
			this.LearnMoreButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.LearnMoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 29, true);
			this.LearnMoreButton.TabIndex = 0;
			this.LearnMoreButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
			this.LearnMoreButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.LearnMoreButton.ToolTipCaption = null;
			this.LearnMoreButton.Click += new System.EventHandler(this.LearnMoreButton_Click);
			// 
			// IgnoreButton
			// 
			this.IgnoreButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("FE105088-FF45-4882-86EA-722AC2A7D8E8", "Ignore");
			this.IgnoreButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.IgnoreButton.IsCaptionOverridden = false;
			this.IgnoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 119, true);
			this.IgnoreButton.Name = "IgnoreButton";
			this.IgnoreButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.IgnoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 29, true);
			this.IgnoreButton.TabIndex = 1;
			this.IgnoreButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
			this.IgnoreButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.IgnoreButton.ToolTipCaption = null;
			// 
			// TopLabel
			// 
			this.TopLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2C909D84-DF60-4F5E-9CFC-E864342BA2C3", "Your billing job contains a cost charge with a margin charge code, which appears to relate to all shipments on this consol.\r\nDid you know that you can apportion the revenue charge as cost on multiple shipments and automatically generate job revenue journals?\r\nTo do this, you need to enable Auto Job Revenue Journal and Gateway Sell Apportionment functionality in the following registry:\r\nAccounting > Job Costing Defaults > Enable Auto Job Revenue Journals.");
			this.TopLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TopLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.TopLabel.Name = "TopLabel";
			this.TopLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 105, true);
			this.TopLabel.TabIndex = 2;
			this.TopLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// RecommendEnableAutoJRJInfoControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("67290FC6-EB44-4D14-BD65-70BD5E24E655", "Confirm Action");
			this.Controls.Add(this.TopLabel);
			this.Controls.Add(this.IgnoreButton);
			this.Controls.Add(this.LearnMoreButton);
			this.Name = "RecommendEnableAutoJRJInfoControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZButton LearnMoreButton;
		ZArchitecture.GUI.ZButton IgnoreButton;
		ZArchitecture.ZLabel TopLabel;
	}
}