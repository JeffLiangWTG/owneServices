namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	partial class UpgradeForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PreferedUpgradeMethodGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DefaultRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.HttpRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SaveToDiskRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReleaseBuildLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReleaseBuildGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SendEmailNotificationAutomaticallyLabel = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.ScheduledDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.AdditionalNotificationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PreferedUpgradeMethodGroupBox.SuspendLayout();
			this.ReleaseBuildGuidFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.ScheduledDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MainStatusBar.Dock = System.Windows.Forms.DockStyle.None;
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 391, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 24, true);
			this.MainStatusBar.TabIndex = 12;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(501);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer);
			// 
			// PreferedUpgradeMethodGroupBox
			// 
			this.PreferedUpgradeMethodGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PreferedUpgradeMethodGroupBox.Controls.Add(this.DescriptionLabel);
			this.PreferedUpgradeMethodGroupBox.Controls.Add(this.DefaultRadioButton);
			this.PreferedUpgradeMethodGroupBox.Controls.Add(this.HttpRadioButton);
			this.PreferedUpgradeMethodGroupBox.Controls.Add(this.SaveToDiskRadioButton);
			this.PreferedUpgradeMethodGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.PreferedUpgradeMethodGroupBox.Name = "PreferedUpgradeMethodGroupBox";
			this.PreferedUpgradeMethodGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 88, true);
			this.PreferedUpgradeMethodGroupBox.TabIndex = 2;
			this.PreferedUpgradeMethodGroupBox.TabStop = false;
			this.PreferedUpgradeMethodGroupBox.Text = "Preferred Upgrade Method";
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionLabel, "Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).Description)));
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 48, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 32, true);
			this.DescriptionLabel.TabIndex = 5;
			this.DescriptionLabel.Text = "Description";
			// 
			// DefaultRadioButton
			// 
			this.DefaultRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.DefaultRadioButton, "IsSendViaDefault");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).IsSendViaDefault)));
			this.DefaultRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DefaultRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.DefaultRadioButton.Name = "DefaultRadioButton";
			this.DefaultRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 24, true);
			this.DefaultRadioButton.TabIndex = 0;
			this.DefaultRadioButton.Text = " Default";
			// 
			// HttpRadioButton
			// 
			this.HttpRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.HttpRadioButton, "IsSendViaHttp");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).IsSendViaHttp)));
			this.HttpRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HttpRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 16, true);
			this.HttpRadioButton.Name = "HttpRadioButton";
			this.HttpRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 24, true);
			this.HttpRadioButton.TabIndex = 3;
			this.HttpRadioButton.Text = " HTTP";
			// 
			// SaveToDiskRadioButton
			// 
			this.SaveToDiskRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.SaveToDiskRadioButton, "IsSaveToDisk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).IsSaveToDisk)));
			this.SaveToDiskRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SaveToDiskRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 16, true);
			this.SaveToDiskRadioButton.Name = "SaveToDiskRadioButton";
			this.SaveToDiskRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 24, true);
			this.SaveToDiskRadioButton.TabIndex = 4;
			this.SaveToDiskRadioButton.Text = "Save to Disk";
			this.SaveToDiskRadioButton.CheckedChanged += new System.EventHandler(this.SaveToDiskRadioButton_CheckedChanged);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 362, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 10;
			this.SendButton.Text = "Send";
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 362, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 11;
			this.CloseButton.Text = "Close";
			// 
			// ReleaseBuildLabel
			// 
			this.ReleaseBuildLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.ReleaseBuildLabel.Name = "ReleaseBuildLabel";
			this.ReleaseBuildLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.ReleaseBuildLabel.TabIndex = 0;
			this.ReleaseBuildLabel.Text = "Release Build:";
			// 
			// ReleaseBuildGuidFindBox
			// 
			this.ReleaseBuildGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReleaseBuildGuidFindBox, "ReleaseBuildPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).ReleaseBuildPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).ReleaseBuilds)));
			this.ReleaseBuildGuidFindBox.BindToList = "ReleaseBuilds";
			this.ReleaseBuildGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			this.ReleaseBuildGuidFindBox.Name = "ReleaseBuildGuidFindBox";
			this.ReleaseBuildGuidFindBox.PreBoundMaxLength = 14;
			this.ReleaseBuildGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 18, true);
			this.ReleaseBuildGuidFindBox.TabIndex = 1;
			// 
			// SendEmailNotificationAutomaticallyLabel
			// 
			this.SendEmailNotificationAutomaticallyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.SendEmailNotificationAutomaticallyLabel, "SendEmailNotificationAutomatically");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).SendEmailNotificationAutomatically)));
			this.SendEmailNotificationAutomaticallyLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendEmailNotificationAutomaticallyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 361, true);
			this.SendEmailNotificationAutomaticallyLabel.Name = "SendEmailNotificationAutomaticallyLabel";
			this.SendEmailNotificationAutomaticallyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 24, true);
			this.SendEmailNotificationAutomaticallyLabel.TabIndex = 9;
			this.SendEmailNotificationAutomaticallyLabel.Text = "Send Email Notification Automatically";
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid1, "Upgrades");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).Upgrades)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequest)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).Upgrades)).SyncRoot)).EnterpriseCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequest)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).Upgrades)).SyncRoot)).CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequest)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).Upgrades)).SyncRoot)).ServerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequest)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).Upgrades)).SyncRoot)).ReleaseRing)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequest)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).Upgrades)).SyncRoot)).SqlVersion)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequest)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).Upgrades)).SyncRoot)).Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequest)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).Upgrades)).SyncRoot)).SupportedUpgradeMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequest)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).Upgrades)).SyncRoot)).SupportedUpgradeMethods)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequest)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).Upgrades)).SyncRoot)).ScheduledDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequest)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).Upgrades)).SyncRoot)).ClientContactPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequest)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).Upgrades)).SyncRoot)).ClientContacts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequest)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).Upgrades)).SyncRoot)).ClientContactEmailAddress)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Code";
			zTextBoxColumnStyleInfo1.ColumnName = "EnterpriseCode";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.Caption = "Company Name";
			zTextBoxColumnStyleInfo2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("UpgradeFormTestHelper|644ae6cf-388e-4296-9821-232062c046ee", "Company Name");
			zTextBoxColumnStyleInfo2.ColumnName = "CompanyName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.Caption = "Server";
			zTextBoxColumnStyleInfo3.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("UpgradeFormTestHelper|75c13514-214e-42e6-bfdf-d81752d321f4", "Server");
			zTextBoxColumnStyleInfo3.ColumnName = "ServerCode";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.Caption = "Ring";
			zTextBoxColumnStyleInfo4.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("UpgradeFormTestHelper|6eece51f-901c-4eb8-bfbb-e542858b818e", "Ring");
			zTextBoxColumnStyleInfo4.ColumnName = "ReleaseRing";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo5.Caption = "SQL Version";
			zTextBoxColumnStyleInfo5.ColumnName = "SqlVersion";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.BindToList = "SupportedUpgradeMethods";
			zDropEditColumnStyleInfo1.Caption = "Method";
			zDropEditColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("UpgradeFormTestHelper|e5bc84f7-6dfc-4532-afd1-3ee1597fd6a1", "Method");
			zDropEditColumnStyleInfo1.ColumnName = "SupportedUpgradeMethod";
			zDropEditColumnStyleInfo1.ShowHorizontalScrollBar = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zDateEditColumnStyleInfo1.Caption = "Scheduled Time";
			zDateEditColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("UpgradeFormTestHelper|4c04dd0e-668c-4a94-89a9-82eff9c8b825", "Scheduled Time");
			zDateEditColumnStyleInfo1.ColumnName = "ScheduledDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zGuidFindBoxColumnStyleInfo1.BindToList = "ClientContacts";
			zGuidFindBoxColumnStyleInfo1.Caption = "Client Contact";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("UpgradeFormTestHelper|71f2fd34-bdf6-4c04-9702-30a4e38c670e", "Client Contact");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ClientContactPK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.Caption = "Notification Email Address";
			zTextBoxColumnStyleInfo6.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("UpgradeFormTestHelper|cb33f018-39b4-4e33-9bfa-332f891f590a", "Notification Email Address", "The email address that the notification email will be sent to.");
			zTextBoxColumnStyleInfo6.ColumnName = "ClientContactEmailAddress";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo7.Caption = "Product";
			zTextBoxColumnStyleInfo7.ColumnName = "Product";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.zGrid1.GridId = "6a495085-5855-443a-84c4-b6be3a52beff";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 160, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 120, true);
			this.zGrid1.TabIndex = 6;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 136, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 23, true);
			this.zLabel1.TabIndex = 4;
			this.zLabel1.Text = "Scheduled DateTime:";
			// 
			// ScheduledDateEdit
			// 
			this.ScheduledDateEdit.AllowDrop = true;
			this.ScheduledDateEdit.AutoCompleteMonthThreshold = 1;
			this.ScheduledDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ScheduledDateEdit, "ScheduledDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).ScheduledDateTime)));
			this.ScheduledDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ScheduledDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 136, true);
			this.ScheduledDateEdit.Name = "ScheduledDateEdit";
			this.ScheduledDateEdit.TabIndex = 5;
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 136, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 23, true);
			this.zLabel2.TabIndex = 3;
			this.zLabel2.Text = "Servers to Upgrade";
			// 
			// zLabel3
			// 
			this.zLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.zLabel3.AutoSize = true;
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 283, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 13, true);
			this.zLabel3.TabIndex = 7;
			this.zLabel3.Text = "Additional Notification Text";
			// 
			// AdditionalNotificationTextBox
			// 
			this.AdditionalNotificationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AdditionalNotificationTextBox, "AdditionalNotification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer)(null)).AdditionalNotification)));
			this.AdditionalNotificationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AdditionalNotificationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 303, true);
			this.AdditionalNotificationTextBox.Multiline = true;
			this.AdditionalNotificationTextBox.Name = "AdditionalNotificationTextBox";
			this.AdditionalNotificationTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.AdditionalNotificationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 50, true);
			this.AdditionalNotificationTextBox.TabIndex = 8;
			// 
			// UpgradeForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 415, true);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.AdditionalNotificationTextBox);
			this.Controls.Add(this.ScheduledDateEdit);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.zGrid1);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.ReleaseBuildGuidFindBox);
			this.Controls.Add(this.ReleaseBuildLabel);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.PreferedUpgradeMethodGroupBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.SendEmailNotificationAutomaticallyLabel);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContainer);
			this.DataSourceTypeName = "Enterprise.Client.EDI.LicenceKeyBuilder.Business.UpgradeRequestCollectionContaine" +
	"r";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 400, true);
			this.Name = "UpgradeForm";
			this.Text = "UpgradeForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SendEmailNotificationAutomaticallyLabel, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.PreferedUpgradeMethodGroupBox, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.ReleaseBuildLabel, 0);
			this.Controls.SetChildIndex(this.ReleaseBuildGuidFindBox, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.zGrid1, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.ScheduledDateEdit, 0);
			this.Controls.SetChildIndex(this.AdditionalNotificationTextBox, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PreferedUpgradeMethodGroupBox.ResumeLayout(false);
			this.PreferedUpgradeMethodGroupBox.PerformLayout();
			this.ReleaseBuildGuidFindBox.ResumeLayout(true);
			this.ReleaseBuildGuidFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.ScheduledDateEdit.ResumeLayout(true);
			this.ScheduledDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		public Enterprise.ZArchitecture.GUI.ZRadioButton DefaultRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton HttpRadioButton;
		public Enterprise.ZArchitecture.GUI.ZButton SendButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		public Enterprise.ZArchitecture.GUI.ZRadioButton SaveToDiskRadioButton;
		private Enterprise.ZArchitecture.ZLabel ReleaseBuildLabel;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ReleaseBuildGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox SendEmailNotificationAutomaticallyLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PreferedUpgradeMethodGroupBox;
		private Enterprise.ZArchitecture.ZGrid zGrid1;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.ZLabel zLabel2;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ScheduledDateEdit;
		private Enterprise.ZArchitecture.ZLabel zLabel3;
		private Enterprise.ZArchitecture.ZTextBox AdditionalNotificationTextBox;
		private Enterprise.ZArchitecture.ZLabel DescriptionLabel;
	}
}
