using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class FindTriageForm
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

		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.leftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.resultsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.resultsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.refineResultsPanel = new Enterprise.ZArchitecture.GUI.ZCollapsiblePanel();
			this.refineResultsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.productAreaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.nodeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.productDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.findTriageNodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.searchButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.descriptionKeyword3TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.descriptionKeyword2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.descriptionKeyword1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.descriptionKeywordsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.showInternalOnlyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.descriptionAndChecklistGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.triageNodePreviewUserControl = new Enterprise.Client.EDI.IncidentManager.GUI.TriageNodePreviewUserControl();
			this.rightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.nextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.mainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.closeButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.mainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.findTriageTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.reviewChecklistTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.reviewChecklistMainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.reviewChecklistLeftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.noteSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.triageNodePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.triageNoteDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.publishedDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.nodeDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.supportNotesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.supportNotesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.notesTextBoxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.supportNotesRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.supportNoteButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.updateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.reviewChecklistRightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.rightSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.checklistItemsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.checklistItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.checklistItemsGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.checklistItemsForMessageGrid = new Enterprise.ZArchitecture.ZGrid();
			this.checklistItemsButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.addToMessageButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.addAllToMessageButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.messageBuilderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.clientMessageBuilderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.clientMessageBuilderTextBoxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ClientMessageBuilderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.reviewChecklistBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.reviewChecklistCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.backToSearchButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.saveAndCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.leftPanel.SuspendLayout();
			this.resultsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.resultsGrid)).BeginInit();
			this.resultsGrid.SuspendLayout();
			this.refineResultsPanel.SuspendLayout();
			this.refineResultsGroupBox.SuspendLayout();
			this.productAreaDropEdit.SuspendLayout();
			this.nodeTypeDropEdit.SuspendLayout();
			this.productDropEdit.SuspendLayout();
			this.findTriageNodeGroupBox.SuspendLayout();
			this.descriptionAndChecklistGroupBox.SuspendLayout();
			this.triageNodePreviewUserControl.SuspendLayout();
			this.rightPanel.SuspendLayout();
			this.mainPanel.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.closeButtonsPanel.SuspendLayout();
			this.mainTabControl.SuspendLayout();
			this.findTriageTabPage.SuspendLayout();
			this.reviewChecklistTabPage.SuspendLayout();
			this.reviewChecklistMainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.reviewChecklistLeftPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.noteSplitContainer)).BeginInit();
			this.noteSplitContainer.Panel1.SuspendLayout();
			this.noteSplitContainer.Panel2.SuspendLayout();
			this.noteSplitContainer.SuspendLayout();
			this.triageNodePanel.SuspendLayout();
			this.triageNoteDetailsGroupBox.SuspendLayout();
			this.supportNotesPanel.SuspendLayout();
			this.supportNotesGroupBox.SuspendLayout();
			this.notesTextBoxPanel.SuspendLayout();
			this.supportNotesRichTextBox.SuspendLayout();
			this.supportNoteButtonsPanel.SuspendLayout();
			this.reviewChecklistRightPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.rightSplitContainer)).BeginInit();
			this.rightSplitContainer.Panel1.SuspendLayout();
			this.rightSplitContainer.Panel2.SuspendLayout();
			this.rightSplitContainer.SuspendLayout();
			this.checklistItemsPanel.SuspendLayout();
			this.checklistItemsGroupBox.SuspendLayout();
			this.checklistItemsGridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checklistItemsForMessageGrid)).BeginInit();
			this.checklistItemsForMessageGrid.SuspendLayout();
			this.checklistItemsButtonsPanel.SuspendLayout();
			this.messageBuilderPanel.SuspendLayout();
			this.clientMessageBuilderGroupBox.SuspendLayout();
			this.clientMessageBuilderTextBoxPanel.SuspendLayout();
			this.reviewChecklistBottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 563, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1053, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(411);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(412);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper);
			// 
			// leftPanel
			// 
			this.leftPanel.Controls.Add(this.resultsGroupBox);
			this.leftPanel.Controls.Add(this.refineResultsPanel);
			this.leftPanel.Controls.Add(this.findTriageNodeGroupBox);
			this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.leftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.leftPanel.Name = "leftPanel";
			this.leftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 502, true);
			this.leftPanel.TabIndex = 0;
			// 
			// resultsGroupBox
			// 
			this.resultsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("a312545c-1380-4b39-b5e9-d8059bf04cd9", "Results");
			this.resultsGroupBox.Controls.Add(this.resultsGrid);
			this.resultsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.resultsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 184, true);
			this.resultsGroupBox.Name = "resultsGroupBox";
			this.resultsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 318, true);
			this.resultsGroupBox.TabIndex = 1;
			this.resultsGroupBox.TabStop = false;
			// 
			// resultsGrid
			// 
			this.resultsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.resultsGrid, "TriageCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).IMT_SupportDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).IMT_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).IMT_Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).IMT_ProductArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).IMT_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).IMT_IsInternal)));
			this.resultsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("6f71ff8d-a03d-46f1-8d9e-3698ca61482a", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "IMT_SupportDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(179);
			zTextBoxColumnStyleInfo2.ColumnName = "IMT_Type";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.ColumnName = "IMT_Product";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.ColumnName = "IMT_ProductArea";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo5.ColumnName = "IMT_Module";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "IMT_IsInternal";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.resultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.resultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.resultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.resultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.resultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.resultsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.resultsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.resultsGrid.GridId = "620c1454-13b8-484c-9777-589ced232300";
			this.resultsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.resultsGrid.LayoutKey = "resultsGrid";
			this.resultsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.resultsGrid.Name = "resultsGrid";
			this.resultsGrid.ReadOnly = true;
			this.resultsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.resultsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 299, true);
			this.resultsGrid.TabIndex = 0;
			// 
			// refineResultsPanel
			// 
			this.refineResultsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.refineResultsPanel.Controls.Add(this.refineResultsGroupBox);
			this.refineResultsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.refineResultsPanel.IsCollapsed = false;
			this.refineResultsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
			this.refineResultsPanel.Name = "refineResultsPanel";
			this.refineResultsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 20, 0, 0, true);
			this.refineResultsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 115, true);
			this.refineResultsPanel.TabIndex = 2;
			this.refineResultsPanel.Text = "Refine Results";
			// 
			// refineResultsGroupBox
			// 
			this.refineResultsGroupBox.Controls.Add(this.productAreaDropEdit);
			this.refineResultsGroupBox.Controls.Add(this.nodeTypeDropEdit);
			this.refineResultsGroupBox.Controls.Add(this.productDropEdit);
			this.refineResultsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.refineResultsGroupBox, false);
			this.refineResultsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			this.refineResultsGroupBox.Name = "refineResultsGroupBox";
			this.refineResultsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 95, true);
			this.refineResultsGroupBox.TabIndex = 3;
			this.refineResultsGroupBox.TabStop = false;
			// 
			// productAreaDropEdit
			// 
			this.productAreaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.productAreaDropEdit, "ProductArea");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).ProductArea)));
			this.productAreaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 64, true);
			this.productAreaDropEdit.Name = "productAreaDropEdit";
			this.productAreaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.productAreaDropEdit.TabIndex = 2;
			// 
			// nodeTypeDropEdit
			// 
			this.nodeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.nodeTypeDropEdit, "NodeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).NodeType)));
			this.nodeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 18, true);
			this.nodeTypeDropEdit.Name = "nodeTypeDropEdit";
			this.nodeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.nodeTypeDropEdit.TabIndex = 0;
			// 
			// productDropEdit
			// 
			this.productDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.productDropEdit, "Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).Product)));
			this.productDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 41, true);
			this.productDropEdit.Name = "productDropEdit";
			this.productDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.productDropEdit.TabIndex = 1;
			// 
			// findTriageNodeGroupBox
			// 
			this.findTriageNodeGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("c349dac2-6c5d-484a-b440-c18ebcbfc5b3", "Find Triage Node");
			this.findTriageNodeGroupBox.Controls.Add(this.searchButton);
			this.findTriageNodeGroupBox.Controls.Add(this.descriptionKeyword3TextBox);
			this.findTriageNodeGroupBox.Controls.Add(this.descriptionKeyword2TextBox);
			this.findTriageNodeGroupBox.Controls.Add(this.descriptionKeyword1TextBox);
			this.findTriageNodeGroupBox.Controls.Add(this.descriptionKeywordsLabel);
			this.findTriageNodeGroupBox.Controls.Add(this.showInternalOnlyCheckBox);
			this.findTriageNodeGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.findTriageNodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.findTriageNodeGroupBox.Name = "findTriageNodeGroupBox";
			this.findTriageNodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 69, true);
			this.findTriageNodeGroupBox.TabIndex = 0;
			this.findTriageNodeGroupBox.TabStop = false;
			// 
			// searchButton
			// 
			this.searchButton.CaptionResourceString = ZClientEDI.Res.GetData("1cdf45b0-483e-409e-a5ad-ad0207940f2b", "Search");
			this.searchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(447, 18, true);
			this.searchButton.Name = "searchButton";
			this.searchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 21, true);
			this.searchButton.TabIndex = 7;
			this.searchButton.ToolTipCaption = null;
			this.searchButton.UseVisualStyleBackColor = true;
			this.searchButton.Click += new System.EventHandler(this.SearchButton_Click);
			// 
			// descriptionKeyword3TextBox
			// 
			this.BindingSource.SetBindingMember(this.descriptionKeyword3TextBox, "DescriptionKeyword3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).DescriptionKeyword3)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.descriptionKeyword3TextBox, false);
			this.descriptionKeyword3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 20, true);
			this.descriptionKeyword3TextBox.Name = "descriptionKeyword3TextBox";
			this.descriptionKeyword3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.descriptionKeyword3TextBox.TabIndex = 6;
			// 
			// descriptionKeyword2TextBox
			// 
			this.BindingSource.SetBindingMember(this.descriptionKeyword2TextBox, "DescriptionKeyword2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).DescriptionKeyword2)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.descriptionKeyword2TextBox, false);
			this.descriptionKeyword2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 20, true);
			this.descriptionKeyword2TextBox.Name = "descriptionKeyword2TextBox";
			this.descriptionKeyword2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.descriptionKeyword2TextBox.TabIndex = 5;
			// 
			// descriptionKeyword1TextBox
			// 
			this.BindingSource.SetBindingMember(this.descriptionKeyword1TextBox, "DescriptionKeyword1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).DescriptionKeyword1)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.descriptionKeyword1TextBox, false);
			this.descriptionKeyword1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 20, true);
			this.descriptionKeyword1TextBox.Name = "descriptionKeyword1TextBox";
			this.descriptionKeyword1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.descriptionKeyword1TextBox.TabIndex = 4;
			// 
			// descriptionKeywordsLabel
			// 
			this.descriptionKeywordsLabel.CaptionResourceString = ZClientEDI.Res.GetData("ca5e7ada-21a9-433b-b839-270962f4500b", "Description keywords");
			this.descriptionKeywordsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.descriptionKeywordsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.descriptionKeywordsLabel.Name = "descriptionKeywordsLabel";
			this.descriptionKeywordsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 23, true);
			this.descriptionKeywordsLabel.TabIndex = 3;
			this.descriptionKeywordsLabel.UseMnemonic = false;
			// 
			// showInternalOnlyCheckBox
			// 
			this.BindingSource.SetBindingMember(this.showInternalOnlyCheckBox, "ShowInternalOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).ShowInternalOnly)));
			this.showInternalOnlyCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("c928b422-5085-4d77-9b71-2ba4e550cf95", "Show Internal Only");
			this.showInternalOnlyCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.showInternalOnlyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 40, true);
			this.showInternalOnlyCheckBox.Name = "showInternalOnlyCheckBox";
			this.showInternalOnlyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 24, true);
			this.showInternalOnlyCheckBox.TabIndex = 2;
			this.showInternalOnlyCheckBox.UseVisualStyleBackColor = true;
			// 
			// descriptionAndChecklistGroupBox
			// 
			this.descriptionAndChecklistGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("2a9000f5-0357-4f0c-811d-87e4602d9a47", "Preview Selected Node");
			this.descriptionAndChecklistGroupBox.Controls.Add(this.triageNodePreviewUserControl);
			this.descriptionAndChecklistGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.descriptionAndChecklistGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.descriptionAndChecklistGroupBox.Name = "descriptionAndChecklistGroupBox";
			this.descriptionAndChecklistGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 502, true);
			this.descriptionAndChecklistGroupBox.TabIndex = 0;
			this.descriptionAndChecklistGroupBox.TabStop = false;
			// 
			// triageNodePreviewUserControl
			// 
			this.triageNodePreviewUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.triageNodePreviewUserControl, "TriageCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)))));
			this.triageNodePreviewUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.triageNodePreviewUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.triageNodePreviewUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.triageNodePreviewUserControl.Name = "triageNodePreviewUserControl";
			this.triageNodePreviewUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 483, true);
			this.triageNodePreviewUserControl.TabIndex = 0;
			// 
			// rightPanel
			// 
			this.rightPanel.Controls.Add(this.descriptionAndChecklistGroupBox);
			this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(538, 0, true);
			this.rightPanel.Name = "rightPanel";
			this.rightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 502, true);
			this.rightPanel.TabIndex = 1;
			// 
			// nextButton
			// 
			this.nextButton.CaptionResourceString = ZClientEDI.Res.GetData("356a0fac-1151-4e84-910c-958fc18fcd1f", "Next");
			this.nextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.nextButton.Name = "nextButton";
			this.nextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.nextButton.TabIndex = 0;
			this.nextButton.ToolTipCaption = null;
			this.nextButton.UseVisualStyleBackColor = true;
			this.nextButton.Click += new System.EventHandler(this.NextButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = ZClientEDI.Res.GetData("4bd8d2e1-a284-44be-9ea2-7b16071bd453", "Cancel");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 5, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// mainPanel
			// 
			this.mainPanel.Controls.Add(this.rightPanel);
			this.mainPanel.Controls.Add(this.leftPanel);
			this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1039, 502, true);
			this.mainPanel.TabIndex = 1;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.closeButtonsPanel);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 505, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1039, 28, true);
			this.bottomPanel.TabIndex = 2;
			// 
			// closeButtonsPanel
			// 
			this.closeButtonsPanel.Controls.Add(this.cancelButton);
			this.closeButtonsPanel.Controls.Add(this.nextButton);
			this.closeButtonsPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.closeButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(879, 0, true);
			this.closeButtonsPanel.Name = "closeButtonsPanel";
			this.closeButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 28, true);
			this.closeButtonsPanel.TabIndex = 0;
			// 
			// mainTabControl
			// 
			this.mainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.mainTabControl.Controls.Add(this.findTriageTabPage);
			this.mainTabControl.Controls.Add(this.reviewChecklistTabPage);
			this.mainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainTabControl.Name = "mainTabControl";
			this.mainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1053, 563, true);
			this.mainTabControl.TabIndex = 8;
			// 
			// findTriageTabPage
			// 
			this.findTriageTabPage.CaptionResourceString = ZClientEDI.Res.GetData("509b6dcd-73f4-4524-8bb4-c2c532dbe48b", "Find Triage");
			this.findTriageTabPage.Controls.Add(this.mainPanel);
			this.findTriageTabPage.Controls.Add(this.bottomPanel);
			this.findTriageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.findTriageTabPage.Name = "findTriageTabPage";
			this.findTriageTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.findTriageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1045, 536, true);
			this.findTriageTabPage.TabIndex = 0;
			this.findTriageTabPage.UseVisualStyleBackColor = true;
			// 
			// reviewChecklistTabPage
			// 
			this.reviewChecklistTabPage.CaptionResourceString = ZClientEDI.Res.GetData("30eb5d2e-944e-408e-b064-93004f34f022", "Review Checklist");
			this.reviewChecklistTabPage.Controls.Add(this.reviewChecklistMainPanel);
			this.reviewChecklistTabPage.Controls.Add(this.reviewChecklistBottomPanel);
			this.reviewChecklistTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.reviewChecklistTabPage.Name = "reviewChecklistTabPage";
			this.reviewChecklistTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.reviewChecklistTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1045, 536, true);
			this.reviewChecklistTabPage.TabIndex = 1;
			this.reviewChecklistTabPage.UseVisualStyleBackColor = true;
			// 
			// reviewChecklistMainPanel
			// 
			this.reviewChecklistMainPanel.Controls.Add(this.mainSplitContainer);
			this.reviewChecklistMainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.reviewChecklistMainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.reviewChecklistMainPanel.Name = "reviewChecklistMainPanel";
			this.reviewChecklistMainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1039, 502, true);
			this.reviewChecklistMainPanel.TabIndex = 1;
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.reviewChecklistLeftPanel);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.reviewChecklistRightPanel);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1039, 502, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(552);
			this.mainSplitContainer.SplitterWidth = 12;
			this.mainSplitContainer.TabIndex = 0;
			// 
			// reviewChecklistLeftPanel
			// 
			this.reviewChecklistLeftPanel.Controls.Add(this.noteSplitContainer);
			this.reviewChecklistLeftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.reviewChecklistLeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.reviewChecklistLeftPanel.Name = "reviewChecklistLeftPanel";
			this.reviewChecklistLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 502, true);
			this.reviewChecklistLeftPanel.TabIndex = 0;
			// 
			// noteSplitContainer
			// 
			this.noteSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.noteSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.noteSplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 0, true);
			this.noteSplitContainer.Name = "noteSplitContainer";
			this.noteSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// noteSplitContainer.Panel1
			// 
			this.noteSplitContainer.Panel1.Controls.Add(this.triageNodePanel);
			this.noteSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 502, true);
			this.noteSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
			// 
			// noteSplitContainer.Panel2
			// 
			this.noteSplitContainer.Panel2.Controls.Add(this.supportNotesPanel);
			this.noteSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			this.noteSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(135);
			this.noteSplitContainer.SplitterWidth = 12;
			this.noteSplitContainer.TabIndex = 0;
			// 
			// triageNodePanel
			// 
			this.triageNodePanel.Controls.Add(this.triageNoteDetailsGroupBox);
			this.triageNodePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.triageNodePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.triageNodePanel.Name = "triageNodePanel";
			this.triageNodePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 135, true);
			this.triageNodePanel.TabIndex = 0;
			// 
			// triageNoteDetailsGroupBox
			// 
			this.triageNoteDetailsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("438f46f5-c101-497d-ba48-8b0d05b231c6", "Triage Node Details");
			this.triageNoteDetailsGroupBox.Controls.Add(this.publishedDescriptionTextBox);
			this.triageNoteDetailsGroupBox.Controls.Add(this.nodeDescriptionTextBox);
			this.triageNoteDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.triageNoteDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.triageNoteDetailsGroupBox.Name = "triageNoteDetailsGroupBox";
			this.triageNoteDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 135, true);
			this.triageNoteDetailsGroupBox.TabIndex = 0;
			this.triageNoteDetailsGroupBox.TabStop = false;
			// 
			// publishedDescriptionTextBox
			// 
			this.publishedDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.publishedDescriptionTextBox, "TriageCollection.PublishedDescriptionText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).PublishedDescriptionText)));
			this.publishedDescriptionTextBox.CaptionResourceString = ZClientEDI.Res.GetData("88ca6509-a028-41e5-9511-45ba7767a08f", "Published Description");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.publishedDescriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.publishedDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 71, true);
			this.publishedDescriptionTextBox.Multiline = true;
			this.publishedDescriptionTextBox.Name = "publishedDescriptionTextBox";
			this.publishedDescriptionTextBox.ReadOnly = true;
			this.publishedDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 55, true);
			this.publishedDescriptionTextBox.TabIndex = 1;
			// 
			// nodeDescriptionTextBox
			// 
			this.nodeDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.nodeDescriptionTextBox, "TriageCollection.IMT_SupportDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).IMT_SupportDescription)));
			this.nodeDescriptionTextBox.CaptionResourceString = ZClientEDI.Res.GetData("40bc4a6e-0ed9-472b-ac9f-8c35d6be88e3", "Node Description");
			this.nodeDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.nodeDescriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.nodeDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 34, true);
			this.nodeDescriptionTextBox.Name = "nodeDescriptionTextBox";
			this.nodeDescriptionTextBox.ReadOnly = true;
			this.nodeDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 20, true);
			this.nodeDescriptionTextBox.TabIndex = 0;
			// 
			// supportNotesPanel
			// 
			this.supportNotesPanel.Controls.Add(this.supportNotesGroupBox);
			this.supportNotesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.supportNotesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.supportNotesPanel.Name = "supportNotesPanel";
			this.supportNotesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 355, true);
			this.supportNotesPanel.TabIndex = 0;
			// 
			// supportNotesGroupBox
			// 
			this.supportNotesGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("53897e68-abf4-45ed-ba2b-39b3b47eb692", "Support Notes");
			this.supportNotesGroupBox.Controls.Add(this.notesTextBoxPanel);
			this.supportNotesGroupBox.Controls.Add(this.supportNoteButtonsPanel);
			this.supportNotesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.supportNotesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.supportNotesGroupBox.Name = "supportNotesGroupBox";
			this.supportNotesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 355, true);
			this.supportNotesGroupBox.TabIndex = 0;
			this.supportNotesGroupBox.TabStop = false;
			// 
			// notesTextBoxPanel
			// 
			this.notesTextBoxPanel.Controls.Add(this.supportNotesRichTextBox);
			this.notesTextBoxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.notesTextBoxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.notesTextBoxPanel.Name = "notesTextBoxPanel";
			this.notesTextBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 310, true);
			this.notesTextBoxPanel.TabIndex = 2;
			// 
			// supportNotesRichTextBox
			// 
			this.BindingSource.SetBindingMember(this.supportNotesRichTextBox, "TriageCollection.SupportNotesAsBlob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).SupportNotesAsBlob)));
			this.supportNotesRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.supportNotesRichTextBox, false);
			this.supportNotesRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.supportNotesRichTextBox.MaxLength = 10000000;
			this.supportNotesRichTextBox.Name = "supportNotesRichTextBox";
			this.supportNotesRichTextBox.ParentZForm = this;
			this.supportNotesRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 310, true);
			this.supportNotesRichTextBox.TabIndex = 0;
			// 
			// supportNoteButtonsPanel
			// 
			this.supportNoteButtonsPanel.Controls.Add(this.updateButton);
			this.supportNoteButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.supportNoteButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 326, true);
			this.supportNoteButtonsPanel.Name = "supportNoteButtonsPanel";
			this.supportNoteButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 26, true);
			this.supportNoteButtonsPanel.TabIndex = 1;
			// 
			// updateButton
			// 
			this.updateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.updateButton.CaptionResourceString = ZClientEDI.Res.GetData("1790a23a-b55f-4e25-a538-c4643186a56b", "Update");
			this.updateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 4, true);
			this.updateButton.Name = "updateButton";
			this.updateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.updateButton.TabIndex = 0;
			this.updateButton.ToolTipCaption = null;
			this.updateButton.UseVisualStyleBackColor = true;
			this.updateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// reviewChecklistRightPanel
			// 
			this.reviewChecklistRightPanel.Controls.Add(this.rightSplitContainer);
			this.reviewChecklistRightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.reviewChecklistRightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.reviewChecklistRightPanel.Name = "reviewChecklistRightPanel";
			this.reviewChecklistRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 502, true);
			this.reviewChecklistRightPanel.TabIndex = 0;
			// 
			// rightSplitContainer
			// 
			this.rightSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rightSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rightSplitContainer.Name = "rightSplitContainer";
			this.rightSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// rightSplitContainer.Panel1
			// 
			this.rightSplitContainer.Panel1.Controls.Add(this.checklistItemsPanel);
			// 
			// rightSplitContainer.Panel2
			// 
			this.rightSplitContainer.Panel2.Controls.Add(this.messageBuilderPanel);
			this.rightSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 502, true);
			this.rightSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(231);
			this.rightSplitContainer.SplitterWidth = 12;
			this.rightSplitContainer.TabIndex = 0;
			// 
			// checklistItemsPanel
			// 
			this.checklistItemsPanel.Controls.Add(this.checklistItemsGroupBox);
			this.checklistItemsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checklistItemsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.checklistItemsPanel.Name = "checklistItemsPanel";
			this.checklistItemsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 231, true);
			this.checklistItemsPanel.TabIndex = 0;
			// 
			// checklistItemsGroupBox
			// 
			this.checklistItemsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("1126dd49-8b9f-4640-9ba1-eed9c855c22c", "Checklist Items");
			this.checklistItemsGroupBox.Controls.Add(this.checklistItemsGridPanel);
			this.checklistItemsGroupBox.Controls.Add(this.checklistItemsButtonsPanel);
			this.checklistItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checklistItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.checklistItemsGroupBox.Name = "checklistItemsGroupBox";
			this.checklistItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 231, true);
			this.checklistItemsGroupBox.TabIndex = 0;
			this.checklistItemsGroupBox.TabStop = false;
			// 
			// checklistItemsGridPanel
			// 
			this.checklistItemsGridPanel.Controls.Add(this.checklistItemsForMessageGrid);
			this.checklistItemsGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checklistItemsGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.checklistItemsGridPanel.Name = "checklistItemsGridPanel";
			this.checklistItemsGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 185, true);
			this.checklistItemsGridPanel.TabIndex = 1;
			// 
			// checklistItemsForMessageGrid
			// 
			this.checklistItemsForMessageGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.checklistItemsForMessageGrid, "TriageCollection.ChecklistPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).ChecklistPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItemPivot)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).ChecklistPivots)).SyncRoot)).IMP_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItemPivot)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).ChecklistPivots)).SyncRoot)).ChecklistItem.IMC_SupportDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItemPivot)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).ChecklistPivots)).SyncRoot)).ChecklistItem.IMC_IsPublished)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItemPivot)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).ChecklistPivots)).SyncRoot)).ChecklistItem.IMC_Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItemPivot)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).TriageCollection)).SyncRoot)).ChecklistPivots)).SyncRoot)).ChecklistItem.PublishedDescriptionText)));
			this.checklistItemsForMessageGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "IMP_Sequence";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo6.ColumnName = "ChecklistItem+IMC_SupportDescription";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("86927de2-63e5-47fd-b88a-4061bc0cbc1e", "Published to Portal");
			zCheckBoxColumnStyleInfo2.ColumnName = "ChecklistItem+IMC_IsPublished";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "ChecklistItem+IMC_Category";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo8.ColumnName = "ChecklistItem+PublishedDescriptionText";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.checklistItemsForMessageGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.checklistItemsForMessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.checklistItemsForMessageGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.checklistItemsForMessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.checklistItemsForMessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.checklistItemsForMessageGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checklistItemsForMessageGrid.GridId = "36418cb1-3681-4f30-984d-9e2f7b4fdd4c";
			this.checklistItemsForMessageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.checklistItemsForMessageGrid.LayoutKey = "checklistItemsGrid";
			this.checklistItemsForMessageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.checklistItemsForMessageGrid.Name = "checklistItemsForMessageGrid";
			this.checklistItemsForMessageGrid.ReadOnly = true;
			this.checklistItemsForMessageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 185, true);
			this.checklistItemsForMessageGrid.TabIndex = 0;
			this.checklistItemsForMessageGrid.DoubleClick += new System.EventHandler(this.ChecklistItemsGrid_DoubleClick);
			// 
			// checklistItemsButtonsPanel
			// 
			this.checklistItemsButtonsPanel.Controls.Add(this.addToMessageButton);
			this.checklistItemsButtonsPanel.Controls.Add(this.addAllToMessageButton);
			this.checklistItemsButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.checklistItemsButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 201, true);
			this.checklistItemsButtonsPanel.Name = "checklistItemsButtonsPanel";
			this.checklistItemsButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 27, true);
			this.checklistItemsButtonsPanel.TabIndex = 0;
			// 
			// addToMessageButton
			// 
			this.addToMessageButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.addToMessageButton.CaptionResourceString = ZClientEDI.Res.GetData("9fd4e98c-6cc8-41d2-aac1-f8e312a6d623", "Add To Message");
			this.addToMessageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 2, true);
			this.addToMessageButton.Name = "addToMessageButton";
			this.addToMessageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 22, true);
			this.addToMessageButton.TabIndex = 1;
			this.addToMessageButton.ToolTipCaption = null;
			this.addToMessageButton.UseVisualStyleBackColor = true;
			this.addToMessageButton.Click += new System.EventHandler(this.AddToMessageButton_Click);
			// 
			// addAllToMessageButton
			// 
			this.addAllToMessageButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.addAllToMessageButton.CaptionResourceString = ZClientEDI.Res.GetData("306f0c5b-8b94-41ad-8fd9-982abafd7e07", "Add All To Message");
			this.addAllToMessageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 2, true);
			this.addAllToMessageButton.Name = "addAllToMessageButton";
			this.addAllToMessageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 22, true);
			this.addAllToMessageButton.TabIndex = 0;
			this.addAllToMessageButton.ToolTipCaption = null;
			this.addAllToMessageButton.UseVisualStyleBackColor = true;
			this.addAllToMessageButton.Click += new System.EventHandler(this.AddAllToMessageButton_Click);
			// 
			// messageBuilderPanel
			// 
			this.messageBuilderPanel.Controls.Add(this.clientMessageBuilderGroupBox);
			this.messageBuilderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageBuilderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messageBuilderPanel.Name = "messageBuilderPanel";
			this.messageBuilderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 259, true);
			this.messageBuilderPanel.TabIndex = 0;
			// 
			// clientMessageBuilderGroupBox
			// 
			this.clientMessageBuilderGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("d1e39e27-e27f-4054-bdc1-99f8b25b39ca", "Client Message Builder");
			this.clientMessageBuilderGroupBox.Controls.Add(this.clientMessageBuilderTextBoxPanel);
			this.clientMessageBuilderGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.clientMessageBuilderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.clientMessageBuilderGroupBox.Name = "clientMessageBuilderGroupBox";
			this.clientMessageBuilderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 259, true);
			this.clientMessageBuilderGroupBox.TabIndex = 0;
			this.clientMessageBuilderGroupBox.TabStop = false;
			// 
			// clientMessageBuilderTextBoxPanel
			// 
			this.clientMessageBuilderTextBoxPanel.Controls.Add(this.ClientMessageBuilderTextBox);
			this.clientMessageBuilderTextBoxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.clientMessageBuilderTextBoxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.clientMessageBuilderTextBoxPanel.Name = "clientMessageBuilderTextBoxPanel";
			this.clientMessageBuilderTextBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 214, true);
			this.clientMessageBuilderTextBoxPanel.TabIndex = 1;
			// 
			// clientMessageBuilderTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientMessageBuilderTextBox, "ClientMessageBuilder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper)(null)).ClientMessageBuilder)));
			this.ClientMessageBuilderTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientMessageBuilderTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClientMessageBuilderTextBox, false);
			this.ClientMessageBuilderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClientMessageBuilderTextBox.Multiline = true;
			this.ClientMessageBuilderTextBox.Name = "clientMessageBuilderTextBox";
			this.ClientMessageBuilderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 214, true);
			this.ClientMessageBuilderTextBox.TabIndex = 0;
			// 
			// reviewChecklistBottomPanel
			// 
			this.reviewChecklistBottomPanel.Controls.Add(this.reviewChecklistCancelButton);
			this.reviewChecklistBottomPanel.Controls.Add(this.backToSearchButton);
			this.reviewChecklistBottomPanel.Controls.Add(this.saveAndCloseButton);
			this.reviewChecklistBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.reviewChecklistBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 505, true);
			this.reviewChecklistBottomPanel.Name = "reviewChecklistBottomPanel";
			this.reviewChecklistBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1039, 28, true);
			this.reviewChecklistBottomPanel.TabIndex = 2;
			// 
			// reviewChecklistCancelButton
			//
			this.reviewChecklistCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.reviewChecklistCancelButton.CaptionResourceString = ZClientEDI.Res.GetData("d5bd83e0-798f-4829-a186-f9ea49d2bb67", "Cancel");
			this.reviewChecklistCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(940, 5, true);
			this.reviewChecklistCancelButton.Name = "reviewChecklistCancelButton";
			this.reviewChecklistCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.reviewChecklistCancelButton.TabIndex = 3;
			this.reviewChecklistCancelButton.ToolTipCaption = null;
			this.reviewChecklistCancelButton.UseVisualStyleBackColor = true;
			this.reviewChecklistCancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// backToSearchButton
			//
			this.backToSearchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.backToSearchButton.CaptionResourceString = ZClientEDI.Res.GetData("a5fd6bbf-8497-4fcd-98f5-9c0b8346e142", "Back To Search");
			this.backToSearchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(730, 5, true);
			this.backToSearchButton.Name = "backToSearchButton";
			this.backToSearchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.backToSearchButton.TabIndex = 2;
			this.backToSearchButton.ToolTipCaption = null;
			this.backToSearchButton.UseVisualStyleBackColor = true;
			this.backToSearchButton.Click += new System.EventHandler(this.BackToSearchButton_Click);
			// 
			// saveAndCloseButton
			// 
			this.saveAndCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveAndCloseButton.CaptionResourceString = ZClientEDI.Res.GetData("a393fe1a-0a6e-4faf-98a7-cb6ec8f9a1e9", "Save && Close");
			this.saveAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(835, 5, true);
			this.saveAndCloseButton.Name = "saveAndCloseButton";
			this.saveAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.saveAndCloseButton.TabIndex = 1;
			this.saveAndCloseButton.ToolTipCaption = null;
			this.saveAndCloseButton.UseVisualStyleBackColor = true;
			this.saveAndCloseButton.Click += new System.EventHandler(this.SaveAndCloseButton_Click);
			// 
			// FindTriageForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1053, 587, true);
			this.Controls.Add(this.mainTabControl);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper);
			this.DataSourceTypeName = "Enterprise.Client.EDI.IncidentManager.GUI.FindTriageFilterHelper";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1066, 625, true);
			this.Name = "FindTriageForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Triage Assist";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.mainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.leftPanel.ResumeLayout(false);
			this.leftPanel.PerformLayout();
			this.resultsGroupBox.ResumeLayout(false);
			this.resultsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.resultsGrid)).EndInit();
			this.resultsGrid.ResumeLayout(false);
			this.resultsGrid.PerformLayout();
			this.refineResultsPanel.ResumeLayout(false);
			this.refineResultsPanel.PerformLayout();
			this.refineResultsGroupBox.ResumeLayout(false);
			this.refineResultsGroupBox.PerformLayout();
			this.productAreaDropEdit.ResumeLayout(true);
			this.productAreaDropEdit.PerformLayout();
			this.nodeTypeDropEdit.ResumeLayout(true);
			this.nodeTypeDropEdit.PerformLayout();
			this.productDropEdit.ResumeLayout(true);
			this.productDropEdit.PerformLayout();
			this.findTriageNodeGroupBox.ResumeLayout(false);
			this.findTriageNodeGroupBox.PerformLayout();
			this.descriptionAndChecklistGroupBox.ResumeLayout(false);
			this.descriptionAndChecklistGroupBox.PerformLayout();
			this.triageNodePreviewUserControl.ResumeLayout(true);
			this.triageNodePreviewUserControl.PerformLayout();
			this.rightPanel.ResumeLayout(false);
			this.rightPanel.PerformLayout();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.closeButtonsPanel.ResumeLayout(false);
			this.closeButtonsPanel.PerformLayout();
			this.mainTabControl.ResumeLayout(false);
			this.mainTabControl.PerformLayout();
			this.findTriageTabPage.ResumeLayout(false);
			this.findTriageTabPage.PerformLayout();
			this.reviewChecklistTabPage.ResumeLayout(false);
			this.reviewChecklistTabPage.PerformLayout();
			this.reviewChecklistMainPanel.ResumeLayout(false);
			this.reviewChecklistMainPanel.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.reviewChecklistLeftPanel.ResumeLayout(false);
			this.reviewChecklistLeftPanel.PerformLayout();
			this.noteSplitContainer.Panel1.ResumeLayout(false);
			this.noteSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.noteSplitContainer)).EndInit();
			this.noteSplitContainer.ResumeLayout(false);
			this.noteSplitContainer.PerformLayout();
			this.triageNodePanel.ResumeLayout(false);
			this.triageNodePanel.PerformLayout();
			this.triageNoteDetailsGroupBox.ResumeLayout(false);
			this.triageNoteDetailsGroupBox.PerformLayout();
			this.supportNotesPanel.ResumeLayout(false);
			this.supportNotesPanel.PerformLayout();
			this.supportNotesGroupBox.ResumeLayout(false);
			this.supportNotesGroupBox.PerformLayout();
			this.notesTextBoxPanel.ResumeLayout(false);
			this.notesTextBoxPanel.PerformLayout();
			this.supportNotesRichTextBox.ResumeLayout(true);
			this.supportNotesRichTextBox.PerformLayout();
			this.supportNoteButtonsPanel.ResumeLayout(false);
			this.supportNoteButtonsPanel.PerformLayout();
			this.reviewChecklistRightPanel.ResumeLayout(false);
			this.reviewChecklistRightPanel.PerformLayout();
			this.rightSplitContainer.Panel1.ResumeLayout(false);
			this.rightSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.rightSplitContainer)).EndInit();
			this.rightSplitContainer.ResumeLayout(false);
			this.rightSplitContainer.PerformLayout();
			this.checklistItemsPanel.ResumeLayout(false);
			this.checklistItemsPanel.PerformLayout();
			this.checklistItemsGroupBox.ResumeLayout(false);
			this.checklistItemsGroupBox.PerformLayout();
			this.checklistItemsGridPanel.ResumeLayout(false);
			this.checklistItemsGridPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checklistItemsForMessageGrid)).EndInit();
			this.checklistItemsForMessageGrid.ResumeLayout(false);
			this.checklistItemsForMessageGrid.PerformLayout();
			this.checklistItemsButtonsPanel.ResumeLayout(false);
			this.checklistItemsButtonsPanel.PerformLayout();
			this.messageBuilderPanel.ResumeLayout(false);
			this.messageBuilderPanel.PerformLayout();
			this.clientMessageBuilderGroupBox.ResumeLayout(false);
			this.clientMessageBuilderGroupBox.PerformLayout();
			this.clientMessageBuilderTextBoxPanel.ResumeLayout(false);
			this.clientMessageBuilderTextBoxPanel.PerformLayout();
			this.reviewChecklistBottomPanel.ResumeLayout(false);
			this.reviewChecklistBottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private ZGroupBox resultsGroupBox;
		private ZGroupBox descriptionAndChecklistGroupBox;

		#endregion

		private ZPanel leftPanel;
		private ZPanel rightPanel;
		private ZGroupBox findTriageNodeGroupBox;
		private ZCheckBox showInternalOnlyCheckBox;
		private ZArchitecture.ZLabel descriptionKeywordsLabel;
		private ZArchitecture.ZTextBox descriptionKeyword1TextBox;
		private ZArchitecture.ZTextBox descriptionKeyword3TextBox;
		private ZArchitecture.ZTextBox descriptionKeyword2TextBox;
		private ZCollapsiblePanel refineResultsPanel;
		private ZDropEdit nodeTypeDropEdit;
		private ZDropEdit productDropEdit;
		private ZDropEdit productAreaDropEdit;
		private ZGroupBox refineResultsGroupBox;
		private ZButton searchButton;
		private ZArchitecture.ZGrid resultsGrid;
		private TriageNodePreviewUserControl triageNodePreviewUserControl;
		private ZButton cancelButton;
		private ZButton nextButton;
		private ZPanel mainPanel;
		private ZPanel bottomPanel;
		private ZPanel closeButtonsPanel;
		private ZTabControl mainTabControl;
		private ZTabPage findTriageTabPage;
		private ZTabPage reviewChecklistTabPage;
		private ZPanel reviewChecklistMainPanel;
		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		private ZPanel reviewChecklistLeftPanel;
		private CargoWise.Windows.UI.KSplitContainer noteSplitContainer;
		private ZPanel triageNodePanel;
		private ZGroupBox triageNoteDetailsGroupBox;
		private ZArchitecture.ZTextBox publishedDescriptionTextBox;
		private ZArchitecture.ZTextBox nodeDescriptionTextBox;
		private ZPanel supportNotesPanel;
		private ZGroupBox supportNotesGroupBox;
		private ZPanel notesTextBoxPanel;
		private ZRichTextBox supportNotesRichTextBox;
		private ZPanel supportNoteButtonsPanel;
		private ZButton updateButton;
		private ZPanel reviewChecklistRightPanel;
		private CargoWise.Windows.UI.KSplitContainer rightSplitContainer;
		private ZPanel checklistItemsPanel;
		private ZGroupBox checklistItemsGroupBox;
		private ZPanel checklistItemsGridPanel;
		private ZArchitecture.ZGrid checklistItemsForMessageGrid;
		private ZPanel checklistItemsButtonsPanel;
		private ZButton addToMessageButton;
		private ZButton addAllToMessageButton;
		private ZPanel messageBuilderPanel;
		private ZGroupBox clientMessageBuilderGroupBox;
		private ZPanel clientMessageBuilderTextBoxPanel;
		internal ZArchitecture.ZTextBox ClientMessageBuilderTextBox;
		private ZPanel reviewChecklistBottomPanel;
		private ZButton reviewChecklistCancelButton;
		private ZButton backToSearchButton;
		private ZButton saveAndCloseButton;
	}
}
