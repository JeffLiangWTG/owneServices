namespace Enterprise.DocumentEngine.GUI.Registry
{
	partial class DigitalSignatureRegistryControl
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

			if (fileDialog != null)
			{
				fileDialog.Dispose();
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
			this.certificateNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.chooseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.signatureDetails = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.signatureDetailsName = new Enterprise.ZArchitecture.ZTextBox();
			this.signatureDetailsLocation = new Enterprise.ZArchitecture.ZTextBox();
			this.signatureDetailsReason = new Enterprise.ZArchitecture.ZTextBox();
			this.fileDialog = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			this.certificatePasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.signatureDetails.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.Registry.DigitalSignatureRegistry);
			// 
			// certificateNameTextBox
			// 
			this.certificateNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.certificateNameTextBox, "CertificateFileName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.DigitalSignatureRegistry)(null)).CertificateFileName)));
			this.certificateNameTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DigitalSignatureRegistryControl|655c260d-db63-46e3-aa19-df6f7e9c5c85", "Certificate file");
			this.certificateNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.certificateNameTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.certificateNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 20, true);
			this.certificateNameTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 3, true);
			this.certificateNameTextBox.Name = "certificateNameTextBox";
			this.certificateNameTextBox.ReadOnly = true;
			this.certificateNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 20, true);
			this.certificateNameTextBox.TabIndex = 0;
			// 
			// chooseButton
			// 
			this.chooseButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DigitalSignatureRegistryControl|5bb1f95e-9e60-4b95-af6a-68551428071d", "Choose...");
			this.chooseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 46, true);
			this.chooseButton.Name = "chooseButton";
			this.chooseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.chooseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.chooseButton.TabIndex = 1;
			this.chooseButton.ToolTipCaption = null;
			this.chooseButton.UseVisualStyleBackColor = true;
			this.chooseButton.Click += new System.EventHandler(this.HandleChooseButtonClick);
			// 
			// signatureDetails
			// 
			this.signatureDetails.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DigitalSignatureRegistryControl|dd6ca24d-d618-41fc-8d6d-6ce2d2fb18db", "Signature details");
			this.signatureDetails.Controls.Add(this.signatureDetailsName);
			this.signatureDetails.Controls.Add(this.signatureDetailsLocation);
			this.signatureDetails.Controls.Add(this.signatureDetailsReason);
			this.signatureDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 127, true);
			this.signatureDetails.Name = "signatureDetails";
			this.signatureDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 165, true);
			this.signatureDetails.TabIndex = 7;
			this.signatureDetails.TabStop = false;
			// 
			// signatureDetailsName
			// 
			this.signatureDetailsName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.signatureDetailsName, "SignatureDetailsName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.DigitalSignatureRegistry)(null)).SignatureDetailsName)));
			this.signatureDetailsName.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DigitalSignatureRegistryControl|6a66e5b0-17d1-407c-b924-d008a5118a14", "Name");
			this.signatureDetailsName.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.signatureDetailsName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 34, true);
			this.signatureDetailsName.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 3, true);
			this.signatureDetailsName.Name = "signatureDetailsName";
			this.signatureDetailsName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.signatureDetailsName.TabIndex = 3;
			// 
			// signatureDetailsLocation
			// 
			this.signatureDetailsLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.signatureDetailsLocation, "SignatureDetailsLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.DigitalSignatureRegistry)(null)).SignatureDetailsLocation)));
			this.signatureDetailsLocation.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DigitalSignatureRegistryControl|cc1523d5-0ccb-42d9-8ef9-7e0ce7e76d97", "Location");
			this.signatureDetailsLocation.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.signatureDetailsLocation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 115, true);
			this.signatureDetailsLocation.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 3, true);
			this.signatureDetailsLocation.Name = "signatureDetailsLocation";
			this.signatureDetailsLocation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.signatureDetailsLocation.TabIndex = 5;
			// 
			// signatureDetailsReason
			// 
			this.signatureDetailsReason.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.signatureDetailsReason, "SignatureDetailsEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.DigitalSignatureRegistry)(null)).SignatureDetailsEmail)));
			this.signatureDetailsReason.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DigitalSignatureRegistryControl|d06f57bb-10a7-454a-960e-ddac77a195fc", "Email");
			this.signatureDetailsReason.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.signatureDetailsReason.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 75, true);
			this.signatureDetailsReason.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 3, true);
			this.signatureDetailsReason.Name = "signatureDetailsReason";
			this.signatureDetailsReason.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 20, true);
			this.signatureDetailsReason.TabIndex = 4;
			// 
			// fileDialog
			// 
			this.fileDialog.AddExtension = true;
			this.fileDialog.CheckFileExists = true;
			this.fileDialog.CheckPathExists = true;
			this.fileDialog.DefaultExt = "";
			this.fileDialog.DereferenceLinks = true;
			this.fileDialog.Filter = "";
			this.fileDialog.FilterIndex = 1;
			this.fileDialog.InitialDirectory = "";
			this.fileDialog.Multiselect = false;
			this.fileDialog.ReadOnlyChecked = false;
			this.fileDialog.RestoreDirectory = false;
			this.fileDialog.ShowHelp = false;
			this.fileDialog.SupportMultiDottedExtensions = false;
			this.fileDialog.Title = "";
			this.fileDialog.ValidateNames = true;
			this.fileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.FileDialog_FileOk);
			// 
			// certificatePasswordTextBox
			// 
			this.certificatePasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.certificatePasswordTextBox, "CertificatePassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.DigitalSignatureRegistry)(null)).CertificatePassword)));
			this.certificatePasswordTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DigitalSignatureRegistryControl|dd9f4a03-0093-4535-93c5-3f83cc8ebf6a", "Certificate password");
			this.certificatePasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.certificatePasswordTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.certificatePasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 93, true);
			this.certificatePasswordTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 3, true);
			this.certificatePasswordTextBox.Name = "certificatePasswordTextBox";
			this.certificatePasswordTextBox.PasswordChar = '*';
			this.certificatePasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 20, true);
			this.certificatePasswordTextBox.TabIndex = 2;
			// 
			// DigitalSignatureRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.certificatePasswordTextBox);
			this.Controls.Add(this.signatureDetails);
			this.Controls.Add(this.chooseButton);
			this.Controls.Add(this.certificateNameTextBox);
			this.Name = "DigitalSignatureRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 311, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.signatureDetails.ResumeLayout(false);
			this.signatureDetails.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox certificateNameTextBox;
		private ZArchitecture.ZTextBox certificatePasswordTextBox;
		private ZArchitecture.GUI.ZGroupBox signatureDetails;
		private ZArchitecture.GUI.ZButton chooseButton;
		private ZArchitecture.GUI.ZOpenFileDialog fileDialog;
		private ZArchitecture.ZTextBox signatureDetailsName;
		private ZArchitecture.ZTextBox signatureDetailsLocation;
		private ZArchitecture.ZTextBox signatureDetailsReason;
	}
}
