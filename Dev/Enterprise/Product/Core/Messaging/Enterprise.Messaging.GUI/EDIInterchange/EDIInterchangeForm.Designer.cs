namespace Enterprise.Messaging.GUI
{
	public partial class EDIInterchangeForm
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.zTextBoxInterchangeNumber = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBoxInterchangeText = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBoxEHubTrackingID = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabelTruncateNotification = new Enterprise.ZArchitecture.ZLabel();
			this.zButtonSaveContentToDisk = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButtonSaveBodyTextToDisk = new Enterprise.ZArchitecture.GUI.ZButton();
			this.systemLastDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.systemCreateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.systemCreateUserFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.systemLastEditUserFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zTextBoxTransportType = new Enterprise.ZArchitecture.ZTextBox();
			this.EventTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.interchangeEventUserControl = new Enterprise.Messaging.GUI.InterchangeEventUserControl();
			this.zTextBoxEdiClient = new Enterprise.ZArchitecture.ZTextBox();
			this.EDIMessageTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EDIMessagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.systemLastDateEdit.SuspendLayout();
			this.systemCreateDateEdit.SuspendLayout();
			this.systemCreateUserFindBox.SuspendLayout();
			this.systemLastEditUserFindBox.SuspendLayout();
			this.EventTabPage.SuspendLayout();
			this.interchangeEventUserControl.SuspendLayout();
			this.EDIMessageTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.EventTabPage);
			this.MainTabControl.Controls.Add(this.EDIMessageTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 437, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.EDIMessageTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.EventTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.zTextBoxEdiClient);
			this.MainTabPage.Controls.Add(this.zTextBoxTransportType);
			this.MainTabPage.Controls.Add(this.systemLastEditUserFindBox);
			this.MainTabPage.Controls.Add(this.systemCreateUserFindBox);
			this.MainTabPage.Controls.Add(this.systemLastDateEdit);
			this.MainTabPage.Controls.Add(this.systemCreateDateEdit);
			this.MainTabPage.Controls.Add(this.zButtonSaveBodyTextToDisk);
			this.MainTabPage.Controls.Add(this.zButtonSaveContentToDisk);
			this.MainTabPage.Controls.Add(this.zLabelTruncateNotification);
			this.MainTabPage.Controls.Add(this.zTextBoxInterchangeText);
			this.MainTabPage.Controls.Add(this.zTextBoxEHubTrackingID);
			this.MainTabPage.Controls.Add(this.zTextBoxInterchangeNumber);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 410, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 389, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 410, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 437, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(529, 6, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(412);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(412);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Messaging.Business.EDIInterchange);
			// 
			// zTextBoxInterchangeNumber
			// 
			this.BindingSource.SetBindingMember(this.zTextBoxInterchangeNumber, "EI_InterchangeNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIInterchange)(null)).EI_InterchangeNum)));
			this.zTextBoxInterchangeNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 11, true);
			this.zTextBoxInterchangeNumber.Name = "zTextBoxInterchangeNumber";
			this.zTextBoxInterchangeNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.zTextBoxInterchangeNumber.TabIndex = 0;
			// 
			// zTextBoxInterchangeText
			// 
			this.zTextBoxInterchangeText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBoxInterchangeText, "EI_InterchangeTextDetailFormatted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIInterchange)(null)).EI_InterchangeTextDetailFormatted)));
			this.zTextBoxInterchangeText.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIInterchangeForm|e8ec0f86-97bc-4fb7-876a-104832dd0b69", "Contents");
			this.zTextBoxInterchangeText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBoxInterchangeText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 134, true);
			this.zTextBoxInterchangeText.Multiline = true;
			this.zTextBoxInterchangeText.Name = "zTextBoxInterchangeText";
			this.zTextBoxInterchangeText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBoxInterchangeText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(643, 272, true);
			this.zTextBoxInterchangeText.TabIndex = 10;
			// 
			// zTextBoxEHubTrackingID
			// 
			this.BindingSource.SetBindingMember(this.zTextBoxEHubTrackingID, "eHubID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIInterchange)(null)).eHubID)));
			this.zTextBoxEHubTrackingID.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIInterchangeForm|7504a054-c855-46d2-b233-a019e41e8a13", "eHub ID", "eHub Tracking ID", "eHub Tracking ID", "");
			this.zTextBoxEHubTrackingID.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 37, true);
			this.zTextBoxEHubTrackingID.Name = "zTextBoxEHubTrackingID";
			this.zTextBoxEHubTrackingID.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.zTextBoxEHubTrackingID.TabIndex = 2;
			// 
			// zLabelTruncateNotification
			// 
			this.zLabelTruncateNotification.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIInterchangeForm|73a11eda-a6bf-46bd-97a5-f3a34161f67e", "Truncated Text", "Truncated Text", "Truncated Text", "");
			this.zLabelTruncateNotification.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelTruncateNotification.ForeColor = System.Drawing.Color.Maroon;
			this.zLabelTruncateNotification.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 112, true);
			this.zLabelTruncateNotification.Name = "zLabelTruncateNotification";
			this.zLabelTruncateNotification.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 19, true);
			this.zLabelTruncateNotification.TabIndex = 3;
			this.zLabelTruncateNotification.UseMnemonic = false;
			// 
			// zButtonSaveContentToDisk
			// 
			this.zButtonSaveContentToDisk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zButtonSaveContentToDisk.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIInterchangeForm|1b2b70fc-36c6-4b38-bbf1-6aeb881d3000", "Save To Disk", "Save To Disk", "Save To Disk", "");
			this.zButtonSaveContentToDisk.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 35, true);
			this.zButtonSaveContentToDisk.Name = "zButtonSaveContentToDisk";
			this.zButtonSaveContentToDisk.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 23, true);
			this.zButtonSaveContentToDisk.TabIndex = 4;
			this.zButtonSaveContentToDisk.ToolTipCaption = null;
			this.zButtonSaveContentToDisk.UseVisualStyleBackColor = true;
			this.zButtonSaveContentToDisk.Click += new System.EventHandler(this.zButtonSaveContentToDisk_Click);
			// 
			// zButtonSaveBodyTextToDisk
			// 
			this.zButtonSaveBodyTextToDisk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zButtonSaveBodyTextToDisk.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIInterchangeForm|d2d8a71c-6621-4d84-88dd-b06bbe4330a9", "Save Body To Disk", "Save Body To Disk", "Save Body To Disk", "");
			this.zButtonSaveBodyTextToDisk.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(629, 35, true);
			this.zButtonSaveBodyTextToDisk.Name = "zButtonSaveBodyTextToDisk";
			this.zButtonSaveBodyTextToDisk.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 23, true);
			this.zButtonSaveBodyTextToDisk.TabIndex = 5;
			this.zButtonSaveBodyTextToDisk.ToolTipCaption = null;
			this.zButtonSaveBodyTextToDisk.UseVisualStyleBackColor = true;
			this.zButtonSaveBodyTextToDisk.Click += new System.EventHandler(this.zButtonSaveBodyTextToDisk_Click);
			// 
			// systemLastDateEdit
			// 
			this.systemLastDateEdit.AllowDrop = true;
			this.systemLastDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.systemLastDateEdit, "EI_SystemLastEditTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Messaging.Business.EDIInterchange)(null)).EI_SystemLastEditTimeUtc)));
			this.systemLastDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			this.systemLastDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 89, true);
			this.systemLastDateEdit.Name = "systemLastDateEdit";
			this.systemLastDateEdit.TabIndex = 8;
			// 
			// systemCreateDateEdit
			// 
			this.systemCreateDateEdit.AllowDrop = true;
			this.systemCreateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.systemCreateDateEdit, "EI_SystemCreateTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Messaging.Business.EDIInterchange)(null)).EI_SystemCreateTimeUtc)));
			this.systemCreateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			this.systemCreateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 63, true);
			this.systemCreateDateEdit.Name = "systemCreateDateEdit";
			this.systemCreateDateEdit.TabIndex = 6;
			// 
			// systemCreateUserFindBox
			// 
			this.systemCreateUserFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.systemCreateUserFindBox, "EI_SystemCreateUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIInterchange)(null)).EI_SystemCreateUser)));
			this.systemCreateUserFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 64, true);
			this.systemCreateUserFindBox.Name = "systemCreateUserFindBox";
			this.systemCreateUserFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.systemCreateUserFindBox.ParentType = null;
			this.systemCreateUserFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.systemCreateUserFindBox.TabIndex = 7;
			// 
			// systemLastEditUserFindBox
			// 
			this.systemLastEditUserFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.systemLastEditUserFindBox, "EI_SystemLastEditUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIInterchange)(null)).EI_SystemLastEditUser)));
			this.systemLastEditUserFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 90, true);
			this.systemLastEditUserFindBox.Name = "systemLastEditUserFindBox";
			this.systemLastEditUserFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.systemLastEditUserFindBox.ParentType = null;
			this.systemLastEditUserFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.systemLastEditUserFindBox.TabIndex = 9;
			// 
			// zTextBoxTransportType
			// 
			this.BindingSource.SetBindingMember(this.zTextBoxTransportType, "EI_TransportType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIInterchange)(null)).EI_TransportType)));
			this.zTextBoxTransportType.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIInterchangeForm|A6BC8D4F-45C5-4B56-81E8-EC1433355144", "Transport Type", "Transport Type", "Transport Type", "");
			this.zTextBoxTransportType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 11, true);
			this.zTextBoxTransportType.Name = "zTextBoxTransportType";
			this.zTextBoxTransportType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.zTextBoxTransportType.TabIndex = 1;
			// 
			// EventTabPage
			// 
			this.EventTabPage.Controls.Add(this.interchangeEventUserControl);
			this.EventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EventTabPage.Name = "EventTabPage";
			this.EventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.EventTabPage.TabIndex = 0;
			this.EventTabPage.Text = "Interchange Event";
			// 
			// interchangeEventUserControl
			// 
			this.interchangeEventUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.interchangeEventUserControl, ".");
			this.interchangeEventUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.interchangeEventUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.interchangeEventUserControl.Name = "interchangeEventUserControl";
			this.interchangeEventUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.interchangeEventUserControl.TabIndex = 0;
			// 
			// zTextBoxEdiClient
			// 
			this.BindingSource.SetBindingMember(this.zTextBoxEdiClient, "CommunicationPartyConfig.Party.Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIInterchange)(null)).CommunicationPartyConfig.Party.Name)));
			this.zTextBoxEdiClient.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIInterchangeForm|43787E86-E2DC-4FF3-85BB-A4FD23FEEE87", "EDI Client", "EDI Client", "EDI Client", "");
			this.zTextBoxEdiClient.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 11, true);
			this.zTextBoxEdiClient.Name = "zTextBoxEdiClient";
			this.zTextBoxEdiClient.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.zTextBoxEdiClient.TabIndex = 11;
			// 
			// EDIMessageTabPage
			//
			this.EDIMessageTabPage.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("b5e8bc61-ba80-42b0-a321-aa43082bf117", "EDI Messages");
			this.EDIMessageTabPage.Controls.Add(this.EDIMessagesGrid);
			this.EDIMessageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EDIMessageTabPage.Name = "EDIMessageTabPage";
			this.EDIMessageTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EDIMessageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1083, 306, true);
			this.EDIMessageTabPage.TabIndex = 0;
			this.EDIMessageTabPage.UseVisualStyleBackColor = true;
			// 
			// EDIMessagesGrid
			// 
			((System.ComponentModel.ISupportInitialize)(this.EDIMessagesGrid)).BeginInit();
			this.BindingSource.SetBindingMember(this.EDIMessagesGrid, "ContainedMessages");
			this.EDIMessagesGrid.SuspendLayout();
			// 
			this.EDIMessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EDIMessagesGrid.GridId = "d2361528-c2e8-4851-8d35-c2f2442664cd";
			this.EDIMessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EDIMessagesGrid.IsWholeRowSelectedOnClick = true;
			this.EDIMessagesGrid.LayoutKey = "EDIMessagesGrid";
			this.EDIMessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EDIMessagesGrid.Name = "EDIMessagesGrid";
			this.EDIMessagesGrid.ReadOnly = true;
			this.EDIMessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1077, 130, true);
			this.EDIMessagesGrid.TabIndex = 0;
			this.EDIMessagesGrid.DoubleClick += new System.EventHandler(this.GridEDIMessages_DoubleClick);
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|a2a2c589-01b3-4da7-8318-40680b754287", "Message Num.");
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|71159008-0c6a-4eca-95a7-76509736a09b", "Direction");
			zTextBoxColumnStyleInfo3.ColumnName = "EM_SendOrReceiveHumanReadable";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|e745ece2-0ef2-4252-b755-91ad6629b3e6", "App. Code");
			zTextBoxColumnStyleInfo4.ColumnName = "EM_ApplicationCode";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|07851fd8-33d5-4376-8a2f-c3841805a1ce", "App. Ref.");
			zTextBoxColumnStyleInfo5.ColumnName = "EM_ApplicationReference";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|6840364f-4381-4e01-976d-ea42202989f7", "Sub Type");
			zTextBoxColumnStyleInfo6.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|3234e3af-66e5-401f-aaef-974dd4581d2e", "Message Sub Type");
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|7213ed8b-3e4d-4c34-b926-c1d145aa4c9f", "Sub Type Desc.");
			zTextBoxColumnStyleInfo7.ColumnName = "EM_MessageSubTypeDescription";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|3234e3af-66e5-401f-aaef-974dd4581d2e", "Message Sub Type");
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|f973e8bb-bdaa-4425-963f-9004f841bde4", "Message Time");
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|7e7bd75f-9725-45d6-b8b3-f3ae00eef17b", "Sending User");
			zTextBoxColumnStyleInfo9.ColumnName = "EM_SendingUser";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|14e1eac0-e064-426c-a8d3-5d354bde3638", "Date Time Interchange");
			zDateEditColumnStyleInfo2.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo10.ColumnName = "EM_MessageTextShort";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|08546a7e-7d26-4ebd-9f8b-7e1596c2fdfe", "Interchange Num.");
			zTextBoxColumnStyleInfo11.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|de8c5325-e324-4d79-9b26-d01e45d7eb22", "Interchange Status");
			zTextBoxColumnStyleInfo12.ColumnName = "EM_InterchangeStatus";
			zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|410341d3-c692-40a2-95ac-2db01a7cb852", "Sender");
			zTextBoxColumnStyleInfo13.ColumnName = "EM_InterchangeSender";
			zTextBoxColumnStyleInfo13.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageFilterControl|ec8f911c-c22b-4b2a-9fb4-d9e65ef799b8", "Receiver");
			zTextBoxColumnStyleInfo14.ColumnName = "EM_InterchangeReceiver";
			zTextBoxColumnStyleInfo14.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EDIMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EDIMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EDIMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EDIMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EDIMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EDIMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.EDIMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.EDIMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.EDIMessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EDIMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.EDIMessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.EDIMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.EDIMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.EDIMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.EDIMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.EDIMessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			((System.ComponentModel.ISupportInitialize)(this.EDIMessagesGrid)).EndInit();
			this.EDIMessagesGrid.ResumeLayout(false);
			this.EDIMessagesGrid.PerformLayout();
			// 
			// EDIInterchangeForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 493, true);
			this.DataSourceAssemblyName = "Enterprise.Messaging.Business";
			this.DataSourceType = typeof(Enterprise.Messaging.Business.EDIInterchange);
			this.DataSourceTypeName = "Enterprise.Messaging.Business.EDIInterchange";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 532, true);
			this.Name = "EDIInterchangeForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.EDIMessageTabPage.ResumeLayout(false);
			this.EDIMessageTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.systemLastDateEdit.ResumeLayout(true);
			this.systemLastDateEdit.PerformLayout();
			this.systemCreateDateEdit.ResumeLayout(true);
			this.systemCreateDateEdit.PerformLayout();
			this.systemCreateUserFindBox.ResumeLayout(true);
			this.systemCreateUserFindBox.PerformLayout();
			this.systemLastEditUserFindBox.ResumeLayout(true);
			this.systemLastEditUserFindBox.PerformLayout();
			this.EventTabPage.ResumeLayout(false);
			this.EventTabPage.PerformLayout();
			this.interchangeEventUserControl.ResumeLayout(true);
			this.interchangeEventUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox zTextBoxInterchangeNumber;
		private Enterprise.ZArchitecture.ZTextBox zTextBoxInterchangeText;
		private ZArchitecture.ZTextBox zTextBoxEHubTrackingID;
		private ZArchitecture.ZLabel zLabelTruncateNotification;
		private Enterprise.ZArchitecture.GUI.ZButton zButtonSaveContentToDisk;
		private Enterprise.ZArchitecture.GUI.ZButton zButtonSaveBodyTextToDisk;
		private Enterprise.ZArchitecture.GUI.ZDateEdit systemLastDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit systemCreateDateEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox systemLastEditUserFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox systemCreateUserFindBox;
		private ZArchitecture.ZTextBox zTextBoxTransportType;
		private Enterprise.ZArchitecture.GUI.ZTabPage EventTabPage;
		InterchangeEventUserControl interchangeEventUserControl;
		private ZArchitecture.ZTextBox zTextBoxEdiClient;
		private ZArchitecture.ZGrid EDIMessagesGrid;
		private Enterprise.ZArchitecture.GUI.ZTabPage EDIMessageTabPage;

	}
}
