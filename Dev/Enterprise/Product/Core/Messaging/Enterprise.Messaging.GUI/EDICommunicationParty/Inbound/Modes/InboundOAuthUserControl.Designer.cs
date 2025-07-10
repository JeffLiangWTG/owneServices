using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	partial class InboundOAuthUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ClientIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorityUrlTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateRequestFileTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OpenFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.VerifyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Tick = new Enterprise.ZArchitecture.ZLabel();
			this.RegisterCertificateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ScopeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.awaitingCertificateGenerationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CertificateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RefreshCertificatesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DownloadCertificateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CertificatesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.IsSelfManagedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CertificateGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CertificatesGrid)).BeginInit();
			this.CertificatesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EDICommunicationParty);
			// 
			// IsSelfManagedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsSelfManagedCheckBox, "InboundConfig.ECC_IsSelfManaged");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).InboundConfig.ECC_IsSelfManaged)));
			this.IsSelfManagedCheckBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("623d54c9-a655-4250-8ff8-1d84a00bbd10", "Self-Managed OAuth Identity Provider");
			this.IsSelfManagedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 3, true);
			this.IsSelfManagedCheckBox.Name = "IsSelfManagedCheckBox";
			this.IsSelfManagedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 15, true);
			this.IsSelfManagedCheckBox.TabIndex = 3;
			this.IsSelfManagedCheckBox.Text = "Self-Managed OAuth Identity Provider";
			this.IsSelfManagedCheckBox.UseVisualStyleBackColor = true;
			this.IsSelfManagedCheckBox.CheckedChanged += new System.EventHandler(this.IsSelfManagedCheckBox_OnCheckedChanged);
			// 
			// ClientIdTextBox
			//
			this.ClientIdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ClientIdTextBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 15, true);
			this.BindingSource.SetBindingMember(this.ClientIdTextBox, "InboundConfig.Auth.ECA_ClientID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).InboundConfig.Auth.ECA_ClientID)));
			this.ClientIdTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("3e036f08-34a9-467d-b8f4-099786df171e", "OAuth Client ID");
			this.ClientIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 53, true);
			this.ClientIdTextBox.Name = "ClientIdTextBox";
			this.ClientIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 15, true);
			this.ClientIdTextBox.ReadOnly = true;
			this.ClientIdTextBox.TabIndex = 6;
			// 
			// AuthorityUrlTextBox
			//
			this.AuthorityUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AuthorityUrlTextBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 15, true);
			this.BindingSource.SetBindingMember(this.AuthorityUrlTextBox, "InboundConfig.Auth.ECA_AuthorizationEndpoint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).InboundConfig.Auth.ECA_AuthorizationEndpoint)));
			this.AuthorityUrlTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("f12864b9-bc98-41c9-9147-600389975b03", "OAuth Authority URL");
			this.AuthorityUrlTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AuthorityUrlTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 28, true);
			this.AuthorityUrlTextBox.Name = "AuthorityUrlTextBox";
			this.AuthorityUrlTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 15, true);
			this.AuthorityUrlTextBox.ReadOnly = true;
			this.AuthorityUrlTextBox.TabIndex = 5;
			// 
			// ScopeTextBox
			//
			this.ScopeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ScopeTextBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 15, true);
			this.BindingSource.SetBindingMember(this.ScopeTextBox, "InboundConfig.Auth.InboundScope");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).InboundConfig.Auth.InboundScope)));
			this.ScopeTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("2c25a14c-e3a8-4284-9526-ebd52d4d7017", "Scope");
			this.ScopeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ScopeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 78, true);
			this.ScopeTextBox.Name = "ScopeTextBox";
			this.ScopeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 15, true);
			this.ScopeTextBox.ReadOnly = true;
			this.ScopeTextBox.TabIndex = 11;
			// 
			// awaitingCertificateGenerationLabel
			// 
			this.awaitingCertificateGenerationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Bolded | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.awaitingCertificateGenerationLabel.ForeColor = System.Drawing.Color.Green;
			this.awaitingCertificateGenerationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 30, true);
			this.awaitingCertificateGenerationLabel.Name = "awaitingCertificateGenerationLabel";
			this.awaitingCertificateGenerationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 15, true);
			this.awaitingCertificateGenerationLabel.TabIndex = 4;
			this.awaitingCertificateGenerationLabel.Text = "Awaiting generation of certificate.";
			this.awaitingCertificateGenerationLabel.UseMnemonic = false;
			this.awaitingCertificateGenerationLabel.Visible = false;
			// 
			// CertificateRequestFileTextBox
			// 
			this.CertificateRequestFileTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("3e036f08-34a9-467d-b8f4-099786df171e", "Certificate Request File");
			this.CertificateRequestFileTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificateRequestFileTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 48, true);
			this.CertificateRequestFileTextBox.Name = "CertificateRequestFileTextBox";
			this.CertificateRequestFileTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 15, true);
			this.CertificateRequestFileTextBox.TabIndex = 5;
			// 
			// OpenFileButton
			// 
			this.OpenFileButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("c0ed806c-a03c-41bc-853e-51ea10fe6ca1", "...", "Open a CSR file.");
			this.OpenFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 48, true);
			this.OpenFileButton.Name = "OpenFileButton";
			this.OpenFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 23, true);
			this.OpenFileButton.TabIndex = 6;
			this.OpenFileButton.ToolTipCaption = null;
			this.OpenFileButton.UseVisualStyleBackColor = true;
			this.OpenFileButton.Click += new System.EventHandler(this.OpenFileButton_Click);
			// 
			// VerifyButton
			// 
			this.VerifyButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("43133a4b-18e4-4aad-b958-5edef6c6b507", "Verify");
			this.VerifyButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.VerifyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 48, true);
			this.VerifyButton.Name = "VerifyButton";
			this.VerifyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 23, true);
			this.VerifyButton.TabIndex = 7;
			this.VerifyButton.ToolTipCaption = null;
			this.VerifyButton.UseVisualStyleBackColor = true;
			this.VerifyButton.Click += new System.EventHandler(this.VerifyButton_Click);
			// 
			// Tick
			// 
			this.Tick.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Largest;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Tick, false);
			this.Tick.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 48, true);
			this.Tick.Name = "Tick";
			this.Tick.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 23, true);
			this.Tick.TabIndex = 8;
			this.Tick.UseMnemonic = false;
			// 
			// RegisterCertificateButton
			// 
			this.RegisterCertificateButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("0e05947c-61b3-42e4-86f6-2099d7013ce7", "Generate Certificate", "Generates the certificate.");
			this.RegisterCertificateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(475, 48, true);
			this.RegisterCertificateButton.Name = "RegisterCertificateButton";
			this.RegisterCertificateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 23, true);
			this.RegisterCertificateButton.TabIndex = 9;
			this.RegisterCertificateButton.ToolTipCaption = null;
			this.RegisterCertificateButton.UseVisualStyleBackColor = true;
			this.RegisterCertificateButton.Click += new System.EventHandler(this.RegisterCertificateButton_Click);
			// 
			// CertificateGroupBox
			//
			this.CertificateGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("0752c3ab-a6ab-4846-9759-0a2d5d4383b2", "Certificate List");
			this.CertificateGroupBox.Controls.Add(this.RegisterCertificateButton);
			this.CertificateGroupBox.Controls.Add(this.Tick);
			this.CertificateGroupBox.Controls.Add(this.VerifyButton);
			this.CertificateGroupBox.Controls.Add(this.OpenFileButton);
			this.CertificateGroupBox.Controls.Add(this.CertificateRequestFileTextBox);
			this.CertificateGroupBox.Controls.Add(this.RefreshCertificatesButton);
			this.CertificateGroupBox.Controls.Add(this.DownloadCertificateButton);
			this.CertificateGroupBox.Controls.Add(this.CertificatesGrid);
			this.CertificateGroupBox.Controls.Add(this.awaitingCertificateGenerationLabel);
			this.CertificateGroupBox.ImeMode = System.Windows.Forms.ImeMode.On;
			this.CertificateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 111, true);
			this.CertificateGroupBox.Name = "CertificateGroupBox";
			this.CertificateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(595, 225, true);
			this.CertificateGroupBox.TabIndex = 24;
			this.CertificateGroupBox.TabStop = false;
			// 
			// RefreshCertificatesButton
			// 
			this.RefreshCertificatesButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("0e05947c-61b3-42e4-86f6-2099d7013ce7", "Refresh Certificates", "Refresh the list of certificates.");
			this.RefreshCertificatesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 80, true);
			this.RefreshCertificatesButton.Name = "RefreshCertificatesButton";
			this.RefreshCertificatesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.RefreshCertificatesButton.TabIndex = 10;
			this.RefreshCertificatesButton.ToolTipCaption = null;
			this.RefreshCertificatesButton.UseVisualStyleBackColor = true;
			this.RefreshCertificatesButton.Click += new System.EventHandler(this.RefreshCertificatesButton_Click);
			// 
			// DownloadCertificateButton
			// 
			this.DownloadCertificateButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("f1d5db24-a14d-4ca5-9f40-3676c0fd2ad9", "Download Certificate...", "Download selected certificate.");
			this.DownloadCertificateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 80, true);
			this.DownloadCertificateButton.Name = "DownloadCertificateButton";
			this.DownloadCertificateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 23, true);
			this.DownloadCertificateButton.TabIndex = 10;
			this.DownloadCertificateButton.ToolTipCaption = null;
			this.DownloadCertificateButton.UseVisualStyleBackColor = true;
			this.DownloadCertificateButton.Click += new System.EventHandler(this.DownloadCertificateButton_Click);
			// 
			// CertificatesGrid
			// 
			this.CertificatesGrid.AllowNavigation = false;
			this.CertificatesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CertificatesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("10d3ee0d-b7b8-4821-b55a-fa5c73a3d1fb", "Common Name");
			zTextBoxColumnStyleInfo2.ColumnName = "CommonName";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("9736f48b-a2c9-4005-ad8d-c56a9aa81632", "Serial Number");
			zTextBoxColumnStyleInfo3.ColumnName = "SerialNumber";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("150f3be5-bb09-46f2-948d-4c578c7cae9c", "Issuer");
			zTextBoxColumnStyleInfo4.ColumnName = "Issuer";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("1bfeb9ee-8a51-4476-bfd3-f6f9fcb73090", "Valid From");
			zTextBoxColumnStyleInfo5.ColumnName = "ValidFrom";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("509e6f07-f49f-4bb9-98b3-12951a7d95ae", "Valid To");
			zTextBoxColumnStyleInfo6.ColumnName = "ValidTo";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("65f09e13-d418-48a2-a9ac-23ef623eb04d", "Certificate");
			zTextBoxColumnStyleInfo7.ColumnName = "CertificatePem";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.CertificatesGrid.CopySelectedRowsAllowed = false;
			this.CertificatesGrid.GridId = "2996080f-4fbf-4734-99e5-b063e2824325";
			this.CertificatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CertificatesGrid.IsWholeRowSelectedOnClick = true;
			this.CertificatesGrid.LayoutKey = "CertificatesGrid";
			this.CertificatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 110, true);
			this.CertificatesGrid.Name = "CertificatesGrid";
			this.CertificatesGrid.ReadOnly = true;
			this.CertificatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 80, true);
			this.CertificatesGrid.TabIndex = 11;
			// 
			// InboundOAuthUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IsSelfManagedCheckBox);
			this.Controls.Add(this.ClientIdTextBox);
			this.Controls.Add(this.AuthorityUrlTextBox);
			this.Controls.Add(this.ScopeTextBox);
			this.Controls.Add(this.CertificateGroupBox);
			this.Name = "InboundOAuthUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 524, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CertificateGroupBox.ResumeLayout(false);
			this.CertificateGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CertificatesGrid)).EndInit();
			this.CertificatesGrid.ResumeLayout(false);
			this.CertificatesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox ClientIdTextBox;
		private ZArchitecture.ZTextBox AuthorityUrlTextBox;
		private ZArchitecture.ZTextBox CertificateRequestFileTextBox;
		private ZArchitecture.GUI.ZButton OpenFileButton;
		private ZArchitecture.GUI.ZButton VerifyButton;
		private ZArchitecture.ZLabel Tick;
		private ZArchitecture.GUI.ZButton RegisterCertificateButton;
		private ZArchitecture.ZTextBox ScopeTextBox;
		private ZArchitecture.GUI.ZGroupBox CertificateGroupBox;
		private ZArchitecture.GUI.ZButton RefreshCertificatesButton;
		private ZArchitecture.GUI.ZButton DownloadCertificateButton;
		private ZArchitecture.ZGrid CertificatesGrid;
		private ZArchitecture.ZLabel awaitingCertificateGenerationLabel;
		private ZCheckBox IsSelfManagedCheckBox;
	}
}
