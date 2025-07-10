using System.Drawing;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class TriageAssistForm
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
			if (disposing)
			{
				components?.Dispose();
				InputDebouncingTimer?.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.DiagnosticCriteriaGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ShowFocusedObjectsOnlyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SearchTermOperatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TriageAssistSearchBar = new TriageAssistSearchBarUserControl();
			this.DiagnosticCriteriaProductDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SymptomInputTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DiagnosticCriteriaTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.DiagnosticCriteriaSearchResultsCollapsiblePanel = new Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistCollapsiblePanel();
			this.DiagnosticCriteriaSearchResultsGrid = new Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistGrid();
			this.DiagnosticCriteriaLinkedCollapsiblePanel = new Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistCollapsiblePanel();
			this.DiagnosticCriteriaLinkedGrid = new Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistGrid();
			this.DiagnosticCriteriaSuggestedCollapsiblePanel = new Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistCollapsiblePanel();
			this.DiagnosticCriteriaSuggestedPanelMain = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DiagnosticCriteriaSuggestedGrid = new Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistGrid();
			this.DiagnosticGuideCollapsiblePanel = new Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistCollapsiblePanel();
			this.DiagnosticGuideTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.ClientQuestionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClientQuestionTextPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ClientQuestionTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.ClientQuestionPopupPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ClientQuestionPopupButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InternalSupportNoteGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InternalSupportNoteTextPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InternalSupportNoteTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.InternalSupportNotePopupPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InternalSupportNotePopupButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TriageNodeTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.TriageNodeCollapsiblePanel = new Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistCollapsiblePanel();
			this.TriageNodePanelMain = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FinaliseSelectedButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TriageNodeTreeView = new Enterprise.ZArchitecture.GUI.ZTreeViewAdv();
			this.DescriptionTreeViewColumn = new Aga.Controls.Tree.TreeColumn();
			this.TypeTreeViewColumn = new Aga.Controls.Tree.TreeColumn();
			this.StatusTreeViewColumn = new Aga.Controls.Tree.TreeColumn();
			this.ProductTreeViewColumn = new Aga.Controls.Tree.TreeColumn();
			this.AreaTreeViewColumn = new Aga.Controls.Tree.TreeColumn();
			this.SectionTreeViewColumn = new Aga.Controls.Tree.TreeColumn();
			this.IsFocusedTreeViewColumn = new Aga.Controls.Tree.TreeColumn();
			this.DescriptionNodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.TypeNodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.StatusNodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.ProductNodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.AreaNodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.SectionNodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.IsFocusedNodeCheckBox = new Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistTreeNodeCheckBox();
			this.TriageNodeActionCollapsiblePanel = new Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistCollapsiblePanel();
			this.TriageNodeActionTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.TriageNodeClientMessageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TriageNodeClientMessageTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.TriageNodeInternalSupportActionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TriageNodeInternalSupportActionTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.TriageNodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TriageNodeFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.ShowFocusedSuggestedCriteriaOnlyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShowFocusedTriageNodesOnlyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.DiagnosticCriteriaGroupBox.SuspendLayout();
			this.SearchTermOperatorDropEdit.SuspendLayout();
			this.TriageAssistSearchBar.SuspendLayout();
			this.DiagnosticCriteriaProductDropEdit.SuspendLayout();
			this.DiagnosticCriteriaTableLayoutPanel.SuspendLayout();
			this.DiagnosticCriteriaSearchResultsCollapsiblePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DiagnosticCriteriaSearchResultsGrid)).BeginInit();
			this.DiagnosticCriteriaSearchResultsGrid.SuspendLayout();
			this.DiagnosticCriteriaLinkedCollapsiblePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DiagnosticCriteriaLinkedGrid)).BeginInit();
			this.DiagnosticCriteriaLinkedGrid.SuspendLayout();
			this.DiagnosticCriteriaSuggestedCollapsiblePanel.SuspendLayout();
			this.DiagnosticCriteriaSuggestedPanelMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DiagnosticCriteriaSuggestedGrid)).BeginInit();
			this.DiagnosticCriteriaSuggestedGrid.SuspendLayout();
			this.DiagnosticGuideCollapsiblePanel.SuspendLayout();
			this.DiagnosticGuideTableLayoutPanel.SuspendLayout();
			this.ClientQuestionGroupBox.SuspendLayout();
			this.ClientQuestionTextPanel.SuspendLayout();
			this.ClientQuestionTextBox.SuspendLayout();
			this.ClientQuestionPopupPanel.SuspendLayout();
			this.InternalSupportNoteGroupBox.SuspendLayout();
			this.InternalSupportNoteTextPanel.SuspendLayout();
			this.InternalSupportNoteTextBox.SuspendLayout();
			this.InternalSupportNotePopupPanel.SuspendLayout();
			this.DiagnosticCriteriaGroupBox.SuspendLayout();
			this.TriageAssistSearchBar.SuspendLayout();
			this.DiagnosticCriteriaProductDropEdit.SuspendLayout();
			this.TriageNodeTableLayoutPanel.SuspendLayout();
			this.TriageNodeCollapsiblePanel.SuspendLayout();
			this.TriageNodePanelMain.SuspendLayout();
			this.TriageNodeTreeView.SuspendLayout();
			this.TriageNodeActionCollapsiblePanel.SuspendLayout();
			this.TriageNodeActionTableLayoutPanel.SuspendLayout();
			this.TriageNodeClientMessageGroupBox.SuspendLayout();
			this.TriageNodeClientMessageTextBox.SuspendLayout();
			this.TriageNodeInternalSupportActionGroupBox.SuspendLayout();
			this.TriageNodeInternalSupportActionTextBox.SuspendLayout();
			this.TriageNodeGroupBox.SuspendLayout();
			this.TriageNodeFindBox.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 784, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1675, 24, true);
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
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject);
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
			this.MainSplitContainer.Panel1.Controls.Add(this.DiagnosticCriteriaTableLayoutPanel);
			this.MainSplitContainer.Panel1.Controls.Add(this.DiagnosticCriteriaGroupBox);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.TriageNodeTableLayoutPanel);
			this.MainSplitContainer.Panel2.Controls.Add(this.TriageNodeGroupBox);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1675, 754, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(706);
			this.MainSplitContainer.TabIndex = 1;
			// 
			// DiagnosticCriteriaGroupBox
			// 
			this.DiagnosticCriteriaGroupBox.Controls.Add(this.TriageAssistSearchBar);
			this.DiagnosticCriteriaGroupBox.Controls.Add(this.ShowFocusedObjectsOnlyCheckBox);
			this.DiagnosticCriteriaGroupBox.Controls.Add(this.DiagnosticCriteriaProductDropEdit);
			this.DiagnosticCriteriaGroupBox.Controls.Add(this.SymptomInputTextBox);
			this.DiagnosticCriteriaGroupBox.Controls.Add(this.SearchTermOperatorDropEdit);
			this.DiagnosticCriteriaGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.DiagnosticCriteriaGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DiagnosticCriteriaGroupBox.Name = "DiagnosticCriteriaGroupBox";
			this.DiagnosticCriteriaGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 63, true);
			this.DiagnosticCriteriaGroupBox.TabIndex = 2;
			this.DiagnosticCriteriaGroupBox.TabStop = false;
			this.DiagnosticCriteriaGroupBox.Text = "Diagnostic Criteria";
			// 
			// SearchTermOperatorDropEdit
			// 
			this.SearchTermOperatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SearchTermOperatorDropEdit, "SearchTermOperator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).SearchTermOperator)));
			this.SearchTermOperatorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SearchTermOperatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 20, true);
			this.SearchTermOperatorDropEdit.Name = "SearchTermOperatorDropEdit";
			this.SearchTermOperatorDropEdit.ShouldResizeByMaxLength = false;
			this.SearchTermOperatorDropEdit.ShowDescriptionBox = false;
			this.SearchTermOperatorDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.SearchTermOperatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.SearchTermOperatorDropEdit.TabIndex = 0;
			this.SearchTermOperatorDropEdit.TabStop = false;
			// 
			// TriageAssistSearchBar
			// 
			this.TriageAssistSearchBar.AllowDrop = true;
			this.TriageAssistSearchBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TriageAssistSearchBar, ".");
			this.TriageAssistSearchBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 45, true);
			this.TriageAssistSearchBar.Name = "TriageAssistSearchBar";
			this.TriageAssistSearchBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 17, true);
			this.TriageAssistSearchBar.TabIndex = 6;
			// 
			// ShowFocusedObjectsOnlyCheckBox
			// 
			this.ShowFocusedObjectsOnlyCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShowFocusedObjectsOnlyCheckBox, "ShowFocusedObjectsOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).ShowFocusedObjectsOnly)));
			this.ShowFocusedObjectsOnlyCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShowFocusedObjectsOnlyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 22, true);
			this.ShowFocusedObjectsOnlyCheckBox.Name = "ShowFocusedObjectsOnlyCheckBox";
			this.ShowFocusedObjectsOnlyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 17, true);
			this.ShowFocusedObjectsOnlyCheckBox.TabIndex = 5;
			this.ShowFocusedObjectsOnlyCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ShowFocusedObjectsOnlyCheckBox.UseVisualStyleBackColor = true;
			// 
			// DiagnosticCriteriaProductDropEdit
			//
			this.DiagnosticCriteriaProductDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DiagnosticCriteriaProductDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DiagnosticCriteriaProductDropEdit, "Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).Product)));
			this.DiagnosticCriteriaProductDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 20, true);
			this.DiagnosticCriteriaProductDropEdit.Name = "DiagnosticCriteriaProductDropEdit";
			this.DiagnosticCriteriaProductDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 20, true);
			this.DiagnosticCriteriaProductDropEdit.TabIndex = 4;
			this.DiagnosticCriteriaProductDropEdit.TextChanged += new System.EventHandler(this.DiagnosticCriteriaProductDropEdit_TextChanged);
			// 
			// SymptomInputTextBox
			//
			this.SymptomInputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.SymptomInputTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SymptomInputTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 20, true);
			this.SymptomInputTextBox.Name = "SymptomInputTextBox";
			this.SymptomInputTextBox.PlaceHolderText = "Start typing a symptom…";
			this.SymptomInputTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.SymptomInputTextBox.TabIndex = 3;
			this.SymptomInputTextBox.TextChanged += new System.EventHandler(this.SymptomInputTextBox_TextChanged);
			// 
			// DiagnosticCriteriaTableLayoutPanel
			// 
			this.DiagnosticCriteriaTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DiagnosticCriteriaTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DiagnosticCriteriaTableLayoutPanel.ColumnCount = 1;
			this.DiagnosticCriteriaTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.DiagnosticCriteriaTableLayoutPanel.Controls.Add(this.DiagnosticCriteriaSearchResultsCollapsiblePanel, 0, 0);
			this.DiagnosticCriteriaTableLayoutPanel.Controls.Add(this.DiagnosticCriteriaLinkedCollapsiblePanel, 0, 1);
			this.DiagnosticCriteriaTableLayoutPanel.Controls.Add(this.DiagnosticCriteriaSuggestedCollapsiblePanel, 0, 2);
			this.DiagnosticCriteriaTableLayoutPanel.Controls.Add(this.DiagnosticGuideCollapsiblePanel, 0, 3);
			this.DiagnosticCriteriaTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 65, true);
			this.DiagnosticCriteriaTableLayoutPanel.Name = "DiagnosticCriteriaTableLayoutPanel";
			this.DiagnosticCriteriaTableLayoutPanel.RowCount = 5;
			this.DiagnosticCriteriaTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.DiagnosticCriteriaTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.DiagnosticCriteriaTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.DiagnosticCriteriaTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.DiagnosticCriteriaTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(1)));
			this.DiagnosticCriteriaTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 686, true);
			this.DiagnosticCriteriaTableLayoutPanel.TabIndex = 5;
			// 
			// DiagnosticCriteriaSearchResultsCollapsiblePanel
			// 
			this.DiagnosticCriteriaSearchResultsCollapsiblePanel.Controls.Add(this.DiagnosticCriteriaSearchResultsGrid);
			this.DiagnosticCriteriaSearchResultsCollapsiblePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DiagnosticCriteriaSearchResultsCollapsiblePanel.IsCollapsed = false;
			this.DiagnosticCriteriaSearchResultsCollapsiblePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DiagnosticCriteriaSearchResultsCollapsiblePanel.Name = "DiagnosticCriteriaSearchResultsCollapsiblePanel";
			this.DiagnosticCriteriaSearchResultsCollapsiblePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 20, 0, 0, true);
			this.DiagnosticCriteriaSearchResultsCollapsiblePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 133, true);
			this.DiagnosticCriteriaSearchResultsCollapsiblePanel.TabIndex = 6;
			this.DiagnosticCriteriaSearchResultsCollapsiblePanel.Text = "Search results";
			this.DiagnosticCriteriaSearchResultsCollapsiblePanel.SizeChanged += new System.EventHandler(this.DiagnosticPanel_SizeChanged);
			// 
			// DiagnosticCriteriaSearchResultsGrid
			// 
			this.DiagnosticCriteriaSearchResultsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DiagnosticCriteriaSearchResultsGrid, "SearchedCriteriaCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).SearchedCriteriaCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).SearchedCriteriaCollection)).SyncRoot)).DiagnosticCriteria.IMD_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).SearchedCriteriaCollection)).SyncRoot)).DiagnosticCriteria.TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).SearchedCriteriaCollection)).SyncRoot)).Confirm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).SearchedCriteriaCollection)).SyncRoot)).Negate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).SearchedCriteriaCollection)).SyncRoot)).Investigate)));
			this.DiagnosticCriteriaSearchResultsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "DiagnosticCriteria+IMD_Description";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			zTextBoxColumnStyleInfo2.Caption = "Type";
			zTextBoxColumnStyleInfo2.ColumnName = "DiagnosticCriteria+TypeDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zCheckBoxColumnStyleInfo1.ColumnName = "Confirm";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCheckBoxColumnStyleInfo2.ColumnName = "Negate";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCheckBoxColumnStyleInfo3.ColumnName = "Investigate";
			zCheckBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			this.DiagnosticCriteriaSearchResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DiagnosticCriteriaSearchResultsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DiagnosticCriteriaSearchResultsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DiagnosticCriteriaSearchResultsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.DiagnosticCriteriaSearchResultsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.DiagnosticCriteriaSearchResultsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DiagnosticCriteriaSearchResultsGrid.GridId = "d3ec1470-528d-49b4-aaca-f90474657e12";
			this.DiagnosticCriteriaSearchResultsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DiagnosticCriteriaSearchResultsGrid.LayoutKey = "DiagnosticCriteriaSearchResultsGrid";
			this.DiagnosticCriteriaSearchResultsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			this.DiagnosticCriteriaSearchResultsGrid.Name = "DiagnosticCriteriaSearchResultsGrid";
			this.DiagnosticCriteriaSearchResultsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 113, true);
			this.DiagnosticCriteriaSearchResultsGrid.TabIndex = 7;
			this.DiagnosticCriteriaSearchResultsGrid.ColourDeciding += new System.EventHandler<Enterprise.ZArchitecture.ColourDecidingEventArgs>(this.DiagnosticCriteriaGrid_ColourDeciding);
			this.DiagnosticCriteriaSearchResultsGrid.DoubleClick += new System.EventHandler(this.DiagnosticCriteriaGrid_DoubleClick);
			// 
			// DiagnosticCriteriaLinkedCollapsiblePanel
			// 
			this.DiagnosticCriteriaLinkedCollapsiblePanel.Controls.Add(this.DiagnosticCriteriaLinkedGrid);
			this.DiagnosticCriteriaLinkedCollapsiblePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DiagnosticCriteriaLinkedCollapsiblePanel.IsCollapsed = false;
			this.DiagnosticCriteriaLinkedCollapsiblePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 142, true);
			this.DiagnosticCriteriaLinkedCollapsiblePanel.Name = "DiagnosticCriteriaLinkedCollapsiblePanel";
			this.DiagnosticCriteriaLinkedCollapsiblePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 20, 0, 0, true);
			this.DiagnosticCriteriaLinkedCollapsiblePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 133, true);
			this.DiagnosticCriteriaLinkedCollapsiblePanel.TabIndex = 8;
			this.DiagnosticCriteriaLinkedCollapsiblePanel.Text = "Linked";
			this.DiagnosticCriteriaLinkedCollapsiblePanel.SizeChanged += new System.EventHandler(this.DiagnosticPanel_SizeChanged);
			// 
			// DiagnosticCriteriaLinkedGrid
			// 
			this.DiagnosticCriteriaLinkedGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DiagnosticCriteriaLinkedGrid, "LinkedCriteriaCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).LinkedCriteriaCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).LinkedCriteriaCollection)).SyncRoot)).DiagnosticCriteria.IMD_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).LinkedCriteriaCollection)).SyncRoot)).DiagnosticCriteria.TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).LinkedCriteriaCollection)).SyncRoot)).Confirm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).LinkedCriteriaCollection)).SyncRoot)).Negate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).LinkedCriteriaCollection)).SyncRoot)).Investigate)));
			this.DiagnosticCriteriaLinkedGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "DiagnosticCriteria+IMD_Description";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			zTextBoxColumnStyleInfo4.Caption = "Type";
			zTextBoxColumnStyleInfo4.ColumnName = "DiagnosticCriteria+TypeDescription";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zCheckBoxColumnStyleInfo4.ColumnName = "Confirm";
			zCheckBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCheckBoxColumnStyleInfo5.ColumnName = "Negate";
			zCheckBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCheckBoxColumnStyleInfo6.ColumnName = "Investigate";
			zCheckBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			this.DiagnosticCriteriaLinkedGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DiagnosticCriteriaLinkedGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DiagnosticCriteriaLinkedGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.DiagnosticCriteriaLinkedGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.DiagnosticCriteriaLinkedGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.DiagnosticCriteriaLinkedGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DiagnosticCriteriaLinkedGrid.GridId = "7be19140-e124-42d8-86df-0e44a19fa0c6";
			this.DiagnosticCriteriaLinkedGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DiagnosticCriteriaLinkedGrid.LayoutKey = "DiagnosticCriteriaLinkedGrid";
			this.DiagnosticCriteriaLinkedGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			this.DiagnosticCriteriaLinkedGrid.Name = "DiagnosticCriteriaLinkedGrid";
			this.DiagnosticCriteriaLinkedGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 113, true);
			this.DiagnosticCriteriaLinkedGrid.TabIndex = 9;
			this.DiagnosticCriteriaLinkedGrid.ColourDeciding += new System.EventHandler<Enterprise.ZArchitecture.ColourDecidingEventArgs>(this.DiagnosticCriteriaGrid_ColourDeciding);
			this.DiagnosticCriteriaLinkedGrid.DoubleClick += new System.EventHandler(this.DiagnosticCriteriaGrid_DoubleClick);
			// 
			// DiagnosticCriteriaSuggestedCollapsiblePanel
			// 
			this.DiagnosticCriteriaSuggestedCollapsiblePanel.Controls.Add(this.DiagnosticCriteriaSuggestedPanelMain);
			this.DiagnosticCriteriaSuggestedCollapsiblePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DiagnosticCriteriaSuggestedCollapsiblePanel.IsCollapsed = false;
			this.DiagnosticCriteriaSuggestedCollapsiblePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 281, true);
			this.DiagnosticCriteriaSuggestedCollapsiblePanel.Name = "DiagnosticCriteriaSuggestedCollapsiblePanel";
			this.DiagnosticCriteriaSuggestedCollapsiblePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 20, 0, 0, true);
			this.DiagnosticCriteriaSuggestedCollapsiblePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 133, true);
			this.DiagnosticCriteriaSuggestedCollapsiblePanel.TabIndex = 10;
			this.DiagnosticCriteriaSuggestedCollapsiblePanel.Text = "Suggested";
			this.DiagnosticCriteriaSuggestedCollapsiblePanel.SizeChanged += new System.EventHandler(this.DiagnosticPanel_SizeChanged);
			// 
			// DiagnosticCriteriaSuggestedPanelMain
			// 
			this.DiagnosticCriteriaSuggestedPanelMain.BackColor = System.Drawing.SystemColors.Control;
			this.DiagnosticCriteriaSuggestedPanelMain.Controls.Add(this.ShowFocusedSuggestedCriteriaOnlyCheckBox);
			this.DiagnosticCriteriaSuggestedPanelMain.Controls.Add(this.DiagnosticCriteriaSuggestedGrid);
			this.DiagnosticCriteriaSuggestedPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DiagnosticCriteriaSuggestedPanelMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DiagnosticCriteriaSuggestedPanelMain.Name = "DiagnosticCriteriaSuggestedPanelMain";
			this.DiagnosticCriteriaSuggestedPanelMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 300, true);
			this.DiagnosticCriteriaSuggestedPanelMain.TabIndex = 0;
			// 
			// DiagnosticCriteriaSuggestedGrid
			// 
			this.DiagnosticCriteriaSuggestedGrid.AllowNavigation = false;
			this.DiagnosticCriteriaSuggestedGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DiagnosticCriteriaSuggestedGrid, "FilteredSuggestedCriteriaCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).FilteredSuggestedCriteriaCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).FilteredSuggestedCriteriaCollection)).SyncRoot)).DiagnosticCriteria.IMD_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).FilteredSuggestedCriteriaCollection)).SyncRoot)).DiagnosticCriteria.TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).FilteredSuggestedCriteriaCollection)).SyncRoot)).Confirm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).FilteredSuggestedCriteriaCollection)).SyncRoot)).Negate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).FilteredSuggestedCriteriaCollection)).SyncRoot)).Investigate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.RelevantDiagnosticCriteria)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).FilteredSuggestedCriteriaCollection)).SyncRoot)).IsFocused)));
			this.DiagnosticCriteriaSuggestedGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.ColumnName = "DiagnosticCriteria+IMD_Description";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			zTextBoxColumnStyleInfo6.Caption = "Type";
			zTextBoxColumnStyleInfo6.ColumnName = "DiagnosticCriteria+TypeDescription";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zCheckBoxColumnStyleInfo7.ColumnName = "Confirm";
			zCheckBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCheckBoxColumnStyleInfo8.ColumnName = "Negate";
			zCheckBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo8.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zCheckBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCheckBoxColumnStyleInfo9.ColumnName = "Investigate";
			zCheckBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo9.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zCheckBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCheckBoxColumnStyleInfo10.ColumnName = "IsFocused";
			zCheckBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo10.IsReadOnly = true;
			zCheckBoxColumnStyleInfo10.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zCheckBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DiagnosticCriteriaSuggestedGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DiagnosticCriteriaSuggestedGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DiagnosticCriteriaSuggestedGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.DiagnosticCriteriaSuggestedGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.DiagnosticCriteriaSuggestedGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo9);
			this.DiagnosticCriteriaSuggestedGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo10);
			this.DiagnosticCriteriaSuggestedGrid.GridId = "0a2597c3-8b6a-45e1-80b2-4a1ec4623d78";
			this.DiagnosticCriteriaSuggestedGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DiagnosticCriteriaSuggestedGrid.LayoutKey = "DiagnosticCriteriaSuggestedGrid";
			this.DiagnosticCriteriaSuggestedGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.DiagnosticCriteriaSuggestedGrid.Name = "DiagnosticCriteriaSuggestedGrid";
			this.DiagnosticCriteriaSuggestedGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 110, true);
			this.DiagnosticCriteriaSuggestedGrid.TabIndex = 11;
			this.DiagnosticCriteriaSuggestedGrid.ColourDeciding += new System.EventHandler<Enterprise.ZArchitecture.ColourDecidingEventArgs>(this.DiagnosticCriteriaGrid_ColourDeciding);
			this.DiagnosticCriteriaSuggestedGrid.DoubleClick += new System.EventHandler(this.DiagnosticCriteriaGrid_DoubleClick);
			// 
			// DiagnosticGuideCollapsiblePanel
			// 
			this.DiagnosticGuideCollapsiblePanel.Controls.Add(this.DiagnosticGuideTableLayoutPanel);
			this.DiagnosticGuideCollapsiblePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DiagnosticGuideCollapsiblePanel.IsCollapsed = false;
			this.DiagnosticGuideCollapsiblePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 420, true);
			this.DiagnosticGuideCollapsiblePanel.Name = "DiagnosticGuideCollapsiblePanel";
			this.DiagnosticGuideCollapsiblePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 20, 0, 0, true);
			this.DiagnosticGuideCollapsiblePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 263, true);
			this.DiagnosticGuideCollapsiblePanel.TabIndex = 12;
			this.DiagnosticGuideCollapsiblePanel.Text = "Diagnostic Guide";
			this.DiagnosticGuideCollapsiblePanel.SizeChanged += new System.EventHandler(this.DiagnosticPanel_SizeChanged);
			// 
			// DiagnosticGuideTableLayoutPanel
			// 
			this.DiagnosticGuideTableLayoutPanel.BackColor = System.Drawing.SystemColors.Control;
			this.DiagnosticGuideTableLayoutPanel.ColumnCount = 1;
			this.DiagnosticGuideTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.DiagnosticGuideTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20)));
			this.DiagnosticGuideTableLayoutPanel.Controls.Add(this.ClientQuestionGroupBox, 0, 1);
			this.DiagnosticGuideTableLayoutPanel.Controls.Add(this.InternalSupportNoteGroupBox, 0, 0);
			this.DiagnosticGuideTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DiagnosticGuideTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			this.DiagnosticGuideTableLayoutPanel.Name = "DiagnosticGuideTableLayoutPanel";
			this.DiagnosticGuideTableLayoutPanel.RowCount = 2;
			this.DiagnosticGuideTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.DiagnosticGuideTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.DiagnosticGuideTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 243, true);
			this.DiagnosticGuideTableLayoutPanel.TabIndex = 0;
			// 
			// ClientQuestionGroupBox
			// 
			this.ClientQuestionGroupBox.Controls.Add(this.ClientQuestionTextPanel);
			this.ClientQuestionGroupBox.Controls.Add(this.ClientQuestionPopupPanel);
			this.ClientQuestionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClientQuestionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 124, true);
			this.ClientQuestionGroupBox.Name = "ClientQuestionGroupBox";
			this.ClientQuestionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 116, true);
			this.ClientQuestionGroupBox.TabIndex = 1;
			this.ClientQuestionGroupBox.TabStop = false;
			this.ClientQuestionGroupBox.Text = "Client Questions";
			// 
			// ClientQuestionTextPanel
			// 
			this.ClientQuestionTextPanel.Controls.Add(this.ClientQuestionTextBox);
			this.ClientQuestionTextPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClientQuestionTextPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 38, true);
			this.ClientQuestionTextPanel.Name = "ClientQuestionTextPanel";
			this.ClientQuestionTextPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 75, true);
			this.ClientQuestionTextPanel.TabIndex = 3;
			// 
			// ClientQuestionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientQuestionTextBox, "DiagnosticGuideClientQuestionRTF");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).DiagnosticGuideClientQuestionRTF)));
			this.ClientQuestionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClientQuestionTextBox.IsAttachButtonVisible = false;
			this.ClientQuestionTextBox.IsInsertImageButtonVisible = false;
			this.ClientQuestionTextBox.IsPopupButtonVisible = false;
			this.ClientQuestionTextBox.IsToolBarVisible = false;
			this.ClientQuestionTextBox.ReadOnlyCascadeToPopup = true;
			this.ClientQuestionTextBox.ReadOnly = true;
			this.ClientQuestionTextBox.PopupFormCaption = ZClientEDI.Res.GetData("c1d01c3f-f383-4f88-9931-30faff919d21", "Client Questions");
			this.ClientQuestionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClientQuestionTextBox.MaxLength = 10000000;
			this.ClientQuestionTextBox.Name = "ClientQuestionTextBox";
			this.ClientQuestionTextBox.ParentZForm = this;
			this.ClientQuestionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 75, true);
			this.ClientQuestionTextBox.TabIndex = 0;
			// 
			// ClientQuestionPopupPanel
			// 
			this.ClientQuestionPopupPanel.Controls.Add(this.ClientQuestionPopupButton);
			this.ClientQuestionPopupPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ClientQuestionPopupPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ClientQuestionPopupPanel.Name = "ClientQuestionPopupPanel";
			this.ClientQuestionPopupPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 22, true);
			this.ClientQuestionPopupPanel.TabIndex = 2;
			// 
			// ClientQuestionPopupButton
			// 
			this.ClientQuestionPopupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ClientQuestionPopupButton.CaptionResourceString = ZClientEDI.Res.GetData("020accd1-6cf1-4b96-a4e2-1a738cd5c17e", "Popup");
			this.ClientQuestionPopupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(595, 2, true);
			this.ClientQuestionPopupButton.Name = "ClientQuestionPopupButton";
			this.ClientQuestionPopupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 19, true);
			this.ClientQuestionPopupButton.TabIndex = 0;
			this.ClientQuestionPopupButton.ToolTipCaption = null;
			this.ClientQuestionPopupButton.UseVisualStyleBackColor = true;
			this.ClientQuestionPopupButton.Click += new System.EventHandler(this.ClientQuestionPopupButton_Click);
			// 
			// InternalSupportNoteGroupBox
			// 
			this.InternalSupportNoteGroupBox.Controls.Add(this.InternalSupportNoteTextPanel);
			this.InternalSupportNoteGroupBox.Controls.Add(this.InternalSupportNotePopupPanel);
			this.InternalSupportNoteGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InternalSupportNoteGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InternalSupportNoteGroupBox.Name = "InternalSupportNoteGroupBox";
			this.InternalSupportNoteGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 115, true);
			this.InternalSupportNoteGroupBox.TabIndex = 0;
			this.InternalSupportNoteGroupBox.TabStop = false;
			this.InternalSupportNoteGroupBox.Text = "Internal Support Notes";
			// 
			// InternalSupportNoteTextPanel
			// 
			this.InternalSupportNoteTextPanel.Controls.Add(this.InternalSupportNoteTextBox);
			this.InternalSupportNoteTextPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InternalSupportNoteTextPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 38, true);
			this.InternalSupportNoteTextPanel.Name = "InternalSupportNoteTextPanel";
			this.InternalSupportNoteTextPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 74, true);
			this.InternalSupportNoteTextPanel.TabIndex = 2;
			// 
			// InternalSupportNoteTextBox
			// 
			this.BindingSource.SetBindingMember(this.InternalSupportNoteTextBox, "DiagnosticGuideInternalSupportNoteRTF");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).DiagnosticGuideInternalSupportNoteRTF)));
			this.InternalSupportNoteTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InternalSupportNoteTextBox.IsAttachButtonVisible = false;
			this.InternalSupportNoteTextBox.IsInsertImageButtonVisible = false;
			this.InternalSupportNoteTextBox.IsPopupButtonVisible = false;
			this.InternalSupportNoteTextBox.IsToolBarVisible = false;
			this.InternalSupportNoteTextBox.ReadOnlyCascadeToPopup = true;
			this.InternalSupportNoteTextBox.ReadOnly = true;
			this.InternalSupportNoteTextBox.PopupFormCaption = ZClientEDI.Res.GetData("7a030c83-fc8a-47d9-b2f1-c8df3ae9bd8c", "Internal Support Notes");
			this.InternalSupportNoteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InternalSupportNoteTextBox.MaxLength = 10000000;
			this.InternalSupportNoteTextBox.Name = "InternalSupportNoteTextBox";
			this.InternalSupportNoteTextBox.ParentZForm = this;
			this.InternalSupportNoteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 74, true);
			this.InternalSupportNoteTextBox.TabIndex = 0;
			// 
			// InternalSupportNotePopupPanel
			// 
			this.InternalSupportNotePopupPanel.Controls.Add(this.InternalSupportNotePopupButton);
			this.InternalSupportNotePopupPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.InternalSupportNotePopupPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InternalSupportNotePopupPanel.Name = "InternalSupportNotePopupPanel";
			this.InternalSupportNotePopupPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 22, true);
			this.InternalSupportNotePopupPanel.TabIndex = 1;
			// 
			// InternalSupportNotePopupButton
			// 
			this.InternalSupportNotePopupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.InternalSupportNotePopupButton.CaptionResourceString = ZClientEDI.Res.GetData("020accd1-6cf1-4b96-a4e2-1a738cd5c17e", "Popup");
			this.InternalSupportNotePopupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(595, 2, true);
			this.InternalSupportNotePopupButton.Name = "InternalSupportNotePopupButton";
			this.InternalSupportNotePopupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 19, true);
			this.InternalSupportNotePopupButton.TabIndex = 0;
			this.InternalSupportNotePopupButton.ToolTipCaption = null;
			this.InternalSupportNotePopupButton.UseVisualStyleBackColor = true;
			this.InternalSupportNotePopupButton.Click += new System.EventHandler(this.InternalSupportNotePopupButton_Click);
			// 
			// TriageNodeTableLayoutPanel
			// 
			this.TriageNodeTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TriageNodeTableLayoutPanel.ColumnCount = 1;
			this.TriageNodeTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TriageNodeTableLayoutPanel.Controls.Add(this.TriageNodeCollapsiblePanel, 0, 0);
			this.TriageNodeTableLayoutPanel.Controls.Add(this.TriageNodeActionCollapsiblePanel, 0, 1);
			this.TriageNodeTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 65, true);
			this.TriageNodeTableLayoutPanel.Name = "TriageNodeTableLayoutPanel";
			this.TriageNodeTableLayoutPanel.RowCount = 3;
			this.TriageNodeTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TriageNodeTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TriageNodeTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(1)));
			this.TriageNodeTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 686, true);
			this.TriageNodeTableLayoutPanel.TabIndex = 4;
			// 
			// TriageNodeCollapsiblePanel
			// 
			this.TriageNodeCollapsiblePanel.Controls.Add(this.TriageNodePanelMain);
			this.TriageNodeCollapsiblePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TriageNodeCollapsiblePanel.IsCollapsed = false;
			this.TriageNodeCollapsiblePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TriageNodeCollapsiblePanel.Name = "TriageNodeCollapsiblePanel";
			this.TriageNodeCollapsiblePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 20, 0, 0, true);
			this.TriageNodeCollapsiblePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 336, true);
			this.TriageNodeCollapsiblePanel.TabIndex = 0;
			this.TriageNodeCollapsiblePanel.Text = "Triage Nodes";
			this.TriageNodeCollapsiblePanel.SizeChanged += new System.EventHandler(this.TriageNodePanel_SizeChanged);
			// 
			// TriageNodePanelMain
			// 
			this.TriageNodePanelMain.BackColor = System.Drawing.SystemColors.Control;
			this.TriageNodePanelMain.Controls.Add(this.ShowFocusedTriageNodesOnlyCheckBox);
			this.TriageNodePanelMain.Controls.Add(this.FinaliseSelectedButton);
			this.TriageNodePanelMain.Controls.Add(this.TriageNodeTreeView);
			this.TriageNodePanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TriageNodePanelMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			this.TriageNodePanelMain.Name = "TriageNodePanelMain";
			this.TriageNodePanelMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 316, true);
			this.TriageNodePanelMain.TabIndex = 0;
			// 
			// FinaliseSelectedButton
			// 
			this.FinaliseSelectedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.FinaliseSelectedButton.IsCaptionOverridden = true;
			this.FinaliseSelectedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(826, 291, true);
			this.FinaliseSelectedButton.Name = "FinaliseSelectedButton";
			this.FinaliseSelectedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 23, true);
			this.FinaliseSelectedButton.TabIndex = 3;
			this.FinaliseSelectedButton.Text = "Finalise Selected";
			this.FinaliseSelectedButton.ToolTipCaption = null;
			this.FinaliseSelectedButton.UseVisualStyleBackColor = true;
			this.FinaliseSelectedButton.Click += new System.EventHandler(this.FinaliseSelectedButton_Click);
			// 
			// TriageNodeTreeView
			// 
			this.TriageNodeTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TriageNodeTreeView.BackColor = System.Drawing.SystemColors.Window;
			this.TriageNodeTreeView.Columns.Add(this.DescriptionTreeViewColumn);
			this.TriageNodeTreeView.Columns.Add(this.TypeTreeViewColumn);
			this.TriageNodeTreeView.Columns.Add(this.StatusTreeViewColumn);
			this.TriageNodeTreeView.Columns.Add(this.ProductTreeViewColumn);
			this.TriageNodeTreeView.Columns.Add(this.AreaTreeViewColumn);
			this.TriageNodeTreeView.Columns.Add(this.SectionTreeViewColumn);
			this.TriageNodeTreeView.Columns.Add(this.IsFocusedTreeViewColumn);
			this.TriageNodeTreeView.DefaultToolTipProvider = null;
			this.TriageNodeTreeView.DragDropMarkColor = System.Drawing.Color.Black;
			this.TriageNodeTreeView.ElementType = null;
			this.TriageNodeTreeView.LineColor = System.Drawing.SystemColors.ButtonShadow;
			this.TriageNodeTreeView.LineDashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
			this.TriageNodeTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.TriageNodeTreeView.Name = "TriageNodeTreeView";
			this.TriageNodeTreeView.NodeControls.Add(this.DescriptionNodeTextBox);
			this.TriageNodeTreeView.NodeControls.Add(this.TypeNodeTextBox);
			this.TriageNodeTreeView.NodeControls.Add(this.StatusNodeTextBox);
			this.TriageNodeTreeView.NodeControls.Add(this.ProductNodeTextBox);
			this.TriageNodeTreeView.NodeControls.Add(this.AreaNodeTextBox);
			this.TriageNodeTreeView.NodeControls.Add(this.SectionNodeTextBox);
			this.TriageNodeTreeView.NodeControls.Add(this.IsFocusedNodeCheckBox);
			this.TriageNodeTreeView.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20);
			this.TriageNodeTreeView.SelectedNode = null;
			this.TriageNodeTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 265, true);
			this.TriageNodeTreeView.TabIndex = 0;
			this.TriageNodeTreeView.UseColumns = true;
			this.TriageNodeTreeView.NodeMouseDoubleClick += new System.EventHandler<Aga.Controls.Tree.TreeNodeAdvMouseEventArgs>(this.TriageNodeTreeView_NodeMouseDoubleClick);
			// 
			// DescriptionTreeViewColumn
			// 
			this.DescriptionTreeViewColumn.Header = "Description";
			this.DescriptionTreeViewColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.DescriptionTreeViewColumn.TooltipText = null;
			this.DescriptionTreeViewColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(520);
			// 
			// TypeTreeViewColumn
			// 
			this.TypeTreeViewColumn.Header = "Type";
			this.TypeTreeViewColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.TypeTreeViewColumn.TooltipText = null;
			this.TypeTreeViewColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			// 
			// StatusTreeViewColumn
			// 
			this.StatusTreeViewColumn.Header = "Status";
			this.StatusTreeViewColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.StatusTreeViewColumn.TooltipText = null;
			this.StatusTreeViewColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			// 
			// ProductTreeViewColumn
			// 
			this.ProductTreeViewColumn.Header = "Product";
			this.ProductTreeViewColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.ProductTreeViewColumn.TooltipText = null;
			this.ProductTreeViewColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			// 
			// AreaTreeViewColumn
			// 
			this.AreaTreeViewColumn.Header = "Area";
			this.AreaTreeViewColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.AreaTreeViewColumn.TooltipText = null;
			this.AreaTreeViewColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			// 
			// SectionTreeViewColumn
			// 
			this.SectionTreeViewColumn.Header = "Section";
			this.SectionTreeViewColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.SectionTreeViewColumn.TooltipText = null;
			this.SectionTreeViewColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			// 
			// IsFocusedTreeViewColumn
			// 
			this.IsFocusedTreeViewColumn.Header = "In Focus";
			this.IsFocusedTreeViewColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.IsFocusedTreeViewColumn.TooltipText = null;
			this.IsFocusedTreeViewColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			this.IsFocusedTreeViewColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// DescriptionNodeTextBox
			// 
			this.DescriptionNodeTextBox.DataPropertyName = "Description";
			this.DescriptionNodeTextBox.IncrementalSearchEnabled = true;
			this.DescriptionNodeTextBox.LeftMargin = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(3);
			this.DescriptionNodeTextBox.ParentColumn = this.DescriptionTreeViewColumn;
			this.DescriptionNodeTextBox.DrawText += new System.EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(this.TreeNodeTextBox_DrawText);
			// 
			// TypeNodeTextBox
			// 
			this.TypeNodeTextBox.DataPropertyName = "Type";
			this.TypeNodeTextBox.IncrementalSearchEnabled = true;
			this.TypeNodeTextBox.LeftMargin = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(3);
			this.TypeNodeTextBox.ParentColumn = this.TypeTreeViewColumn;
			this.TypeNodeTextBox.DrawText += new System.EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(this.TreeNodeTextBox_DrawText);
			// 
			// StatusNodeTextBox
			// 
			this.StatusNodeTextBox.DataPropertyName = "Status";
			this.StatusNodeTextBox.IncrementalSearchEnabled = true;
			this.StatusNodeTextBox.LeftMargin = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(3);
			this.StatusNodeTextBox.ParentColumn = this.StatusTreeViewColumn;
			this.StatusNodeTextBox.DrawText += new System.EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(this.TreeNodeTextBox_DrawText);
			// 
			// ProductNodeTextBox
			// 
			this.ProductNodeTextBox.DataPropertyName = "Product";
			this.ProductNodeTextBox.IncrementalSearchEnabled = true;
			this.ProductNodeTextBox.LeftMargin = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(3);
			this.ProductNodeTextBox.ParentColumn = this.ProductTreeViewColumn;
			this.ProductNodeTextBox.DrawText += new System.EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(this.TreeNodeTextBox_DrawText);
			// 
			// AreaNodeTextBox
			// 
			this.AreaNodeTextBox.DataPropertyName = "Area";
			this.AreaNodeTextBox.IncrementalSearchEnabled = true;
			this.AreaNodeTextBox.LeftMargin = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(3);
			this.AreaNodeTextBox.ParentColumn = this.AreaTreeViewColumn;
			this.AreaNodeTextBox.DrawText += new System.EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(this.TreeNodeTextBox_DrawText);
			// 
			// SectionNodeTextBox
			// 
			this.SectionNodeTextBox.DataPropertyName = "Section";
			this.SectionNodeTextBox.IncrementalSearchEnabled = true;
			this.SectionNodeTextBox.LeftMargin = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(3);
			this.SectionNodeTextBox.ParentColumn = this.SectionTreeViewColumn;
			this.SectionNodeTextBox.DrawText += new System.EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(this.TreeNodeTextBox_DrawText);
			// 
			// IsFocusedNodeCheckBox
			// 
			this.IsFocusedNodeCheckBox.DataPropertyName = "IsFocused";
			this.IsFocusedNodeCheckBox.IncrementalSearchEnabled = true;
			this.IsFocusedNodeCheckBox.LeftMargin = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(3);
			this.IsFocusedNodeCheckBox.ParentColumn = this.IsFocusedTreeViewColumn;
			this.IsFocusedNodeCheckBox.VerticalAlign = Aga.Controls.Tree.VerticalAlignment.Center;
			// 
			// TriageNodeActionCollapsiblePanel
			// 
			this.TriageNodeActionCollapsiblePanel.Controls.Add(this.TriageNodeActionTableLayoutPanel);
			this.TriageNodeActionCollapsiblePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TriageNodeActionCollapsiblePanel.IsCollapsed = false;
			this.TriageNodeActionCollapsiblePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 345, true);
			this.TriageNodeActionCollapsiblePanel.Name = "TriageNodeActionCollapsiblePanel";
			this.TriageNodeActionCollapsiblePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 20, 0, 0, true);
			this.TriageNodeActionCollapsiblePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 336, true);
			this.TriageNodeActionCollapsiblePanel.TabIndex = 1;
			this.TriageNodeActionCollapsiblePanel.Text = "Actions for finalized triage node";
			this.TriageNodeActionCollapsiblePanel.SizeChanged += new System.EventHandler(this.TriageNodePanel_SizeChanged);
			// 
			// TriageNodeActionTableLayoutPanel
			// 
			this.TriageNodeActionTableLayoutPanel.BackColor = System.Drawing.SystemColors.Control;
			this.TriageNodeActionTableLayoutPanel.ColumnCount = 1;
			this.TriageNodeActionTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TriageNodeActionTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20)));
			this.TriageNodeActionTableLayoutPanel.Controls.Add(this.TriageNodeClientMessageGroupBox, 0, 1);
			this.TriageNodeActionTableLayoutPanel.Controls.Add(this.TriageNodeInternalSupportActionGroupBox, 0, 0);
			this.TriageNodeActionTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TriageNodeActionTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			this.TriageNodeActionTableLayoutPanel.Name = "TriageNodeActionTableLayoutPanel";
			this.TriageNodeActionTableLayoutPanel.RowCount = 2;
			this.TriageNodeActionTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TriageNodeActionTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TriageNodeActionTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 316, true);
			this.TriageNodeActionTableLayoutPanel.TabIndex = 1;
			// 
			// TriageNodeClientMessageGroupBox
			// 
			this.TriageNodeClientMessageGroupBox.Controls.Add(this.TriageNodeClientMessageTextBox);
			this.TriageNodeClientMessageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TriageNodeClientMessageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 161, true);
			this.TriageNodeClientMessageGroupBox.Name = "TriageNodeClientMessageGroupBox";
			this.TriageNodeClientMessageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(951, 152, true);
			this.TriageNodeClientMessageGroupBox.TabIndex = 2;
			this.TriageNodeClientMessageGroupBox.TabStop = false;
			this.TriageNodeClientMessageGroupBox.Text = "Client Message";
			// 
			// TriageNodeClientMessageTextBox
			// 
			this.BindingSource.SetBindingMember(this.TriageNodeClientMessageTextBox, "FinalisedTriageNodeClientMessageRTF");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).FinalisedTriageNodeClientMessageRTF)));
			this.TriageNodeClientMessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TriageNodeClientMessageTextBox.IsAttachButtonVisible = false;
			this.TriageNodeClientMessageTextBox.IsInsertImageButtonVisible = false;
			this.TriageNodeClientMessageTextBox.IsPopupButtonVisible = false;
			this.TriageNodeClientMessageTextBox.IsToolBarVisible = false;
			this.TriageNodeClientMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TriageNodeClientMessageTextBox.MaxLength = 2147483647;
			this.TriageNodeClientMessageTextBox.Name = "TriageNodeClientMessageTextBox";
			this.TriageNodeClientMessageTextBox.ParentZForm = this;
			this.TriageNodeClientMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(945, 133, true);
			this.TriageNodeClientMessageTextBox.TabIndex = 0;
			// 
			// TriageNodeInternalSupportActionGroupBox
			// 
			this.TriageNodeInternalSupportActionGroupBox.Controls.Add(this.TriageNodeInternalSupportActionTextBox);
			this.TriageNodeInternalSupportActionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TriageNodeInternalSupportActionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TriageNodeInternalSupportActionGroupBox.Name = "TriageNodeInternalSupportActionGroupBox";
			this.TriageNodeInternalSupportActionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(951, 152, true);
			this.TriageNodeInternalSupportActionGroupBox.TabIndex = 1;
			this.TriageNodeInternalSupportActionGroupBox.TabStop = false;
			this.TriageNodeInternalSupportActionGroupBox.Text = "Internal Support Actions";
			// 
			// TriageNodeInternalSupportActionTextBox
			// 
			this.BindingSource.SetBindingMember(this.TriageNodeInternalSupportActionTextBox, "FinalisedTriageNodeInternalSupportActionRTF");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).FinalisedTriageNodeInternalSupportActionRTF)));
			this.TriageNodeInternalSupportActionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TriageNodeInternalSupportActionTextBox.IsAttachButtonVisible = false;
			this.TriageNodeInternalSupportActionTextBox.IsInsertImageButtonVisible = false;
			this.TriageNodeInternalSupportActionTextBox.IsPopupButtonVisible = false;
			this.TriageNodeInternalSupportActionTextBox.IsToolBarVisible = false;
			this.TriageNodeInternalSupportActionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TriageNodeInternalSupportActionTextBox.MaxLength = 2147483647;
			this.TriageNodeInternalSupportActionTextBox.Name = "TriageNodeInternalSupportActionTextBox";
			this.TriageNodeInternalSupportActionTextBox.ParentZForm = this;
			this.TriageNodeInternalSupportActionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(945, 133, true);
			this.TriageNodeInternalSupportActionTextBox.TabIndex = 0;
			// 
			// TriageNodeGroupBox
			// 
			this.TriageNodeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TriageNodeGroupBox.Controls.Add(this.TriageNodeFindBox);
			this.TriageNodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TriageNodeGroupBox.Name = "TriageNodeGroupBox";
			this.TriageNodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 56, true);
			this.TriageNodeGroupBox.TabIndex = 3;
			this.TriageNodeGroupBox.TabStop = false;
			this.TriageNodeGroupBox.Text = "Triage Node";
			// 
			// TriageNodeFindBox
			// 
			this.TriageNodeFindBox.AllowDrop = true;
			this.TriageNodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TriageNodeFindBox, "Parent.TriagePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).Parent.TriagePK)));
			this.TriageNodeFindBox.CaptionResourceString = ZClientEDI.Res.GetData("60f97ce3-8582-44b1-843c-28f93f14b534", "Finalized Triage Node:");
			this.TriageNodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 20, true);
			this.TriageNodeFindBox.Name = "TriageNodeFindBox";
			this.TriageNodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TriageNodeFindBox.ParentType = null;
			this.TriageNodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(829, 20, true);
			this.TriageNodeFindBox.TabIndex = 0;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 759, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1675, 25, true);
			this.PostingButtonsUserControl.TabIndex = 14;
			// 
			// ShowFocusedSuggestedCriteriaOnlyCheckBox
			// 
			this.ShowFocusedSuggestedCriteriaOnlyCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShowFocusedSuggestedCriteriaOnlyCheckBox, "ShowFocusedSuggestedCriteriaOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).ShowFocusedSuggestedCriteriaOnly)));
			this.ShowFocusedSuggestedCriteriaOnlyCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShowFocusedSuggestedCriteriaOnlyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(529, 3, true);
			this.ShowFocusedSuggestedCriteriaOnlyCheckBox.Name = "ShowFocusedSuggestedCriteriaOnlyCheckBox";
			this.ShowFocusedSuggestedCriteriaOnlyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 17, true);
			this.ShowFocusedSuggestedCriteriaOnlyCheckBox.TabIndex = 12;
			this.ShowFocusedSuggestedCriteriaOnlyCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ShowFocusedSuggestedCriteriaOnlyCheckBox.UseVisualStyleBackColor = true;
			// 
			// ShowFocusedTriageNodesOnlyCheckBox
			// 
			this.ShowFocusedTriageNodesOnlyCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShowFocusedTriageNodesOnlyCheckBox, "ShowFocusedTriageNodesOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).ShowFocusedTriageNodesOnly)));
			this.ShowFocusedTriageNodesOnlyCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShowFocusedTriageNodesOnlyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(789, 5, true);
			this.ShowFocusedTriageNodesOnlyCheckBox.Name = "ShowFocusedTriageNodesOnlyCheckBox";
			this.ShowFocusedTriageNodesOnlyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 17, true);
			this.ShowFocusedTriageNodesOnlyCheckBox.TabIndex = 13;
			this.ShowFocusedTriageNodesOnlyCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ShowFocusedTriageNodesOnlyCheckBox.UseVisualStyleBackColor = true;
			// 
			// TriageAssistForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1675, 808, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.MainSplitContainer);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject);
			this.DataSourceTypeName = "Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1675, 808, true);
			this.Name = "TriageAssistForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Triage Assist";
			this.Controls.SetChildIndex(this.MainSplitContainer, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.DiagnosticCriteriaGroupBox.ResumeLayout(false);
			this.DiagnosticCriteriaGroupBox.PerformLayout();
			this.TriageAssistSearchBar.ResumeLayout(true);
			this.TriageAssistSearchBar.PerformLayout();
			this.DiagnosticCriteriaProductDropEdit.ResumeLayout(true);
			this.DiagnosticCriteriaProductDropEdit.PerformLayout();
			this.DiagnosticCriteriaTableLayoutPanel.ResumeLayout(false);
			this.DiagnosticCriteriaTableLayoutPanel.PerformLayout();
			this.DiagnosticCriteriaSearchResultsCollapsiblePanel.ResumeLayout(false);
			this.DiagnosticCriteriaSearchResultsCollapsiblePanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DiagnosticCriteriaSearchResultsGrid)).EndInit();
			this.DiagnosticCriteriaSearchResultsGrid.ResumeLayout(false);
			this.DiagnosticCriteriaSearchResultsGrid.PerformLayout();
			this.DiagnosticCriteriaLinkedCollapsiblePanel.ResumeLayout(false);
			this.DiagnosticCriteriaLinkedCollapsiblePanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DiagnosticCriteriaLinkedGrid)).EndInit();
			this.DiagnosticCriteriaLinkedGrid.ResumeLayout(false);
			this.DiagnosticCriteriaLinkedGrid.PerformLayout();
			this.DiagnosticCriteriaSuggestedCollapsiblePanel.ResumeLayout(false);
			this.DiagnosticCriteriaSuggestedCollapsiblePanel.PerformLayout();
			this.DiagnosticCriteriaSuggestedPanelMain.ResumeLayout(false);
			this.DiagnosticCriteriaSuggestedPanelMain.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DiagnosticCriteriaSuggestedGrid)).EndInit();
			this.DiagnosticCriteriaSuggestedGrid.ResumeLayout(false);
			this.DiagnosticCriteriaSuggestedGrid.PerformLayout();
			this.DiagnosticGuideCollapsiblePanel.ResumeLayout(false);
			this.DiagnosticGuideCollapsiblePanel.PerformLayout();
			this.DiagnosticGuideTableLayoutPanel.ResumeLayout(false);
			this.DiagnosticGuideTableLayoutPanel.PerformLayout();
			this.ClientQuestionGroupBox.ResumeLayout(false);
			this.ClientQuestionGroupBox.PerformLayout();
			this.ClientQuestionTextPanel.ResumeLayout(false);
			this.ClientQuestionTextPanel.PerformLayout();
			this.ClientQuestionTextBox.ResumeLayout(true);
			this.ClientQuestionTextBox.PerformLayout();
			this.ClientQuestionPopupPanel.ResumeLayout(false);
			this.ClientQuestionPopupPanel.PerformLayout();
			this.InternalSupportNoteGroupBox.ResumeLayout(false);
			this.InternalSupportNoteGroupBox.PerformLayout();
			this.InternalSupportNoteTextPanel.ResumeLayout(false);
			this.InternalSupportNoteTextPanel.PerformLayout();
			this.InternalSupportNoteTextBox.ResumeLayout(true);
			this.InternalSupportNoteTextBox.PerformLayout();
			this.InternalSupportNotePopupPanel.ResumeLayout(false);
			this.InternalSupportNotePopupPanel.PerformLayout();
			this.DiagnosticCriteriaGroupBox.ResumeLayout(false);
			this.DiagnosticCriteriaGroupBox.PerformLayout();
			this.SearchTermOperatorDropEdit.ResumeLayout(true);
			this.SearchTermOperatorDropEdit.PerformLayout();
			this.TriageAssistSearchBar.ResumeLayout(true);
			this.TriageAssistSearchBar.PerformLayout();
			this.DiagnosticCriteriaProductDropEdit.ResumeLayout(true);
			this.DiagnosticCriteriaProductDropEdit.PerformLayout();
			this.TriageNodeTableLayoutPanel.ResumeLayout(false);
			this.TriageNodeTableLayoutPanel.PerformLayout();
			this.TriageNodeCollapsiblePanel.ResumeLayout(false);
			this.TriageNodeCollapsiblePanel.PerformLayout();
			this.TriageNodePanelMain.ResumeLayout(false);
			this.TriageNodePanelMain.PerformLayout();
			this.TriageNodeTreeView.ResumeLayout(false);
			this.TriageNodeTreeView.PerformLayout();
			this.TriageNodeActionCollapsiblePanel.ResumeLayout(false);
			this.TriageNodeActionCollapsiblePanel.PerformLayout();
			this.TriageNodeActionTableLayoutPanel.ResumeLayout(false);
			this.TriageNodeActionTableLayoutPanel.PerformLayout();
			this.TriageNodeClientMessageGroupBox.ResumeLayout(false);
			this.TriageNodeClientMessageGroupBox.PerformLayout();
			this.TriageNodeClientMessageTextBox.ResumeLayout(true);
			this.TriageNodeClientMessageTextBox.PerformLayout();
			this.TriageNodeInternalSupportActionGroupBox.ResumeLayout(false);
			this.TriageNodeInternalSupportActionGroupBox.PerformLayout();
			this.TriageNodeInternalSupportActionTextBox.ResumeLayout(true);
			this.TriageNodeInternalSupportActionTextBox.PerformLayout();
			this.TriageNodeGroupBox.ResumeLayout(false);
			this.TriageNodeGroupBox.PerformLayout();
			this.TriageNodeFindBox.ResumeLayout(true);
			this.TriageNodeFindBox.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}



		#endregion

		private CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		private CargoWise.Windows.UI.KTableLayoutPanel DiagnosticCriteriaTableLayoutPanel;
		private Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistCollapsiblePanel DiagnosticCriteriaSearchResultsCollapsiblePanel;
		private Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistCollapsiblePanel DiagnosticCriteriaLinkedCollapsiblePanel;
		private Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistCollapsiblePanel DiagnosticCriteriaSuggestedCollapsiblePanel;
		private Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistCollapsiblePanel DiagnosticGuideCollapsiblePanel;
		private Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistGrid DiagnosticCriteriaSearchResultsGrid;
		private Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistGrid DiagnosticCriteriaLinkedGrid;
		private Enterprise.Client.EDI.IncidentManager.GUI.TriageAssistGrid DiagnosticCriteriaSuggestedGrid;
		private ZArchitecture.GUI.ZGroupBox DiagnosticCriteriaGroupBox;
		private ZArchitecture.GUI.ZDropEdit DiagnosticCriteriaProductDropEdit;
		private ZArchitecture.ZTextBox SymptomInputTextBox;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZArchitecture.GUI.ZGroupBox TriageNodeGroupBox;
		private ZArchitecture.GUI.ZGuidFindBox TriageNodeFindBox;
		private CargoWise.Windows.UI.KTableLayoutPanel TriageNodeTableLayoutPanel;
		private TriageAssistCollapsiblePanel TriageNodeCollapsiblePanel;
		private TriageAssistCollapsiblePanel TriageNodeActionCollapsiblePanel;
		private Aga.Controls.Tree.TreeColumn DescriptionTreeViewColumn;
		private Aga.Controls.Tree.TreeColumn TypeTreeViewColumn;
		private Aga.Controls.Tree.TreeColumn StatusTreeViewColumn;
		private Aga.Controls.Tree.TreeColumn ProductTreeViewColumn;
		private Aga.Controls.Tree.TreeColumn AreaTreeViewColumn;
		private Aga.Controls.Tree.TreeColumn SectionTreeViewColumn;
		private Aga.Controls.Tree.TreeColumn IsFocusedTreeViewColumn;
		private Aga.Controls.Tree.NodeControls.NodeTextBox DescriptionNodeTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox TypeNodeTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox StatusNodeTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox ProductNodeTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox AreaNodeTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox SectionNodeTextBox;
		private Aga.Controls.Tree.NodeControls.NodeCheckBox IsFocusedNodeCheckBox;
		private CargoWise.Windows.UI.KTableLayoutPanel DiagnosticGuideTableLayoutPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel TriageNodeActionTableLayoutPanel;
		private ZArchitecture.GUI.ZGroupBox InternalSupportNoteGroupBox;
		private ZArchitecture.GUI.ZRichTextBox InternalSupportNoteTextBox;
		private ZArchitecture.GUI.ZGroupBox ClientQuestionGroupBox;
		private ZArchitecture.GUI.ZRichTextBox ClientQuestionTextBox;
		private ZArchitecture.GUI.ZGroupBox TriageNodeClientMessageGroupBox;
		private ZArchitecture.GUI.ZRichTextBox TriageNodeClientMessageTextBox;
		private ZArchitecture.GUI.ZGroupBox TriageNodeInternalSupportActionGroupBox;
		private ZArchitecture.GUI.ZRichTextBox TriageNodeInternalSupportActionTextBox;
		private ZArchitecture.GUI.ZPanel TriageNodePanelMain;
		private ZArchitecture.GUI.ZPanel DiagnosticCriteriaSuggestedPanelMain;
		private ZArchitecture.GUI.ZButton FinaliseSelectedButton;
		private ZArchitecture.GUI.ZTreeViewAdv TriageNodeTreeView;
		private ZArchitecture.GUI.ZPanel InternalSupportNoteTextPanel;
		private ZArchitecture.GUI.ZPanel InternalSupportNotePopupPanel;
		private ZArchitecture.GUI.ZButton InternalSupportNotePopupButton;
		private ZArchitecture.GUI.ZPanel ClientQuestionPopupPanel;
		private ZArchitecture.GUI.ZButton ClientQuestionPopupButton;
		private ZArchitecture.GUI.ZPanel ClientQuestionTextPanel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private ZArchitecture.GUI.ZCheckBox ShowFocusedObjectsOnlyCheckBox;
		private ZArchitecture.GUI.ZCheckBox ShowFocusedSuggestedCriteriaOnlyCheckBox;
		private ZArchitecture.GUI.ZCheckBox ShowFocusedTriageNodesOnlyCheckBox;
		private TriageAssistSearchBarUserControl TriageAssistSearchBar;
		private ZArchitecture.GUI.ZDropEdit SearchTermOperatorDropEdit;
	}
}
