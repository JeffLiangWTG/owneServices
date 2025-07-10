namespace Enterprise.Customs.GB.GUI
{
	partial class CredentialAuthorisationForm
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.checkBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.checkBox2 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.checkBox3 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.checkBox4 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.label1 = new Enterprise.ZArchitecture.ZLabel();
            this.ButtonAuthorise = new Enterprise.ZArchitecture.GUI.ZButton();
            this.label2 = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 426, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 24, true);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("72eea0c6-28aa-438f-acb5-06c3cff7e587", "I understand that I will be taken to the www.gov.uk website, over which WTG have " +
        "no control. ");
            this.checkBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 270, true);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 17, true);
            this.checkBox1.TabIndex = 1;
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.CheckBox_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("5b75b265-5303-4bd1-9cc5-1b731c744c2c", "I already know my Government Gateway credentials, I have the ability to act on be" +
		"half of my company,\r\nand my company is already registered for access to necessary services.");
            this.checkBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 290, true);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 31, true);
            this.checkBox2.TabIndex = 2;
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.CheckBox_CheckedChanged);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("1d4a9140-a222-4233-95f0-770dc63da898", "I acknowledge that I must repeat this process periodically, and that ultimately t" +
        "he ability and responsibility to grant access lies with me.");
            this.checkBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 323, true);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 17, true);
            this.checkBox3.TabIndex = 3;
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.CheckBox_CheckedChanged);
			//
			// checkBox4
			//
			this.checkBox4.AutoSize = true;
            this.checkBox4.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("D923CEFE-091F-4E32-9B6A-10BAE1FAD8A6", "I understand that CW1 will ask for all supported scopes (applications) regardless " +
		"of which applications you have ticked;\r\nthese ticks are used to help CW1 select an existing token for a given application and not used to request scopes for a new token.");
			this.checkBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 343, true);
			this.checkBox4.Name = "checkBox4";
			this.checkBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 29, true);
			this.checkBox4.TabIndex = 4;
			this.checkBox4.UseVisualStyleBackColor = true;
			this.checkBox4.CheckedChanged += new System.EventHandler(this.CheckBox_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 42, true);
            this.label1.Name = "label1";
            this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 13, true);
            this.label1.TabIndex = 5;
            this.label1.Text = "label1";
            // 
            // ButtonAuthorise
            // 
            this.ButtonAuthorise.Enabled = false;
            this.ButtonAuthorise.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(543, 379, true);
            this.ButtonAuthorise.Name = "ButtonAuthorise";
			this.ButtonAuthorise.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
            this.ButtonAuthorise.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 35, true);
            this.ButtonAuthorise.TabIndex = 6;
            this.ButtonAuthorise.ToolTipCaption = null;
            this.ButtonAuthorise.UseVisualStyleBackColor = true;
            this.ButtonAuthorise.Click += new System.EventHandler(this.ButtonAuthorise_Click);
            // 
            // label2
            // 
            this.label2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 13, true);
            this.label2.Name = "label2";
            this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 17, true);
            this.label2.TabIndex = 7;
            this.label2.Text = "label2";
            // 
            // CredentialAuthorisationForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 450, true);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ButtonAuthorise);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Name = "CredentialAuthorisationForm";
            this.Controls.SetChildIndex(this.checkBox1, 0);
            this.Controls.SetChildIndex(this.checkBox2, 0);
            this.Controls.SetChildIndex(this.checkBox3, 0);
            this.Controls.SetChildIndex(this.checkBox4, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.ButtonAuthorise, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZCheckBox checkBox1;
		public Enterprise.ZArchitecture.GUI.ZCheckBox checkBox2;
		public Enterprise.ZArchitecture.GUI.ZCheckBox checkBox3;
		public Enterprise.ZArchitecture.GUI.ZCheckBox checkBox4;
		public Enterprise.ZArchitecture.ZLabel label1;
		public Enterprise.ZArchitecture.GUI.ZButton ButtonAuthorise;
		public Enterprise.ZArchitecture.ZLabel label2;
	}
}
