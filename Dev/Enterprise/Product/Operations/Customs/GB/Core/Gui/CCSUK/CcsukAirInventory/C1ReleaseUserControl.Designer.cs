namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class C1ReleaseUserControl
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
            this.releasedUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.AlreadyReleasedGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.releasedUpDown)).BeginInit();
            this.releasedUpDown.SuspendLayout();
            this.SuspendLayout();
            // 
            // ReleaseNowButton
            // 
            this.ReleaseNowButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("bfa508e2-74ba-4f24-a406-bdd33b004f9d", "Release && Print C1");
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
            this.AlreadyReleasedGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AlreadyReleasedGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("C1ReleaseUserControl|PiecesAlreadyReleasedGroupBox", "Pieces already released...");
            this.AlreadyReleasedGroupBox.Controls.Add(this.LogLabel);
            this.AlreadyReleasedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 28, true);
            this.AlreadyReleasedGroupBox.Name = "AlreadyReleasedGroupBox";
            this.AlreadyReleasedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 100, true);
            this.AlreadyReleasedGroupBox.TabIndex = 2;
            this.AlreadyReleasedGroupBox.TabStop = false;
            // 
            // LogLabel
            // 
            this.LogLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.LogLabel, "LogOfReleases");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.LogLabel.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("C1ReleaseUserControl|LogLabel", ".");
            this.LogLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 16, true);
            this.LogLabel.Name = "LogLabel";
            this.LogLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 81, true);
            this.LogLabel.TabIndex = 0;
            this.LogLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.LogLabel.UseMnemonic = false;
            // 
            // releasedUpDown
            // 
            this.BindingSource.SetBindingMember(this.releasedUpDown, "NumberOfPieces");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            this.releasedUpDown.BindTo = "NumberOfPieces";
            this.releasedUpDown.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("C1ReleaseUserControl|AlreadyReleasedGroupBox", "Pieces");
            this.releasedUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 5, true);
            this.releasedUpDown.Name = "releasedUpDown";
            this.releasedUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
            this.releasedUpDown.TabIndex = 0;
            // 
            // C1ReleaseUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.releasedUpDown);
            this.Controls.Add(this.AlreadyReleasedGroupBox);
            this.Controls.Add(this.ReleaseNowButton);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 131, true);
            this.Name = "C1ReleaseUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 131, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.AlreadyReleasedGroupBox.ResumeLayout(false);
            this.AlreadyReleasedGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.releasedUpDown)).EndInit();
            this.releasedUpDown.ResumeLayout(false);
            this.releasedUpDown.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton ReleaseNowButton;
		private ZArchitecture.GUI.ZGroupBox AlreadyReleasedGroupBox;
		private ZArchitecture.ZLabel LogLabel;
		private ZArchitecture.GUI.ZNumericUpDown releasedUpDown;

	}
}
