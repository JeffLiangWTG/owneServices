namespace Enterprise.ServiceManager.GUI
{
	partial class ServiceTaskProxyConfigurationControl
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
		void InitializeComponent()
		{
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.useSemicolonsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.doNotUseAddressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.bypassAddressesBox = new Enterprise.ZArchitecture.ZTextBox();
			this.bypassLocalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.portCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.passwordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.usernameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.automaticDetectionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.enableAuthenticationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.hostTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ServiceManager.Business.StmServiceHost);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceTaskProxyConfigurationControl|71d55ed9-b5cb-45e6-9fe5-c3d30bb646dd", "Proxy Configuration");
			this.zGroupBox1.Controls.Add(this.useSemicolonsLabel);
			this.zGroupBox1.Controls.Add(this.doNotUseAddressLabel);
			this.zGroupBox1.Controls.Add(this.bypassAddressesBox);
			this.zGroupBox1.Controls.Add(this.bypassLocalCheckBox);
			this.zGroupBox1.Controls.Add(this.portCalcEdit);
			this.zGroupBox1.Controls.Add(this.passwordTextBox);
			this.zGroupBox1.Controls.Add(this.usernameTextBox);
			this.zGroupBox1.Controls.Add(this.automaticDetectionCheckBox);
			this.zGroupBox1.Controls.Add(this.enableAuthenticationCheckBox);
			this.zGroupBox1.Controls.Add(this.hostTextBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 280, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// useSemicolonsLabel
			// 
			this.useSemicolonsLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("cdf215db-5883-4af0-ab83-1017671fae0c", "Use semicolons to separate entries.");
			this.useSemicolonsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 245, true);
			this.useSemicolonsLabel.Name = "useSemicolonsLabel";
			this.useSemicolonsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 20, true);
			this.useSemicolonsLabel.TabIndex = 15;
			// 
			// doNotUseAddressLabel
			// 
			this.doNotUseAddressLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("dac513d1-20f5-47de-ab98-cddae0881e66", "Do not use proxy server for addresses beginning with:");
			this.doNotUseAddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 165, true);
			this.doNotUseAddressLabel.Name = "doNotUseAddressLabel";
			this.doNotUseAddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 20, true);
			this.doNotUseAddressLabel.TabIndex = 14;
			// 
			// bypassAddressesBox
			// 
			this.BindingSource.SetBindingMember(this.bypassAddressesBox, "ProxyBypassList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceHost)(null)).ProxyBypassList)));
			this.bypassAddressesBox.CaptionResourceString = null;
			this.bypassAddressesBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.bypassAddressesBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 188, true);
			this.bypassAddressesBox.Multiline = true;
			this.bypassAddressesBox.Name = "bypassAddressesBox";
			this.bypassAddressesBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.bypassAddressesBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 54, true);
			this.bypassAddressesBox.TabIndex = 13;
			// 
			// bypassLocalCheckBox
			// 
			this.bypassLocalCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.bypassLocalCheckBox, "ProxyBypassOnLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceHost)(null)).ProxyBypassOnLocal)));
			this.bypassLocalCheckBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("a79a40ab-e417-4893-a6e6-16dd7dc4a74b", "Bypass proxy server for local addresses");
			this.bypassLocalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.bypassLocalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 141, true);
			this.bypassLocalCheckBox.Name = "bypassLocalCheckBox";
			this.bypassLocalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 17, true);
			this.bypassLocalCheckBox.TabIndex = 12;
			this.bypassLocalCheckBox.UseVisualStyleBackColor = true;
			// 
			// portCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.portCalcEdit, "SH_ProxyPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ServiceManager.Business.StmServiceHost)(null)).SH_ProxyPort)));
			this.portCalcEdit.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceTaskProxyConfigurationControl|c8a73cf9-6515-454e-831a-c1c5938ce931", "Port");
			this.portCalcEdit.DecimalPlaces = 0;
			this.portCalcEdit.Decimals = 0;
			this.portCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 49, true);
			this.portCalcEdit.Name = "portCalcEdit";
			this.portCalcEdit.ShowGroupSeparators = false;
			this.portCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.portCalcEdit.TabIndex = 8;
			this.portCalcEdit.Text = "0";
			this.portCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// passwordTextBox
			// 
			this.BindingSource.SetBindingMember(this.passwordTextBox, "SH_ProxyPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceHost)(null)).SH_ProxyPassword)));
			this.passwordTextBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceTaskProxyConfigurationControl|6c704ae2-21dc-4d69-b31c-9f42dc9099fe", "Password");
			this.passwordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.passwordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 106, true);
			this.passwordTextBox.Name = "passwordTextBox";
			this.passwordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.passwordTextBox.TabIndex = 11;
			this.passwordTextBox.UseSystemPasswordChar = true;
			// 
			// usernameTextBox
			// 
			this.BindingSource.SetBindingMember(this.usernameTextBox, "SH_ProxyUserName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceHost)(null)).SH_ProxyUserName)));
			this.usernameTextBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceTaskProxyConfigurationControl|e5e7dfd0-f126-49c2-90a6-8476de0ad1ad", "Username");
			this.usernameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.usernameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 106, true);
			this.usernameTextBox.Name = "usernameTextBox";
			this.usernameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.usernameTextBox.TabIndex = 10;
			// 
			// automaticDetectionCheckBox
			// 
			this.automaticDetectionCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.automaticDetectionCheckBox, "SH_ProxyAutoDetect");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceHost)(null)).SH_ProxyAutoDetect)));
			this.automaticDetectionCheckBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceTaskProxyConfigurationControl|1112faa5-2de6-4a9d-ae98-f51c3988bd59", "Use windows account Internet properties");
			this.automaticDetectionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.automaticDetectionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.automaticDetectionCheckBox.Name = "automaticDetectionCheckBox";
			this.automaticDetectionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 17, true);
			this.automaticDetectionCheckBox.TabIndex = 6;
			this.automaticDetectionCheckBox.UseVisualStyleBackColor = true;
			// 
			// enableAuthenticationCheckBox
			// 
			this.enableAuthenticationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.enableAuthenticationCheckBox, "SH_ProxyAuthentication");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceHost)(null)).SH_ProxyAuthentication)));
			this.enableAuthenticationCheckBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceTaskProxyConfigurationControl|aa39b039-810e-4867-a0a2-c18ff7965730", "Enable authentication");
			this.enableAuthenticationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.enableAuthenticationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 75, true);
			this.enableAuthenticationCheckBox.Name = "enableAuthenticationCheckBox";
			this.enableAuthenticationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 17, true);
			this.enableAuthenticationCheckBox.TabIndex = 9;
			this.enableAuthenticationCheckBox.UseVisualStyleBackColor = true;
			// 
			// hostTextBox
			// 
			this.BindingSource.SetBindingMember(this.hostTextBox, "SH_ProxyHost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceHost)(null)).SH_ProxyHost)));
			this.hostTextBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceTaskProxyConfigurationControl|7098146b-cfda-47aa-a11c-4f197d095440", "Host");
			this.hostTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.hostTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 49, true);
			this.hostTextBox.Name = "hostTextBox";
			this.hostTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.hostTextBox.TabIndex = 7;
			// 
			// ServiceTaskProxyConfigurationControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Name = "ServiceTaskProxyConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 286, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.ZCalcEdit portCalcEdit;
		private ZArchitecture.ZTextBox passwordTextBox;
		private ZArchitecture.ZTextBox usernameTextBox;
		private ZArchitecture.GUI.ZCheckBox automaticDetectionCheckBox;
		private ZArchitecture.GUI.ZCheckBox enableAuthenticationCheckBox;
		private ZArchitecture.ZTextBox hostTextBox;
		private ZArchitecture.ZLabel doNotUseAddressLabel;
		private ZArchitecture.ZTextBox bypassAddressesBox;
		private ZArchitecture.GUI.ZCheckBox bypassLocalCheckBox;
		private ZArchitecture.ZLabel useSemicolonsLabel;

	}
}
