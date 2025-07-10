using System.Windows.Forms;

using Enterprise.ZArchitecture.GUI;
namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap
{
	partial class MapTreeForm
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

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.F))
			{
				mapTreeUserControl.ShowFindForm();
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.tabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.macrosTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.macroGrid = new Enterprise.ZArchitecture.ZGrid();
			this.macroDescriptionUsageSplitter = new CargoWise.Windows.UI.KSplitter();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.macroDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.dataPropertiesSplitter = new CargoWise.Windows.UI.KSplitter();
			this.bottomRightButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.uxmlStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.selectXmlButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.evaluateMacroButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.actionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.editMacroPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.buttonEvaluateJob = new Enterprise.ZArchitecture.GUI.ZButton();
			this.buttonAddMacro = new Enterprise.ZArchitecture.GUI.ZButton();
			this.textBoxMacro = new Enterprise.ZArchitecture.ZTextBox();
			this.buttonEvaluateMacro = new Enterprise.ZArchitecture.GUI.ZButton();
			this.textBoxEvaluationResult = new Enterprise.ZArchitecture.ZTextBox();
			this.mainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.editMacroSplitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.tabControl.SuspendLayout();
			this.macrosTab.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.macroGrid)).BeginInit();
			this.macroGrid.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.bottomRightButtonPanel.SuspendLayout();
			this.editMacroPanel.SuspendLayout();
			this.mainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 554, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.ValueProviders.DataReflectorValueProviderWrapper);
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.tabControl.Controls.Add(this.macrosTab);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tabControl.Name = "tabControl";
			this.tabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 352, true);
			this.tabControl.TabIndex = 1;
			// 
			// macrosTab
			// 
			this.macrosTab.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MapTreeForm|645c1ddd-6e5f-490c-8463-467dcad993ec", "Macros", "Macros", "Caption for Macros Tab in Data Field Map Form.");
			this.macrosTab.Controls.Add(this.macroGrid);
			this.macrosTab.Controls.Add(this.macroDescriptionUsageSplitter);
			this.macrosTab.Controls.Add(this.zPanel1);
			this.macrosTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 13, true);
			this.macrosTab.Name = "macrosTab";
			this.macrosTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.macrosTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 334, true);
			this.macrosTab.TabIndex = 1;
			this.macrosTab.UseVisualStyleBackColor = true;
			// 
			// macroGrid
			// 
			this.macroGrid.AllowDragDropWithChanges = false;
			this.macroGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.macroGrid, "ValueProviderMap.Macros");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.ValueProviders.DataReflectorValueProviderWrapper)(null)).ValueProviderMap.Macros)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.ValueProviders.MacroValueProviderMap)(((System.Collections.IList)(((Enterprise.DocumentEngine.ValueProviders.DataReflectorValueProviderWrapper)(null)).ValueProviderMap.Macros)).SyncRoot)).Usage)));
			this.macroGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MapTreeForm|512c5ab7-9823-4a8d-9c6b-9d57330f41b0", "Macro", "Macro", "");
			zTextBoxColumnStyleInfo1.ColumnName = "Usage";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(700);
			this.macroGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.macroGrid.CopySelectedRowsAllowed = false;
			this.macroGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.macroGrid.GridId = "fc17f179-2091-4cd1-8484-b8ae367fbc19";
			this.macroGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.macroGrid.IsWholeRowSelectedOnClick = true;
			this.macroGrid.LayoutKey = "macroGrid";
			this.macroGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.macroGrid.Name = "macroGrid";
			this.macroGrid.ReadOnly = true;
			this.macroGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.macroGrid.RowHeadersVisible = false;
			this.macroGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 211, true);
			this.macroGrid.TabIndex = 0;
			this.macroGrid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MacroGrid_MouseDown);
			// 
			// macroDescriptionUsageSplitter
			// 
			this.macroDescriptionUsageSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.macroDescriptionUsageSplitter.DoNotSaveSplitterLayout = false;
			this.macroDescriptionUsageSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 227, true);
			this.macroDescriptionUsageSplitter.Name = "macroDescriptionUsageSplitter";
			this.macroDescriptionUsageSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 4, true);
			this.macroDescriptionUsageSplitter.TabIndex = 7;
			this.macroDescriptionUsageSplitter.TabStop = false;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.macroDescriptionTextBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 231, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 100, true);
			this.zPanel1.TabIndex = 6;
			// 
			// macroDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.macroDescriptionTextBox, "ValueProviderMap.Macros.Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.ValueProviders.MacroValueProviderMap)(((System.Collections.IList)(((Enterprise.DocumentEngine.ValueProviders.DataReflectorValueProviderWrapper)(null)).ValueProviderMap.Macros)).SyncRoot)).Description)));
			this.macroDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.macroDescriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.macroDescriptionTextBox, false);
			this.macroDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.macroDescriptionTextBox.Multiline = true;
			this.macroDescriptionTextBox.Name = "macroDescriptionTextBox";
			this.macroDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.macroDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 100, true);
			this.macroDescriptionTextBox.TabIndex = 1;
			// 
			// dataPropertiesSplitter
			// 
			this.dataPropertiesSplitter.Dock = System.Windows.Forms.DockStyle.Right;
			this.dataPropertiesSplitter.DoNotSaveSplitterLayout = false;
			this.dataPropertiesSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 3, true);
			this.dataPropertiesSplitter.Name = "dataPropertiesSplitter";
			this.dataPropertiesSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(4, 328, true);
			this.dataPropertiesSplitter.TabIndex = 7;
			this.dataPropertiesSplitter.TabStop = false;
			// 
			// bottomRightButtonPanel
			// 
			this.bottomRightButtonPanel.Controls.Add(this.uxmlStatusLabel);
			this.bottomRightButtonPanel.Controls.Add(this.selectXmlButton);
			this.bottomRightButtonPanel.Controls.Add(this.evaluateMacroButton);
			this.bottomRightButtonPanel.Controls.Add(this.actionButton);
			this.bottomRightButtonPanel.Controls.Add(this.closeButton);
			this.bottomRightButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomRightButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 523, true);
			this.bottomRightButtonPanel.Name = "bottomRightButtonPanel";
			this.bottomRightButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 31, true);
			this.bottomRightButtonPanel.TabIndex = 4;
			// 
			// uxmlStatusLabel
			// 
			this.uxmlStatusLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MapTreeForm|8b2ccc27-2a4c-4f8a-b2ca-d42f171c1fed", "UXML Not Loaded");
			this.uxmlStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.uxmlStatusLabel.ForeColor = System.Drawing.Color.Red;
			this.uxmlStatusLabel.IsFontBold = true;
			this.uxmlStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 6, true);
			this.uxmlStatusLabel.Name = "uxmlStatusLabel";
			this.uxmlStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 23, true);
			this.uxmlStatusLabel.TabIndex = 5;
			this.uxmlStatusLabel.UseMnemonic = false;
			this.uxmlStatusLabel.Visible = false;
			// 
			// selectXmlButton
			// 
			this.selectXmlButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MapTreeForm|e81e1bba-873b-46e1-ae4b-3772795bf374", "&Choose UXML...", "Will let you choose an XML file containing the UXML message to act as XML data source for evaluation.");
			this.selectXmlButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 6, true);
			this.selectXmlButton.Name = "selectXmlButton";
			this.selectXmlButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 23, true);
			this.selectXmlButton.TabIndex = 1;
			this.selectXmlButton.ToolTipCaption = null;
			this.selectXmlButton.UseVisualStyleBackColor = true;
			this.selectXmlButton.Visible = false;
			this.selectXmlButton.Click += new System.EventHandler(this.selectXmlButton_Click);
			// 
			// evaluateMacroButton
			// 
			this.evaluateMacroButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.evaluateMacroButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MapTreeForm|3dffe09a-c3e5-40c5-b9a7-abd6f764c6a9", "&Evaluate in New Window", "Evaluate the Macro expression in New Form");
			this.evaluateMacroButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 6, true);
			this.evaluateMacroButton.Name = "evaluateMacroButton";
			this.evaluateMacroButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 23, true);
			this.evaluateMacroButton.TabIndex = 4;
			this.evaluateMacroButton.ToolTipCaption = null;
			this.evaluateMacroButton.UseVisualStyleBackColor = true;
			this.evaluateMacroButton.Click += new System.EventHandler(this.evaluateMacroButton_Click);
			// 
			// actionButton
			// 
			this.actionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.actionButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MapTreeForm|88a53a09-dcaf-4c49-8738-10c0bbb5e477", "&Select", "Will copy the currently selected macro back to the selected location in the previous form and close this form.");
			this.actionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 6, true);
			this.actionButton.Name = "actionButton";
			this.actionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 23, true);
			this.actionButton.TabIndex = 2;
			this.actionButton.ToolTipCaption = null;
			this.actionButton.UseVisualStyleBackColor = true;
			this.actionButton.Click += new System.EventHandler(this.actionButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MapTreeForm|05d58a95-26f2-4c89-93b8-f6cbc29b77ff", "&Close", "Close the form.");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(664, 6, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 23, true);
			this.closeButton.TabIndex = 3;
			this.closeButton.ToolTipCaption = null;
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// editMacroPanel
			// 
			this.editMacroPanel.Controls.Add(this.buttonEvaluateJob);
			this.editMacroPanel.Controls.Add(this.buttonAddMacro);
			this.editMacroPanel.Controls.Add(this.textBoxMacro);
			this.editMacroPanel.Controls.Add(this.buttonEvaluateMacro);
			this.editMacroPanel.Controls.Add(this.textBoxEvaluationResult);
			this.editMacroPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.editMacroPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 352, true);
			this.editMacroPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 171, true);
			this.editMacroPanel.Name = "editMacroPanel";
			this.editMacroPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 171, true);
			this.editMacroPanel.TabIndex = 6;
			// 
			// buttonEvaluateJob
			// 
			this.buttonEvaluateJob.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.buttonEvaluateJob.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MapTreeForm|A16A5D9D-63FC-4619-A0FE-3D7BE2B981D5", "Evaluate Job", "Evaluate current Macro expression with selected job");
			this.buttonEvaluateJob.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 83, true);
			this.buttonEvaluateJob.Name = "buttonEvaluateJob";
			this.buttonEvaluateJob.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.buttonEvaluateJob.TabIndex = 4;
			this.buttonEvaluateJob.ToolTipCaption = null;
			this.buttonEvaluateJob.UseVisualStyleBackColor = true;
			this.buttonEvaluateJob.Visible = false;
			this.buttonEvaluateJob.Click += new System.EventHandler(this.buttonEvaluateJob_Click);
			// 
			// buttonAddMacro
			// 
			this.buttonAddMacro.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.buttonAddMacro.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MapTreeForm|590b03b1-d197-4dcd-beca-d2fb6fb9c353", "Add", "Add Selected Macro to a Macro expression.");
			this.buttonAddMacro.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 1, true);
			this.buttonAddMacro.Name = "buttonAddMacro";
			this.buttonAddMacro.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.buttonAddMacro.TabIndex = 0;
			this.buttonAddMacro.ToolTipCaption = null;
			this.buttonAddMacro.UseVisualStyleBackColor = true;
			this.buttonAddMacro.Click += new System.EventHandler(this.buttonAddMacro_Click);
			// 
			// textBoxMacro
			// 
			this.textBoxMacro.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxMacro.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MapTreeForm|af6a4b3a-db5b-403a-8919-f360be056ab1", "Macro expression", "Expression combined from single or many macros.");
			this.textBoxMacro.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.textBoxMacro.HideSelection = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.textBoxMacro, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.textBoxMacro.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 30, true);
			this.textBoxMacro.Multiline = true;
			this.textBoxMacro.Name = "textBoxMacro";
			this.textBoxMacro.ResetPosition = false;
			this.textBoxMacro.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxMacro.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 50, true);
			this.textBoxMacro.TabIndex = 1;
			// 
			// buttonEvaluateMacro
			// 
			this.buttonEvaluateMacro.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.buttonEvaluateMacro.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MapTreeForm|E13FBB2A-1D1C-4E17-BA15-D6ABE5753D0C", "Evaluate", "Evaluate current Macro expression.");
			this.buttonEvaluateMacro.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 83, true);
			this.buttonEvaluateMacro.Name = "buttonEvaluateMacro";
			this.buttonEvaluateMacro.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.buttonEvaluateMacro.TabIndex = 2;
			this.buttonEvaluateMacro.ToolTipCaption = null;
			this.buttonEvaluateMacro.UseVisualStyleBackColor = true;
			this.buttonEvaluateMacro.Click += new System.EventHandler(this.buttonEvaluateMacro_Click);
			// 
			// textBoxEvaluationResult
			// 
			this.textBoxEvaluationResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxEvaluationResult.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MapTreeForm|F6F1AB51-2D48-4EF3-BDA6-67B7EB6E89F3", "Evaluation Result");
			this.textBoxEvaluationResult.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.textBoxEvaluationResult.HideSelection = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.textBoxEvaluationResult, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.textBoxEvaluationResult.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 115, true);
			this.textBoxEvaluationResult.Multiline = true;
			this.textBoxEvaluationResult.Name = "textBoxEvaluationResult";
			this.textBoxEvaluationResult.ResetPosition = false;
			this.textBoxEvaluationResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxEvaluationResult.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 50, true);
			this.textBoxEvaluationResult.TabIndex = 3;
			// 
			// mainPanel
			// 
			this.mainPanel.Controls.Add(this.tabControl);
			this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 352, true);
			this.mainPanel.TabIndex = 6;
			// 
			// editMacroSplitter
			// 
			this.editMacroSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.editMacroSplitter.DoNotSaveSplitterLayout = false;
			this.editMacroSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 348, true);
			this.editMacroSplitter.Name = "editMacroSplitter";
			this.editMacroSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 4, true);
			this.editMacroSplitter.TabIndex = 8;
			this.editMacroSplitter.TabStop = false;
			// 
			// MapTreeForm
			// 
			this.AcceptButton = this.actionButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.closeButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 578, true);
			this.Controls.Add(this.editMacroSplitter);
			this.Controls.Add(this.mainPanel);
			this.Controls.Add(this.editMacroPanel);
			this.Controls.Add(this.bottomRightButtonPanel);
			this.DataSourceType = typeof(Enterprise.DocumentEngine.ValueProviders.DataReflectorValueProviderWrapper);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 572, true);
			this.Name = "MapTreeForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Load += new System.EventHandler(this.MapTreeForm_Load);
			this.Resize += new System.EventHandler(this.MapTreeForm_Resize);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomRightButtonPanel, 0);
			this.Controls.SetChildIndex(this.editMacroPanel, 0);
			this.Controls.SetChildIndex(this.mainPanel, 0);
			this.Controls.SetChildIndex(this.editMacroSplitter, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.tabControl.ResumeLayout(false);
			this.tabControl.PerformLayout();
			this.macrosTab.ResumeLayout(false);
			this.macrosTab.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.macroGrid)).EndInit();
			this.macroGrid.ResumeLayout(false);
			this.macroGrid.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.bottomRightButtonPanel.ResumeLayout(false);
			this.bottomRightButtonPanel.PerformLayout();
			this.editMacroPanel.ResumeLayout(false);
			this.editMacroPanel.PerformLayout();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZTabControl tabControl;
		public Enterprise.ZArchitecture.GUI.ZTabPage dataPropertiesTab;
		public Enterprise.ZArchitecture.GUI.ZTabPage macrosTab;
		internal Enterprise.ZArchitecture.GUI.ZTabPage variablesTab;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
		private Enterprise.ZArchitecture.ZTextBox macroDescriptionTextBox;
		private CargoWise.Windows.UI.KSplitter macroDescriptionUsageSplitter;
		private CargoWise.Windows.UI.KSplitter dataPropertiesSplitter;
		private ZPanel bottomRightButtonPanel;
		internal ZButton actionButton;
		internal ZButton closeButton;
		internal MapTreeUserControl mapTreeUserControl;
		internal MapTreeUserControl variableTreeUserControl;
		private Enterprise.ZArchitecture.ZGrid macroGrid;
		internal ZPanel editMacroPanel;
		private ZPanel mainPanel;
		internal CargoWise.Windows.UI.KSplitter editMacroSplitter;
		internal Enterprise.ZArchitecture.ZTextBox textBoxMacro;
		internal Enterprise.ZArchitecture.ZTextBox textBoxEvaluationResult;
		internal ZButton buttonAddMacro;
		internal ZButton buttonEvaluateMacro;
		internal ZButton evaluateMacroButton;
		internal ZButton selectXmlButton;
		private ZArchitecture.ZLabel uxmlStatusLabel;
		internal ZButton buttonEvaluateJob;
	}
}
