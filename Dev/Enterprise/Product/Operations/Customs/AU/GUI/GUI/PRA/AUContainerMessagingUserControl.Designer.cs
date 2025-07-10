using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.PRA.GUI
{
	public partial class AUContainerMessagingUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBox pRAStatusTextBox;
			Enterprise.ZArchitecture.ZLabel zLabel1;
			Enterprise.ZArchitecture.ZTextBox rawMessageTextBox;
			Enterprise.ZArchitecture.ZGrid messageGrid;
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.headerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.messageListSplitter = new CargoWise.Windows.UI.KSplitter();
			pRAStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			rawMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			messageGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(messageGrid)).BeginInit();
			this.headerPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.IPRAContainerMessaging);
			// 
			// PRAStatusTextBox
			// 
			this.BindingSource.SetBindingMember(pRAStatusTextBox, "CurrentPRAStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.IPRAContainerMessaging)(null)).CurrentPRAStatus)));
			pRAStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			pRAStatusTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUContainerMessagingUserControl|46b5b276-22c7-413d-a4c5-3db84ea00a3b", "Current PRA Status");
			pRAStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 8, true);
			pRAStatusTextBox.Name = "PRAStatusTextBox";
			pRAStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 20, true);
			pRAStatusTextBox.TabIndex = 1;
			// 
			// zLabel1
			// 
			zLabel1.AutoSize = true;
			zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 11, true);
			zLabel1.Name = "zLabel1";
			zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 13, true);
			zLabel1.TabIndex = 0;
			zLabel1.Text = "Current PRA Status:";
			// 
			// RawMessageTextBox
			// 
			this.BindingSource.SetBindingMember(rawMessageTextBox, "PRAMessages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Freight.Business.IPRAContainerMessaging)(null)).PRAMessages)).SyncRoot)).EM_FormattedMessageText)));
			rawMessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			rawMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(565, 40, true);
			rawMessageTextBox.Multiline = true;
			rawMessageTextBox.Name = "RawMessageTextBox";
			rawMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 512, true);
			rawMessageTextBox.TabIndex = 3;
			// 
			// MessageGrid
			// 
			messageGrid.AllowNavigation = false;
			messageGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(messageGrid, "PRAMessages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.IPRAContainerMessaging)(null)).PRAMessages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Freight.Business.IPRAContainerMessaging)(null)).PRAMessages)).SyncRoot)).EM_SendOrReceiveHumanReadable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Freight.Business.IPRAContainerMessaging)(null)).PRAMessages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Freight.Business.IPRAContainerMessaging)(null)).PRAMessages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Freight.Business.IPRAContainerMessaging)(null)).PRAMessages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Freight.Business.IPRAContainerMessaging)(null)).PRAMessages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Freight.Business.IPRAContainerMessaging)(null)).PRAMessages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Freight.Business.IPRAContainerMessaging)(null)).PRAMessages)).SyncRoot)).EM_MessageDescriptionFromSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Freight.Business.IPRAContainerMessaging)(null)).PRAMessages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Freight.Business.IPRAContainerMessaging)(null)).PRAMessages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Freight.Business.IPRAContainerMessaging)(null)).PRAMessages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Freight.Business.IPRAContainerMessaging)(null)).PRAMessages)).SyncRoot)).EM_MessageText)));
			messageGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Send Or Receive Human Readable";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUContainerMessagingUserControl|01c31351-3207-44a4-bd14-9d2fabc7b081", "Send Or Receive Human Readable");
			zTextBoxColumnStyleInfo1.ColumnName = "EM_SendOrReceiveHumanReadable";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "Message No.";
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Caption = "Message Time";
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo2.Caption = "Create Time (UTC)";
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Caption = "Date Interch. Sent";
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUContainerMessagingUserControl|da282c94-54f3-4587-bc9e-83c8cc92135e", "Date Interch. Sent", "Date Interchange Sent.");
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Caption = "Interch. No";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUContainerMessagingUserControl|a8a753d0-45df-49e1-9f21-724a85dbe2bb", "Interch. No", "Interchange No.");
			zTextBoxColumnStyleInfo3.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo4.Caption = "Message Description From Sub Type";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUContainerMessagingUserControl|1ba1deda-e914-4bc5-8f53-b535d816121d", "Message Description From Sub Type");
			zTextBoxColumnStyleInfo4.ColumnName = "EM_MessageDescriptionFromSubType";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Caption = "Status";
			zTextBoxColumnStyleInfo5.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Caption = "User";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUContainerMessagingUserControl|f44b2e01-1d3c-4707-9bfb-84af2b775f93", "User");
			zTextBoxColumnStyleInfo6.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Caption = "Type";
			zTextBoxColumnStyleInfo7.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo8.Caption = "Message Text";
			zTextBoxColumnStyleInfo8.ColumnName = "EM_MessageText";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.IsVisible = false;
			messageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			messageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			messageGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			messageGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			messageGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			messageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			messageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			messageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			messageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			messageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			messageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			messageGrid.GridId = "a1274a0d-7d3d-49a0-b2a5-2410688bccc1";
			messageGrid.Dock = System.Windows.Forms.DockStyle.Left;
			messageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			messageGrid.LayoutKey = "MessageGrid";
			messageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			messageGrid.Name = "MessageGrid";
			messageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 512, true);
			messageGrid.TabIndex = 1;
			// 
			// HeaderPanel
			// 
			this.headerPanel.Controls.Add(pRAStatusTextBox);
			this.headerPanel.Controls.Add(zLabel1);
			this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.headerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.headerPanel.Name = "HeaderPanel";
			this.headerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 40, true);
			this.headerPanel.TabIndex = 0;
			// 
			// MessageListSplitter
			// 
			this.messageListSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(562, 40, true);
			this.messageListSplitter.Name = "MessageListSplitter";
			this.messageListSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 512, true);
			this.messageListSplitter.TabIndex = 2;
			this.messageListSplitter.TabStop = false;
			// 
			// AUContainerMessagingUserControl
			// 
			this.Controls.Add(rawMessageTextBox);
			this.Controls.Add(this.messageListSplitter);
			this.Controls.Add(messageGrid);
			this.Controls.Add(this.headerPanel);
			this.Name = "AUContainerMessagingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 552, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(messageGrid)).EndInit();
			this.headerPanel.ResumeLayout(false);
			this.headerPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		CargoWise.Windows.UI.KSplitter messageListSplitter;
		ZPanel headerPanel;
	}
}
