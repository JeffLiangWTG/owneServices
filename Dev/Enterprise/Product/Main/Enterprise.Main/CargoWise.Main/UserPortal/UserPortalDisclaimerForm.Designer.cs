namespace Enterprise.UserPortal
{
	partial class UserPortalDisclaimerForm
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
			this.MainTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.linkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.btnOK = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 231, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 10, true);
			this.MainStatusBar.TabIndex = 4;
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.UserPortal.UserPortalDisclaimerBizO);
			// 
			// MainTextLabel
			// 
			this.MainTextLabel.CaptionResourceString = CargoWise.Main.Res.GetData("1cf54947-5137-42aa-a325-2465046397ea", "You are about to send your staff profile information over the Internet. The information is used to log you into myaccount.cargowise.com. It is strongly encrypted and secured and will at all times be used in compliance with the relevant data privacy laws (including the Australian Privacy Act 1988 (Cth) and the General Data Protection Regulation (Regulation (EU) 2016/679)). \r\n\r\nBy pressing the \'OK\' button, you acknowledge this data collection notice and are in agreement with the Data Collection Privacy Policy found on our website. To learn more about our data collection policy please click here.");
			this.MainTextLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MainTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 16, true);
			this.MainTextLabel.Name = "MainTextLabel";
			this.MainTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 111, true);
			this.MainTextLabel.TabIndex = 0;
			// 
			// linkLabel
			// 
			this.linkLabel.CaptionResourceString = CargoWise.Main.Res.GetData("d7f64a43-2ce9-44b4-a33b-ed87c26530e3", "https://www.wisetechglobal.com/privacy-policy");
			this.linkLabel.IsFontBold = false;
			this.linkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 135, true);
			this.linkLabel.Name = "linkLabel";
			this.linkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 20, true);
			this.linkLabel.TabIndex = 0;
			this.linkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
			// 
			// btnOK
			// 
			this.btnOK.CaptionResourceString = CargoWise.Main.Res.GetData("UserPortalDisclaimerForm|108e9e3d-f04f-4340-8ed4-1b0f3fb22959", "OK");
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 201, true);
			this.btnOK.Name = "btnOK";
			this.btnOK.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.btnOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.btnOK.TabIndex = 2;
			this.btnOK.ToolTipCaption = null;
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.CaptionResourceString = CargoWise.Main.Res.GetData("UserPortalDisclaimerForm|fc0b37c7-bfda-4001-8e6b-b6ab998097f5", "Cancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 201, true);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.btnCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.ToolTipCaption = null;
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox1, "DoNotShowAgainNextTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.UserPortal.UserPortalDisclaimerBizO)(null)).DoNotShowAgainNextTime)));
			this.zCheckBox1.CaptionResourceString = CargoWise.Main.Res.GetData("UserPortalDisclaimerForm|85c9a6c2-e5bb-4a50-adde-967c7656732d", "Do not show this dialog again");
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 173, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 16, true);
			this.zCheckBox1.TabIndex = 1;
			// 
			// UserPortalDisclaimerForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWise.Main.Res.GetData("UserPortalDisclaimerForm|4ae18a44-3eae-41fa-a17c-76ef4ddb8144", "Disclaimer");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 241, true);
			this.Controls.Add(this.zCheckBox1);
			this.Controls.Add(this.linkLabel);
			this.Controls.Add(this.MainTextLabel);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.DataSourceType = typeof(Enterprise.UserPortal.UserPortalDisclaimerBizO);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "UserPortalDisclaimerForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.btnOK, 0);
			this.Controls.SetChildIndex(this.btnCancel, 0);
			this.Controls.SetChildIndex(this.MainTextLabel, 0);
			this.Controls.SetChildIndex(this.linkLabel, 0);
			this.Controls.SetChildIndex(this.zCheckBox1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel MainTextLabel;
		private Enterprise.ZArchitecture.GUI.ZLinkLabel linkLabel;
		private Enterprise.ZArchitecture.GUI.ZButton btnOK;
		private Enterprise.ZArchitecture.GUI.ZButton btnCancel;
		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
	}
}
