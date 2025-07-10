using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class ExportMessagesUserControl
	{
		void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZTextBoxColumnStyleInfo();
			this.inBondHeadersHistorySplitter = new CargoWise.Windows.UI.KSplitter();
			this.messageDetailsTabPage = new ZTabPage();
			this.relatedRecordsGroupBox = new ZGroupBox();
			this.entriesGrid = new ZGrid();
			this.htmlInterpretationBox = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			this.MainPanel.SuspendLayout();
			this.MessagesTabControl.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.messageDetailsTabPage.SuspendLayout();
			this.relatedRecordsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.entriesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 534, true);
			// 
			// MessagesTabControl
			// 
			this.MessagesTabControl.Controls.Add(this.messageDetailsTabPage);
			this.MessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 534, true);
			this.MessagesTabControl.TabIndex = 0;
			this.MessagesTabControl.Controls.SetChildIndex(this.messageDetailsTabPage, 0);
			this.MessagesTabControl.Controls.SetChildIndex(this.MessageTextTabPage, 0);
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 507, true);
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "CustomsEntryHeaders.Messages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_FormattedMessageText);
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 501, true);
			this.MessageTextTextBox.WordWrap = false;
			// 
			// HistoryGroupBox
			// 
			this.HistoryGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ExportMessagesUserControl|069ae9d9-70a8-4aa5-892e-9225f1831902", "Message History");
			this.HistoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(559, 534, true);
			this.HistoryGroupBox.TabIndex = 1;
			// 
			// MessagesGrid
			// 
			this.BindingSource.SetBindingMember(this.MessagesGrid, "CustomsEntryHeaders.Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_MessageNum);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_MessageSubType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_MessageDateTime);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_InterchangeNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_DateTimeInterchangeSent);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_User);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_MessageSubTypeDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_Status);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_IsActive);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_ApplicationCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_ApplicationReference);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_InterchangeStatus);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_MessageType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_ReceiveTransmit);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_SystemCreateUser);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_SendWithMessageErrors);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_MessageDescriptionFromSubType);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ExportMessagesUserControl|a2a5ecce-1768-4bc9-98f9-89b81dc23293", "Message Type Desc.");
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageSubTypeDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo2.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.ColumnName = "EM_IsActive";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ExportMessagesUserControl|4d0870a0-0cc1-469d-91f6-5648aa38b4e6", "App. Code");
			zTextBoxColumnStyleInfo3.ColumnName = "EM_ApplicationCode";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ExportMessagesUserControl|972bd3ba-233e-4845-9c29-4dd1395948d6", "App. Ref");
			zTextBoxColumnStyleInfo4.ColumnName = "EM_ApplicationReference";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ExportMessagesUserControl|b4f40ce1-8113-46a6-98e8-1c2a20e9e199", "Int Status");
			zTextBoxColumnStyleInfo5.ColumnName = "EM_InterchangeStatus";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo6.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo7.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo8.ColumnName = "EM_SystemCreateUser";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zCheckBoxColumnStyleInfo2.ColumnName = "EM_SendWithMessageErrors";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ExportMessagesUserControl|b30ffa8b-3da8-4b36-9ca3-e94359460d16", "Message Sub Type");
			zTextBoxColumnStyleInfo9.ColumnName = "EM_MessageDescriptionFromSubType";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MessagesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 515, true);
			this.MessagesGrid.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.JobDeclaration);
			// 
			// InBondHeadersHistorySplitter
			// 
			this.inBondHeadersHistorySplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.inBondHeadersHistorySplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 109, true);
			this.inBondHeadersHistorySplitter.Name = "InBondHeadersHistorySplitter";
			this.inBondHeadersHistorySplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 3, true);
			this.inBondHeadersHistorySplitter.TabIndex = 2;
			this.inBondHeadersHistorySplitter.TabStop = false;
			// 
			// MessageDetailsTabPage
			// 
			this.messageDetailsTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ExportMessagesUserControl|5886a959-45f9-4d37-93e3-ff1e1d8d4ab0", "Message Details");
			this.messageDetailsTabPage.Controls.Add(this.htmlInterpretationBox);
			this.messageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.messageDetailsTabPage.Name = "MessageDetailsTabPage";
			this.messageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.messageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 507, true);
			this.messageDetailsTabPage.TabIndex = 0;
			// 
			// RelatedRecordsGroupBox
			// 
			this.relatedRecordsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ExportMessagesUserControl|8fd3bace-5226-4fb4-981a-dad7896eaffa", "Message History");
			this.relatedRecordsGroupBox.Controls.Add(this.entriesGrid);
			this.relatedRecordsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.relatedRecordsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.relatedRecordsGroupBox.Name = "RelatedRecordsGroupBox";
			this.relatedRecordsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 109, true);
			this.relatedRecordsGroupBox.TabIndex = 0;
			this.relatedRecordsGroupBox.TabStop = false;
			// 
			// EntriesGrid
			// 
			this.entriesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.entriesGrid, "CustomsEntryHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.JobDeclaration)(null)).CustomsEntryHeaders);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).CH_BGMReference);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).CH_Status);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).MessageStatusDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).CH_EntryStatus);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).CH_CERSProofOfReportNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).EntryNumber);
			this.entriesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ExportMessagesUserControl|51827cde-68e6-4ff8-b6b4-026ff8bc762c", "Reference");
			zTextBoxColumnStyleInfo10.ColumnName = "CH_BGMReference";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.ColumnName = "CH_Status";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ExportMessagesUserControl|f22652f0-03a2-4e39-998a-3d38ed93d054", "Status Description");
			zTextBoxColumnStyleInfo12.ColumnName = "MessageStatusDescription";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ExportMessagesUserControl|89e40695-5d17-48be-9bfa-9c9ad79ee1c1", "Entry Status");
			zTextBoxColumnStyleInfo13.ColumnName = "CH_EntryStatus";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ExportMessagesUserControl|ba9ff237-1ff5-494f-a707-139713c0fc13", "Proof of Report #");
			zTextBoxColumnStyleInfo14.ColumnName = "CH_CERSProofOfReportNumber";
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ExportMessagesUserControl|afc29ec0-549b-470e-a9c0-e5a5a49cb2ab", "Transaction #");
			zTextBoxColumnStyleInfo15.ColumnName = "EntryNumber";
			zTextBoxColumnStyleInfo15.IsReadOnly = true;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.entriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.entriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.entriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.entriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.entriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.entriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.entriesGrid.GridId = "abcdd923-3e88-4cb7-9457-849265e92a17";
			this.entriesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.entriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.entriesGrid.LayoutKey = "EntriesGrid";
			this.entriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.entriesGrid.Name = "EntriesGrid";
			this.entriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 90, true);
			this.entriesGrid.TabIndex = 0;
			// 
			// HtmlInterpretationBox
			// 
			this.htmlInterpretationBox.AllowWebBrowserDrop = false;
			this.BindingSource.SetBindingMember(this.htmlInterpretationBox, "CustomsEntryHeaders.Messages.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((Business.JobDeclaration)(null)).CustomsEntryHeaders.SyncRoot)).Messages)).SyncRoot)).EM_MessageInterpretation);
			this.htmlInterpretationBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.htmlInterpretationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.htmlInterpretationBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.htmlInterpretationBox.Name = "HtmlInterpretationBox";
			this.htmlInterpretationBox.ScriptErrorsSuppressed = true;
			this.htmlInterpretationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 501, true);
			this.htmlInterpretationBox.TabIndex = 1;
			this.htmlInterpretationBox.UseFixedWidthForPlainText = true;
			// 
			// ExportMessagesUserControl
			// 
			this.Controls.Add(this.inBondHeadersHistorySplitter);
			this.Controls.Add(this.relatedRecordsGroupBox);
			this.Name = "ExportMessagesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 646, true);
			this.Controls.SetChildIndex(this.relatedRecordsGroupBox, 0);
			this.Controls.SetChildIndex(this.inBondHeadersHistorySplitter, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.MainPanel.ResumeLayout(false);
			this.MessagesTabControl.ResumeLayout(false);
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.HistoryGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.messageDetailsTabPage.ResumeLayout(false);
			this.messageDetailsTabPage.PerformLayout();
			this.relatedRecordsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.entriesGrid)).EndInit();
			this.ResumeLayout(false);
		}

		CargoWise.Windows.UI.KSplitter inBondHeadersHistorySplitter;
		ZTabPage messageDetailsTabPage;
		ZGroupBox relatedRecordsGroupBox;
		ZGrid entriesGrid;
		Enterprise.Messaging.GUI.HtmlInterpretationBox htmlInterpretationBox;
	}
}
