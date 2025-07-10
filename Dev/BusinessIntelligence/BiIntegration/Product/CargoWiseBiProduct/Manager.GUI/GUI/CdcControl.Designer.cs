namespace CargoWise.Bi.Product.Manager.GUI
{
	partial class CdcControl
	{
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.cdcInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.hasCdcErrorsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.lastCdcScanTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.lastTransactionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.earliestTransactionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.cdcEnabledTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.dbNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.serverTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.cdcErrorLabel = new Enterprise.ZArchitecture.ZLabel();
			this.cdcSchemaErrorTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.cdcTableCountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.refreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.copyInfoButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cdcTabControl = new System.Windows.Forms.TabControl();
			this.cdcSchemaErrorTabPage = new System.Windows.Forms.TabPage();
			this.cdcLogTabPage = new System.Windows.Forms.TabPage();
			this.cdcErrorsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.cdcInfoGroupBox.SuspendLayout();
			this.cdcTabControl.SuspendLayout();
			this.cdcSchemaErrorTabPage.SuspendLayout();
			this.cdcLogTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.cdcErrorsGrid)).BeginInit();
			this.cdcErrorsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoWise.Bi.Product.Manager.Business.CdcInformation);
			// 
			// cdcInfoGroupBox
			// 
			this.cdcInfoGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cdcInfoGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.cdcInfoGroupBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("41a6d698-ed65-4b09-8335-eeb8afde32ac", "CDC Information");
			this.cdcInfoGroupBox.Controls.Add(this.hasCdcErrorsTextBox);
			this.cdcInfoGroupBox.Controls.Add(this.lastCdcScanTextBox);
			this.cdcInfoGroupBox.Controls.Add(this.lastTransactionTextBox);
			this.cdcInfoGroupBox.Controls.Add(this.earliestTransactionTextBox);
			this.cdcInfoGroupBox.Controls.Add(this.cdcEnabledTextBox);
			this.cdcInfoGroupBox.Controls.Add(this.dbNameTextBox);
			this.cdcInfoGroupBox.Controls.Add(this.serverTextBox);
			this.cdcInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.cdcInfoGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, true);
			this.cdcInfoGroupBox.Name = "cdcInfoGroupBox";
			this.cdcInfoGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, true);
			this.cdcInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 159, true);
			this.cdcInfoGroupBox.TabIndex = 0;
			this.cdcInfoGroupBox.TabStop = false;
			// 
			// hasCdcErrorsTextBox
			// 
			this.hasCdcErrorsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.hasCdcErrorsTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.hasCdcErrorsTextBox, "HasCdcErrorsText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).HasCdcErrorsText)));
			this.hasCdcErrorsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.hasCdcErrorsTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("002e8d90-af8f-45c1-b73a-adab820e6c17", "Has CDC Errors", "Has CDC Errors during the last 32 sessions");
			this.hasCdcErrorsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.hasCdcErrorsTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.hasCdcErrorsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 134, true);
			this.hasCdcErrorsTextBox.Name = "hasCdcErrorsTextBox";
			this.hasCdcErrorsTextBox.ReadOnly = true;
			this.hasCdcErrorsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 13, true);
			this.hasCdcErrorsTextBox.TabIndex = 9;
			// 
			// lastCdcScanTextBox
			// 
			this.lastCdcScanTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lastCdcScanTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.lastCdcScanTextBox, "DateOfLastCdcScanText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).DateOfLastCdcScanText)));
			this.lastCdcScanTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.lastCdcScanTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("9f319232-a5fe-463b-870b-0f802f9190ef", "Last CDC Scan", "Date of Last CDC Scan (Server Time)");
			this.lastCdcScanTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.lastCdcScanTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.lastCdcScanTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 117, true);
			this.lastCdcScanTextBox.Name = "lastCdcScanTextBox";
			this.lastCdcScanTextBox.ReadOnly = true;
			this.lastCdcScanTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 13, true);
			this.lastCdcScanTextBox.TabIndex = 8;
			// 
			// lastTransactionTextBox
			// 
			this.lastTransactionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lastTransactionTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.lastTransactionTextBox, "DateOfLastScannedTransactionText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).DateOfLastScannedTransactionText)));
			this.lastTransactionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.lastTransactionTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("cb9e4d9e-1084-4d2a-8306-456d9b8ec9b0", "Last Scanned Transaction", "Date of Last Scanned CDC Transaction (Server Time)");
			this.lastTransactionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.lastTransactionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.lastTransactionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 98, true);
			this.lastTransactionTextBox.Name = "lastTransactionTextBox";
			this.lastTransactionTextBox.ReadOnly = true;
			this.lastTransactionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 13, true);
			this.lastTransactionTextBox.TabIndex = 7;
			// 
			// earliestTransactionTextBox
			// 
			this.earliestTransactionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.earliestTransactionTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.earliestTransactionTextBox, "DateOfEarliestScannedTransactionText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).DateOfEarliestScannedTransactionText)));
			this.earliestTransactionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.earliestTransactionTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("96c85736-bc44-465f-8240-2d16d682766f", "Earliest Scanned Transaction", "Date of Earliest Scanned CDC Transaction (Server Time)");
			this.earliestTransactionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.earliestTransactionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.earliestTransactionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 81, true);
			this.earliestTransactionTextBox.Name = "earliestTransactionTextBox";
			this.earliestTransactionTextBox.ReadOnly = true;
			this.earliestTransactionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 13, true);
			this.earliestTransactionTextBox.TabIndex = 6;
			// 
			// cdcEnabledTextBox
			// 
			this.cdcEnabledTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cdcEnabledTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.cdcEnabledTextBox, "IsCdcEnabledText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).IsCdcEnabledText)));
			this.cdcEnabledTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.cdcEnabledTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("f9212875-cb47-4a9d-bbe0-abdcb240588a", "CDC Enabled");
			this.cdcEnabledTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cdcEnabledTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.cdcEnabledTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 53, true);
			this.cdcEnabledTextBox.Name = "cdcEnabledTextBox";
			this.cdcEnabledTextBox.ReadOnly = true;
			this.cdcEnabledTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 13, true);
			this.cdcEnabledTextBox.TabIndex = 3;
			// 
			// dbNameTextBox
			// 
			this.dbNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dbNameTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.dbNameTextBox, "DatabaseName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).DatabaseName)));
			this.dbNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dbNameTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("511726e7-3fc4-4ebc-8e6b-b1edb513b9c2", "Database Name");
			this.dbNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.dbNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.dbNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 35, true);
			this.dbNameTextBox.Name = "dbNameTextBox";
			this.dbNameTextBox.ReadOnly = true;
			this.dbNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 13, true);
			this.dbNameTextBox.TabIndex = 2;
			// 
			// serverTextBox
			// 
			this.serverTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.serverTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.serverTextBox, "ServerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).ServerName)));
			this.serverTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.serverTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("2ffb8b42-ba98-4e60-93e2-95bcce2253dc", "Server Name");
			this.serverTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.serverTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.serverTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 18, true);
			this.serverTextBox.Name = "serverTextBox";
			this.serverTextBox.ReadOnly = true;
			this.serverTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 13, true);
			this.serverTextBox.TabIndex = 1;
			// 
			// cdcErrorLabel
			//
			this.cdcErrorLabel.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("80035b25-86f1-49b8-92cf-5de2f5d32803", "CDC Schema Errors");
			this.cdcErrorLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.cdcErrorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 44, true);
			this.cdcErrorLabel.Name = "cdcErrorLabel";
			this.cdcErrorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 18, true);
			this.cdcErrorLabel.TabIndex = 8;
			// 
			// cdcSchemaErrorTextBox
			// 
			this.cdcSchemaErrorTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.cdcSchemaErrorTextBox, "CdcSchemaErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).CdcSchemaErrors)));
			this.cdcSchemaErrorTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("d491676c-2828-4109-ad6b-353ce45d1872", "CDC Errors");
			this.cdcSchemaErrorTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cdcSchemaErrorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 65, true);
			this.cdcSchemaErrorTextBox.Multiline = true;
			this.cdcSchemaErrorTextBox.Name = "cdcSchemaErrorTextBox";
			this.cdcSchemaErrorTextBox.ReadOnly = true;
			this.cdcSchemaErrorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 252, true);
			this.cdcSchemaErrorTextBox.TabIndex = 7;
			// 
			// cdcTableCountTextBox
			// 
			this.cdcTableCountTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cdcTableCountTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.cdcTableCountTextBox, "NumberOfCdcEnabledTables");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((int)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).NumberOfCdcEnabledTables)));
			this.cdcTableCountTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.cdcTableCountTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("e08adccc-09cb-46c8-bc6e-8cd916cda758", "CDC Table Count");
			this.cdcTableCountTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cdcTableCountTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.cdcTableCountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 15, true);
			this.cdcTableCountTextBox.Name = "cdcTableCountTextBox";
			this.cdcTableCountTextBox.ReadOnly = true;
			this.cdcTableCountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 13, true);
			this.cdcTableCountTextBox.TabIndex = 6;
			// 
			// refreshButton
			// 
			this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.refreshButton.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("96e49ceb-69a1-4e22-99d4-b7e3a7d47662", "Refresh");
			this.refreshButton.IsCaptionOverridden = false;
			this.refreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(465, 620, true);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.refreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 34, true);
			this.refreshButton.TabIndex = 13;
			this.refreshButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.refreshButton.ToolTipCaption = null;
			this.refreshButton.UseVisualStyleBackColor = true;
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// copyInfoButton
			// 
			this.copyInfoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.copyInfoButton.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("d56ff089-74ef-4456-a9ee-f38fd4117dc3", "Copy Info");
			this.copyInfoButton.IsCaptionOverridden = false;
			this.copyInfoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(561, 620, true);
			this.copyInfoButton.Name = "copyInfoButton";
			this.copyInfoButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.copyInfoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 34, true);
			this.copyInfoButton.TabIndex = 14;
			this.copyInfoButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.copyInfoButton.ToolTipCaption = null;
			this.copyInfoButton.UseVisualStyleBackColor = true;
			this.copyInfoButton.Click += new System.EventHandler(this.copyInfoButton_Click);
			// 
			// cdcTabControl
			//
			this.cdcTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.cdcTabControl.Controls.Add(this.cdcSchemaErrorTabPage);
			this.cdcTabControl.Controls.Add(this.cdcLogTabPage);
			this.cdcTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 178, true);
			this.cdcTabControl.Name = "cdcTabControl";
			this.cdcTabControl.SelectedIndex = 0;
			this.cdcTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(639, 430);

			this.cdcTabControl.TabIndex = 15;
			// 
			// cdcSchemaErrorTabPage
			// 
			this.cdcSchemaErrorTabPage.Controls.Add(this.cdcErrorLabel);
			this.cdcSchemaErrorTabPage.Controls.Add(this.cdcTableCountTextBox);
			this.cdcSchemaErrorTabPage.Controls.Add(this.cdcSchemaErrorTextBox);
			this.cdcSchemaErrorTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29);
			this.cdcSchemaErrorTabPage.Name = "cdcSchemaErrorTabPage";
			this.cdcSchemaErrorTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3);
			this.cdcSchemaErrorTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 323);
			this.cdcSchemaErrorTabPage.TabIndex = 0;
			this.cdcSchemaErrorTabPage.Text = "Schema Errors";
			// 
			// cdcLogTabPage
			// 
			this.cdcLogTabPage.Controls.Add(this.cdcErrorsGrid);
			this.cdcLogTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29);
			this.cdcLogTabPage.Name = "cdcLogTabPage";
			this.cdcLogTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3);
			this.cdcLogTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 323);
			this.cdcLogTabPage.TabIndex = 1;
			this.cdcLogTabPage.Text = "CDC Errors";
			this.cdcLogTabPage.UseVisualStyleBackColor = true;
			// 
			// cdcErrorsGrid
			// 
			this.cdcErrorsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.cdcErrorsGrid, "CdcErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).CdcErrors)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((CargoWise.Bi.Product.Manager.Business.CdcError)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).CdcErrors)).SyncRoot)).session_id)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((CargoWise.Bi.Product.Manager.Business.CdcError)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).CdcErrors)).SyncRoot)).phase_number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((CargoWise.Bi.Product.Manager.Business.CdcError)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).CdcErrors)).SyncRoot)).entry_time)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((CargoWise.Bi.Product.Manager.Business.CdcError)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).CdcErrors)).SyncRoot)).error_number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((CargoWise.Bi.Product.Manager.Business.CdcError)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).CdcErrors)).SyncRoot)).error_severity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((CargoWise.Bi.Product.Manager.Business.CdcError)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).CdcErrors)).SyncRoot)).error_state)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.CdcError)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).CdcErrors)).SyncRoot)).error_message)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.CdcError)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).CdcErrors)).SyncRoot)).start_lsn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.CdcError)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).CdcErrors)).SyncRoot)).begin_lsn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.CdcError)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.CdcInformation)(null)).CdcErrors)).SyncRoot)).sequence_value)));
			this.cdcErrorsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo11.Caption = "";
			zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo11.ColumnName = "session_id";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.ColumnName = "phase_number";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.ColumnName = "entry_time";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.ColumnName = "error_number";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.ColumnName = "error_severity";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.ColumnName = "error_state";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.ColumnName = "error_message";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.ColumnName = "start_lsn";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.ColumnName = "begin_lsn";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo20.ColumnName = "sequence_value";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.cdcErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.cdcErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.cdcErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.cdcErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.cdcErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.cdcErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.cdcErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.cdcErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.cdcErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.cdcErrorsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.cdcErrorsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cdcErrorsGrid.GridId = "bd9ab030-99e1-4380-994d-2e7eaf9eafb4";
			this.cdcErrorsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.cdcErrorsGrid.LayoutKey = "cdcErrorsGrid";
			this.cdcErrorsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cdcErrorsGrid.Name = "cdcErrorsGrid";
			this.cdcErrorsGrid.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			this.cdcErrorsGrid.ReadOnly = true;
			this.cdcErrorsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 317, true);
			this.cdcErrorsGrid.TabIndex = 1;
			// 
			// CdcControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.cdcTabControl);
			this.Controls.Add(this.copyInfoButton);
			this.Controls.Add(this.refreshButton);
			this.Controls.Add(this.cdcInfoGroupBox);
			this.Name = "CdcControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 665, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.cdcInfoGroupBox.ResumeLayout(false);
			this.cdcInfoGroupBox.PerformLayout();
			this.cdcTabControl.ResumeLayout(false);
			this.cdcSchemaErrorTabPage.ResumeLayout(false);
			this.cdcSchemaErrorTabPage.PerformLayout();
			this.cdcLogTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.cdcErrorsGrid)).EndInit();
			this.cdcErrorsGrid.ResumeLayout(false);
			this.cdcErrorsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox cdcInfoGroupBox;
		private Enterprise.ZArchitecture.ZTextBox cdcEnabledTextBox;
		private Enterprise.ZArchitecture.ZTextBox dbNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox serverTextBox;
		private Enterprise.ZArchitecture.ZTextBox earliestTransactionTextBox;
		private Enterprise.ZArchitecture.ZTextBox lastCdcScanTextBox;
		private Enterprise.ZArchitecture.ZTextBox lastTransactionTextBox;
		private Enterprise.ZArchitecture.ZTextBox hasCdcErrorsTextBox;
		private Enterprise.ZArchitecture.ZTextBox cdcSchemaErrorTextBox;
		private Enterprise.ZArchitecture.ZTextBox cdcTableCountTextBox;
		private Enterprise.ZArchitecture.ZLabel cdcErrorLabel;
		private Enterprise.ZArchitecture.GUI.ZButton refreshButton;
		private Enterprise.ZArchitecture.GUI.ZButton copyInfoButton;
		private System.Windows.Forms.TabControl cdcTabControl;
		private System.Windows.Forms.TabPage cdcSchemaErrorTabPage;
		private System.Windows.Forms.TabPage cdcLogTabPage;
		public Enterprise.ZArchitecture.ZGrid cdcErrorsGrid;
	}
}
