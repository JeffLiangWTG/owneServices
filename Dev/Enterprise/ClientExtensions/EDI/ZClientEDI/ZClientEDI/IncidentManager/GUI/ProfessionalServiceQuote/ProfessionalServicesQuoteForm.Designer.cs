namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class ProfessionalServicesQuoteForm
	{
		protected Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		private Enterprise.ZArchitecture.GUI.ZButton CancelTaskButton;
		private Enterprise.ZArchitecture.GUI.ZButton CompleteOrReopenTaskButton;
		private Enterprise.ZArchitecture.ZTextBox IncidentNumberTextBox;
		protected Enterprise.ZArchitecture.ZLabel WorkNumberLabel;
		private Enterprise.ZArchitecture.ZLabel AddedByLabel;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox AddedByGuidFindBox;
		private Enterprise.ZArchitecture.ZLabel DescriptionHeaderLabel;
		private Enterprise.ZArchitecture.ZTextBox DescriptionHeaderTextBox;
		private System.ComponentModel.IContainer components;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ClientHeaderGuidFindBox;
		private Enterprise.ZArchitecture.ZLabel ClientHeaderLabel;
		protected Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		protected Enterprise.ZArchitecture.GUI.ZButton EmailButton;
		internal Enterprise.ZArchitecture.GUI.ZButton ConvertToWorkItemButton;
		protected Enterprise.ZArchitecture.ZLabel EmailSentLabel;
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage IncidentTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PricingAndMaintenanceFeeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit MaintenanceAndUpgradeAcceptedDateEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox DepositAmountCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox QuoteAmountCalcFindBox;
		private Enterprise.ZArchitecture.ZLabel MaintenanceAndUpgradeAcceptedLabel;
		private Enterprise.ZArchitecture.ZLabel DepositAmountLabel;
		private Enterprise.ZArchitecture.ZLabel QuoteAmountLabel;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox ClientAndContactDetailsGroupBox;
		private Enterprise.ZArchitecture.ZLabel ClientFullNameLabel;
		private Enterprise.ZArchitecture.ZLabel ClientLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZButton EditContactButton;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox ContactGuidFindBox;
		protected Enterprise.ZArchitecture.ZLabel ContractLabel;
		protected Enterprise.ZArchitecture.ZLabel ContractStatusLabel;
		protected Enterprise.ZArchitecture.ZTextBox ContactEmailTextBox;
		protected Enterprise.ZArchitecture.ZTextBox BranchFaxNumberTextBox;
		protected Enterprise.ZArchitecture.ZLabel BranchPhoneNumberLabel;
		protected Enterprise.ZArchitecture.ZTextBox BranchPhoneNumberTextBox;
		protected Enterprise.ZArchitecture.ZLabel ContactLabel;
		protected Enterprise.ZArchitecture.ZLabel BranchFaxNumberLabel;
		protected Enterprise.ZArchitecture.ZLabel ContactEmailLabel;
		protected Enterprise.ZArchitecture.ZLabel ContactNumberLabel;
		protected Enterprise.ZArchitecture.ZTextBox ContactNumberTextBox;
		private Enterprise.ZArchitecture.GUI.ZAddressControl ClientAddressControl;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox StatusGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton NoRadioButton;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton YesRadioButton;
		protected Enterprise.ZArchitecture.ZLabel WorkChargableLabel;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit RequiredByDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit LoggedAtDateEdit;
		private Enterprise.ZArchitecture.ZLabel LoggedAtLabel;
		private Enterprise.ZArchitecture.ZLabel RequiredByLabel;
		protected Enterprise.ZArchitecture.ZLabel StatusLabel;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox IncidentDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit WorkItemTypeDropEdit;
		private Enterprise.ZArchitecture.ZLabel QuotationTypeLabel;
		private Enterprise.ZArchitecture.ZCalcEdit EstimatedWorkHoursCalcEdit;
		private Enterprise.ZArchitecture.ZLabel EstimatedWorkHoursLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ProductDropEdit;
		private Enterprise.ZArchitecture.ZLabel ProductLabel;
		private Enterprise.ZArchitecture.ZLabel ProgramAreaLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ProgramAreaDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox CustomerServiceContactCodeFindBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox AssignedToCodeFindBox;
		protected Enterprise.ZArchitecture.ZLabel CustomerServiceContactLabel;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox AssignedTeamGuidFindBox;
		protected Enterprise.ZArchitecture.ZLabel AssignedToLabel;
		protected Enterprise.ZArchitecture.ZLabel AssignedTeamLabel;
		protected Enterprise.ZArchitecture.ZLabel CommentsLabel;
		protected Enterprise.ZArchitecture.ZTextBox CommentsTextBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit RequestedPriorityDropEdit;
		protected Enterprise.ZArchitecture.ZLabel RequestedPriorityLabel;
		protected Enterprise.ZArchitecture.GUI.ZPanel DescriptionAndDetailsPanel;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZRichTextBox DetailsRichTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.ZTextBox InvoiceDetailsTextBox;
		private Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		private Enterprise.ZArchitecture.ZLabel DescriptionLabel;
		protected Enterprise.ZArchitecture.GUI.ZTabPage RelatedItemsTabPage;
		private EDIWorkflowTabPage WorkflowTabPage;
		protected Enterprise.ZArchitecture.GUI.ZStmNoteTabPage NotesTabPage;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage EventsTabPage;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel3;
		private Core.Forms.ZPostingButtonsUserControl PostingButtons;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel2;
		protected Enterprise.ZArchitecture.GUI.ZPanel EmailMenuPanel;
		protected CargoWise.Windows.UI.KMenuStrip EmailMenuStrip;
		protected Enterprise.ZArchitecture.GUI.ZToolStripMenuItem emailToolStripMenuItem;
		internal Enterprise.ZArchitecture.GUI.ZToolStripMenuItem correspondenceFromOutlookToolStripMenuItem;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ClientHeaderGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ClientHeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IncidentNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionHeaderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionHeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CancelTaskButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CompleteOrReopenTaskButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddedByLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AddedByGuidFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.WorkNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.correspondenceFromOutlookToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EmailButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EmailMenuPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EmailMenuStrip = new CargoWise.Windows.UI.KMenuStrip();
			this.emailToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.ConvertToWorkItemButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zPanel3 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.EmailSentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.IncidentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PricingAndMaintenanceFeeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MaintenanceAndUpgradeAcceptedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DepositAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.QuoteAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.MaintenanceAndUpgradeAcceptedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DepositAmountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.QuoteAmountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ClientAndContactDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClientFullNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ClientLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EditContactButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ContactGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ContractLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContractStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BranchFaxNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BranchPhoneNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BranchPhoneNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BranchFaxNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactEmailLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClientAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.StatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NoRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.YesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.WorkChargableLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RequiredByDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LoggedAtDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LoggedAtLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RequiredByLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IncidentDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WorkItemTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QuotationTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EstimatedWorkHoursCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EstimatedWorkHoursLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProductDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProductLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProgramAreaLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProgramAreaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomerServiceContactCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AssignedToCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomerServiceContactLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AssignedTeamGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AssignedToLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AssignedTeamLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CommentsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CommentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RequestedPriorityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RequestedPriorityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DescriptionAndDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailsRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InvoiceDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RelatedItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.WorkflowTabPage = new Enterprise.Client.EDI.IncidentManager.GUI.EDIWorkflowTabPage(this.components);
			this.NotesTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.EventsTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			this.ClientHeaderGuidFindBox.SuspendLayout();
			this.AddedByGuidFindBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.EmailMenuPanel.SuspendLayout();
			this.zPanel3.SuspendLayout();
			this.PostingButtons.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.IncidentTabPage.SuspendLayout();
			this.PricingAndMaintenanceFeeGroupBox.SuspendLayout();
			this.MaintenanceAndUpgradeAcceptedDateEdit.SuspendLayout();
			this.DepositAmountCalcFindBox.SuspendLayout();
			this.QuoteAmountCalcFindBox.SuspendLayout();
			this.ClientAndContactDetailsGroupBox.SuspendLayout();
			this.ContactGuidFindBox.SuspendLayout();
			this.ClientAddressControl.SuspendLayout();
			this.StatusGroupBox.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.RequiredByDateEdit.SuspendLayout();
			this.LoggedAtDateEdit.SuspendLayout();
			this.IncidentDetailsGroupBox.SuspendLayout();
			this.WorkItemTypeDropEdit.SuspendLayout();
			this.ProductDropEdit.SuspendLayout();
			this.ProgramAreaDropEdit.SuspendLayout();
			this.CustomerServiceContactCodeFindBox.SuspendLayout();
			this.AssignedToCodeFindBox.SuspendLayout();
			this.AssignedTeamGuidFindBox.SuspendLayout();
			this.RequestedPriorityDropEdit.SuspendLayout();
			this.DescriptionAndDetailsPanel.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.DetailsRichTextBox.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 628, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 24, true);
			this.MainStatusBar.TabIndex = 3;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(496);
			//
			// ErrorStatusBarPanel
			//
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(497);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote);
			//
			// TopPanel
			//
			this.TopPanel.Controls.Add(this.ClientHeaderGuidFindBox);
			this.TopPanel.Controls.Add(this.ClientHeaderLabel);
			this.TopPanel.Controls.Add(this.IncidentNumberTextBox);
			this.TopPanel.Controls.Add(this.DescriptionHeaderTextBox);
			this.TopPanel.Controls.Add(this.DescriptionHeaderLabel);
			this.TopPanel.Controls.Add(this.CancelTaskButton);
			this.TopPanel.Controls.Add(this.CompleteOrReopenTaskButton);
			this.TopPanel.Controls.Add(this.AddedByLabel);
			this.TopPanel.Controls.Add(this.AddedByGuidFindBox);
			this.TopPanel.Controls.Add(this.WorkNumberLabel);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 58, true);
			this.TopPanel.TabIndex = 0;
			//
			// ClientHeaderGuidFindBox
			//
			this.ClientHeaderGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientHeaderGuidFindBox, "ClientHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).ClientHeader)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).Lookups.ActiveClientList)));
			this.ClientHeaderGuidFindBox.BindToList = "Lookups+ActiveClientList";
			this.ClientHeaderGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			this.ClientHeaderGuidFindBox.Name = "ClientHeaderGuidFindBox";
			this.ClientHeaderGuidFindBox.PreBoundMaxLength = 10;
			this.ClientHeaderGuidFindBox.ShowDescriptionBox = false;
			this.ClientHeaderGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.ClientHeaderGuidFindBox.TabIndex = 11;
			//
			// ClientHeaderLabel
			//
			this.ClientHeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.ClientHeaderLabel.Name = "ClientHeaderLabel";
			this.ClientHeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 23, true);
			this.ClientHeaderLabel.TabIndex = 10;
			this.ClientHeaderLabel.Text = "Client:";
			//
			// IncidentNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.IncidentNumberTextBox, "IM_IncidentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_IncidentNumber)));
			this.IncidentNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.IncidentNumberTextBox.Name = "IncidentNumberTextBox";
			this.IncidentNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.IncidentNumberTextBox.TabIndex = 1;
			//
			// DescriptionHeaderTextBox
			//
			this.BindingSource.SetBindingMember(this.DescriptionHeaderTextBox, "DescriptionHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).DescriptionHeader)));
			this.DescriptionHeaderTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionHeaderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 32, true);
			this.DescriptionHeaderTextBox.Name = "DescriptionHeaderTextBox";
			this.DescriptionHeaderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 20, true);
			this.DescriptionHeaderTextBox.TabIndex = 7;
			//
			// DescriptionHeaderLabel
			//
			this.DescriptionHeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 32, true);
			this.DescriptionHeaderLabel.Name = "DescriptionHeaderLabel";
			this.DescriptionHeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.DescriptionHeaderLabel.TabIndex = 6;
			this.DescriptionHeaderLabel.Text = "Description:";
			//
			// CancelTaskButton
			//
			this.CancelTaskButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelTaskButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(912, 8, true);
			this.CancelTaskButton.Name = "CancelTaskButton";
			this.CancelTaskButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.CancelTaskButton.TabIndex = 9;
			this.CancelTaskButton.Text = "Cancel Task";
			this.CancelTaskButton.Click += new System.EventHandler(this.CancelTaskButton_Click);
			//
			// CompleteOrReopenTaskButton
			//
			this.CompleteOrReopenTaskButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CompleteOrReopenTaskButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(800, 8, true);
			this.CompleteOrReopenTaskButton.Name = "CompleteOrReopenTaskButton";
			this.CompleteOrReopenTaskButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.CompleteOrReopenTaskButton.TabIndex = 8;
			this.CompleteOrReopenTaskButton.Text = "Complete Task";
			this.CompleteOrReopenTaskButton.Click += new System.EventHandler(this.CompleteOrReopenTaskButton_Click);
			//
			// AddedByLabel
			//
			this.AddedByLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 8, true);
			this.AddedByLabel.Name = "AddedByLabel";
			this.AddedByLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.AddedByLabel.TabIndex = 2;
			this.AddedByLabel.Text = "Added By:";
			//
			// AddedByGuidFindBox
			//
			this.AddedByGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddedByGuidFindBox, "IM_SystemCreateUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).Lookups.CustServiceContacts)));
			this.AddedByGuidFindBox.BindToList = "Lookups+CustServiceContacts";
			this.AddedByGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 8, true);
			this.AddedByGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.AddedByGuidFindBox.Name = "AddedByGuidFindBox";
			this.AddedByGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.AddedByGuidFindBox.TabIndex = 3;
			//
			// WorkNumberLabel
			//
			this.WorkNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.WorkNumberLabel.Name = "WorkNumberLabel";
			this.WorkNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.WorkNumberLabel.TabIndex = 0;
			this.WorkNumberLabel.Text = "Incident Number:";
			//
			// correspondenceFromOutlookToolStripMenuItem
			//
			this.correspondenceFromOutlookToolStripMenuItem.Name = "correspondenceFromOutlookToolStripMenuItem";
			this.correspondenceFromOutlookToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 22, true);
			this.correspondenceFromOutlookToolStripMenuItem.Text = "Correspondence from Outlook";
			this.correspondenceFromOutlookToolStripMenuItem.Click += new System.EventHandler(this.correspondenceFromOutlookToolStripMenuItem_Click);
			//
			// BottomPanel
			//
			this.BottomPanel.Controls.Add(this.zPanel1);
			this.BottomPanel.Controls.Add(this.EmailSentLabel);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 571, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 57, true);
			this.BottomPanel.TabIndex = 12;
			//
			// zPanel1
			//
			this.zPanel1.Controls.Add(this.zPanel2);
			this.zPanel1.Controls.Add(this.zPanel3);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 31, true);
			this.zPanel1.TabIndex = 5;
			//
			// zPanel2
			//
			this.zPanel2.Controls.Add(this.EmailButton);
			this.zPanel2.Controls.Add(this.EmailMenuPanel);
			this.zPanel2.Controls.Add(this.ConvertToWorkItemButton);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Right;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 31, true);
			this.zPanel2.TabIndex = 13;
			//
			// EmailButton
			//
			this.EmailButton.AutoSize = true;
			this.EmailButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 5, true);
			this.EmailButton.Name = "EmailButton";
			this.EmailButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 23, true);
			this.EmailButton.TabIndex = 13;
			this.EmailButton.Text = "Email";
			this.EmailButton.Click += new System.EventHandler(this.EmailButton_Click);
			//
			// EmailMenuPanel
			//
			this.EmailMenuPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.EmailMenuPanel.Controls.Add(this.EmailMenuStrip);
			this.EmailMenuPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 7, true);
			this.EmailMenuPanel.Name = "EmailMenuPanel";
			this.EmailMenuPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 21, true);
			this.EmailMenuPanel.TabIndex = 17;
			//
			// EmailMenuStrip
			//
			this.EmailMenuStrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EmailMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.emailToolStripMenuItem });
			this.EmailMenuStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EmailMenuStrip.Name = "EmailMenuStrip";
			this.EmailMenuStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 19, true);
			this.EmailMenuStrip.TabIndex = 0;
			//
			// emailToolStripMenuItem
			//
			this.emailToolStripMenuItem.Name = "emailToolStripMenuItem";
			this.emailToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 15, true);
			//
			// ConvertToWorkItemButton
			//
			this.ConvertToWorkItemButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
			this.ConvertToWorkItemButton.Name = "ConvertToWorkItemButton";
			this.ConvertToWorkItemButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 22, true);
			this.ConvertToWorkItemButton.TabIndex = 7;
			this.ConvertToWorkItemButton.Text = "Convert to Work Item";
			this.ConvertToWorkItemButton.Click += new System.EventHandler(this.ConvertToWorkItemButton_Click);
			//
			// zPanel3
			//
			this.zPanel3.AutoSize = true;
			this.zPanel3.Controls.Add(this.PostingButtons);
			this.zPanel3.Dock = System.Windows.Forms.DockStyle.Right;
			this.zPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(755, 0, true);
			this.zPanel3.Name = "zPanel3";
			this.zPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 31, true);
			this.zPanel3.TabIndex = 13;
			//
			// PostingButtons
			//
			this.PostingButtons.AllowDrop = true;
			this.PostingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 3, true);
			this.PostingButtons.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtons.Name = "PostingButtons";
			this.PostingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 25, true);
			this.PostingButtons.TabIndex = 11;
			//
			// EmailSentLabel
			//
			this.EmailSentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.EmailSentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 10, true);
			this.EmailSentLabel.Name = "EmailSentLabel";
			this.EmailSentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 13, true);
			this.EmailSentLabel.TabIndex = 12;
			this.EmailSentLabel.Text = "An email has been sent to the client for this task";
			this.EmailSentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			//
			// MainTabControl
			//
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.IncidentTabPage);
			this.MainTabControl.Controls.Add(this.RelatedItemsTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.NotesTabPage);
			this.MainTabControl.Controls.Add(this.EventsTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 58, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 513, true);
			this.MainTabControl.TabIndex = 13;
			this.MainTabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.MainTabControl_Selecting);
			//
			// IncidentTabPage
			//
			this.IncidentTabPage.Controls.Add(this.PricingAndMaintenanceFeeGroupBox);
			this.IncidentTabPage.Controls.Add(this.ClientAndContactDetailsGroupBox);
			this.IncidentTabPage.Controls.Add(this.StatusGroupBox);
			this.IncidentTabPage.Controls.Add(this.IncidentDetailsGroupBox);
			this.IncidentTabPage.Controls.Add(this.DescriptionAndDetailsPanel);
			this.IncidentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.IncidentTabPage.Name = "IncidentTabPage";
			this.IncidentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 486, true);
			this.IncidentTabPage.TabIndex = 0;
			this.IncidentTabPage.Text = "Professional Services Quote";
			//
			// PricingAndMaintenanceFeeGroupBox
			//
			this.PricingAndMaintenanceFeeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PricingAndMaintenanceFeeGroupBox.Controls.Add(this.MaintenanceAndUpgradeAcceptedDateEdit);
			this.PricingAndMaintenanceFeeGroupBox.Controls.Add(this.DepositAmountCalcFindBox);
			this.PricingAndMaintenanceFeeGroupBox.Controls.Add(this.QuoteAmountCalcFindBox);
			this.PricingAndMaintenanceFeeGroupBox.Controls.Add(this.MaintenanceAndUpgradeAcceptedLabel);
			this.PricingAndMaintenanceFeeGroupBox.Controls.Add(this.DepositAmountLabel);
			this.PricingAndMaintenanceFeeGroupBox.Controls.Add(this.QuoteAmountLabel);
			this.PricingAndMaintenanceFeeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 151, true);
			this.PricingAndMaintenanceFeeGroupBox.Name = "PricingAndMaintenanceFeeGroupBox";
			this.PricingAndMaintenanceFeeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 114, true);
			this.PricingAndMaintenanceFeeGroupBox.TabIndex = 5;
			this.PricingAndMaintenanceFeeGroupBox.TabStop = false;
			this.PricingAndMaintenanceFeeGroupBox.Text = "Pricing && Maintenance Fee";
			//
			// MaintenanceAndUpgradeAcceptedDateEdit
			//
			this.MaintenanceAndUpgradeAcceptedDateEdit.AllowDrop = true;
			this.MaintenanceAndUpgradeAcceptedDateEdit.AutoCompleteMonthThreshold = 1;
			this.MaintenanceAndUpgradeAcceptedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.MaintenanceAndUpgradeAcceptedDateEdit, "IM_UpgradeAssuranceAccepted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_UpgradeAssuranceAccepted)));
			this.MaintenanceAndUpgradeAcceptedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.MaintenanceAndUpgradeAcceptedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 79, true);
			this.MaintenanceAndUpgradeAcceptedDateEdit.Name = "MaintenanceAndUpgradeAcceptedDateEdit";
			this.MaintenanceAndUpgradeAcceptedDateEdit.TabIndex = 5;
			//
			// DepositAmountCalcFindBox
			//
			this.DepositAmountCalcFindBox.AllowDrop = true;
			this.DepositAmountCalcFindBox.BindToAmount = "IM_DepositAmountRequired";
			this.DepositAmountCalcFindBox.BindToList = "Lookups+QuoteCurrencies";
			this.DepositAmountCalcFindBox.BindToUnit = "IM_RX_NKQuoteCurrency";
			this.DepositAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.DepositAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 46, true);
			this.DepositAmountCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.DepositAmountCalcFindBox.Name = "DepositAmountCalcFindBox";
			this.DepositAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.DepositAmountCalcFindBox.TabIndex = 3;
			//
			// QuoteAmountCalcFindBox
			//
			this.QuoteAmountCalcFindBox.AllowDrop = true;
			this.QuoteAmountCalcFindBox.BindToAmount = "IM_QuoteAmount";
			this.QuoteAmountCalcFindBox.BindToList = "Lookups+QuoteCurrencies";
			this.QuoteAmountCalcFindBox.BindToUnit = "IM_RX_NKQuoteCurrency";
			this.QuoteAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.QuoteAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 15, true);
			this.QuoteAmountCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.QuoteAmountCalcFindBox.Name = "QuoteAmountCalcFindBox";
			this.QuoteAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.QuoteAmountCalcFindBox.TabIndex = 1;
			//
			// MaintenanceAndUpgradeAcceptedLabel
			//
			this.MaintenanceAndUpgradeAcceptedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 71, true);
			this.MaintenanceAndUpgradeAcceptedLabel.Name = "MaintenanceAndUpgradeAcceptedLabel";
			this.MaintenanceAndUpgradeAcceptedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 30, true);
			this.MaintenanceAndUpgradeAcceptedLabel.TabIndex = 4;
			this.MaintenanceAndUpgradeAcceptedLabel.Text = "Maintenance && Upgrade Accepted:";
			//
			// DepositAmountLabel
			//
			this.DepositAmountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 46, true);
			this.DepositAmountLabel.Name = "DepositAmountLabel";
			this.DepositAmountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.DepositAmountLabel.TabIndex = 2;
			this.DepositAmountLabel.Text = "Deposit Amount:";
			//
			// QuoteAmountLabel
			//
			this.QuoteAmountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.QuoteAmountLabel.Name = "QuoteAmountLabel";
			this.QuoteAmountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 21, true);
			this.QuoteAmountLabel.TabIndex = 0;
			this.QuoteAmountLabel.Text = "Quote Amount:";
			//
			// ClientAndContactDetailsGroupBox
			//
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ClientFullNameLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ClientLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.EditContactButton);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ContactGuidFindBox);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ContractLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ContractStatusLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ContactEmailTextBox);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.BranchFaxNumberTextBox);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.BranchPhoneNumberLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.BranchPhoneNumberTextBox);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ContactLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.BranchFaxNumberLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ContactEmailLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ContactNumberLabel);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ContactNumberTextBox);
			this.ClientAndContactDetailsGroupBox.Controls.Add(this.ClientAddressControl);
			this.ClientAndContactDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.ClientAndContactDetailsGroupBox.Name = "ClientAndContactDetailsGroupBox";
			this.ClientAndContactDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 257, true);
			this.ClientAndContactDetailsGroupBox.TabIndex = 4;
			this.ClientAndContactDetailsGroupBox.TabStop = false;
			this.ClientAndContactDetailsGroupBox.Text = "Client and Contact Details";
			//
			// ClientFullNameLabel
			//
			this.BindingSource.SetBindingMember(this.ClientFullNameLabel, "ClientName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).ClientName)));
			this.ClientFullNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 15, true);
			this.ClientFullNameLabel.Name = "ClientFullNameLabel";
			this.ClientFullNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 21, true);
			this.ClientFullNameLabel.TabIndex = 0;
			this.ClientFullNameLabel.Text = "<Client Full Name>";
			//
			// ClientLabel
			//
			this.ClientLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.ClientLabel.Name = "ClientLabel";
			this.ClientLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 21, true);
			this.ClientLabel.TabIndex = 1;
			this.ClientLabel.Text = "Client:";
			//
			// EditContactButton
			//
			this.EditContactButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 119, true);
			this.EditContactButton.Name = "EditContactButton";
			this.EditContactButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 21, true);
			this.EditContactButton.TabIndex = 7;
			this.EditContactButton.Text = "Edit";
			this.EditContactButton.Click += new System.EventHandler(this.EditContactButton_Click);
			//
			// ContactGuidFindBox
			//
			this.ContactGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactGuidFindBox, "IM_OC_Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_OC_Contact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).Lookups.ListHelper.OrganisationContactList)));
			this.ContactGuidFindBox.BindToList = "Lookups+ListHelper+OrganisationContactList";
			this.ContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 119, true);
			this.ContactGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			this.ContactGuidFindBox.Name = "ContactGuidFindBox";
			this.ContactGuidFindBox.PopupCaption = "Select the Contact";
			this.ContactGuidFindBox.PreBoundMaxLength = 25;
			this.ContactGuidFindBox.ShowDescriptionBox = false;
			this.ContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.ContactGuidFindBox.TabIndex = 6;
			//
			// ContractLabel
			//
			this.ContractLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 97, true);
			this.ContractLabel.Name = "ContractLabel";
			this.ContractLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			this.ContractLabel.TabIndex = 3;
			this.ContractLabel.Text = "Contract:";
			//
			// ContractStatusLabel
			//
			this.ContractStatusLabel.AccessibleDescription = "";
			this.BindingSource.SetBindingMember(this.ContractStatusLabel, "IM_ClientContractStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_ClientContractStatus)));
			this.ContractStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 97, true);
			this.ContractStatusLabel.Name = "ContractStatusLabel";
			this.ContractStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 22, true);
			this.ContractStatusLabel.TabIndex = 4;
			this.ContractStatusLabel.Text = "<Contract Status>";
			//
			// ContactEmailTextBox
			//
			this.BindingSource.SetBindingMember(this.ContactEmailTextBox, "Contact+OC_Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).Contact.OC_Email)));
			this.ContactEmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 163, true);
			this.ContactEmailTextBox.Name = "ContactEmailTextBox";
			this.ContactEmailTextBox.ReadOnly = true;
			this.ContactEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.ContactEmailTextBox.TabIndex = 11;
			//
			// BranchFaxNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.BranchFaxNumberTextBox, "IM_Calc_BranchFax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_Calc_BranchFax)));
			this.BranchFaxNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BranchFaxNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 208, true);
			this.BranchFaxNumberTextBox.Name = "BranchFaxNumberTextBox";
			this.BranchFaxNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.BranchFaxNumberTextBox.TabIndex = 15;
			//
			// BranchPhoneNumberLabel
			//
			this.BranchPhoneNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 186, true);
			this.BranchPhoneNumberLabel.Name = "BranchPhoneNumberLabel";
			this.BranchPhoneNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 21, true);
			this.BranchPhoneNumberLabel.TabIndex = 12;
			this.BranchPhoneNumberLabel.Text = "Branch No:";
			//
			// BranchPhoneNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.BranchPhoneNumberTextBox, "IM_Calc_BranchPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_Calc_BranchPhone)));
			this.BranchPhoneNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BranchPhoneNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 186, true);
			this.BranchPhoneNumberTextBox.Name = "BranchPhoneNumberTextBox";
			this.BranchPhoneNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.BranchPhoneNumberTextBox.TabIndex = 13;
			//
			// ContactLabel
			//
			this.ContactLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 119, true);
			this.ContactLabel.Name = "ContactLabel";
			this.ContactLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 21, true);
			this.ContactLabel.TabIndex = 5;
			this.ContactLabel.Text = "Contact:";
			//
			// BranchFaxNumberLabel
			//
			this.BranchFaxNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 208, true);
			this.BranchFaxNumberLabel.Name = "BranchFaxNumberLabel";
			this.BranchFaxNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.BranchFaxNumberLabel.TabIndex = 14;
			this.BranchFaxNumberLabel.Text = "Branch Fax:";
			//
			// ContactEmailLabel
			//
			this.ContactEmailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 163, true);
			this.ContactEmailLabel.Name = "ContactEmailLabel";
			this.ContactEmailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			this.ContactEmailLabel.TabIndex = 10;
			this.ContactEmailLabel.Text = "Contact Email:";
			//
			// ContactNumberLabel
			//
			this.ContactNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 141, true);
			this.ContactNumberLabel.Name = "ContactNumberLabel";
			this.ContactNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 21, true);
			this.ContactNumberLabel.TabIndex = 8;
			this.ContactNumberLabel.Text = "Contact No:";
			//
			// ContactNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.ContactNumberTextBox, "Contact+OC_Phone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).Contact.OC_Phone)));
			this.ContactNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 141, true);
			this.ContactNumberTextBox.Name = "ContactNumberTextBox";
			this.ContactNumberTextBox.ReadOnly = true;
			this.ContactNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 20, true);
			this.ContactNumberTextBox.TabIndex = 9;
			//
			// ClientAddressControl
			//
			this.ClientAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientAddressControl, "IM_OA_BranchAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_OA_BranchAddress)));
			this.ClientAddressControl.BindToOrgList = "Lookups+ActiveClientList";
			this.ClientAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 37, true);
			this.ClientAddressControl.Name = "ClientAddressControl";
			this.ClientAddressControl.PopupCaption = "";
			this.ClientAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 58, true);
			this.ClientAddressControl.TabIndex = 2;
			//
			// StatusGroupBox
			//
			this.StatusGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.StatusGroupBox.Controls.Add(this.NoRadioButton);
			this.StatusGroupBox.Controls.Add(this.YesRadioButton);
			this.StatusGroupBox.Controls.Add(this.WorkChargableLabel);
			this.StatusGroupBox.Controls.Add(this.StatusDropEdit);
			this.StatusGroupBox.Controls.Add(this.RequiredByDateEdit);
			this.StatusGroupBox.Controls.Add(this.LoggedAtDateEdit);
			this.StatusGroupBox.Controls.Add(this.LoggedAtLabel);
			this.StatusGroupBox.Controls.Add(this.RequiredByLabel);
			this.StatusGroupBox.Controls.Add(this.StatusLabel);
			this.StatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 8, true);
			this.StatusGroupBox.Name = "StatusGroupBox";
			this.StatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 137, true);
			this.StatusGroupBox.TabIndex = 2;
			this.StatusGroupBox.TabStop = false;
			this.StatusGroupBox.Text = "Status";
			//
			// NoRadioButton
			//
			this.NoRadioButton.AutoCheck = false;
			this.NoRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NoRadioButton, "IM_ChargableWorkNoSelection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_ChargableWorkNoSelection)));
			this.NoRadioButton.CaptionResourceString = ZClientEDI.Res.GetData("ProfessionalServicesQuoteForm|2babd423-2159-4cd9-8e86-e9a8e035cbf2", "No");
			this.NoRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NoRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 78, true);
			this.NoRadioButton.Name = "NoRadioButton";
			this.NoRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
			this.NoRadioButton.TabIndex = 11;
			this.NoRadioButton.Text = "No";
			//
			// YesRadioButton
			//
			this.YesRadioButton.AutoCheck = false;
			this.YesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.YesRadioButton, "IM_ChargableWorkYesSelection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_ChargableWorkYesSelection)));
			this.YesRadioButton.CaptionResourceString = ZClientEDI.Res.GetData("ProfessionalServicesQuoteForm|38882814-6421-4fda-8d91-d97e49691a10", "Yes");
			this.YesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.YesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 78, true);
			this.YesRadioButton.Name = "YesRadioButton";
			this.YesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 17, true);
			this.YesRadioButton.TabIndex = 10;
			this.YesRadioButton.Text = "Yes";
			//
			// WorkChargableLabel
			//
			this.WorkChargableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 75, true);
			this.WorkChargableLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 16, true);
			this.WorkChargableLabel.Name = "WorkChargableLabel";
			this.WorkChargableLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 16, true);
			this.WorkChargableLabel.TabIndex = 9;
			this.WorkChargableLabel.Text = "Work Chargeable:";
			//
			// StatusDropEdit
			//
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "IM_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).Lookups.StatusList)));
			this.StatusDropEdit.BindToList = "Lookups+StatusList";
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 104, true);
			this.StatusDropEdit.MaxItemsToShowInDropDown = 20;
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.PreBoundMaxLength = 3;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.StatusDropEdit.TabIndex = 8;
			//
			// RequiredByDateEdit
			//
			this.RequiredByDateEdit.AllowDrop = true;
			this.RequiredByDateEdit.AutoCompleteMonthThreshold = 1;
			this.RequiredByDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RequiredByDateEdit, "IM_RequiredBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_RequiredBy)));
			this.RequiredByDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.RequiredByDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 45, true);
			this.RequiredByDateEdit.Name = "RequiredByDateEdit";
			this.RequiredByDateEdit.TabIndex = 3;
			//
			// LoggedAtDateEdit
			//
			this.LoggedAtDateEdit.AllowDrop = true;
			this.LoggedAtDateEdit.AutoCompleteMonthThreshold = 1;
			this.LoggedAtDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LoggedAtDateEdit, "IM_SystemCreateTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_SystemCreateTimeUtc)));
			this.LoggedAtDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LoggedAtDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
			this.LoggedAtDateEdit.Name = "LoggedAtDateEdit";
			this.LoggedAtDateEdit.TabIndex = 1;
			//
			// LoggedAtLabel
			//
			this.LoggedAtLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.LoggedAtLabel.Name = "LoggedAtLabel";
			this.LoggedAtLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.LoggedAtLabel.TabIndex = 0;
			this.LoggedAtLabel.Text = "Logged At:";
			//
			// RequiredByLabel
			//
			this.RequiredByLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 45, true);
			this.RequiredByLabel.Name = "RequiredByLabel";
			this.RequiredByLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.RequiredByLabel.TabIndex = 2;
			this.RequiredByLabel.Text = "Required By:";
			//
			// StatusLabel
			//
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 104, true);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 23, true);
			this.StatusLabel.TabIndex = 7;
			this.StatusLabel.Text = "Status:";
			//
			// IncidentDetailsGroupBox
			//
			this.IncidentDetailsGroupBox.Controls.Add(this.WorkItemTypeDropEdit);
			this.IncidentDetailsGroupBox.Controls.Add(this.QuotationTypeLabel);
			this.IncidentDetailsGroupBox.Controls.Add(this.EstimatedWorkHoursCalcEdit);
			this.IncidentDetailsGroupBox.Controls.Add(this.EstimatedWorkHoursLabel);
			this.IncidentDetailsGroupBox.Controls.Add(this.ProductDropEdit);
			this.IncidentDetailsGroupBox.Controls.Add(this.ProductLabel);
			this.IncidentDetailsGroupBox.Controls.Add(this.ProgramAreaLabel);
			this.IncidentDetailsGroupBox.Controls.Add(this.ProgramAreaDropEdit);
			this.IncidentDetailsGroupBox.Controls.Add(this.CustomerServiceContactCodeFindBox);
			this.IncidentDetailsGroupBox.Controls.Add(this.AssignedToCodeFindBox);
			this.IncidentDetailsGroupBox.Controls.Add(this.CustomerServiceContactLabel);
			this.IncidentDetailsGroupBox.Controls.Add(this.AssignedTeamGuidFindBox);
			this.IncidentDetailsGroupBox.Controls.Add(this.AssignedToLabel);
			this.IncidentDetailsGroupBox.Controls.Add(this.AssignedTeamLabel);
			this.IncidentDetailsGroupBox.Controls.Add(this.CommentsLabel);
			this.IncidentDetailsGroupBox.Controls.Add(this.CommentsTextBox);
			this.IncidentDetailsGroupBox.Controls.Add(this.RequestedPriorityDropEdit);
			this.IncidentDetailsGroupBox.Controls.Add(this.RequestedPriorityLabel);
			this.IncidentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 8, true);
			this.IncidentDetailsGroupBox.Name = "IncidentDetailsGroupBox";
			this.IncidentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 257, true);
			this.IncidentDetailsGroupBox.TabIndex = 1;
			this.IncidentDetailsGroupBox.TabStop = false;
			this.IncidentDetailsGroupBox.Text = "Incident Details";
			//
			// WorkItemTypeDropEdit
			//
			this.WorkItemTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WorkItemTypeDropEdit, "IM_WorkItemType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_WorkItemType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).Lookups.WorkItemTypeList)));
			this.WorkItemTypeDropEdit.BindToList = "Lookups.WorkItemTypeList";
			this.WorkItemTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 199, true);
			this.WorkItemTypeDropEdit.MaxItemsToShowInDropDown = 20;
			this.WorkItemTypeDropEdit.Name = "WorkItemTypeDropEdit";
			this.WorkItemTypeDropEdit.PreBoundMaxLength = 3;
			this.WorkItemTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.WorkItemTypeDropEdit.TabIndex = 9;
			//
			// QuotationTypeLabel
			//
			this.QuotationTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 199, true);
			this.QuotationTypeLabel.Name = "QuotationTypeLabel";
			this.QuotationTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.QuotationTypeLabel.TabIndex = 20;
			this.QuotationTypeLabel.Text = "Quotation Type:";
			//
			// EstimatedWorkHoursCalcEdit
			//
			this.EstimatedWorkHoursCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.EstimatedWorkHoursCalcEdit, "IM_EstimatedHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_EstimatedHours)));
			this.EstimatedWorkHoursCalcEdit.DecimalPlaces = 0;
			this.EstimatedWorkHoursCalcEdit.Decimals = 0;
			this.EstimatedWorkHoursCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 224, true);
			this.EstimatedWorkHoursCalcEdit.Name = "EstimatedWorkHoursCalcEdit";
			this.EstimatedWorkHoursCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.EstimatedWorkHoursCalcEdit.TabIndex = 10;
			this.EstimatedWorkHoursCalcEdit.Text = "0";
			this.EstimatedWorkHoursCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// EstimatedWorkHoursLabel
			//
			this.EstimatedWorkHoursLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 224, true);
			this.EstimatedWorkHoursLabel.Name = "EstimatedWorkHoursLabel";
			this.EstimatedWorkHoursLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.EstimatedWorkHoursLabel.TabIndex = 22;
			this.EstimatedWorkHoursLabel.Text = "Est. Work Hours:";
			//
			// ProductDropEdit
			//
			this.ProductDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductDropEdit, "IM_Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).Lookups.ListHelper.ProductList)));
			this.ProductDropEdit.BindToList = "Lookups+ListHelper+ProductList";
			this.ProductDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 99, true);
			this.ProductDropEdit.MaxItemsToShowInDropDown = 20;
			this.ProductDropEdit.Name = "ProductDropEdit";
			this.ProductDropEdit.PreBoundMaxLength = 3;
			this.ProductDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ProductDropEdit.TabIndex = 5;
			//
			// ProductLabel
			//
			this.ProductLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 99, true);
			this.ProductLabel.Name = "ProductLabel";
			this.ProductLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 21, true);
			this.ProductLabel.TabIndex = 11;
			this.ProductLabel.Text = "Product:";
			//
			// ProgramAreaLabel
			//
			this.ProgramAreaLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 123, true);
			this.ProgramAreaLabel.Name = "ProgramAreaLabel";
			this.ProgramAreaLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 22, true);
			this.ProgramAreaLabel.TabIndex = 13;
			this.ProgramAreaLabel.Text = "Program Area:";
			//
			// ProgramAreaDropEdit
			//
			this.ProgramAreaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProgramAreaDropEdit, "IM_ProgramArea");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_ProgramArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).Lookups.ListHelper.ProgramAreaList)));
			this.ProgramAreaDropEdit.BindToList = "Lookups+ListHelper+ProgramAreaList";
			this.ProgramAreaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 124, true);
			this.ProgramAreaDropEdit.Name = "ProgramAreaDropEdit";
			this.ProgramAreaDropEdit.PreBoundMaxLength = 3;
			this.ProgramAreaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ProgramAreaDropEdit.TabIndex = 6;
			//
			// CustomerServiceContactCodeFindBox
			//
			this.CustomerServiceContactCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomerServiceContactCodeFindBox, "IM_GS_NKCustServiceContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_GS_NKCustServiceContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).Lookups.CustServiceContacts)));
			this.CustomerServiceContactCodeFindBox.BindToList = "Lookups+CustServiceContacts";
			this.CustomerServiceContactCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 23, true);
			this.CustomerServiceContactCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.CustomerServiceContactCodeFindBox.Name = "CustomerServiceContactCodeFindBox";
			this.CustomerServiceContactCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.CustomerServiceContactCodeFindBox.TabIndex = 2;
			//
			// AssignedToCodeFindBox
			//
			this.AssignedToCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AssignedToCodeFindBox, "IM_GS_NKAssignedToCurrent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_GS_NKAssignedToCurrent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).Lookups.ListHelper.ActiveGroupDependentStaffList)));
			this.AssignedToCodeFindBox.BindToList = "Lookups+ListHelper+ActiveGroupDependentStaffList";
			this.AssignedToCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 74, true);
			this.AssignedToCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.AssignedToCodeFindBox.Name = "AssignedToCodeFindBox";
			this.AssignedToCodeFindBox.PopupCaption = "Select the Staff to assignt this Incident to";
			this.AssignedToCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.AssignedToCodeFindBox.TabIndex = 4;
			//
			// CustomerServiceContactLabel
			//
			this.CustomerServiceContactLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.CustomerServiceContactLabel.Name = "CustomerServiceContactLabel";
			this.CustomerServiceContactLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 28, true);
			this.CustomerServiceContactLabel.TabIndex = 4;
			this.CustomerServiceContactLabel.Text = "Customer Service Contact:";
			//
			// AssignedTeamGuidFindBox
			//
			this.AssignedTeamGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AssignedTeamGuidFindBox, "IM_GG_Team");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_GG_Team)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).Lookups.ListHelper.ActiveStaffDependentGroupList)));
			this.AssignedTeamGuidFindBox.BindToList = "Lookups+ListHelper+ActiveStaffDependentGroupList";
			this.AssignedTeamGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 48, true);
			this.AssignedTeamGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbGroup;
			this.AssignedTeamGuidFindBox.Name = "AssignedTeamGuidFindBox";
			this.AssignedTeamGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.AssignedTeamGuidFindBox.TabIndex = 3;
			//
			// AssignedToLabel
			//
			this.AssignedToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 73, true);
			this.AssignedToLabel.Name = "AssignedToLabel";
			this.AssignedToLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.AssignedToLabel.TabIndex = 4;
			this.AssignedToLabel.Text = "Assigned To:";
			//
			// AssignedTeamLabel
			//
			this.AssignedTeamLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 47, true);
			this.AssignedTeamLabel.Name = "AssignedTeamLabel";
			this.AssignedTeamLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.AssignedTeamLabel.TabIndex = 2;
			this.AssignedTeamLabel.Text = "Assigned Team:";
			//
			// CommentsLabel
			//
			this.CommentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 148, true);
			this.CommentsLabel.Name = "CommentsLabel";
			this.CommentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.CommentsLabel.TabIndex = 6;
			this.CommentsLabel.Text = "Comments:";
			//
			// CommentsTextBox
			//
			this.BindingSource.SetBindingMember(this.CommentsTextBox, "IM_SubCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_SubCategory)));
			this.CommentsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CommentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 149, true);
			this.CommentsTextBox.Name = "CommentsTextBox";
			this.CommentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.CommentsTextBox.TabIndex = 7;
			//
			// RequestedPriorityDropEdit
			//
			this.RequestedPriorityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RequestedPriorityDropEdit, "IM_Priority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_Priority)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).Lookups.PriorityList)));
			this.RequestedPriorityDropEdit.BindToList = "Lookups+PriorityList";
			this.RequestedPriorityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 174, true);
			this.RequestedPriorityDropEdit.Name = "RequestedPriorityDropEdit";
			this.RequestedPriorityDropEdit.PreBoundMaxLength = 3;
			this.RequestedPriorityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.RequestedPriorityDropEdit.TabIndex = 8;
			//
			// RequestedPriorityLabel
			//
			this.RequestedPriorityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 173, true);
			this.RequestedPriorityLabel.Name = "RequestedPriorityLabel";
			this.RequestedPriorityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.RequestedPriorityLabel.TabIndex = 8;
			this.RequestedPriorityLabel.Text = "Req. Priority:";
			//
			// DescriptionAndDetailsPanel
			//
			this.DescriptionAndDetailsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DescriptionAndDetailsPanel.Controls.Add(this.DetailsGroupBox);
			this.DescriptionAndDetailsPanel.Controls.Add(this.zGroupBox1);
			this.DescriptionAndDetailsPanel.Controls.Add(this.DescriptionTextBox);
			this.DescriptionAndDetailsPanel.Controls.Add(this.DescriptionLabel);
			this.DescriptionAndDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 271, true);
			this.DescriptionAndDetailsPanel.Name = "DescriptionAndDetailsPanel";
			this.DescriptionAndDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 213, true);
			this.DescriptionAndDetailsPanel.TabIndex = 3;
			//
			// DetailsGroupBox
			//
			this.DetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DetailsGroupBox.Controls.Add(this.DetailsRichTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 85, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 127, true);
			this.DetailsGroupBox.TabIndex = 3;
			this.DetailsGroupBox.TabStop = false;
			this.DetailsGroupBox.Text = "Details (enter details of the problem the client is having)";
			//
			// DetailsRichTextBox
			//
			this.DetailsRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DetailsRichTextBox, "IM_Details");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_Details)));
			this.DetailsRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 15, true);
			this.DetailsRichTextBox.MaxLength = 10000000;
			this.DetailsRichTextBox.Name = "DetailsRichTextBox";
			this.DetailsRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 107, true);
			this.DetailsRichTextBox.TabIndex = 0;
			//
			// zGroupBox1
			//
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.Controls.Add(this.InvoiceDetailsTextBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 5, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 63, true);
			this.zGroupBox1.TabIndex = 4;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Invoice Details";
			//
			// InvoiceDetailsTextBox
			//
			this.InvoiceDetailsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.InvoiceDetailsTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.InvoiceDetailsTextBox, "InvoiceDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).InvoiceDetails)));
			this.InvoiceDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.InvoiceDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.InvoiceDetailsTextBox.Multiline = true;
			this.InvoiceDetailsTextBox.Name = "InvoiceDetailsTextBox";
			this.InvoiceDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 42, true);
			this.InvoiceDetailsTextBox.TabIndex = 0;
			//
			// DescriptionTextBox
			//
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "IM_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote)(null)).IM_Description)));
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 15, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 20, true);
			this.DescriptionTextBox.TabIndex = 1;
			//
			// DescriptionLabel
			//
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.DescriptionLabel.TabIndex = 0;
			this.DescriptionLabel.Text = "Description:";
			//
			// RelatedItemsTabPage
			//
			this.RelatedItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelatedItemsTabPage.Name = "RelatedItemsTabPage";
			this.RelatedItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 486, true);
			this.RelatedItemsTabPage.TabIndex = 6;
			this.RelatedItemsTabPage.Text = "Related Items";
			//
			// WorkflowTabPage
			//
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 486, true);
			this.WorkflowTabPage.TabIndex = 8;
			//
			// NotesTabPage
			//
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NotesTabPage.Name = "NotesTabPage";
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 486, true);
			this.NotesTabPage.TabIndex = 2;
			//
			// EventsTabPage
			//
			this.EventsTabPage.ExcludeFromBindingOnSave = true;
			this.EventsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EventsTabPage.Name = "EventsTabPage";
			this.EventsTabPage.ShouldBeReadOnlyInViewMode = false;
			this.EventsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 486, true);
			this.EventsTabPage.TabIndex = 3;
			//
			// ProfessionalServicesQuoteForm
			//
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 652, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.TopPanel);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote);
			this.DataSourceTypeName = "Enterprise.Client.EDI.IncidentManager.Business.ProfessionalServicesQuote";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 691, true);
			this.Name = "ProfessionalServicesQuoteForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "IncidentManagerForm";
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.ClientHeaderGuidFindBox.ResumeLayout(true);
			this.ClientHeaderGuidFindBox.PerformLayout();
			this.AddedByGuidFindBox.ResumeLayout(true);
			this.AddedByGuidFindBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.EmailMenuPanel.ResumeLayout(false);
			this.EmailMenuPanel.PerformLayout();
			this.zPanel3.ResumeLayout(false);
			this.zPanel3.PerformLayout();
			this.PostingButtons.ResumeLayout(true);
			this.PostingButtons.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.EventsTabPage.PerformLayout();
			this.NotesTabPage.PerformLayout();
			this.IncidentTabPage.ResumeLayout(false);
			this.IncidentTabPage.PerformLayout();
			this.PricingAndMaintenanceFeeGroupBox.ResumeLayout(false);
			this.PricingAndMaintenanceFeeGroupBox.PerformLayout();
			this.MaintenanceAndUpgradeAcceptedDateEdit.ResumeLayout(true);
			this.MaintenanceAndUpgradeAcceptedDateEdit.PerformLayout();
			this.DepositAmountCalcFindBox.ResumeLayout(true);
			this.DepositAmountCalcFindBox.PerformLayout();
			this.QuoteAmountCalcFindBox.ResumeLayout(true);
			this.QuoteAmountCalcFindBox.PerformLayout();
			this.ClientAndContactDetailsGroupBox.ResumeLayout(false);
			this.ClientAndContactDetailsGroupBox.PerformLayout();
			this.ContactGuidFindBox.ResumeLayout(true);
			this.ContactGuidFindBox.PerformLayout();
			this.ClientAddressControl.ResumeLayout(true);
			this.ClientAddressControl.PerformLayout();
			this.StatusGroupBox.ResumeLayout(false);
			this.StatusGroupBox.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.RequiredByDateEdit.ResumeLayout(true);
			this.RequiredByDateEdit.PerformLayout();
			this.LoggedAtDateEdit.ResumeLayout(true);
			this.LoggedAtDateEdit.PerformLayout();
			this.IncidentDetailsGroupBox.ResumeLayout(false);
			this.IncidentDetailsGroupBox.PerformLayout();
			this.WorkItemTypeDropEdit.ResumeLayout(true);
			this.WorkItemTypeDropEdit.PerformLayout();
			this.ProductDropEdit.ResumeLayout(true);
			this.ProductDropEdit.PerformLayout();
			this.ProgramAreaDropEdit.ResumeLayout(true);
			this.ProgramAreaDropEdit.PerformLayout();
			this.CustomerServiceContactCodeFindBox.ResumeLayout(true);
			this.CustomerServiceContactCodeFindBox.PerformLayout();
			this.AssignedToCodeFindBox.ResumeLayout(true);
			this.AssignedToCodeFindBox.PerformLayout();
			this.AssignedTeamGuidFindBox.ResumeLayout(true);
			this.AssignedTeamGuidFindBox.PerformLayout();
			this.RequestedPriorityDropEdit.ResumeLayout(true);
			this.RequestedPriorityDropEdit.PerformLayout();
			this.DescriptionAndDetailsPanel.ResumeLayout(false);
			this.DescriptionAndDetailsPanel.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.DetailsRichTextBox.ResumeLayout(true);
			this.DetailsRichTextBox.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
