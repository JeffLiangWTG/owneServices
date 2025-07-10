namespace Enterprise.Accounting.GUI
{
	partial class LoginFormWithTwoCredentialSupportBranchDepartmentLevel
	{
		#region Component Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.PasswordTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.LoginTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.oGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// oGroupBox1
			// 
			this.oGroupBox1.Text = "Authorising User 1";
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 329, true);
			// 
			// CancelXButton
			// 
			this.CancelXButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(459, 329, true);
			// 
			// PasswordTextBox2
			// 
			this.PasswordTextBox2.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.PasswordTextBox2.CaptionResourceString = null;
			this.PasswordTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 284, true);
			this.PasswordTextBox2.Name = "PasswordTextBox2";
			this.PasswordTextBox2.PasswordChar = '*';
			this.PasswordTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 17, true);
			this.PasswordTextBox2.TabIndex = 12;
			// 
			// LoginTextBox2
			// 
			this.LoginTextBox2.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.LoginTextBox2.CaptionResourceString = null;
			this.LoginTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LoginTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 259, true);
			this.LoginTextBox2.Name = "LoginTextBox2";
			this.LoginTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 17, true);
			this.LoginTextBox2.TabIndex = 11;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.zGroupBox1.Controls.Add(this.zLabel1);
			this.zGroupBox1.Controls.Add(this.zLabel2);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 231, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 87, true);
			this.zGroupBox1.TabIndex = 13;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Authorizing User 2";
			// 
			// zLabel1
			// 
			this.zLabel1.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 54, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.zLabel1.TabIndex = 1;
			this.zLabel1.Text = "Password:";
			// 
			// zLabel2
			// 
			this.zLabel2.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 30, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			this.zLabel2.TabIndex = 0;
			this.zLabel2.Text = "Username:";
			// 
			// LoginFormWithTwoCredentialSupportBranchDepartmentLevel
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 361, true);
			this.Controls.Add(this.PasswordTextBox2);
			this.Controls.Add(this.LoginTextBox2);
			this.Controls.Add(this.zGroupBox1);
			this.Name = "LoginFormWithTwoCredentialSupportBranchDepartmentLevel";
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.oGroupBox1, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelXButton, 0);
			this.Controls.SetChildIndex(this.LoginTextBox2, 0);
			this.Controls.SetChildIndex(this.PasswordTextBox2, 0);
			this.oGroupBox1.ResumeLayout(false);
			this.oGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal Enterprise.ZArchitecture.ZTextBox PasswordTextBox2;
		internal Enterprise.ZArchitecture.ZTextBox LoginTextBox2;
		Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		Enterprise.ZArchitecture.ZLabel zLabel2;
	}
}
