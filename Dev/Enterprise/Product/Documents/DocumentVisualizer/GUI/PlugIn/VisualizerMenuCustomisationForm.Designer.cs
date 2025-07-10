namespace Enterprise.DocumentVisualizer.GUI
{
	partial class VisualizerMenuCustomisationForm
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

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo zTranslatableTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo zTranslatableTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.visualizerMenuDetailsTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.visualizerSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.visualizerDeliveryRestrictionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.visualizerDeliveryRestrictionConditionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.visualizerDeliveryRestrictionDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.visualizerPublishedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.visualizerPrimaryDocumentPicker = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.visualizerFilterTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.emailSubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.visualizerAutoDeliveryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.visualizerMenuPathTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.visualizerDocGroupDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TemplatesUsedGroupBox.SuspendLayout();
			this.PivotAndChildMenuTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TemplatesUsedGrid)).BeginInit();
			this.TemplatesUsedGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MenusGrid)).BeginInit();
			this.MenusGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.availableTemplatesGrid)).BeginInit();
			this.availableTemplatesGrid.SuspendLayout();
			this.menuDetailsTab.SuspendLayout();
			this.filterAndDescPanel.SuspendLayout();
			this.documentOptionsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.visualizerMenuDetailsTab.SuspendLayout();
			this.visualizerPrimaryDocumentPicker.SuspendLayout();
			this.visualizerMenuPathTextBox.SuspendLayout();
			this.visualizerDocGroupDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// PivotAndChildMenuTabControl
			// 
			this.PivotAndChildMenuTabControl.Controls.Add(this.visualizerMenuDetailsTab);
			this.PivotAndChildMenuTabControl.Controls.SetChildIndex(this.visualizerMenuDetailsTab, 0);
			this.PivotAndChildMenuTabControl.Controls.SetChildIndex(this.menuDetailsTab, 0);
			// 
			// TemplatesUsedGrid
			// 
			zTextBoxColumnStyleInfo1.ColumnName = "SI_DocumentTitle";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.ColumnName = "SI_IsSystemDefined";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "SI_RT_DocType";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zTextBoxColumnStyleInfo2.ColumnName = "SO_Name";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TemplatesUsedGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TemplatesUsedGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TemplatesUsedGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TemplatesUsedGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			// 
			// MenusGrid
			// 
			zTranslatableTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("VisualizerMenuCustomisationForm|SU_MenuName", "Name");
			zTranslatableTextBoxColumnStyleInfo1.ColumnName = "SU_MenuName";
			zTranslatableTextBoxColumnStyleInfo1.IsMandatory = true;
			zTranslatableTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zCheckBoxColumnStyleInfo2.ColumnName = "SU_IsSystemDefined";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCheckBoxColumnStyleInfo3.ColumnName = "SU_IsPublished";
			zCheckBoxColumnStyleInfo3.IsMandatory = true;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "SU_MenuIndex";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo3.ColumnName = "SU_FilterList";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTranslatableTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("VisualizerMenuCustomisationForm|SU_Hint_Localized", "Description");
			zTranslatableTextBoxColumnStyleInfo2.ColumnName = "SU_Hint";
			zTranslatableTextBoxColumnStyleInfo2.IsVisible = false;
			zTranslatableTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo4.ColumnName = "SU_Purpose";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "SU_ContactType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo4.ColumnName = "SU_PreventAutoDelivery";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo2.BindToList = "DeliveryRestrictionTypeList";
			zDropEditColumnStyleInfo2.ColumnName = "SU_DeliveryRestrictionType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("VisualizerMenuCustomisationForm|6362B0B5-82AC-4E61-AF38-5CD051069F58", "Delivery Restriction Description");
			zTextBoxColumnStyleInfo5.ColumnName = "SU_DeliveryRestrictionDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zMultiControlColumnStyleInfo1.ColumnName = "SU_DeliveryRestrictionMacro";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "DeliveryRestrictionConditionFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.MenusGrid.ColumnStyles.Add(zTranslatableTextBoxColumnStyleInfo1);
			this.MenusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.MenusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.MenusGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MenusGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MenusGrid.ColumnStyles.Add(zTranslatableTextBoxColumnStyleInfo2);
			this.MenusGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MenusGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MenusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.MenusGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MenusGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.MenusGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			// 
			// systemCheckBox
			// 
			this.systemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			// 
			// clientCheckBox
			// 
			this.clientCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 17, true);
			// 
			// visualCheckBox
			// 
			this.visualCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 17, true);
			// 
			// publishedCheckBox
			// 
			this.publishedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			// 
			// modifyCheckBox
			// 
			this.modifyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 17, true);
			// 
			// localCheckBox
			// 
			this.localCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			// 
			// autoDeliveryCheckBox
			// 
			this.autoDeliveryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 17, true);
			// 
			// docPackCheckBox
			// 
			this.docPackCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 17, true);
			// 
			// zipDocPackCheckBox
			// 
			this.zipDocPackCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			// 
			// webCheckBox
			// 
			this.webCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 17, true);
			// 
			// webIsVisibleCheckBox
			// 
			this.webIsVisibleCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 17, true);
			// 
			// MustRunOnlineCheckBox
			// 
			this.MustRunOnlineCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation);
			// 
			// visualizerMenuDetailsTab
			// 
			this.visualizerMenuDetailsTab.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("VisualizerMenuCustomisationForm|6ccef3a6-b954-4689-aff2-122a2321d8d2", "Details");
			this.visualizerMenuDetailsTab.Controls.Add(this.visualizerSystemCheckBox);
			this.visualizerMenuDetailsTab.Controls.Add(this.visualizerPublishedCheckBox);
			this.visualizerMenuDetailsTab.Controls.Add(this.visualizerPrimaryDocumentPicker);
			this.visualizerMenuDetailsTab.Controls.Add(this.visualizerFilterTextBox);
			this.visualizerMenuDetailsTab.Controls.Add(this.emailSubjectTextBox);
			this.visualizerMenuDetailsTab.Controls.Add(this.visualizerAutoDeliveryCheckBox);
			this.visualizerMenuDetailsTab.Controls.Add(this.visualizerMenuPathTextBox);
			this.visualizerMenuDetailsTab.Controls.Add(this.visualizerDocGroupDropEdit);
			this.visualizerMenuDetailsTab.Controls.Add(this.visualizerDeliveryRestrictionDropEdit);
			this.visualizerMenuDetailsTab.Controls.Add(this.visualizerDeliveryRestrictionConditionTextBox);
			this.visualizerMenuDetailsTab.Controls.Add(this.visualizerDeliveryRestrictionDescriptionTextBox);
			this.visualizerMenuDetailsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.visualizerMenuDetailsTab.Name = "visualizerMenuDetailsTab";
			this.visualizerMenuDetailsTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.visualizerMenuDetailsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 478, true);
			this.visualizerMenuDetailsTab.TabIndex = 0;
			// 
			// visualizerSystemCheckBox
			// 
			this.visualizerSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.visualizerSystemCheckBox, "Menus.SU_IsSystemDefined");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Business.StmMenuItemBase)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation)(null)).Menus)).SyncRoot)).SU_IsSystemDefined)));
			this.visualizerSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.visualizerSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 14, true);
			this.visualizerSystemCheckBox.Name = "visualizerSystemCheckBox";
			this.visualizerSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			this.visualizerSystemCheckBox.TabIndex = 0;
			this.visualizerSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// visualizerPublishedCheckBox
			// 
			this.visualizerPublishedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.visualizerPublishedCheckBox, "Menus.SU_IsPublished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Business.StmMenuItemBase)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation)(null)).Menus)).SyncRoot)).SU_IsPublished)));
			this.visualizerPublishedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.visualizerPublishedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 16, true);
			this.visualizerPublishedCheckBox.Name = "visualizerPublishedCheckBox";
			this.visualizerPublishedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			this.visualizerPublishedCheckBox.TabIndex = 1;
			this.visualizerPublishedCheckBox.UseVisualStyleBackColor = true;
			// 
			// visualizerPrimaryDocumentPicker
			// 
			this.visualizerPrimaryDocumentPicker.AllowDrop = true;
			this.visualizerPrimaryDocumentPicker.AutoSize = true;
			this.BindingSource.SetBindingMember(this.visualizerPrimaryDocumentPicker, "Menus.SU_PrimaryDocPackItemId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Business.StmMenuItemBase)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation)(null)).Menus)).SyncRoot)).SU_PrimaryDocPackItemId)));
			this.visualizerPrimaryDocumentPicker.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("MenuCustomisationForm|897cc6cc-4a3b-4d5a-9d1b-999ddaed1d2a", "Primary Document");
			this.visualizerPrimaryDocumentPicker.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.visualizerPrimaryDocumentPicker.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 95, true);
			this.visualizerPrimaryDocumentPicker.Name = "visualizerPrimaryDocumentPicker";
			this.visualizerPrimaryDocumentPicker.ShowDescriptionBox = false;
			this.visualizerPrimaryDocumentPicker.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.visualizerPrimaryDocumentPicker.TabIndex = 5;
			// 
			// visualizerFilterTextBox
			// 
			this.BindingSource.SetBindingMember(this.visualizerFilterTextBox, "Menus.SU_FilterList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Business.StmMenuItemBase)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation)(null)).Menus)).SyncRoot)).SU_FilterList)));
			this.visualizerFilterTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.visualizerFilterTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 121, true);
			this.visualizerFilterTextBox.Multiline = true;
			this.visualizerFilterTextBox.Name = "visualizerFilterTextBox";
			this.visualizerFilterTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 60, true);
			this.visualizerFilterTextBox.TabIndex = 6;
			// 
			// emailSubjectTextBox
			// 
			this.BindingSource.SetBindingMember(this.emailSubjectTextBox, "Menus.SU_EmailSubjectLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Business.StmMenuItemBase)(((System.Collections.IList)(((Enterprise.DocumentEngine.MenuCustomisation)(null)).Menus)).SyncRoot)).SU_EmailSubjectLine)));
			this.emailSubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.emailSubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 185, true);
			this.emailSubjectTextBox.Name = "emailSubjectTextBox";
			this.emailSubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 18, true);
			this.emailSubjectTextBox.TabIndex = 7;
			// 
			// visualizerAutoDeliveryCheckBox
			// 
			this.visualizerAutoDeliveryCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.visualizerAutoDeliveryCheckBox, "Menus.SU_PreventAutoDelivery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Business.StmMenuItemBase)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation)(null)).Menus)).SyncRoot)).SU_PreventAutoDelivery)));
			this.visualizerAutoDeliveryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.visualizerAutoDeliveryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 16, true);
			this.visualizerAutoDeliveryCheckBox.Name = "visualizerAutoDeliveryCheckBox";
			this.visualizerAutoDeliveryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 17, true);
			this.visualizerAutoDeliveryCheckBox.TabIndex = 2;
			this.visualizerAutoDeliveryCheckBox.UseVisualStyleBackColor = true;
			// 
			// visualizerMenuPathTextBox
			// 
			this.visualizerMenuPathTextBox.AcceptsReturn = false;
			this.visualizerMenuPathTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.visualizerMenuPathTextBox, "Menus.SU_MenuPath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Business.StmMenuItemBase)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation)(null)).Menus)).SyncRoot)).SU_MenuPath)));
			this.visualizerMenuPathTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.visualizerMenuPathTextBox.GridCurrent = null;
			this.visualizerMenuPathTextBox.GridMember = null;
			this.visualizerMenuPathTextBox.IsLanguageEditingEnabled = true;
			this.visualizerMenuPathTextBox.IsMultiLine = false;
			this.visualizerMenuPathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 43, true);
			this.visualizerMenuPathTextBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 20, true);
			this.visualizerMenuPathTextBox.Name = "visualizerMenuPathTextBox";
			this.visualizerMenuPathTextBox.ReadOnly = false;
			this.visualizerMenuPathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 20, true);
			this.visualizerMenuPathTextBox.TabIndex = 3;
			// 
			// visualizerDocGroupDropEdit
			// 
			this.visualizerDocGroupDropEdit.AllowDrop = true;
			this.visualizerDocGroupDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.visualizerDocGroupDropEdit, "Menus.SU_ContactType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Business.StmMenuItemBase)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation)(null)).Menus)).SyncRoot)).SU_ContactType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.StmMenuItemBase)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation)(null)).Menus)).SyncRoot)).ContactTypeList)));
			this.visualizerDocGroupDropEdit.BindToList = "Menus.ContactTypeList";
			this.visualizerDocGroupDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 69, true);
			this.visualizerDocGroupDropEdit.Name = "visualizerDocGroupDropEdit";
			this.visualizerDocGroupDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.visualizerDocGroupDropEdit.TabIndex = 4;
			//
			// DeliveryRestrictionDropEdit
			//
			this.visualizerDeliveryRestrictionDropEdit.AllowDrop = true;
			this.visualizerDeliveryRestrictionDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.visualizerDeliveryRestrictionDropEdit, "Menus.SU_DeliveryRestrictionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Business.StmMenuItemBase)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation)(null)).Menus)).SyncRoot)).SU_DeliveryRestrictionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.StmMenuItemBase)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation)(null)).Menus)).SyncRoot)).DeliveryRestrictionTypeList)));
			this.visualizerDeliveryRestrictionDropEdit.BindToList = "Menus.DeliveryRestrictionTypeList";
			this.visualizerDeliveryRestrictionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 210, true);
			this.visualizerDeliveryRestrictionDropEdit.Name = "visualizerDeliveryRestrictionDropEdit";
			this.visualizerDeliveryRestrictionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.visualizerDeliveryRestrictionDropEdit.TabIndex = 8;
			//
			// DeliveryRestrictionConditionTextBox
			//
			this.BindingSource.SetBindingMember(this.visualizerDeliveryRestrictionConditionTextBox, "Menus.SU_DeliveryRestrictionMacro");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Business.StmMenuItemBase)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation)(null)).Menus)).SyncRoot)).SU_DeliveryRestrictionMacro)));
			this.visualizerDeliveryRestrictionConditionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.visualizerDeliveryRestrictionConditionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 236, true);
			this.visualizerDeliveryRestrictionConditionTextBox.Multiline = true;
			this.visualizerDeliveryRestrictionConditionTextBox.Name = "visualizerDeliveryRestrictionConditionTextBox";
			this.visualizerDeliveryRestrictionConditionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 54, true);
			this.visualizerDeliveryRestrictionConditionTextBox.TabIndex = 9;
			//
			// DeliveryRestrictionDescriptionTextBox
			//
			this.BindingSource.SetBindingMember(this.visualizerDeliveryRestrictionDescriptionTextBox, "Menus.SU_DeliveryRestrictionDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Business.StmMenuItemBase)(((System.Collections.IList)(((Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation)(null)).Menus)).SyncRoot)).SU_DeliveryRestrictionDescription)));
			this.visualizerDeliveryRestrictionDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.visualizerDeliveryRestrictionDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 296, true);
			this.visualizerDeliveryRestrictionDescriptionTextBox.Multiline = true;
			this.visualizerDeliveryRestrictionDescriptionTextBox.Name = "visualizerDeliveryRestrictionDescriptionTextBox";
			this.visualizerDeliveryRestrictionDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 28, true);
			this.visualizerDeliveryRestrictionDescriptionTextBox.TabIndex = 10;
			// 
			// VisualizerMenuCustomisationForm
			// 
			this.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("VisualizerMenuCustomisationForm|FormCaption", "Customize Forms Menus");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 686, true);
			this.DataSourceType = typeof(Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation);
			this.DataSourceTypeName = "Enterprise.DocumentVisualizer.Business.VisualizerMenuCustomisation";
			this.Name = "VisualizerMenuCustomisationForm";
			this.TemplatesUsedGroupBox.ResumeLayout(false);
			this.TemplatesUsedGroupBox.PerformLayout();
			this.PivotAndChildMenuTabControl.ResumeLayout(false);
			this.PivotAndChildMenuTabControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TemplatesUsedGrid)).EndInit();
			this.TemplatesUsedGrid.ResumeLayout(false);
			this.TemplatesUsedGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MenusGrid)).EndInit();
			this.MenusGrid.ResumeLayout(false);
			this.MenusGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.availableTemplatesGrid)).EndInit();
			this.availableTemplatesGrid.ResumeLayout(false);
			this.availableTemplatesGrid.PerformLayout();
			this.menuDetailsTab.ResumeLayout(false);
			this.menuDetailsTab.PerformLayout();
			this.filterAndDescPanel.ResumeLayout(false);
			this.filterAndDescPanel.PerformLayout();
			this.documentOptionsPanel.ResumeLayout(false);
			this.documentOptionsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.visualizerMenuDetailsTab.ResumeLayout(false);
			this.visualizerMenuDetailsTab.PerformLayout();
			this.visualizerPrimaryDocumentPicker.ResumeLayout(true);
			this.visualizerPrimaryDocumentPicker.PerformLayout();
			this.visualizerMenuPathTextBox.ResumeLayout(true);
			this.visualizerMenuPathTextBox.PerformLayout();
			this.visualizerDocGroupDropEdit.ResumeLayout(true);
			this.visualizerDocGroupDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected Enterprise.ZArchitecture.GUI.ZTabPage visualizerMenuDetailsTab;
		Enterprise.ZArchitecture.GUI.ZCheckBox visualizerSystemCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox visualizerPublishedCheckBox;
		Enterprise.ZArchitecture.ZTextBox visualizerFilterTextBox;
		Enterprise.ZArchitecture.ZTextBox emailSubjectTextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox visualizerAutoDeliveryCheckBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit visualizerDocGroupDropEdit;
		protected Enterprise.ZArchitecture.ZTranslatableTextControl visualizerMenuPathTextBox;
		ZArchitecture.GUI.ZGuidDropEdit visualizerPrimaryDocumentPicker;
		ZArchitecture.GUI.ZDropEdit visualizerDeliveryRestrictionDropEdit;
		ZArchitecture.ZTextBox visualizerDeliveryRestrictionConditionTextBox;
		ZArchitecture.ZTextBox visualizerDeliveryRestrictionDescriptionTextBox;
		#endregion
	}
}
