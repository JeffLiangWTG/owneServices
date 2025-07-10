
using System;
using Enterprise.Registry.Business.eServices;

namespace Enterprise.Messaging.GUI
{
	partial class EDIMessageStandAloneUserControl
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
			this.messageDetailsGroupBoxHeight = 180;
            this.messageDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.interchangeDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.interchangeEHubIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.interchangeStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.interchangeTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.interchangeNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.processingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.applicationReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.messageTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.zTextBoxMessageNumber = new Enterprise.ZArchitecture.ZTextBox();
            this.messageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.messageDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.systemCreateTimeUtc = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.applicationTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.messageTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.messageSubTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.directionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.transportTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.externalReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.linkedEDIMessageNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.showLinkedEDIMessageButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.messageContentsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.contentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.zLabelTruncateNotification = new Enterprise.ZArchitecture.ZLabel();
            this.senderReceiverPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.zTextBoxEdiClient = new Enterprise.ZArchitecture.ZTextBox();
            this.SaveAndOpenButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.receiverTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.senderTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.zButtonSaveRawMessage = new Enterprise.ZArchitecture.GUI.ZButton();
            this.zButtonSaveFormatedMessage = new Enterprise.ZArchitecture.GUI.ZButton();
            this.showInterchangeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.messageDetailsPanel.SuspendLayout();
            this.interchangeDetailsGroupBox.SuspendLayout();
            this.interchangeTimeDateEdit.SuspendLayout();
            this.processingDetailsGroupBox.SuspendLayout();
            this.messageTimeDateEdit.SuspendLayout();
            this.messageDetailsGroupBox.SuspendLayout();
            this.systemCreateTimeUtc.SuspendLayout();
            this.messageContentsPanel.SuspendLayout();
            this.contentsGroupBox.SuspendLayout();
            this.senderReceiverPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Messaging.Business.EDIMessage);
			// 
			// messageDetailsPanel
			//
			this.messageDetailsPanel.AutoScroll = true;
			this.messageDetailsPanel.Controls.Add(this.interchangeDetailsGroupBox);
            this.messageDetailsPanel.Controls.Add(this.processingDetailsGroupBox);
            this.messageDetailsPanel.Controls.Add(this.messageDetailsGroupBox);
            this.messageDetailsPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.messageDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.messageDetailsPanel.Name = "messageDetailsPanel";
            this.messageDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 380, true);
            this.messageDetailsPanel.TabIndex = 22;
			// 
			// interchangeDetailsGroupBox
			//
			this.interchangeDetailsGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|9deeacc6-38a0-47cd-b9fc-824064911a96", "Interchange Details");
            this.interchangeDetailsGroupBox.Controls.Add(this.interchangeEHubIDTextBox);
            this.interchangeDetailsGroupBox.Controls.Add(this.interchangeStatusTextBox);
            this.interchangeDetailsGroupBox.Controls.Add(this.interchangeTimeDateEdit);
            this.interchangeDetailsGroupBox.Controls.Add(this.interchangeNumberTextBox);
			this.interchangeDetailsGroupBox.Controls.Add(this.showInterchangeButton);
            this.interchangeDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.interchangeDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 248, true);
            this.interchangeDetailsGroupBox.Name = "interchangeDetailsGroupBox";
            this.interchangeDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 135, true);
            this.interchangeDetailsGroupBox.TabIndex = 21;
            this.interchangeDetailsGroupBox.TabStop = false;
            // 
            // interchangeEHubIDTextBox
            // 
            this.BindingSource.SetBindingMember(this.interchangeEHubIDTextBox, "InterchangeEHubID");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).InterchangeEHubID)));
            this.interchangeEHubIDTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|dea4f963-e250-49fa-8902-441e6ce3763a", "eHub Tracking Id");
            this.interchangeEHubIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.interchangeEHubIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 84, true);
            this.interchangeEHubIDTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.interchangeEHubIDTextBox.Name = "interchangeEHubIDTextBox";
            this.interchangeEHubIDTextBox.ReadOnly = true;
            this.interchangeEHubIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.interchangeEHubIDTextBox.TabIndex = 25;
            // 
            // interchangeStatusTextBox
            // 
            this.BindingSource.SetBindingMember(this.interchangeStatusTextBox, "EM_InterchangeStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_InterchangeStatus)));
            this.interchangeStatusTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|1ebeaef8-1312-454e-8e62-39557fb5ec02", "Interchange Status");
            this.interchangeStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.interchangeStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 61, true);
            this.interchangeStatusTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.interchangeStatusTextBox.Name = "interchangeStatusTextBox";
            this.interchangeStatusTextBox.ReadOnly = true;
            this.interchangeStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.interchangeStatusTextBox.TabIndex = 24;
            // 
            // interchangeTimeDateEdit
            // 
            this.interchangeTimeDateEdit.AllowDrop = true;
            this.interchangeTimeDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.interchangeTimeDateEdit, "EM_DateTimeInterchangeSent");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_DateTimeInterchangeSent)));
            this.interchangeTimeDateEdit.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|6921da4d-06a4-496b-8b00-43b8b58632fe", "Interchange Time");
            this.interchangeTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
            this.interchangeTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 17, true);
            this.interchangeTimeDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.interchangeTimeDateEdit.Name = "interchangeTimeDateEdit";
			this.interchangeTimeDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.interchangeTimeDateEdit.TabIndex = 22;
            // 
            // interchangeNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.interchangeNumberTextBox, "EM_InterchangeNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_InterchangeNumber)));
            this.interchangeNumberTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|40fdb15b-7f76-49ea-a0e3-8cb006ba5b01", "Interchange No.", "Interchange Number.");
            this.interchangeNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.interchangeNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 39, true);
            this.interchangeNumberTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.interchangeNumberTextBox.Name = "interchangeNumberTextBox";
            this.interchangeNumberTextBox.ReadOnly = true;
            this.interchangeNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.interchangeNumberTextBox.TabIndex = 23;
            // 
            // processingDetailsGroupBox
            // 
            this.processingDetailsGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|7d794cdc-0edb-4583-8e59-af32d5fd1943", "Processing Details");
            this.processingDetailsGroupBox.Controls.Add(this.applicationReferenceTextBox);
            this.processingDetailsGroupBox.Controls.Add(this.messageTimeDateEdit);
            this.processingDetailsGroupBox.Controls.Add(this.zTextBoxMessageNumber);
            this.processingDetailsGroupBox.Controls.Add(this.messageStatusTextBox);
            this.processingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.processingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 135, true);
            this.processingDetailsGroupBox.Name = "processingDetailsGroupBox";
            this.processingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 113, true);
            this.processingDetailsGroupBox.TabIndex = 10;
            this.processingDetailsGroupBox.TabStop = false;
            // 
            // applicationReferenceTextBox
            // 
            this.BindingSource.SetBindingMember(this.applicationReferenceTextBox, "EM_ApplicationReference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_ApplicationReference)));
            this.applicationReferenceTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|3fb966d7-f3b9-4460-818f-4efe2c9e8ba0", "Application Reference", "Application Reference Number.");
            this.applicationReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.applicationReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 83, true);
            this.applicationReferenceTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.applicationReferenceTextBox.Name = "applicationReferenceTextBox";
            this.applicationReferenceTextBox.ReadOnly = true;
            this.applicationReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.applicationReferenceTextBox.TabIndex = 18;
            // 
            // messageTimeDateEdit
            // 
            this.messageTimeDateEdit.AllowDrop = true;
            this.messageTimeDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.messageTimeDateEdit, "EM_MessageDateTime");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageDateTime)));
            this.messageTimeDateEdit.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|db2876b8-e1b7-4ea4-9d76-0da4a6a34710", "Message Sent Time", "Time this EDI Message was sent.");
            this.messageTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
            this.messageTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 17, true);
            this.messageTimeDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.messageTimeDateEdit.Name = "messageTimeDateEdit";
			this.messageTimeDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.messageTimeDateEdit.TabIndex = 15;
            // 
            // zTextBoxMessageNumber
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxMessageNumber, "EM_MessageNum");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageNum)));
            this.zTextBoxMessageNumber.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.zTextBoxMessageNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 39, true);
            this.zTextBoxMessageNumber.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.zTextBoxMessageNumber.Name = "zTextBoxMessageNumber";
            this.zTextBoxMessageNumber.ReadOnly = true;
            this.zTextBoxMessageNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.zTextBoxMessageNumber.TabIndex = 16;
            // 
            // messageStatusTextBox
            // 
            this.BindingSource.SetBindingMember(this.messageStatusTextBox, "MessageStatusWithDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).MessageStatusWithDescription)));
            this.messageStatusTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|0e979727-0c56-4b35-8d92-7a47a3d5bbac", "Status", "EDI Message Status.");
            this.messageStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.messageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 61, true);
            this.messageStatusTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.messageStatusTextBox.Name = "messageStatusTextBox";
            this.messageStatusTextBox.ReadOnly = true;
            this.messageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.messageStatusTextBox.TabIndex = 17;
			// 
			// messageDetailsGroupBox
			// 
			this.messageDetailsGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|b5bf1861-0554-49b2-b74e-88b064a59970", "Message Details");
            this.messageDetailsGroupBox.Controls.Add(this.systemCreateTimeUtc);
            this.messageDetailsGroupBox.Controls.Add(this.applicationTypeTextBox);
            this.messageDetailsGroupBox.Controls.Add(this.messageTypeTextBox);
            this.messageDetailsGroupBox.Controls.Add(this.messageSubTypeTextBox);
            this.messageDetailsGroupBox.Controls.Add(this.directionTextBox);
			this.messageDetailsGroupBox.Controls.Add(this.transportTypeTextBox);
			this.messageDetailsGroupBox.Controls.Add(this.externalReferenceNumberTextBox);
			this.messageDetailsGroupBox.Controls.Add(this.linkedEDIMessageNumberTextBox);
			this.messageDetailsGroupBox.Controls.Add(this.showLinkedEDIMessageButton);
			this.messageDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.messageDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.messageDetailsGroupBox.Name = "messageDetailsGroupBox";
            this.messageDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, messageDetailsGroupBoxHeight, true);
            this.messageDetailsGroupBox.TabIndex = 1;
            this.messageDetailsGroupBox.TabStop = false;
            // 
            // systemCreateTimeUtc
            // 
            this.systemCreateTimeUtc.AllowDrop = true;
            this.systemCreateTimeUtc.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.systemCreateTimeUtc, "EM_SystemCreateTimeUtc");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_SystemCreateTimeUtc)));
            this.systemCreateTimeUtc.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("8064dd9b-e307-44ca-addd-5d3a4864fc91", "Create Time (UTC)");
            this.systemCreateTimeUtc.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
            this.systemCreateTimeUtc.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 129, true);
            this.systemCreateTimeUtc.Name = "systemCreateTimeUtc";
			this.systemCreateTimeUtc.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.systemCreateTimeUtc.TabIndex = 26;
            // 
            // applicationTypeTextBox
            // 
            this.BindingSource.SetBindingMember(this.applicationTypeTextBox, "ApplicationCodeWithDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).ApplicationCodeWithDescription)));
            this.applicationTypeTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|f6ef4ebb-bf38-4f8f-997f-501f2a863ea1", "Application Type");
            this.applicationTypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.applicationTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 17, true);
            this.applicationTypeTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.applicationTypeTextBox.Name = "applicationTypeTextBox";
            this.applicationTypeTextBox.ReadOnly = true;
            this.applicationTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.applicationTypeTextBox.TabIndex = 2;
            // 
            // messageTypeTextBox
            // 
            this.BindingSource.SetBindingMember(this.messageTypeTextBox, "MessageTypeWithDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).MessageTypeWithDescription)));
            this.messageTypeTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|457cec43-1d22-41fb-a978-a9520e29ca49", "Message Type", "EDI Message Type.");
            this.messageTypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.messageTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 39, true);
            this.messageTypeTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.messageTypeTextBox.Name = "messageTypeTextBox";
            this.messageTypeTextBox.ReadOnly = true;
            this.messageTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.messageTypeTextBox.TabIndex = 3;
            // 
            // messageSubTypeTextBox
            // 
            this.BindingSource.SetBindingMember(this.messageSubTypeTextBox, "MessageSubTypeWithDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).MessageSubTypeWithDescription)));
            this.messageSubTypeTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|ccb73aff-b1f8-458a-98ef-6c244c1c67ba", "Message Sub Type");
            this.messageSubTypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.messageSubTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 61, true);
            this.messageSubTypeTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.messageSubTypeTextBox.Name = "messageSubTypeTextBox";
            this.messageSubTypeTextBox.ReadOnly = true;
            this.messageSubTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.messageSubTypeTextBox.TabIndex = 4;
            // 
            // directionTextBox
            // 
            this.BindingSource.SetBindingMember(this.directionTextBox, "EM_SendOrReceiveHumanReadable");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_SendOrReceiveHumanReadable)));
            this.directionTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|20d4ed7d-020b-436b-8d38-0a9ad1c862f3", "Direction");
            this.directionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.directionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 83, true);
            this.directionTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.directionTextBox.Name = "directionTextBox";
            this.directionTextBox.ReadOnly = true;
            this.directionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.directionTextBox.TabIndex = 5;
			// 
			// transportTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.transportTypeTextBox, "EM_TransportType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_TransportType)));
			this.transportTypeTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|8fdcd76b-6b39-4aa1-9eb7-e24472444a74", "Transport Type");
			this.transportTypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.transportTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 107, true);
			this.transportTypeTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.transportTypeTextBox.Name = "transportTypeTextBox";
			this.transportTypeTextBox.ReadOnly = true;
			this.transportTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.transportTypeTextBox.TabIndex = 6;
			// 
			// externalReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.externalReferenceNumberTextBox, "EM_ExternalReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_ExternalReferenceNumber)));
			this.externalReferenceNumberTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|6458DD3C-7266-4724-8BD8-8209730C4A49", "External Reference No.");
			this.externalReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.externalReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 151, true);
			this.externalReferenceNumberTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.externalReferenceNumberTextBox.Name = "externalReferenceNumberTextBox";
			this.externalReferenceNumberTextBox.ReadOnly = true;
			this.externalReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.externalReferenceNumberTextBox.TabIndex = 41;
			// 
			// linkedEDIMessageNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.linkedEDIMessageNumberTextBox, "LinkedEDIMessageNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).LinkedEDIMessageNumber)));
			this.linkedEDIMessageNumberTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|0011ED21-1365-4402-BFFD-0A0BB49B59A6", "Linked Message No.");
			this.linkedEDIMessageNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.linkedEDIMessageNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 173, true);
			this.linkedEDIMessageNumberTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.linkedEDIMessageNumberTextBox.Name = "linkedEDIMessageNumberTextBox";
			this.linkedEDIMessageNumberTextBox.ReadOnly = true;
			this.linkedEDIMessageNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.linkedEDIMessageNumberTextBox.TabIndex = 42;
			// 
			// showLinkedEDIMessageButton
			// 
			this.showLinkedEDIMessageButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|6E120C9D-7BB9-4D7B-B75C-DF5B5EC03491", "View", "Brings up the linked EDI Request/Response Message for the current Message.");
			this.showLinkedEDIMessageButton.IsCaptionOverridden = false;
			this.showLinkedEDIMessageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 195, true);
			this.showLinkedEDIMessageButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.showLinkedEDIMessageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.showLinkedEDIMessageButton.Name = "showLinkedEDIMessageButton";
			this.showLinkedEDIMessageButton.TabIndex = 43;
			this.showLinkedEDIMessageButton.ToolTipCaption = null;
			this.showLinkedEDIMessageButton.UseVisualStyleBackColor = true;
			this.showLinkedEDIMessageButton.Click += new System.EventHandler(this.ShowLinkedEDIMessageButton_Click);
			// 
			// messageContentsPanel
			// 
			this.messageContentsPanel.Controls.Add(this.contentsGroupBox);
            this.messageContentsPanel.Controls.Add(this.senderReceiverPanel);
            this.messageContentsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messageContentsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 0, true);
            this.messageContentsPanel.Name = "messageContentsPanel";
            this.messageContentsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(579, 400, true);
            this.messageContentsPanel.TabIndex = 24;
            // 
            // contentsGroupBox
            // 
            this.contentsGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|b5c4c2a2-5c53-4050-b311-0631d8fde2b7", "Message Contents");
            this.contentsGroupBox.Controls.Add(this.MessageTextTextBox);
            this.contentsGroupBox.Controls.Add(this.zLabelTruncateNotification);
            this.contentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 57, true);
            this.contentsGroupBox.Name = "contentsGroupBox";
            this.contentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(579, 323, true);
            this.contentsGroupBox.TabIndex = 38;
            this.contentsGroupBox.TabStop = false;
            // 
            // MessageTextTextBox
            // 
            this.BindingSource.SetBindingMember(this.MessageTextTextBox, "EM_MessageTextDetail");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageTextDetail)));
            this.MessageTextTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|32e5a4f7-976f-40c4-9b5e-829b1e5a8b96", "Contents");
            this.MessageTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageTextTextBox.IsDynamicMultiline = true;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageTextTextBox, false);
            this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 41, true);
            this.MessageTextTextBox.Multiline = true;
            this.MessageTextTextBox.Name = "MessageTextTextBox";
            this.MessageTextTextBox.ReadOnly = true;
            this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 279, true);
            this.MessageTextTextBox.TabIndex = 40;
            // 
            // zLabelTruncateNotification
            // 
            this.zLabelTruncateNotification.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|74439e4f-eb48-42ae-a2df-57f233811be2", "Truncated text", "Truncated text", "Truncated text", "");
            this.zLabelTruncateNotification.Dock = System.Windows.Forms.DockStyle.Top;
            this.zLabelTruncateNotification.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabelTruncateNotification.ForeColor = System.Drawing.Color.Maroon;
            this.zLabelTruncateNotification.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.zLabelTruncateNotification.Name = "zLabelTruncateNotification";
            this.zLabelTruncateNotification.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 25, true);
            this.zLabelTruncateNotification.TabIndex = 39;
            this.zLabelTruncateNotification.UseMnemonic = false;
            // 
            // senderReceiverPanel
            // 
            this.senderReceiverPanel.Controls.Add(this.zTextBoxEdiClient);
            this.senderReceiverPanel.Controls.Add(this.SaveAndOpenButton);
            this.senderReceiverPanel.Controls.Add(this.receiverTextBox);
            this.senderReceiverPanel.Controls.Add(this.senderTextBox);
            this.senderReceiverPanel.Controls.Add(this.zButtonSaveRawMessage);
            this.senderReceiverPanel.Controls.Add(this.zButtonSaveFormatedMessage);
            this.senderReceiverPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.senderReceiverPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.senderReceiverPanel.Name = "senderReceiverPanel";
            this.senderReceiverPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(579, 57, true);
            this.senderReceiverPanel.TabIndex = 20;
            // 
            // zTextBoxEdiClient
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxEdiClient, "CommunicationPartyConfig.Party.Name");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).CommunicationPartyConfig.Party.Name)));
            this.zTextBoxEdiClient.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|b6a6534e-99fc-4be0-b45d-4d604d80358c", "EDI Client", "EDI Client associated with this EDI Message.");
            this.zTextBoxEdiClient.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.zTextBoxEdiClient.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 30, true);
            this.zTextBoxEdiClient.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 1, 2, 1, true);
            this.zTextBoxEdiClient.Name = "zTextBoxEdiClient";
            this.zTextBoxEdiClient.ReadOnly = true;
            this.zTextBoxEdiClient.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.zTextBoxEdiClient.TabIndex = 35;
            // 
            // SaveAndOpenButton
            // 
            this.SaveAndOpenButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.SaveAndOpenButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|915ae065-0549-47ea-b703-49fce0ddce03", "Save and Open", "Save and Open", "Save and Open", "");
            this.SaveAndOpenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 3, true);
            this.SaveAndOpenButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
            this.SaveAndOpenButton.Name = "SaveAndOpenButton";
            this.SaveAndOpenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 23, true);
            this.SaveAndOpenButton.TabIndex = 34;
            this.SaveAndOpenButton.ToolTipCaption = null;
            this.SaveAndOpenButton.UseVisualStyleBackColor = true;
            this.SaveAndOpenButton.Visible = false;
            this.SaveAndOpenButton.Click += new System.EventHandler(this.SaveAndOpenButton_Click);
            // 
            // receiverTextBox
            // 
            this.BindingSource.SetBindingMember(this.receiverTextBox, "Interchange.EI_To");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).Interchange.EI_To)));
            this.receiverTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|cfc66f76-f155-45f0-9ce8-920781a00ffe", "Receiver", "Receiver of this EDI Message.");
            this.receiverTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.receiverTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 30, true);
            this.receiverTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 1, 2, 1, true);
            this.receiverTextBox.Name = "receiverTextBox";
            this.receiverTextBox.ReadOnly = true;
            this.receiverTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
            this.receiverTextBox.TabIndex = 31;
            // 
            // senderTextBox
            // 
            this.BindingSource.SetBindingMember(this.senderTextBox, "Interchange.EI_From");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).Interchange.EI_From)));
            this.senderTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|a0daa7d1-731a-4925-934a-553f903ad07e", "Sender", "Sender of this EDI Message.");
            this.senderTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.senderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 8, true);
            this.senderTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.senderTextBox.Name = "senderTextBox";
            this.senderTextBox.ReadOnly = true;
            this.senderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
            this.senderTextBox.TabIndex = 30;
            // 
            // zButtonSaveRawMessage
            // 
            this.zButtonSaveRawMessage.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.zButtonSaveRawMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(285, 3, true);
            this.zButtonSaveRawMessage.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
            this.zButtonSaveRawMessage.Name = "zButtonSaveRawMessage";
            this.zButtonSaveRawMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 23, true);
            this.zButtonSaveRawMessage.TabIndex = 32;
            this.zButtonSaveRawMessage.ToolTipCaption = null;
            this.zButtonSaveRawMessage.UseVisualStyleBackColor = true;
            this.zButtonSaveRawMessage.Click += new System.EventHandler(this.zButtonSaveRawMessage_Click);
            // 
            // zButtonSaveFormatedMessage
            // 
            this.zButtonSaveFormatedMessage.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.zButtonSaveFormatedMessage.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|22e2aef2-6dce-46b1-9c09-27484019dcec", "Save To Disk", "Save To Disk", "Save To Disk", "");
            this.zButtonSaveFormatedMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 28, true);
            this.zButtonSaveFormatedMessage.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
            this.zButtonSaveFormatedMessage.Name = "zButtonSaveFormatedMessage";
            this.zButtonSaveFormatedMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 23, true);
            this.zButtonSaveFormatedMessage.TabIndex = 33;
            this.zButtonSaveFormatedMessage.ToolTipCaption = null;
            this.zButtonSaveFormatedMessage.UseVisualStyleBackColor = true;
            this.zButtonSaveFormatedMessage.Click += new System.EventHandler(this.zButtonSaveFormatedMessage_Click);
			// 
			// showInterchangeButton
			// 
			this.showInterchangeButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageStandAloneUserControl|DED2C52C-310F-41A2-B260-FB16B3FFAD22", "View", "Brings up the related EDI Interchange of the current Message.");
			this.showInterchangeButton.IsCaptionOverridden = false;
			this.showInterchangeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 106, true);
			this.showInterchangeButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.showInterchangeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.showInterchangeButton.Name = "showInterchangeButton";
			this.showInterchangeButton.TabIndex = 27;
			this.showInterchangeButton.ToolTipCaption = null;
			this.showInterchangeButton.UseVisualStyleBackColor = true;
			this.showInterchangeButton.Click += new System.EventHandler(this.ShowInterchangeButton_Click);
            // 
            // EDIMessageStandAloneUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.messageContentsPanel);
            this.Controls.Add(this.messageDetailsPanel);
            this.Name = "EDIMessageStandAloneUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 380, true);
            this.Load += new System.EventHandler(this.EDIMessageStandAloneUserControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.messageDetailsPanel.ResumeLayout(false);
            this.messageDetailsPanel.PerformLayout();
            this.interchangeDetailsGroupBox.ResumeLayout(false);
            this.interchangeDetailsGroupBox.PerformLayout();
            this.interchangeTimeDateEdit.ResumeLayout(true);
            this.interchangeTimeDateEdit.PerformLayout();
            this.processingDetailsGroupBox.ResumeLayout(false);
            this.processingDetailsGroupBox.PerformLayout();
            this.messageTimeDateEdit.ResumeLayout(true);
            this.messageTimeDateEdit.PerformLayout();
            this.messageDetailsGroupBox.ResumeLayout(false);
            this.messageDetailsGroupBox.PerformLayout();
            this.systemCreateTimeUtc.ResumeLayout(true);
            this.systemCreateTimeUtc.PerformLayout();
            this.messageContentsPanel.ResumeLayout(false);
            this.messageContentsPanel.PerformLayout();
            this.contentsGroupBox.ResumeLayout(false);
            this.contentsGroupBox.PerformLayout();
            this.senderReceiverPanel.ResumeLayout(false);
            this.senderReceiverPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
		}

		#endregion

		protected ZArchitecture.GUI.ZPanel messageDetailsPanel;
		protected ZArchitecture.GUI.ZPanel messageContentsPanel;
		private ZArchitecture.GUI.ZGroupBox contentsGroupBox;
		protected ZArchitecture.ZTextBox MessageTextTextBox;
		protected ZArchitecture.ZLabel zLabelTruncateNotification;
		private ZArchitecture.GUI.ZPanel senderReceiverPanel;
		protected ZArchitecture.ZTextBox receiverTextBox;
		protected ZArchitecture.ZTextBox senderTextBox;
		private ZArchitecture.GUI.ZButton zButtonSaveRawMessage;
		private ZArchitecture.GUI.ZButton zButtonSaveFormatedMessage;
		private ZArchitecture.GUI.ZGroupBox messageDetailsGroupBox;
		private ZArchitecture.ZTextBox applicationTypeTextBox;
		private ZArchitecture.ZTextBox messageTypeTextBox;
		protected ZArchitecture.ZTextBox messageSubTypeTextBox;
		protected ZArchitecture.ZTextBox directionTextBox;
		protected ZArchitecture.ZTextBox transportTypeTextBox;
		protected ZArchitecture.ZTextBox externalReferenceNumberTextBox;
		protected ZArchitecture.ZTextBox linkedEDIMessageNumberTextBox;
		protected ZArchitecture.GUI.ZButton showLinkedEDIMessageButton;
		protected ZArchitecture.GUI.ZGroupBox processingDetailsGroupBox;
		protected ZArchitecture.ZTextBox applicationReferenceTextBox;
		protected ZArchitecture.GUI.ZDateEdit messageTimeDateEdit;
		protected ZArchitecture.ZTextBox zTextBoxMessageNumber;
		private ZArchitecture.ZTextBox messageStatusTextBox;
		private ZArchitecture.GUI.ZGroupBox interchangeDetailsGroupBox;
		protected ZArchitecture.ZTextBox interchangeStatusTextBox;
		protected ZArchitecture.GUI.ZDateEdit interchangeTimeDateEdit;
		protected ZArchitecture.ZTextBox interchangeNumberTextBox;
		private ZArchitecture.GUI.ZButton SaveAndOpenButton;
		protected ZArchitecture.ZTextBox interchangeEHubIDTextBox;
		protected ZArchitecture.GUI.ZDateEdit systemCreateTimeUtc;
		protected ZArchitecture.ZTextBox zTextBoxEdiClient;
		protected ZArchitecture.GUI.ZButton showInterchangeButton;
		private int messageDetailsGroupBoxHeight;
	}
}
