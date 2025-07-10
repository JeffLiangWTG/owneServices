using System;
using System.Windows.Forms;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CcsukMessagesUserControl
	{ 
		void AddMenuAndQueueIdleWorker()
		{
			UserIdleWorker.QueueWorkItem(this, 0, new MethodInvoker(RefreshWebBrowser), null); // updates the web browser control's HTML with the text in the hidden text box, but only when the computer first becomes idle. 				
		}

		void InterpretedMessageTextBox_TextChanged(object sender, EventArgs e)
		{
			RefreshWebBrowser();
		}

		void RefreshWebBrowser()
		{
			MessageInterpretationWebBrowser.DocumentText = InterpretedMessageTextBox.Text;
		}



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

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.splitContainerGridVersusMessageDetails = new CargoWise.Windows.UI.KSplitContainer();
			this.MessageDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessageInterpretationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageInterpretationWebBrowser = new Enterprise.ZArchitecture.GUI.ZWebBrowser();
			this.MessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InterpretedMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerGridVersusMessageDetails)).BeginInit();
			this.splitContainerGridVersusMessageDetails.Panel1.SuspendLayout();
			this.splitContainerGridVersusMessageDetails.Panel2.SuspendLayout();
			this.splitContainerGridVersusMessageDetails.SuspendLayout();
			this.MessageDetailsTabControl.SuspendLayout();
			this.MessageInterpretationTabPage.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)).SyncRoot)).EM_ReceiveTransmit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)).SyncRoot)).Interchange.EI_To)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)).SyncRoot)).Interchange.EI_From)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)).SyncRoot)).EM_SystemLastEditUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)).SyncRoot)).EM_ApplicationReference)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = null;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("a3b4f565-2b0e-4c92-983a-7cd645eb46d8", "Message date-time");
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("096e28d9-5649-4b10-a450-048e87f026a1", "Created date-time");
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = null;
			zTextBoxColumnStyleInfo3.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = null;
			zTextBoxColumnStyleInfo4.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("febb0091-4ccd-4ab8-bafb-904f719251c8", "Sub type");
			zTextBoxColumnStyleInfo5.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("40cde5d2-c08c-493a-b7e9-587cb2432213", "Interchange number");
			zTextBoxColumnStyleInfo6.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo7.Caption = null;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("a48b7d2b-357a-443a-b55c-230c75333579", "Recipient");
			zTextBoxColumnStyleInfo7.ColumnName = "Interchange+EI_To";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zTextBoxColumnStyleInfo8.Caption = null;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("625d0c06-715d-4be8-a1b0-8338b71dae0b", "Sender");
			zTextBoxColumnStyleInfo8.ColumnName = "Interchange+EI_From";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zTextBoxColumnStyleInfo9.Caption = null;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("4774b10d-963b-4396-a23e-9af1c8dccc1d", "User");
			zTextBoxColumnStyleInfo9.ColumnName = EDIMessage.Schema.EM_SystemCreateUser;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45); 
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("5774b10d-963b-4396-a23e-9af1c8dccc1d", "Reference");
			zTextBoxColumnStyleInfo10.ColumnName = "EM_ApplicationReference";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.zGrid1.CopySelectedRowsAllowed = true;
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.GridId = "ce4b1508-1ff0-47b8-9053-d26251519159";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.ReadOnly = true;
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 161, true);
			this.zGrid1.TabIndex = 0;
			// 
			// splitContainerGridVersusMessageDetails
			// 
			this.splitContainerGridVersusMessageDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerGridVersusMessageDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerGridVersusMessageDetails.Name = "splitContainerGridVersusMessageDetails";
			this.splitContainerGridVersusMessageDetails.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerGridVersusMessageDetails.Panel1
			// 
			this.splitContainerGridVersusMessageDetails.Panel1.Controls.Add(this.zGrid1);
			// 
			// splitContainerGridVersusMessageDetails.Panel2
			// 
			this.splitContainerGridVersusMessageDetails.Panel2.Controls.Add(this.MessageDetailsTabControl);
			this.splitContainerGridVersusMessageDetails.Panel2.Controls.Add(this.InterpretedMessageTextBox);
			this.splitContainerGridVersusMessageDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 310, true);
			this.splitContainerGridVersusMessageDetails.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(161);
			this.splitContainerGridVersusMessageDetails.TabIndex = 1;
			// 
			// MessageDetailsTabControl
			// 
			this.MessageDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MessageDetailsTabControl.Controls.Add(this.MessageInterpretationTabPage);
			this.MessageDetailsTabControl.Controls.Add(this.MessageTextTabPage);
			this.MessageDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageDetailsTabControl.Name = "MessageDetailsTabControl";
			this.MessageDetailsTabControl.SelectedIndex = 0;
			this.MessageDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 145, true);
			this.MessageDetailsTabControl.TabIndex = 2;
			// 
			// MessageInterpretationTabPage
			// 
			this.MessageInterpretationTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("3f17e332-f450-4ae3-b468-36fb3df17595", "Interpretation");
			this.MessageInterpretationTabPage.Controls.Add(this.MessageInterpretationWebBrowser);
			this.MessageInterpretationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageInterpretationTabPage.Name = "MessageInterpretationTabPage";
			this.MessageInterpretationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageInterpretationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 118, true);
			this.MessageInterpretationTabPage.TabIndex = 0;
			this.MessageInterpretationTabPage.UseVisualStyleBackColor = true;
			// 
			// MessageInterpretationWebBrowser
			// 
			this.MessageInterpretationWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageInterpretationWebBrowser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageInterpretationWebBrowser.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.MessageInterpretationWebBrowser.Name = "MessageInterpretationWebBrowser";
			this.MessageInterpretationWebBrowser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 112, true);
			this.MessageInterpretationWebBrowser.TabIndex = 0;
			this.MessageInterpretationWebBrowser.Url = new System.Uri("about:blank", System.UriKind.Absolute);
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("2706232b-259e-43f0-b52c-c67954b3a0ae", "Text");
			this.MessageTextTabPage.Controls.Add(this.MessageTextTextBox);
			this.MessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTextTabPage.Name = "MessageTextTabPage";
			this.MessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 118, true);
			this.MessageTextTabPage.TabIndex = 1;
			this.MessageTextTabPage.UseVisualStyleBackColor = true;
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "Messages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)).SyncRoot)).EM_FormattedMessageText)));
			this.MessageTextTextBox.CaptionResourceString = null;
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 112, true);
			this.MessageTextTextBox.TabIndex = 0;
			// 
			// InterpretedMessageTextBox
			// 
			this.BindingSource.SetBindingMember(this.InterpretedMessageTextBox, "Messages.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Messages)).SyncRoot)).EM_MessageInterpretation)));
			this.InterpretedMessageTextBox.CaptionResourceString = null;
			this.InterpretedMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.InterpretedMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 52, true);
			this.InterpretedMessageTextBox.Name = "InterpretedMessageTextBox";
			this.InterpretedMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.InterpretedMessageTextBox.TabIndex = 1;
			this.InterpretedMessageTextBox.TabStop = false;
			this.InterpretedMessageTextBox.TextChanged += new System.EventHandler(this.InterpretedMessageTextBox_TextChanged);
			// 
			// CcsukMessagesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainerGridVersusMessageDetails);
			this.Name = "CcsukMessagesUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.splitContainerGridVersusMessageDetails.Panel1.ResumeLayout(false);
			this.splitContainerGridVersusMessageDetails.Panel2.ResumeLayout(false);
			this.splitContainerGridVersusMessageDetails.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerGridVersusMessageDetails)).EndInit();
			this.splitContainerGridVersusMessageDetails.ResumeLayout(false);
			this.MessageDetailsTabControl.ResumeLayout(false);
			this.MessageInterpretationTabPage.ResumeLayout(false);
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.ResumeLayout(false);

		}

		private ZArchitecture.ZGrid zGrid1;
		private CargoWise.Windows.UI.KSplitContainer splitContainerGridVersusMessageDetails;
		private ZArchitecture.ZTextBox InterpretedMessageTextBox;
		private ZTabControl MessageDetailsTabControl;
		private ZTabPage MessageInterpretationTabPage;
		private ZWebBrowser MessageInterpretationWebBrowser;
		private ZTabPage MessageTextTabPage;
		private ZArchitecture.ZTextBox MessageTextTextBox;


	}
}
