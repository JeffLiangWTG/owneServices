using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class OrganisationPlugInUserControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.Customs.GUI.MessageCodeColumnStyleInfo messageCodeColumnStyleInfo1 = new Enterprise.Customs.GUI.MessageCodeColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.Customs.GUI.MessageCodeColumnStyleInfo messageCodeColumnStyleInfo2 = new Enterprise.Customs.GUI.MessageCodeColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.theSplitter = new CargoWise.Windows.UI.KSplitter();
			this.historyPanel = new CargoWise.Windows.UI.KPanel();
			this.messageTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EM_MessageTextBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.historyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OrderedMessagesBoundGrid = new Enterprise.Messaging.GUI.MessageZGrid();
			this.messageTextPanel = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.historyPanel.SuspendLayout();
			this.messageTextGroupBox.SuspendLayout();
			this.historyGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrderedMessagesBoundGrid)).BeginInit();
			this.messageTextPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper);
			// 
			// TheSplitter
			// 
			this.theSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(716, 0, true);
			this.theSplitter.Name = "TheSplitter";
			this.theSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 472, true);
			this.theSplitter.TabIndex = 3;
			this.theSplitter.TabStop = false;
			// 
			// HistoryPanel
			// 
			this.historyPanel.Controls.Add(this.messageTextGroupBox);
			this.historyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.historyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(719, 0, true);
			this.historyPanel.Name = "HistoryPanel";
			this.historyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 472, true);
			this.historyPanel.TabIndex = 4;
			// 
			// MessageTextGroupBox
			// 
			this.messageTextGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("OrganisationPlugInUserControl|5d45ab7b-9588-4a54-832c-6ae5b5ba022a", "Message Text");
			this.messageTextGroupBox.Controls.Add(this.EM_MessageTextBoundTextBox);
			this.messageTextGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messageTextGroupBox.Name = "MessageTextGroupBox";
			this.messageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 472, true);
			this.messageTextGroupBox.TabIndex = 0;
			this.messageTextGroupBox.TabStop = false;
			// 
			// EM_MessageTextBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.EM_MessageTextBoundTextBox, "Messages.HumanReadableMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).HumanReadableMessage)));
			this.EM_MessageTextBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EM_MessageTextBoundTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EM_MessageTextBoundTextBox, false);
			this.EM_MessageTextBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EM_MessageTextBoundTextBox.Multiline = true;
			this.EM_MessageTextBoundTextBox.Name = "EM_MessageTextBoundTextBox";
			this.EM_MessageTextBoundTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.EM_MessageTextBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 453, true);
			this.EM_MessageTextBoundTextBox.TabIndex = 0;
			// 
			// HistoryGroupBox
			// 
			this.historyGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("OrganisationPlugInUserControl|6ea6456b-678b-4b7a-8241-fbdbd8c00e6e", "History");
			this.historyGroupBox.Controls.Add(this.OrderedMessagesBoundGrid);
			this.historyGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.historyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.historyGroupBox.Name = "HistoryGroupBox";
			this.historyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 472, true);
			this.historyGroupBox.TabIndex = 0;
			this.historyGroupBox.TabStop = false;
			// 
			// OrderedMessagesBoundGrid
			// 
			this.OrderedMessagesBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrderedMessagesBoundGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).EM_MessageSubTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).EM_ReceiveTransmit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).EM_Status)));
			this.OrderedMessagesBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.ToolTip = "Message Number";
			messageCodeColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			messageCodeColumnStyleInfo1.ColumnName = "EM_MessageType";
			messageCodeColumnStyleInfo1.ToolTip = "Message Type";
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			messageCodeColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			messageCodeColumnStyleInfo2.ColumnName = "EM_MessageSubType";
			messageCodeColumnStyleInfo2.ToolTip = "Message Sub Type";
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageSubTypeDescription";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ToolTip = "Interchange Number";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.ToolTip = "Date Time Interchange Sent";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ToolTip = "Message Sent by User ";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			this.OrderedMessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrderedMessagesBoundGrid.ColumnStyles.Add(messageCodeColumnStyleInfo1);
			this.OrderedMessagesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OrderedMessagesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.OrderedMessagesBoundGrid.ColumnStyles.Add(messageCodeColumnStyleInfo2);
			this.OrderedMessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrderedMessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OrderedMessagesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.OrderedMessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.OrderedMessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.OrderedMessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.OrderedMessagesBoundGrid.CopySelectedRowsAllowed = true;
			this.OrderedMessagesBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrderedMessagesBoundGrid.GridId = "244e9d0f-a188-43b2-a862-17288274566f";
			this.OrderedMessagesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrderedMessagesBoundGrid.LayoutKey = "zGrid1";
			this.OrderedMessagesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OrderedMessagesBoundGrid.Name = "OrderedMessagesBoundGrid";
			this.OrderedMessagesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 453, true);
			this.OrderedMessagesBoundGrid.TabIndex = 2;
			// 
			// MessageTextPanel
			// 
			this.messageTextPanel.Controls.Add(this.historyGroupBox);
			this.messageTextPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.messageTextPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messageTextPanel.Name = "MessageTextPanel";
			this.messageTextPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 472, true);
			this.messageTextPanel.TabIndex = 2;
			// 
			// OrganisationPlugInUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.historyPanel);
			this.Controls.Add(this.theSplitter);
			this.Controls.Add(this.messageTextPanel);
			this.Name = "OrganisationPlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 472, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.historyPanel.ResumeLayout(false);
			this.messageTextGroupBox.ResumeLayout(false);
			this.messageTextGroupBox.PerformLayout();
			this.historyGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.OrderedMessagesBoundGrid)).EndInit();
			this.messageTextPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private CargoWise.Windows.UI.KSplitter theSplitter;
		private CargoWise.Windows.UI.KPanel historyPanel;
		private ZGroupBox messageTextGroupBox;
		private ZGroupBox historyGroupBox;
		public Messaging.GUI.MessageZGrid OrderedMessagesBoundGrid;
		private CargoWise.Windows.UI.KPanel messageTextPanel;
		public ZArchitecture.ZTextBox EM_MessageTextBoundTextBox;
	}
}
