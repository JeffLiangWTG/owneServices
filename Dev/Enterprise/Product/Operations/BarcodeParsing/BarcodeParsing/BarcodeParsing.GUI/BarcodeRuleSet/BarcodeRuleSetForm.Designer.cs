using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BarcodeParsing.GUI
{
	partial class BarcodeRuleSetForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.BarcodeParsing.GUI.ZTextBoxColumnStyleWithMaxLengthInfo zTextBoxColumnStyleWithMaxLengthInfo2 = new Enterprise.BarcodeParsing.GUI.ZTextBoxColumnStyleWithMaxLengthInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.HeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RelatedEntityGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SupplierGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BuyerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ParsingRuleComponentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ParsingRuleComponentsGridUserControl = new Enterprise.BarcodeParsing.GUI.BarcodeRuleComponentsUserControl();
			this.ParsingRulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ParsingRulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RulesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RulesTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ParsingRulesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ValidationRulesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ValidationRulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ValidationRulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HeaderGroupBox.SuspendLayout();
			this.RelatedEntityGuidFindBox.SuspendLayout();
			this.SupplierGuidFindBox.SuspendLayout();
			this.BuyerGuidFindBox.SuspendLayout();
			this.ModuleDropEdit.SuspendLayout();
			this.ParsingRuleComponentsGroupBox.SuspendLayout();
			this.ParsingRuleComponentsGridUserControl.SuspendLayout();
			this.ParsingRulesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParsingRulesGrid)).BeginInit();
			this.ParsingRulesGrid.SuspendLayout();
			this.RulesPanel.SuspendLayout();
			this.RulesTabControl.SuspendLayout();
			this.ParsingRulesTabPage.SuspendLayout();
			this.ValidationRulesTabPage.SuspendLayout();
			this.ValidationRulesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ValidationRulesGrid)).BeginInit();
			this.ValidationRulesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			//
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 524, true);
			// 
			// MainTabPage
			//
			this.MainTabPage.Controls.Add(this.RulesPanel);
			this.MainTabPage.Controls.Add(this.HeaderGroupBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 501, true);
			this.MainTabPage.Text = Enterprise.BarcodeParsing.GUI.Res.GetData("D4EB4DB8-E983-4B23-86C9-B06DECB1C43D", "Details").Caption;
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 496, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 524, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BarcodeParsing.Business.BarcodeRuleSet);
			// 
			// HeaderGroupBox
			// 
			this.HeaderGroupBox.CaptionResourceString = Enterprise.BarcodeParsing.GUI.Res.GetData("af63e27a-f2ee-41c9-aed8-0fbdfdc1adc8", "Rule Set");
			this.HeaderGroupBox.Controls.Add(this.RelatedEntityGuidFindBox);
			this.HeaderGroupBox.Controls.Add(this.IsSystemCheckBox);
			this.HeaderGroupBox.Controls.Add(this.SupplierGuidFindBox);
			this.HeaderGroupBox.Controls.Add(this.BuyerGuidFindBox);
			this.HeaderGroupBox.Controls.Add(this.ModuleDropEdit);
			this.HeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.HeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderGroupBox.Name = "HeaderGroupBox";
			this.HeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 125, true);
			this.HeaderGroupBox.TabIndex = 0;
			this.HeaderGroupBox.TabStop = false;
			// 
			// RelatedEntityGuidFindBox
			// 
			this.RelatedEntityGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedEntityGuidFindBox, "BRS_RelatedEntityId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).BRS_RelatedEntityId)));
			this.RelatedEntityGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 97, true);
			this.RelatedEntityGuidFindBox.Name = "RelatedEntityGuidFindBox";
			this.RelatedEntityGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.RelatedEntityGuidFindBox.TabIndex = 3;
			// 
			// IsSystemCheckBox
			// 
			this.IsSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSystemCheckBox, "BRS_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).BRS_IsSystem)));
			this.IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 0, true);
			this.IsSystemCheckBox.Name = "IsSystemCheckBox";
			this.IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 17, true);
			this.IsSystemCheckBox.TabIndex = 3;
			this.IsSystemCheckBox.TabStop = false;
			this.IsSystemCheckBox.UseVisualStyleBackColor = true;
			this.IsSystemCheckBox.Visible = false;
			// 
			// SupplierGuidFindBox
			// 
			this.SupplierGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierGuidFindBox, "BRS_OH_Supplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).BRS_OH_Supplier)));
			this.SupplierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 71, true);
			this.SupplierGuidFindBox.Name = "SupplierGuidFindBox";
			this.SupplierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.SupplierGuidFindBox.TabIndex = 2;
			// 
			// BuyerGuidFindBox
			// 
			this.BuyerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerGuidFindBox, "BRS_OH_Buyer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).BRS_OH_Buyer)));
			this.BuyerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 45, true);
			this.BuyerGuidFindBox.Name = "BuyerGuidFindBox";
			this.BuyerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.BuyerGuidFindBox.TabIndex = 1;
			// 
			// ModuleDropEdit
			// 
			this.ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModuleDropEdit, "BRS_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).BRS_Module)));
			this.ModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 19, true);
			this.ModuleDropEdit.Name = "ModuleDropEdit";
			this.ModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ModuleDropEdit.TabIndex = 0;
			// 
			// ParsingRuleComponentsGroupBox
			// 
			this.ParsingRuleComponentsGroupBox.CaptionResourceString = Enterprise.BarcodeParsing.GUI.Res.GetData("d5043c4b-6473-4787-ae2c-24e835f66e9e", "Components");
			this.ParsingRuleComponentsGroupBox.Controls.Add(this.ParsingRuleComponentsGridUserControl);
			this.ParsingRuleComponentsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ParsingRuleComponentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 217, true);
			this.ParsingRuleComponentsGroupBox.Name = "ParsingRuleComponentsGroupBox";
			this.ParsingRuleComponentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 154, true);
			this.ParsingRuleComponentsGroupBox.TabIndex = 2;
			this.ParsingRuleComponentsGroupBox.TabStop = false;
			// 
			// ParsingRuleComponentsGridUserControl
			// 
			this.ParsingRuleComponentsGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ParsingRuleComponentsGridUserControl, "Rules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BarcodeParsing.Business.BarcodeRule)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).Rules)).SyncRoot)))));
			this.ParsingRuleComponentsGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParsingRuleComponentsGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ParsingRuleComponentsGridUserControl.Name = "ParsingRuleComponentsGridUserControl";
			this.ParsingRuleComponentsGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 135, true);
			this.ParsingRuleComponentsGridUserControl.TabIndex = 0;
			// 
			// ParsingRulesGroupBox
			// 
			this.ParsingRulesGroupBox.CaptionResourceString = Enterprise.BarcodeParsing.GUI.Res.GetData("976FC92D-14D3-48DC-9280-AD7D8736A587", "Rules");
			this.ParsingRulesGroupBox.Controls.Add(this.ParsingRulesGrid);
			this.ParsingRulesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParsingRulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ParsingRulesGroupBox.Name = "ParsingRulesGroupBox";
			this.ParsingRulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 217, true);
			this.ParsingRulesGroupBox.TabIndex = 1;
			this.ParsingRulesGroupBox.TabStop = false;
			// 
			// ParsingRulesGrid
			// 
			this.ParsingRulesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ParsingRulesGrid, "Rules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).Rules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).Rules)).SyncRoot)).BRU_RuleNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).Rules)).SyncRoot)).BRU_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).Rules)).SyncRoot)).TerminatorType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).Rules)).SyncRoot)).BRU_Terminator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).Rules)).SyncRoot)).IsPartialRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).Rules)).SyncRoot)).IsGS1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).Rules)).SyncRoot)).SampleBarcode)));
			this.ParsingRulesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "BRU_RuleNumber";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zTextBoxColumnStyleInfo5.ColumnName = "BRU_Name";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "TerminatorType";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleWithMaxLengthInfo2.ColumnName = "BRU_Terminator";
			zTextBoxColumnStyleWithMaxLengthInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCheckBoxColumnStyleInfo3.ColumnName = "IsPartialRule";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCheckBoxColumnStyleInfo4.ColumnName = "IsGS1";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "SampleBarcode";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.ParsingRulesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ParsingRulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ParsingRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ParsingRulesGrid.ColumnStyles.Add(zTextBoxColumnStyleWithMaxLengthInfo2);
			this.ParsingRulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.ParsingRulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.ParsingRulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ParsingRulesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParsingRulesGrid.GridId = "e0ac9bd5-501b-4e50-84b7-df9c34dd2e8c";
			this.ParsingRulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ParsingRulesGrid.LayoutKey = "ParsingRulesGrid";
			this.ParsingRulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ParsingRulesGrid.Name = "ParsingRulesGrid";
			this.ParsingRulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 198, true);
			this.ParsingRulesGrid.TabIndex = 0;
			// 
			// RulesPanel
			// 
			this.RulesPanel.Controls.Add(this.RulesTabControl);
			this.RulesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RulesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 17, true);
			this.RulesPanel.Name = "RulesPanel";
			this.RulesPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 0, 3, true);
			this.RulesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 380, true);
			this.RulesPanel.TabIndex = 10;
			// 
			// RulesTabControl
			// 
			this.RulesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.RulesTabControl.Controls.Add(this.ParsingRulesTabPage);
			this.RulesTabControl.Controls.Add(this.ValidationRulesTabPage);
			this.RulesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RulesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.RulesTabControl.Name = "RulesTabControl";
			this.RulesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 380, true);
			this.RulesTabControl.TabIndex = 0;
			// 
			// ParsingRulesTabPage
			// 
			this.ParsingRulesTabPage.CaptionResourceString = Enterprise.BarcodeParsing.GUI.Res.GetData("01A35FF4-CD07-418E-9A84-453CF2F9F4CB", "Parsing Rules");
			this.ParsingRulesTabPage.Controls.Add(this.ParsingRulesGroupBox);
			this.ParsingRulesTabPage.Controls.Add(this.ParsingRuleComponentsGroupBox);
			this.ParsingRulesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ParsingRulesTabPage.Name = "ParsingRulesTabPage";
			this.ParsingRulesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 353, true);
			this.ParsingRulesTabPage.TabIndex = 0;
			// 
			// ValidationRulesTabPage
			// 
			this.ValidationRulesTabPage.Controls.Add(this.ValidationRulesGroupBox);
			this.ValidationRulesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.ValidationRulesTabPage.Name = "ValidationRulesTabPage";
			this.ValidationRulesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 58, true);
			this.ValidationRulesTabPage.TabIndex = 1;
			this.ValidationRulesTabPage.Text = Enterprise.BarcodeParsing.GUI.Res.GetData("EA3ECED1-A990-499C-A5E4-F47E83D20290", "Validation Rules").Caption;
			// 
			// ValidationRulesGroupBox
			//
			this.ValidationRulesGroupBox.CaptionResourceString = Enterprise.BarcodeParsing.GUI.Res.GetData("976FC92D-14D3-48DC-9280-AD7D8736A587", "Rules");
			this.ValidationRulesGroupBox.Controls.Add(this.ValidationRulesGrid);
			this.ValidationRulesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationRulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValidationRulesGroupBox.Name = "ValidationRulesGroupBox";
			this.ValidationRulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 58, true);
			this.ValidationRulesGroupBox.TabIndex = 1;
			this.ValidationRulesGroupBox.TabStop = false;
			// 
			// ValidationRulesGrid
			// 
			this.ValidationRulesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ValidationRulesGrid, "ValidationRules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).ValidationRules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeValidationRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).ValidationRules)).SyncRoot)).BVR_Prefix)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeValidationRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).ValidationRules)).SyncRoot)).LengthTypeForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BarcodeParsing.Business.BarcodeValidationRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).ValidationRules)).SyncRoot)).BVR_MinLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BarcodeParsing.Business.BarcodeValidationRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).ValidationRules)).SyncRoot)).BVR_MaxLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeValidationRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).ValidationRules)).SyncRoot)).BVR_Format)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeValidationRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).ValidationRules)).SyncRoot)).BVR_TargetField)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeValidationRule)(((System.Collections.IList)(((Enterprise.BarcodeParsing.Business.BarcodeRuleSet)(null)).ValidationRules)).SyncRoot)).TargetFieldDescription)));
			this.ValidationRulesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "BVR_Prefix";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "LengthTypeForBinding";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "BVR_MinLength";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "BVR_MaxLength";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo6.ColumnName = "BVR_Format";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo7.ColumnName = "BVR_TargetField";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "TargetFieldDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ValidationRulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ValidationRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ValidationRulesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ValidationRulesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ValidationRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ValidationRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.ValidationRulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ValidationRulesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationRulesGrid.GridId = "f02464a1-1e9e-418b-b514-fd45ee392a73";
			this.ValidationRulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ValidationRulesGrid.LayoutKey = "ValidationRulesGrid";
			this.ValidationRulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ValidationRulesGrid.Name = "ValidationRulesGrid";
			this.ValidationRulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 41, true);
			this.ValidationRulesGrid.TabIndex = 0;
			// 
			// BarcodeRuleSetForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 580, true);
			this.DataSourceType = typeof(Enterprise.BarcodeParsing.Business.BarcodeRuleSet);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 618, true);
			this.Name = "BarcodeRuleSetForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HeaderGroupBox.ResumeLayout(false);
			this.HeaderGroupBox.PerformLayout();
			this.RelatedEntityGuidFindBox.ResumeLayout(true);
			this.RelatedEntityGuidFindBox.PerformLayout();
			this.SupplierGuidFindBox.ResumeLayout(true);
			this.SupplierGuidFindBox.PerformLayout();
			this.BuyerGuidFindBox.ResumeLayout(true);
			this.BuyerGuidFindBox.PerformLayout();
			this.ModuleDropEdit.ResumeLayout(true);
			this.ModuleDropEdit.PerformLayout();
			this.ParsingRuleComponentsGroupBox.ResumeLayout(false);
			this.ParsingRuleComponentsGroupBox.PerformLayout();
			this.ParsingRuleComponentsGridUserControl.ResumeLayout(true);
			this.ParsingRuleComponentsGridUserControl.PerformLayout();
			this.ParsingRulesGroupBox.ResumeLayout(false);
			this.ParsingRulesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParsingRulesGrid)).EndInit();
			this.ParsingRulesGrid.ResumeLayout(false);
			this.ParsingRulesGrid.PerformLayout();
			this.RulesPanel.ResumeLayout(false);
			this.RulesPanel.PerformLayout();
			this.RulesTabControl.ResumeLayout(false);
			this.RulesTabControl.PerformLayout();
			this.ParsingRulesTabPage.ResumeLayout(false);
			this.ParsingRulesTabPage.PerformLayout();
			this.ValidationRulesTabPage.ResumeLayout(false);
			this.ValidationRulesTabPage.PerformLayout();
			this.ValidationRulesGroupBox.ResumeLayout(false);
			this.ValidationRulesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ValidationRulesGrid)).EndInit();
			this.ValidationRulesGrid.ResumeLayout(false);
			this.ValidationRulesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox HeaderGroupBox;
		private ZArchitecture.GUI.ZDropEdit ModuleDropEdit;
		protected ZArchitecture.GUI.ZGuidFindBox SupplierGuidFindBox;
		protected ZArchitecture.GUI.ZGuidFindBox BuyerGuidFindBox;
		private ZArchitecture.GUI.ZCheckBox IsSystemCheckBox;
		private ZArchitecture.GUI.ZGroupBox ParsingRuleComponentsGroupBox;
		protected BarcodeRuleComponentsUserControl ParsingRuleComponentsGridUserControl;
		private ZArchitecture.GUI.ZGroupBox ParsingRulesGroupBox;
		protected ZArchitecture.ZGrid ParsingRulesGrid;
		protected ZArchitecture.GUI.ZTabPage ValidationRulesTabPage;
		protected ZArchitecture.GUI.ZGuidFindBox RelatedEntityGuidFindBox;
		private ZArchitecture.GUI.ZGroupBox ValidationRulesGroupBox;
		protected ZArchitecture.ZGrid ValidationRulesGrid;
		protected ZTemplateTabControl RulesTabControl;
		private ZTabPage ParsingRulesTabPage;
		private ZPanel RulesPanel;
	}
}
