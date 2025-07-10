namespace Enterprise.Customs.IT.GUI
{
	partial class GlbStaffForm_ITCredentialsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AccAndUserGroupIT = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NodeListGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ITAccUserGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CertificateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ITCertificatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CertificateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
			this.CertificateExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CertificatePasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificatePasswordStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.XadesCertificateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.XadesCertificatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.XadesCertificateChipsetDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClearXadesCertificateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddXadesCertificateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.XadesCertificateSerialNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.XadesCertificateChooseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.XadesCertificateInformationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.XadesCertificateCertificateAuthorityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AutomaticSignatureGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AutomaticSignatureUserControl = new Enterprise.Customs.IT.GUI.GlbStaffAutomaticSignatureUserControl();
			this.ITCredentialsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AccAndUserGroupIT.SuspendLayout();
			this.NodeListGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ITAccUserGrid)).BeginInit();
			this.ITAccUserGrid.SuspendLayout();
			this.CertificateGroupBox.SuspendLayout();
			this.ITCertificatePanel.SuspendLayout();
			this.CertificateLoaderUserControl.SuspendLayout();
			this.CertificateExpiryDateEdit.SuspendLayout();
			this.XadesCertificateGroupBox.SuspendLayout();
			this.XadesCertificatePanel.SuspendLayout();
			this.XadesCertificateChipsetDropEdit.SuspendLayout();
			this.XadesCertificateCertificateAuthorityDropEdit.SuspendLayout();
			this.AutomaticSignatureGroupBox.SuspendLayout();
			this.AutomaticSignatureUserControl.SuspendLayout();
			this.ITCredentialsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// CredentialsHintLabel
			// 
			this.CredentialsHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.GlbStaffWrapper);
			// 
			// AccAndUserGroupIT
			// 
			this.AccAndUserGroupIT.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("GlbStaffForm|B31D5B8D-29EA-4A69-B0B2-1DDF48B1E7DD", "Subscriptions Management");
			this.AccAndUserGroupIT.Controls.Add(this.NodeListGroupBox);
			this.AccAndUserGroupIT.Controls.Add(this.CertificateGroupBox);
			this.AccAndUserGroupIT.Controls.Add(this.XadesCertificateGroupBox);
			this.AccAndUserGroupIT.Controls.Add(this.AutomaticSignatureGroupBox);
			this.AccAndUserGroupIT.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccAndUserGroupIT.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AccAndUserGroupIT.Name = "AccAndUserGroupIT";
			this.AccAndUserGroupIT.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 550, true);
			this.AccAndUserGroupIT.TabIndex = 1;
			this.AccAndUserGroupIT.TabStop = false;
			// 
			// NodeListGroupBox
			// 
			this.NodeListGroupBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("GlbStaffForm|0B1A2B72-18D5-4FA6-AF5C-ECAEC2D19A58", "Allowed node list");
			this.NodeListGroupBox.Controls.Add(this.ITAccUserGrid);
			this.NodeListGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NodeListGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 470, true);
			this.NodeListGroupBox.Name = "NodeListGroupBox";
			this.NodeListGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 77, true);
			this.NodeListGroupBox.TabIndex = 2;
			this.NodeListGroupBox.TabStop = false;
			// 
			// ITAccUserGrid
			// 
			this.ITAccUserGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ITAccUserGrid, "PasswordCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbStaffWrapper)(null)).PasswordCollection)));
			this.ITAccUserGrid.CaptionVisible = false;
			this.ITAccUserGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ITAccUserGrid.GridId = "d35b96df-79f1-483f-8770-3aa78d5599f0";
			this.ITAccUserGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ITAccUserGrid.LayoutKey = "ITAccUserGrid";
			this.ITAccUserGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ITAccUserGrid.Name = "ITAccUserGrid";
			this.ITAccUserGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 58, true);
			this.ITAccUserGrid.TabIndex = 1;
			this.ITAccUserGrid.AfterBind += new System.EventHandler(this.ITAccUserGrid_AfterBind);
			this.ITAccUserGrid.CurrentCellChanged += new System.EventHandler(this.ITAccUserGrid_CurrentCellChanged);
			// 
			// CertificateGroupBox
			// 
			this.CertificateGroupBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("GlbStaffForm|092969B4-9D40-4B4D-B72E-A9370F6225CF", "Certificate");
			this.CertificateGroupBox.Controls.Add(this.ITCertificatePanel);
			this.CertificateGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.CertificateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 360, true);
			this.CertificateGroupBox.Name = "CertificateGroupBox";
			this.CertificateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 110, true);
			this.CertificateGroupBox.TabIndex = 1;
			this.CertificateGroupBox.TabStop = false;
			// 
			// ITCertificatePanel
			// 
			this.ITCertificatePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ITCertificatePanel.Controls.Add(this.CertificateLabel);
			this.ITCertificatePanel.Controls.Add(this.CertificateLoaderUserControl);
			this.ITCertificatePanel.Controls.Add(this.CertificateExpiryDateEdit);
			this.ITCertificatePanel.Controls.Add(this.CertificatePasswordTextBox);
			this.ITCertificatePanel.Controls.Add(this.CertificatePasswordStatusTextBox);
			this.ITCertificatePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ITCertificatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ITCertificatePanel.Name = "ITCertificatePanel";
			this.ITCertificatePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ITCertificatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 91, true);
			this.ITCertificatePanel.TabIndex = 2;
			// 
			// CertificateLabel
			// 
			this.CertificateLabel.AutoSize = true;
			this.CertificateLabel.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("31A4734C-6FDE-418F-AAEE-E3E746103F5E", "Certificate");
			this.CertificateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CertificateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 13, true);
			this.CertificateLabel.Name = "CertificateLabel";
			this.CertificateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 13, true);
			this.CertificateLabel.TabIndex = 0;
			this.CertificateLabel.UseMnemonic = false;
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "PasswordCollection.GP_Certificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.GlbBrokerExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).GP_Certificate)));
			this.CertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = null;
			this.CertificateLoaderUserControl.FileDataAsString = "";
			this.CertificateLoaderUserControl.FileDialogTitle = "";
			this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
			this.CertificateLoaderUserControl.InitialDirectory = "";
			this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 8, true);
			this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
			this.CertificateLoaderUserControl.ReadOnly = false;
			this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateLoaderUserControl.TabIndex = 1;
			// 
			// CertificateExpiryDateEdit
			// 
			this.CertificateExpiryDateEdit.AllowDrop = true;
			this.CertificateExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.CertificateExpiryDateEdit, "PasswordCollection.GP_ExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.GlbBrokerExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).GP_ExpiryDate)));
			this.CertificateExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 63, true);
			this.CertificateExpiryDateEdit.Name = "CertificateExpiryDateEdit";
			this.CertificateExpiryDateEdit.TabIndex = 4;
			// 
			// CertificatePasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificatePasswordTextBox, "PasswordCollection.CurrentDecryptedCertificatePassphrase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.GlbBrokerExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).CurrentDecryptedCertificatePassphrase)));
			this.CertificatePasswordTextBox.CaptionResourceString = null;
			this.CertificatePasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificatePasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 37, true);
			this.CertificatePasswordTextBox.Name = "CertificatePasswordTextBox";
			this.CertificatePasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.CertificatePasswordTextBox.TabIndex = 2;
			this.CertificatePasswordTextBox.UseSystemPasswordChar = true;
			// 
			// CertificatePasswordStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificatePasswordStatusTextBox, "PasswordCollection.PasswordStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.GlbBrokerExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).PasswordStatus)));
			this.CertificatePasswordStatusTextBox.CaptionResourceString = null;
			this.CertificatePasswordStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificatePasswordStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(369, 37, true);
			this.CertificatePasswordStatusTextBox.Name = "CertificatePasswordStatusTextBox";
			this.CertificatePasswordStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.CertificatePasswordStatusTextBox.TabIndex = 3;
			// 
			// XadesCertificateGroupBox
			// 
			this.XadesCertificateGroupBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("c7d809a8-b641-4054-b713-f69f0b01148d", "XADES Certificate");
			this.XadesCertificateGroupBox.Controls.Add(this.XadesCertificatePanel);
			this.XadesCertificateGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.XadesCertificateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 175, true);
			this.XadesCertificateGroupBox.Name = "XadesCertificateGroupBox";
			this.XadesCertificateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 185, true);
			this.XadesCertificateGroupBox.TabIndex = 0;
			this.XadesCertificateGroupBox.TabStop = false;
			// 
			// XadesCertificatePanel
			// 
			this.XadesCertificatePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.XadesCertificatePanel.Controls.Add(this.XadesCertificateChipsetDropEdit);
			this.XadesCertificatePanel.Controls.Add(this.ClearXadesCertificateButton);
			this.XadesCertificatePanel.Controls.Add(this.AddXadesCertificateButton);
			this.XadesCertificatePanel.Controls.Add(this.XadesCertificateSerialNumberTextBox);
			this.XadesCertificatePanel.Controls.Add(this.XadesCertificateChooseButton);
			this.XadesCertificatePanel.Controls.Add(this.XadesCertificateInformationButton);
			this.XadesCertificatePanel.Controls.Add(this.XadesCertificateCertificateAuthorityDropEdit);
			this.XadesCertificatePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.XadesCertificatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.XadesCertificatePanel.Name = "XadesCertificatePanel";
			this.XadesCertificatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 166, true);
			this.XadesCertificatePanel.TabIndex = 0;
			// 
			// XadesCertificateChipsetDropEdit
			// 
			this.XadesCertificateChipsetDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.XadesCertificateChipsetDropEdit, "CryptokiCertificateCollection.GP_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.CryptokiExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbStaffWrapper)(null)).CryptokiCertificateCollection)).SyncRoot)).GP_Name)));
			this.XadesCertificateChipsetDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 57, true);
			this.XadesCertificateChipsetDropEdit.Name = "XadesCertificateChipsetDropEdit";
			this.XadesCertificateChipsetDropEdit.ShouldResizeByMaxLength = false;
			this.XadesCertificateChipsetDropEdit.ShowDescriptionBox = false;
			this.XadesCertificateChipsetDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.XadesCertificateChipsetDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.XadesCertificateChipsetDropEdit.TabIndex = 3;
			// 
			// ClearXadesCertificateButton
			// 
			this.ClearXadesCertificateButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("df2f9237-cc74-4f10-b7be-984914da8362", "Clear Certificate", "Clear XADES Certificate");
			this.ClearXadesCertificateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 3, true);
			this.ClearXadesCertificateButton.Name = "ClearXadesCertificateButton";
			this.ClearXadesCertificateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 23, true);
			this.ClearXadesCertificateButton.TabIndex = 1;
			this.ClearXadesCertificateButton.ToolTipCaption = null;
			// 
			// AddXadesCertificateButton
			// 
			this.AddXadesCertificateButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("bee239a3-5361-491a-9cf8-18ac4af30b64", "Add Certificate", "Add XADES Certificate");
			this.AddXadesCertificateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 3, true);
			this.AddXadesCertificateButton.Name = "AddXadesCertificateButton";
			this.AddXadesCertificateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 23, true);
			this.AddXadesCertificateButton.TabIndex = 0;
			this.AddXadesCertificateButton.ToolTipCaption = null;
			// 
			// XadesCertificateSerialNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.XadesCertificateSerialNumberTextBox, "CryptokiCertificateCollection.GP_CertificateSerialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.CryptokiExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbStaffWrapper)(null)).CryptokiCertificateCollection)).SyncRoot)).GP_CertificateSerialNumber)));
			this.XadesCertificateSerialNumberTextBox.CaptionResourceString = null;
			this.XadesCertificateSerialNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.XadesCertificateSerialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 112, true);
			this.XadesCertificateSerialNumberTextBox.Name = "XadesCertificateSerialNumberTextBox";
			this.XadesCertificateSerialNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.XadesCertificateSerialNumberTextBox.TabIndex = 5;
			// 
			// XadesCertificateChooseButton
			// 
			this.XadesCertificateChooseButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("69e76a66-c922-434f-b51d-4a2960468f93", "Choose Certificate");
			this.XadesCertificateChooseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 83, true);
			this.XadesCertificateChooseButton.Name = "XadesCertificateChooseButton";
			this.XadesCertificateChooseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.XadesCertificateChooseButton.TabIndex = 4;
			this.XadesCertificateChooseButton.ToolTipCaption = null;
			this.XadesCertificateChooseButton.UseVisualStyleBackColor = true;
			// 
			// XadesCertificateInformationButton
			// 
			this.XadesCertificateInformationButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("661afdcd-6d23-4427-93e1-686807fddcbe", "Certificate Information");
			this.XadesCertificateInformationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 138, true);
			this.XadesCertificateInformationButton.Name = "XadesCertificateInformationButton";
			this.XadesCertificateInformationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.XadesCertificateInformationButton.TabIndex = 6;
			this.XadesCertificateInformationButton.ToolTipCaption = null;
			this.XadesCertificateInformationButton.UseVisualStyleBackColor = true;
			// 
			// XadesCertificateCertificateAuthorityDropEdit
			// 
			this.XadesCertificateCertificateAuthorityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.XadesCertificateCertificateAuthorityDropEdit, "CryptokiCertificateCollection.GP_CertificateAuthority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.CryptokiExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbStaffWrapper)(null)).CryptokiCertificateCollection)).SyncRoot)).GP_CertificateAuthority)));
			this.XadesCertificateCertificateAuthorityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 31, true);
			this.XadesCertificateCertificateAuthorityDropEdit.Name = "XadesCertificateCertificateAuthorityDropEdit";
			this.XadesCertificateCertificateAuthorityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.XadesCertificateCertificateAuthorityDropEdit.TabIndex = 2;
			// 
			// AutomaticSignatureGroupBox
			// 
			this.AutomaticSignatureGroupBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("05b6535d-ae62-43ba-9e3a-da04abe20103", "Automatic Signature");
			this.AutomaticSignatureGroupBox.Controls.Add(this.AutomaticSignatureUserControl);
			this.AutomaticSignatureGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.AutomaticSignatureGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AutomaticSignatureGroupBox.Name = "AutomaticSignatureGroupBox";
			this.AutomaticSignatureGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 159, true);
			this.AutomaticSignatureGroupBox.TabIndex = 3;
			this.AutomaticSignatureGroupBox.TabStop = false;
			// 
			// AutomaticSignatureUserControl
			// 
			this.AutomaticSignatureUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AutomaticSignatureUserControl, ".");
			this.AutomaticSignatureUserControl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.AutomaticSignatureUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AutomaticSignatureUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AutomaticSignatureUserControl.Name = "AutomaticSignatureUserControl";
			this.AutomaticSignatureUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 140, true);
			this.AutomaticSignatureUserControl.TabIndex = 0;
			// 
			// ITCredentialsPanel
			// 
			this.ITCredentialsPanel.Controls.Add(this.AccAndUserGroupIT);
			this.ITCredentialsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ITCredentialsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.ITCredentialsPanel.Name = "ITCredentialsPanel";
			this.ITCredentialsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 550, true);
			this.ITCredentialsPanel.TabIndex = 1;
			// 
			// GlbStaffForm_ITCredentialsUserControl
			// 
			this.Controls.Add(this.ITCredentialsPanel);
			this.Name = "GlbStaffForm_ITCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 573, true);
			this.Controls.SetChildIndex(this.CredentialsHintLabel, 0);
			this.Controls.SetChildIndex(this.ITCredentialsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AccAndUserGroupIT.ResumeLayout(false);
			this.AccAndUserGroupIT.PerformLayout();
			this.NodeListGroupBox.ResumeLayout(false);
			this.NodeListGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ITAccUserGrid)).EndInit();
			this.ITAccUserGrid.ResumeLayout(false);
			this.ITAccUserGrid.PerformLayout();
			this.CertificateGroupBox.ResumeLayout(false);
			this.CertificateGroupBox.PerformLayout();
			this.ITCertificatePanel.ResumeLayout(false);
			this.ITCertificatePanel.PerformLayout();
			this.CertificateLoaderUserControl.ResumeLayout(true);
			this.CertificateLoaderUserControl.PerformLayout();
			this.CertificateExpiryDateEdit.ResumeLayout(true);
			this.CertificateExpiryDateEdit.PerformLayout();
			this.XadesCertificateGroupBox.ResumeLayout(false);
			this.XadesCertificateGroupBox.PerformLayout();
			this.XadesCertificatePanel.ResumeLayout(false);
			this.XadesCertificatePanel.PerformLayout();
			this.XadesCertificateChipsetDropEdit.ResumeLayout(true);
			this.XadesCertificateChipsetDropEdit.PerformLayout();
			this.XadesCertificateCertificateAuthorityDropEdit.ResumeLayout(true);
			this.XadesCertificateCertificateAuthorityDropEdit.PerformLayout();
			this.AutomaticSignatureGroupBox.ResumeLayout(false);
			this.AutomaticSignatureGroupBox.PerformLayout();
			this.AutomaticSignatureUserControl.ResumeLayout(true);
			this.AutomaticSignatureUserControl.PerformLayout();
			this.ITCredentialsPanel.ResumeLayout(false);
			this.ITCredentialsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel ITCertificatePanel;
		Enterprise.ZArchitecture.GUI.ZPanel ITCredentialsPanel;
		Enterprise.Registry.GUI.DigitalCertificateControl_p12 CertificateLoaderUserControl;
		Enterprise.ZArchitecture.ZGrid ITAccUserGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox AccAndUserGroupIT;
		Enterprise.ZArchitecture.ZTextBox CertificatePasswordTextBox;
		Enterprise.ZArchitecture.ZTextBox CertificatePasswordStatusTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit CertificateExpiryDateEdit;
		Enterprise.ZArchitecture.GUI.ZGroupBox NodeListGroupBox;
		Enterprise.ZArchitecture.ZLabel CertificateLabel;
		Enterprise.ZArchitecture.GUI.ZGroupBox CertificateGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox XadesCertificateGroupBox;
		Enterprise.ZArchitecture.GUI.ZPanel XadesCertificatePanel;
		ZArchitecture.GUI.ZButton XadesCertificateInformationButton;
		ZArchitecture.GUI.ZButton XadesCertificateChooseButton;
		ZArchitecture.ZTextBox XadesCertificateSerialNumberTextBox;
		ZArchitecture.GUI.ZDropEdit XadesCertificateChipsetDropEdit;
		ZArchitecture.GUI.ZDropEdit XadesCertificateCertificateAuthorityDropEdit;
		ZArchitecture.GUI.ZButton AddXadesCertificateButton;
		ZArchitecture.GUI.ZButton ClearXadesCertificateButton;
		Enterprise.ZArchitecture.GUI.ZGroupBox AutomaticSignatureGroupBox;
		GlbStaffAutomaticSignatureUserControl AutomaticSignatureUserControl;
	}
}
