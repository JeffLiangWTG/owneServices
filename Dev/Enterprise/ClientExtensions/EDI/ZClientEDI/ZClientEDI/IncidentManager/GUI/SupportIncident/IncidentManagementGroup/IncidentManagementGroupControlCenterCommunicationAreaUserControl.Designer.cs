namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class IncidentManagementGroupControlCenterCommunicationAreaUserControl
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
			contactPhoneDiallerUserControl?.Dispose();
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.eConversationTableLayout = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.eConversationMessageBoxsplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.conversationMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.eConversationMessageListUserControl1 = new Enterprise.EConversation.GUI.EConversationMessageListUserControl();
			this.incidentDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.organizationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.replytoIncidentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.organizationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.databaseLabel = new Enterprise.ZArchitecture.ZLabel();
			this.enterpriseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.contactTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.enterpriseIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.databaseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.contactPhoneDiallerUserControl = new Enterprise.Client.EDI.IncidentManager.GUI.IncidentContactPhoneDiallerUserControl();
			this.eConversationButtonsFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.sendMessageButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.addInternalLogButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.eConversationTableLayout.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.eConversationMessageBoxsplitContainer)).BeginInit();
			this.eConversationMessageBoxsplitContainer.Panel1.SuspendLayout();
			this.eConversationMessageBoxsplitContainer.Panel2.SuspendLayout();
			this.eConversationMessageBoxsplitContainer.SuspendLayout();
			this.eConversationMessageListUserControl1.SuspendLayout();
			this.incidentDetailsGroupBox.SuspendLayout();
			this.contactPhoneDiallerUserControl.SuspendLayout();
			this.eConversationButtonsFlowLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncident);
			// 
			// eConversationTableLayout
			// 
			this.eConversationTableLayout.ColumnCount = 2;
			this.eConversationTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.eConversationTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.eConversationTableLayout.Controls.Add(this.eConversationMessageBoxsplitContainer, 0, 1);
			this.eConversationTableLayout.Controls.Add(this.incidentDetailsGroupBox, 0, 0);
			this.eConversationTableLayout.Controls.Add(this.eConversationButtonsFlowLayoutPanel, 1, 1);
			this.eConversationTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eConversationTableLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.eConversationTableLayout.Name = "eConversationTableLayout";
			this.eConversationTableLayout.RowCount = 2;
			this.eConversationTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(143)));
			this.eConversationTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150)));
			this.eConversationTableLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 294, true);
			this.eConversationTableLayout.TabIndex = 7;
			// 
			// eConversationMessageBoxsplitContainer
			// 
			this.eConversationMessageBoxsplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eConversationMessageBoxsplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.eConversationMessageBoxsplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 144, true);
			this.eConversationMessageBoxsplitContainer.Name = "eConversationMessageBoxsplitContainer";
			this.eConversationMessageBoxsplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// eConversationMessageBoxsplitContainer.Panel1
			// 
			this.eConversationMessageBoxsplitContainer.Panel1.Controls.Add(this.conversationMessageTextBox);
			this.eConversationMessageBoxsplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 148, true);
			this.eConversationMessageBoxsplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(58);
			// 
			// eConversationMessageBoxsplitContainer.Panel2
			// 
			this.eConversationMessageBoxsplitContainer.Panel2.Controls.Add(this.eConversationMessageListUserControl1);
			this.eConversationMessageBoxsplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(70);
			this.eConversationMessageBoxsplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(64);
			this.eConversationMessageBoxsplitContainer.SplitterWidth = 16;
			this.eConversationMessageBoxsplitContainer.TabIndex = 3;
			// 
			// conversationMessageTextBox
			// 
			this.conversationMessageTextBox.CaptionResourceString = null;
			this.conversationMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.conversationMessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.conversationMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.conversationMessageTextBox.Multiline = true;
			this.conversationMessageTextBox.Name = "conversationMessageTextBox";
			this.conversationMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.conversationMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 64, true);
			this.conversationMessageTextBox.TabIndex = 1;
			// 
			// eConversationMessageListUserControl1
			// 
			this.eConversationMessageListUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.eConversationMessageListUserControl1, "EConversation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.EConversation.Business.IConversation)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).EConversation)));
			this.eConversationMessageListUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eConversationMessageListUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.eConversationMessageListUserControl1.Name = "eConversationMessageListUserControl1";
			this.eConversationMessageListUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 76, true);
			this.eConversationMessageListUserControl1.TabIndex = 1;
			// 
			// incidentDetailsGroupBox
			// 
			this.incidentDetailsGroupBox.Controls.Add(this.organizationTextBox);
			this.incidentDetailsGroupBox.Controls.Add(this.replytoIncidentLabel);
			this.incidentDetailsGroupBox.Controls.Add(this.zLabel3);
			this.incidentDetailsGroupBox.Controls.Add(this.organizationLabel);
			this.incidentDetailsGroupBox.Controls.Add(this.databaseLabel);
			this.incidentDetailsGroupBox.Controls.Add(this.enterpriseCodeTextBox);
			this.incidentDetailsGroupBox.Controls.Add(this.contactTextBox);
			this.incidentDetailsGroupBox.Controls.Add(this.enterpriseIDTextBox);
			this.incidentDetailsGroupBox.Controls.Add(this.databaseTextBox);
			this.incidentDetailsGroupBox.Controls.Add(this.contactPhoneDiallerUserControl);
			this.incidentDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.incidentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.incidentDetailsGroupBox.Name = "incidentDetailsGroupBox";
			this.incidentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 140, true);
			this.incidentDetailsGroupBox.TabIndex = 5;
			this.incidentDetailsGroupBox.TabStop = false;
			this.incidentDetailsGroupBox.Text = "Incident Details";
			// 
			// organizationTextBox
			// 
			this.BindingSource.SetBindingMember(this.organizationTextBox, "ClientCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ClientCode)));
			this.organizationTextBox.CaptionResourceString = ZClientEDI.Res.GetData("e39e2067-6a3d-4897-84aa-eb6670d36d72", "Organization");
			this.organizationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 18, true);
			this.organizationTextBox.Name = "organizationTextBox";
			this.organizationTextBox.ReadOnly = true;
			this.organizationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.organizationTextBox.TabIndex = 1;
			// 
			// replytoIncidentLabel
			// 
			this.replytoIncidentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.replytoIncidentLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.replytoIncidentLabel, "Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).Number)));
			this.replytoIncidentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.replytoIncidentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(680, 121, true);
			this.replytoIncidentLabel.Name = "replytoIncidentLabel";
			this.replytoIncidentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.replytoIncidentLabel.TabIndex = 2;
			// 
			// zLabel3
			// 
			this.zLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel3.AutoSize = true;
			this.zLabel3.CaptionResourceString = ZClientEDI.Res.GetData("40c80725-6e05-4546-8d8b-bdc60d255d74", "Reply to Incident");
			this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(572, 122, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 13, true);
			this.zLabel3.TabIndex = 1;
			this.zLabel3.Text = "Replying to Incident:";
			// 
			// organizationLabel
			// 
			this.organizationLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.organizationLabel, "ClientName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ClientName)));
			this.organizationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.organizationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 18, true);
			this.organizationLabel.Name = "organizationLabel";
			this.organizationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.organizationLabel.TabIndex = 1;
			// 
			// databaseLabel
			// 
			this.databaseLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.databaseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 95, true);
			this.databaseLabel.Name = "databaseLabel";
			this.databaseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.databaseLabel.TabIndex = 3;
			// 
			// enterpriseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.enterpriseCodeTextBox, "EnterpriseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).EnterpriseCode)));
			this.enterpriseCodeTextBox.CaptionResourceString = ZClientEDI.Res.GetData("1e4a6b04-85c8-4370-830a-578bf0a6fb96", "Enterprise Code");
			this.enterpriseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(279, 72, true);
			this.enterpriseCodeTextBox.Name = "enterpriseCodeTextBox";
			this.enterpriseCodeTextBox.ReadOnly = true;
			this.enterpriseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 15, true);
			this.enterpriseCodeTextBox.TabIndex = 2;
			// 
			// contactTextBox
			// 
			this.BindingSource.SetBindingMember(this.contactTextBox, "ContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).ContactName)));
			this.contactTextBox.CaptionResourceString = ZClientEDI.Res.GetData("8fbc0d5c-196f-44c1-a2d3-213ea43b874b", "Contact");
			this.contactTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 45, true);
			this.contactTextBox.Name = "contactTextBox";
			this.contactTextBox.ReadOnly = true;
			this.contactTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.contactTextBox.TabIndex = 3;
			// 
			// enterpriseIDTextBox
			// 
			this.enterpriseIDTextBox.CaptionResourceString = ZClientEDI.Res.GetData("1283791a-c1bf-4150-95a0-f13af563dfa5", "Enterprise ID");
			this.enterpriseIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 69, true);
			this.enterpriseIDTextBox.Name = "enterpriseIDTextBox";
			this.enterpriseIDTextBox.ReadOnly = true;
			this.enterpriseIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.enterpriseIDTextBox.TabIndex = 4;
			// 
			// databaseTextBox
			// 
			this.BindingSource.SetBindingMember(this.databaseTextBox, "DatabaseServerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).DatabaseServerCode)));
			this.databaseTextBox.CaptionResourceString = ZClientEDI.Res.GetData("771a7be3-cc5e-489c-aaf6-3a2542ac4107", "Database");
			this.databaseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 95, true);
			this.databaseTextBox.Name = "databaseTextBox";
			this.databaseTextBox.ReadOnly = true;
			this.databaseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.databaseTextBox.TabIndex = 6;
			// 
			// contactPhoneDiallerUserControl
			// 
			this.contactPhoneDiallerUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.contactPhoneDiallerUserControl, "IM_OH_Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(null)).IM_OH_Client)));
			this.contactPhoneDiallerUserControl.CurrentOrg = null;
			this.contactPhoneDiallerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 45, true);
			this.contactPhoneDiallerUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.contactPhoneDiallerUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.contactPhoneDiallerUserControl.Name = "contactPhoneDiallerUserControl";
			this.contactPhoneDiallerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.contactPhoneDiallerUserControl.TabIndex = 6;
			// 
			// eConversationButtonsFlowLayoutPanel
			// 
			this.eConversationButtonsFlowLayoutPanel.AutoSize = true;
			this.eConversationButtonsFlowLayoutPanel.Controls.Add(this.sendMessageButton);
			this.eConversationButtonsFlowLayoutPanel.Controls.Add(this.addInternalLogButton);
			this.eConversationButtonsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eConversationButtonsFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(767, 144, true);
			this.eConversationButtonsFlowLayoutPanel.Name = "eConversationButtonsFlowLayoutPanel";
			this.eConversationButtonsFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 148, true);
			this.eConversationButtonsFlowLayoutPanel.TabIndex = 5;
			// 
			// sendMessageButton
			// 
			this.sendMessageButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.sendMessageButton.CaptionResourceString = ZClientEDI.Res.GetData("c714799e-b6b1-41d3-9ce7-93ba84e0817c", "Send");
			this.sendMessageButton.Enabled = false;
			this.sendMessageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.sendMessageButton.Name = "sendMessageButton";
			this.sendMessageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 54, true);
			this.sendMessageButton.TabIndex = 3;
			this.sendMessageButton.ToolTipCaption = null;
			this.sendMessageButton.UseVisualStyleBackColor = true;
			// 
			// addInternalLogButton
			// 
			this.addInternalLogButton.CaptionResourceString = ZClientEDI.Res.GetData("7720eedb-3f84-4d53-8c79-a4c5e3eb91dc", "Add Internal Log");
			this.addInternalLogButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 1, true);
			this.addInternalLogButton.Name = "addInternalLogButton";
			this.addInternalLogButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 54, true);
			this.addInternalLogButton.TabIndex = 5;
			this.addInternalLogButton.ToolTipCaption = null;
			this.addInternalLogButton.UseVisualStyleBackColor = true;
			this.addInternalLogButton.Click += new System.EventHandler(this.AddInternalLogButton_Click);
			// 
			// IncidentManagementGroupControlCenterCommunicationAreaUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.eConversationTableLayout);
			this.Name = "IncidentManagementGroupControlCenterCommunicationAreaUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 294, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.eConversationTableLayout.ResumeLayout(false);
			this.eConversationTableLayout.PerformLayout();
			this.eConversationMessageBoxsplitContainer.Panel1.ResumeLayout(false);
			this.eConversationMessageBoxsplitContainer.Panel1.PerformLayout();
			this.eConversationMessageBoxsplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.eConversationMessageBoxsplitContainer)).EndInit();
			this.eConversationMessageBoxsplitContainer.ResumeLayout(false);
			this.eConversationMessageBoxsplitContainer.PerformLayout();
			this.eConversationMessageListUserControl1.ResumeLayout(true);
			this.eConversationMessageListUserControl1.PerformLayout();
			this.incidentDetailsGroupBox.ResumeLayout(false);
			this.incidentDetailsGroupBox.PerformLayout();
			this.contactPhoneDiallerUserControl.ResumeLayout(true);
			this.contactPhoneDiallerUserControl.PerformLayout();
			this.eConversationButtonsFlowLayoutPanel.ResumeLayout(false);
			this.eConversationButtonsFlowLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		CargoWise.Windows.UI.KTableLayoutPanel eConversationTableLayout;
		CargoWise.Windows.UI.KSplitContainer eConversationMessageBoxsplitContainer;
		ZArchitecture.ZTextBox conversationMessageTextBox;
		CargoWise.Windows.UI.KFlowLayoutPanel eConversationButtonsFlowLayoutPanel;
		ZArchitecture.GUI.ZButton sendMessageButton;
		ZArchitecture.GUI.ZGroupBox incidentDetailsGroupBox;
		ZArchitecture.ZLabel replytoIncidentLabel;
		ZArchitecture.ZLabel zLabel3;
		ZArchitecture.ZTextBox organizationTextBox;
		ZArchitecture.ZLabel organizationLabel;
		ZArchitecture.ZLabel databaseLabel;
		ZArchitecture.ZTextBox enterpriseCodeTextBox;
		ZArchitecture.ZTextBox contactTextBox;
		ZArchitecture.ZTextBox enterpriseIDTextBox;
		ZArchitecture.ZTextBox databaseTextBox;
		IncidentContactPhoneDiallerUserControl contactPhoneDiallerUserControl;
		EConversation.GUI.EConversationMessageListUserControl eConversationMessageListUserControl1;
		ZArchitecture.GUI.ZButton addInternalLogButton;
	}
}
