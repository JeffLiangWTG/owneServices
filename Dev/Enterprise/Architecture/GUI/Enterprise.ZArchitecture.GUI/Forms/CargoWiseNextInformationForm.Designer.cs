using System.Drawing;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Core.Forms
{
	partial class EnterpriseInformationCWNextForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected System.ComponentModel.IContainer components = null;

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
		protected void InitializeComponent()
		{
            this.CopyToClipboardButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.LicenceCodeLabel = new Enterprise.ZArchitecture.ZLabel();
            this.CompanyNameLabel = new Enterprise.ZArchitecture.ZLabel();
            this.pictureBox1 = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            this.CopyrightLabel = new Enterprise.ZArchitecture.ZLabel();
            this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.VersionNumberLabel = new Enterprise.ZArchitecture.ZLabel();
            this.label5 = new Enterprise.ZArchitecture.ZLabel();
            this.ReleaseLabel = new Enterprise.ZArchitecture.ZLabel();
            this.label17 = new Enterprise.ZArchitecture.ZLabel();
            this.VersionDateLabel = new Enterprise.ZArchitecture.ZLabel();
            this.label6 = new Enterprise.ZArchitecture.ZLabel();
            this.DBVersionNumber = new Enterprise.ZArchitecture.ZLabel();
            this.label8 = new Enterprise.ZArchitecture.ZLabel();
            this.ClientDocVersionLabel = new Enterprise.ZArchitecture.ZLabel();
            this.SysDocVersionLabel = new Enterprise.ZArchitecture.ZLabel();
            this.label14 = new Enterprise.ZArchitecture.ZLabel();
            this.label13 = new Enterprise.ZArchitecture.ZLabel();
            this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.SQLServerVersionLabel = new Enterprise.ZArchitecture.ZLabel();
            this.label3 = new Enterprise.ZArchitecture.ZLabel();
            this.DbSecurityDataLabel = new Enterprise.ZArchitecture.ZLabel();
            this.DbSecurityLabel = new Enterprise.ZArchitecture.ZLabel();
            this.DBServerLabel = new Enterprise.ZArchitecture.ZLabel();
            this.label9 = new Enterprise.ZArchitecture.ZLabel();
            this.DBNameLabel = new Enterprise.ZArchitecture.ZLabel();
            this.label4 = new Enterprise.ZArchitecture.ZLabel();
            this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.TerminalServerModeLabel = new Enterprise.ZArchitecture.ZLabel();
            this.label12 = new Enterprise.ZArchitecture.ZLabel();
            this.FrameworkVersionLabel = new Enterprise.ZArchitecture.ZLabel();
            this.label15 = new Enterprise.ZArchitecture.ZLabel();
            this.SystemTypeLabel = new Enterprise.ZArchitecture.ZLabel();
            this.SystemTypeDataLabel = new Enterprise.ZArchitecture.ZLabel();
            this.label11 = new Enterprise.ZArchitecture.ZLabel();
            this.CountryLabel = new Enterprise.ZArchitecture.ZLabel();
            this.AzureApplicationClientIdLabel = new Enterprise.ZArchitecture.ZLabel();
            this.label16 = new Enterprise.ZArchitecture.ZLabel();
            this.clientIPAddressValueLabel = new Enterprise.ZArchitecture.ZLabel();
            this.clientIPAddressHeadingLabel = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.zGroupBox1.SuspendLayout();
            this.zGroupBox2.SuspendLayout();
            this.zGroupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // CopyToClipboardButton
            // 
            this.CopyToClipboardButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.CopyToClipboardButton.IsCaptionOverridden = true;
            this.CopyToClipboardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(603, 455, true);
            this.CopyToClipboardButton.Name = "CopyToClipboardButton";
            this.CopyToClipboardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
            this.CopyToClipboardButton.TabIndex = 9;
            this.CopyToClipboardButton.Text = "Copy Info";
            this.CopyToClipboardButton.ToolTipCaption = null;
            this.CopyToClipboardButton.UseVisualStyleBackColor = true;
            this.CopyToClipboardButton.Click += new System.EventHandler(this.CopyToClipboardButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseButton.IsCaptionOverridden = true;
            this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(679, 455, true);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 20, true);
            this.CloseButton.TabIndex = 10;
            this.CloseButton.Text = "Close";
            this.CloseButton.ToolTipCaption = null;
            this.CloseButton.UseVisualStyleBackColor = true;
            // 
            // LicenceCodeLabel
            // 
            this.LicenceCodeLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LicenceCodeLabel.BackColor = System.Drawing.Color.Transparent;
            this.LicenceCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.LicenceCodeLabel.ForeColor = System.Drawing.Color.Black;
            this.LicenceCodeLabel.IsFontBold = true;
            this.LicenceCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(601, 436, true);
            this.LicenceCodeLabel.Name = "LicenceCodeLabel";
            this.LicenceCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
            this.LicenceCodeLabel.TabIndex = 40;
            this.LicenceCodeLabel.Text = "LicenceCode";
            this.LicenceCodeLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.LicenceCodeLabel.UseMnemonic = false;
            // 
            // CompanyNameLabel
            // 
            this.CompanyNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.CompanyNameLabel.BackColor = System.Drawing.Color.Transparent;
            this.CompanyNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.CompanyNameLabel.ForeColor = System.Drawing.Color.Black;
            this.CompanyNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(285, 436, true);
            this.CompanyNameLabel.Name = "CompanyNameLabel";
            this.CompanyNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 30, true);
            this.CompanyNameLabel.TabIndex = 38;
            this.CompanyNameLabel.Text = "Company Name";
            this.CompanyNameLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.CompanyNameLabel.UseMnemonic = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Enterprise.ZArchitecture.GUI.Properties.Resources.AboutNextLogo;
            this.pictureBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 14, true);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 55, true);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 46;
            this.pictureBox1.TabStop = false;
            // 
            // CopyrightLabel
            // 
            this.CopyrightLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.CopyrightLabel.AutoSize = true;
            this.CopyrightLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.CopyrightLabel.IsFontBold = true;
            this.CopyrightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 436, true);
            this.CopyrightLabel.Name = "CopyrightLabel";
            this.CopyrightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 13, true);
            this.CopyrightLabel.TabIndex = 7;
            this.CopyrightLabel.Text = "CR <CompanyName>";
            this.CopyrightLabel.UseMnemonic = false;
            // 
            // zGroupBox1
            // 
            this.zGroupBox1.Controls.Add(this.VersionNumberLabel);
            this.zGroupBox1.Controls.Add(this.label5);
            this.zGroupBox1.Controls.Add(this.ReleaseLabel);
            this.zGroupBox1.Controls.Add(this.label17);
            this.zGroupBox1.Controls.Add(this.VersionDateLabel);
            this.zGroupBox1.Controls.Add(this.label6);
            this.zGroupBox1.Controls.Add(this.DBVersionNumber);
            this.zGroupBox1.Controls.Add(this.label8);
            this.zGroupBox1.Controls.Add(this.ClientDocVersionLabel);
            this.zGroupBox1.Controls.Add(this.SysDocVersionLabel);
            this.zGroupBox1.Controls.Add(this.label14);
            this.zGroupBox1.Controls.Add(this.label13);
            this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 100, true);
            this.zGroupBox1.Name = "zGroupBox1";
            this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 320, true);
            this.zGroupBox1.TabIndex = 47;
            this.zGroupBox1.TabStop = false;
            this.zGroupBox1.Text = "Application Version";
            // 
            // VersionNumberLabel
            // 
            this.VersionNumberLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.VersionNumberLabel.AutoSize = true;
            this.VersionNumberLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.VersionNumberLabel.ForeColor = System.Drawing.Color.Black;
            this.VersionNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 36, true);
            this.VersionNumberLabel.Name = "VersionNumberLabel";
            this.VersionNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 13, true);
            this.VersionNumberLabel.TabIndex = 1;
            this.VersionNumberLabel.Text = "VersionNumber";
            this.VersionNumberLabel.UseMnemonic = false;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label5.AutoSize = true;
            this.label5.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.label5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 17, true);
            this.label5.Name = "label5";
            this.label5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 13, true);
            this.label5.TabIndex = 0;
            this.label5.Text = "Version Number:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label5.UseMnemonic = false;
            // 
            // ReleaseLabel
            // 
            this.ReleaseLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.ReleaseLabel.AutoSize = true;
            this.ReleaseLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.ReleaseLabel.ForeColor = System.Drawing.Color.Black;
            this.ReleaseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 75, true);
            this.ReleaseLabel.Name = "ReleaseLabel";
            this.ReleaseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 13, true);
            this.ReleaseLabel.TabIndex = 25;
            this.ReleaseLabel.Text = "Release";
            this.ReleaseLabel.UseMnemonic = false;
            this.ReleaseLabel.Visible = false;
            // 
            // label17
            // 
            this.label17.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label17.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.label17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 56, true);
            this.label17.Name = "label17";
            this.label17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 14, true);
            this.label17.TabIndex = 24;
            this.label17.Text = "Release:";
            this.label17.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label17.UseMnemonic = false;
            this.label17.Visible = false;
            // 
            // VersionDateLabel
            // 
            this.VersionDateLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.VersionDateLabel.AutoSize = true;
            this.VersionDateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.VersionDateLabel.ForeColor = System.Drawing.Color.Black;
            this.VersionDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 75, true);
            this.VersionDateLabel.Name = "VersionDateLabel";
            this.VersionDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 13, true);
            this.VersionDateLabel.TabIndex = 3;
            this.VersionDateLabel.Text = "VersionDate";
            this.VersionDateLabel.UseMnemonic = false;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label6.AutoSize = true;
            this.label6.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.label6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 56, true);
            this.label6.Name = "label6";
            this.label6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 13, true);
            this.label6.TabIndex = 2;
            this.label6.Text = "Executable Date:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label6.UseMnemonic = false;
            // 
            // DBVersionNumber
            // 
            this.DBVersionNumber.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.DBVersionNumber.AutoSize = true;
            this.DBVersionNumber.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.DBVersionNumber.ForeColor = System.Drawing.Color.Black;
            this.DBVersionNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 115, true);
            this.DBVersionNumber.Name = "DBVersionNumber";
            this.DBVersionNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 67, true);
            this.DBVersionNumber.TabIndex = 5;
            this.DBVersionNumber.Text = "Schema = 0000.0\r\nScript = 000.0\r\nData = 0000\r\nTransformation = 000.0\r\nCLR Assembl" +
    "ies = 000.0";
            this.DBVersionNumber.UseMnemonic = false;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label8.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.label8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 96, true);
            this.label8.Name = "label8";
            this.label8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 14, true);
            this.label8.TabIndex = 4;
            this.label8.Text = "Database Version:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label8.UseMnemonic = false;
            // 
            // ClientDocVersionLabel
            // 
            this.ClientDocVersionLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.ClientDocVersionLabel.AutoSize = true;
            this.ClientDocVersionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.ClientDocVersionLabel.ForeColor = System.Drawing.Color.Black;
            this.ClientDocVersionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 255, true);
            this.ClientDocVersionLabel.Name = "ClientDocVersionLabel";
            this.ClientDocVersionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 13, true);
            this.ClientDocVersionLabel.TabIndex = 23;
            this.ClientDocVersionLabel.Text = "ClientDocVersionAndName";
            this.ClientDocVersionLabel.UseMnemonic = false;
            // 
            // SysDocVersionLabel
            // 
            this.SysDocVersionLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.SysDocVersionLabel.AutoSize = true;
            this.SysDocVersionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.SysDocVersionLabel.ForeColor = System.Drawing.Color.Black;
            this.SysDocVersionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 215, true);
            this.SysDocVersionLabel.Name = "SysDocVersionLabel";
            this.SysDocVersionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 13, true);
            this.SysDocVersionLabel.TabIndex = 21;
            this.SysDocVersionLabel.Text = "SysDocVersion";
            this.SysDocVersionLabel.UseMnemonic = false;
            // 
            // label14
            // 
            this.label14.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label14.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.label14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 236, true);
            this.label14.Name = "label14";
            this.label14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 14, true);
            this.label14.TabIndex = 22;
            this.label14.Text = "Client Documents:";
            this.label14.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label14.UseMnemonic = false;
            // 
            // label13
            // 
            this.label13.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label13.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.label13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 195, true);
            this.label13.Name = "label13";
            this.label13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 14, true);
            this.label13.TabIndex = 20;
            this.label13.Text = "System Documents:";
            this.label13.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label13.UseMnemonic = false;
            // 
            // zGroupBox2
            // 
            this.zGroupBox2.Controls.Add(this.SQLServerVersionLabel);
            this.zGroupBox2.Controls.Add(this.label3);
            this.zGroupBox2.Controls.Add(this.DbSecurityDataLabel);
            this.zGroupBox2.Controls.Add(this.DbSecurityLabel);
            this.zGroupBox2.Controls.Add(this.DBServerLabel);
            this.zGroupBox2.Controls.Add(this.label9);
            this.zGroupBox2.Controls.Add(this.DBNameLabel);
            this.zGroupBox2.Controls.Add(this.label4);
            this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 100, true);
            this.zGroupBox2.Name = "zGroupBox2";
            this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 320, true);
            this.zGroupBox2.TabIndex = 48;
            this.zGroupBox2.TabStop = false;
            this.zGroupBox2.Text = "Database Information";
            // 
            // SQLServerVersionLabel
            // 
            this.SQLServerVersionLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.SQLServerVersionLabel.AutoSize = true;
            this.SQLServerVersionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.SQLServerVersionLabel.ForeColor = System.Drawing.Color.Black;
            this.SQLServerVersionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 158, true);
            this.SQLServerVersionLabel.Name = "SQLServerVersionLabel";
            this.SQLServerVersionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 40, true);
            this.SQLServerVersionLabel.TabIndex = 27;
            this.SQLServerVersionLabel.Text = "10.00.2531.00 (x64)\r\nSQL Server 2008\r\nDeveloper Edition (64-bit)";
            this.SQLServerVersionLabel.UseMnemonic = false;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.label3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 139, true);
            this.label3.Name = "label3";
            this.label3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 14, true);
            this.label3.TabIndex = 26;
            this.label3.Text = "SQL Server Version:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label3.UseMnemonic = false;
            // 
            // DbSecurityDataLabel
            // 
            this.DbSecurityDataLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.DbSecurityDataLabel.AutoSize = true;
            this.DbSecurityDataLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.DbSecurityDataLabel.ForeColor = System.Drawing.Color.Black;
            this.DbSecurityDataLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 118, true);
            this.DbSecurityDataLabel.Name = "DbSecurityDataLabel";
            this.DbSecurityDataLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
            this.DbSecurityDataLabel.TabIndex = 29;
            this.DbSecurityDataLabel.Text = "Locked/Open";
            this.DbSecurityDataLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.DbSecurityDataLabel.UseMnemonic = false;
            // 
            // DbSecurityLabel
            // 
            this.DbSecurityLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.DbSecurityLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.DbSecurityLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.DbSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 99, true);
            this.DbSecurityLabel.Name = "DbSecurityLabel";
            this.DbSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 14, true);
            this.DbSecurityLabel.TabIndex = 28;
            this.DbSecurityLabel.Text = "Database Security:";
            this.DbSecurityLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.DbSecurityLabel.UseMnemonic = false;
            // 
            // DBServerLabel
            // 
            this.DBServerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.DBServerLabel.AutoSize = true;
            this.DBServerLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.DBServerLabel.ForeColor = System.Drawing.Color.Black;
            this.DBServerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 37, true);
            this.DBServerLabel.Name = "DBServerLabel";
            this.DBServerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 13, true);
            this.DBServerLabel.TabIndex = 9;
            this.DBServerLabel.Text = "DBServerName";
            this.DBServerLabel.UseMnemonic = false;
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label9.AutoSize = true;
            this.label9.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.label9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 18, true);
            this.label9.Name = "label9";
            this.label9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 13, true);
            this.label9.TabIndex = 8;
            this.label9.Text = "Database Server:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label9.UseMnemonic = false;
            // 
            // DBNameLabel
            // 
            this.DBNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.DBNameLabel.AutoSize = true;
            this.DBNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.DBNameLabel.ForeColor = System.Drawing.Color.Black;
            this.DBNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 78, true);
            this.DBNameLabel.Name = "DBNameLabel";
            this.DBNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 13, true);
            this.DBNameLabel.TabIndex = 13;
            this.DBNameLabel.Text = "DBDatabaseName";
            this.DBNameLabel.UseMnemonic = false;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label4.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.label4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 58, true);
            this.label4.Name = "label4";
            this.label4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 14, true);
            this.label4.TabIndex = 12;
            this.label4.Text = "Database Name:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label4.UseMnemonic = false;
            // 
            // zGroupBox3
            // 
            this.zGroupBox3.Controls.Add(this.clientIPAddressValueLabel);
            this.zGroupBox3.Controls.Add(this.clientIPAddressHeadingLabel);
            this.zGroupBox3.Controls.Add(this.TerminalServerModeLabel);
            this.zGroupBox3.Controls.Add(this.label12);
            this.zGroupBox3.Controls.Add(this.FrameworkVersionLabel);
            this.zGroupBox3.Controls.Add(this.label15);
            this.zGroupBox3.Controls.Add(this.SystemTypeLabel);
            this.zGroupBox3.Controls.Add(this.SystemTypeDataLabel);
            this.zGroupBox3.Controls.Add(this.label11);
            this.zGroupBox3.Controls.Add(this.CountryLabel);
            this.zGroupBox3.Controls.Add(this.AzureApplicationClientIdLabel);
            this.zGroupBox3.Controls.Add(this.label16);
            this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 100, true);
            this.zGroupBox3.Name = "zGroupBox3";
            this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 320, true);
            this.zGroupBox3.TabIndex = 49;
            this.zGroupBox3.TabStop = false;
            this.zGroupBox3.Text = "System Information";
            // 
            // TerminalServerModeLabel
            // 
            this.TerminalServerModeLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.TerminalServerModeLabel.AutoSize = true;
            this.TerminalServerModeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.TerminalServerModeLabel.ForeColor = System.Drawing.Color.Black;
            this.TerminalServerModeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 157, true);
            this.TerminalServerModeLabel.Name = "TerminalServerModeLabel";
            this.TerminalServerModeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
            this.TerminalServerModeLabel.TabIndex = 15;
            this.TerminalServerModeLabel.Text = "Yes/No";
            this.TerminalServerModeLabel.UseMnemonic = false;
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label12.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.label12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 138, true);
            this.label12.Name = "label12";
            this.label12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 14, true);
            this.label12.TabIndex = 14;
            this.label12.Text = "Terminal Server:";
            this.label12.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label12.UseMnemonic = false;
            // 
            // FrameworkVersionLabel
            // 
            this.FrameworkVersionLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.FrameworkVersionLabel.AutoSize = true;
            this.FrameworkVersionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.FrameworkVersionLabel.ForeColor = System.Drawing.Color.Black;
            this.FrameworkVersionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 117, true);
            this.FrameworkVersionLabel.Name = "FrameworkVersionLabel";
            this.FrameworkVersionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 13, true);
            this.FrameworkVersionLabel.TabIndex = 17;
            this.FrameworkVersionLabel.Text = "FrameworkVersion";
            this.FrameworkVersionLabel.UseMnemonic = false;
            // 
            // label15
            // 
            this.label15.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label15.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.label15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 98, true);
            this.label15.Name = "label15";
            this.label15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 14, true);
            this.label15.TabIndex = 16;
            this.label15.Text = ".NET Framework:";
            this.label15.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label15.UseMnemonic = false;
            // 
            // SystemTypeLabel
            // 
            this.SystemTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.SystemTypeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.SystemTypeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.SystemTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 18, true);
            this.SystemTypeLabel.Name = "SystemTypeLabel";
            this.SystemTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 14, true);
            this.SystemTypeLabel.TabIndex = 10;
            this.SystemTypeLabel.Text = "System Type:";
            this.SystemTypeLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.SystemTypeLabel.UseMnemonic = false;
            // 
            // SystemTypeDataLabel
            // 
            this.SystemTypeDataLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.SystemTypeDataLabel.AutoSize = true;
            this.SystemTypeDataLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.SystemTypeDataLabel.ForeColor = System.Drawing.Color.Black;
            this.SystemTypeDataLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 37, true);
            this.SystemTypeDataLabel.Name = "SystemTypeDataLabel";
            this.SystemTypeDataLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 13, true);
            this.SystemTypeDataLabel.TabIndex = 11;
            this.SystemTypeDataLabel.Text = "SystemType (PRD/TST)";
            this.SystemTypeDataLabel.UseMnemonic = false;
            // 
            // label11
            // 
            this.label11.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label11.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.label11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 58, true);
            this.label11.Name = "label11";
            this.label11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 14, true);
            this.label11.TabIndex = 18;
            this.label11.Text = "Country/Region:";
            this.label11.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label11.UseMnemonic = false;
            // 
            // CountryLabel
            // 
            this.CountryLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.CountryLabel.AutoSize = true;
            this.CountryLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.CountryLabel.ForeColor = System.Drawing.Color.Black;
            this.CountryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 77, true);
            this.CountryLabel.Name = "CountryLabel";
            this.CountryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 13, true);
            this.CountryLabel.TabIndex = 19;
            this.CountryLabel.Text = "Country Country Country Country";
            this.CountryLabel.UseMnemonic = false;
            // 
            // AzureApplicationClientIdLabel
            // 
            this.AzureApplicationClientIdLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.AzureApplicationClientIdLabel.AutoSize = true;
            this.AzureApplicationClientIdLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.AzureApplicationClientIdLabel.ForeColor = System.Drawing.Color.Black;
            this.AzureApplicationClientIdLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 197, true);
            this.AzureApplicationClientIdLabel.Name = "AzureApplicationClientIdLabel";
            this.AzureApplicationClientIdLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.AzureApplicationClientIdLabel.TabIndex = 31;
            this.AzureApplicationClientIdLabel.UseMnemonic = false;
            // 
            // label16
            // 
            this.label16.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label16.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.label16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.label16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 178, true);
            this.label16.Name = "label16";
            this.label16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 14, true);
            this.label16.TabIndex = 30;
            this.label16.Text = "Application Id:";
            this.label16.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label16.UseMnemonic = false;
            // 
            // clientIPAddressValueLabel
            // 
            this.clientIPAddressValueLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.clientIPAddressValueLabel.AutoSize = true;
            this.clientIPAddressValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.clientIPAddressValueLabel.ForeColor = System.Drawing.Color.Black;
            this.clientIPAddressValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 233, true);
            this.clientIPAddressValueLabel.Name = "clientIPAddressValueLabel";
            this.clientIPAddressValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.clientIPAddressValueLabel.TabIndex = 33;
            this.clientIPAddressValueLabel.UseMnemonic = false;
            // 
            // clientIPAddressHeadingLabel
            // 
            this.clientIPAddressHeadingLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.clientIPAddressHeadingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.clientIPAddressHeadingLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(144)))), ((int)(((byte)(157)))));
            this.clientIPAddressHeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 215, true);
            this.clientIPAddressHeadingLabel.Name = "clientIPAddressHeadingLabel";
            this.clientIPAddressHeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 14, true);
            this.clientIPAddressHeadingLabel.TabIndex = 32;
            this.clientIPAddressHeadingLabel.Text = "Client IP Address:";
            this.clientIPAddressHeadingLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.clientIPAddressHeadingLabel.UseMnemonic = false;
            // 
            // EnterpriseInformationCWNextForm
            // 
            this.AcceptButton = this.CloseButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CancelButton = this.CloseButton;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 480, true);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.CopyToClipboardButton);
            this.Controls.Add(this.zGroupBox3);
            this.Controls.Add(this.CopyrightLabel);
            this.Controls.Add(this.zGroupBox2);
            this.Controls.Add(this.zGroupBox1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.LicenceCodeLabel);
            this.Controls.Add(this.CompanyNameLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EnterpriseInformationCWNextForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.zGroupBox1.ResumeLayout(false);
            this.zGroupBox1.PerformLayout();
            this.zGroupBox2.ResumeLayout(false);
            this.zGroupBox2.PerformLayout();
            this.zGroupBox3.ResumeLayout(false);
            this.zGroupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private Enterprise.ZArchitecture.ZLabel label18;
		private Enterprise.ZArchitecture.GUI.ZButton CopyToClipboardButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.ZArchitecture.ZLabel ReleaseLabel;
		private Enterprise.ZArchitecture.ZLabel label17;
		internal Enterprise.ZArchitecture.ZLabel FrameworkVersionLabel;
		private Enterprise.ZArchitecture.ZLabel label15;
		internal Enterprise.ZArchitecture.ZLabel DBNameLabel;
		private Enterprise.ZArchitecture.ZLabel label4;
		internal Enterprise.ZArchitecture.ZLabel ClientDocVersionLabel;
		private Enterprise.ZArchitecture.ZLabel label14;
		internal Enterprise.ZArchitecture.ZLabel SysDocVersionLabel;
		private Enterprise.ZArchitecture.ZLabel label13;
		internal Enterprise.ZArchitecture.ZLabel CountryLabel;
		private Enterprise.ZArchitecture.ZLabel label11;
		internal Enterprise.ZArchitecture.ZLabel SystemTypeDataLabel;
		private Enterprise.ZArchitecture.ZLabel SystemTypeLabel;
		internal Enterprise.ZArchitecture.ZLabel DBServerLabel;
		private Enterprise.ZArchitecture.ZLabel label9;
		internal Enterprise.ZArchitecture.ZLabel DBVersionNumber;
		private Enterprise.ZArchitecture.ZLabel label8;
		internal Enterprise.ZArchitecture.ZLabel VersionDateLabel;
		private Enterprise.ZArchitecture.ZLabel label6;
		internal Enterprise.ZArchitecture.ZLabel VersionNumberLabel;
		private Enterprise.ZArchitecture.ZLabel label5;
		internal Enterprise.ZArchitecture.ZLabel LicenceCodeLabel;
		internal Enterprise.ZArchitecture.ZLabel CompanyNameLabel;
		private Enterprise.ZArchitecture.ZLabel label3;
		internal Enterprise.ZArchitecture.ZLabel SQLServerVersionLabel;
		internal Enterprise.ZArchitecture.ZLabel DbSecurityDataLabel;
		private Enterprise.ZArchitecture.ZLabel DbSecurityLabel;
		private Enterprise.ZArchitecture.GUI.ZPictureBox pictureBox1;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZGroupBox zGroupBox2;
		private ZArchitecture.GUI.ZGroupBox zGroupBox3;
		internal Enterprise.ZArchitecture.ZLabel TerminalServerModeLabel;
		private Enterprise.ZArchitecture.ZLabel label12;
		internal Enterprise.ZArchitecture.ZLabel CopyrightLabel;
		private Enterprise.ZArchitecture.ZLabel label16;
		internal Enterprise.ZArchitecture.ZLabel AzureApplicationClientIdLabel;
		internal ZArchitecture.ZLabel clientIPAddressValueLabel;
		private ZArchitecture.ZLabel clientIPAddressHeadingLabel;
	}
}
