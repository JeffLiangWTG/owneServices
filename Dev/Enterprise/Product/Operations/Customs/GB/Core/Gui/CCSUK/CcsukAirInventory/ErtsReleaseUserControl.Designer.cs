using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class ErtsReleaseUserControl
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
		void InitializeComponent()
		{
            this.ReleaseNowButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.AlreadyReleasedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.LogLabel = new Enterprise.ZArchitecture.ZLabel();
            this.releasedCountTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.partialReleaseHelpLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            this.Status2Box = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.AlreadyReleasedGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentC1Release);
			// 
			// ReleaseNowButton
			// 
			this.ReleaseNowButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("5cdddde7-974d-4e3d-8703-1fb2ebccb9d6", "Print RRA");
            this.ReleaseNowButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 6, true);
            this.ReleaseNowButton.Name = "ReleaseNowButton";
            this.ReleaseNowButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
            this.ReleaseNowButton.TabIndex = 1;
            this.ReleaseNowButton.ToolTipCaption = null;
            this.ReleaseNowButton.UseVisualStyleBackColor = true;
            this.ReleaseNowButton.Click += new System.EventHandler(this.ReleaseNowButton_Click);
            // 
            // AlreadyReleasedGroupBox
            // 
            this.AlreadyReleasedGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AlreadyReleasedGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("ErtsReleaseUserControl|AlreadyReleasedGroupBox", "Pieces already released...");
            this.AlreadyReleasedGroupBox.Controls.Add(this.LogLabel);
            this.AlreadyReleasedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 48, true);
            this.AlreadyReleasedGroupBox.Name = "AlreadyReleasedGroupBox";
            this.AlreadyReleasedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 80, true);
            this.AlreadyReleasedGroupBox.TabIndex = 3;
            this.AlreadyReleasedGroupBox.TabStop = false;
            // 
            // LogLabel
            // 
            this.LogLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.LogLabel, "LogOfReleases");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentC1Release)(null)).LogOfReleases)));
            this.LogLabel.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("C1ReleaseUserControl|LogLabel", ".");
            this.LogLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.LogLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 16, true);
            this.LogLabel.Name = "LogLabel";
            this.LogLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 61, true);
            this.LogLabel.TabIndex = 4;
            this.LogLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.LogLabel.UseMnemonic = false;
            // 
            // releasedCountTextBox
            // 
            this.BindingSource.SetBindingMember(this.releasedCountTextBox, "NumberOfPieces");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentC1Release)(null)).NumberOfPieces)));
            this.releasedCountTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("C1ReleaseUserControl|AlreadyReleasedGroupBox", "Pieces");
            this.releasedCountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 6, true);
            this.releasedCountTextBox.Name = "releasedCountTextBox";
            this.releasedCountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
            this.releasedCountTextBox.TabIndex = 0;
            // 
            // partialReleaseHelpLink
            // 
            this.partialReleaseHelpLink.AutoSize = true;
			this.partialReleaseHelpLink.IsFontBold = false;
            this.partialReleaseHelpLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 32, true);
            this.partialReleaseHelpLink.Name = "partialReleaseHelpLink";
            this.partialReleaseHelpLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 13, true);
			this.partialReleaseHelpLink.TabIndex = 2;
            this.partialReleaseHelpLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.partialReleaseHelpLink_Click);
            // 
            // Status2Box
            // 
            this.BindingSource.SetBindingMember(this.Status2Box, "Status2Granted");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentC1Release)(null)).Status2Granted)));
            this.Status2Box.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("1f0f7bc9-184f-48cb-9fa2-669d00e82b15", "S2", "Status 2", "Status 2 (commercial/fiscal standing) granted or revoked");
            this.Status2Box.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.Status2Box, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
            this.Status2Box.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 6, true);
            this.Status2Box.Name = "Status2Box";
            this.Status2Box.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
            this.Status2Box.TabIndex = 3;
            // 
            // ErtsReleaseUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.partialReleaseHelpLink);
            this.Controls.Add(this.releasedCountTextBox);
            this.Controls.Add(this.Status2Box);
            this.Controls.Add(this.AlreadyReleasedGroupBox);
            this.Controls.Add(this.ReleaseNowButton);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 131, true);
            this.Name = "ErtsReleaseUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 131, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.AlreadyReleasedGroupBox.ResumeLayout(false);
            this.AlreadyReleasedGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton ReleaseNowButton;
		private ZArchitecture.GUI.ZGroupBox AlreadyReleasedGroupBox;
		private ZArchitecture.ZLabel LogLabel;
		private ZArchitecture.ZTextBox  releasedCountTextBox;
		private ZLinkLabel partialReleaseHelpLink;
		private ZArchitecture.ZTextBox Status2Box;

	}
}
