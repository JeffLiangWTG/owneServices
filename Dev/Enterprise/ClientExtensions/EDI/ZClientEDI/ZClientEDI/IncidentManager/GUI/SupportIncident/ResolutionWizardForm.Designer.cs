namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class ResolutionWizardForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.MainFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.TopFlagLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ShouldContentBeDevelopedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ShouldContentBeDevelopedFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.ContentBeDevelopYesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ContentBeDevelopedNotWorthRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ContentIrrelevantERequestRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.PleaseProvideLinkGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LinksToExistingContentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PleaseProvideReasonGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PleaseProvideReasonTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.OtherReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProvideReasonFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.ComplexEdgeCaseRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.HighlyClientSpecificRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.OtherRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.WhatChangesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RecommendToContentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HowCanWeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MakeContentEasierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PleaseSelectReasonGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ResolutionMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomerFacingResolutionMessageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ResolutionMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BottomFlagLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PopUpEConversationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PleaseChooseOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PleaseChooseOptionsFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.CompletelySolvedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.PartlySolvedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ContentNotBeFoundRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.MainTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainFlowLayoutPanel.SuspendLayout();
			this.ShouldContentBeDevelopedGroupBox.SuspendLayout();
			this.ShouldContentBeDevelopedFlowLayoutPanel.SuspendLayout();
			this.PleaseProvideLinkGroupBox.SuspendLayout();
			this.PleaseProvideReasonGroupBox.SuspendLayout();
			this.PleaseProvideReasonTableLayoutPanel.SuspendLayout();
			this.ProvideReasonFlowLayoutPanel.SuspendLayout();
			this.WhatChangesGroupBox.SuspendLayout();
			this.HowCanWeGroupBox.SuspendLayout();
			this.PleaseSelectReasonGroupBox.SuspendLayout();
			this.ResolutionMethodDropEdit.SuspendLayout();
			this.CustomerFacingResolutionMessageGroupBox.SuspendLayout();
			this.PleaseChooseOptionsGroupBox.SuspendLayout();
			this.PleaseChooseOptionsFlowLayoutPanel.SuspendLayout();
			this.MainTableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MessageLabel
			// 
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 28, true);
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(626, 775, true);
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 23, true);
			this.CloseButton.Text = "Save && Close";
			this.CloseButton.TabIndex = 10;
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(731, 775, true);
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 23, true);
			this.CancelButtonX.TabIndex = 11;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 802, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction);
			// 
			// MainFlowLayoutPanel
			// 
			this.MainFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MainFlowLayoutPanel.Controls.Add(this.TopFlagLabel);
			this.MainFlowLayoutPanel.Controls.Add(this.ShouldContentBeDevelopedGroupBox);
			this.MainFlowLayoutPanel.Controls.Add(this.PleaseProvideLinkGroupBox);
			this.MainFlowLayoutPanel.Controls.Add(this.PleaseProvideReasonGroupBox);
			this.MainFlowLayoutPanel.Controls.Add(this.WhatChangesGroupBox);
			this.MainFlowLayoutPanel.Controls.Add(this.HowCanWeGroupBox);
			this.MainFlowLayoutPanel.Controls.Add(this.PleaseSelectReasonGroupBox);
			this.MainFlowLayoutPanel.Controls.Add(this.CustomerFacingResolutionMessageGroupBox);
			this.MainFlowLayoutPanel.Controls.Add(this.BottomFlagLabel);
			this.MainFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.MainFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 99, true);
			this.MainFlowLayoutPanel.Name = "MainFlowLayoutPanel";
			this.MainFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 701, true);
			this.MainFlowLayoutPanel.TabIndex = 18;
			this.MainFlowLayoutPanel.WrapContents = false;
			this.MainFlowLayoutPanel.SizeChanged += new System.EventHandler(this.MainFlowLayoutPanel_SizeChanged);
			// 
			// TopFlagLabel
			// 
			this.TopFlagLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TopFlagLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.TopFlagLabel.Name = "TopFlagLabel";
			this.TopFlagLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 1, true);
			this.TopFlagLabel.TabIndex = 0;
			this.TopFlagLabel.UseMnemonic = false;
			// 
			// ShouldContentBeDevelopedGroupBox
			// 
			this.ShouldContentBeDevelopedGroupBox.Controls.Add(this.ShouldContentBeDevelopedFlowLayoutPanel);
			this.ShouldContentBeDevelopedGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShouldContentBeDevelopedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 3, true);
			this.ShouldContentBeDevelopedGroupBox.Name = "ShouldContentBeDevelopedGroupBox";
			this.ShouldContentBeDevelopedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 93, true);
			this.ShouldContentBeDevelopedGroupBox.TabIndex = 1;
			this.ShouldContentBeDevelopedGroupBox.TabStop = false;
			this.ShouldContentBeDevelopedGroupBox.Text = "Should content be developed, or its discoverability enhanced?";
			this.ShouldContentBeDevelopedGroupBox.Visible = false;
			// 
			// ShouldContentBeDevelopedFlowLayoutPanel
			// 
			this.ShouldContentBeDevelopedFlowLayoutPanel.Controls.Add(this.ContentBeDevelopYesRadioButton);
			this.ShouldContentBeDevelopedFlowLayoutPanel.Controls.Add(this.ContentBeDevelopedNotWorthRadioButton);
			this.ShouldContentBeDevelopedFlowLayoutPanel.Controls.Add(this.ContentIrrelevantERequestRadioButton);
			this.ShouldContentBeDevelopedFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShouldContentBeDevelopedFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.ShouldContentBeDevelopedFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ShouldContentBeDevelopedFlowLayoutPanel.Name = "ShouldContentBeDevelopedFlowLayoutPanel";
			this.ShouldContentBeDevelopedFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 76, true);
			this.ShouldContentBeDevelopedFlowLayoutPanel.TabIndex = 5;
			this.ShouldContentBeDevelopedFlowLayoutPanel.WrapContents = false;
			// 
			// ContentBeDevelopYesRadioButton
			// 
			this.ContentBeDevelopYesRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.ContentBeDevelopYesRadioButton, "ContentBeDevelopYesOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).ContentBeDevelopYesOption)));
			this.ContentBeDevelopYesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 2, true);
			this.ContentBeDevelopYesRadioButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, 2, 2, 2, true);
			this.ContentBeDevelopYesRadioButton.Name = "ContentBeDevelopYesRadioButton";
			this.ContentBeDevelopYesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 19, true);
			this.ContentBeDevelopYesRadioButton.TabIndex = 4;
			this.ContentBeDevelopYesRadioButton.TabStop = true;
			this.ContentBeDevelopYesRadioButton.Text = "Yes";
			this.ContentBeDevelopYesRadioButton.UseVisualStyleBackColor = true;
			this.ContentBeDevelopYesRadioButton.Click += new System.EventHandler(this.ContentBeDevelopYesRadioButton_Click);
			// 
			// ContentBeDevelopedNotWorthRadioButton
			// 
			this.ContentBeDevelopedNotWorthRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.ContentBeDevelopedNotWorthRadioButton, "ContentBeDevelopNotWorthOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).ContentBeDevelopNotWorthOption)));
			this.ContentBeDevelopedNotWorthRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 25, true);
			this.ContentBeDevelopedNotWorthRadioButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, 2, 2, 2, true);
			this.ContentBeDevelopedNotWorthRadioButton.Name = "ContentBeDevelopedNotWorthRadioButton";
			this.ContentBeDevelopedNotWorthRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 19, true);
			this.ContentBeDevelopedNotWorthRadioButton.TabIndex = 5;
			this.ContentBeDevelopedNotWorthRadioButton.TabStop = true;
			this.ContentBeDevelopedNotWorthRadioButton.Text = "Probably not worth it";
			this.ContentBeDevelopedNotWorthRadioButton.UseVisualStyleBackColor = true;
			this.ContentBeDevelopedNotWorthRadioButton.Click += new System.EventHandler(this.ContentBeDevelopedNotWorthRadioButton_Click);
			// 
			// ContentIrrelevantERequestRadioButton
			// 
			this.ContentIrrelevantERequestRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.ContentIrrelevantERequestRadioButton, "ContentIsIrrelevantOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).ContentIsIrrelevantOption)));
			this.ContentIrrelevantERequestRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 47, true);
			this.ContentIrrelevantERequestRadioButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, 2, 2, 2, true);
			this.ContentIrrelevantERequestRadioButton.Name = "ContentIrrelevantERequestRadioButton";
			this.ContentIrrelevantERequestRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 19, true);
			this.ContentIrrelevantERequestRadioButton.TabIndex = 6;
			this.ContentIrrelevantERequestRadioButton.TabStop = true;
			this.ContentIrrelevantERequestRadioButton.Text = "Content is irrelevant to this eRequest";
			this.ContentIrrelevantERequestRadioButton.UseVisualStyleBackColor = true;
			this.ContentIrrelevantERequestRadioButton.Click += new System.EventHandler(this.ContentIrrelevantERequestRadioButton_Click);
			// 
			// PleaseProvideLinkGroupBox
			// 
			this.PleaseProvideLinkGroupBox.Controls.Add(this.LinksToExistingContentTextBox);
			this.PleaseProvideLinkGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PleaseProvideLinkGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 99, true);
			this.PleaseProvideLinkGroupBox.Name = "PleaseProvideLinkGroupBox";
			this.PleaseProvideLinkGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 79, true);
			this.PleaseProvideLinkGroupBox.TabIndex = 2;
			this.PleaseProvideLinkGroupBox.TabStop = false;
			this.PleaseProvideLinkGroupBox.Text = "Please provide link(s) to the existing content";
			this.PleaseProvideLinkGroupBox.Visible = false;
			// 
			// LinksToExistingContentTextBox
			// 
			this.LinksToExistingContentTextBox.AcceptsReturn = true;
			this.LinksToExistingContentTextBox.AcceptsTab = true;
			this.BindingSource.SetBindingMember(this.LinksToExistingContentTextBox, "LinksToExistingContent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).LinksToExistingContent)));
			this.LinksToExistingContentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LinksToExistingContentTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinksToExistingContentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.LinksToExistingContentTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, true);
			this.LinksToExistingContentTextBox.Multiline = true;
			this.LinksToExistingContentTextBox.Name = "LinksToExistingContentTextBox";
			this.LinksToExistingContentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.LinksToExistingContentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 62, true);
			this.LinksToExistingContentTextBox.TabIndex = 1;
			// 
			// PleaseProvideReasonGroupBox
			// 
			this.PleaseProvideReasonGroupBox.Controls.Add(this.PleaseProvideReasonTableLayoutPanel);
			this.PleaseProvideReasonGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PleaseProvideReasonGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 182, true);
			this.PleaseProvideReasonGroupBox.Name = "PleaseProvideReasonGroupBox";
			this.PleaseProvideReasonGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 133, true);
			this.PleaseProvideReasonGroupBox.TabIndex = 3;
			this.PleaseProvideReasonGroupBox.TabStop = false;
			this.PleaseProvideReasonGroupBox.Text = "Please provide a reason";
			this.PleaseProvideReasonGroupBox.Visible = false;
			// 
			// PleaseProvideReasonTableLayoutPanel
			// 
			this.PleaseProvideReasonTableLayoutPanel.ColumnCount = 1;
			this.PleaseProvideReasonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.PleaseProvideReasonTableLayoutPanel.Controls.Add(this.OtherReasonTextBox, 0, 1);
			this.PleaseProvideReasonTableLayoutPanel.Controls.Add(this.ProvideReasonFlowLayoutPanel, 0, 0);
			this.PleaseProvideReasonTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PleaseProvideReasonTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.PleaseProvideReasonTableLayoutPanel.Name = "PleaseProvideReasonTableLayoutPanel";
			this.PleaseProvideReasonTableLayoutPanel.RowCount = 2;
			this.PleaseProvideReasonTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.PleaseProvideReasonTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.PleaseProvideReasonTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 116, true);
			this.PleaseProvideReasonTableLayoutPanel.TabIndex = 8;
			// 
			// OtherReasonTextBox
			// 
			this.OtherReasonTextBox.AcceptsReturn = true;
			this.OtherReasonTextBox.AcceptsTab = true;
			this.OtherReasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OtherReasonTextBox, "ReasonText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).ReasonText)));
			this.OtherReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OtherReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 73, true);
			this.OtherReasonTextBox.Multiline = true;
			this.OtherReasonTextBox.Name = "OtherReasonTextBox";
			this.OtherReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OtherReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 41, true);
			this.OtherReasonTextBox.TabIndex = 4;
			// 
			// ProvideReasonFlowLayoutPanel
			// 
			this.ProvideReasonFlowLayoutPanel.Controls.Add(this.ComplexEdgeCaseRadioButton);
			this.ProvideReasonFlowLayoutPanel.Controls.Add(this.HighlyClientSpecificRadioButton);
			this.ProvideReasonFlowLayoutPanel.Controls.Add(this.OtherRadioButton);
			this.ProvideReasonFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProvideReasonFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.ProvideReasonFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ProvideReasonFlowLayoutPanel.Name = "ProvideReasonFlowLayoutPanel";
			this.ProvideReasonFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 67, true);
			this.ProvideReasonFlowLayoutPanel.TabIndex = 9;
			this.ProvideReasonFlowLayoutPanel.WrapContents = false;
			// 
			// ComplexEdgeCaseRadioButton
			// 
			this.ComplexEdgeCaseRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.ComplexEdgeCaseRadioButton, "ComplexEdgeCaseOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).ComplexEdgeCaseOption)));
			this.ComplexEdgeCaseRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 2, true);
			this.ComplexEdgeCaseRadioButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, 2, 2, 2, true);
			this.ComplexEdgeCaseRadioButton.Name = "ComplexEdgeCaseRadioButton";
			this.ComplexEdgeCaseRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 19, true);
			this.ComplexEdgeCaseRadioButton.TabIndex = 6;
			this.ComplexEdgeCaseRadioButton.TabStop = true;
			this.ComplexEdgeCaseRadioButton.Text = "Complex edge case";
			this.ComplexEdgeCaseRadioButton.UseVisualStyleBackColor = true;
			// 
			// HighlyClientSpecificRadioButton
			// 
			this.HighlyClientSpecificRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.HighlyClientSpecificRadioButton, "HighlyClientSpecificOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).HighlyClientSpecificOption)));
			this.HighlyClientSpecificRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 25, true);
			this.HighlyClientSpecificRadioButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, 2, 2, 2, true);
			this.HighlyClientSpecificRadioButton.Name = "HighlyClientSpecificRadioButton";
			this.HighlyClientSpecificRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 19, true);
			this.HighlyClientSpecificRadioButton.TabIndex = 5;
			this.HighlyClientSpecificRadioButton.TabStop = true;
			this.HighlyClientSpecificRadioButton.Text = "Highly client specific / not broadly useful";
			this.HighlyClientSpecificRadioButton.UseVisualStyleBackColor = true;
			// 
			// OtherRadioButton
			// 
			this.OtherRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.OtherRadioButton, "OtherOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).OtherOption)));
			this.OtherRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 47, true);
			this.OtherRadioButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, 2, 2, 2, true);
			this.OtherRadioButton.Name = "OtherRadioButton";
			this.OtherRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 19, true);
			this.OtherRadioButton.TabIndex = 7;
			this.OtherRadioButton.TabStop = true;
			this.OtherRadioButton.Text = "Other";
			this.OtherRadioButton.UseVisualStyleBackColor = true;
			// 
			// WhatChangesGroupBox
			// 
			this.WhatChangesGroupBox.Controls.Add(this.RecommendToContentTextBox);
			this.WhatChangesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WhatChangesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 319, true);
			this.WhatChangesGroupBox.Name = "WhatChangesGroupBox";
			this.WhatChangesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 82, true);
			this.WhatChangesGroupBox.TabIndex = 4;
			this.WhatChangesGroupBox.TabStop = false;
			this.WhatChangesGroupBox.Text = "What changes do you recommend to content? Include a summary of the answer to this" +
    " training question";
			this.WhatChangesGroupBox.Visible = false;
			// 
			// RecommendToContentTextBox
			// 
			this.RecommendToContentTextBox.AcceptsReturn = true;
			this.RecommendToContentTextBox.AcceptsTab = true;
			this.BindingSource.SetBindingMember(this.RecommendToContentTextBox, "RecommendToContentText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).RecommendToContentText)));
			this.RecommendToContentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RecommendToContentTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecommendToContentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.RecommendToContentTextBox.Multiline = true;
			this.RecommendToContentTextBox.Name = "RecommendToContentTextBox";
			this.RecommendToContentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.RecommendToContentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 65, true);
			this.RecommendToContentTextBox.TabIndex = 1;
			// 
			// HowCanWeGroupBox
			// 
			this.HowCanWeGroupBox.Controls.Add(this.MakeContentEasierTextBox);
			this.HowCanWeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HowCanWeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 405, true);
			this.HowCanWeGroupBox.Name = "HowCanWeGroupBox";
			this.HowCanWeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 84, true);
			this.HowCanWeGroupBox.TabIndex = 5;
			this.HowCanWeGroupBox.TabStop = false;
			this.HowCanWeGroupBox.Text = "How can we make this content easier for the next customer to find? Include, possi" +
    "ble keywords, filters, etc";
			this.HowCanWeGroupBox.Visible = false;
			// 
			// MakeContentEasierTextBox
			// 
			this.MakeContentEasierTextBox.AcceptsReturn = true;
			this.MakeContentEasierTextBox.AcceptsTab = true;
			this.BindingSource.SetBindingMember(this.MakeContentEasierTextBox, "ContentEasierText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).ContentEasierText)));
			this.MakeContentEasierTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MakeContentEasierTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MakeContentEasierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.MakeContentEasierTextBox.Multiline = true;
			this.MakeContentEasierTextBox.Name = "MakeContentEasierTextBox";
			this.MakeContentEasierTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MakeContentEasierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 67, true);
			this.MakeContentEasierTextBox.TabIndex = 1;
			// 
			// PleaseSelectReasonGroupBox
			// 
			this.PleaseSelectReasonGroupBox.Controls.Add(this.ResolutionMethodDropEdit);
			this.PleaseSelectReasonGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PleaseSelectReasonGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 493, true);
			this.PleaseSelectReasonGroupBox.Name = "PleaseSelectReasonGroupBox";
			this.PleaseSelectReasonGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 55, true);
			this.PleaseSelectReasonGroupBox.TabIndex = 6;
			this.PleaseSelectReasonGroupBox.TabStop = false;
			this.PleaseSelectReasonGroupBox.Text = "Please select a reason";
			this.PleaseSelectReasonGroupBox.Visible = false;
			// 
			// ResolutionMethodDropEdit
			// 
			this.ResolutionMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ResolutionMethodDropEdit, "ResolutionMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).ResolutionMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).ActiveCloseStatusDispositionList)));
			this.ResolutionMethodDropEdit.BindToList = "ActiveCloseStatusDispositionList";
			this.ResolutionMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 25, true);
			this.ResolutionMethodDropEdit.Name = "ResolutionMethodDropEdit";
			this.ResolutionMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 17, true);
			this.ResolutionMethodDropEdit.TabIndex = 1;
			// 
			// CustomerFacingResolutionMessageGroupBox
			// 
			this.CustomerFacingResolutionMessageGroupBox.Controls.Add(this.ResolutionMessageTextBox);
			this.CustomerFacingResolutionMessageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomerFacingResolutionMessageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 551, true);
			this.CustomerFacingResolutionMessageGroupBox.Name = "CustomerFacingResolutionMessageGroupBox";
			this.CustomerFacingResolutionMessageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 112, true);
			this.CustomerFacingResolutionMessageGroupBox.TabIndex = 7;
			this.CustomerFacingResolutionMessageGroupBox.TabStop = false;
			this.CustomerFacingResolutionMessageGroupBox.Text = "Customer facing resolution message (optional):";
			this.CustomerFacingResolutionMessageGroupBox.Visible = false;
			// 
			// ResolutionMessageTextBox
			// 
			this.ResolutionMessageTextBox.AcceptsReturn = true;
			this.ResolutionMessageTextBox.AcceptsTab = true;
			this.BindingSource.SetBindingMember(this.ResolutionMessageTextBox, "Comment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).Comment)));
			this.ResolutionMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ResolutionMessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResolutionMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ResolutionMessageTextBox.Multiline = true;
			this.ResolutionMessageTextBox.Name = "ResolutionMessageTextBox";
			this.ResolutionMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ResolutionMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 95, true);
			this.ResolutionMessageTextBox.TabIndex = 1;
			this.ResolutionMessageTextBox.TextChanged += new System.EventHandler(this.ResolutionMessageTextBox_TextChanged);
			// 
			// BottomFlagLabel
			// 
			this.BottomFlagLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BottomFlagLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.BottomFlagLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 665, true);
			this.BottomFlagLabel.Name = "BottomFlagLabel";
			this.BottomFlagLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 1, true);
			this.BottomFlagLabel.TabIndex = 8;
			this.BottomFlagLabel.UseMnemonic = false;
			// 
			// PopUpEConversationButton
			// 
			this.PopUpEConversationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PopUpEConversationButton.IsCaptionOverridden = true;
			this.PopUpEConversationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(691, 14, true);
			this.PopUpEConversationButton.Name = "PopUpEConversationButton";
			this.PopUpEConversationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23, true);
			this.PopUpEConversationButton.TabIndex = 9;
			this.PopUpEConversationButton.Text = "Pop-up eConversation";
			this.PopUpEConversationButton.ToolTipCaption = null;
			this.PopUpEConversationButton.UseVisualStyleBackColor = true;
			this.PopUpEConversationButton.Click += new System.EventHandler(this.PopUpEConversationButton_Click);
			// 
			// PleaseChooseOptionsGroupBox
			// 
			this.PleaseChooseOptionsGroupBox.Controls.Add(this.PleaseChooseOptionsFlowLayoutPanel);
			this.PleaseChooseOptionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PleaseChooseOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.PleaseChooseOptionsGroupBox.Name = "PleaseChooseOptionsGroupBox";
			this.PleaseChooseOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 93, true);
			this.PleaseChooseOptionsGroupBox.TabIndex = 0;
			this.PleaseChooseOptionsGroupBox.TabStop = false;
			this.PleaseChooseOptionsGroupBox.Text = "Please choose from the following options:";
			// 
			// PleaseChooseOptionsFlowLayoutPanel
			// 
			this.PleaseChooseOptionsFlowLayoutPanel.Controls.Add(this.CompletelySolvedRadioButton);
			this.PleaseChooseOptionsFlowLayoutPanel.Controls.Add(this.PartlySolvedRadioButton);
			this.PleaseChooseOptionsFlowLayoutPanel.Controls.Add(this.ContentNotBeFoundRadioButton);
			this.PleaseChooseOptionsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PleaseChooseOptionsFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.PleaseChooseOptionsFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.PleaseChooseOptionsFlowLayoutPanel.Name = "PleaseChooseOptionsFlowLayoutPanel";
			this.PleaseChooseOptionsFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 76, true);
			this.PleaseChooseOptionsFlowLayoutPanel.TabIndex = 4;
			this.PleaseChooseOptionsFlowLayoutPanel.WrapContents = false;
			// 
			// CompletelySolvedRadioButton
			// 
			this.CompletelySolvedRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.CompletelySolvedRadioButton, "CompletelySolvedOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).CompletelySolvedOption)));
			this.CompletelySolvedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 2, true);
			this.CompletelySolvedRadioButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, 2, 2, 2, true);
			this.CompletelySolvedRadioButton.Name = "CompletelySolvedRadioButton";
			this.CompletelySolvedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 19, true);
			this.CompletelySolvedRadioButton.TabIndex = 1;
			this.CompletelySolvedRadioButton.TabStop = true;
			this.CompletelySolvedRadioButton.Text = "Available content COMPLETELY solved the client query";
			this.CompletelySolvedRadioButton.UseVisualStyleBackColor = true;
			this.CompletelySolvedRadioButton.Click += new System.EventHandler(this.CompletelySolvedRadioButton_Click);
			// 
			// PartlySolvedRadioButton
			// 
			this.PartlySolvedRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.PartlySolvedRadioButton, "PartlySolvedOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).PartlySolvedOption)));
			this.PartlySolvedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 25, true);
			this.PartlySolvedRadioButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, 2, 2, 2, true);
			this.PartlySolvedRadioButton.Name = "PartlySolvedRadioButton";
			this.PartlySolvedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 19, true);
			this.PartlySolvedRadioButton.TabIndex = 2;
			this.PartlySolvedRadioButton.TabStop = true;
			this.PartlySolvedRadioButton.Text = "Available content PARTLY solved the client query";
			this.PartlySolvedRadioButton.UseVisualStyleBackColor = true;
			this.PartlySolvedRadioButton.Click += new System.EventHandler(this.PartlySolvedRadioButton_Click);
			// 
			// ContentNotBeFoundRadioButton
			// 
			this.ContentNotBeFoundRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.ContentNotBeFoundRadioButton, "ContentCouldNotBeFoundOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction)(null)).ContentCouldNotBeFoundOption)));
			this.ContentNotBeFoundRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 47, true);
			this.ContentNotBeFoundRadioButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, 2, 2, 2, true);
			this.ContentNotBeFoundRadioButton.Name = "ContentNotBeFoundRadioButton";
			this.ContentNotBeFoundRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 19, true);
			this.ContentNotBeFoundRadioButton.TabIndex = 3;
			this.ContentNotBeFoundRadioButton.TabStop = true;
			this.ContentNotBeFoundRadioButton.Text = "Content could not be found";
			this.ContentNotBeFoundRadioButton.UseVisualStyleBackColor = true;
			this.ContentNotBeFoundRadioButton.Click += new System.EventHandler(this.ContentNotBeFoundRadioButton_Click);
			// 
			// MainTableLayoutPanel
			// 
			this.MainTableLayoutPanel.ColumnCount = 1;
			this.MainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTableLayoutPanel.Controls.Add(this.PleaseChooseOptionsGroupBox, 0, 0);
			this.MainTableLayoutPanel.Controls.Add(this.MainFlowLayoutPanel, 0, 1);
			this.MainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTableLayoutPanel.Name = "MainTableLayoutPanel";
			this.MainTableLayoutPanel.RowCount = 2;
			this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 802, true);
			this.MainTableLayoutPanel.TabIndex = 19;
			// 
			// ResolutionWizardForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 826, true);
			this.Controls.Add(this.PopUpEConversationButton);
			this.Controls.Add(this.MainTableLayoutPanel);
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentResolutionWizardAction);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(InitialMinimumFormWidth, InitialMinimumFormHeight, true);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(InitialMaximumFormWidth, InitialMaximumFormHeight, true);
			this.Name = "ResolutionWizardForm";
			this.Text = "CR5 Resolution Assistant";
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainTableLayoutPanel, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.PopUpEConversationButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainFlowLayoutPanel.ResumeLayout(false);
			this.MainFlowLayoutPanel.PerformLayout();
			this.ShouldContentBeDevelopedGroupBox.ResumeLayout(false);
			this.ShouldContentBeDevelopedGroupBox.PerformLayout();
			this.ShouldContentBeDevelopedFlowLayoutPanel.ResumeLayout(false);
			this.ShouldContentBeDevelopedFlowLayoutPanel.PerformLayout();
			this.PleaseProvideLinkGroupBox.ResumeLayout(false);
			this.PleaseProvideLinkGroupBox.PerformLayout();
			this.PleaseProvideReasonGroupBox.ResumeLayout(false);
			this.PleaseProvideReasonGroupBox.PerformLayout();
			this.PleaseProvideReasonTableLayoutPanel.ResumeLayout(false);
			this.PleaseProvideReasonTableLayoutPanel.PerformLayout();
			this.ProvideReasonFlowLayoutPanel.ResumeLayout(false);
			this.ProvideReasonFlowLayoutPanel.PerformLayout();
			this.WhatChangesGroupBox.ResumeLayout(false);
			this.WhatChangesGroupBox.PerformLayout();
			this.HowCanWeGroupBox.ResumeLayout(false);
			this.HowCanWeGroupBox.PerformLayout();
			this.PleaseSelectReasonGroupBox.ResumeLayout(false);
			this.PleaseSelectReasonGroupBox.PerformLayout();
			this.ResolutionMethodDropEdit.ResumeLayout(true);
			this.ResolutionMethodDropEdit.PerformLayout();
			this.CustomerFacingResolutionMessageGroupBox.ResumeLayout(false);
			this.CustomerFacingResolutionMessageGroupBox.PerformLayout();
			this.PleaseChooseOptionsGroupBox.ResumeLayout(false);
			this.PleaseChooseOptionsGroupBox.PerformLayout();
			this.PleaseChooseOptionsFlowLayoutPanel.ResumeLayout(false);
			this.PleaseChooseOptionsFlowLayoutPanel.PerformLayout();
			this.MainTableLayoutPanel.ResumeLayout(false);
			this.MainTableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private CargoWise.Windows.UI.KFlowLayoutPanel MainFlowLayoutPanel;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox PleaseChooseOptionsGroupBox;
		public Enterprise.ZArchitecture.GUI.ZGroupBox ShouldContentBeDevelopedGroupBox;
		public Enterprise.ZArchitecture.GUI.ZGroupBox PleaseProvideLinkGroupBox;
		public Enterprise.ZArchitecture.GUI.ZGroupBox PleaseProvideReasonGroupBox;
		public Enterprise.ZArchitecture.GUI.ZGroupBox WhatChangesGroupBox;
		public Enterprise.ZArchitecture.GUI.ZGroupBox HowCanWeGroupBox;
		public Enterprise.ZArchitecture.GUI.ZGroupBox PleaseSelectReasonGroupBox;
		public Enterprise.ZArchitecture.GUI.ZGroupBox CustomerFacingResolutionMessageGroupBox;
		private Enterprise.ZArchitecture.ZTextBox LinksToExistingContentTextBox;
		public Enterprise.ZArchitecture.ZTextBox OtherReasonTextBox;
		private Enterprise.ZArchitecture.ZTextBox RecommendToContentTextBox;
		private Enterprise.ZArchitecture.ZTextBox MakeContentEasierTextBox;
		private Enterprise.ZArchitecture.ZTextBox ResolutionMessageTextBox;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton CompletelySolvedRadioButton;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton PartlySolvedRadioButton;
		protected Enterprise.ZArchitecture.GUI.ZRadioButton ContentNotBeFoundRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ContentBeDevelopedNotWorthRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ContentBeDevelopYesRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ContentIrrelevantERequestRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton HighlyClientSpecificRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ComplexEdgeCaseRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton OtherRadioButton;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ResolutionMethodDropEdit;
		private Enterprise.ZArchitecture.ZLabel BottomFlagLabel;
		private CargoWise.Windows.UI.KFlowLayoutPanel ProvideReasonFlowLayoutPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel PleaseProvideReasonTableLayoutPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel MainTableLayoutPanel;
		private Enterprise.ZArchitecture.ZLabel TopFlagLabel;
		private CargoWise.Windows.UI.KFlowLayoutPanel ShouldContentBeDevelopedFlowLayoutPanel;
		private CargoWise.Windows.UI.KFlowLayoutPanel PleaseChooseOptionsFlowLayoutPanel;
		private Enterprise.ZArchitecture.GUI.ZButton PopUpEConversationButton;
		private Enterprise.ZArchitecture.GUI.Tools.SpellChecker spellChecker;
	}
}
