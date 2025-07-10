using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SCDUserControl
	{
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
			this.leftPanel = new CargoWise.Windows.UI.KPanel();
			this.historyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.messagesGrid = new Enterprise.Messaging.GUI.MessageZGrid();
			this.theSplitter = new CargoWise.Windows.UI.KSplitter();
			this.rightPanel = new CargoWise.Windows.UI.KPanel();
			this.messageTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.messageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.topPanel = new CargoWise.Windows.UI.KPanel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.impendingArrivalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zGuidFindBox2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zGuidFindBox1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.lastErrorMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.messageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.bottomPanel = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.leftPanel.SuspendLayout();
			this.historyGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.messagesGrid)).BeginInit();
			this.rightPanel.SuspendLayout();
			this.messageTextGroupBox.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer);
			// 
			// LeftPanel
			// 
			this.leftPanel.Controls.Add(this.historyGroupBox);
			this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.leftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.leftPanel.Name = "LeftPanel";
			this.leftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 392, true);
			this.leftPanel.TabIndex = 2;
			// 
			// HistoryGroupBox
			// 
			this.historyGroupBox.Controls.Add(this.messagesGrid);
			this.historyGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.historyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.historyGroupBox.Name = "HistoryGroupBox";
			this.historyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 392, true);
			this.historyGroupBox.TabIndex = 0;
			this.historyGroupBox.TabStop = false;
			this.historyGroupBox.Text = "History";
			// 
			// MessagesGrid
			// 
			this.messagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.messagesGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).Messages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).Messages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).Messages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).Messages)).SyncRoot)).EM_Status)));
			this.messagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Message";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.ToolTip = "Message Number";
			zTextBoxColumnStyleInfo2.Caption = "Type";
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.Caption = "Sub Type";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ToolTip = "Message Sub Type";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.Caption = "Message Time";
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo2.Caption = "Create Time (UTC)";
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zTextBoxColumnStyleInfo4.Caption = "Interchange";
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ToolTip = "Interchange Number";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.Caption = "Interchange DT";
			zDateEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.ToolTip = "Date Time Interchange sent";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.ToolTip = "Message sent by user";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.Caption = "Status";
			zTextBoxColumnStyleInfo6.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo6.ToolTip = "Message Status";
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.messagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.messagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.messagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.messagesGrid.GridId = "0ca8ec3b-1c5a-4c16-a67f-e629e4921e54";
			this.messagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.messagesGrid.LayoutKey = "zGrid1";
			this.messagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.messagesGrid.Name = "MessagesGrid";
			this.messagesGrid.ReadOnly = true;
			this.messagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 373, true);
			this.messagesGrid.TabIndex = 2;
			// 
			// TheSplitter
			// 
			this.theSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 0, true);
			this.theSplitter.Name = "TheSplitter";
			this.theSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 392, true);
			this.theSplitter.TabIndex = 3;
			this.theSplitter.TabStop = false;
			// 
			// RightPanel
			// 
			this.rightPanel.Controls.Add(this.messageTextGroupBox);
			this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(603, 0, true);
			this.rightPanel.Name = "RightPanel";
			this.rightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 392, true);
			this.rightPanel.TabIndex = 4;
			// 
			// MessageTextGroupBox
			// 
			this.messageTextGroupBox.Controls.Add(this.messageTextTextBox);
			this.messageTextGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messageTextGroupBox.Name = "MessageTextGroupBox";
			this.messageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 392, true);
			this.messageTextGroupBox.TabIndex = 0;
			this.messageTextGroupBox.TabStop = false;
			this.messageTextGroupBox.Text = "Message Text";
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.messageTextTextBox, "Messages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).Messages)).SyncRoot)).EM_FormattedMessageText)));
			this.messageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.messageTextTextBox.Multiline = true;
			this.messageTextTextBox.Name = "MessageTextTextBox";
			this.messageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.messageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 373, true);
			this.messageTextTextBox.TabIndex = 0;
			// 
			// TopPanel
			// 
			this.topPanel.Controls.Add(this.zLabel3);
			this.topPanel.Controls.Add(this.impendingArrivalLabel);
			this.topPanel.Controls.Add(this.zGuidFindBox2);
			this.topPanel.Controls.Add(this.zGuidFindBox1);
			this.topPanel.Controls.Add(this.lastErrorMessageTextBox);
			this.topPanel.Controls.Add(this.zLabel2);
			this.topPanel.Controls.Add(this.zLabel1);
			this.topPanel.Controls.Add(this.messageStatusTextBox);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "TopPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 88, true);
			this.topPanel.TabIndex = 0;
			// 
			// zLabel3
			// 
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 40, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.zLabel3.TabIndex = 7;
			this.zLabel3.Text = "Cargo Status Advice Message:";
			// 
			// ImpendingArrivalLabel
			// 
			this.impendingArrivalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 8, true);
			this.impendingArrivalLabel.Name = "ImpendingArrivalLabel";
			this.impendingArrivalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.impendingArrivalLabel.TabIndex = 6;
			this.impendingArrivalLabel.Text = "Impending Arrival Message:";
			// 
			// zGuidFindBox2
			// 
			this.BindingSource.SetBindingMember(this.zGuidFindBox2, "CSAMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).CSAMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).CSAMessage_List)));
			this.zGuidFindBox2.BindToList = "CSAMessage_List";
			this.zGuidFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(584, 40, true);
			this.zGuidFindBox2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.AU.CusSCADepotContainer;
			this.zGuidFindBox2.Name = "zGuidFindBox2";
			this.zGuidFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.zGuidFindBox2.TabIndex = 5;
			// 
			// zGuidFindBox1
			// 
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "IMPMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).IMPMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).IMPMessage_List)));
			this.zGuidFindBox1.BindToList = "IMPMessage_List";
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(584, 8, true);
			this.zGuidFindBox1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.AU.CusSCADepotContainer;
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.zGuidFindBox1.TabIndex = 4;
			// 
			// LastErrorMessageTextBox
			// 
			this.BindingSource.SetBindingMember(this.lastErrorMessageTextBox, "LastErrorMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).LastErrorMessage)));
			this.lastErrorMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 40, true);
			this.lastErrorMessageTextBox.Multiline = true;
			this.lastErrorMessageTextBox.Name = "LastErrorMessageTextBox";
			this.lastErrorMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 40, true);
			this.lastErrorMessageTextBox.TabIndex = 3;
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel2.TabIndex = 2;
			this.zLabel2.Text = "Last Error:";
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.zLabel1.TabIndex = 1;
			this.zLabel1.Text = "Status:";
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.messageStatusTextBox, "MessageStateText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.SeaCargoDepotContainer)(null)).MessageStateText)));
			this.messageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 8, true);
			this.messageStatusTextBox.Name = "MessageStatusTextBox";
			this.messageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.messageStatusTextBox.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.bottomPanel.Controls.Add(this.rightPanel);
			this.bottomPanel.Controls.Add(this.theSplitter);
			this.bottomPanel.Controls.Add(this.leftPanel);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 88, true);
			this.bottomPanel.Name = "BottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 392, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// SCDUserControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.bottomPanel);
			this.Controls.Add(this.topPanel);
			this.Name = "SCDUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 480, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.leftPanel.ResumeLayout(false);
			this.historyGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.messagesGrid)).EndInit();
			this.rightPanel.ResumeLayout(false);
			this.messageTextGroupBox.ResumeLayout(false);
			this.messageTextGroupBox.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private ZGroupBox historyGroupBox;
		private ZGroupBox messageTextGroupBox;
		private CargoWise.Windows.UI.KPanel leftPanel;
		private CargoWise.Windows.UI.KPanel rightPanel;
		private Messaging.GUI.MessageZGrid messagesGrid;
		private ZArchitecture.ZTextBox messageTextTextBox;
		private CargoWise.Windows.UI.KSplitter theSplitter;
		private CargoWise.Windows.UI.KPanel topPanel;
		private ZArchitecture.ZTextBox messageStatusTextBox;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZTextBox lastErrorMessageTextBox;
		private ZGuidFindBox zGuidFindBox1;
		private ZGuidFindBox zGuidFindBox2;
		private ZArchitecture.ZLabel impendingArrivalLabel;
		private ZArchitecture.ZLabel zLabel3;
		private CargoWise.Windows.UI.KPanel bottomPanel;
	}
}
