namespace Enterprise.DocumentEngine.GUI
{
	partial class DocDeliveryForm
	{
		#region Windows Form Designer generated code

		private Enterprise.ZArchitecture.GUI.ZPanel zPanel2;
		protected Enterprise.DocumentEngine.GUI.DocumentDelivery.PrinterHelpControl printerHelpControl1;
		internal Enterprise.ZArchitecture.GUI.ZButton VisualiseButton;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox PrintAsDraftCheckbox;
		Enterprise.ZArchitecture.GUI.ZPanel bottomPanel;
		Enterprise.ZArchitecture.GUI.ZPanel pageRangesPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox DocumentsGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox IncludedEDocsGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox ShowOnlyPrintersUserCanPrintToCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZGuidDropEdit PrinterSelectionGuidDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZButton DeliverButton;
		internal Enterprise.ZArchitecture.GUI.ZButton PreviewButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox MultiDocPackGroupbox;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton AutoDeliverMultipleDocPackRadioButton;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton PrintMultipleDocPackRadioButton;
		Enterprise.ZArchitecture.ZCalcEdit NumCopiesCalcEdit;
		internal Enterprise.ZArchitecture.ZTextBox CoverNoteTextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox IncludeCoverNoteCheckBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox IndividualDocPackGroupBox;
		protected Enterprise.ZArchitecture.ZGrid DocumentsGrid;
		protected Enterprise.ZArchitecture.ZGrid IncludedEDocsGrid;
		internal Enterprise.ZArchitecture.ZGrid RecipientsGrid;
		public Enterprise.ZArchitecture.ZLabel MultiDocPacksLabel;
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage MainPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage DocumentsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage IncludedEDocsTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage CoverNotePage;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo ZCalcEditColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.DeliverButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PreviewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MultiDocPackGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PrintMultipleDocPackRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.AutoDeliverMultipleDocPackRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.NumMultiDocPacksLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MultiDocPacksLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CoverNoteTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IncludeCoverNoteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IndividualDocPackGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RecipientsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.IncludedEDocsGrid = new ZGridThatNotNeedNotifyHasChanges();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IncludedEDocsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.IncludedEDocsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CoverNotePage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NumCopiesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PrinterSelectionGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.PrintAsDraftCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.pageRangesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TotalPagesOfLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalPagesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PageRangesSpecifiedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PageRangesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LanguageZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BackgroundDeliveryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.printerHelpControl1 = new Enterprise.DocumentEngine.GUI.DocumentDelivery.PrinterHelpControl();
			this.VisualiseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.saveAsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ShowOnlyPrintersUserCanPrintToCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MultiDocPackGroupbox.SuspendLayout();
			this.IndividualDocPackGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IncludedEDocsGrid)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.MainPage.SuspendLayout();
			this.DocumentsTabPage.SuspendLayout();
			this.IncludedEDocsTabPage.SuspendLayout();
			this.DocumentsGroupBox.SuspendLayout();
			this.IncludedEDocsGroupBox.SuspendLayout();
			this.CoverNotePage.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.pageRangesPanel.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 239, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 4, true);
			this.MainStatusBar.TabIndex = 2;
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.DeliveryInstructions);
			// 
			// DeliverButton
			// 
			this.DeliverButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|c541962f-8fed-44c6-9425-a087eae35548", "&Deliver");
			this.DeliverButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.DeliverButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 0, true);
			this.DeliverButton.Name = "DeliverButton";
			this.DeliverButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
			this.DeliverButton.TabIndex = 3;
			this.DeliverButton.Click += new System.EventHandler(this.DeliverButton_Click);
			// 
			// PreviewButton
			// 
			this.PreviewButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|a839fb86-f693-4615-ab56-df02fa67e20e", "Pre&view");
			this.PreviewButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.PreviewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 0, true);
			this.PreviewButton.Name = "PreviewButton";
			this.PreviewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
			this.PreviewButton.TabIndex = 5;
			this.PreviewButton.Visible = false;
			this.PreviewButton.Click += new System.EventHandler(this.PreviewButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|cad08e4e-d449-4ef0-8fc7-461de3dd0ded", "&Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(652, 0, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// MultiDocPackGroupbox
			// 
			this.MultiDocPackGroupbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MultiDocPackGroupbox.Controls.Add(this.PrintMultipleDocPackRadioButton);
			this.MultiDocPackGroupbox.Controls.Add(this.AutoDeliverMultipleDocPackRadioButton);
			this.MultiDocPackGroupbox.Controls.Add(this.NumMultiDocPacksLabel);
			this.MultiDocPackGroupbox.Controls.Add(this.MultiDocPacksLabel);
			this.MultiDocPackGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MultiDocPackGroupbox.Name = "MultiDocPackGroupbox";
			this.MultiDocPackGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 111, true);
			this.MultiDocPackGroupbox.TabIndex = 1;
			this.MultiDocPackGroupbox.TabStop = false;
			this.MultiDocPackGroupbox.Visible = false;
			// 
			// PrintMultipleDocPackRadioButton
			// 
			this.PrintMultipleDocPackRadioButton.AutoCheck = false;
			this.PrintMultipleDocPackRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintMultipleDocPackRadioButton, "PrintMultiDocPack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).PrintMultiDocPack)));
			this.PrintMultipleDocPackRadioButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|3813d73e-0d14-4850-924f-1bf6e0cf6c6c", "Print multiple document packs");
			this.PrintMultipleDocPackRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintMultipleDocPackRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 80, true);
			this.PrintMultipleDocPackRadioButton.Name = "PrintMultipleDocPackRadioButton";
			this.PrintMultipleDocPackRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.PrintMultipleDocPackRadioButton.TabIndex = 4;
			// 
			// AutoDeliverMultipleDocPackRadioButton
			// 
			this.AutoDeliverMultipleDocPackRadioButton.AutoCheck = false;
			this.AutoDeliverMultipleDocPackRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoDeliverMultipleDocPackRadioButton, "AutoDeliverMultiDocPack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).AutoDeliverMultiDocPack)));
			this.AutoDeliverMultipleDocPackRadioButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|21676a9b-2d27-463e-bd63-0a493f45681d", "Auto-deliver multiple document packs");
			this.AutoDeliverMultipleDocPackRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutoDeliverMultipleDocPackRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 57, true);
			this.AutoDeliverMultipleDocPackRadioButton.Name = "AutoDeliverMultipleDocPackRadioButton";
			this.AutoDeliverMultipleDocPackRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 17, true);
			this.AutoDeliverMultipleDocPackRadioButton.TabIndex = 3;
			// 
			// NumMultiDocPacksLabel
			// 
			this.BindingSource.SetBindingMember(this.NumMultiDocPacksLabel, "DocumentPackCountAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).DocumentPackCountAsString)));
			this.NumMultiDocPacksLabel.ForeColor = System.Drawing.SystemColors.Control;
			this.NumMultiDocPacksLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 163, true);
			this.NumMultiDocPacksLabel.Name = "NumMultiDocPacksLabel";
			this.NumMultiDocPacksLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			this.NumMultiDocPacksLabel.TabIndex = 1;
			this.NumMultiDocPacksLabel.Text = "999";
			this.NumMultiDocPacksLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.NumMultiDocPacksLabel.TextChanged += new System.EventHandler(this.NumMultiDocPacksLabel_TextChanged);
			// 
			// MultiDocPacksLabel
			// 
			this.MultiDocPacksLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 26, true);
			this.MultiDocPacksLabel.Name = "MultiDocPacksLabel";
			this.MultiDocPacksLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 21, true);
			this.MultiDocPacksLabel.TabIndex = 0;
			// 
			// CoverNoteTextBox
			// 
			this.CoverNoteTextBox.AcceptsReturn = true;
			this.CoverNoteTextBox.AcceptsTab = true;
			this.CoverNoteTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CoverNoteTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CoverNoteTextBox, "CoverNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).CoverNote)));
			this.CoverNoteTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CoverNoteTextBox, false);
			this.CoverNoteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 30, true);
			this.CoverNoteTextBox.Multiline = true;
			this.CoverNoteTextBox.Name = "CoverNoteTextBox";
			this.CoverNoteTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.CoverNoteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 180, true);
			this.CoverNoteTextBox.TabIndex = 1;
			// 
			// IncludeCoverNoteCheckBox
			// 
			this.IncludeCoverNoteCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncludeCoverNoteCheckBox, "IncludeCoverNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).IncludeCoverNote)));
			this.IncludeCoverNoteCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|21cb4740-feff-4478-ae47-48bcb62d9636", "Include a Cover Note");
			this.IncludeCoverNoteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeCoverNoteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.IncludeCoverNoteCheckBox.Name = "IncludeCoverNoteCheckBox";
			this.IncludeCoverNoteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IncludeCoverNoteCheckBox.TabIndex = 0;
			this.IncludeCoverNoteCheckBox.UseVisualStyleBackColor = false;
			// 
			// IndividualDocPackGroupBox
			// 
			this.IndividualDocPackGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|f5c46ae1-5549-4af2-a113-49bb38af51d5", "Recipients");
			this.IndividualDocPackGroupBox.Controls.Add(this.RecipientsGrid);
			this.IndividualDocPackGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IndividualDocPackGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IndividualDocPackGroupBox.Name = "IndividualDocPackGroupBox";
			this.IndividualDocPackGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 216, true);
			this.IndividualDocPackGroupBox.TabIndex = 0;
			this.IndividualDocPackGroupBox.TabStop = false;
			// 
			// RecipientsGrid
			// 
			this.RecipientsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RecipientsGrid, "Recipients");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).Recipients)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).Recipients)).SyncRoot)).OrgHeaderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).Recipients)).SyncRoot)).CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).Recipients)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).Recipients)).SyncRoot)).StaffCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).Recipients)).SyncRoot)).DeliveryMethodDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).Recipients)).SyncRoot)).AttachmentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).Recipients)).SyncRoot)).SendIndividually)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).Recipients)).SyncRoot)).DeliveryAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).Recipients)).SyncRoot)).Salutation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.DocDeliveryContact)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).Recipients)).SyncRoot)).EmailFromAddressWithType)));
			this.RecipientsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo6.BindToList = "DeliveryRecipientTypes";
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|f678d12c-87e9-4ebd-b6ef-902232de3098", "Deliver To");
			zDropEditColumnStyleInfo6.ColumnName = "DeliveryRecipientType";
			zDropEditColumnStyleInfo6.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|03653c99-b456-4ec4-acff-9a168864fd16", "Organization");
			zGuidFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OrgHeaderPK";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|09fda265-dd05-4420-bab2-4ff3ff643205", "Company Name");
			zTextBoxColumnStyleInfo1.ColumnName = "CompanyName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|7b267572-6e46-4d34-9296-ab0332e998f5", "Name");
			zDropEditColumnStyleInfo1.ColumnName = "Name";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|B164716F-CE5F-492E-A645-88BD37019DF6", "Staff");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "StaffCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|ea6ec0fd-44ad-431d-9e16-7d41e015df6c", "Delivery Method");
			zDropEditColumnStyleInfo2.ColumnName = "DeliveryMethodDescription";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|dfc07fde-94f1-47d8-a396-5858fa76cf77", "Type", "Attachment Type", "");
			zDropEditColumnStyleInfo3.ColumnName = "AttachmentType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|940617fa-f5cf-46c7-a8ef-ffa10608b398", "Send Individually");
			zCheckBoxColumnStyleInfo3.ColumnName = "SendIndividually";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|533f7dfc-8e92-4378-9637-19f0db4c7c07", "Salutation");
			zDropEditColumnStyleInfo4.ColumnName = "Salutation";
			zDropEditColumnStyleInfo4.IsVisible = false;
			zDropEditColumnStyleInfo4.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|502B6B7E-EBE4-42DA-A28A-C7BB73486DCF", "Send From");
			zDropEditColumnStyleInfo5.ColumnName = "EmailFromAddressWithType";
			zDropEditColumnStyleInfo5.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo5.IsVisible = true;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);

			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.RecipientsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.RecipientsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.RecipientsGrid.CopySelectedRowsAllowed = true;
			this.RecipientsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecipientsGrid.GridId = "57556a2e-362f-442a-ad07-1194bc022a29";
			this.RecipientsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RecipientsGrid.LayoutKey = "zGrid1";
			this.RecipientsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.RecipientsGrid.Name = "RecipientsGrid";
			this.RecipientsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 197, true);
			this.RecipientsGrid.TabIndex = 0;
			// 
			// DocumentsGrid
			// 
			this.DocumentsGrid.AllowNavigation = false;
			this.DocumentsGrid.DisableImportDataMenuItem = true;
			this.BindingSource.SetBindingMember(this.DocumentsGrid, "DocumentsToBeDelivered");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).DocumentsToBeDelivered)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.IDeliverable)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).DocumentsToBeDelivered)).SyncRoot)).IncludedInPrint)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.IDeliverable)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).DocumentsToBeDelivered)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.IDeliverable)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).DocumentsToBeDelivered)).SyncRoot)).AllAvailableDeliveryModes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentEngine.IDeliverable)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).DocumentsToBeDelivered)).SyncRoot)).PrinterDetails.PrintQueuePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.DocumentEngine.IDeliverable)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).DocumentsToBeDelivered)).SyncRoot)).PrinterDetails.NumberOfCopies)));
			this.DocumentsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|ae43037f-6c2d-4b91-bb34-fcd4f7ee27ca", "Include");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludedInPrint";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|6e368631-ec7c-48ca-a64e-f08b9bb1476f", "Document Name");
			zTextBoxColumnStyleInfo3.ColumnName = "Name";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|76d3de58-05fd-47be-82a9-a9da6eb74a68", "Delivery Mode");
			zTextBoxColumnStyleInfo4.ColumnName = "AllAvailableDeliveryModes";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|cd5e09ca-e080-40eb-8852-56bd1a69c544", "Doc Type");
			zTextBoxColumnStyleInfo5.ColumnName = "DocumentTypeCode";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|c4ec1934-aa74-4992-9bef-0c25ce01a268", "Doc Type Description");
			zTextBoxColumnStyleInfo6.ColumnName = "DocumentTypeDescription";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|B8634B36-ECAA-4FE1-A195-20B4D428D7DA", "Printer Override");
			zGuidDropEditColumnStyleInfo1.ColumnName = "PrinterDetails.PrintQueuePK";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|36CCE5AC-4B72-462E-8332-57AC6B96C995", "Copies");
			zCalcEditColumnStyleInfo1.ColumnName = "PrinterDetails.NumberOfCopies";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DocumentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DocumentsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.DocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DocumentsGrid.CopySelectedRowsAllowed = true;
			this.DocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentsGrid.GridId = "ca561170-a84d-46ef-9ccb-b5b70157d55c";
			this.DocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocumentsGrid.LayoutKey = "DocumentsGrid";
			this.DocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DocumentsGrid.Name = "DocumentsGrid";
			this.DocumentsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.DocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 197, true);
			this.DocumentsGrid.TabIndex = 1;
			// 
			// IncludedEDocsGrid
			// 
			this.IncludedEDocsGrid.AllowNavigation = false;
			this.IncludedEDocsGrid.DisableImportDataMenuItem = true;
			this.BindingSource.SetBindingMember(this.IncludedEDocsGrid, "EDocsToBeDelivered");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).EDocsToBeDelivered)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.IDeliverable)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).EDocsToBeDelivered)).SyncRoot)).IncludedInPrint)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.IDeliverable)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).EDocsToBeDelivered)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.IDeliverable)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).EDocsToBeDelivered)).SyncRoot)).AllAvailableDeliveryModes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZByte)(((Enterprise.DocumentEngine.IDeliverable)(((System.Collections.IList)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).EDocsToBeDelivered)).SyncRoot)).Index)));
			this.IncludedEDocsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|f894331a-120a-4481-be49-b18e92a5f70f", "Include");
			zCheckBoxColumnStyleInfo2.ColumnName = "IncludedInPrint";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|e0226b40-64ec-4fe9-8bfe-a92757e85e51", "Document Name");
			zTextBoxColumnStyleInfo7.ColumnName = "NameForBinding";
			zTextBoxColumnStyleInfo7.IsMandatory = true;
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|d90e64bd-3d4a-49b9-8c75-bddfa1063e77", "Delivery Mode");
			zTextBoxColumnStyleInfo8.ColumnName = "AllAvailableDeliveryModes";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|e316d07e-f226-4c14-a6dd-ecc6c0874a16", "Doc Type");
			zTextBoxColumnStyleInfo9.ColumnName = "DocumentTypeCode";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|654eeb0c-a3d6-421e-8af4-bd71efae3efd", "Doc Type Description");
			zTextBoxColumnStyleInfo10.ColumnName = "DocumentTypeDescription";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			ZCalcEditColumnStyleInfo.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|2D4F04BE-1E5F-4027-B98E-9050FE964F81", "Index");
			ZCalcEditColumnStyleInfo.ColumnName = "Index";
			ZCalcEditColumnStyleInfo.IsReadOnly = false;
			ZCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			ZCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.IncludedEDocsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.IncludedEDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.IncludedEDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.IncludedEDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.IncludedEDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.IncludedEDocsGrid.ColumnStyles.Add(ZCalcEditColumnStyleInfo);
			this.IncludedEDocsGrid.CopySelectedRowsAllowed = true;
			this.IncludedEDocsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IncludedEDocsGrid.GridId = "e4024c26-10aa-4d1c-8b2c-40ee945767c7";
			this.IncludedEDocsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.IncludedEDocsGrid.LayoutKey = "IncludedEDocsGrid";
			this.IncludedEDocsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.IncludedEDocsGrid.Name = "IncludedEDocsGrid";
			this.IncludedEDocsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.IncludedEDocsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 197, true);
			this.IncludedEDocsGrid.TabIndex = 2;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.MainPage);
			this.MainTabControl.Controls.Add(this.DocumentsTabPage);
			this.MainTabControl.Controls.Add(this.IncludedEDocsTabPage);
			this.MainTabControl.Controls.Add(this.CoverNotePage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 243, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainPage
			// 
			this.MainPage.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|edffa17c-0e84-4f35-ba95-80b0551cf125", "Destination");
			this.MainPage.Controls.Add(this.MultiDocPackGroupbox);
			this.MainPage.Controls.Add(this.IndividualDocPackGroupBox);
			this.MainPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainPage.Name = "MainPage";
			this.MainPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 216, true);
			this.MainPage.TabIndex = 0;
			// 
			// DocumentsTabPage
			// 
			this.DocumentsTabPage.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|6bac67a3-5005-4e92-9ec1-e9b80cec57dd", "Documents to Send");
			this.DocumentsTabPage.Controls.Add(this.DocumentsGroupBox);
			this.DocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DocumentsTabPage.Name = "DocumentsTabPage";
			this.DocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 216, true);
			this.DocumentsTabPage.TabIndex = 1;
			// 
			// DocumentsGroupBox
			// 
			this.DocumentsGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|77ab042c-409a-45a8-882f-b8d2e5db01a1", "Documents");
			this.DocumentsGroupBox.Controls.Add(this.DocumentsGrid);
			this.DocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentsGroupBox.Name = "DocumentsGroupBox";
			this.DocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 216, true);
			this.DocumentsGroupBox.TabIndex = 2;
			this.DocumentsGroupBox.TabStop = false;
			// 
			// IncludedEDocsTabPage
			// 
			this.IncludedEDocsTabPage.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|4728b240-ff0b-4f51-a95c-4101265f05cd", "Included eDocs");
			this.IncludedEDocsTabPage.Controls.Add(this.IncludedEDocsGroupBox);
			this.IncludedEDocsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.IncludedEDocsTabPage.Name = "IncludedEDocsTabPage";
			this.IncludedEDocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 216, true);
			this.IncludedEDocsTabPage.TabIndex = 2;
			// 
			// IncludedEDocsGroupBox
			// 
			this.IncludedEDocsGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|b75c5840-2bf0-4a58-92ed-273d1921d7a2", "eDocs");
			this.IncludedEDocsGroupBox.Controls.Add(this.IncludedEDocsGrid);
			this.IncludedEDocsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IncludedEDocsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IncludedEDocsGroupBox.Name = "IncludedEDocsGroupBox";
			this.IncludedEDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 216, true);
			this.IncludedEDocsGroupBox.TabIndex = 2;
			this.IncludedEDocsGroupBox.TabStop = false;
			// 
			// CoverNotePage
			// 
			this.CoverNotePage.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|0cd2b3eb-9073-47ac-9c67-fd018505e9ec", "Cover Note");
			this.CoverNotePage.Controls.Add(this.IncludeCoverNoteCheckBox);
			this.CoverNotePage.Controls.Add(this.CoverNoteTextBox);
			this.CoverNotePage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CoverNotePage.Name = "CoverNotePage";
			this.CoverNotePage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 216, true);
			this.CoverNotePage.TabIndex = 2;
			// 
			// NumCopiesCalcEdit
			// 
			this.NumCopiesCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.NumCopiesCalcEdit, "PrinterDelivery+NumberOfCopies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).PrinterDelivery.NumberOfCopies)));
			this.NumCopiesCalcEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|16235ae3-d902-40a2-8c6c-ad7a5c4434ab", "Copies");
			this.NumCopiesCalcEdit.DecimalPlaces = 0;
			this.NumCopiesCalcEdit.Decimals = 0;
			this.NumCopiesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 5, true);
			this.NumCopiesCalcEdit.Name = "NumCopiesCalcEdit";
			this.NumCopiesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.NumCopiesCalcEdit.TabIndex = 3;
			this.NumCopiesCalcEdit.Text = "0";
			this.NumCopiesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PrinterSelectionGuidDropEdit
			// 
			this.PrinterSelectionGuidDropEdit.AllowDrop = true;
			this.PrinterSelectionGuidDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrinterSelectionGuidDropEdit, "PrinterDelivery+PrintQueuePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).PrinterDelivery.PrintQueuePK)));
			this.PrinterSelectionGuidDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|0b8a34b7-7aec-47f0-abea-46fd0a04237b", "Printer");
			this.PrinterSelectionGuidDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PrinterSelectionGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(54, 4, true);
			this.PrinterSelectionGuidDropEdit.Name = "PrinterSelectionGuidDropEdit";
			this.PrinterSelectionGuidDropEdit.PreBoundMaxLength = 38;
			this.PrinterSelectionGuidDropEdit.ShowDescriptionBox = false;
			this.PrinterSelectionGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.PrinterSelectionGuidDropEdit.TabIndex = 1;
			// 
			// PrintAsDraftCheckbox
			// 
			this.PrintAsDraftCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintAsDraftCheckbox, "IsDraft");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).IsDraft)));
			this.PrintAsDraftCheckbox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|2bf68f36-53e5-4a07-b98e-ba4dcae55b73", "Draft");
			this.PrintAsDraftCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintAsDraftCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 7, true);
			this.PrintAsDraftCheckbox.Name = "PrintAsDraftCheckbox";
			this.PrintAsDraftCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 17, true);
			this.PrintAsDraftCheckbox.TabIndex = 4;
			// 
			// zPanel1
			// 
			this.bottomPanel.Controls.Add(this.pageRangesPanel);
			this.bottomPanel.Controls.Add(this.LanguageZDropEdit);
			this.bottomPanel.Controls.Add(this.zPanel2);
			this.bottomPanel.Controls.Add(this.ShowOnlyPrintersUserCanPrintToCheckBox);
			this.bottomPanel.Controls.Add(this.PrinterSelectionGuidDropEdit);
			this.bottomPanel.Controls.Add(this.PrintAsDraftCheckbox);
			this.bottomPanel.Controls.Add(this.NumCopiesCalcEdit);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 243, true);
			this.bottomPanel.Name = "zPanel1";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 76, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// zPanel3
			// 
			this.pageRangesPanel.Controls.Add(this.TotalPagesOfLabel);
			this.pageRangesPanel.Controls.Add(this.TotalPagesLabel);
			this.pageRangesPanel.Controls.Add(this.PageRangesSpecifiedCheckBox);
			this.pageRangesPanel.Controls.Add(this.PageRangesTextBox);
			this.pageRangesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 25, true);
			this.pageRangesPanel.Name = "zPanel3";
			this.pageRangesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 25, true);
			this.pageRangesPanel.TabIndex = 6;
			// 
			// TotalPageOfLabel
			// 
			this.TotalPagesOfLabel.AutoSize = true;
			this.TotalPagesOfLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("271B103D-1233-491D-8497-998DEB916E95", "of");
			this.TotalPagesOfLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TotalPagesOfLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 6, true);
			this.TotalPagesOfLabel.Name = "TotalPageOfLabel";
			this.TotalPagesOfLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 13, true);
			this.TotalPagesOfLabel.TabIndex = 15;
			// 
			// TotalPageLabel
			// 
			this.TotalPagesLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.TotalPagesLabel, "DataSourceRowCountIfPageRangesSpecifiable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).DataSourceRowCountIfPageRangesSpecifiable)));
			this.TotalPagesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TotalPagesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 6, true);
			this.TotalPagesLabel.Name = "TotalPageLabel";
			this.TotalPagesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.TotalPagesLabel.TabIndex = 16;
			// 
			// PageRangesSpecifiedCheckBox
			// 
			this.PageRangesSpecifiedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PageRangesSpecifiedCheckBox, "PageRangesSpecified");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).PageRangesSpecified)));
			this.PageRangesSpecifiedCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("9898621D-ACD3-4ABC-A961-974E80B7274A", "Specific Page Ranges:");
			this.PageRangesSpecifiedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PageRangesSpecifiedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 5, true);
			this.PageRangesSpecifiedCheckBox.Name = "PageRangesSpecifiedCheckBox";
			this.PageRangesSpecifiedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.PageRangesSpecifiedCheckBox.TabIndex = 13;
			this.PageRangesSpecifiedCheckBox.CheckedChanged += PageRangesSpecifiedCheckBox_CheckedChanged;
			// 
			// PageRangeTextBox
			// 
			this.PageRangesTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PageRangesTextBox, "SpecifiedPageRangesText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).SpecifiedPageRangesText)));
			this.PageRangesTextBox.CaptionResourceString = null;
			this.PageRangesTextBox.DecimalPlaces = 0;
			this.PageRangesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 3, true);
			this.PageRangesTextBox.Name = "PageRangeTextBox";
			this.PageRangesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 20, true);
			this.PageRangesTextBox.TabIndex = 14;
			this.PageRangesTextBox.Text = "0";
			this.PageRangesTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PageRangesTextBox.Enabled = this.PageRangesSpecifiedCheckBox.Checked;
			// 
			// LanguageZDropEdit
			// 
			this.LanguageZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LanguageZDropEdit, "Language");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).Language)));
			this.LanguageZDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|d378f966-434a-4177-b195-48f8e6dcdd94", "Print Language");
			this.LanguageZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 28, true);
			this.LanguageZDropEdit.Name = "LanguageZDropEdit";
			this.LanguageZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 20, true);
			this.LanguageZDropEdit.TabIndex = 6;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.BackgroundDeliveryCheckBox);
			this.zPanel2.Controls.Add(this.DeliverButton);
			this.zPanel2.Controls.Add(this.printerHelpControl1);
			this.zPanel2.Controls.Add(this.VisualiseButton);
			this.zPanel2.Controls.Add(this.PreviewButton);
			this.zPanel2.Controls.Add(this.saveAsButton);
			this.zPanel2.Controls.Add(this.CloseButton);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 54, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 22, true);
			this.zPanel2.TabIndex = 12;
			// 
			// ScheduleCheckBox
			// 
			this.BindingSource.SetBindingMember(this.BackgroundDeliveryCheckBox, "BackgroundDelivery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).BackgroundDelivery)));
			this.BackgroundDeliveryCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|32bc5ffd-cde6-4153-ad07-8f39897241c6", "Schedule");
			this.BackgroundDeliveryCheckBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.BackgroundDeliveryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BackgroundDeliveryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 0, true);
			this.BackgroundDeliveryCheckBox.Name = "BackgroundDeliveryCheckBox";
			this.BackgroundDeliveryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 22, true);
			this.BackgroundDeliveryCheckBox.TabIndex = 2;
			// 
			// printerHelpControl1
			// 
			this.printerHelpControl1.AllowDrop = true;
			this.printerHelpControl1.AutoSize = true;
			this.printerHelpControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.printerHelpControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 22, true);
			this.printerHelpControl1.Name = "printerHelpControl1";
			this.printerHelpControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 22, true);
			this.printerHelpControl1.TabIndex = 1;
			// 
			// VisualiseButton
			// 
			this.VisualiseButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|3216bccc-01d4-4ba1-8d09-aa4ddf067827", "&Modify");
			this.VisualiseButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.VisualiseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(436, 0, true);
			this.VisualiseButton.Name = "VisualiseButton";
			this.VisualiseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
			this.VisualiseButton.TabIndex = 4;
			this.VisualiseButton.Click += new System.EventHandler(this.VisualiseButton_Click);
			// 
			// saveAsButton
			// 
			this.saveAsButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|ef2b5744-3abe-4680-972e-efd308271550", "&Save As...");
			this.saveAsButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.saveAsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 0, true);
			this.saveAsButton.Name = "saveAsButton";
			this.saveAsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
			this.saveAsButton.TabIndex = 6;
			// 
			// ShowOnlyPrintersUserCanPrintToCheckBox
			// 
			this.ShowOnlyPrintersUserCanPrintToCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowOnlyPrintersUserCanPrintToCheckBox, "PrinterDelivery+ShowOnlyPrintersUserCanPrintTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.DeliveryInstructions)(null)).PrinterDelivery.ShowOnlyPrintersUserCanPrintTo)));
			this.ShowOnlyPrintersUserCanPrintToCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|dc5bd0eb-a7c7-4c61-9043-9cdfe52e1525", "Only show printers I can access");
			this.ShowOnlyPrintersUserCanPrintToCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowOnlyPrintersUserCanPrintToCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(471, 7, true);
			this.ShowOnlyPrintersUserCanPrintToCheckBox.Name = "ShowOnlyPrintersUserCanPrintToCheckBox";
			this.ShowOnlyPrintersUserCanPrintToCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 17, true);
			this.ShowOnlyPrintersUserCanPrintToCheckBox.TabIndex = 5;
			this.ShowOnlyPrintersUserCanPrintToCheckBox.CheckedChanged += ShowOnlyPrintersUserCanPrintToCheckBox_CheckedChanged;
			// 
			// DocDeliveryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DocDeliveryForm|e6957cb8-6956-40aa-80f4-091414e28d89", "Deliver Documents");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 319, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceAssemblyName = "Enterprise.DocumentEngine";
			this.DataSourceType = typeof(Enterprise.DocumentEngine.DeliveryInstructions);
			this.DataSourceTypeName = "Enterprise.DocumentEngine.DeliveryInstructions";
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 353, true);
			this.Name = "DocDeliveryForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.ShouldSerializeTabPageMethods = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MultiDocPackGroupbox.ResumeLayout(false);
			this.MultiDocPackGroupbox.PerformLayout();
			this.IndividualDocPackGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DocumentsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IncludedEDocsGrid)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainPage.ResumeLayout(false);
			this.DocumentsTabPage.ResumeLayout(false);
			this.DocumentsGroupBox.ResumeLayout(false);
			this.IncludedEDocsTabPage.ResumeLayout(false);
			this.IncludedEDocsGroupBox.ResumeLayout(false);
			this.CoverNotePage.ResumeLayout(false);
			this.CoverNotePage.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.pageRangesPanel.ResumeLayout(false);
			this.pageRangesPanel.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		Enterprise.ZArchitecture.ZLabel NumMultiDocPacksLabel;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox BackgroundDeliveryCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit LanguageZDropEdit;
		internal ZArchitecture.GUI.ZButton saveAsButton;
		private System.ComponentModel.IContainer components;
		internal Enterprise.ZArchitecture.ZLabel TotalPagesOfLabel;
		internal Enterprise.ZArchitecture.ZLabel TotalPagesLabel;
		internal ZArchitecture.ZTextBox PageRangesTextBox;
		internal ZArchitecture.GUI.ZCheckBox PageRangesSpecifiedCheckBox;
	}
}
