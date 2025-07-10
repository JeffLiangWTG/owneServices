namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class RFPMessagingUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MessagingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GridAndMessagesplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MessageCollectionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessageDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessageInterpretationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageInterpretationWebBrowser = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			this.MessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PermitDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QH_ExportPermitNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QH_RequestForPermitNumberStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QH_RequestForPermitNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessagingGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GridAndMessagesplitContainer)).BeginInit();
			this.GridAndMessagesplitContainer.Panel1.SuspendLayout();
			this.GridAndMessagesplitContainer.Panel2.SuspendLayout();
			this.GridAndMessagesplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageCollectionGrid)).BeginInit();
			this.MessageCollectionGrid.SuspendLayout();
			this.MessageDetailsTabControl.SuspendLayout();
			this.MessageInterpretationTabPage.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.PermitDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader);
			// 
			// MessagingGroupBox
			// 
			this.MessagingGroupBox.Controls.Add(this.GridAndMessagesplitContainer);
			this.MessagingGroupBox.Controls.Add(this.PermitDetailsGroupBox);
			this.MessagingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagingGroupBox.Name = "MessagingGroupBox";
			this.MessagingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1143, 297, true);
			this.MessagingGroupBox.TabIndex = 1;
			this.MessagingGroupBox.TabStop = false;
			this.MessagingGroupBox.Text = "Messaging";
			// 
			// GridAndMessagesplitContainer
			// 
			this.GridAndMessagesplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridAndMessagesplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 59, true);
			this.GridAndMessagesplitContainer.Name = "GridAndMessagesplitContainer";
			// 
			// GridAndMessagesplitContainer.Panel1
			// 
			this.GridAndMessagesplitContainer.Panel1.Controls.Add(this.MessageCollectionGrid);
			// 
			// GridAndMessagesplitContainer.Panel2
			// 
			this.GridAndMessagesplitContainer.Panel2.Controls.Add(this.MessageDetailsTabControl);
			this.GridAndMessagesplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1137, 235, true);
			this.GridAndMessagesplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(911);
			this.GridAndMessagesplitContainer.TabIndex = 1;
			// 
			// MessageCollectionGrid
			// 
			this.MessageCollectionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessageCollectionGrid, "QuarantineExDocHeader+Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Messages)).SyncRoot)).EM_SendOrReceiveHumanReadable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Messages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Messages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Messages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Messages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Messages)).SyncRoot)).EM_InterchangeStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Messages)).SyncRoot)).EM_MessageSubTypeDescription)));
			this.MessageCollectionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Direction";
			zTextBoxColumnStyleInfo1.ColumnName = "EM_SendOrReceiveHumanReadable";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "Message No.";
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.Caption = "Interchange No.";
			zTextBoxColumnStyleInfo3.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.Caption = "Interchange Sent";
			zDateEditColumnStyleInfo1.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.Caption = "Message Type";
			zTextBoxColumnStyleInfo4.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Caption = "Status";
			zTextBoxColumnStyleInfo5.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.Caption = "User";
			zTextBoxColumnStyleInfo6.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "EM_InterchangeStatus";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo8.Caption = "Sub Type";
			zTextBoxColumnStyleInfo8.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo8.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ef706470-d00f-4316-b4f7-50124f661e26", "Message Sub Type");
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.Caption = "Sub Type Desc.";
			zTextBoxColumnStyleInfo9.ColumnName = "EM_MessageSubTypeDescription";
			zTextBoxColumnStyleInfo9.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ef706470-d00f-4316-b4f7-50124f661e26", "Message Sub Type");
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo9.IsVisible = false;
			this.MessageCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessageCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessageCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessageCollectionGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessageCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessageCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessageCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MessageCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MessageCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MessageCollectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.MessageCollectionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageCollectionGrid.GridId = "22dfafd3-1bc9-4b2f-969b-6b8a70118709";
			this.MessageCollectionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessageCollectionGrid.LayoutKey = "MessageCollectionGrid";
			this.MessageCollectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageCollectionGrid.Name = "MessageCollectionGrid";
			this.MessageCollectionGrid.ReadOnly = true;
			this.MessageCollectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(911, 235, true);
			this.MessageCollectionGrid.TabIndex = 0;
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
			this.MessageDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 235, true);
			this.MessageDetailsTabControl.TabIndex = 2;
			// 
			// MessageInterpretationTabPage
			// 
			this.MessageInterpretationTabPage.Controls.Add(this.MessageInterpretationWebBrowser);
			this.MessageInterpretationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageInterpretationTabPage.Name = "MessageInterpretationTabPage";
			this.MessageInterpretationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageInterpretationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 208, true);
			this.MessageInterpretationTabPage.TabIndex = 0;
			this.MessageInterpretationTabPage.Text = "Interpretation";
			this.MessageInterpretationTabPage.UseVisualStyleBackColor = true;
			// 
			// MessageInterpretationWebBrowser
			// 
			this.BindingSource.SetBindingMember(this.MessageInterpretationWebBrowser, "QuarantineExDocHeader+Messages.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Messages)).SyncRoot)).EM_MessageInterpretation)));
			this.MessageInterpretationWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageInterpretationWebBrowser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageInterpretationWebBrowser.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.MessageInterpretationWebBrowser.Name = "MessageInterpretationWebBrowser";
			this.MessageInterpretationWebBrowser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 202, true);
			this.MessageInterpretationWebBrowser.TabIndex = 0;
			this.MessageInterpretationWebBrowser.Url = new System.Uri("about:blank", System.UriKind.Absolute);
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Controls.Add(this.MessageTextBox);
			this.MessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTextTabPage.Name = "MessageTextTabPage";
			this.MessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 208, true);
			this.MessageTextTabPage.TabIndex = 1;
			this.MessageTextTabPage.Text = "Text";
			this.MessageTextTabPage.UseVisualStyleBackColor = true;
			// 
			// MessageTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextBox, "QuarantineExDocHeader+Messages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Messages)).SyncRoot)).EM_FormattedMessageText)));
			this.MessageTextBox.CaptionResourceString = null;
			this.MessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageTextBox.Multiline = true;
			this.MessageTextBox.Name = "MessageTextBox";
			this.MessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 202, true);
			this.MessageTextBox.TabIndex = 0;
			// 
			// PermitDetailsGroupBox
			// 
			this.PermitDetailsGroupBox.Controls.Add(this.QH_ExportPermitNumberTextBox);
			this.PermitDetailsGroupBox.Controls.Add(this.QH_RequestForPermitNumberStatusDescriptionTextBox);
			this.PermitDetailsGroupBox.Controls.Add(this.QH_RequestForPermitNumberTextBox);
			this.PermitDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.PermitDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PermitDetailsGroupBox.Name = "PermitDetailsGroupBox";
			this.PermitDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1137, 43, true);
			this.PermitDetailsGroupBox.TabIndex = 0;
			this.PermitDetailsGroupBox.TabStop = false;
			this.PermitDetailsGroupBox.Text = "Permit Details";
			// 
			// QH_ExportPermitNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.QH_ExportPermitNumberTextBox, "QuarantineExDocHeader+QH_ExportPermitNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ExportPermitNumber)));
			this.QH_ExportPermitNumberTextBox.CaptionResourceString = null;
			this.QH_ExportPermitNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(746, 16, true);
			this.QH_ExportPermitNumberTextBox.Name = "QH_ExportPermitNumberTextBox";
			this.QH_ExportPermitNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.QH_ExportPermitNumberTextBox.TabIndex = 5;
			// 
			// QH_RequestForPermitNumberStatusDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.QH_RequestForPermitNumberStatusDescriptionTextBox, "QuarantineExDocHeader+QH_RequestForPermitNumberStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_RequestForPermitNumberStatusDescription)));
			this.QH_RequestForPermitNumberStatusDescriptionTextBox.CaptionResourceString = null;
			this.QH_RequestForPermitNumberStatusDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.QH_RequestForPermitNumberStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 16, true);
			this.QH_RequestForPermitNumberStatusDescriptionTextBox.Name = "QH_RequestForPermitNumberStatusDescriptionTextBox";
			this.QH_RequestForPermitNumberStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.QH_RequestForPermitNumberStatusDescriptionTextBox.TabIndex = 3;
			// 
			// QH_RequestForPermitNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.QH_RequestForPermitNumberTextBox, "QuarantineExDocHeader+QH_RequestForPermitNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_RequestForPermitNumber)));
			this.QH_RequestForPermitNumberTextBox.CaptionResourceString = null;
			this.QH_RequestForPermitNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 16, true);
			this.QH_RequestForPermitNumberTextBox.Name = "QH_RequestForPermitNumberTextBox";
			this.QH_RequestForPermitNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 20, true);
			this.QH_RequestForPermitNumberTextBox.TabIndex = 1;
			// 
			// RFPMessagingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MessagingGroupBox);
			this.Name = "RFPMessagingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1143, 297, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagingGroupBox.ResumeLayout(false);
			this.MessagingGroupBox.PerformLayout();
			this.GridAndMessagesplitContainer.Panel1.ResumeLayout(false);
			this.GridAndMessagesplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.GridAndMessagesplitContainer)).EndInit();
			this.GridAndMessagesplitContainer.ResumeLayout(false);
			this.GridAndMessagesplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageCollectionGrid)).EndInit();
			this.MessageCollectionGrid.ResumeLayout(false);
			this.MessageCollectionGrid.PerformLayout();
			this.MessageDetailsTabControl.ResumeLayout(false);
			this.MessageDetailsTabControl.PerformLayout();
			this.MessageInterpretationTabPage.ResumeLayout(false);
			this.MessageInterpretationTabPage.PerformLayout();
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.PermitDetailsGroupBox.ResumeLayout(false);
			this.PermitDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MessagingGroupBox;
		private CargoWise.Windows.UI.KSplitContainer GridAndMessagesplitContainer;
		private ZArchitecture.ZGrid MessageCollectionGrid;
		private ZArchitecture.GUI.ZTabControl MessageDetailsTabControl;
		private ZArchitecture.GUI.ZTabPage MessageInterpretationTabPage;
		private Messaging.GUI.HtmlInterpretationBox MessageInterpretationWebBrowser;
		private ZArchitecture.GUI.ZTabPage MessageTextTabPage;
		private ZArchitecture.ZTextBox MessageTextBox;
		public ZArchitecture.GUI.ZGroupBox PermitDetailsGroupBox;
		private ZArchitecture.ZTextBox QH_ExportPermitNumberTextBox;
		public ZArchitecture.ZTextBox QH_RequestForPermitNumberStatusDescriptionTextBox;
		public ZArchitecture.ZTextBox QH_RequestForPermitNumberTextBox;
	}
}
