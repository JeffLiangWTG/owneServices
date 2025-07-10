namespace Enterprise.Customs.KR.GUI
{
	partial class EDIMessageMainUserControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RequestReceiveDocumentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RequestReceiveDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RequestReceiveDocumentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.messageDetailsPanel.SuspendLayout();
			this.messageContentsPanel.SuspendLayout();
			this.processingDetailsGroupBox.SuspendLayout();
			this.systemCreateTimeUtc.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RequestReceiveDocumentGrid)).BeginInit();
			this.RequestReceiveDocumentGrid.SuspendLayout();
			this.RequestReceiveDocumentGroupBox.SuspendLayout();
			this.RequestReceiveDocumentPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// messageDetailsPanel
			// 
			this.messageDetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.messageDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 152, true);
			// 
			// messageContentsPanel
			// 
			this.messageContentsPanel.Dock = System.Windows.Forms.DockStyle.None;
			this.messageContentsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(771, 286, true);
			this.messageContentsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 51, true);
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 0, true);
			// 
			// directionTextBox
			// 
			this.directionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 61, true);
			// 
			// processingDetailsGroupBox
			// 
			this.processingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.processingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 0, true);
			// 
			// transportTypeTextBox
			// 
			this.transportTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 83, true);
			// 
			// systemCreateTimeUtc
			// 
			this.systemCreateTimeUtc.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 105, true);
			// 
			// externalReferenceNumberTextBox
			// 
			this.externalReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 127, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.EDIMessage);
			// 
			// RequestReceiveDocumentGrid
			// 
			this.RequestReceiveDocumentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RequestReceiveDocumentGrid, "CusPollingTransactions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).CPT_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).CPT_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).CPT_Reference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).CPT_TransactionID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).DOCMessage.EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).DOCMessage.EM_StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).DOCMessage.EM_SystemLastEditTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).RCVMessage.EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).RCVMessage.EM_StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).RCVMessage.EM_SystemLastEditTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).DOCMessage.Interchange.EI_InterchangeNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).DOCMessage.Interchange.EI_InterchangeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).DOCMessage.Interchange.EI_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).RCVMessage.Interchange.EI_InterchangeNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).RCVMessage.Interchange.EI_InterchangeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).RCVMessage.Interchange.EI_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).DOCMessage.EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusPollingTransaction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).CusPollingTransactions)).SyncRoot)).RCVMessage.EM_Status)));
			this.RequestReceiveDocumentGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("96390a4a-750e-4440-b4f3-50a8003aed7f", "Status");
			zTextBoxColumnStyleInfo1.ColumnName = "CPT_Status";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(64);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("98b10f8b-6d89-4393-9ec9-9e35e91e76d1", "Created Time (UTC)");
			zDateEditColumnStyleInfo1.ColumnName = "CPT_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("73983404-e30b-496d-b3a8-c28694e68ad8", "Message Type");
			zTextBoxColumnStyleInfo2.ColumnName = "CPT_Reference";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("c17c3949-491a-45e0-ad3e-7213b4eb7b12", "Transaction ID");
			zTextBoxColumnStyleInfo3.ColumnName = "CPT_TransactionID";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(227);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("7cf3b7fb-a8db-4708-a84b-a65e28955e53", "DOC Message Number");
			zTextBoxColumnStyleInfo4.ColumnName = "DOCMessage+EM_MessageNum";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("5a2d3e54-41c0-4045-be84-d7f547e4935b", "DOC Meg. Status Name");
			zTextBoxColumnStyleInfo5.ColumnName = "DOCMessage+EM_StatusDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("a0f4747d-262e-445d-9676-66e1496ff31a", "DOC Meg. Send Time (UTC)");
			zDateEditColumnStyleInfo2.ColumnName = "DOCMessage+EM_SystemLastEditTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(147);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("1148b284-df33-4d86-a671-52feb3081ed3", "RCV Message Number");
			zTextBoxColumnStyleInfo6.ColumnName = "RCVMessage+EM_MessageNum";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(137);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("aee6bdf6-35ea-412e-91c3-ce3d3c3173b5", "RCV Meg. Status Name");
			zTextBoxColumnStyleInfo7.ColumnName = "RCVMessage+EM_StatusDescription";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(137);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("a7800413-c33b-4c6f-84c6-3c16987a778e", "RCV Meg. Receive Time(UTC)");
			zDateEditColumnStyleInfo3.ColumnName = "RCVMessage+EM_SystemLastEditTimeUtc";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(177);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("2f20eb17-af0f-4739-b9e3-d4c4bdb4abc9", "DOC Interchange Number");
			zTextBoxColumnStyleInfo8.ColumnName = "DOCMessage+Interchange+EI_InterchangeNum";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(136);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("9b282e98-6316-4916-a16a-f79d9b011dbc", "DOC Interchange Type");
			zTextBoxColumnStyleInfo9.ColumnName = "DOCMessage+Interchange+EI_InterchangeType";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(136);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("c8748dc0-f5ce-4deb-b1f1-6caa6dd62a98", "DOD Interchange Status");
			zTextBoxColumnStyleInfo10.ColumnName = "DOCMessage+Interchange+EI_Status";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("2164f1b2-f3ff-4514-a093-2eb898cd632f", "RCV Interchange Number");
			zTextBoxColumnStyleInfo11.ColumnName = "RCVMessage+Interchange+EI_InterchangeNum";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(153);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("5af26b4a-8f86-4fb6-be55-68d3649a0d3a", "RCV Interchange Type");
			zTextBoxColumnStyleInfo12.ColumnName = "RCVMessage+Interchange+EI_InterchangeType";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("f35cb218-1c7e-42c2-a20a-87194c85626f", "RCV Interchange Status");
			zTextBoxColumnStyleInfo13.ColumnName = "RCVMessage+Interchange+EI_Status";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("4b24789e-f4d6-430b-84f6-fbd2de34e834", "DOC Meg. Status");
			zTextBoxColumnStyleInfo14.ColumnName = "DOCMessage+EM_Status";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("7c616d89-7f85-404e-8a4d-ec917d2b4d99", "RCV Meg. Status");
			zTextBoxColumnStyleInfo15.ColumnName = "RCVMessage+EM_Status";
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.RequestReceiveDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.RequestReceiveDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RequestReceiveDocumentGrid.GridId = "2df70a88-100f-452a-b597-84ac8c5cd3c5";
			this.RequestReceiveDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RequestReceiveDocumentGrid.LayoutKey = "RequestReceiveDocumentGrid";
			this.RequestReceiveDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.RequestReceiveDocumentGrid.Name = "RequestReceiveDocumentGrid";
			this.RequestReceiveDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 215, true);
			this.RequestReceiveDocumentGrid.TabIndex = 0;
			// 
			// RequestReceiveDocumentGroupBox
			// 
			this.RequestReceiveDocumentGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("4ef23be0-e526-4de3-9aaa-64bce12e8e68", "Request to Receive Document List");
			this.RequestReceiveDocumentGroupBox.Controls.Add(this.RequestReceiveDocumentGrid);
			this.RequestReceiveDocumentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RequestReceiveDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RequestReceiveDocumentGroupBox.Name = "RequestReceiveDocumentGroupBox";
			this.RequestReceiveDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 234, true);
			this.RequestReceiveDocumentGroupBox.TabIndex = 3;
			this.RequestReceiveDocumentGroupBox.TabStop = false;
			// 
			// RequestReceiveDocumentPanel
			// 
			this.RequestReceiveDocumentPanel.Controls.Add(this.RequestReceiveDocumentGroupBox);
			this.RequestReceiveDocumentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RequestReceiveDocumentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 119, true);
			this.RequestReceiveDocumentPanel.Name = "RequestReceiveDocumentPanel";
			this.RequestReceiveDocumentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 234, true);
			this.RequestReceiveDocumentPanel.TabIndex = 22;
			// 
			// EDIMessageMainUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.RequestReceiveDocumentPanel);
			this.Name = "EDIMessageMainUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 353, true);
			this.Controls.SetChildIndex(this.messageDetailsPanel, 0);
			this.Controls.SetChildIndex(this.messageContentsPanel, 0);
			this.Controls.SetChildIndex(this.RequestReceiveDocumentPanel, 0);
			this.messageDetailsPanel.ResumeLayout(false);
			this.messageDetailsPanel.PerformLayout();
			this.messageContentsPanel.ResumeLayout(false);
			this.messageContentsPanel.PerformLayout();
			this.processingDetailsGroupBox.ResumeLayout(false);
			this.processingDetailsGroupBox.PerformLayout();
			this.systemCreateTimeUtc.ResumeLayout(true);
			this.systemCreateTimeUtc.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RequestReceiveDocumentGrid)).EndInit();
			this.RequestReceiveDocumentGrid.ResumeLayout(false);
			this.RequestReceiveDocumentGrid.PerformLayout();
			this.RequestReceiveDocumentGroupBox.ResumeLayout(false);
			this.RequestReceiveDocumentGroupBox.PerformLayout();
			this.RequestReceiveDocumentPanel.ResumeLayout(false);
			this.RequestReceiveDocumentPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZGrid RequestReceiveDocumentGrid;
		private ZArchitecture.GUI.ZGroupBox RequestReceiveDocumentGroupBox;
		private ZArchitecture.GUI.ZPanel RequestReceiveDocumentPanel;
	}
}
