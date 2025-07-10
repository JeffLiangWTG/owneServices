
namespace Enterprise.Customs.HK.GUI
{
	partial class TraxonMessageUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MessagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.HistoryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TopPanel = new CargoWise.Windows.UI.KPanel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.Traxon_MessageStatusBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HistoryAndMessageTestPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TheSplitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessagesGrid.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			this.MessageTextGroupBox.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.HistoryAndMessageTestPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.HK.Business.TraxonConsolStatus);
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesGrid, "OrderedMessages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.HK.Business.TraxonConsolStatus)(null)).OrderedMessages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.HK.Business.TraxonMessage)(((System.Collections.IList)(((Enterprise.Customs.HK.Business.TraxonConsolStatus)(null)).OrderedMessages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.HK.Business.TraxonMessage)(((System.Collections.IList)(((Enterprise.Customs.HK.Business.TraxonConsolStatus)(null)).OrderedMessages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.HK.Business.TraxonMessage)(((System.Collections.IList)(((Enterprise.Customs.HK.Business.TraxonConsolStatus)(null)).OrderedMessages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.HK.Business.TraxonMessage)(((System.Collections.IList)(((Enterprise.Customs.HK.Business.TraxonConsolStatus)(null)).OrderedMessages)).SyncRoot)).EM_ReceiveTransmit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.HK.Business.TraxonMessage)(((System.Collections.IList)(((Enterprise.Customs.HK.Business.TraxonConsolStatus)(null)).OrderedMessages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.HK.Business.TraxonMessage)(((System.Collections.IList)(((Enterprise.Customs.HK.Business.TraxonConsolStatus)(null)).OrderedMessages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.HK.Business.TraxonMessage)(((System.Collections.IList)(((Enterprise.Customs.HK.Business.TraxonConsolStatus)(null)).OrderedMessages)).SyncRoot)).EM_User)));
			this.MessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Message";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.ToolTip = "Message Number";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.Caption = "Message Time";
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.Caption = "Create Time (UTC)";
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "Send/Receive";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo2.ToolTip = "Send/Receive";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.Caption = "Interchange";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo3.ToolTip = "Interchange Number";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.Caption = "Interchange DT";
			zDateEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.ToolTip = "Date Time Interchange sent";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo4.ToolTip = "Message sent by user";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGrid.GridId = "e63b7728-1a27-49c8-a347-fc9b90598f19";
			this.MessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesGrid.LayoutKey = "zGrid1";
			this.MessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessagesGrid.Name = "MessagesGrid";
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 485, true);
			this.MessagesGrid.TabIndex = 3;
			// 
			// HistoryGroupBox
			// 
			this.HistoryGroupBox.Controls.Add(this.MessagesGrid);
			this.HistoryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HistoryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HistoryGroupBox.Name = "HistoryGroupBox";
			this.HistoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 504, true);
			this.HistoryGroupBox.TabIndex = 4;
			this.HistoryGroupBox.TabStop = false;
			this.HistoryGroupBox.Text = "History";
			// 
			// MessageTextGroupBox
			// 
			this.MessageTextGroupBox.Controls.Add(this.MessageTextTextBox);
			this.MessageTextGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.MessageTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 0, true);
			this.MessageTextGroupBox.Name = "MessageTextGroupBox";
			this.MessageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 504, true);
			this.MessageTextGroupBox.TabIndex = 6;
			this.MessageTextGroupBox.TabStop = false;
			this.MessageTextGroupBox.Text = "Message Text";
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "OrderedMessages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.HK.Business.TraxonMessage)(((System.Collections.IList)(((Enterprise.Customs.HK.Business.TraxonConsolStatus)(null)).OrderedMessages)).SyncRoot)).EM_FormattedMessageText)));
			this.MessageTextTextBox.CaptionResourceString = null;
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 485, true);
			this.MessageTextTextBox.TabIndex = 0;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.zLabel2);
			this.TopPanel.Controls.Add(this.Traxon_MessageStatusBoundTextBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 40, true);
			this.TopPanel.TabIndex = 7;
			// 
			// zLabel2
			// 
			this.zLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 8, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 23, true);
			this.zLabel2.TabIndex = 3;
			this.zLabel2.Text = "Status:";
			// 
			// Traxon_MessageStatusBoundTextBox
			// 
			this.Traxon_MessageStatusBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.Traxon_MessageStatusBoundTextBox, "Traxon_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.HK.Business.TraxonConsolStatus)(null)).Traxon_MessageStatus)));
			this.Traxon_MessageStatusBoundTextBox.CaptionResourceString = null;
			this.Traxon_MessageStatusBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Traxon_MessageStatusBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 8, true);
			this.Traxon_MessageStatusBoundTextBox.Name = "Traxon_MessageStatusBoundTextBox";
			this.Traxon_MessageStatusBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 20, true);
			this.Traxon_MessageStatusBoundTextBox.TabIndex = 1;
			this.Traxon_MessageStatusBoundTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.Traxon_MessageStatusBoundTextBox.BackColorChanged += new System.EventHandler(this.Traxon_MessageStatusBoundTextBox4_UpdateColor);
			this.Traxon_MessageStatusBoundTextBox.TextChanged += new System.EventHandler(this.Traxon_MessageStatusBoundTextBox4_UpdateColor);
			// 
			// HistoryAndMessageTestPanel
			// 
			this.HistoryAndMessageTestPanel.Controls.Add(this.TheSplitter);
			this.HistoryAndMessageTestPanel.Controls.Add(this.HistoryGroupBox);
			this.HistoryAndMessageTestPanel.Controls.Add(this.MessageTextGroupBox);
			this.HistoryAndMessageTestPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HistoryAndMessageTestPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.HistoryAndMessageTestPanel.Name = "HistoryAndMessageTestPanel";
			this.HistoryAndMessageTestPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 504, true);
			this.HistoryAndMessageTestPanel.TabIndex = 8;
			// 
			// TheSplitter
			// 
			this.TheSplitter.Dock = System.Windows.Forms.DockStyle.Right;
			this.TheSplitter.DoNotSaveSplitterLayout = false;
			this.TheSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(573, 0, true);
			this.TheSplitter.Name = "TheSplitter";
			this.TheSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 504, true);
			this.TheSplitter.TabIndex = 7;
			this.TheSplitter.TabStop = false;
			// 
			// TraxonMessageUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HistoryAndMessageTestPanel);
			this.Controls.Add(this.TopPanel);
			this.Name = "TraxonMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 544, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessagesGrid.ResumeLayout(false);
			this.MessagesGrid.PerformLayout();
			this.HistoryGroupBox.ResumeLayout(false);
			this.HistoryGroupBox.PerformLayout();
			this.MessageTextGroupBox.ResumeLayout(false);
			this.MessageTextGroupBox.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.HistoryAndMessageTestPanel.ResumeLayout(false);
			this.HistoryAndMessageTestPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Enterprise.ZArchitecture.ZGrid MessagesGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox HistoryGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MessageTextGroupBox;
		private Enterprise.ZArchitecture.ZTextBox MessageTextTextBox;
		private CargoWise.Windows.UI.KPanel TopPanel;
		private Enterprise.ZArchitecture.ZLabel zLabel2;
		private Enterprise.ZArchitecture.ZTextBox Traxon_MessageStatusBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZPanel HistoryAndMessageTestPanel;
		private CargoWise.Windows.UI.KSplitter TheSplitter;
	}
}
