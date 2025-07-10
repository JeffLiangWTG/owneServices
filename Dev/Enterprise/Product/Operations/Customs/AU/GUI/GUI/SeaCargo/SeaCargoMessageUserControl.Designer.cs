using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoMessageUserControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.leftPanel = new CargoWise.Windows.UI.KPanel();
			this.historyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.messagesGrid = new Enterprise.Messaging.GUI.MessageZGrid();
			this.theSplitter = new CargoWise.Windows.UI.KSplitter();
			this.rightPanel = new CargoWise.Windows.UI.KPanel();
			this.messageTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.messageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.leftPanel.SuspendLayout();
			this.historyGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.messagesGrid)).BeginInit();
			this.rightPanel.SuspendLayout();
			this.messageTextGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusSCAHouse);
			// 
			// LeftPanel
			// 
			this.leftPanel.Controls.Add(this.historyGroupBox);
			this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.leftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.leftPanel.Name = "LeftPanel";
			this.leftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 544, true);
			this.leftPanel.TabIndex = 2;
			// 
			// HistoryGroupBox
			// 
			this.historyGroupBox.Controls.Add(this.messagesGrid);
			this.historyGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.historyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.historyGroupBox.Name = "HistoryGroupBox";
			this.historyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 544, true);
			this.historyGroupBox.TabIndex = 0;
			this.historyGroupBox.TabStop = false;
			this.historyGroupBox.Text = "History";
			// 
			// MessagesGrid
			// 
			this.messagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.messagesGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_Status)));
			this.messagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Message";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.ToolTip = "Message Number";
			zTextBoxColumnStyleInfo2.Caption = "Sub Type";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ToolTip = "Message Sub Type";
			zDateEditColumnStyleInfo1.Caption = "Message Time";
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo2.Caption = "Create Time (UTC)";
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zTextBoxColumnStyleInfo3.Caption = "Interchange";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ToolTip = "Interchange Number";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.Caption = "Interchange DT";
			zDateEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.ToolTip = "Date Time Interchange sent";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ToolTip = "Message sent by user";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Caption = "Status";
			zTextBoxColumnStyleInfo5.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo5.ToolTip = "Message Status";
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.messagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.messagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.messagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.messagesGrid.GridId = "b4cbb6eb-3622-4e85-a1ce-39f5f97790f0";
			this.messagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.messagesGrid.LayoutKey = "zGrid1";
			this.messagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.messagesGrid.Name = "MessagesGrid";
			this.messagesGrid.ReadOnly = true;
			this.messagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 525, true);
			this.messagesGrid.TabIndex = 2;
			// 
			// TheSplitter
			// 
			this.theSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 0, true);
			this.theSplitter.Name = "TheSplitter";
			this.theSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 544, true);
			this.theSplitter.TabIndex = 3;
			this.theSplitter.TabStop = false;
			// 
			// RightPanel
			// 
			this.rightPanel.Controls.Add(this.messageTextGroupBox);
			this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(603, 0, true);
			this.rightPanel.Name = "RightPanel";
			this.rightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 544, true);
			this.rightPanel.TabIndex = 4;
			// 
			// MessageTextGroupBox
			// 
			this.messageTextGroupBox.Controls.Add(this.messageTextTextBox);
			this.messageTextGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messageTextGroupBox.Name = "MessageTextGroupBox";
			this.messageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 544, true);
			this.messageTextGroupBox.TabIndex = 0;
			this.messageTextGroupBox.TabStop = false;
			this.messageTextGroupBox.Text = "Message Text";
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.messageTextTextBox, "Messages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Messages)).SyncRoot)).EM_FormattedMessageText)));
			this.messageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.messageTextTextBox.Multiline = true;
			this.messageTextTextBox.Name = "MessageTextTextBox";
			this.messageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 525, true);
			this.messageTextTextBox.TabIndex = 0;
			// 
			// SeaCargoMessageUserControl
			// 
			this.Controls.Add(this.rightPanel);
			this.Controls.Add(this.theSplitter);
			this.Controls.Add(this.leftPanel);
			this.Name = "SeaCargoMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 544, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.leftPanel.ResumeLayout(false);
			this.historyGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.messagesGrid)).EndInit();
			this.rightPanel.ResumeLayout(false);
			this.messageTextGroupBox.ResumeLayout(false);
			this.messageTextGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}

		private ZGroupBox historyGroupBox;
		private ZGroupBox messageTextGroupBox;
		private CargoWise.Windows.UI.KPanel leftPanel;
		private CargoWise.Windows.UI.KPanel rightPanel;
		private Messaging.GUI.MessageZGrid messagesGrid;
		private ZArchitecture.ZTextBox messageTextTextBox;
		private CargoWise.Windows.UI.KSplitter theSplitter;
	}
}
