#if DEBUG
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.SDF
{
	partial class StmSystemDefinedFieldForm : ZChildForm
	{
		private Enterprise.ZArchitecture.GUI.ZGroupBox ValidationAndDefaultGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit UpperValueCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit LowerValueCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox HintGroupBox;
		private Enterprise.ZArchitecture.ZTextBox HintTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SpecificCountriesGroupBox;
		internal Enterprise.ZArchitecture.ZGrid SpecificCountriesGrid;
		internal Enterprise.ZArchitecture.ZGrid FieldsGrid;
		internal Enterprise.DocumentEngine.GUI.SDF.StmSystemDefinedFieldForm.FixedSizeZDropEdit DefaultValueDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ValidationRuleDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZButton CheckoutButton;
		internal Enterprise.ZArchitecture.GUI.ZButton UndoCheckoutButton;
		internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl FieldTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage FieldColumnsTabPage;
		private Enterprise.ZArchitecture.GUI.ZPanel ButtonsPanel;
		private Enterprise.ZArchitecture.GUI.ZTabPage FieldTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox FieldColumnValidationAndDefaultGroupBox;
		private Enterprise.DocumentEngine.GUI.SDF.StmSystemDefinedFieldForm.FixedSizeZDropEdit FieldColumnDefaultValueDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit FieldColumnValidationRuleDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit FieldColumnUpperValueCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit FieldColumnLowerValueCalcEdit;
		internal Enterprise.ZArchitecture.ZGrid FieldColumnsGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox FieldColumnHintGroupBox;
		private Enterprise.ZArchitecture.ZTextBox FieldColumnHintTextBox;
		private Enterprise.ZArchitecture.ZLabel FieldColumnsDescriptionLabel;
		private Enterprise.ZArchitecture.ZLabel SpecificCountriesLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ForTestingOnlyGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZButton EditWithoutCheckoutButton;
		internal Enterprise.ZArchitecture.GUI.ZButton SaveWithoutCheckInButton;
		internal Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;

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

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule", Justification = "Generated code")]
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo zTranslatableTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo zTranslatableTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FieldTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.FieldTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ValidationAndDefaultGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DefaultValueDropEdit = new Enterprise.DocumentEngine.GUI.SDF.StmSystemDefinedFieldForm.FixedSizeZDropEdit();
			this.ValidationRuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UpperValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LowerValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SpecificCountriesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SpecificCountriesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SpecificCountriesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.HintGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HintTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FieldColumnsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FieldColumnsDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FieldColumnValidationAndDefaultGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FieldColumnDefaultValueDropEdit = new Enterprise.DocumentEngine.GUI.SDF.StmSystemDefinedFieldForm.FixedSizeZDropEdit();
			this.FieldColumnValidationRuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FieldColumnUpperValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FieldColumnLowerValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FieldColumnHintGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FieldColumnHintTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FieldColumnsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ForTestingOnlyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SaveWithoutCheckInButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EditWithoutCheckoutButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CheckoutButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UndoCheckoutButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FieldsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.FieldTabControl.SuspendLayout();
			this.FieldTabPage.SuspendLayout();
			this.ValidationAndDefaultGroupBox.SuspendLayout();
			this.DefaultValueDropEdit.SuspendLayout();
			this.ValidationRuleDropEdit.SuspendLayout();
			this.SpecificCountriesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SpecificCountriesGrid)).BeginInit();
			this.SpecificCountriesGrid.SuspendLayout();
			this.HintGroupBox.SuspendLayout();
			this.FieldColumnsTabPage.SuspendLayout();
			this.FieldColumnValidationAndDefaultGroupBox.SuspendLayout();
			this.FieldColumnDefaultValueDropEdit.SuspendLayout();
			this.FieldColumnValidationRuleDropEdit.SuspendLayout();
			this.FieldColumnHintGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FieldColumnsGrid)).BeginInit();
			this.FieldColumnsGrid.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.ForTestingOnlyGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FieldsGrid)).BeginInit();
			this.FieldsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 635, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 22, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(264);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(265);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.FieldTabControl);
			this.BottomPanel.Controls.Add(this.ButtonsPanel);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 204, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 431, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// FieldTabControl
			// 
			this.FieldTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.FieldTabControl.Controls.Add(this.FieldTabPage);
			this.FieldTabControl.Controls.Add(this.FieldColumnsTabPage);
			this.FieldTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FieldTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FieldTabControl.Name = "FieldTabControl";
			this.FieldTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 342, true);
			this.FieldTabControl.TabIndex = 0;
			// 
			// FieldTabPage
			// 
			this.FieldTabPage.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|9ce8a27c-8dbf-4263-872a-a93fe049a3ae", "Field");
			this.FieldTabPage.Controls.Add(this.ValidationAndDefaultGroupBox);
			this.FieldTabPage.Controls.Add(this.SpecificCountriesGroupBox);
			this.FieldTabPage.Controls.Add(this.HintGroupBox);
			this.FieldTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.FieldTabPage.Name = "FieldTabPage";
			this.FieldTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 320, true);
			this.FieldTabPage.TabIndex = 0;
			// 
			// ValidationAndDefaultGroupBox
			// 
			this.ValidationAndDefaultGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ValidationAndDefaultGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|49cb61fd-155e-46b5-970d-dce11e8cedba", "Validation and Default");
			this.ValidationAndDefaultGroupBox.Controls.Add(this.DefaultValueDropEdit);
			this.ValidationAndDefaultGroupBox.Controls.Add(this.ValidationRuleDropEdit);
			this.ValidationAndDefaultGroupBox.Controls.Add(this.UpperValueCalcEdit);
			this.ValidationAndDefaultGroupBox.Controls.Add(this.LowerValueCalcEdit);
			this.ValidationAndDefaultGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 176, true);
			this.ValidationAndDefaultGroupBox.Name = "ValidationAndDefaultGroupBox";
			this.ValidationAndDefaultGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 67, true);
			this.ValidationAndDefaultGroupBox.TabIndex = 1;
			this.ValidationAndDefaultGroupBox.TabStop = false;
			// 
			// DefaultValueDropEdit
			// 
			this.DefaultValueDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefaultValueDropEdit, "Fields.S1_Default");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).S1_Default)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).Lookups.Defaults)));
			this.DefaultValueDropEdit.BindToList = "Fields.Lookups+Defaults";
			this.DefaultValueDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DefaultValueDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 37, true);
			this.DefaultValueDropEdit.Name = "DefaultValueDropEdit";
			this.DefaultValueDropEdit.PreBoundMaxLength = 16;
			this.DefaultValueDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.DefaultValueDropEdit.TabIndex = 3;
			// 
			// ValidationRuleDropEdit
			// 
			this.ValidationRuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValidationRuleDropEdit, "Fields.S1_Validation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).S1_Validation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).Lookups.Validations)));
			this.ValidationRuleDropEdit.BindToList = "Fields.Lookups+Validations";
			this.ValidationRuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 15, true);
			this.ValidationRuleDropEdit.Name = "ValidationRuleDropEdit";
			this.ValidationRuleDropEdit.PreBoundMaxLength = 3;
			this.ValidationRuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.ValidationRuleDropEdit.TabIndex = 1;
			// 
			// UpperValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.UpperValueCalcEdit, "Fields.S1_UpperValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).S1_UpperValue)));
			this.UpperValueCalcEdit.BindToDecimalPlaces = "Fields.RangeValueDecimalPlaces";
			this.UpperValueCalcEdit.CaptionResourceString = null;
			this.UpperValueCalcEdit.DecimalPlaces = 2;
			this.UpperValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 37, true);
			this.UpperValueCalcEdit.Name = "UpperValueCalcEdit";
			this.UpperValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.UpperValueCalcEdit.TabIndex = 7;
			this.UpperValueCalcEdit.Text = "0.00";
			this.UpperValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LowerValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LowerValueCalcEdit, "Fields.S1_LowerValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).S1_LowerValue)));
			this.LowerValueCalcEdit.BindToDecimalPlaces = "Fields.RangeValueDecimalPlaces";
			this.LowerValueCalcEdit.CaptionResourceString = null;
			this.LowerValueCalcEdit.DecimalPlaces = 2;
			this.LowerValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 15, true);
			this.LowerValueCalcEdit.Name = "LowerValueCalcEdit";
			this.LowerValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.LowerValueCalcEdit.TabIndex = 5;
			this.LowerValueCalcEdit.Text = "0.00";
			this.LowerValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SpecificCountriesGroupBox
			// 
			this.SpecificCountriesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SpecificCountriesGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|a9f8f341-01b6-49a4-b2b0-829ce9ee7caa", "Specific Countries/Regions");
			this.SpecificCountriesGroupBox.Controls.Add(this.SpecificCountriesLabel);
			this.SpecificCountriesGroupBox.Controls.Add(this.SpecificCountriesGrid);
			this.SpecificCountriesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.SpecificCountriesGroupBox.Name = "SpecificCountriesGroupBox";
			this.SpecificCountriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 161, true);
			this.SpecificCountriesGroupBox.TabIndex = 0;
			this.SpecificCountriesGroupBox.TabStop = false;
			// 
			// SpecificCountriesLabel
			// 
			this.SpecificCountriesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SpecificCountriesLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|969bb5a4-e81d-4bc3-88dc-13d128e87d21", "", "If this list is empty, the field will apply to all countries/regions. If you add a country/region, the field will apply only to that country/region. If you add a country/region and mark it as suppressed, the field will apply to all countries/regions except for that one.");
			this.SpecificCountriesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SpecificCountriesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.SpecificCountriesLabel.Name = "SpecificCountriesLabel";
			this.SpecificCountriesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 42, true);
			this.SpecificCountriesLabel.TabIndex = 0;
			// 
			// SpecificCountriesGrid
			// 
			this.SpecificCountriesGrid.AllowNavigation = false;
			this.SpecificCountriesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SpecificCountriesGrid, "Fields.FieldCountries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldCountries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldCountry)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldCountries)).SyncRoot)).S1_RN_NKCntrySpecific)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldCountry)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldCountries)).SyncRoot)).S1_IsSuppressed)));
			this.SpecificCountriesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "S1_RN_NKCntrySpecific";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "S1_IsSuppressed";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SpecificCountriesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.SpecificCountriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.SpecificCountriesGrid.GridId = "c6a2f182-fea4-4fae-abc9-bd5e0d71d3e7";
			this.SpecificCountriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SpecificCountriesGrid.LayoutKey = "zGrid1";
			this.SpecificCountriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 59, true);
			this.SpecificCountriesGrid.Name = "SpecificCountriesGrid";
			this.SpecificCountriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 95, true);
			this.SpecificCountriesGrid.TabIndex = 1;
			// 
			// HintGroupBox
			// 
			this.HintGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.HintGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|f5154578-7c03-478c-b682-eaeac2d24087", "Hint");
			this.HintGroupBox.Controls.Add(this.HintTextBox);
			this.HintGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 250, true);
			this.HintGroupBox.Name = "HintGroupBox";
			this.HintGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 67, true);
			this.HintGroupBox.TabIndex = 2;
			this.HintGroupBox.TabStop = false;
			// 
			// HintTextBox
			// 
			this.HintTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.HintTextBox, "Fields.S1_Hint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).S1_Hint)));
			this.HintTextBox.CaptionResourceString = null;
			this.HintTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HintTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.HintTextBox.Multiline = true;
			this.HintTextBox.Name = "HintTextBox";
			this.HintTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.HintTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 44, true);
			this.HintTextBox.TabIndex = 0;
			// 
			// FieldColumnsTabPage
			// 
			this.FieldColumnsTabPage.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|877dce48-5e69-44c7-9e01-de2770328f05", "Columns");
			this.FieldColumnsTabPage.Controls.Add(this.FieldColumnsDescriptionLabel);
			this.FieldColumnsTabPage.Controls.Add(this.FieldColumnValidationAndDefaultGroupBox);
			this.FieldColumnsTabPage.Controls.Add(this.FieldColumnHintGroupBox);
			this.FieldColumnsTabPage.Controls.Add(this.FieldColumnsGrid);
			this.FieldColumnsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.FieldColumnsTabPage.Name = "FieldColumnsTabPage";
			this.FieldColumnsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 320, true);
			this.FieldColumnsTabPage.TabIndex = 1;
			// 
			// FieldColumnsDescriptionLabel
			// 
			this.FieldColumnsDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FieldColumnsDescriptionLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|21f09a8c-dbc6-4bbb-a049-84dc64e3fdfd", "", "If the field Type is a Grid, you can define the Columns for the Grid here. If you change the Type of the field from a Grid to another value, all Columns will be removed.");
			this.FieldColumnsDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FieldColumnsDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.FieldColumnsDescriptionLabel.Name = "FieldColumnsDescriptionLabel";
			this.FieldColumnsDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 30, true);
			this.FieldColumnsDescriptionLabel.TabIndex = 0;
			// 
			// FieldColumnValidationAndDefaultGroupBox
			// 
			this.FieldColumnValidationAndDefaultGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FieldColumnValidationAndDefaultGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|6d9df08e-0aba-452b-83cc-c9a0a6e4c65e", "Validation and Default");
			this.FieldColumnValidationAndDefaultGroupBox.Controls.Add(this.FieldColumnDefaultValueDropEdit);
			this.FieldColumnValidationAndDefaultGroupBox.Controls.Add(this.FieldColumnValidationRuleDropEdit);
			this.FieldColumnValidationAndDefaultGroupBox.Controls.Add(this.FieldColumnUpperValueCalcEdit);
			this.FieldColumnValidationAndDefaultGroupBox.Controls.Add(this.FieldColumnLowerValueCalcEdit);
			this.FieldColumnValidationAndDefaultGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 176, true);
			this.FieldColumnValidationAndDefaultGroupBox.Name = "FieldColumnValidationAndDefaultGroupBox";
			this.FieldColumnValidationAndDefaultGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 67, true);
			this.FieldColumnValidationAndDefaultGroupBox.TabIndex = 2;
			this.FieldColumnValidationAndDefaultGroupBox.TabStop = false;
			// 
			// FieldColumnDefaultValueDropEdit
			// 
			this.FieldColumnDefaultValueDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FieldColumnDefaultValueDropEdit, "Fields.FieldColumns.S1_Default");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).S1_Default)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).Lookups.Defaults)));
			this.FieldColumnDefaultValueDropEdit.BindToList = "Fields.FieldColumns.Lookups+Defaults";
			this.FieldColumnDefaultValueDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FieldColumnDefaultValueDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 37, true);
			this.FieldColumnDefaultValueDropEdit.Name = "FieldColumnDefaultValueDropEdit";
			this.FieldColumnDefaultValueDropEdit.PreBoundMaxLength = 16;
			this.FieldColumnDefaultValueDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.FieldColumnDefaultValueDropEdit.TabIndex = 3;
			// 
			// FieldColumnValidationRuleDropEdit
			// 
			this.FieldColumnValidationRuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FieldColumnValidationRuleDropEdit, "Fields.FieldColumns.S1_Validation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).S1_Validation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).Lookups.Validations)));
			this.FieldColumnValidationRuleDropEdit.BindToList = "Fields.FieldColumns.Lookups+Validations";
			this.FieldColumnValidationRuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 15, true);
			this.FieldColumnValidationRuleDropEdit.Name = "FieldColumnValidationRuleDropEdit";
			this.FieldColumnValidationRuleDropEdit.PreBoundMaxLength = 3;
			this.FieldColumnValidationRuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.FieldColumnValidationRuleDropEdit.TabIndex = 1;
			// 
			// FieldColumnUpperValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FieldColumnUpperValueCalcEdit, "Fields.FieldColumns.S1_UpperValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).S1_UpperValue)));
			this.FieldColumnUpperValueCalcEdit.BindToDecimalPlaces = "Fields.FieldColumns.RangeValueDecimalPlaces";
			this.FieldColumnUpperValueCalcEdit.CaptionResourceString = null;
			this.FieldColumnUpperValueCalcEdit.DecimalPlaces = 2;
			this.FieldColumnUpperValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 37, true);
			this.FieldColumnUpperValueCalcEdit.Name = "FieldColumnUpperValueCalcEdit";
			this.FieldColumnUpperValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.FieldColumnUpperValueCalcEdit.TabIndex = 7;
			this.FieldColumnUpperValueCalcEdit.Text = "0.00";
			this.FieldColumnUpperValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FieldColumnLowerValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FieldColumnLowerValueCalcEdit, "Fields.FieldColumns.S1_LowerValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).S1_LowerValue)));
			this.FieldColumnLowerValueCalcEdit.BindToDecimalPlaces = "Fields.FieldColumns.RangeValueDecimalPlaces";
			this.FieldColumnLowerValueCalcEdit.CaptionResourceString = null;
			this.FieldColumnLowerValueCalcEdit.DecimalPlaces = 2;
			this.FieldColumnLowerValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 15, true);
			this.FieldColumnLowerValueCalcEdit.Name = "FieldColumnLowerValueCalcEdit";
			this.FieldColumnLowerValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.FieldColumnLowerValueCalcEdit.TabIndex = 5;
			this.FieldColumnLowerValueCalcEdit.Text = "0.00";
			this.FieldColumnLowerValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FieldColumnHintGroupBox
			// 
			this.FieldColumnHintGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FieldColumnHintGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|f2f9990f-94c1-45a9-bfaa-997740392c04", "Hint");
			this.FieldColumnHintGroupBox.Controls.Add(this.FieldColumnHintTextBox);
			this.FieldColumnHintGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 250, true);
			this.FieldColumnHintGroupBox.Name = "FieldColumnHintGroupBox";
			this.FieldColumnHintGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 67, true);
			this.FieldColumnHintGroupBox.TabIndex = 3;
			this.FieldColumnHintGroupBox.TabStop = false;
			// 
			// FieldColumnHintTextBox
			// 
			this.FieldColumnHintTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FieldColumnHintTextBox, "Fields.FieldColumns.S1_Hint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).S1_Hint)));
			this.FieldColumnHintTextBox.CaptionResourceString = null;
			this.FieldColumnHintTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FieldColumnHintTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.FieldColumnHintTextBox.Multiline = true;
			this.FieldColumnHintTextBox.Name = "FieldColumnHintTextBox";
			this.FieldColumnHintTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.FieldColumnHintTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 44, true);
			this.FieldColumnHintTextBox.TabIndex = 0;
			// 
			// FieldColumnsGrid
			// 
			this.FieldColumnsGrid.AllowNavigation = false;
			this.FieldColumnsGrid.AllowSorting = false;
			this.FieldColumnsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FieldColumnsGrid, "Fields.FieldColumns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).S1_OrderColumn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).S1_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).S1_Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).S1_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).Lookups.Types)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).S1_Precision)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).S1_DisplayEditRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldColumn)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).FieldColumns)).SyncRoot)).Lookups.DisplayEditRules)));
			this.FieldColumnsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "S1_OrderColumn";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "S1_Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "S1_Category";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.BindToList = "Lookups.Types";
			zDropEditColumnStyleInfo1.ColumnName = "S1_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "S1_Precision";
			zCalcEditColumnStyleInfo2.Decimals = 1;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.BindToList = "Lookups.DisplayEditRules";
			zDropEditColumnStyleInfo2.ColumnName = "S1_DisplayEditRule";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.FieldColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FieldColumnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FieldColumnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FieldColumnsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.FieldColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.FieldColumnsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.FieldColumnsGrid.GridId = "e9e1924e-9a1e-4674-81fe-5571419f56cc";
			this.FieldColumnsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FieldColumnsGrid.LayoutKey = "zGrid1";
			this.FieldColumnsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 37, true);
			this.FieldColumnsGrid.Name = "FieldColumnsGrid";
			this.FieldColumnsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 131, true);
			this.FieldColumnsGrid.TabIndex = 1;
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.SaveButton);
			this.ButtonsPanel.Controls.Add(this.ForTestingOnlyGroupBox);
			this.ButtonsPanel.Controls.Add(this.CheckoutButton);
			this.ButtonsPanel.Controls.Add(this.UndoCheckoutButton);
			this.ButtonsPanel.Controls.Add(this.CloseButton);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 342, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 89, true);
			this.ButtonsPanel.TabIndex = 1;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|63e4f143-774e-4be9-9532-67bd6a19d653", "Save");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 63, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.SaveButton.TabIndex = 6;
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// ForTestingOnlyGroupBox
			// 
			this.ForTestingOnlyGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ForTestingOnlyGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|d0113918-c797-47ef-894a-e7773e3600f8", "For Testing Only");
			this.ForTestingOnlyGroupBox.Controls.Add(this.SaveWithoutCheckInButton);
			this.ForTestingOnlyGroupBox.Controls.Add(this.EditWithoutCheckoutButton);
			this.ForTestingOnlyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.ForTestingOnlyGroupBox.Name = "ForTestingOnlyGroupBox";
			this.ForTestingOnlyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 52, true);
			this.ForTestingOnlyGroupBox.TabIndex = 0;
			this.ForTestingOnlyGroupBox.TabStop = false;
			// 
			// SaveWithoutCheckInButton
			// 
			this.SaveWithoutCheckInButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveWithoutCheckInButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|b80efba8-d03b-413c-b5ca-f5dd9a3af796", "Save without Check-in");
			this.SaveWithoutCheckInButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 22, true);
			this.SaveWithoutCheckInButton.Name = "SaveWithoutCheckInButton";
			this.SaveWithoutCheckInButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 22, true);
			this.SaveWithoutCheckInButton.TabIndex = 1;
			this.SaveWithoutCheckInButton.ToolTipCaption = null;
			this.SaveWithoutCheckInButton.Click += new System.EventHandler(this.SaveWithoutCheckInButton_Click);
			// 
			// EditWithoutCheckoutButton
			// 
			this.EditWithoutCheckoutButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|0f2b9ab2-b5b3-44f5-9dd2-6869ce8a99a7", "Edit without Checkout");
			this.EditWithoutCheckoutButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 22, true);
			this.EditWithoutCheckoutButton.Name = "EditWithoutCheckoutButton";
			this.EditWithoutCheckoutButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 22, true);
			this.EditWithoutCheckoutButton.TabIndex = 0;
			this.EditWithoutCheckoutButton.ToolTipCaption = null;
			this.EditWithoutCheckoutButton.Click += new System.EventHandler(this.EditWithoutCheckoutButton_Click);
			// 
			// CheckoutButton
			// 
			this.CheckoutButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CheckoutButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|4252c0e2-fdc8-4be3-b3e7-0c138ece7e5e", "Checkout");
			this.CheckoutButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 63, true);
			this.CheckoutButton.Name = "CheckoutButton";
			this.CheckoutButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CheckoutButton.TabIndex = 1;
			this.CheckoutButton.ToolTipCaption = null;
			this.CheckoutButton.Click += new System.EventHandler(this.CheckoutButton_Click);
			// 
			// UndoCheckoutButton
			// 
			this.UndoCheckoutButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.UndoCheckoutButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|ca73aea7-5302-4b22-b98e-09e758575e1e", "Undo Checkout");
			this.UndoCheckoutButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 63, true);
			this.UndoCheckoutButton.Name = "UndoCheckoutButton";
			this.UndoCheckoutButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.UndoCheckoutButton.TabIndex = 2;
			this.UndoCheckoutButton.ToolTipCaption = null;
			this.UndoCheckoutButton.Click += new System.EventHandler(this.UndoCheckoutButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|13146531-bfb1-4fe8-b764-14b9434b0428", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 63, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// FieldsGrid
			// 
			this.FieldsGrid.AllowNavigation = false;
			this.FieldsGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.FieldsGrid, "Fields");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).S1_Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).S1_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).S1_Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).S1_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).Lookups.Types)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).S1_Precision)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).S1_DisplayEditRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedField)(((System.Collections.IList)(((Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager)(null)).Fields)).SyncRoot)).Lookups.DisplayEditRules)));
			this.FieldsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "S1_Order";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTranslatableTextBoxColumnStyleInfo1.ColumnName = "S1_Name";
			zTranslatableTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTranslatableTextBoxColumnStyleInfo2.ColumnName = "S1_Category";
			zTranslatableTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.BindToList = "Lookups.Types";
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "S1_Type";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zCalcEditColumnStyleInfo4.ColumnName = "S1_Precision";
			zCalcEditColumnStyleInfo4.Decimals = 1;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo4.BindToList = "Lookups.DisplayEditRules";
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "S1_DisplayEditRule";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.FieldsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.FieldsGrid.ColumnStyles.Add(zTranslatableTextBoxColumnStyleInfo1);
			this.FieldsGrid.ColumnStyles.Add(zTranslatableTextBoxColumnStyleInfo2);
			this.FieldsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.FieldsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.FieldsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.FieldsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FieldsGrid.GridId = "664f735e-0eb6-48ad-b58e-d8a3106c4bda";
			this.FieldsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FieldsGrid.LayoutKey = "ExportDocumentAttributesGrid";
			this.FieldsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FieldsGrid.Name = "FieldsGrid";
			this.FieldsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 204, true);
			this.FieldsGrid.TabIndex = 0;
			// 
			// StmSystemDefinedFieldForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmSystemDefinedFieldForm|af51f699-a8f7-4e23-9de9-3688a36067a2", "System Defined Fields");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 657, true);
			this.Controls.Add(this.FieldsGrid);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceAssemblyName = "Enterprise.DocumentEngine";
			this.DataSourceType = typeof(Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager);
			this.DataSourceTypeName = "Enterprise.DocumentEngine.SDF.StmSystemDefinedFieldManager";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 642, true);
			this.Name = "StmSystemDefinedFieldForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.FieldsGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.FieldTabControl.ResumeLayout(false);
			this.FieldTabControl.PerformLayout();
			this.FieldTabPage.ResumeLayout(false);
			this.FieldTabPage.PerformLayout();
			this.ValidationAndDefaultGroupBox.ResumeLayout(false);
			this.ValidationAndDefaultGroupBox.PerformLayout();
			this.DefaultValueDropEdit.ResumeLayout(true);
			this.DefaultValueDropEdit.PerformLayout();
			this.ValidationRuleDropEdit.ResumeLayout(true);
			this.ValidationRuleDropEdit.PerformLayout();
			this.SpecificCountriesGroupBox.ResumeLayout(false);
			this.SpecificCountriesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SpecificCountriesGrid)).EndInit();
			this.SpecificCountriesGrid.ResumeLayout(false);
			this.SpecificCountriesGrid.PerformLayout();
			this.HintGroupBox.ResumeLayout(false);
			this.HintGroupBox.PerformLayout();
			this.FieldColumnsTabPage.ResumeLayout(false);
			this.FieldColumnsTabPage.PerformLayout();
			this.FieldColumnValidationAndDefaultGroupBox.ResumeLayout(false);
			this.FieldColumnValidationAndDefaultGroupBox.PerformLayout();
			this.FieldColumnDefaultValueDropEdit.ResumeLayout(true);
			this.FieldColumnDefaultValueDropEdit.PerformLayout();
			this.FieldColumnValidationRuleDropEdit.ResumeLayout(true);
			this.FieldColumnValidationRuleDropEdit.PerformLayout();
			this.FieldColumnHintGroupBox.ResumeLayout(false);
			this.FieldColumnHintGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FieldColumnsGrid)).EndInit();
			this.FieldColumnsGrid.ResumeLayout(false);
			this.FieldColumnsGrid.PerformLayout();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.ForTestingOnlyGroupBox.ResumeLayout(false);
			this.ForTestingOnlyGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FieldsGrid)).EndInit();
			this.FieldsGrid.ResumeLayout(false);
			this.FieldsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
#endif
