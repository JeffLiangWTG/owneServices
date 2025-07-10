namespace Enterprise.Client.SWL.GUI
{
	partial class ShipnetSetupUserControl
	{
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (BrowserDialog != null)
				{
					BrowserDialog.Dispose();
				}
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ShipnetSetupTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ShipnetEDITabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.IsShipnetCarrierCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EDISettingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BrowseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PortNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PortNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ExportFileNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportFileNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PasswordLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ExportFileExtensionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UsernameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportFileExtensionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UsernameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DestinationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ServerAddressSubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DestinationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ServerAddressSubjectLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EDITypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DebtorControlCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EDITypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DebtorControlCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CreditorControlCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CreditorControlCode = new Enterprise.ZArchitecture.ZTextBox();
			this.ChargeGroupTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ChargeCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChargeCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ChargeGroupsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChargeGroupsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BrowserDialog = new Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipnetSetupTabControl.SuspendLayout();
			this.ShipnetEDITabPage.SuspendLayout();
			this.ShipnetEDITabPage.SuspendLayout();
			this.EDISettingsGroupBox.SuspendLayout();
			this.EDITypeDropEdit.SuspendLayout();
			this.ChargeGroupTabPage.SuspendLayout();
			this.ChargeGroupTabPage.SuspendLayout();
			this.ChargeCodesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodesGrid)).BeginInit();
			this.ChargeCodesGrid.SuspendLayout();
			this.ChargeGroupsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargeGroupsGrid)).BeginInit();
			this.ChargeGroupsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject);
			// 
			// ShipnetSetupTabControl
			// 
			this.ShipnetSetupTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ShipnetSetupTabControl.Controls.Add(this.ShipnetEDITabPage);
			this.ShipnetSetupTabControl.Controls.Add(this.ChargeGroupTabPage);
			this.ShipnetSetupTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipnetSetupTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipnetSetupTabControl.Name = "ShipnetSetupTabControl";
			this.ShipnetSetupTabControl.SelectedIndex = 0;
			this.ShipnetSetupTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 623, true);
			this.ShipnetSetupTabControl.TabIndex = 3;
			// 
			// ShipnetEDITabPage
			// 
			this.ShipnetEDITabPage.Controls.Add(this.IsShipnetCarrierCheckBox);
			this.ShipnetEDITabPage.Controls.Add(this.EDISettingsGroupBox);
			this.ShipnetEDITabPage.Controls.Add(this.EDITypeDropEdit);
			this.ShipnetEDITabPage.Controls.Add(this.DebtorControlCodeLabel);
			this.ShipnetEDITabPage.Controls.Add(this.EDITypeLabel);
			this.ShipnetEDITabPage.Controls.Add(this.DebtorControlCodeTextBox);
			this.ShipnetEDITabPage.Controls.Add(this.CreditorControlCodeLabel);
			this.ShipnetEDITabPage.Controls.Add(this.CreditorControlCode);
			this.ShipnetEDITabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ShipnetEDITabPage.Name = "ShipnetEDITabPage";
			this.ShipnetEDITabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 596, true);
			this.ShipnetEDITabPage.TabIndex = 1;
			this.ShipnetEDITabPage.Text = "EDI Settings";
			// 
			// IsShipnetCarrierCheckBox
			// 
			this.IsShipnetCarrierCheckBox.AutoSize = true;
			this.IsShipnetCarrierCheckBox.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.IsShipnetCarrierCheckBox, "IsShipnetCarrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).IsShipnetCarrier)));
			this.IsShipnetCarrierCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsShipnetCarrierCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsShipnetCarrierCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 6, true);
			this.IsShipnetCarrierCheckBox.Name = "IsShipnetCarrierCheckBox";
			this.IsShipnetCarrierCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 17, true);
			this.IsShipnetCarrierCheckBox.TabIndex = 0;
			this.IsShipnetCarrierCheckBox.Text = "Send Data To Shipnet     ";
			this.IsShipnetCarrierCheckBox.UseVisualStyleBackColor = true;
			this.IsShipnetCarrierCheckBox.CheckedChanged += new System.EventHandler(this.IsShipnetCarrierCheckBox_CheckedChanged);
			// 
			// EDISettingsGroupBox
			// 
			this.EDISettingsGroupBox.Controls.Add(this.BrowseButton);
			this.EDISettingsGroupBox.Controls.Add(this.PortNumberCalcEdit);
			this.EDISettingsGroupBox.Controls.Add(this.PortNumberLabel);
			this.EDISettingsGroupBox.Controls.Add(this.ExportFileNameLabel);
			this.EDISettingsGroupBox.Controls.Add(this.PasswordTextBox);
			this.EDISettingsGroupBox.Controls.Add(this.ExportFileNameTextBox);
			this.EDISettingsGroupBox.Controls.Add(this.PasswordLabel);
			this.EDISettingsGroupBox.Controls.Add(this.ExportFileExtensionLabel);
			this.EDISettingsGroupBox.Controls.Add(this.UsernameTextBox);
			this.EDISettingsGroupBox.Controls.Add(this.ExportFileExtensionTextBox);
			this.EDISettingsGroupBox.Controls.Add(this.UsernameLabel);
			this.EDISettingsGroupBox.Controls.Add(this.DestinationLabel);
			this.EDISettingsGroupBox.Controls.Add(this.ServerAddressSubjectTextBox);
			this.EDISettingsGroupBox.Controls.Add(this.DestinationTextBox);
			this.EDISettingsGroupBox.Controls.Add(this.ServerAddressSubjectLabel);
			this.EDISettingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 107, true);
			this.EDISettingsGroupBox.Name = "EDISettingsGroupBox";
			this.EDISettingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 210, true);
			this.EDISettingsGroupBox.TabIndex = 19;
			this.EDISettingsGroupBox.TabStop = false;
			this.EDISettingsGroupBox.Text = "EDI Settings";
			// 
			// BrowseButton
			// 
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 63, true);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 23, true);
			this.BrowseButton.TabIndex = 3;
			this.BrowseButton.Text = "...";
			this.BrowseButton.UseVisualStyleBackColor = true;
			this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
			// 
			// PortNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PortNumberCalcEdit, "CommunicationMode+EK_PortNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).CommunicationMode.EK_PortNumber)));
			this.PortNumberCalcEdit.DecimalPlaces = 2;
			this.PortNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 117, true);
			this.PortNumberCalcEdit.Name = "PortNumberCalcEdit";
			this.PortNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.PortNumberCalcEdit.TabIndex = 5;
			this.PortNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PortNumberLabel
			// 
			this.PortNumberLabel.AutoSize = true;
			this.PortNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 120, true);
			this.PortNumberLabel.Name = "PortNumberLabel";
			this.PortNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.PortNumberLabel.TabIndex = 19;
			this.PortNumberLabel.Text = "Port Number:";
			// 
			// ExportFileNameLabel
			// 
			this.ExportFileNameLabel.AutoSize = true;
			this.ExportFileNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.ExportFileNameLabel.Name = "ExportFileNameLabel";
			this.ExportFileNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 13, true);
			this.ExportFileNameLabel.TabIndex = 6;
			this.ExportFileNameLabel.Text = "Export File Name:";
			// 
			// PasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.PasswordTextBox, "CommunicationMode+EK_Password");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).CommunicationMode.EK_Password)));
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 169, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.PasswordTextBox.TabIndex = 7;
			this.PasswordTextBox.UseSystemPasswordChar = true;
			// 
			// ExportFileNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportFileNameTextBox, "CommunicationMode+EK_Filename");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).CommunicationMode.EK_Filename)));
			this.ExportFileNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExportFileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 13, true);
			this.ExportFileNameTextBox.Name = "ExportFileNameTextBox";
			this.ExportFileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ExportFileNameTextBox.TabIndex = 0;
			// 
			// PasswordLabel
			// 
			this.PasswordLabel.AutoSize = true;
			this.PasswordLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 172, true);
			this.PasswordLabel.Name = "PasswordLabel";
			this.PasswordLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 13, true);
			this.PasswordLabel.TabIndex = 17;
			this.PasswordLabel.Text = "Password:";
			// 
			// ExportFileExtensionLabel
			// 
			this.ExportFileExtensionLabel.AutoSize = true;
			this.ExportFileExtensionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 42, true);
			this.ExportFileExtensionLabel.Name = "ExportFileExtensionLabel";
			this.ExportFileExtensionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 13, true);
			this.ExportFileExtensionLabel.TabIndex = 8;
			this.ExportFileExtensionLabel.Text = "Export File Extension:";
			// 
			// UsernameTextBox
			// 
			this.BindingSource.SetBindingMember(this.UsernameTextBox, "CommunicationMode+EK_LoginName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).CommunicationMode.EK_LoginName)));
			this.UsernameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UsernameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 143, true);
			this.UsernameTextBox.Name = "UsernameTextBox";
			this.UsernameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.UsernameTextBox.TabIndex = 6;
			// 
			// ExportFileExtensionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportFileExtensionTextBox, "CommunicationMode+EK_FileFormat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).CommunicationMode.EK_FileFormat)));
			this.ExportFileExtensionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExportFileExtensionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 39, true);
			this.ExportFileExtensionTextBox.Name = "ExportFileExtensionTextBox";
			this.ExportFileExtensionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.ExportFileExtensionTextBox.TabIndex = 1;
			// 
			// UsernameLabel
			// 
			this.UsernameLabel.AutoSize = true;
			this.UsernameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 146, true);
			this.UsernameLabel.Name = "UsernameLabel";
			this.UsernameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.UsernameLabel.TabIndex = 15;
			this.UsernameLabel.Text = "Username:";
			// 
			// DestinationLabel
			// 
			this.DestinationLabel.AutoSize = true;
			this.DestinationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 68, true);
			this.DestinationLabel.Name = "DestinationLabel";
			this.DestinationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 13, true);
			this.DestinationLabel.TabIndex = 11;
			this.DestinationLabel.Text = "Destination:";
			// 
			// ServerAddressSubjectTextBox
			// 
			this.BindingSource.SetBindingMember(this.ServerAddressSubjectTextBox, "CommunicationMode+EK_ServerAddressSubject");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).CommunicationMode.EK_ServerAddressSubject)));
			this.ServerAddressSubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ServerAddressSubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 91, true);
			this.ServerAddressSubjectTextBox.Name = "ServerAddressSubjectTextBox";
			this.ServerAddressSubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ServerAddressSubjectTextBox.TabIndex = 4;
			// 
			// DestinationTextBox
			// 
			this.BindingSource.SetBindingMember(this.DestinationTextBox, "CommunicationMode+EK_Destination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).CommunicationMode.EK_Destination)));
			this.DestinationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DestinationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 65, true);
			this.DestinationTextBox.Name = "DestinationTextBox";
			this.DestinationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.DestinationTextBox.TabIndex = 2;
			// 
			// ServerAddressSubjectLabel
			// 
			this.ServerAddressSubjectLabel.AutoSize = true;
			this.ServerAddressSubjectLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 94, true);
			this.ServerAddressSubjectLabel.Name = "ServerAddressSubjectLabel";
			this.ServerAddressSubjectLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 13, true);
			this.ServerAddressSubjectLabel.TabIndex = 13;
			this.ServerAddressSubjectLabel.Text = "Server Address Subject:";
			// 
			// EDITypeDropEdit
			// 
			this.EDITypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EDITypeDropEdit, "CommunicationMode+EK_CommunicationsTransport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).CommunicationMode.EK_CommunicationsTransport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).EDIExportMappingConfigurationList)));
			this.EDITypeDropEdit.BindToList = "EDIExportMappingConfigurationList";
			this.EDITypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 81, true);
			this.EDITypeDropEdit.Name = "EDITypeDropEdit";
			this.EDITypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.EDITypeDropEdit.TabIndex = 3;
			// 
			// DebtorControlCodeLabel
			// 
			this.DebtorControlCodeLabel.AutoSize = true;
			this.DebtorControlCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 32, true);
			this.DebtorControlCodeLabel.Name = "DebtorControlCodeLabel";
			this.DebtorControlCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 13, true);
			this.DebtorControlCodeLabel.TabIndex = 0;
			this.DebtorControlCodeLabel.Text = "Debtor Control Code:";
			// 
			// EDITypeLabel
			// 
			this.EDITypeLabel.AutoSize = true;
			this.EDITypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 84, true);
			this.EDITypeLabel.Name = "EDITypeLabel";
			this.EDITypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 13, true);
			this.EDITypeLabel.TabIndex = 1;
			this.EDITypeLabel.Text = "EDI Type:";
			// 
			// DebtorControlCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.DebtorControlCodeTextBox, "DebtorControlCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).DebtorControlCode)));
			this.DebtorControlCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DebtorControlCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 29, true);
			this.DebtorControlCodeTextBox.Name = "DebtorControlCodeTextBox";
			this.DebtorControlCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.DebtorControlCodeTextBox.TabIndex = 1;
			// 
			// CreditorControlCodeLabel
			// 
			this.CreditorControlCodeLabel.AutoSize = true;
			this.CreditorControlCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 58, true);
			this.CreditorControlCodeLabel.Name = "CreditorControlCodeLabel";
			this.CreditorControlCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 13, true);
			this.CreditorControlCodeLabel.TabIndex = 3;
			this.CreditorControlCodeLabel.Text = "Creditor Control Code:";
			// 
			// CreditorControlCode
			// 
			this.BindingSource.SetBindingMember(this.CreditorControlCode, "CreditorControlCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).CreditorControlCode)));
			this.CreditorControlCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CreditorControlCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 55, true);
			this.CreditorControlCode.Name = "CreditorControlCode";
			this.CreditorControlCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.CreditorControlCode.TabIndex = 2;
			// 
			// ChargeGroupTabPage
			// 
			this.ChargeGroupTabPage.Controls.Add(this.ChargeCodesGroupBox);
			this.ChargeGroupTabPage.Controls.Add(this.ChargeGroupsGroupBox);
			this.ChargeGroupTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ChargeGroupTabPage.Name = "ChargeGroupTabPage";
			this.ChargeGroupTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 596, true);
			this.ChargeGroupTabPage.TabIndex = 0;
			this.ChargeGroupTabPage.Text = "Charge Group";
			// 
			// ChargeCodesGroupBox
			// 
			this.ChargeCodesGroupBox.Controls.Add(this.ChargeCodesGrid);
			this.ChargeCodesGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ChargeCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(266, 0, true);
			this.ChargeCodesGroupBox.Name = "ChargeCodesGroupBox";
			this.ChargeCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 596, true);
			this.ChargeCodesGroupBox.TabIndex = 6;
			this.ChargeCodesGroupBox.TabStop = false;
			this.ChargeCodesGroupBox.Text = "Charge Codes";
			this.ChargeCodesGroupBox.Visible = false;
			// 
			// ChargeCodesGrid
			// 
			this.ChargeCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargeCodesGrid, "ChargeGroups.Charges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.SWL.Business.ShipnetChargeGroup)(((System.Collections.IList)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).ChargeGroups)).SyncRoot)).Charges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.SWL.Business.ShipnetCharge)(((System.Collections.IList)(((Enterprise.Client.SWL.Business.ShipnetChargeGroup)(((System.Collections.IList)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).ChargeGroups)).SyncRoot)).Charges)).SyncRoot)).ChargePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.SWL.Business.ShipnetCharge)(((System.Collections.IList)(((Enterprise.Client.SWL.Business.ShipnetChargeGroup)(((System.Collections.IList)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).ChargeGroups)).SyncRoot)).Charges)).SyncRoot)).ChargeCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.SWL.Business.ShipnetCharge)(((System.Collections.IList)(((Enterprise.Client.SWL.Business.ShipnetChargeGroup)(((System.Collections.IList)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).ChargeGroups)).SyncRoot)).Charges)).SyncRoot)).ChargeCodeDesc)));
			this.ChargeCodesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo2.BindToList = "ChargeCodeList";
			zGuidFindBoxColumnStyleInfo2.Caption = "Charge Code";
			zGuidFindBoxColumnStyleInfo2.ColumnName = "ChargePK";
			zGuidFindBoxColumnStyleInfo2.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCode;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.Caption = "Description";
			zTextBoxColumnStyleInfo4.ColumnName = "ChargeCodeDesc";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.ChargeCodesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ChargeCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ChargeCodesGrid.CopySelectedRowsAllowed = true;
			this.ChargeCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargeCodesGrid.GridId = "2d7f75ba-dcae-409a-8d88-f081298884cc";
			this.ChargeCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeCodesGrid.LayoutKey = "zGrid1";
			this.ChargeCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ChargeCodesGrid.Name = "ChargeCodesGrid";
			this.ChargeCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 577, true);
			this.ChargeCodesGrid.TabIndex = 0;
			// 
			// ChargeGroupsGroupBox
			// 
			this.ChargeGroupsGroupBox.Controls.Add(this.ChargeGroupsGrid);
			this.ChargeGroupsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ChargeGroupsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargeGroupsGroupBox.Name = "ChargeGroupsGroupBox";
			this.ChargeGroupsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 596, true);
			this.ChargeGroupsGroupBox.TabIndex = 5;
			this.ChargeGroupsGroupBox.TabStop = false;
			this.ChargeGroupsGroupBox.Text = "Charge Groups";
			// 
			// ChargeGroupsGrid
			// 
			this.ChargeGroupsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargeGroupsGrid, "ChargeGroups");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).ChargeGroups)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.SWL.Business.ShipnetChargeGroup)(((System.Collections.IList)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).ChargeGroups)).SyncRoot)).ChargeGroupCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.SWL.Business.ShipnetChargeGroup)(((System.Collections.IList)(((Enterprise.Client.SWL.Business.ShipnetSetupBusinessObject)(null)).ChargeGroups)).SyncRoot)).ChargeGroupDescription)));
			this.ChargeGroupsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Code";
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeGroupCode";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.Caption = "Description";
			zTextBoxColumnStyleInfo5.ColumnName = "ChargeGroupDescription";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.ChargeGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargeGroupsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ChargeGroupsGrid.CopySelectedRowsAllowed = true;
			this.ChargeGroupsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargeGroupsGrid.GridId = "fc8ef2af-2725-449b-a9ff-060ac617bd44";
			this.ChargeGroupsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeGroupsGrid.LayoutKey = "ChargeGroupsGrid";
			this.ChargeGroupsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ChargeGroupsGrid.Name = "ChargeGroupsGrid";
			this.ChargeGroupsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 577, true);
			this.ChargeGroupsGrid.TabIndex = 0;
			// 
			// BrowserDialog
			// 
			this.BrowserDialog.CreateDirectory = false;
			this.BrowserDialog.Description = "Selet Directory To Save File";
			this.BrowserDialog.RequireMappablePath = false;
			this.BrowserDialog.RootFolder = System.Environment.SpecialFolder.Desktop;
			this.BrowserDialog.ShowNewFolderButton = true;
			// 
			// ShipnetSetupUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ShipnetSetupTabControl);
			this.Name = "ShipnetSetupUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 623, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipnetSetupTabControl.ResumeLayout(false);
			this.ShipnetSetupTabControl.PerformLayout();
			this.ShipnetEDITabPage.ResumeLayout(false);
			this.ShipnetEDITabPage.PerformLayout();
			this.ShipnetEDITabPage.ResumeLayout(false);
			this.ShipnetEDITabPage.PerformLayout();
			this.EDISettingsGroupBox.ResumeLayout(false);
			this.EDISettingsGroupBox.PerformLayout();
			this.EDITypeDropEdit.ResumeLayout(true);
			this.EDITypeDropEdit.PerformLayout();
			this.ChargeGroupTabPage.ResumeLayout(false);
			this.ChargeGroupTabPage.PerformLayout();
			this.ChargeGroupTabPage.ResumeLayout(false);
			this.ChargeCodesGroupBox.ResumeLayout(false);
			this.ChargeCodesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodesGrid)).EndInit();
			this.ChargeCodesGrid.ResumeLayout(false);
			this.ChargeCodesGrid.PerformLayout();
			this.ChargeGroupsGroupBox.ResumeLayout(false);
			this.ChargeGroupsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargeGroupsGrid)).EndInit();
			this.ChargeGroupsGrid.ResumeLayout(false);
			this.ChargeGroupsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl ShipnetSetupTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage ChargeGroupTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ChargeCodesGroupBox;
		private Enterprise.ZArchitecture.ZGrid ChargeCodesGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ChargeGroupsGroupBox;
		private Enterprise.ZArchitecture.ZGrid ChargeGroupsGrid;
		private Enterprise.ZArchitecture.GUI.ZTabPage ShipnetEDITabPage;
		private Enterprise.ZArchitecture.ZLabel DebtorControlCodeLabel;
		private Enterprise.ZArchitecture.ZTextBox CreditorControlCode;
		private Enterprise.ZArchitecture.ZLabel CreditorControlCodeLabel;
		private Enterprise.ZArchitecture.ZTextBox DebtorControlCodeTextBox;
		private Enterprise.ZArchitecture.ZLabel EDITypeLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit EDITypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox EDISettingsGroupBox;
		private System.ComponentModel.IContainer components;
		protected Enterprise.ZArchitecture.ZTextBox ExportFileExtensionTextBox;
		protected Enterprise.ZArchitecture.ZLabel ExportFileExtensionLabel;
		protected Enterprise.ZArchitecture.ZTextBox ExportFileNameTextBox;
		protected Enterprise.ZArchitecture.ZLabel ExportFileNameLabel;
		protected Enterprise.ZArchitecture.ZTextBox PasswordTextBox;
		protected Enterprise.ZArchitecture.ZLabel PasswordLabel;
		protected Enterprise.ZArchitecture.ZTextBox UsernameTextBox;
		protected Enterprise.ZArchitecture.ZLabel UsernameLabel;
		protected Enterprise.ZArchitecture.ZTextBox ServerAddressSubjectTextBox;
		protected Enterprise.ZArchitecture.ZLabel ServerAddressSubjectLabel;
		protected Enterprise.ZArchitecture.ZTextBox DestinationTextBox;
		protected Enterprise.ZArchitecture.ZLabel DestinationLabel;
		protected Enterprise.ZArchitecture.ZLabel PortNumberLabel;
		protected Enterprise.ZArchitecture.ZCalcEdit PortNumberCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZButton BrowseButton;
		protected Enterprise.ZArchitecture.GUI.ZFolderBrowserDialog BrowserDialog;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsShipnetCarrierCheckBox;
	}
}
