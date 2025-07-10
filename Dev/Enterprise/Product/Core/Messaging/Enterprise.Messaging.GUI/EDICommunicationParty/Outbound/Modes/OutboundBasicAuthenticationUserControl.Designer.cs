namespace Enterprise.Messaging.GUI
{
	partial class OutboundBasicAuthenticationUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.UsernameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EDICommunicationParty);
            // 
            // UsernameTextBox
            // 
            this.BindingSource.SetBindingMember(this.UsernameTextBox, "OutboundConfig.Auth.ECA_Username");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).OutboundConfig.Auth.ECA_Username)));
            this.UsernameTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("e905918e-5e1b-46fe-9321-a301c142f567", "Username");
            this.UsernameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.UsernameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 3, true);
            this.UsernameTextBox.Name = "UsernameTextBox";
            this.UsernameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.UsernameTextBox.TabIndex = 3;
            // 
            // PasswordTextBox
            // 
            this.BindingSource.SetBindingMember(this.PasswordTextBox, "OutboundConfig.Auth.ECA_Password");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).OutboundConfig.Auth.ECA_Password)));
            this.PasswordTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("ebd329c3-0c18-431b-adb7-7fc67bb0241c", "Password");
            this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 27, true);
            this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.PasswordTextBox.TabIndex = 4;
            // 
            // OutboundBasicAuthenticationUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.PasswordTextBox);
            this.Controls.Add(this.UsernameTextBox);
            this.Name = "OutboundBasicAuthenticationUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 286, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox UsernameTextBox;
		private ZArchitecture.ZTextBox PasswordTextBox;
	}
}
