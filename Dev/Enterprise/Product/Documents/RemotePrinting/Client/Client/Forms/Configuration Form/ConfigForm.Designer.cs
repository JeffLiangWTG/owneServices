using System.Windows.Forms;

namespace Enterprise.RemotePrinting.Client
{
	partial class ConfigForm
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "CargoWise One WebPrint is a seperate entity regardless of product")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1014:EmbeddedIconRule", Justification = "Embedding a print icon, not a product icon")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
			this.DialogStatusStrip = new System.Windows.Forms.StatusStrip();
			this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.CloseButton = new System.Windows.Forms.Button();
			this.SaveButton = new System.Windows.Forms.Button();
			this.WebPrintClientTabControl = new System.Windows.Forms.TabControl();
			this.ConnectionConfigTabPage = new System.Windows.Forms.TabPage();
			this.ConnectionConfigPanel = new System.Windows.Forms.Panel();
			this.ConnectionConfigGroupBox = new System.Windows.Forms.GroupBox();
			this.ProxyGroupBox = new System.Windows.Forms.GroupBox();
			this.ProxyUseDefaultSystemSettingsGroupBox = new System.Windows.Forms.GroupBox();
			this.DefaultProxyPortTextBox = new System.Windows.Forms.TextBox();
			this.ProxyUseDefaultSystemSettingsCheckBox = new System.Windows.Forms.CheckBox();
			this.DefaultProxyPortLabel = new System.Windows.Forms.Label();
			this.DefaultProxyAddressTextBox = new System.Windows.Forms.TextBox();
			this.DefaultProxyAddressLabel = new System.Windows.Forms.Label();
			this.ProxyPortTextBox = new System.Windows.Forms.TextBox();
			this.ProxyPortLabel = new System.Windows.Forms.Label();
			this.ProxyEnabledCheckBox = new System.Windows.Forms.CheckBox();
			this.ProxyPwdLabel = new System.Windows.Forms.Label();
			this.ProxyPwdTextBox = new System.Windows.Forms.TextBox();
			this.ProxyUserLabel = new System.Windows.Forms.Label();
			this.ProxyUserTextBox = new System.Windows.Forms.TextBox();
			this.ProxyAddressLabel = new System.Windows.Forms.Label();
			this.ProxyAddressTextBox = new System.Windows.Forms.TextBox();
			this.LocalMachineNameLabel = new System.Windows.Forms.Label();
			this.TestConnectionButton = new System.Windows.Forms.Button();
			this.ConnectionRetryAttemptsTextBox = new System.Windows.Forms.TextBox();
			this.ConnectionRetryAttemptsLabel = new System.Windows.Forms.Label();
			this.RetryDelayLabel = new System.Windows.Forms.Label();
			this.RetryDelayTextBox = new System.Windows.Forms.TextBox();
			this.LocalMachineNameTextBox = new System.Windows.Forms.TextBox();
			this.WebServicePwdLabel = new System.Windows.Forms.Label();
			this.WebServicePwdTextBox = new System.Windows.Forms.TextBox();
			this.WebServiceUserLabel = new System.Windows.Forms.Label();
			this.WebServiceUserTextBox = new System.Windows.Forms.TextBox();
			this.WebServiceUrlLabel = new System.Windows.Forms.Label();
			this.WebServiceUrlTextBox = new System.Windows.Forms.TextBox();
			this.GeneralSettingsConfigTabPage = new System.Windows.Forms.TabPage();
			this.groupBoxKeepAlive = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxKeepAliveInterval = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxKeepAliveTime = new System.Windows.Forms.TextBox();
			this.checkBoxKeepAlive = new System.Windows.Forms.CheckBox();
			this.groupBoxAppSetting = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.MemoryUsageMonitoringTextBox = new System.Windows.Forms.TextBox();
			this.EnableMemoryUsageMonitoring = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textBoxJobPrintingTimeout = new System.Windows.Forms.TextBox();
			this.AppSettingLabel2 = new System.Windows.Forms.Label();
			this.RemotePrintingServiceTimeoutInSecondsTextBox = new System.Windows.Forms.TextBox();
			this.PrintNudgingGroupBox = new System.Windows.Forms.GroupBox();
			this.EnableSignalRCheckBox = new System.Windows.Forms.CheckBox();
			this.EnablePauseSignalRCheckBox = new System.Windows.Forms.CheckBox();
			this.ReconnectionAttemptsTextBox = new System.Windows.Forms.TextBox();
			this.ReconnectionAttemptsLable = new System.Windows.Forms.Label();
			this.ReconnectionLimitMinutesTextBox = new System.Windows.Forms.TextBox();
			this.ReconnectionLimitMinutesLabel = new System.Windows.Forms.Label();
			this.PauseSignalRMinutesTextBox = new System.Windows.Forms.TextBox();
			this.PauseSignalRMinutesLabel = new System.Windows.Forms.Label();
			this.checkBoxEnableVerboseLogging = new System.Windows.Forms.CheckBox();
			this.EnableExpect100ContinueCheckBox = new System.Windows.Forms.CheckBox();
			this.RefreshTimeForNewPrintersScanLabel = new System.Windows.Forms.Label();
			this.RefreshTimeForNewPrintersScanTextBox = new System.Windows.Forms.TextBox();
			this.PauseInSecondsLabel = new System.Windows.Forms.Label();
			this.RequestPauseInSecondsTextBox = new System.Windows.Forms.TextBox();
			this.tabPageUpdate = new System.Windows.Forms.TabPage();
			this.CheckUpdateButton = new System.Windows.Forms.Button();
			this.AppSettingLabel1 = new System.Windows.Forms.Label();
			this.NumberOfLoopsToCheckForUpdateTextBox = new System.Windows.Forms.TextBox();
			this.groupBoxNotifications = new System.Windows.Forms.GroupBox();
			this.checkBoxNotifyBeforeUpdate = new System.Windows.Forms.CheckBox();
			this.checkBoxNotifyAfterUpdate = new System.Windows.Forms.CheckBox();
			this.checkBoxUpdateNotification = new System.Windows.Forms.CheckBox();
			this.groupBoxUpdateDuring = new System.Windows.Forms.GroupBox();
			this.checkBoxUpdateOnSunday = new System.Windows.Forms.CheckBox();
			this.checkBoxUpdateOnSaturday = new System.Windows.Forms.CheckBox();
			this.checkBoxUpdateOnFriday = new System.Windows.Forms.CheckBox();
			this.checkBoxUpdateOnThursday = new System.Windows.Forms.CheckBox();
			this.checkBoxUpdateOnWednesday = new System.Windows.Forms.CheckBox();
			this.checkBoxUpdateOnTuesday = new System.Windows.Forms.CheckBox();
			this.checkBoxUpdateOnMonday = new System.Windows.Forms.CheckBox();
			this.dateTimeUpdateTo = new System.Windows.Forms.DateTimePicker();
			this.dateTimeUpdateFrom = new System.Windows.Forms.DateTimePicker();
			this.labelTo = new System.Windows.Forms.Label();
			this.labelFrom = new System.Windows.Forms.Label();
			this.labelDays = new System.Windows.Forms.Label();
			this.textBoxUpdateNDays = new System.Windows.Forms.TextBox();
			this.checkBoxAutoUpdateAfterNDays = new System.Windows.Forms.CheckBox();
			this.checkBoxAutoUpdateMinorVersion = new System.Windows.Forms.CheckBox();
			this.checkBoxAutoUpdateMajorVersion = new System.Windows.Forms.CheckBox();
			this.groupBoxUpdateMode = new System.Windows.Forms.GroupBox();
			this.radioButtonUpdateCustom = new System.Windows.Forms.RadioButton();
			this.radioButtonUpdateManual = new System.Windows.Forms.RadioButton();
			this.radioButtonAutoUpdate = new System.Windows.Forms.RadioButton();
			this.PauseAutoUpdateLabel = new System.Windows.Forms.Label();
			this.PauseAutoUpdateHoursLabel = new System.Windows.Forms.Label();
			this.PauseAutoUpdateHoursTextBox = new System.Windows.Forms.TextBox();
			this.LogManagerTabPage = new System.Windows.Forms.TabPage();
			this.OpenLogDirectoryButton = new System.Windows.Forms.Button();
			this.LogFileDirectoryLabel = new System.Windows.Forms.Label();
			this.LogPathTextBox = new System.Windows.Forms.TextBox();
			this.LogSettingsGroupBox = new System.Windows.Forms.GroupBox();
			this.EnableCleaningOldLogCheckBox = new System.Windows.Forms.CheckBox();
			this.DayToKeepAliveLabel = new System.Windows.Forms.Label();
			this.LogFileDayToKeepTextBox = new System.Windows.Forms.TextBox();
			this.ServiceTabPage = new System.Windows.Forms.TabPage();
			this.ServiceSettingsGroupBox = new System.Windows.Forms.GroupBox();
			this.ServiceStatusTextBox = new System.Windows.Forms.TextBox();
			this.ServiceConfigNameTextBox = new System.Windows.Forms.TextBox();
			this.ServiceStartModeLabel = new System.Windows.Forms.Label();
			this.ServiceConfigNameLabel = new System.Windows.Forms.Label();
			this.ServiceDisplayNameLabel = new System.Windows.Forms.Label();
			this.ServiceDisplayNameTextBox = new System.Windows.Forms.TextBox();
			this.ServiceNameLabel = new System.Windows.Forms.Label();
			this.CheckServiceStatus = new System.Windows.Forms.Button();
			this.ServiceInstallationNameTextBox = new System.Windows.Forms.TextBox();
			this.ServiceStartModeComboBox = new System.Windows.Forms.ComboBox();
			this.ServiceStatusLabel = new System.Windows.Forms.Label();
			this.InstallServiceButton = new System.Windows.Forms.Button();
			this.UninstallServiceButton = new System.Windows.Forms.Button();
			this.StartServiceButton = new System.Windows.Forms.Button();
			this.StopServiceButton = new System.Windows.Forms.Button();
			this.DeleteConfigurationButton = new System.Windows.Forms.Button();
			this.DuplicateConfigurationButton = new System.Windows.Forms.Button();
			this.RenameConfigurationButton = new System.Windows.Forms.Button();
			this.NewConfigurationButton = new System.Windows.Forms.Button();
			this.ConfigurationsLabel = new System.Windows.Forms.Label();
			this.ConfigurationsComboBox = new System.Windows.Forms.ComboBox();
			this.toolTipManager = new System.Windows.Forms.ToolTip(this.components);
			this.DialogStatusStrip.SuspendLayout();
			this.WebPrintClientTabControl.SuspendLayout();
			this.ConnectionConfigTabPage.SuspendLayout();
			this.ConnectionConfigPanel.SuspendLayout();
			this.ConnectionConfigGroupBox.SuspendLayout();
			this.ProxyGroupBox.SuspendLayout();
			this.ProxyUseDefaultSystemSettingsGroupBox.SuspendLayout();
			this.GeneralSettingsConfigTabPage.SuspendLayout();
			this.groupBoxKeepAlive.SuspendLayout();
			this.PrintNudgingGroupBox.SuspendLayout();
			this.groupBoxAppSetting.SuspendLayout();
			this.tabPageUpdate.SuspendLayout();
			this.groupBoxNotifications.SuspendLayout();
			this.groupBoxUpdateDuring.SuspendLayout();
			this.groupBoxUpdateMode.SuspendLayout();
			this.LogManagerTabPage.SuspendLayout();
			this.LogSettingsGroupBox.SuspendLayout();
			this.ServiceTabPage.SuspendLayout();
			this.ServiceSettingsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// DialogStatusStrip
			// 
			this.DialogStatusStrip.AllowMerge = false;
			this.DialogStatusStrip.GripMargin = new System.Windows.Forms.Padding(0);
			this.DialogStatusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.DialogStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
			this.DialogStatusStrip.Location = new System.Drawing.Point(0, 636);
			this.DialogStatusStrip.Name = "DialogStatusStrip";
			this.DialogStatusStrip.Size = new System.Drawing.Size(440, 22);
			this.DialogStatusStrip.SizingGrip = false;
			this.DialogStatusStrip.Stretch = false;
			this.DialogStatusStrip.TabIndex = 10;
			this.DialogStatusStrip.DoubleClick += new System.EventHandler(this.DialogStatusStrip_DoubleClick);
			// 
			// StatusLabel
			// 
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = new System.Drawing.Size(0, 17);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = new System.Drawing.Point(356, 606);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(75, 23);
			this.CloseButton.TabIndex = 9;
			this.CloseButton.Text = "Cancel";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.SaveButton.Location = new System.Drawing.Point(275, 606);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = new System.Drawing.Size(75, 23);
			this.SaveButton.TabIndex = 8;
			this.SaveButton.Text = "Save";
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// WebPrintClientTabControl
			// 
			this.WebPrintClientTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WebPrintClientTabControl.Controls.Add(this.ConnectionConfigTabPage);
			this.WebPrintClientTabControl.Controls.Add(this.GeneralSettingsConfigTabPage);
			this.WebPrintClientTabControl.Controls.Add(this.tabPageUpdate);
			this.WebPrintClientTabControl.Controls.Add(this.LogManagerTabPage);
			this.WebPrintClientTabControl.Controls.Add(this.ServiceTabPage);
			this.WebPrintClientTabControl.Location = new System.Drawing.Point(0, 44);
			this.WebPrintClientTabControl.Name = "WebPrintClientTabControl";
			this.WebPrintClientTabControl.SelectedIndex = 0;
			this.WebPrintClientTabControl.Size = new System.Drawing.Size(440, 556);
			this.WebPrintClientTabControl.TabIndex = 7;
			this.WebPrintClientTabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.WebPrintClientTab_Selected);
			// 
			// ConnectionConfigTabPage
			// 
			this.ConnectionConfigTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ConnectionConfigTabPage.Controls.Add(this.ConnectionConfigPanel);
			this.ConnectionConfigTabPage.Location = new System.Drawing.Point(4, 22);
			this.ConnectionConfigTabPage.Name = "ConnectionConfigTabPage";
			this.ConnectionConfigTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.ConnectionConfigTabPage.Size = new System.Drawing.Size(432, 530);
			this.ConnectionConfigTabPage.TabIndex = 0;
			this.ConnectionConfigTabPage.Text = "Connection";
			// 
			// ConnectionConfigPanel
			// 
			this.ConnectionConfigPanel.Controls.Add(this.ConnectionConfigGroupBox);
			this.ConnectionConfigPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConnectionConfigPanel.Location = new System.Drawing.Point(3, 3);
			this.ConnectionConfigPanel.Name = "ConnectionConfigPanel";
			this.ConnectionConfigPanel.Size = new System.Drawing.Size(426, 524);
			this.ConnectionConfigPanel.TabIndex = 0;
			// 
			// ConnectionConfigGroupBox
			// 
			this.ConnectionConfigGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ConnectionConfigGroupBox.Controls.Add(this.ProxyGroupBox);
			this.ConnectionConfigGroupBox.Controls.Add(this.LocalMachineNameLabel);
			this.ConnectionConfigGroupBox.Controls.Add(this.TestConnectionButton);
			this.ConnectionConfigGroupBox.Controls.Add(this.ConnectionRetryAttemptsTextBox);
			this.ConnectionConfigGroupBox.Controls.Add(this.ConnectionRetryAttemptsLabel);
			this.ConnectionConfigGroupBox.Controls.Add(this.RetryDelayLabel);
			this.ConnectionConfigGroupBox.Controls.Add(this.RetryDelayTextBox);
			this.ConnectionConfigGroupBox.Controls.Add(this.LocalMachineNameTextBox);
			this.ConnectionConfigGroupBox.Controls.Add(this.WebServicePwdLabel);
			this.ConnectionConfigGroupBox.Controls.Add(this.WebServicePwdTextBox);
			this.ConnectionConfigGroupBox.Controls.Add(this.WebServiceUserLabel);
			this.ConnectionConfigGroupBox.Controls.Add(this.WebServiceUserTextBox);
			this.ConnectionConfigGroupBox.Controls.Add(this.WebServiceUrlLabel);
			this.ConnectionConfigGroupBox.Controls.Add(this.WebServiceUrlTextBox);
			this.ConnectionConfigGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ConnectionConfigGroupBox.Location = new System.Drawing.Point(3, 3);
			this.ConnectionConfigGroupBox.Name = "ConnectionConfigGroupBox";
			this.ConnectionConfigGroupBox.Size = new System.Drawing.Size(423, 518);
			this.ConnectionConfigGroupBox.TabIndex = 0;
			this.ConnectionConfigGroupBox.TabStop = false;
			this.ConnectionConfigGroupBox.Text = "Connection Settings";
			// 
			// ProxyGroupBox
			// 
			this.ProxyGroupBox.Controls.Add(this.ProxyUseDefaultSystemSettingsGroupBox);
			this.ProxyGroupBox.Controls.Add(this.ProxyPortTextBox);
			this.ProxyGroupBox.Controls.Add(this.ProxyPortLabel);
			this.ProxyGroupBox.Controls.Add(this.ProxyEnabledCheckBox);
			this.ProxyGroupBox.Controls.Add(this.ProxyPwdLabel);
			this.ProxyGroupBox.Controls.Add(this.ProxyPwdTextBox);
			this.ProxyGroupBox.Controls.Add(this.ProxyUserLabel);
			this.ProxyGroupBox.Controls.Add(this.ProxyUserTextBox);
			this.ProxyGroupBox.Controls.Add(this.ProxyAddressLabel);
			this.ProxyGroupBox.Controls.Add(this.ProxyAddressTextBox);
			this.ProxyGroupBox.Location = new System.Drawing.Point(6, 197);
			this.ProxyGroupBox.Name = "ProxyGroupBox";
			this.ProxyGroupBox.Size = new System.Drawing.Size(412, 242);
			this.ProxyGroupBox.TabIndex = 8;
			this.ProxyGroupBox.TabStop = false;
			this.ProxyGroupBox.Text = "Proxy";
			// 
			// ProxyUseDefaultSystemSettingsGroupBox
			// 
			this.ProxyUseDefaultSystemSettingsGroupBox.Controls.Add(this.DefaultProxyPortTextBox);
			this.ProxyUseDefaultSystemSettingsGroupBox.Controls.Add(this.ProxyUseDefaultSystemSettingsCheckBox);
			this.ProxyUseDefaultSystemSettingsGroupBox.Controls.Add(this.DefaultProxyPortLabel);
			this.ProxyUseDefaultSystemSettingsGroupBox.Controls.Add(this.DefaultProxyAddressTextBox);
			this.ProxyUseDefaultSystemSettingsGroupBox.Controls.Add(this.DefaultProxyAddressLabel);
			this.ProxyUseDefaultSystemSettingsGroupBox.Location = new System.Drawing.Point(6, 44);
			this.ProxyUseDefaultSystemSettingsGroupBox.Name = "ProxyUseDefaultSystemSettingsGroupBox";
			this.ProxyUseDefaultSystemSettingsGroupBox.Size = new System.Drawing.Size(400, 104);
			this.ProxyUseDefaultSystemSettingsGroupBox.TabIndex = 1;
			this.ProxyUseDefaultSystemSettingsGroupBox.TabStop = false;
			this.ProxyUseDefaultSystemSettingsGroupBox.Text = "Default system settings";
			// 
			// DefaultProxyPortTextBox
			// 
			this.DefaultProxyPortTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DefaultProxyPortTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DefaultProxyPortTextBox.Location = new System.Drawing.Point(348, 67);
			this.DefaultProxyPortTextBox.Name = "DefaultProxyPortTextBox";
			this.DefaultProxyPortTextBox.ReadOnly = true;
			this.DefaultProxyPortTextBox.Size = new System.Drawing.Size(46, 20);
			this.DefaultProxyPortTextBox.TabIndex = 4;
			this.DefaultProxyPortTextBox.TabStop = false;
			// 
			// ProxyUseDefaultSystemSettingsCheckBox
			// 
			this.ProxyUseDefaultSystemSettingsCheckBox.AutoSize = true;
			this.ProxyUseDefaultSystemSettingsCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProxyUseDefaultSystemSettingsCheckBox.Location = new System.Drawing.Point(6, 22);
			this.ProxyUseDefaultSystemSettingsCheckBox.Name = "ProxyUseDefaultSystemSettingsCheckBox";
			this.ProxyUseDefaultSystemSettingsCheckBox.Size = new System.Drawing.Size(154, 17);
			this.ProxyUseDefaultSystemSettingsCheckBox.TabIndex = 0;
			this.ProxyUseDefaultSystemSettingsCheckBox.Text = "Use default system settings";
			this.ProxyUseDefaultSystemSettingsCheckBox.UseVisualStyleBackColor = true;
			this.ProxyUseDefaultSystemSettingsCheckBox.CheckedChanged += new System.EventHandler(this.ProxyUseDefaultSystemSettingsCheckBox_Click);
			// 
			// DefaultProxyPortLabel
			// 
			this.DefaultProxyPortLabel.AutoSize = true;
			this.DefaultProxyPortLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DefaultProxyPortLabel.Location = new System.Drawing.Point(348, 51);
			this.DefaultProxyPortLabel.Name = "DefaultProxyPortLabel";
			this.DefaultProxyPortLabel.Size = new System.Drawing.Size(29, 13);
			this.DefaultProxyPortLabel.TabIndex = 3;
			this.DefaultProxyPortLabel.Text = "Port:";
			// 
			// DefaultProxyAddressTextBox
			// 
			this.DefaultProxyAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DefaultProxyAddressTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DefaultProxyAddressTextBox.Location = new System.Drawing.Point(6, 67);
			this.DefaultProxyAddressTextBox.Name = "DefaultProxyAddressTextBox";
			this.DefaultProxyAddressTextBox.ReadOnly = true;
			this.DefaultProxyAddressTextBox.Size = new System.Drawing.Size(336, 20);
			this.DefaultProxyAddressTextBox.TabIndex = 2;
			this.DefaultProxyAddressTextBox.TabStop = false;
			// 
			// DefaultProxyAddressLabel
			// 
			this.DefaultProxyAddressLabel.AutoSize = true;
			this.DefaultProxyAddressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DefaultProxyAddressLabel.Location = new System.Drawing.Point(6, 51);
			this.DefaultProxyAddressLabel.Name = "DefaultProxyAddressLabel";
			this.DefaultProxyAddressLabel.Size = new System.Drawing.Size(77, 13);
			this.DefaultProxyAddressLabel.TabIndex = 1;
			this.DefaultProxyAddressLabel.Text = "Proxy Address:";
			// 
			// ProxyPortTextBox
			// 
			this.ProxyPortTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ProxyPortTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProxyPortTextBox.Location = new System.Drawing.Point(354, 167);
			this.ProxyPortTextBox.Name = "ProxyPortTextBox";
			this.ProxyPortTextBox.Size = new System.Drawing.Size(52, 20);
			this.ProxyPortTextBox.TabIndex = 5;
			this.ProxyPortTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ProxyPortTextBox_KeyPress);
			// 
			// ProxyPortLabel
			// 
			this.ProxyPortLabel.AutoSize = true;
			this.ProxyPortLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProxyPortLabel.Location = new System.Drawing.Point(354, 151);
			this.ProxyPortLabel.Name = "ProxyPortLabel";
			this.ProxyPortLabel.Size = new System.Drawing.Size(29, 13);
			this.ProxyPortLabel.TabIndex = 4;
			this.ProxyPortLabel.Text = "Port:";
			// 
			// ProxyEnabledCheckBox
			// 
			this.ProxyEnabledCheckBox.AutoSize = true;
			this.ProxyEnabledCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProxyEnabledCheckBox.Location = new System.Drawing.Point(6, 21);
			this.ProxyEnabledCheckBox.Name = "ProxyEnabledCheckBox";
			this.ProxyEnabledCheckBox.Size = new System.Drawing.Size(65, 17);
			this.ProxyEnabledCheckBox.TabIndex = 0;
			this.ProxyEnabledCheckBox.Text = "Enabled";
			this.ProxyEnabledCheckBox.UseVisualStyleBackColor = true;
			this.ProxyEnabledCheckBox.CheckedChanged += new System.EventHandler(this.ProxyEnabledCheckBox_Click);
			// 
			// ProxyPwdLabel
			// 
			this.ProxyPwdLabel.AutoSize = true;
			this.ProxyPwdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProxyPwdLabel.Location = new System.Drawing.Point(216, 194);
			this.ProxyPwdLabel.Name = "ProxyPwdLabel";
			this.ProxyPwdLabel.Size = new System.Drawing.Size(56, 13);
			this.ProxyPwdLabel.TabIndex = 8;
			this.ProxyPwdLabel.Text = "Password:";
			// 
			// ProxyPwdTextBox
			// 
			this.ProxyPwdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ProxyPwdTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProxyPwdTextBox.Location = new System.Drawing.Point(216, 210);
			this.ProxyPwdTextBox.Name = "ProxyPwdTextBox";
			this.ProxyPwdTextBox.PasswordChar = '*';
			this.ProxyPwdTextBox.Size = new System.Drawing.Size(190, 20);
			this.ProxyPwdTextBox.TabIndex = 9;
			// 
			// ProxyUserLabel
			// 
			this.ProxyUserLabel.AutoSize = true;
			this.ProxyUserLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProxyUserLabel.Location = new System.Drawing.Point(6, 194);
			this.ProxyUserLabel.Name = "ProxyUserLabel";
			this.ProxyUserLabel.Size = new System.Drawing.Size(32, 13);
			this.ProxyUserLabel.TabIndex = 6;
			this.ProxyUserLabel.Text = "User:";
			// 
			// ProxyUserTextBox
			// 
			this.ProxyUserTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProxyUserTextBox.Location = new System.Drawing.Point(6, 210);
			this.ProxyUserTextBox.Name = "ProxyUserTextBox";
			this.ProxyUserTextBox.Size = new System.Drawing.Size(204, 20);
			this.ProxyUserTextBox.TabIndex = 7;
			// 
			// ProxyAddressLabel
			// 
			this.ProxyAddressLabel.AutoSize = true;
			this.ProxyAddressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProxyAddressLabel.Location = new System.Drawing.Point(6, 151);
			this.ProxyAddressLabel.Name = "ProxyAddressLabel";
			this.ProxyAddressLabel.Size = new System.Drawing.Size(77, 13);
			this.ProxyAddressLabel.TabIndex = 2;
			this.ProxyAddressLabel.Text = "Proxy Address:";
			// 
			// ProxyAddressTextBox
			// 
			this.ProxyAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ProxyAddressTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProxyAddressTextBox.Location = new System.Drawing.Point(6, 167);
			this.ProxyAddressTextBox.Name = "ProxyAddressTextBox";
			this.ProxyAddressTextBox.Size = new System.Drawing.Size(342, 20);
			this.ProxyAddressTextBox.TabIndex = 3;
			// 
			// LocalMachineNameLabel
			// 
			this.LocalMachineNameLabel.AutoSize = true;
			this.LocalMachineNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LocalMachineNameLabel.Location = new System.Drawing.Point(6, 109);
			this.LocalMachineNameLabel.Name = "LocalMachineNameLabel";
			this.LocalMachineNameLabel.Size = new System.Drawing.Size(109, 13);
			this.LocalMachineNameLabel.TabIndex = 6;
			this.LocalMachineNameLabel.Text = "Local Computer Alias:";
			// 
			// TestConnectionButton
			// 
			this.TestConnectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.TestConnectionButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TestConnectionButton.Location = new System.Drawing.Point(6, 485);
			this.TestConnectionButton.Name = "TestConnectionButton";
			this.TestConnectionButton.Size = new System.Drawing.Size(101, 23);
			this.TestConnectionButton.TabIndex = 13;
			this.TestConnectionButton.Text = "Test Connection";
			this.TestConnectionButton.UseVisualStyleBackColor = true;
			this.TestConnectionButton.Click += new System.EventHandler(this.TestConnectionButton_Click);
			// 
			// ConnectionRetryAttemptsTextBox
			// 
			this.ConnectionRetryAttemptsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ConnectionRetryAttemptsTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ConnectionRetryAttemptsTextBox.Location = new System.Drawing.Point(100, 158);
			this.ConnectionRetryAttemptsTextBox.MaxLength = 2;
			this.ConnectionRetryAttemptsTextBox.Name = "ConnectionRetryAttemptsTextBox";
			this.ConnectionRetryAttemptsTextBox.Size = new System.Drawing.Size(116, 20);
			this.ConnectionRetryAttemptsTextBox.TabIndex = 12;
			this.toolTipManager.SetToolTip(this.ConnectionRetryAttemptsTextBox, "Number of attempts client will try to connect to Web Service when starting.");
			this.ConnectionRetryAttemptsTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ConnectionRetryAttemptsTextBox_KeyPress);
			// 
			// ConnectionRetryAttemptsLabel
			// 
			this.ConnectionRetryAttemptsLabel.AutoSize = true;
			this.ConnectionRetryAttemptsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ConnectionRetryAttemptsLabel.Location = new System.Drawing.Point(6, 161);
			this.ConnectionRetryAttemptsLabel.Name = "ConnectionRetryAttemptsLabel";
			this.ConnectionRetryAttemptsLabel.Size = new System.Drawing.Size(95, 13);
			this.ConnectionRetryAttemptsLabel.TabIndex = 11;
			this.ConnectionRetryAttemptsLabel.Text = "Connection retries:";
			// 
			// RetryDelayLabel
			// 
			this.RetryDelayLabel.AutoSize = true;
			this.RetryDelayLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RetryDelayLabel.Location = new System.Drawing.Point(222, 161);
			this.RetryDelayLabel.Name = "RetryDelayLabel";
			this.RetryDelayLabel.Size = new System.Drawing.Size(112, 13);
			this.RetryDelayLabel.TabIndex = 9;
			this.RetryDelayLabel.Text = "Retry delay (seconds):";
			// 
			// RetryDelayTextBox
			// 
			this.RetryDelayTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RetryDelayTextBox.Location = new System.Drawing.Point(335, 158);
			this.RetryDelayTextBox.Name = "RetryDelayTextBox";
			this.RetryDelayTextBox.Size = new System.Drawing.Size(75, 20);
			this.RetryDelayTextBox.TabIndex = 10;
			this.toolTipManager.SetToolTip(this.RetryDelayTextBox, "Delay in seconds between attempts to connect to Web Service.");
			this.RetryDelayTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RetryDelayTextBox_KeyPress);
			// 
			// LocalMachineNameTextBox
			// 
			this.LocalMachineNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LocalMachineNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LocalMachineNameTextBox.Location = new System.Drawing.Point(6, 126);
			this.LocalMachineNameTextBox.Name = "LocalMachineNameTextBox";
			this.LocalMachineNameTextBox.Size = new System.Drawing.Size(210, 20);
			this.LocalMachineNameTextBox.TabIndex = 7;
			// 
			// WebServicePwdLabel
			// 
			this.WebServicePwdLabel.AutoSize = true;
			this.WebServicePwdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.WebServicePwdLabel.Location = new System.Drawing.Point(222, 64);
			this.WebServicePwdLabel.Name = "WebServicePwdLabel";
			this.WebServicePwdLabel.Size = new System.Drawing.Size(56, 13);
			this.WebServicePwdLabel.TabIndex = 4;
			this.WebServicePwdLabel.Text = "Password:";
			// 
			// WebServicePwdTextBox
			// 
			this.WebServicePwdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WebServicePwdTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.WebServicePwdTextBox.Location = new System.Drawing.Point(222, 80);
			this.WebServicePwdTextBox.Name = "WebServicePwdTextBox";
			this.WebServicePwdTextBox.PasswordChar = '*';
			this.WebServicePwdTextBox.Size = new System.Drawing.Size(192, 20);
			this.WebServicePwdTextBox.TabIndex = 5;
			// 
			// WebServiceUserLabel
			// 
			this.WebServiceUserLabel.AutoSize = true;
			this.WebServiceUserLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.WebServiceUserLabel.Location = new System.Drawing.Point(6, 65);
			this.WebServiceUserLabel.Name = "WebServiceUserLabel";
			this.WebServiceUserLabel.Size = new System.Drawing.Size(32, 13);
			this.WebServiceUserLabel.TabIndex = 2;
			this.WebServiceUserLabel.Text = "User:";
			// 
			// WebServiceUserTextBox
			// 
			this.WebServiceUserTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.WebServiceUserTextBox.Location = new System.Drawing.Point(6, 80);
			this.WebServiceUserTextBox.Name = "WebServiceUserTextBox";
			this.WebServiceUserTextBox.Size = new System.Drawing.Size(210, 20);
			this.WebServiceUserTextBox.TabIndex = 3;
			// 
			// WebServiceUrlLabel
			// 
			this.WebServiceUrlLabel.AutoSize = true;
			this.WebServiceUrlLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.WebServiceUrlLabel.Location = new System.Drawing.Point(6, 22);
			this.WebServiceUrlLabel.Name = "WebServiceUrlLabel";
			this.WebServiceUrlLabel.Size = new System.Drawing.Size(97, 13);
			this.WebServiceUrlLabel.TabIndex = 0;
			this.WebServiceUrlLabel.Text = "Web Service URL:";
			// 
			// WebServiceUrlTextBox
			// 
			this.WebServiceUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WebServiceUrlTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.WebServiceUrlTextBox.Location = new System.Drawing.Point(6, 38);
			this.WebServiceUrlTextBox.Name = "WebServiceUrlTextBox";
			this.WebServiceUrlTextBox.Size = new System.Drawing.Size(408, 20);
			this.WebServiceUrlTextBox.TabIndex = 1;
			// 
			// GeneralSettingsConfigTabPage
			// 
			this.GeneralSettingsConfigTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.GeneralSettingsConfigTabPage.Controls.Add(this.groupBoxKeepAlive);
			this.GeneralSettingsConfigTabPage.Controls.Add(this.PrintNudgingGroupBox);
			this.GeneralSettingsConfigTabPage.Controls.Add(this.groupBoxAppSetting);
			this.GeneralSettingsConfigTabPage.Controls.Add(this.RefreshTimeForNewPrintersScanLabel);
			this.GeneralSettingsConfigTabPage.Controls.Add(this.RefreshTimeForNewPrintersScanTextBox);
			this.GeneralSettingsConfigTabPage.Controls.Add(this.PauseInSecondsLabel);
			this.GeneralSettingsConfigTabPage.Controls.Add(this.RequestPauseInSecondsTextBox);
			this.GeneralSettingsConfigTabPage.Location = new System.Drawing.Point(4, 22);
			this.GeneralSettingsConfigTabPage.Name = "GeneralSettingsConfigTabPage";
			this.GeneralSettingsConfigTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.GeneralSettingsConfigTabPage.Size = new System.Drawing.Size(432, 530);
			this.GeneralSettingsConfigTabPage.TabIndex = 1;
			this.GeneralSettingsConfigTabPage.Text = "General Settings";
			// 
			// groupBoxKeepAlive
			// 
			this.groupBoxKeepAlive.Controls.Add(this.label1);
			this.groupBoxKeepAlive.Controls.Add(this.textBoxKeepAliveInterval);
			this.groupBoxKeepAlive.Controls.Add(this.label2);
			this.groupBoxKeepAlive.Controls.Add(this.textBoxKeepAliveTime);
			this.groupBoxKeepAlive.Controls.Add(this.checkBoxKeepAlive);
			this.groupBoxKeepAlive.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBoxKeepAlive.Location = new System.Drawing.Point(8, 93);
			this.groupBoxKeepAlive.Name = "groupBoxKeepAlive";
			this.groupBoxKeepAlive.Size = new System.Drawing.Size(416, 137);
			this.groupBoxKeepAlive.TabIndex = 4;
			this.groupBoxKeepAlive.TabStop = false;
			this.groupBoxKeepAlive.Text = "Keep Alive";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(6, 90);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(341, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Interval between keep-alive packets if not acknowledged (in seconds):";
			// 
			// textBoxKeepAliveInterval
			// 
			this.textBoxKeepAliveInterval.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxKeepAliveInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxKeepAliveInterval.Location = new System.Drawing.Point(6, 106);
			this.textBoxKeepAliveInterval.MaxLength = 7;
			this.textBoxKeepAliveInterval.Name = "textBoxKeepAliveInterval";
			this.textBoxKeepAliveInterval.Size = new System.Drawing.Size(150, 20);
			this.textBoxKeepAliveInterval.TabIndex = 4;
			this.textBoxKeepAliveInterval.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxKeepAliveInterval_KeyPress);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(6, 46);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(322, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Time of no activity before sending keep-alive packets (in seconds):";
			// 
			// textBoxKeepAliveTime
			// 
			this.textBoxKeepAliveTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxKeepAliveTime.Location = new System.Drawing.Point(6, 62);
			this.textBoxKeepAliveTime.MaxLength = 7;
			this.textBoxKeepAliveTime.Name = "textBoxKeepAliveTime";
			this.textBoxKeepAliveTime.Size = new System.Drawing.Size(150, 20);
			this.textBoxKeepAliveTime.TabIndex = 2;
			this.textBoxKeepAliveTime.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxKeepAliveTime_KeyPress);
			// 
			// checkBoxKeepAlive
			// 
			this.checkBoxKeepAlive.AutoSize = true;
			this.checkBoxKeepAlive.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxKeepAlive.Location = new System.Drawing.Point(6, 19);
			this.checkBoxKeepAlive.Name = "checkBoxKeepAlive";
			this.checkBoxKeepAlive.Size = new System.Drawing.Size(65, 17);
			this.checkBoxKeepAlive.TabIndex = 0;
			this.checkBoxKeepAlive.Text = "Enabled";
			this.checkBoxKeepAlive.UseVisualStyleBackColor = true;
			this.checkBoxKeepAlive.CheckedChanged += new System.EventHandler(this.checkBoxKeepAlive_CheckedChanged);
			// 
			// groupBoxAppSetting
			// 
			this.groupBoxAppSetting.Controls.Add(this.label4);
			this.groupBoxAppSetting.Controls.Add(this.MemoryUsageMonitoringTextBox);
			this.groupBoxAppSetting.Controls.Add(this.EnableMemoryUsageMonitoring);
			this.groupBoxAppSetting.Controls.Add(this.label3);
			this.groupBoxAppSetting.Controls.Add(this.textBoxJobPrintingTimeout);
			this.groupBoxAppSetting.Controls.Add(this.AppSettingLabel2);
			this.groupBoxAppSetting.Controls.Add(this.RemotePrintingServiceTimeoutInSecondsTextBox);
			this.groupBoxAppSetting.Controls.Add(this.checkBoxEnableVerboseLogging);
			this.groupBoxAppSetting.Controls.Add(this.EnableExpect100ContinueCheckBox);
			this.groupBoxAppSetting.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBoxAppSetting.Location = new System.Drawing.Point(8, 340);
			this.groupBoxAppSetting.Name = "groupBoxAppSetting";
			this.groupBoxAppSetting.Size = new System.Drawing.Size(416, 180);
			this.groupBoxAppSetting.TabIndex = 5;
			this.groupBoxAppSetting.TabStop = false;
			this.groupBoxAppSetting.Text = "Application Settings";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(6, 90);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(127, 13);
			this.label4.TabIndex = 28;
			this.label4.Text = "Memory threshold (in mb):";
			// 
			// MemoryUsageMonitoringTextBox
			// 
			this.MemoryUsageMonitoringTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MemoryUsageMonitoringTextBox.Location = new System.Drawing.Point(230, 90);
			this.MemoryUsageMonitoringTextBox.MaxLength = 5;
			this.MemoryUsageMonitoringTextBox.Name = "MemoryUsageMonitoringTextBox";
			this.MemoryUsageMonitoringTextBox.Size = new System.Drawing.Size(150, 20);
			this.MemoryUsageMonitoringTextBox.TabIndex = 27;
			this.MemoryUsageMonitoringTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.memoryUsageMonitoringTextBox_KeyPress);
			// 
			// EnableMemoryUsageMonitoring
			// 
			this.EnableMemoryUsageMonitoring.AutoSize = true;
			this.EnableMemoryUsageMonitoring.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EnableMemoryUsageMonitoring.Location = new System.Drawing.Point(6, 65);
			this.EnableMemoryUsageMonitoring.Name = "EnableMemoryUsageMonitoring";
			this.EnableMemoryUsageMonitoring.Size = new System.Drawing.Size(185, 17);
			this.EnableMemoryUsageMonitoring.TabIndex = 26;
			this.EnableMemoryUsageMonitoring.Text = "Enable Memory Usage Monitoring";
			this.EnableMemoryUsageMonitoring.UseVisualStyleBackColor = true;
			this.EnableMemoryUsageMonitoring.CheckedChanged += new System.EventHandler(this.enableMemoryUsageMonitoring_CheckedChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(6, 140);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(157, 13);
			this.label3.TabIndex = 24;
			this.label3.Text = "Job printing timeout (in minutes):";
			// 
			// textBoxJobPrintingTimeout
			// 
			this.textBoxJobPrintingTimeout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxJobPrintingTimeout.Location = new System.Drawing.Point(230, 140);
			this.textBoxJobPrintingTimeout.MaxLength = 7;
			this.textBoxJobPrintingTimeout.Name = "textBoxJobPrintingTimeout";
			this.textBoxJobPrintingTimeout.Size = new System.Drawing.Size(150, 20);
			this.textBoxJobPrintingTimeout.TabIndex = 25;
			this.textBoxJobPrintingTimeout.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxJobPrintingTimeout_KeyPress);
			this.textBoxJobPrintingTimeout.Leave += new System.EventHandler(this.textBoxJobPrintingTimeout_Leave);
			// 
			// AppSettingLabel2
			// 
			this.AppSettingLabel2.AutoSize = true;
			this.AppSettingLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AppSettingLabel2.Location = new System.Drawing.Point(6, 115);
			this.AppSettingLabel2.Name = "AppSettingLabel2";
			this.AppSettingLabel2.Size = new System.Drawing.Size(218, 13);
			this.AppSettingLabel2.TabIndex = 1;
			this.AppSettingLabel2.Text = "Remote printing service timeout (in seconds):";
			// 
			// RemotePrintingServiceTimeoutInSecondsTextBox
			// 
			this.RemotePrintingServiceTimeoutInSecondsTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RemotePrintingServiceTimeoutInSecondsTextBox.Location = new System.Drawing.Point(230, 115);
			this.RemotePrintingServiceTimeoutInSecondsTextBox.MaxLength = 7;
			this.RemotePrintingServiceTimeoutInSecondsTextBox.Name = "RemotePrintingServiceTimeoutInSecondsTextBox";
			this.RemotePrintingServiceTimeoutInSecondsTextBox.Size = new System.Drawing.Size(150, 20);
			this.RemotePrintingServiceTimeoutInSecondsTextBox.TabIndex = 2;
			this.RemotePrintingServiceTimeoutInSecondsTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxAppSettingTime_KeyPress);
			// 
			// PrintNudgingGroupBox
			//
			this.PrintNudgingGroupBox.Controls.Add(this.EnableSignalRCheckBox);
			this.PrintNudgingGroupBox.Controls.Add(this.EnablePauseSignalRCheckBox);
			this.PrintNudgingGroupBox.Controls.Add(this.ReconnectionAttemptsTextBox);
			this.PrintNudgingGroupBox.Controls.Add(this.ReconnectionAttemptsLable);
			this.PrintNudgingGroupBox.Controls.Add(this.ReconnectionLimitMinutesTextBox);
			this.PrintNudgingGroupBox.Controls.Add(this.ReconnectionLimitMinutesLabel);
			this.PrintNudgingGroupBox.Controls.Add(this.PauseSignalRMinutesTextBox);
			this.PrintNudgingGroupBox.Controls.Add(this.PauseSignalRMinutesLabel);
			this.PrintNudgingGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PrintNudgingGroupBox.Location = new System.Drawing.Point(8, 236);
			this.PrintNudgingGroupBox.Name = "PrintNudgingGroupBox";
			this.PrintNudgingGroupBox.Size = new System.Drawing.Size(416, 100);
			this.PrintNudgingGroupBox.TabIndex = 4;
			this.PrintNudgingGroupBox.TabStop = false;
			this.PrintNudgingGroupBox.Text = "Print Nudging";
			// 
			// EnableSignalRCheckBox
			// 
			this.EnableSignalRCheckBox.AutoSize = true;
			this.EnableSignalRCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EnableSignalRCheckBox.Location = new System.Drawing.Point(6, 19);
			this.EnableSignalRCheckBox.Name = "EnableSignalRCheckBox";
			this.EnableSignalRCheckBox.Size = new System.Drawing.Size(126, 17);
			this.EnableSignalRCheckBox.TabIndex = 0;
			this.EnableSignalRCheckBox.Text = "Enable Print Nudging";
			this.EnableSignalRCheckBox.UseVisualStyleBackColor = true;
			this.EnableSignalRCheckBox.CheckedChanged += new System.EventHandler(this.EnableSignalRCheckBox_CheckedChanged);
			//
			// EnablePauseSignalRCheckBox
			//
			this.EnablePauseSignalRCheckBox.AutoSize = true;
			this.EnablePauseSignalRCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EnablePauseSignalRCheckBox.Location = new System.Drawing.Point(6, 45);
			this.EnablePauseSignalRCheckBox.Name = "EnableSignalRCheckBox";
			this.EnablePauseSignalRCheckBox.Size = new System.Drawing.Size(100, 17);
			this.EnablePauseSignalRCheckBox.TabIndex = 0;
			this.EnablePauseSignalRCheckBox.Text = "Enable Pause";
			this.EnablePauseSignalRCheckBox.UseVisualStyleBackColor = true;
			this.EnablePauseSignalRCheckBox.CheckedChanged += new System.EventHandler(this.EnablePauseSignalRCheckBox_CheckedChanged);
			// 
			// ConnectionAttemptTextBox
			// 
			this.ReconnectionAttemptsTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ReconnectionAttemptsTextBox.Location = new System.Drawing.Point(6, 70);
			this.ReconnectionAttemptsTextBox.MaxLength = 7;
			this.ReconnectionAttemptsTextBox.Name = "ConnectionAttemptTextBox";
			this.ReconnectionAttemptsTextBox.Size = new System.Drawing.Size(50, 20);
			this.ReconnectionAttemptsTextBox.TabIndex = 2;
			this.ReconnectionAttemptsTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxAppSettingTime_KeyPress);
			// 
			// ConnectionAttemptLable
			// 
			this.ReconnectionAttemptsLable.AutoSize = true;
			this.ReconnectionAttemptsLable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ReconnectionAttemptsLable.Location = new System.Drawing.Point(60, 73);
			this.ReconnectionAttemptsLable.Name = "ConnectionAttemptLable";
			this.ReconnectionAttemptsLable.Size = new System.Drawing.Size(65, 13);
			this.ReconnectionAttemptsLable.TabIndex = 1;
			this.ReconnectionAttemptsLable.Text = "connection attempts in";
			// 
			// ConnectionAttemptMinutesTextBox
			// 
			this.ReconnectionLimitMinutesTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ReconnectionLimitMinutesTextBox.Location = new System.Drawing.Point(173, 70);
			this.ReconnectionLimitMinutesTextBox.MaxLength = 7;
			this.ReconnectionLimitMinutesTextBox.Name = "ConnectionAttemptMinutesTextBox";
			this.ReconnectionLimitMinutesTextBox.Size = new System.Drawing.Size(50, 20);
			this.ReconnectionLimitMinutesTextBox.TabIndex = 2;
			this.ReconnectionLimitMinutesTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxAppSettingTime_KeyPress);
			// 
			// ConnectionAttemptMinutesLabel
			// 
			this.ReconnectionLimitMinutesLabel.AutoSize = true;
			this.ReconnectionLimitMinutesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ReconnectionLimitMinutesLabel.Location = new System.Drawing.Point(223, 73);
			this.ReconnectionLimitMinutesLabel.Name = "ConnectionAttemptMinutesLabel";
			this.ReconnectionLimitMinutesLabel.Size = new System.Drawing.Size(65, 13);
			this.ReconnectionLimitMinutesLabel.TabIndex = 1;
			this.ReconnectionLimitMinutesLabel.Text = "Minutes,";
			// 
			// PauseMinutesTextBox
			// 
			this.PauseSignalRMinutesTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PauseSignalRMinutesTextBox.Location = new System.Drawing.Point(270, 70);
			this.PauseSignalRMinutesTextBox.MaxLength = 7;
			this.PauseSignalRMinutesTextBox.Name = "PauseMinutesTextBox";
			this.PauseSignalRMinutesTextBox.Size = new System.Drawing.Size(50, 20);
			this.PauseSignalRMinutesTextBox.TabIndex = 2;
			this.PauseSignalRMinutesTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxAppSettingTime_KeyPress);
			// 
			// PauseMinutesLabel
			// 
			this.PauseSignalRMinutesLabel.AutoSize = true;
			this.PauseSignalRMinutesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PauseSignalRMinutesLabel.Location = new System.Drawing.Point(320, 73);
			this.PauseSignalRMinutesLabel.Name = "ConnectionAttemptMinutesLabel";
			this.PauseSignalRMinutesLabel.Size = new System.Drawing.Size(65, 13);
			this.PauseSignalRMinutesLabel.TabIndex = 1;
			this.PauseSignalRMinutesLabel.Text = "minutes for pause";
			// 
			// checkBoxEnableVerboseLogging
			// 
			this.checkBoxEnableVerboseLogging.AutoSize = true;
			this.checkBoxEnableVerboseLogging.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxEnableVerboseLogging.Location = new System.Drawing.Point(6, 42);
			this.checkBoxEnableVerboseLogging.Name = "checkBoxEnableVerboseLogging";
			this.checkBoxEnableVerboseLogging.Size = new System.Drawing.Size(142, 17);
			this.checkBoxEnableVerboseLogging.TabIndex = 23;
			this.checkBoxEnableVerboseLogging.Text = "Enable Verbose Logging";
			this.toolTipManager.SetToolTip(this.checkBoxEnableVerboseLogging, "When enabled, will log additional details about printing process.");
			this.checkBoxEnableVerboseLogging.UseVisualStyleBackColor = true;
			// 
			// EnableExpect100ContinueCheckBox
			// 
			this.EnableExpect100ContinueCheckBox.AutoSize = true;
			this.EnableExpect100ContinueCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EnableExpect100ContinueCheckBox.Location = new System.Drawing.Point(6, 19);
			this.EnableExpect100ContinueCheckBox.Name = "EnableExpect100ContinueCheckBox";
			this.EnableExpect100ContinueCheckBox.Size = new System.Drawing.Size(259, 17);
			this.EnableExpect100ContinueCheckBox.TabIndex = 23;
			this.EnableExpect100ContinueCheckBox.Text = "Enable 100-Continue behavior for HTTP requests";
			this.EnableExpect100ContinueCheckBox.UseVisualStyleBackColor = true;
			// 
			// RefreshTimeForNewPrintersScanLabel
			// 
			this.RefreshTimeForNewPrintersScanLabel.AutoSize = true;
			this.RefreshTimeForNewPrintersScanLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RefreshTimeForNewPrintersScanLabel.Location = new System.Drawing.Point(8, 47);
			this.RefreshTimeForNewPrintersScanLabel.Name = "RefreshTimeForNewPrintersScanLabel";
			this.RefreshTimeForNewPrintersScanLabel.Size = new System.Drawing.Size(254, 13);
			this.RefreshTimeForNewPrintersScanLabel.TabIndex = 2;
			this.RefreshTimeForNewPrintersScanLabel.Text = "Pause Between Scan For New Printers (in seconds):";
			// 
			// RefreshTimeForNewPrintersScanTextBox
			// 
			this.RefreshTimeForNewPrintersScanTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RefreshTimeForNewPrintersScanTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RefreshTimeForNewPrintersScanTextBox.Location = new System.Drawing.Point(8, 63);
			this.RefreshTimeForNewPrintersScanTextBox.MaxLength = 7;
			this.RefreshTimeForNewPrintersScanTextBox.Name = "RefreshTimeForNewPrintersScanTextBox";
			this.RefreshTimeForNewPrintersScanTextBox.Size = new System.Drawing.Size(150, 20);
			this.RefreshTimeForNewPrintersScanTextBox.TabIndex = 3;
			this.RefreshTimeForNewPrintersScanTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RefreshTimeForNewPrintersScanTextBox_KeyPress);
			this.RefreshTimeForNewPrintersScanTextBox.Leave += new System.EventHandler(this.RefreshTimeForNewPrintersScanTextBox_Leave);
			// 
			// PauseInSecondsLabel
			// 
			this.PauseInSecondsLabel.AutoSize = true;
			this.PauseInSecondsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PauseInSecondsLabel.Location = new System.Drawing.Point(8, 3);
			this.PauseInSecondsLabel.Name = "PauseInSecondsLabel";
			this.PauseInSecondsLabel.Size = new System.Drawing.Size(213, 13);
			this.PauseInSecondsLabel.TabIndex = 0;
			this.PauseInSecondsLabel.Text = "Pause Between Job Requests (in seconds):";
			// 
			// RequestPauseInSecondsTextBox
			// 
			this.RequestPauseInSecondsTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RequestPauseInSecondsTextBox.Location = new System.Drawing.Point(8, 19);
			this.RequestPauseInSecondsTextBox.MaxLength = 3;
			this.RequestPauseInSecondsTextBox.Name = "RequestPauseInSecondsTextBox";
			this.RequestPauseInSecondsTextBox.Size = new System.Drawing.Size(150, 20);
			this.RequestPauseInSecondsTextBox.TabIndex = 1;
			this.RequestPauseInSecondsTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RequestPauseInSecondsTextBox_KeyPress);
			this.RequestPauseInSecondsTextBox.Leave += new System.EventHandler(this.RequestPauseInSecondsTextBox_Leave);
			// 
			// tabPageUpdate
			// 
			this.tabPageUpdate.Controls.Add(this.CheckUpdateButton);
			this.tabPageUpdate.Controls.Add(this.AppSettingLabel1);
			this.tabPageUpdate.Controls.Add(this.NumberOfLoopsToCheckForUpdateTextBox);
			this.tabPageUpdate.Controls.Add(this.groupBoxNotifications);
			this.tabPageUpdate.Controls.Add(this.groupBoxUpdateDuring);
			this.tabPageUpdate.Controls.Add(this.labelDays);
			this.tabPageUpdate.Controls.Add(this.textBoxUpdateNDays);
			this.tabPageUpdate.Controls.Add(this.checkBoxAutoUpdateAfterNDays);
			this.tabPageUpdate.Controls.Add(this.checkBoxAutoUpdateMinorVersion);
			this.tabPageUpdate.Controls.Add(this.checkBoxAutoUpdateMajorVersion);
			this.tabPageUpdate.Controls.Add(this.groupBoxUpdateMode);
			this.tabPageUpdate.Location = new System.Drawing.Point(4, 22);
			this.tabPageUpdate.Name = "tabPageUpdate";
			this.tabPageUpdate.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageUpdate.Size = new System.Drawing.Size(432, 530);
			this.tabPageUpdate.TabIndex = 2;
			this.tabPageUpdate.Text = "Update Settings";
			// 
			// CheckUpdateButton
			// 
			this.CheckUpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CheckUpdateButton.Location = new System.Drawing.Point(8, 18);
			this.CheckUpdateButton.Name = "CheckUpdateButton";
			this.CheckUpdateButton.Size = new System.Drawing.Size(130, 23);
			this.CheckUpdateButton.TabIndex = 0;
			this.CheckUpdateButton.Text = "Check for Updates";
			this.CheckUpdateButton.UseVisualStyleBackColor = true;
			this.CheckUpdateButton.Click += new System.EventHandler(this.CheckUpdateButton_Click);
			// 
			// AppSettingLabel1
			// 
			this.AppSettingLabel1.AutoSize = true;
			this.AppSettingLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AppSettingLabel1.Location = new System.Drawing.Point(30, 229);
			this.AppSettingLabel1.Name = "AppSettingLabel1";
			this.AppSettingLabel1.Size = new System.Drawing.Size(183, 13);
			this.AppSettingLabel1.TabIndex = 9;
			this.AppSettingLabel1.Text = "Number of loops to check for update:";
			// 
			// NumberOfLoopsToCheckForUpdateTextBox
			// 
			this.NumberOfLoopsToCheckForUpdateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.NumberOfLoopsToCheckForUpdateTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NumberOfLoopsToCheckForUpdateTextBox.Location = new System.Drawing.Point(244, 229);
			this.NumberOfLoopsToCheckForUpdateTextBox.MaxLength = 7;
			this.NumberOfLoopsToCheckForUpdateTextBox.Name = "NumberOfLoopsToCheckForUpdateTextBox";
			this.NumberOfLoopsToCheckForUpdateTextBox.Size = new System.Drawing.Size(150, 20);
			this.NumberOfLoopsToCheckForUpdateTextBox.TabIndex = 10;
			// 
			// groupBoxNotifications
			// 
			this.groupBoxNotifications.Controls.Add(this.checkBoxNotifyBeforeUpdate);
			this.groupBoxNotifications.Controls.Add(this.checkBoxNotifyAfterUpdate);
			this.groupBoxNotifications.Controls.Add(this.checkBoxUpdateNotification);
			this.groupBoxNotifications.Location = new System.Drawing.Point(8, 378);
			this.groupBoxNotifications.Name = "groupBoxNotifications";
			this.groupBoxNotifications.Size = new System.Drawing.Size(416, 106);
			this.groupBoxNotifications.TabIndex = 8;
			this.groupBoxNotifications.TabStop = false;
			this.groupBoxNotifications.Text = "Update Notifications";
			// 
			// checkBoxNotifyBeforeUpdate
			// 
			this.checkBoxNotifyBeforeUpdate.AutoSize = true;
			this.checkBoxNotifyBeforeUpdate.Location = new System.Drawing.Point(11, 51);
			this.checkBoxNotifyBeforeUpdate.Name = "checkBoxNotifyBeforeUpdate";
			this.checkBoxNotifyBeforeUpdate.Size = new System.Drawing.Size(93, 17);
			this.checkBoxNotifyBeforeUpdate.TabIndex = 1;
			this.checkBoxNotifyBeforeUpdate.Text = "Notify on Start";
			this.toolTipManager.SetToolTip(this.checkBoxNotifyBeforeUpdate, "Send email to contacts specified in CW1 registry item\r\nNotification / WebPrint No" +
        "tification Group");
			this.checkBoxNotifyBeforeUpdate.UseVisualStyleBackColor = true;
			// 
			// checkBoxNotifyAfterUpdate
			// 
			this.checkBoxNotifyAfterUpdate.AutoSize = true;
			this.checkBoxNotifyAfterUpdate.Location = new System.Drawing.Point(11, 74);
			this.checkBoxNotifyAfterUpdate.Name = "checkBoxNotifyAfterUpdate";
			this.checkBoxNotifyAfterUpdate.Size = new System.Drawing.Size(98, 17);
			this.checkBoxNotifyAfterUpdate.TabIndex = 2;
			this.checkBoxNotifyAfterUpdate.Text = "Notify on Finish";
			this.toolTipManager.SetToolTip(this.checkBoxNotifyAfterUpdate, "Send email to contacts specified in CW1 registry item\r\nNotification / WebPrint No" +
        "tification Group");
			this.checkBoxNotifyAfterUpdate.UseVisualStyleBackColor = true;
			// 
			// checkBoxUpdateNotification
			// 
			this.checkBoxUpdateNotification.AutoSize = true;
			this.checkBoxUpdateNotification.Location = new System.Drawing.Point(11, 28);
			this.checkBoxUpdateNotification.Name = "checkBoxUpdateNotification";
			this.checkBoxUpdateNotification.Size = new System.Drawing.Size(173, 17);
			this.checkBoxUpdateNotification.TabIndex = 0;
			this.checkBoxUpdateNotification.Text = "Notify when update is available";
			this.toolTipManager.SetToolTip(this.checkBoxUpdateNotification, "Send email to contacts specified in CW1 registry item\r\nNotification / WebPrint No" +
        "tification Group");
			this.checkBoxUpdateNotification.UseVisualStyleBackColor = true;
			// 
			// groupBoxUpdateDuring
			// 
			this.groupBoxUpdateDuring.Controls.Add(this.checkBoxUpdateOnSunday);
			this.groupBoxUpdateDuring.Controls.Add(this.checkBoxUpdateOnSaturday);
			this.groupBoxUpdateDuring.Controls.Add(this.checkBoxUpdateOnFriday);
			this.groupBoxUpdateDuring.Controls.Add(this.checkBoxUpdateOnThursday);
			this.groupBoxUpdateDuring.Controls.Add(this.checkBoxUpdateOnWednesday);
			this.groupBoxUpdateDuring.Controls.Add(this.checkBoxUpdateOnTuesday);
			this.groupBoxUpdateDuring.Controls.Add(this.checkBoxUpdateOnMonday);
			this.groupBoxUpdateDuring.Controls.Add(this.dateTimeUpdateTo);
			this.groupBoxUpdateDuring.Controls.Add(this.dateTimeUpdateFrom);
			this.groupBoxUpdateDuring.Controls.Add(this.labelTo);
			this.groupBoxUpdateDuring.Controls.Add(this.labelFrom);
			this.groupBoxUpdateDuring.Location = new System.Drawing.Point(8, 262);
			this.groupBoxUpdateDuring.Name = "groupBoxUpdateDuring";
			this.groupBoxUpdateDuring.Size = new System.Drawing.Size(416, 110);
			this.groupBoxUpdateDuring.TabIndex = 7;
			this.groupBoxUpdateDuring.TabStop = false;
			this.groupBoxUpdateDuring.Text = "Update Frequency";
			// 
			// checkBoxUpdateOnSunday
			// 
			this.checkBoxUpdateOnSunday.AutoSize = true;
			this.checkBoxUpdateOnSunday.Location = new System.Drawing.Point(194, 77);
			this.checkBoxUpdateOnSunday.Name = "checkBoxUpdateOnSunday";
			this.checkBoxUpdateOnSunday.Size = new System.Drawing.Size(62, 17);
			this.checkBoxUpdateOnSunday.TabIndex = 10;
			this.checkBoxUpdateOnSunday.Text = "Sunday";
			this.checkBoxUpdateOnSunday.UseVisualStyleBackColor = true;
			// 
			// checkBoxUpdateOnSaturday
			// 
			this.checkBoxUpdateOnSaturday.AutoSize = true;
			this.checkBoxUpdateOnSaturday.Location = new System.Drawing.Point(97, 77);
			this.checkBoxUpdateOnSaturday.Name = "checkBoxUpdateOnSaturday";
			this.checkBoxUpdateOnSaturday.Size = new System.Drawing.Size(68, 17);
			this.checkBoxUpdateOnSaturday.TabIndex = 9;
			this.checkBoxUpdateOnSaturday.Text = "Saturday";
			this.checkBoxUpdateOnSaturday.UseVisualStyleBackColor = true;
			// 
			// checkBoxUpdateOnFriday
			// 
			this.checkBoxUpdateOnFriday.AutoSize = true;
			this.checkBoxUpdateOnFriday.Location = new System.Drawing.Point(11, 77);
			this.checkBoxUpdateOnFriday.Name = "checkBoxUpdateOnFriday";
			this.checkBoxUpdateOnFriday.Size = new System.Drawing.Size(54, 17);
			this.checkBoxUpdateOnFriday.TabIndex = 8;
			this.checkBoxUpdateOnFriday.Text = "Friday";
			this.checkBoxUpdateOnFriday.UseVisualStyleBackColor = true;
			// 
			// checkBoxUpdateOnThursday
			// 
			this.checkBoxUpdateOnThursday.AutoSize = true;
			this.checkBoxUpdateOnThursday.Location = new System.Drawing.Point(291, 54);
			this.checkBoxUpdateOnThursday.Name = "checkBoxUpdateOnThursday";
			this.checkBoxUpdateOnThursday.Size = new System.Drawing.Size(70, 17);
			this.checkBoxUpdateOnThursday.TabIndex = 7;
			this.checkBoxUpdateOnThursday.Text = "Thursday";
			this.checkBoxUpdateOnThursday.UseVisualStyleBackColor = true;
			// 
			// checkBoxUpdateOnWednesday
			// 
			this.checkBoxUpdateOnWednesday.AutoSize = true;
			this.checkBoxUpdateOnWednesday.Location = new System.Drawing.Point(194, 54);
			this.checkBoxUpdateOnWednesday.Name = "checkBoxUpdateOnWednesday";
			this.checkBoxUpdateOnWednesday.Size = new System.Drawing.Size(83, 17);
			this.checkBoxUpdateOnWednesday.TabIndex = 6;
			this.checkBoxUpdateOnWednesday.Text = "Wednesday";
			this.checkBoxUpdateOnWednesday.UseVisualStyleBackColor = true;
			// 
			// checkBoxUpdateOnTuesday
			// 
			this.checkBoxUpdateOnTuesday.AutoSize = true;
			this.checkBoxUpdateOnTuesday.Location = new System.Drawing.Point(97, 54);
			this.checkBoxUpdateOnTuesday.Name = "checkBoxUpdateOnTuesday";
			this.checkBoxUpdateOnTuesday.Size = new System.Drawing.Size(67, 17);
			this.checkBoxUpdateOnTuesday.TabIndex = 5;
			this.checkBoxUpdateOnTuesday.Text = "Tuesday";
			this.checkBoxUpdateOnTuesday.UseVisualStyleBackColor = true;
			// 
			// checkBoxUpdateOnMonday
			// 
			this.checkBoxUpdateOnMonday.AutoSize = true;
			this.checkBoxUpdateOnMonday.Location = new System.Drawing.Point(11, 54);
			this.checkBoxUpdateOnMonday.Name = "checkBoxUpdateOnMonday";
			this.checkBoxUpdateOnMonday.Size = new System.Drawing.Size(64, 17);
			this.checkBoxUpdateOnMonday.TabIndex = 4;
			this.checkBoxUpdateOnMonday.Text = "Monday";
			this.checkBoxUpdateOnMonday.UseVisualStyleBackColor = true;
			// 
			// dateTimeUpdateTo
			// 
			this.dateTimeUpdateTo.CustomFormat = "";
			this.dateTimeUpdateTo.Format = System.Windows.Forms.DateTimePickerFormat.Time;
			this.dateTimeUpdateTo.Location = new System.Drawing.Point(237, 28);
			this.dateTimeUpdateTo.Name = "dateTimeUpdateTo";
			this.dateTimeUpdateTo.ShowUpDown = true;
			this.dateTimeUpdateTo.Size = new System.Drawing.Size(104, 20);
			this.dateTimeUpdateTo.TabIndex = 3;
			// 
			// dateTimeUpdateFrom
			// 
			this.dateTimeUpdateFrom.CustomFormat = "";
			this.dateTimeUpdateFrom.Format = System.Windows.Forms.DateTimePickerFormat.Time;
			this.dateTimeUpdateFrom.Location = new System.Drawing.Point(47, 28);
			this.dateTimeUpdateFrom.Name = "dateTimeUpdateFrom";
			this.dateTimeUpdateFrom.ShowUpDown = true;
			this.dateTimeUpdateFrom.Size = new System.Drawing.Size(104, 20);
			this.dateTimeUpdateFrom.TabIndex = 1;
			// 
			// labelTo
			// 
			this.labelTo.AutoSize = true;
			this.labelTo.Location = new System.Drawing.Point(208, 31);
			this.labelTo.Name = "labelTo";
			this.labelTo.Size = new System.Drawing.Size(23, 13);
			this.labelTo.TabIndex = 2;
			this.labelTo.Text = "To:";
			// 
			// labelFrom
			// 
			this.labelFrom.AutoSize = true;
			this.labelFrom.Location = new System.Drawing.Point(8, 31);
			this.labelFrom.Name = "labelFrom";
			this.labelFrom.Size = new System.Drawing.Size(33, 13);
			this.labelFrom.TabIndex = 0;
			this.labelFrom.Text = "From:";
			// 
			// labelDays
			// 
			this.labelDays.AutoSize = true;
			this.labelDays.Location = new System.Drawing.Point(340, 204);
			this.labelDays.Name = "labelDays";
			this.labelDays.Size = new System.Drawing.Size(29, 13);
			this.labelDays.TabIndex = 6;
			this.labelDays.Text = "days";
			// 
			// textBoxUpdateNDays
			// 
			this.textBoxUpdateNDays.Location = new System.Drawing.Point(244, 200);
			this.textBoxUpdateNDays.Name = "textBoxUpdateNDays";
			this.textBoxUpdateNDays.Size = new System.Drawing.Size(87, 20);
			this.textBoxUpdateNDays.TabIndex = 5;
			this.textBoxUpdateNDays.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxUpdateNDays_KeyPress);
			// 
			// checkBoxAutoUpdateAfterNDays
			// 
			this.checkBoxAutoUpdateAfterNDays.AutoSize = true;
			this.checkBoxAutoUpdateAfterNDays.Location = new System.Drawing.Point(14, 203);
			this.checkBoxAutoUpdateAfterNDays.Name = "checkBoxAutoUpdateAfterNDays";
			this.checkBoxAutoUpdateAfterNDays.Size = new System.Drawing.Size(216, 17);
			this.checkBoxAutoUpdateAfterNDays.TabIndex = 4;
			this.checkBoxAutoUpdateAfterNDays.Text = "Automatically update if not updated after";
			this.checkBoxAutoUpdateAfterNDays.UseVisualStyleBackColor = true;
			this.checkBoxAutoUpdateAfterNDays.CheckedChanged += new System.EventHandler(this.checkBoxAutoUpdateAfterNDays_CheckedChanged);
			// 
			// checkBoxAutoUpdateMinorVersion
			// 
			this.checkBoxAutoUpdateMinorVersion.AutoSize = true;
			this.checkBoxAutoUpdateMinorVersion.Location = new System.Drawing.Point(14, 180);
			this.checkBoxAutoUpdateMinorVersion.Name = "checkBoxAutoUpdateMinorVersion";
			this.checkBoxAutoUpdateMinorVersion.Size = new System.Drawing.Size(225, 17);
			this.checkBoxAutoUpdateMinorVersion.TabIndex = 3;
			this.checkBoxAutoUpdateMinorVersion.Text = "Automatically update to new Minor version";
			this.checkBoxAutoUpdateMinorVersion.UseVisualStyleBackColor = true;
			// 
			// checkBoxAutoUpdateMajorVersion
			// 
			this.checkBoxAutoUpdateMajorVersion.AutoSize = true;
			this.checkBoxAutoUpdateMajorVersion.Location = new System.Drawing.Point(14, 157);
			this.checkBoxAutoUpdateMajorVersion.Name = "checkBoxAutoUpdateMajorVersion";
			this.checkBoxAutoUpdateMajorVersion.Size = new System.Drawing.Size(225, 17);
			this.checkBoxAutoUpdateMajorVersion.TabIndex = 2;
			this.checkBoxAutoUpdateMajorVersion.Text = "Automatically update to new Major version";
			this.checkBoxAutoUpdateMajorVersion.UseVisualStyleBackColor = true;
			// 
			// groupBoxUpdateMode
			// 
			this.groupBoxUpdateMode.Controls.Add(this.radioButtonUpdateCustom);
			this.groupBoxUpdateMode.Controls.Add(this.radioButtonUpdateManual);
			this.groupBoxUpdateMode.Controls.Add(this.radioButtonAutoUpdate);
			this.groupBoxUpdateMode.Controls.Add(this.PauseAutoUpdateLabel);
			this.groupBoxUpdateMode.Controls.Add(this.PauseAutoUpdateHoursTextBox);
			this.groupBoxUpdateMode.Controls.Add(this.PauseAutoUpdateHoursLabel);
			this.groupBoxUpdateMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBoxUpdateMode.Location = new System.Drawing.Point(8, 47);
			this.groupBoxUpdateMode.Name = "groupBoxUpdateMode";
			this.groupBoxUpdateMode.Size = new System.Drawing.Size(416, 93);
			this.groupBoxUpdateMode.TabIndex = 1;
			this.groupBoxUpdateMode.TabStop = false;
			this.groupBoxUpdateMode.Text = "Update Schedule";
			// 
			// radioButtonUpdateCustom
			// 
			this.radioButtonUpdateCustom.AutoSize = true;
			this.radioButtonUpdateCustom.Location = new System.Drawing.Point(9, 42);
			this.radioButtonUpdateCustom.Name = "radioButtonUpdateCustom";
			this.radioButtonUpdateCustom.Size = new System.Drawing.Size(60, 17);
			this.radioButtonUpdateCustom.TabIndex = 1;
			this.radioButtonUpdateCustom.TabStop = true;
			this.radioButtonUpdateCustom.Text = "Custom";
			this.radioButtonUpdateCustom.UseVisualStyleBackColor = true;
			this.radioButtonUpdateCustom.CheckedChanged += new System.EventHandler(this.radioButtonAutoUpdate_CheckedChanged);
			// 
			// radioButtonUpdateManual
			// 
			this.radioButtonUpdateManual.AutoSize = true;
			this.radioButtonUpdateManual.Location = new System.Drawing.Point(9, 65);
			this.radioButtonUpdateManual.Name = "radioButtonUpdateManual";
			this.radioButtonUpdateManual.Size = new System.Drawing.Size(66, 17);
			this.radioButtonUpdateManual.TabIndex = 2;
			this.radioButtonUpdateManual.TabStop = true;
			this.radioButtonUpdateManual.Text = "Disabled";
			this.radioButtonUpdateManual.UseVisualStyleBackColor = true;
			this.radioButtonUpdateManual.CheckedChanged += new System.EventHandler(this.radioButtonAutoUpdate_CheckedChanged);
			// 
			// radioButtonAutoUpdate
			// 
			this.radioButtonAutoUpdate.AutoSize = true;
			this.radioButtonAutoUpdate.Location = new System.Drawing.Point(9, 19);
			this.radioButtonAutoUpdate.Name = "radioButtonAutoUpdate";
			this.radioButtonAutoUpdate.Size = new System.Drawing.Size(72, 17);
			this.radioButtonAutoUpdate.TabIndex = 0;
			this.radioButtonAutoUpdate.TabStop = true;
			this.radioButtonAutoUpdate.Text = "Automatic";
			this.radioButtonAutoUpdate.UseVisualStyleBackColor = true;
			this.radioButtonAutoUpdate.CheckedChanged += new System.EventHandler(this.radioButtonAutoUpdate_CheckedChanged);
			// 
			// PauseAutoUpdateLabel
			// 
			this.PauseAutoUpdateLabel.AutoSize = true;
			this.PauseAutoUpdateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PauseAutoUpdateLabel.Location = new System.Drawing.Point(100, 19);
			this.PauseAutoUpdateLabel.Name = "PauseAutoUpdateLabel";
			this.PauseAutoUpdateLabel.Size = new System.Drawing.Size(100, 13);
			this.PauseAutoUpdateLabel.Text = "Pause update on failure";
			// 
			// PauseAutoUpdateTextBox
			// 
			this.PauseAutoUpdateHoursTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PauseAutoUpdateHoursTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PauseAutoUpdateHoursTextBox.Location = new System.Drawing.Point(235, 15);
			this.PauseAutoUpdateHoursTextBox.MaxLength = 7;
			this.PauseAutoUpdateHoursTextBox.Name = "PauseAutoUpdateTextBox";
			this.PauseAutoUpdateHoursTextBox.Size = new System.Drawing.Size(87, 20);
			this.PauseAutoUpdateHoursTextBox.TabIndex = 1;
			// 
			// PauseAutoUpdateHoursLabel
			// 
			this.PauseAutoUpdateHoursLabel.AutoSize = true;
			this.PauseAutoUpdateHoursLabel.Location = new System.Drawing.Point(330, 19);
			this.PauseAutoUpdateHoursLabel.Name = "PauseAutoUpdateHoursLabel";
			this.PauseAutoUpdateHoursLabel.Size = new System.Drawing.Size(29, 13);
			this.PauseAutoUpdateHoursLabel.Text = "hours";
			// 
			// LogManagerTabPage
			// 
			this.LogManagerTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.LogManagerTabPage.Controls.Add(this.OpenLogDirectoryButton);
			this.LogManagerTabPage.Controls.Add(this.LogFileDirectoryLabel);
			this.LogManagerTabPage.Controls.Add(this.LogPathTextBox);
			this.LogManagerTabPage.Controls.Add(this.LogSettingsGroupBox);
			this.LogManagerTabPage.Location = new System.Drawing.Point(4, 22);
			this.LogManagerTabPage.Name = "LogManagerTabPage";
			this.LogManagerTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.LogManagerTabPage.Size = new System.Drawing.Size(432, 530);
			this.LogManagerTabPage.TabIndex = 3;
			this.LogManagerTabPage.Text = "Log Settings";
			// 
			// OpenLogDirectoryButton
			// 
			this.OpenLogDirectoryButton.Location = new System.Drawing.Point(347, 206);
			this.OpenLogDirectoryButton.Name = "OpenLogDirectoryButton";
			this.OpenLogDirectoryButton.Size = new System.Drawing.Size(72, 21);
			this.OpenLogDirectoryButton.TabIndex = 4;
			this.OpenLogDirectoryButton.Text = "Open";
			this.OpenLogDirectoryButton.UseVisualStyleBackColor = true;
			this.OpenLogDirectoryButton.Click += new System.EventHandler(this.OpenLogDirectoryButton_Click);
			// 
			// LogFileDirectoryLabel
			// 
			this.LogFileDirectoryLabel.AutoSize = true;
			this.LogFileDirectoryLabel.Location = new System.Drawing.Point(8, 182);
			this.LogFileDirectoryLabel.Name = "LogFileDirectoryLabel";
			this.LogFileDirectoryLabel.Size = new System.Drawing.Size(92, 13);
			this.LogFileDirectoryLabel.TabIndex = 2;
			this.LogFileDirectoryLabel.Text = "Log File Directory:";
			// 
			// LogPathTextBox
			// 
			this.LogPathTextBox.Location = new System.Drawing.Point(11, 207);
			this.LogPathTextBox.Name = "LogPathTextBox";
			this.LogPathTextBox.ReadOnly = true;
			this.LogPathTextBox.Size = new System.Drawing.Size(333, 20);
			this.LogPathTextBox.TabIndex = 3;
			// 
			// LogSettingsGroupBox
			// 
			this.LogSettingsGroupBox.AutoSize = true;
			this.LogSettingsGroupBox.Controls.Add(this.EnableCleaningOldLogCheckBox);
			this.LogSettingsGroupBox.Controls.Add(this.DayToKeepAliveLabel);
			this.LogSettingsGroupBox.Controls.Add(this.LogFileDayToKeepTextBox);
			this.LogSettingsGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LogSettingsGroupBox.Location = new System.Drawing.Point(11, 23);
			this.LogSettingsGroupBox.Name = "LogSettingsGroupBox";
			this.LogSettingsGroupBox.Size = new System.Drawing.Size(408, 142);
			this.LogSettingsGroupBox.TabIndex = 1;
			this.LogSettingsGroupBox.TabStop = false;
			this.LogSettingsGroupBox.Text = "Cleaning old log files";
			// 
			// EnableCleaningOldLogCheckBox
			// 
			this.EnableCleaningOldLogCheckBox.AutoSize = true;
			this.EnableCleaningOldLogCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EnableCleaningOldLogCheckBox.Location = new System.Drawing.Point(15, 39);
			this.EnableCleaningOldLogCheckBox.Name = "EnableCleaningOldLogCheckBox";
			this.EnableCleaningOldLogCheckBox.Size = new System.Drawing.Size(169, 17);
			this.EnableCleaningOldLogCheckBox.TabIndex = 0;
			this.EnableCleaningOldLogCheckBox.Text = "Enable cleaning of old log files";
			this.EnableCleaningOldLogCheckBox.UseVisualStyleBackColor = true;
			this.EnableCleaningOldLogCheckBox.CheckedChanged += new System.EventHandler(this.EnableCleaningOldLogCheckBox_CheckedChanged);
			// 
			// DayToKeepAliveLabel
			// 
			this.DayToKeepAliveLabel.AutoSize = true;
			this.DayToKeepAliveLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DayToKeepAliveLabel.Location = new System.Drawing.Point(14, 79);
			this.DayToKeepAliveLabel.Name = "DayToKeepAliveLabel";
			this.DayToKeepAliveLabel.Size = new System.Drawing.Size(161, 13);
			this.DayToKeepAliveLabel.TabIndex = 0;
			this.DayToKeepAliveLabel.Text = "Number of days to keep log files:";
			// 
			// LogFileDayToKeepTextBox
			// 
			this.LogFileDayToKeepTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LogFileDayToKeepTextBox.Location = new System.Drawing.Point(214, 76);
			this.LogFileDayToKeepTextBox.MaxLength = 2;
			this.LogFileDayToKeepTextBox.Name = "LogFileDayToKeepTextBox";
			this.LogFileDayToKeepTextBox.Size = new System.Drawing.Size(65, 20);
			this.LogFileDayToKeepTextBox.TabIndex = 1;
			this.LogFileDayToKeepTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.LogFileDayToKeepTextBox_KeyPress);
			// 
			// ServiceTabPage
			// 
			this.ServiceTabPage.Controls.Add(this.ServiceSettingsGroupBox);
			this.ServiceTabPage.Controls.Add(this.InstallServiceButton);
			this.ServiceTabPage.Controls.Add(this.UninstallServiceButton);
			this.ServiceTabPage.Controls.Add(this.StartServiceButton);
			this.ServiceTabPage.Controls.Add(this.StopServiceButton);
			this.ServiceTabPage.Location = new System.Drawing.Point(4, 22);
			this.ServiceTabPage.Name = "ServiceTabPage";
			this.ServiceTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.ServiceTabPage.Size = new System.Drawing.Size(432, 530);
			this.ServiceTabPage.TabIndex = 3;
			this.ServiceTabPage.Text = "Service";
			// 
			// ServiceSettingsGroupBox
			// 
			this.ServiceSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ServiceSettingsGroupBox.Controls.Add(this.ServiceStatusTextBox);
			this.ServiceSettingsGroupBox.Controls.Add(this.ServiceConfigNameTextBox);
			this.ServiceSettingsGroupBox.Controls.Add(this.ServiceStartModeLabel);
			this.ServiceSettingsGroupBox.Controls.Add(this.ServiceConfigNameLabel);
			this.ServiceSettingsGroupBox.Controls.Add(this.ServiceDisplayNameLabel);
			this.ServiceSettingsGroupBox.Controls.Add(this.ServiceDisplayNameTextBox);
			this.ServiceSettingsGroupBox.Controls.Add(this.ServiceNameLabel);
			this.ServiceSettingsGroupBox.Controls.Add(this.CheckServiceStatus);
			this.ServiceSettingsGroupBox.Controls.Add(this.ServiceInstallationNameTextBox);
			this.ServiceSettingsGroupBox.Controls.Add(this.ServiceStartModeComboBox);
			this.ServiceSettingsGroupBox.Controls.Add(this.ServiceStatusLabel);
			this.ServiceSettingsGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ServiceSettingsGroupBox.Location = new System.Drawing.Point(3, 3);
			this.ServiceSettingsGroupBox.Name = "ServiceSettingsGroupBox";
			this.ServiceSettingsGroupBox.Size = new System.Drawing.Size(423, 308);
			this.ServiceSettingsGroupBox.TabIndex = 0;
			this.ServiceSettingsGroupBox.TabStop = false;
			this.ServiceSettingsGroupBox.Text = "Service Settings";
			// 
			// ServiceStatusTextBox
			// 
			this.ServiceStatusTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ServiceStatusTextBox.Location = new System.Drawing.Point(132, 256);
			this.ServiceStatusTextBox.Name = "ServiceStatusTextBox";
			this.ServiceStatusTextBox.ReadOnly = true;
			this.ServiceStatusTextBox.Size = new System.Drawing.Size(168, 20);
			this.ServiceStatusTextBox.TabIndex = 29;
			// 
			// ServiceConfigNameTextBox
			// 
			this.ServiceConfigNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ServiceConfigNameTextBox.Location = new System.Drawing.Point(9, 100);
			this.ServiceConfigNameTextBox.Name = "ServiceConfigNameTextBox";
			this.ServiceConfigNameTextBox.ReadOnly = true;
			this.ServiceConfigNameTextBox.Size = new System.Drawing.Size(407, 20);
			this.ServiceConfigNameTextBox.TabIndex = 36;
			// 
			// ServiceStartModeLabel
			// 
			this.ServiceStartModeLabel.AutoSize = true;
			this.ServiceStartModeLabel.BackColor = System.Drawing.Color.Transparent;
			this.ServiceStartModeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ServiceStartModeLabel.Location = new System.Drawing.Point(5, 211);
			this.ServiceStartModeLabel.Name = "ServiceStartModeLabel";
			this.ServiceStartModeLabel.Size = new System.Drawing.Size(98, 13);
			this.ServiceStartModeLabel.TabIndex = 37;
			this.ServiceStartModeLabel.Text = "Service start mode:";
			// 
			// ServiceConfigNameLabel
			// 
			this.ServiceConfigNameLabel.AutoSize = true;
			this.ServiceConfigNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ServiceConfigNameLabel.Location = new System.Drawing.Point(6, 84);
			this.ServiceConfigNameLabel.Name = "ServiceConfigNameLabel";
			this.ServiceConfigNameLabel.Size = new System.Drawing.Size(103, 13);
			this.ServiceConfigNameLabel.TabIndex = 32;
			this.ServiceConfigNameLabel.Text = "Configuration Name:";
			// 
			// ServiceDisplayNameLabel
			// 
			this.ServiceDisplayNameLabel.AutoSize = true;
			this.ServiceDisplayNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ServiceDisplayNameLabel.Location = new System.Drawing.Point(5, 139);
			this.ServiceDisplayNameLabel.Name = "ServiceDisplayNameLabel";
			this.ServiceDisplayNameLabel.Size = new System.Drawing.Size(114, 13);
			this.ServiceDisplayNameLabel.TabIndex = 28;
			this.ServiceDisplayNameLabel.Text = "Service Display Name:";
			// 
			// ServiceDisplayNameTextBox
			// 
			this.ServiceDisplayNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ServiceDisplayNameTextBox.Location = new System.Drawing.Point(8, 155);
			this.ServiceDisplayNameTextBox.Name = "ServiceDisplayNameTextBox";
			this.ServiceDisplayNameTextBox.ReadOnly = true;
			this.ServiceDisplayNameTextBox.Size = new System.Drawing.Size(408, 20);
			this.ServiceDisplayNameTextBox.TabIndex = 26;
			// 
			// ServiceNameLabel
			// 
			this.ServiceNameLabel.AutoSize = true;
			this.ServiceNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ServiceNameLabel.Location = new System.Drawing.Point(6, 31);
			this.ServiceNameLabel.Name = "ServiceNameLabel";
			this.ServiceNameLabel.Size = new System.Drawing.Size(77, 13);
			this.ServiceNameLabel.TabIndex = 27;
			this.ServiceNameLabel.Text = "Service Name:";
			// 
			// CheckServiceStatus
			// 
			this.CheckServiceStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CheckServiceStatus.Location = new System.Drawing.Point(315, 254);
			this.CheckServiceStatus.Name = "CheckServiceStatus";
			this.CheckServiceStatus.Size = new System.Drawing.Size(101, 23);
			this.CheckServiceStatus.TabIndex = 30;
			this.CheckServiceStatus.Text = "Check Status";
			this.CheckServiceStatus.UseVisualStyleBackColor = true;
			this.CheckServiceStatus.Click += new System.EventHandler(this.CheckServiceStatusButton_Click);
			// 
			// ServiceInstallationNameTextBox
			// 
			this.ServiceInstallationNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ServiceInstallationNameTextBox.Location = new System.Drawing.Point(8, 47);
			this.ServiceInstallationNameTextBox.Name = "ServiceInstallationNameTextBox";
			this.ServiceInstallationNameTextBox.ReadOnly = true;
			this.ServiceInstallationNameTextBox.Size = new System.Drawing.Size(408, 20);
			this.ServiceInstallationNameTextBox.TabIndex = 25;
			this.ServiceInstallationNameTextBox.TabStop = false;
			// 
			// ServiceStartModeComboBox
			// 
			this.ServiceStartModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ServiceStartModeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ServiceStartModeComboBox.FormattingEnabled = true;
			this.ServiceStartModeComboBox.Location = new System.Drawing.Point(132, 208);
			this.ServiceStartModeComboBox.MaxDropDownItems = 2;
			this.ServiceStartModeComboBox.Name = "ServiceStartModeComboBox";
			this.ServiceStartModeComboBox.Size = new System.Drawing.Size(168, 21);
			this.ServiceStartModeComboBox.TabIndex = 31;
			// 
			// ServiceStatusLabel
			// 
			this.ServiceStatusLabel.AutoSize = true;
			this.ServiceStatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ServiceStatusLabel.Location = new System.Drawing.Point(6, 259);
			this.ServiceStatusLabel.Name = "ServiceStatusLabel";
			this.ServiceStatusLabel.Size = new System.Drawing.Size(77, 13);
			this.ServiceStatusLabel.TabIndex = 33;
			this.ServiceStatusLabel.Text = "Service status:";
			// 
			// InstallServiceButton
			// 
			this.InstallServiceButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.InstallServiceButton.Location = new System.Drawing.Point(8, 341);
			this.InstallServiceButton.Name = "InstallServiceButton";
			this.InstallServiceButton.Size = new System.Drawing.Size(89, 23);
			this.InstallServiceButton.TabIndex = 23;
			this.InstallServiceButton.Text = "Install";
			this.InstallServiceButton.UseVisualStyleBackColor = true;
			this.InstallServiceButton.Click += new System.EventHandler(this.InstallServiceButton_Click);
			// 
			// UninstallServiceButton
			// 
			this.UninstallServiceButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.UninstallServiceButton.Location = new System.Drawing.Point(319, 341);
			this.UninstallServiceButton.Name = "UninstallServiceButton";
			this.UninstallServiceButton.Size = new System.Drawing.Size(101, 23);
			this.UninstallServiceButton.TabIndex = 24;
			this.UninstallServiceButton.Text = "Uninstall";
			this.UninstallServiceButton.UseVisualStyleBackColor = true;
			this.UninstallServiceButton.Click += new System.EventHandler(this.UninstallServiceButton_Click);
			// 
			// StartServiceButton
			// 
			this.StartServiceButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StartServiceButton.Location = new System.Drawing.Point(113, 341);
			this.StartServiceButton.Name = "StartServiceButton";
			this.StartServiceButton.Size = new System.Drawing.Size(89, 23);
			this.StartServiceButton.TabIndex = 34;
			this.StartServiceButton.Text = "Start";
			this.StartServiceButton.UseVisualStyleBackColor = true;
			this.StartServiceButton.Click += new System.EventHandler(this.StartServiceButton_Click);
			// 
			// StopServiceButton
			// 
			this.StopServiceButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.StopServiceButton.Location = new System.Drawing.Point(218, 341);
			this.StopServiceButton.Name = "StopServiceButton";
			this.StopServiceButton.Size = new System.Drawing.Size(86, 23);
			this.StopServiceButton.TabIndex = 35;
			this.StopServiceButton.Text = "Stop";
			this.StopServiceButton.UseVisualStyleBackColor = true;
			this.StopServiceButton.Click += new System.EventHandler(this.StopServiceButton_Click);
			// 
			// DeleteConfigurationButton
			// 
			this.DeleteConfigurationButton.Image = ((System.Drawing.Image)(resources.GetObject("DeleteConfigurationButton.Image")));
			this.DeleteConfigurationButton.Location = new System.Drawing.Point(401, 3);
			this.DeleteConfigurationButton.Name = "DeleteConfigurationButton";
			this.DeleteConfigurationButton.Size = new System.Drawing.Size(22, 23);
			this.DeleteConfigurationButton.TabIndex = 5;
			this.toolTipManager.SetToolTip(this.DeleteConfigurationButton, "Delete this configuration");
			this.DeleteConfigurationButton.UseVisualStyleBackColor = true;
			this.DeleteConfigurationButton.Click += new System.EventHandler(this.DeleteConfigurationButton_Click);
			// 
			// DuplicateConfigurationButton
			// 
			this.DuplicateConfigurationButton.Image = ((System.Drawing.Image)(resources.GetObject("DuplicateConfigurationButton.Image")));
			this.DuplicateConfigurationButton.Location = new System.Drawing.Point(376, 3);
			this.DuplicateConfigurationButton.Name = "DuplicateConfigurationButton";
			this.DuplicateConfigurationButton.Size = new System.Drawing.Size(22, 23);
			this.DuplicateConfigurationButton.TabIndex = 4;
			this.toolTipManager.SetToolTip(this.DuplicateConfigurationButton, "Duplicate this configuration");
			this.DuplicateConfigurationButton.UseVisualStyleBackColor = true;
			this.DuplicateConfigurationButton.Click += new System.EventHandler(this.DuplicateConfigurationButton_Click);
			// 
			// RenameConfigurationButton
			// 
			this.RenameConfigurationButton.Image = ((System.Drawing.Image)(resources.GetObject("RenameConfigurationButton.Image")));
			this.RenameConfigurationButton.Location = new System.Drawing.Point(351, 3);
			this.RenameConfigurationButton.Name = "RenameConfigurationButton";
			this.RenameConfigurationButton.Size = new System.Drawing.Size(22, 23);
			this.RenameConfigurationButton.TabIndex = 3;
			this.toolTipManager.SetToolTip(this.RenameConfigurationButton, "Rename this configuration");
			this.RenameConfigurationButton.UseVisualStyleBackColor = true;
			this.RenameConfigurationButton.Click += new System.EventHandler(this.RenameConfigurationButton_Click);
			// 
			// NewConfigurationButton
			// 
			this.NewConfigurationButton.Image = ((System.Drawing.Image)(resources.GetObject("NewConfigurationButton.Image")));
			this.NewConfigurationButton.Location = new System.Drawing.Point(326, 3);
			this.NewConfigurationButton.Name = "NewConfigurationButton";
			this.NewConfigurationButton.Size = new System.Drawing.Size(22, 23);
			this.NewConfigurationButton.TabIndex = 2;
			this.toolTipManager.SetToolTip(this.NewConfigurationButton, "Add a new configuration");
			this.NewConfigurationButton.UseVisualStyleBackColor = true;
			this.NewConfigurationButton.Click += new System.EventHandler(this.NewConfigurationButton_Click);
			// 
			// ConfigurationsLabel
			// 
			this.ConfigurationsLabel.AutoSize = true;
			this.ConfigurationsLabel.Location = new System.Drawing.Point(12, 9);
			this.ConfigurationsLabel.Name = "ConfigurationsLabel";
			this.ConfigurationsLabel.Size = new System.Drawing.Size(103, 13);
			this.ConfigurationsLabel.TabIndex = 0;
			this.ConfigurationsLabel.Text = "Configuration Name:";
			// 
			// ConfigurationsComboBox
			// 
			this.ConfigurationsComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ConfigurationsComboBox.FormattingEnabled = true;
			this.ConfigurationsComboBox.Location = new System.Drawing.Point(139, 4);
			this.ConfigurationsComboBox.MaxDropDownItems = 10;
			this.ConfigurationsComboBox.Name = "ConfigurationsComboBox";
			this.ConfigurationsComboBox.Size = new System.Drawing.Size(180, 21);
			this.ConfigurationsComboBox.TabIndex = 1;
			this.ConfigurationsComboBox.SelectedIndexChanged += new System.EventHandler(this.ConfigurationsComboBox_SelectedIndexChanged);
			this.ConfigurationsComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ConfigurationsComboBox_KeyPress);
			// 
			// ConfigForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(440, 658);
			this.Controls.Add(this.WebPrintClientTabControl);
			this.Controls.Add(this.DeleteConfigurationButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.DuplicateConfigurationButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.RenameConfigurationButton);
			this.Controls.Add(this.DialogStatusStrip);
			this.Controls.Add(this.NewConfigurationButton);
			this.Controls.Add(this.ConfigurationsLabel);
			this.Controls.Add(this.ConfigurationsComboBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ConfigForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "CargoWise One WebPrint Client Configuration";
			this.Load += new System.EventHandler(this.ConfigForm_Load);
			this.DialogStatusStrip.ResumeLayout(false);
			this.DialogStatusStrip.PerformLayout();
			this.WebPrintClientTabControl.ResumeLayout(false);
			this.ConnectionConfigTabPage.ResumeLayout(false);
			this.ConnectionConfigPanel.ResumeLayout(false);
			this.ConnectionConfigGroupBox.ResumeLayout(false);
			this.ConnectionConfigGroupBox.PerformLayout();
			this.ProxyGroupBox.ResumeLayout(false);
			this.ProxyGroupBox.PerformLayout();
			this.ProxyUseDefaultSystemSettingsGroupBox.ResumeLayout(false);
			this.ProxyUseDefaultSystemSettingsGroupBox.PerformLayout();
			this.GeneralSettingsConfigTabPage.ResumeLayout(false);
			this.GeneralSettingsConfigTabPage.PerformLayout();
			this.groupBoxKeepAlive.ResumeLayout(false);
			this.groupBoxKeepAlive.PerformLayout();
			this.PrintNudgingGroupBox.ResumeLayout(false);
			this.PrintNudgingGroupBox.PerformLayout();
			this.groupBoxAppSetting.ResumeLayout(false);
			this.groupBoxAppSetting.PerformLayout();
			this.tabPageUpdate.ResumeLayout(false);
			this.tabPageUpdate.PerformLayout();
			this.groupBoxNotifications.ResumeLayout(false);
			this.groupBoxNotifications.PerformLayout();
			this.groupBoxUpdateDuring.ResumeLayout(false);
			this.groupBoxUpdateDuring.PerformLayout();
			this.groupBoxUpdateMode.ResumeLayout(false);
			this.groupBoxUpdateMode.PerformLayout();
			this.LogManagerTabPage.ResumeLayout(false);
			this.LogManagerTabPage.PerformLayout();
			this.LogSettingsGroupBox.ResumeLayout(false);
			this.LogSettingsGroupBox.PerformLayout();
			this.ServiceTabPage.ResumeLayout(false);
			this.ServiceSettingsGroupBox.ResumeLayout(false);
			this.ServiceSettingsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip DialogStatusStrip;
		protected System.Windows.Forms.ToolStripStatusLabel StatusLabel;
		protected System.Windows.Forms.Button CloseButton;
		protected System.Windows.Forms.Button SaveButton;
		private System.Windows.Forms.TabControl WebPrintClientTabControl;
		private System.Windows.Forms.TabPage ConnectionConfigTabPage;
		private System.Windows.Forms.Panel ConnectionConfigPanel;
		private System.Windows.Forms.Button DeleteConfigurationButton;
		private System.Windows.Forms.Button DuplicateConfigurationButton;
		private System.Windows.Forms.Button RenameConfigurationButton;
		private System.Windows.Forms.Button NewConfigurationButton;
		private System.Windows.Forms.Label ConfigurationsLabel;
		protected System.Windows.Forms.ComboBox ConfigurationsComboBox;
		private System.Windows.Forms.Button CheckUpdateButton;
		private ToolTip toolTipManager;
		private System.Windows.Forms.TabPage GeneralSettingsConfigTabPage;
		private System.Windows.Forms.TabPage ServiceTabPage;
		private System.Windows.Forms.Label RefreshTimeForNewPrintersScanLabel;
		protected System.Windows.Forms.TextBox RefreshTimeForNewPrintersScanTextBox;
		private System.Windows.Forms.Label PauseInSecondsLabel;
		protected System.Windows.Forms.TextBox RequestPauseInSecondsTextBox;
		private System.Windows.Forms.GroupBox ConnectionConfigGroupBox;
		private System.Windows.Forms.GroupBox ProxyGroupBox;
		private System.Windows.Forms.GroupBox ServiceSettingsGroupBox;
		private System.Windows.Forms.GroupBox ProxyUseDefaultSystemSettingsGroupBox;
		private System.Windows.Forms.TextBox DefaultProxyPortTextBox;
		private System.Windows.Forms.CheckBox ProxyUseDefaultSystemSettingsCheckBox;
		private System.Windows.Forms.Label DefaultProxyPortLabel;
		private System.Windows.Forms.TextBox DefaultProxyAddressTextBox;
		private System.Windows.Forms.Label DefaultProxyAddressLabel;
		private System.Windows.Forms.TextBox ProxyPortTextBox;
		private System.Windows.Forms.Label ProxyPortLabel;
		private System.Windows.Forms.CheckBox ProxyEnabledCheckBox;
		private System.Windows.Forms.Label ProxyPwdLabel;
		protected System.Windows.Forms.TextBox ProxyPwdTextBox;
		private System.Windows.Forms.Label ProxyUserLabel;
		private System.Windows.Forms.TextBox ProxyUserTextBox;
		private System.Windows.Forms.Label ProxyAddressLabel;
		private System.Windows.Forms.TextBox ProxyAddressTextBox;
		private System.Windows.Forms.Label LocalMachineNameLabel;
		private System.Windows.Forms.Button TestConnectionButton;
		private System.Windows.Forms.TextBox LocalMachineNameTextBox;
		private System.Windows.Forms.Label WebServicePwdLabel;
		protected System.Windows.Forms.TextBox WebServicePwdTextBox;
		private System.Windows.Forms.Label WebServiceUserLabel;
		private System.Windows.Forms.TextBox WebServiceUserTextBox;
		private System.Windows.Forms.Label WebServiceUrlLabel;
		private System.Windows.Forms.TextBox WebServiceUrlTextBox;
		private GroupBox groupBoxKeepAlive;
		private Label label1;
		protected TextBox textBoxKeepAliveInterval;
		private Label label2;
		private TextBox textBoxKeepAliveTime;
		private CheckBox checkBoxKeepAlive;
		private CheckBox EnableExpect100ContinueCheckBox;
		private GroupBox groupBoxAppSetting;
		private Label AppSettingLabel2;
		private TextBox RemotePrintingServiceTimeoutInSecondsTextBox;
		private GroupBox PrintNudgingGroupBox;
		protected CheckBox EnableSignalRCheckBox;
		protected CheckBox EnablePauseSignalRCheckBox;
		protected TextBox ReconnectionAttemptsTextBox;
		private Label ReconnectionAttemptsLable;
		protected TextBox ReconnectionLimitMinutesTextBox;
		private Label ReconnectionLimitMinutesLabel;
		protected TextBox PauseSignalRMinutesTextBox;
		private Label PauseSignalRMinutesLabel;
		private TabPage tabPageUpdate;
		private GroupBox groupBoxUpdateMode;
		protected RadioButton radioButtonAutoUpdate;
		private Label PauseAutoUpdateLabel;
		private Label PauseAutoUpdateHoursLabel;
		protected TextBox PauseAutoUpdateHoursTextBox;
		private GroupBox groupBoxUpdateDuring;
		private Label labelDays;
		private TextBox textBoxUpdateNDays;
		private CheckBox checkBoxAutoUpdateAfterNDays;
		private CheckBox checkBoxAutoUpdateMinorVersion;
		private CheckBox checkBoxAutoUpdateMajorVersion;
		private CheckBox checkBoxUpdateNotification;
		protected RadioButton radioButtonUpdateCustom;
		protected RadioButton radioButtonUpdateManual;
		private CheckBox checkBoxNotifyAfterUpdate;
		private CheckBox checkBoxNotifyBeforeUpdate;
		private CheckBox checkBoxUpdateOnSunday;
		private CheckBox checkBoxUpdateOnSaturday;
		private CheckBox checkBoxUpdateOnFriday;
		private CheckBox checkBoxUpdateOnThursday;
		private CheckBox checkBoxUpdateOnWednesday;
		private CheckBox checkBoxUpdateOnTuesday;
		private CheckBox checkBoxUpdateOnMonday;
		private DateTimePicker dateTimeUpdateTo;
		private DateTimePicker dateTimeUpdateFrom;
		private Label labelTo;
		private Label labelFrom;
		private GroupBox groupBoxNotifications;
		private Label ConnectionRetryAttemptsLabel;
		private Label RetryDelayLabel;
		private TextBox ConnectionRetryAttemptsTextBox;
		private TextBox RetryDelayTextBox;
		private CheckBox checkBoxEnableVerboseLogging;
		private Label label3;
		private TextBox textBoxJobPrintingTimeout;
		private TabPage LogManagerTabPage;
		private CheckBox EnableCleaningOldLogCheckBox;
		private GroupBox LogSettingsGroupBox;
		private Label DayToKeepAliveLabel;
		private Button OpenLogDirectoryButton;
		private TextBox LogPathTextBox;
		private Label LogFileDirectoryLabel;
		protected TextBox LogFileDayToKeepTextBox;
		private Label AppSettingLabel1;
		protected TextBox NumberOfLoopsToCheckForUpdateTextBox;
		private CheckBox EnableMemoryUsageMonitoring;
		private TextBox MemoryUsageMonitoringTextBox;
		private Label label4;
		protected TextBox ServiceStatusTextBox;
		private TextBox ServiceConfigNameTextBox;
		private Label ServiceStartModeLabel;
		private Label ServiceConfigNameLabel;
		private Label ServiceDisplayNameLabel;
		private Button StopServiceButton;
		private TextBox ServiceDisplayNameTextBox;
		private Button StartServiceButton;
		private Label ServiceNameLabel;
		private Button CheckServiceStatus;
		private TextBox ServiceInstallationNameTextBox;
		protected ComboBox ServiceStartModeComboBox;
		private Label ServiceStatusLabel;
		private Button UninstallServiceButton;
		private Button InstallServiceButton;
	}
}
