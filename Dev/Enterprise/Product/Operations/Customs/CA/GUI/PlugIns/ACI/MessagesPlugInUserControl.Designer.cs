namespace Enterprise.Customs.CA.GUI
{
	partial class MessagesPlugInUserControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessagesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessageDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageTextZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HistoryMessageSplitter = new CargoWise.Windows.UI.KSplitter();
			this.HistoryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.HtmlInterpretationBox = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.MessagesTabControl.SuspendLayout();
			this.MessageDetailsTabPage.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CusSCAHouse);
			// 
			// splitter1
			// 
			this.splitter1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 16, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(5, 556, true);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.HistoryGroupBox);
			this.MainPanel.Controls.Add(this.HistoryMessageSplitter);
			this.MainPanel.Controls.Add(this.MessagesTabControl);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 608, true);
			this.MainPanel.TabIndex = 14;
			// 
			// MessagesTabControl
			// 
			this.MessagesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MessagesTabControl.Controls.Add(this.MessageDetailsTabPage);
			this.MessagesTabControl.Controls.Add(this.MessageTextTabPage);
			this.MessagesTabControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.MessagesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(609, 0, true);
			this.MessagesTabControl.Name = "MessagesTabControl";
			this.MessagesTabControl.SelectedIndex = 0;
			this.MessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 608, true);
			this.MessagesTabControl.TabIndex = 12;
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|ba938a2c-2bdc-4167-9e78-9afa5334d169", "Message Details");
			this.MessageDetailsTabPage.Controls.Add(this.HtmlInterpretationBox);
			this.MessageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageDetailsTabPage.Name = "MessageDetailsTabPage";
			this.MessageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 581, true);
			this.MessageDetailsTabPage.TabIndex = 1;
			this.MessageDetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|45e07741-4526-431f-ba83-634a59c9d223", "Message Text");
			this.MessageTextTabPage.Controls.Add(this.MessageTextZTextBox);
			this.MessageTextTabPage.Controls.Add(this.MessageTextTextBox);
			this.MessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTextTabPage.Name = "MessageTextTabPage";
			this.MessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 581, true);
			this.MessageTextTabPage.TabIndex = 0;
			// 
			// MessageTextZTextBox
			// 
			this.MessageTextZTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MessageTextZTextBox, "Messages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_FormattedMessageText)));
			this.MessageTextZTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageTextZTextBox.Multiline = true;
			this.MessageTextZTextBox.Name = "MessageTextZTextBox";
			this.MessageTextZTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageTextZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 575, true);
			this.MessageTextZTextBox.TabIndex = 2;
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "Messages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_FormattedMessageText)));
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 575, true);
			this.MessageTextTextBox.TabIndex = 1;
			// 
			// HistoryMessageSplitter
			// 
			this.HistoryMessageSplitter.Dock = System.Windows.Forms.DockStyle.Right;
			this.HistoryMessageSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(606, 0, true);
			this.HistoryMessageSplitter.Name = "HistoryMessageSplitter";
			this.HistoryMessageSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 608, true);
			this.HistoryMessageSplitter.TabIndex = 11;
			this.HistoryMessageSplitter.TabStop = false;
			// 
			// HistoryGroupBox
			// 
			this.HistoryGroupBox.Controls.Add(this.MessagesGrid);
			this.HistoryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HistoryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HistoryGroupBox.Name = "HistoryGroupBox";
			this.HistoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 608, true);
			this.HistoryGroupBox.TabIndex = 10;
			this.HistoryGroupBox.TabStop = false;
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_ReceiveTransmit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_InterchangeStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_ApplicationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_MessageSubTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_ApplicationReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_SendWithMessageErrors)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_IsActive)));
			this.MessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|96e80b7c-a984-4c29-8b98-072d48a0f440", "Message No.");
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|bf02c9c9-ad03-49f4-bb13-019155c6fc40", "Sub Type");
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|e13dd4cf-79c0-47d4-b59a-f8dc6e112cb9", "Status");
			zTextBoxColumnStyleInfo3.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|b9ca8a38-9be7-451f-bc43-e8b55c6b6833", "Message Time");
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|22cfc632-eda6-4e83-b3be-e4bf6628b16b", "Create Time (UTC)");
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|214f12ec-477e-4b7b-a15c-1f985e15bb92", "Interchange No.");
			zTextBoxColumnStyleInfo4.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|451acad4-ae33-43b8-b505-847e5698e2c2", "Interchange Sent");
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|b3c4f438-40be-48c1-bee4-a79a27cac8a1", "Sender");
			zTextBoxColumnStyleInfo5.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|21e64744-297e-42e3-bc48-104412b5e8f6", "Type");
			zTextBoxColumnStyleInfo6.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|5ad147f9-04a9-4f42-8ab7-bce9de79c9d0", "Direction");
			zTextBoxColumnStyleInfo7.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|abefbbe1-c3fa-4d7b-9157-b90a0c866b6f", "Interchange Status");
			zTextBoxColumnStyleInfo8.ColumnName = "EM_InterchangeStatus";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|54410713-ecb9-4d3f-81f4-d712b5c1f4c3", "Application Code");
			zTextBoxColumnStyleInfo9.ColumnName = "EM_ApplicationCode";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|1643ecc6-3a02-4e90-8b86-a59b03c74049", "Message Sub Type Description");
			zTextBoxColumnStyleInfo10.ColumnName = "EM_MessageSubTypeDescription";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|31a2dcfe-9bd4-4e06-be76-8a185983a4c2", "User");
			zTextBoxColumnStyleInfo11.ColumnName = "EM_SystemCreateUser";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|fbb49407-a5df-4d48-a22e-05fdd71e4e1a", "Unique Reference No. (URN)");
			zTextBoxColumnStyleInfo12.ColumnName = "EM_ApplicationReference";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|5788e8c1-1974-4219-8ebc-9fe1490b8b51", "Sent With Errors");
			zCheckBoxColumnStyleInfo1.ColumnName = "EM_SendWithMessageErrors";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MessagesPlugInUserControl|99454bb6-9078-4b59-a949-72a2631e4af8", "Active");
			zCheckBoxColumnStyleInfo2.ColumnName = "EM_IsActive";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.MessagesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.MessagesGrid.GridId = "0c39df3e-ae86-4079-a974-9b6949894e26";
			this.MessagesGrid.CopySelectedRowsAllowed = true;
			this.MessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesGrid.LayoutKey = "zGrid1";
			this.MessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessagesGrid.Name = "MessagesGrid";
			this.MessagesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 589, true);
			this.MessagesGrid.TabIndex = 2;
			// 
			// HtmlInterpretationBox
			// 
			this.HtmlInterpretationBox.AllowWebBrowserDrop = false;
			this.BindingSource.SetBindingMember(this.HtmlInterpretationBox, "Messages.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_MessageInterpretation)));
			this.HtmlInterpretationBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HtmlInterpretationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HtmlInterpretationBox.Name = "HtmlInterpretationBox";
			this.HtmlInterpretationBox.ScriptErrorsSuppressed = true;
			this.HtmlInterpretationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 575, true);
			this.HtmlInterpretationBox.TabIndex = 1;
			// 
			// MessagesPlugInUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Name = "MessagesPlugInUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 608, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MessagesTabControl.ResumeLayout(false);
			this.MessageDetailsTabPage.ResumeLayout(false);
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.HistoryGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitter splitter1;
		protected ZArchitecture.GUI.ZPanel MainPanel;
		protected ZArchitecture.GUI.ZTabControl MessagesTabControl;
		private ZArchitecture.GUI.ZTabPage MessageDetailsTabPage;
		protected ZArchitecture.GUI.ZTabPage MessageTextTabPage;
		protected ZArchitecture.ZTextBox MessageTextZTextBox;
		protected ZArchitecture.ZTextBox MessageTextTextBox;
		private CargoWise.Windows.UI.KSplitter HistoryMessageSplitter;
		protected internal ZArchitecture.GUI.ZGroupBox HistoryGroupBox;
		public ZArchitecture.ZGrid MessagesGrid;
		protected Enterprise.Messaging.GUI.HtmlInterpretationBox HtmlInterpretationBox;
	}
}
