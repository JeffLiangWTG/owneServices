namespace Enterprise.Security.ActiveDirectory.GUI
{
	partial class ADPasswordSettingsForm
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
		/// 
		new void InitializeComponent()
		{
			this.maximumPasswordAge = new Enterprise.ZArchitecture.ZCalcEdit();
			this.enforceMaximumPasswordAge = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.enforceMinimumPasswordLength = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.minimumPasswordLength = new Enterprise.ZArchitecture.ZCalcEdit();
			this.passwordHistoryLength = new Enterprise.ZArchitecture.ZCalcEdit();
			this.enforcePasswordHistoryLength = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.enablePasswordComplexity = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.enforceMinimumPasswordAge = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.minimumPasswordAge = new Enterprise.ZArchitecture.ZCalcEdit();
			this.lockoutThreshold = new Enterprise.ZArchitecture.ZCalcEdit();
			this.enforcePasswordLockoutPolicy = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.lockoutObservationWindow = new Enterprise.ZArchitecture.ZCalcEdit();
			this.lockoutDuration = new Enterprise.ZArchitecture.ZCalcEdit();
			this.requireAdminUnlock = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.saveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.passwordSettingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.overrideDomainPasswordPolicy = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.adPSOName = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.passwordSettingsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 490, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Security.ActiveDirectory.ADPasswordSettings);
			// 
			// maximumPasswordAge
			// 
			this.BindingSource.SetBindingMember(this.maximumPasswordAge, "MaximumPasswordAge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).MaximumPasswordAge)));
			this.maximumPasswordAge.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.maximumPasswordAge.DecimalPlaces = 0;
			this.maximumPasswordAge.Decimals = 0;
			this.maximumPasswordAge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 215, true);
			this.maximumPasswordAge.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 3, true);
			this.maximumPasswordAge.Name = "maximumPasswordAge";
			this.maximumPasswordAge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.maximumPasswordAge.TabIndex = 9;
			this.maximumPasswordAge.Text = "0";
			this.maximumPasswordAge.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// enforceMaximumPasswordAge
			// 
			this.enforceMaximumPasswordAge.AutoSize = true;
			this.BindingSource.SetBindingMember(this.enforceMaximumPasswordAge, "EnforceMaximumPasswordAge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).EnforceMaximumPasswordAge)));
			this.enforceMaximumPasswordAge.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.enforceMaximumPasswordAge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 198, true);
			this.enforceMaximumPasswordAge.Name = "enforceMaximumPasswordAge";
			this.enforceMaximumPasswordAge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.enforceMaximumPasswordAge.TabIndex = 8;
			// 
			// enforceMinimumPasswordLength
			// 
			this.enforceMinimumPasswordLength.AutoSize = true;
			this.BindingSource.SetBindingMember(this.enforceMinimumPasswordLength, "EnforceMinimumPasswordLength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).EnforceMinimumPasswordLength)));
			this.enforceMinimumPasswordLength.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.enforceMinimumPasswordLength.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 19, true);
			this.enforceMinimumPasswordLength.Name = "enforceMinimumPasswordLength";
			this.enforceMinimumPasswordLength.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.enforceMinimumPasswordLength.TabIndex = 1;
			// 
			// minimumPasswordLength
			// 
			this.BindingSource.SetBindingMember(this.minimumPasswordLength, "MinimumPasswordLength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).MinimumPasswordLength)));
			this.minimumPasswordLength.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.minimumPasswordLength.DecimalPlaces = 0;
			this.minimumPasswordLength.Decimals = 0;
			this.minimumPasswordLength.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 36, true);
			this.minimumPasswordLength.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 3, true);
			this.minimumPasswordLength.Name = "minimumPasswordLength";
			this.minimumPasswordLength.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.minimumPasswordLength.TabIndex = 2;
			this.minimumPasswordLength.Text = "0";
			this.minimumPasswordLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// passwordHistoryLength
			// 
			this.BindingSource.SetBindingMember(this.passwordHistoryLength, "PasswordHistoryLength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).PasswordHistoryLength)));
			this.passwordHistoryLength.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.passwordHistoryLength.DecimalPlaces = 0;
			this.passwordHistoryLength.Decimals = 0;
			this.passwordHistoryLength.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 88, true);
			this.passwordHistoryLength.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 3, true);
			this.passwordHistoryLength.Name = "passwordHistoryLength";
			this.passwordHistoryLength.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.passwordHistoryLength.TabIndex = 4;
			this.passwordHistoryLength.Text = "0";
			this.passwordHistoryLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// enforcePasswordHistoryLength
			// 
			this.enforcePasswordHistoryLength.AutoSize = true;
			this.BindingSource.SetBindingMember(this.enforcePasswordHistoryLength, "EnforcePasswordHistoryLength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).EnforcePasswordHistoryLength)));
			this.enforcePasswordHistoryLength.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.enforcePasswordHistoryLength.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 63, true);
			this.enforcePasswordHistoryLength.Name = "enforcePasswordHistoryLength";
			this.enforcePasswordHistoryLength.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.enforcePasswordHistoryLength.TabIndex = 3;
			// 
			// enablePasswordComplexity
			// 
			this.enablePasswordComplexity.AutoSize = true;
			this.BindingSource.SetBindingMember(this.enablePasswordComplexity, "PasswordComplexityEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).PasswordComplexityEnabled)));
			this.enablePasswordComplexity.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.enablePasswordComplexity.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 113, true);
			this.enablePasswordComplexity.Name = "enablePasswordComplexity";
			this.enablePasswordComplexity.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.enablePasswordComplexity.TabIndex = 5;
			// 
			// enforceMinimumPasswordAge
			// 
			this.enforceMinimumPasswordAge.AutoSize = true;
			this.BindingSource.SetBindingMember(this.enforceMinimumPasswordAge, "EnforceMinimumPasswordAge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).EnforceMinimumPasswordAge)));
			this.enforceMinimumPasswordAge.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.enforceMinimumPasswordAge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 146, true);
			this.enforceMinimumPasswordAge.Name = "enforceMinimumPasswordAge";
			this.enforceMinimumPasswordAge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.enforceMinimumPasswordAge.TabIndex = 6;
			// 
			// minimumPasswordAge
			// 
			this.BindingSource.SetBindingMember(this.minimumPasswordAge, "MinimumPasswordAge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).MinimumPasswordAge)));
			this.minimumPasswordAge.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.minimumPasswordAge.DecimalPlaces = 0;
			this.minimumPasswordAge.Decimals = 0;
			this.minimumPasswordAge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 163, true);
			this.minimumPasswordAge.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 3, true);
			this.minimumPasswordAge.Name = "minimumPasswordAge";
			this.minimumPasswordAge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.minimumPasswordAge.TabIndex = 7;
			this.minimumPasswordAge.Text = "0";
			this.minimumPasswordAge.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// lockoutThreshold
			// 
			this.BindingSource.SetBindingMember(this.lockoutThreshold, "LockoutThreshold");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).LockoutThreshold)));
			this.lockoutThreshold.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.lockoutThreshold.DecimalPlaces = 0;
			this.lockoutThreshold.Decimals = 0;
			this.lockoutThreshold.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 262, true);
			this.lockoutThreshold.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 3, true);
			this.lockoutThreshold.Name = "lockoutThreshold";
			this.lockoutThreshold.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.lockoutThreshold.TabIndex = 11;
			this.lockoutThreshold.Text = "0";
			this.lockoutThreshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// enforcePasswordLockoutPolicy
			// 
			this.enforcePasswordLockoutPolicy.AutoSize = true;
			this.BindingSource.SetBindingMember(this.enforcePasswordLockoutPolicy, "EnforcePasswordLockoutPolicy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).EnforcePasswordLockoutPolicy)));
			this.enforcePasswordLockoutPolicy.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.enforcePasswordLockoutPolicy.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 245, true);
			this.enforcePasswordLockoutPolicy.Name = "enforcePasswordLockoutPolicy";
			this.enforcePasswordLockoutPolicy.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.enforcePasswordLockoutPolicy.TabIndex = 10;
			// 
			// lockoutObservationWindow
			// 
			this.BindingSource.SetBindingMember(this.lockoutObservationWindow, "LockoutObservationWindow");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).LockoutObservationWindow)));
			this.lockoutObservationWindow.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.lockoutObservationWindow.DecimalPlaces = 0;
			this.lockoutObservationWindow.Decimals = 0;
			this.lockoutObservationWindow.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 290, true);
			this.lockoutObservationWindow.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 3, true);
			this.lockoutObservationWindow.Name = "lockoutObservationWindow";
			this.lockoutObservationWindow.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.lockoutObservationWindow.TabIndex = 12;
			this.lockoutObservationWindow.Text = "0";
			this.lockoutObservationWindow.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// lockoutDuration
			// 
			this.BindingSource.SetBindingMember(this.lockoutDuration, "LockoutDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).LockoutDuration)));
			this.lockoutDuration.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.lockoutDuration.DecimalPlaces = 0;
			this.lockoutDuration.Decimals = 0;
			this.lockoutDuration.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 323, true);
			this.lockoutDuration.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 3, true);
			this.lockoutDuration.Name = "lockoutDuration";
			this.lockoutDuration.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.lockoutDuration.TabIndex = 13;
			this.lockoutDuration.Text = "0";
			this.lockoutDuration.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// requireAdminUnlock
			// 
			this.requireAdminUnlock.AutoSize = true;
			this.BindingSource.SetBindingMember(this.requireAdminUnlock, "RequireAdminUnlock");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).RequireAdminUnlock)));
			this.requireAdminUnlock.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.requireAdminUnlock.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 349, true);
			this.requireAdminUnlock.Name = "requireAdminUnlock";
			this.requireAdminUnlock.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.requireAdminUnlock.TabIndex = 14;
			// 
			// saveButton
			// 
			this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveButton.CaptionResourceString = Enterprise.Security.ActiveDirectory.GUI.Res.GetData("5C1A4901-24F7-40FF-93B4-B3A977C2D467", "Save");
			this.saveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 463, true);
			this.saveButton.Name = "saveButton";
			this.saveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.saveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.saveButton.TabIndex = 15;
			this.saveButton.ToolTipCaption = null;
			this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Security.ActiveDirectory.GUI.Res.GetData("0D125E4D-7B09-4145-B6C0-2B8B7B3AB8FD", "Cancel");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 463, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.cancelButton.TabIndex = 16;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// passwordSettingsGroupBox
			// 
			this.passwordSettingsGroupBox.CaptionResourceString = Enterprise.Security.ActiveDirectory.GUI.Res.GetData("1E55F9AA-A50C-4DC2-8B18-356521D65A79", "Password Settings");
			this.passwordSettingsGroupBox.Controls.Add(this.minimumPasswordLength);
			this.passwordSettingsGroupBox.Controls.Add(this.enforceMinimumPasswordLength);
			this.passwordSettingsGroupBox.Controls.Add(this.passwordHistoryLength);
			this.passwordSettingsGroupBox.Controls.Add(this.enforcePasswordHistoryLength);
			this.passwordSettingsGroupBox.Controls.Add(this.maximumPasswordAge);
			this.passwordSettingsGroupBox.Controls.Add(this.requireAdminUnlock);
			this.passwordSettingsGroupBox.Controls.Add(this.enforceMaximumPasswordAge);
			this.passwordSettingsGroupBox.Controls.Add(this.lockoutDuration);
			this.passwordSettingsGroupBox.Controls.Add(this.lockoutObservationWindow);
			this.passwordSettingsGroupBox.Controls.Add(this.enablePasswordComplexity);
			this.passwordSettingsGroupBox.Controls.Add(this.lockoutThreshold);
			this.passwordSettingsGroupBox.Controls.Add(this.minimumPasswordAge);
			this.passwordSettingsGroupBox.Controls.Add(this.enforcePasswordLockoutPolicy);
			this.passwordSettingsGroupBox.Controls.Add(this.enforceMinimumPasswordAge);
			this.passwordSettingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 45, true);
			this.passwordSettingsGroupBox.Name = "passwordSettingsGroupBox";
			this.passwordSettingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 377, true);
			this.passwordSettingsGroupBox.TabIndex = 0;
			this.passwordSettingsGroupBox.TabStop = false;
			// 
			// overrideDomainPasswordPolicy
			// 
			this.overrideDomainPasswordPolicy.AutoSize = true;
			this.BindingSource.SetBindingMember(this.overrideDomainPasswordPolicy, "OverrideDomainPasswordPolicy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).OverrideDomainPasswordPolicy)));
			this.overrideDomainPasswordPolicy.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.overrideDomainPasswordPolicy.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 22, true);
			this.overrideDomainPasswordPolicy.Name = "overrideDomainPasswordPolicy";
			this.overrideDomainPasswordPolicy.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 17, true);
			this.overrideDomainPasswordPolicy.TabIndex = 0;
			this.overrideDomainPasswordPolicy.CheckedChanged += new System.EventHandler(this.OverrideDomainPasswordPolicy_CheckedChanged);
			// 
			// adPSOName
			// 
			this.BindingSource.SetBindingMember(this.adPSOName, "ADPasswordSettingsObjectName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Security.ActiveDirectory.ADPasswordSettings)(null)).ADPasswordSettingsObjectName)));
			this.adPSOName.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.adPSOName.Enabled = false;
			this.adPSOName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 432, true);
			this.adPSOName.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 3, true);
			this.adPSOName.Name = "adPSOName";
			this.adPSOName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.adPSOName.TabIndex = 17;
			// 
			// ADPasswordSettingsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Security.ActiveDirectory.GUI.Res.GetData("C10771FD-5DE4-470D-9B57-1C6811E4AFF2", "Override Domain Password Policy");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 514, true);
			this.Controls.Add(this.adPSOName);
			this.Controls.Add(this.overrideDomainPasswordPolicy);
			this.Controls.Add(this.passwordSettingsGroupBox);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.cancelButton);
			this.DataSourceType = typeof(Enterprise.Security.ActiveDirectory.ADPasswordSettings);
			this.Name = "ADPasswordSettingsForm";
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.saveButton, 0);
			this.Controls.SetChildIndex(this.passwordSettingsGroupBox, 0);
			this.Controls.SetChildIndex(this.overrideDomainPasswordPolicy, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.adPSOName, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.passwordSettingsGroupBox.ResumeLayout(false);
			this.passwordSettingsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit maximumPasswordAge;
		private ZArchitecture.GUI.ZCheckBox enforceMaximumPasswordAge;
		private ZArchitecture.GUI.ZCheckBox enforceMinimumPasswordLength;
		private ZArchitecture.ZCalcEdit minimumPasswordLength;
		private ZArchitecture.ZCalcEdit passwordHistoryLength;
		private ZArchitecture.GUI.ZCheckBox enforcePasswordHistoryLength;
		private ZArchitecture.GUI.ZCheckBox enablePasswordComplexity;
		private ZArchitecture.GUI.ZCheckBox enforceMinimumPasswordAge;
		private ZArchitecture.ZCalcEdit minimumPasswordAge;
		private ZArchitecture.ZCalcEdit lockoutThreshold;
		private ZArchitecture.GUI.ZCheckBox enforcePasswordLockoutPolicy;
		private ZArchitecture.ZCalcEdit lockoutObservationWindow;
		private ZArchitecture.ZCalcEdit lockoutDuration;
		private ZArchitecture.GUI.ZCheckBox requireAdminUnlock;
		private ZArchitecture.GUI.ZButton saveButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZGroupBox passwordSettingsGroupBox;
		private ZArchitecture.GUI.ZCheckBox overrideDomainPasswordPolicy;
		private ZArchitecture.ZTextBox adPSOName;
	}
}