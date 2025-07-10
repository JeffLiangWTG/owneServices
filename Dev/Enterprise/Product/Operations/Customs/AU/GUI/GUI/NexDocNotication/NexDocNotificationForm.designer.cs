namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class NexDocNotificationForm
	{
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MessagesGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessagesContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MessagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessageTextGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageContentTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessageSummaryTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageSummaryBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExporterReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RexNumberNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DetailsGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AcknowledgementStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransferringExporterIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ForwardingGroupIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReceivingExporterIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReceivedDateTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotificationTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessagesGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesContainer)).BeginInit();
			this.MessagesContainer.Panel1.SuspendLayout();
			this.MessagesContainer.Panel2.SuspendLayout();
			this.MessagesContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessagesGrid.SuspendLayout();
			this.MessageTextGroup.SuspendLayout();
			this.MessageContentTabControl.SuspendLayout();
			this.MessageSummaryTabPage.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.DetailsGroup.SuspendLayout();
			this.ReceivedDateTimeDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(921, 485, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.MessagesGroup);
			this.MainTabPage.Controls.Add(this.DetailsGroup);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(913, 458, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(913, 458, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(913, 458, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(921, 485, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(921, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification);
			// 
			// MessagesGroup
			// 
			this.MessagesGroup.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("13a4c910-92df-4b7d-baad-0e58a28610d4", "Messages");
			this.MessagesGroup.Controls.Add(this.MessagesContainer);
			this.MessagesGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 165, true);
			this.MessagesGroup.Name = "MessagesGroup";
			this.MessagesGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(913, 293, true);
			this.MessagesGroup.TabIndex = 0;
			this.MessagesGroup.TabStop = false;
			// 
			// MessagesContainer
			// 
			this.MessagesContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessagesContainer.Name = "MessagesContainer";
			// 
			// MessagesContainer.Panel1
			// 
			this.MessagesContainer.Panel1.Controls.Add(this.MessagesGrid);
			// 
			// MessagesContainer.Panel2
			// 
			this.MessagesContainer.Panel2.Controls.Add(this.MessageTextGroup);
			this.MessagesContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(907, 274, true);
			this.MessagesContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(575);
			this.MessagesContainer.TabIndex = 0;
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).Messages)).SyncRoot)).EM_ReceiveTransmit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).Messages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).Messages)).SyncRoot)).EM_InterchangeStatus)));
			this.MessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104);
			zTextBoxColumnStyleInfo2.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(62);
			zTextBoxColumnStyleInfo4.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(52);
			zTextBoxColumnStyleInfo5.ColumnName = "EM_InterchangeStatus";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGrid.GridId = "89634c52-3d54-42e6-8865-087c7dea5f08";
			this.MessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesGrid.LayoutKey = "MessagesGrid";
			this.MessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesGrid.Name = "MessagesGrid";
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 274, true);
			this.MessagesGrid.TabIndex = 0;
			// 
			// MessageTextGroup
			// 
			this.MessageTextGroup.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("7cdaafa7-3005-41bb-943b-a1413acc1adf", "Message Text");
			this.MessageTextGroup.Controls.Add(this.MessageContentTabControl);
			this.MessageTextGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageTextGroup.Name = "MessageTextGroup";
			this.MessageTextGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 274, true);
			this.MessageTextGroup.TabIndex = 0;
			this.MessageTextGroup.TabStop = false;
			// 
			// MessageContentTabControl
			// 
			this.MessageContentTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MessageContentTabControl.Controls.Add(this.MessageSummaryTabPage);
			this.MessageContentTabControl.Controls.Add(this.MessageTextTabPage);
			this.MessageContentTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageContentTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageContentTabControl.Name = "MessageContentTabControl";
			this.MessageContentTabControl.SelectedIndex = 0;
			this.MessageContentTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 255, true);
			this.MessageContentTabControl.TabIndex = 0;
			// 
			// MessageSummaryTabPage
			// 
			this.MessageSummaryTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("f66f5156-f90d-42a1-a565-321ebe34d0ce", "Message Summary");
			this.MessageSummaryTabPage.Controls.Add(this.MessageSummaryBox);
			this.MessageSummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageSummaryTabPage.Name = "MessageSummaryTabPage";
			this.MessageSummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 228, true);
			this.MessageSummaryTabPage.TabIndex = 0;
			// 
			// MessageSummaryBox
			// 
			this.MessageSummaryBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("c57d6e8c-c299-4abc-88b9-6cbf69269999", "Message Summary");
			this.MessageSummaryBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageSummaryBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageSummaryBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageSummaryBox.Multiline = true;
			this.MessageSummaryBox.Name = "MessageSummaryBox";
			this.MessageSummaryBox.ReadOnly = true;
			this.MessageSummaryBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageSummaryBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 228, true);
			this.MessageSummaryBox.TabIndex = 0;
			this.MessageSummaryBox.WordWrap = false;
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("42194602-631d-46c7-bddb-cf52f22bfc63", "Message Text");
			this.MessageTextTabPage.Controls.Add(this.MessageTextBox);
			this.MessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTextTabPage.Name = "MessageTextTabPage";
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 228, true);
			this.MessageTextTabPage.TabIndex = 1;
			// 
			// MessageTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextBox, "Messages.EM_MessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).Messages)).SyncRoot)).EM_MessageText)));
			this.MessageTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("68f94347-99d7-4f8a-b17e-cb401501f021", "Message Text");
			this.MessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageTextBox.Multiline = true;
			this.MessageTextBox.Name = "MessageTextBox";
			this.MessageTextBox.ReadOnly = true;
			this.MessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 228, true);
			this.MessageTextBox.TabIndex = 0;
			// 
			// ExporterReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExporterReferenceTextBox, "QN_ExporterReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).QN_ExporterReference)));
			this.ExporterReferenceTextBox.CaptionResourceString = null;
			this.ExporterReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 67, true);
			this.ExporterReferenceTextBox.Name = "ExporterReferenceTextBox";
			this.ExporterReferenceTextBox.ReadOnly = true;
			this.ExporterReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.ExporterReferenceTextBox.TabIndex = 3;
			// 
			// RexNumberNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.RexNumberNameTextBox, "QN_RexNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).QN_RexNumber)));
			this.RexNumberNameTextBox.CaptionResourceString = null;
			this.RexNumberNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 42, true);
			this.RexNumberNameTextBox.Name = "RexNumberNameTextBox";
			this.RexNumberNameTextBox.ReadOnly = true;
			this.RexNumberNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.RexNumberNameTextBox.TabIndex = 2;
			// 
			// DetailsGroup
			// 
			this.DetailsGroup.Controls.Add(this.AcknowledgementStatusTextBox);
			this.DetailsGroup.Controls.Add(this.TransferringExporterIDTextBox);
			this.DetailsGroup.Controls.Add(this.ForwardingGroupIDTextBox);
			this.DetailsGroup.Controls.Add(this.ReceivingExporterIDTextBox);
			this.DetailsGroup.Controls.Add(this.ReceivedDateTimeDateEdit);
			this.DetailsGroup.Controls.Add(this.MessageStatusTextBox);
			this.DetailsGroup.Controls.Add(this.RexNumberNameTextBox);
			this.DetailsGroup.Controls.Add(this.ExporterReferenceTextBox);
			this.DetailsGroup.Controls.Add(this.NotificationTypeTextBox);
			this.DetailsGroup.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DetailsGroup, false);
			this.DetailsGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroup.Name = "DetailsGroup";
			this.DetailsGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(913, 165, true);
			this.DetailsGroup.TabIndex = 0;
			this.DetailsGroup.TabStop = false;
			// 
			// AcknowledgementStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.AcknowledgementStatusTextBox, "AcknowledgeStatusAndDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).AcknowledgeStatusAndDescription)));
			this.AcknowledgementStatusTextBox.CaptionResourceString = null;
			this.AcknowledgementStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(585, 122, true);
			this.AcknowledgementStatusTextBox.Name = "AcknowledgementStatusTextBox";
			this.AcknowledgementStatusTextBox.ReadOnly = true;
			this.AcknowledgementStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.AcknowledgementStatusTextBox.TabIndex = 9;
			// 
			// TransferringExporterIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransferringExporterIDTextBox, "QN_TransferringExporterID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).QN_TransferringExporterID)));
			this.TransferringExporterIDTextBox.CaptionResourceString = null;
			this.TransferringExporterIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(585, 94, true);
			this.TransferringExporterIDTextBox.Name = "TransferringExporterIDTextBox";
			this.TransferringExporterIDTextBox.ReadOnly = true;
			this.TransferringExporterIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.TransferringExporterIDTextBox.TabIndex = 8;
			// 
			// ForwardingGroupIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.ForwardingGroupIDTextBox, "QN_ForwardingGroupID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).QN_ForwardingGroupID)));
			this.ForwardingGroupIDTextBox.CaptionResourceString = null;
			this.ForwardingGroupIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(585, 67, true);
			this.ForwardingGroupIDTextBox.Name = "ForwardingGroupIDTextBox";
			this.ForwardingGroupIDTextBox.ReadOnly = true;
			this.ForwardingGroupIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.ForwardingGroupIDTextBox.TabIndex = 7;
			// 
			// ReceivingExporterIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReceivingExporterIDTextBox, "QN_ReceivingExporterID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).QN_ReceivingExporterID)));
			this.ReceivingExporterIDTextBox.CaptionResourceString = null;
			this.ReceivingExporterIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(585, 42, true);
			this.ReceivingExporterIDTextBox.Name = "ReceivingExporterIDTextBox";
			this.ReceivingExporterIDTextBox.ReadOnly = true;
			this.ReceivingExporterIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.ReceivingExporterIDTextBox.TabIndex = 6;
			// 
			// ReceivedDateTimeDateEdit
			// 
			this.ReceivedDateTimeDateEdit.AllowDrop = true;
			this.ReceivedDateTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReceivedDateTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReceivedDateTimeDateEdit, "QN_ReceivedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).QN_ReceivedDate)));
			this.ReceivedDateTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReceivedDateTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(585, 17, true);
			this.ReceivedDateTimeDateEdit.Name = "ReceivedDateTimeDateEdit";
			this.ReceivedDateTimeDateEdit.TabIndex = 5;
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "MessageStatusAndDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).MessageStatusAndDescription)));
			this.MessageStatusTextBox.CaptionResourceString = null;
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 122, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.ReadOnly = true;
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.MessageStatusTextBox.TabIndex = 4;
			// 
			// NotificationTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotificationTypeTextBox, "QN_NotificationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification)(null)).QN_NotificationType)));
			this.NotificationTypeTextBox.CaptionResourceString = null;
			this.NotificationTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 17, true);
			this.NotificationTypeTextBox.Name = "NotificationTypeTextBox";
			this.NotificationTypeTextBox.ReadOnly = true;
			this.NotificationTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.NotificationTypeTextBox.TabIndex = 1;
			// 
			// NexDocNotificationForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(921, 541, true);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.QuarantineNexDocNotification";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(935, 573, true);
			this.Name = "NexDocNotificationForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagesGroup.ResumeLayout(false);
			this.MessagesGroup.PerformLayout();
			this.MessagesContainer.Panel1.ResumeLayout(false);
			this.MessagesContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesContainer)).EndInit();
			this.MessagesContainer.ResumeLayout(false);
			this.MessagesContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessagesGrid.ResumeLayout(false);
			this.MessagesGrid.PerformLayout();
			this.MessageTextGroup.ResumeLayout(false);
			this.MessageTextGroup.PerformLayout();
			this.MessageContentTabControl.ResumeLayout(false);
			this.MessageContentTabControl.PerformLayout();
			this.MessageSummaryTabPage.ResumeLayout(false);
			this.MessageSummaryTabPage.PerformLayout();
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.DetailsGroup.ResumeLayout(false);
			this.DetailsGroup.PerformLayout();
			this.ReceivedDateTimeDateEdit.ResumeLayout(true);
			this.ReceivedDateTimeDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZGroupBox MessagesGroup;
		private Enterprise.ZArchitecture.ZGrid MessagesGrid;
		private CargoWise.Windows.UI.KSplitContainer MessagesContainer;
		private ZArchitecture.GUI.ZGroupBox DetailsGroup;
		private ZArchitecture.ZTextBox RexNumberNameTextBox;
		private ZArchitecture.ZTextBox ExporterReferenceTextBox;
		private ZArchitecture.GUI.ZGroupBox MessageTextGroup;
		private ZArchitecture.ZTextBox MessageSummaryBox;
		private ZArchitecture.ZTextBox MessageTextBox;
		private ZArchitecture.GUI.ZTabPage MessageTextTabPage;
		private ZArchitecture.GUI.ZTabPage MessageSummaryTabPage;
		private ZArchitecture.GUI.ZTabControl MessageContentTabControl;
		private ZArchitecture.ZTextBox NotificationTypeTextBox;
		private ZArchitecture.GUI.ZDateEdit ReceivedDateTimeDateEdit;
		private ZArchitecture.ZTextBox MessageStatusTextBox;
		private ZArchitecture.ZTextBox AcknowledgementStatusTextBox;
		private ZArchitecture.ZTextBox TransferringExporterIDTextBox;
		private ZArchitecture.ZTextBox ForwardingGroupIDTextBox;
		private ZArchitecture.ZTextBox ReceivingExporterIDTextBox;
		private System.ComponentModel.IContainer components;
	}
}
