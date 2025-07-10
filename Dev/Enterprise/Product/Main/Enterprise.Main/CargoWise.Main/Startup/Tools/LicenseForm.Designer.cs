using System.ComponentModel;
using CargoWiseOne.ResourceStrings;

namespace CargoWise.Main.Startup.Tools
{
	partial class LicenseForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

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
			this.txtLicense = new Enterprise.ZArchitecture.ZTextBox();
			this.btnAccept = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnDecline = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.chkHasReadLicense = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 513, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 24, true);
			// 
			// txtLicense
			// 
			this.txtLicense.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtLicense.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.txtLicense.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 48, true);
			this.txtLicense.Multiline = true;
			this.txtLicense.Name = "txtLicense";
			this.txtLicense.ReadOnly = true;
			this.txtLicense.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtLicense.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 417, true);
			this.txtLicense.TabIndex = 2;
			// 
			// btnAccept
			// 
			this.btnAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAccept.CaptionResourceString = CargoWise.Main.Res.GetData("5d831f8f-ea0a-47ec-bbc4-11b66ef49b1a", "Accept");
			this.btnAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnAccept.Enabled = false;
			this.btnAccept.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 477, true);
			this.btnAccept.Name = "btnAccept";
			this.btnAccept.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.btnAccept.TabIndex = 5;
			this.btnAccept.ToolTipCaption = null;
			this.btnAccept.UseVisualStyleBackColor = true;
			// 
			// btnDecline
			// 
			this.btnDecline.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDecline.CaptionResourceString = CargoWise.Main.Res.GetData("056f16b6-754c-4d20-961d-9f40a8cadc1a", "Decline");
			this.btnDecline.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnDecline.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 477, true);
			this.btnDecline.Name = "btnDecline";
			this.btnDecline.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.btnDecline.TabIndex = 4;
			this.btnDecline.ToolTipCaption = null;
			this.btnDecline.UseVisualStyleBackColor = true;
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel1.CaptionResourceString = CargoWise.Main.Res.GetData("0cccf700-7bb2-4e01-8779-3893d042288d", "This feature requires a 3rd party tool to be downloaded and installed onto the computer running CargoWise. To proceed, you will need to accept the 3rd party tool license agreement displayed below.");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 9, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 27, true);
			this.zLabel1.TabIndex = 1;
			// 
			// chkHasReadLicense
			// 
			this.chkHasReadLicense.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.chkHasReadLicense.BackColor = System.Drawing.SystemColors.Control;
			this.chkHasReadLicense.CaptionResourceString = CargoWise.Main.Res.GetData("79e5d409-8688-4540-85f2-fda4d88984d9", "I read the license agreement");
			this.chkHasReadLicense.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 477, true);
			this.chkHasReadLicense.Name = "chkHasReadLicense";
			this.chkHasReadLicense.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 24, true);
			this.chkHasReadLicense.TabIndex = 3;
			this.chkHasReadLicense.UseVisualStyleBackColor = false;
			this.chkHasReadLicense.CheckedChanged += new System.EventHandler(this.HasReadLicenseChanged);
			// 
			// LicenseForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWise.Main.Res.GetData("01218506-d4c2-4be6-a724-a3025bc88f0a", "License Agreement");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 537, true);
			this.Controls.Add(this.chkHasReadLicense);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.btnDecline);
			this.Controls.Add(this.btnAccept);
			this.Controls.Add(this.txtLicense);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 572, true);
			this.Name = "LicenseForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.txtLicense, 0);
			this.Controls.SetChildIndex(this.btnAccept, 0);
			this.Controls.SetChildIndex(this.btnDecline, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.chkHasReadLicense, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox txtLicense;
		private Enterprise.ZArchitecture.GUI.ZButton btnAccept;
		private Enterprise.ZArchitecture.GUI.ZButton btnDecline;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.GUI.ZCheckBox chkHasReadLicense;
	}
}
