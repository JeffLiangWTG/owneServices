namespace Enterprise.Customs.IT.GUI.Registry.AccountsManagement
{
	partial class AccountManagementUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AccountsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
			this.ITCertificatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CertAccountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertForLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ExciseNumbersPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EmcsNotificationEnabled = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExciseNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExciseNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AccountDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AccountDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AccountsGrid)).BeginInit();
			this.AccountsGrid.SuspendLayout();
			this.CertificateLoaderUserControl.SuspendLayout();
			this.ITCertificatePanel.SuspendLayout();
			this.ExciseNumbersPanel.SuspendLayout();
			this.ExciseNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExciseNumbersGrid)).BeginInit();
			this.ExciseNumbersGrid.SuspendLayout();
			this.AccountDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AccountDetailsGrid)).BeginInit();
			this.AccountDetailsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Registry.AccountCollection);
			// 
			// AccountsGrid
			// 
			this.AccountsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AccountsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IT.Registry.Account)(null)))));
			this.AccountsGrid.CaptionVisible = false;
			this.AccountsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccountsGrid.GridId = "7D5C1C60-EFF6-4300-9734-E407649429CE";
			this.AccountsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AccountsGrid.LayoutKey = "AccountsGrid";
			this.AccountsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AccountsGrid.Name = "AccountsGrid";
			this.AccountsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 132, true);
			this.AccountsGrid.TabIndex = 1;
			this.AccountsGrid.AfterBind += new System.EventHandler(this.AccountsGrid_AfterBind);
			this.AccountsGrid.CurrentCellChanged += new System.EventHandler(this.AccountsGrid_CurrentCellChanged);
			this.AccountsGrid.Leave += new System.EventHandler(this.AccountsGrid_Leave);
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "AccountCertificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Registry.Account)(null)).AccountCertificate)));
			this.CertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = null;
			this.CertificateLoaderUserControl.FileDataAsString = "";
			this.CertificateLoaderUserControl.FileDialogTitle = "";
			this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
			this.CertificateLoaderUserControl.InitialDirectory = "";
			this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 7, true);
			this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
			this.CertificateLoaderUserControl.ReadOnly = false;
			this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateLoaderUserControl.TabIndex = 0;
			// 
			// ITCertificatePanel
			// 
			this.ITCertificatePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ITCertificatePanel.Controls.Add(this.CertificateLoaderUserControl);
			this.ITCertificatePanel.Controls.Add(this.CertAccountTextBox);
			this.ITCertificatePanel.Controls.Add(this.CertForLabel);
			this.ITCertificatePanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ITCertificatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 132, true);
			this.ITCertificatePanel.Name = "ITCertificatePanel";
			this.ITCertificatePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(58, 7, 58, 7, true);
			this.ITCertificatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 40, true);
			this.ITCertificatePanel.TabIndex = 14;
			// 
			// CertAccountTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertAccountTextBox, "AccountNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Registry.Account)(null)).AccountNumber)));
			this.CertAccountTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("AccountManagementUserControl|51c629d4-a890-4fa6-abe4-e8d9d5ea906f", "Next Password");
			this.CertAccountTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertAccountTextBox.Enabled = false;
			this.CertAccountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 10, true);
			this.CertAccountTextBox.Name = "CertAccountTextBox";
			this.CertAccountTextBox.ReadOnly = true;
			this.CertAccountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.CertAccountTextBox.TabIndex = 7;
			this.CertAccountTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// CertForLabel
			// 
			this.CertForLabel.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("AccountManagementUserControl|37BB3730-812B-457C-AB63-1A03A31EEA95", "", "Certificate For:");
			this.CertForLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CertForLabel.IsFontBold = true;
			this.CertForLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 9, true);
			this.CertForLabel.Name = "CertForLabel";
			this.CertForLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.CertForLabel.TabIndex = 0;
			// 
			// ExciseNumbersPanel
			// 
			this.ExciseNumbersPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ExciseNumbersPanel.Controls.Add(this.EmcsNotificationEnabled);
			this.ExciseNumbersPanel.Controls.Add(this.ExciseNumbersGroupBox);
			this.ExciseNumbersPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ExciseNumbersPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 318, true);
			this.ExciseNumbersPanel.Name = "ExciseNumbersPanel";
			this.ExciseNumbersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 128, true);
			this.ExciseNumbersPanel.TabIndex = 16;
			// 
			// EmcsNotificationEnabled
			// 
			this.BindingSource.SetBindingMember(this.EmcsNotificationEnabled, "EmcsNotificationEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IT.Registry.Account)(null)).EmcsNotificationEnabled)));
			this.EmcsNotificationEnabled.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("4EBA962F-417B-41B9-A969-208070342255", "Enable EMCS-Notification");
			this.EmcsNotificationEnabled.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			this.EmcsNotificationEnabled.Name = "EmcsNotificationEnabled";
			this.EmcsNotificationEnabled.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 24, true);
			this.EmcsNotificationEnabled.TabIndex = 0;
			// 
			// ExciseNumbersGroupBox
			// 
			this.ExciseNumbersGroupBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("AA25987A-8449-4ECC-A528-32B8B47BD7B3", "Excise Number List");
			this.ExciseNumbersGroupBox.Controls.Add(this.ExciseNumbersGrid);
			this.ExciseNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ExciseNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 37, true);
			this.ExciseNumbersGroupBox.Name = "ExciseNumbersGroupBox";
			this.ExciseNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 87, true);
			this.ExciseNumbersGroupBox.TabIndex = 1;
			this.ExciseNumbersGroupBox.TabStop = false;
			// 
			// ExciseNumbersGrid
			// 
			this.ExciseNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExciseNumbersGrid, "ExciseNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IT.Registry.Account)(null)).ExciseNumbers)));
			this.ExciseNumbersGrid.CaptionVisible = false;
			this.ExciseNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExciseNumbersGrid.GridId = "1c31bfa9-dd74-4615-9d45-5571acd16fe7";
			this.ExciseNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExciseNumbersGrid.LayoutKey = "ExciseNumbersGrid";
			this.ExciseNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ExciseNumbersGrid.Name = "ExciseNumbersGrid";
			this.ExciseNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 68, true);
			this.ExciseNumbersGrid.TabIndex = 0;
			// 
			// AccountDetailsGroupBox
			// 
			this.AccountDetailsGroupBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("EF9E6D02-DE32-4C7B-AA35-0C12B6B8645B", "Account Details");
			this.AccountDetailsGroupBox.Controls.Add(this.AccountDetailsGrid);
			this.AccountDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AccountDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 212, true);
			this.AccountDetailsGroupBox.Name = "AccountDetailsGroupBox";
			this.AccountDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 106, true);
			this.AccountDetailsGroupBox.TabIndex = 17;
			this.AccountDetailsGroupBox.TabStop = false;
			// 
			// AccountDetailsGrid
			// 
			this.AccountDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AccountDetailsGrid, "AccountDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IT.Registry.Account)(null)).AccountDetails)));
			this.AccountDetailsGrid.CaptionVisible = false;
			this.AccountDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccountDetailsGrid.GridId = "4c6496ab-0a56-43ff-8927-62848330aed1";
			this.AccountDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AccountDetailsGrid.LayoutKey = "AccountDetailsGrid";
			this.AccountDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AccountDetailsGrid.Name = "AccountDetailsGrid";
			this.AccountDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 87, true);
			this.AccountDetailsGrid.TabIndex = 0;
			// 
			// AccountManagementUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AccountsGrid);
			this.Controls.Add(this.ITCertificatePanel);
			this.Controls.Add(this.AccountDetailsGroupBox);
			this.Controls.Add(this.ExciseNumbersPanel);
			this.Name = "AccountManagementUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 446, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AccountsGrid)).EndInit();
			this.AccountsGrid.ResumeLayout(false);
			this.AccountsGrid.PerformLayout();
			this.CertificateLoaderUserControl.ResumeLayout(true);
			this.CertificateLoaderUserControl.PerformLayout();
			this.ITCertificatePanel.ResumeLayout(false);
			this.ITCertificatePanel.PerformLayout();
			this.ExciseNumbersPanel.ResumeLayout(false);
			this.ExciseNumbersPanel.PerformLayout();
			this.ExciseNumbersGroupBox.ResumeLayout(false);
			this.ExciseNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExciseNumbersGrid)).EndInit();
			this.ExciseNumbersGrid.ResumeLayout(false);
			this.ExciseNumbersGrid.PerformLayout();
			this.AccountDetailsGroupBox.ResumeLayout(false);
			this.AccountDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AccountDetailsGrid)).EndInit();
			this.AccountDetailsGrid.ResumeLayout(false);
			this.AccountDetailsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.Registry.GUI.DigitalCertificateControl_p12 CertificateLoaderUserControl;
		Enterprise.ZArchitecture.ZTextBox CertAccountTextBox;
		Enterprise.ZArchitecture.ZLabel CertForLabel;
		public Enterprise.ZArchitecture.GUI.ZPanel ITCertificatePanel;
		public Enterprise.ZArchitecture.ZGrid AccountsGrid;
		private ZArchitecture.GUI.ZPanel ExciseNumbersPanel;
		private ZArchitecture.GUI.ZCheckBox EmcsNotificationEnabled;
		private ZArchitecture.GUI.ZGroupBox ExciseNumbersGroupBox;
		private Enterprise.ZArchitecture.ZGrid ExciseNumbersGrid;
		private ZArchitecture.GUI.ZGroupBox AccountDetailsGroupBox;
		private Enterprise.ZArchitecture.ZGrid AccountDetailsGrid;

	}
}
