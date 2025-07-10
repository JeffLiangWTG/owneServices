using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.PdfiumWrapper;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class SplitDocumentForm : ZChildForm
	{
		ZArchitecture.GUI.ZButton SplitButton;
		ZArchitecture.GUI.ZButton ConfirmButton;
		ZArchitecture.GUI.ZButton CloseButton;
		ZGroupBox SplitConditionGroupBox;
		ZArchitecture.ZLabel zLabel3;
		protected ZArchitecture.ZCalcEdit SizeInMBEdit;
		ZArchitecture.ZCalcEdit SizeInKBEdit;
		ZPanel zPanel1;
		ZGroupBox zSplitResultGroupBox;
		ZSplitResultGrid DocumentSplitResultGrid;
		CargoWise.Windows.UI.KSplitContainer kSplitContainer1;
		ZSplitConfigGrid DocumentSplitConfigGrid;
		ZLabel zLabel1;
		CargoWise.Windows.UI.KSplitContainer kSplitContainer2;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SplitButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ConfirmButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SplitConditionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.kSplitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.SizeInKBEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SizeInMBEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.DocumentSplitConfigGrid = new Enterprise.DocumentScanning.GUI.SplitDocumentForm.ZSplitConfigGrid();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zSplitResultGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DocumentSplitResultGrid = new Enterprise.DocumentScanning.GUI.ZSplitResultGrid();
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SplitConditionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer2)).BeginInit();
			this.kSplitContainer2.Panel1.SuspendLayout();
			this.kSplitContainer2.Panel2.SuspendLayout();
			this.kSplitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentSplitConfigGrid)).BeginInit();
			this.DocumentSplitConfigGrid.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.zSplitResultGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentSplitResultGrid)).BeginInit();
			this.DocumentSplitResultGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 591, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentScanning.Business.DocumentSplitManager);
			// 
			// SplitButton
			// 
			this.SplitButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|fbe2e438-e63e-4be9-8e34-9e2cce7627df", "Split");
			this.SplitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 8, true);
			this.SplitButton.Name = "SplitButton";
			this.SplitButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SplitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.SplitButton.TabIndex = 3;
			this.SplitButton.ToolTipCaption = null;
			this.SplitButton.UseVisualStyleBackColor = true;
			// 
			// ConfirmButton
			// 
			this.ConfirmButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ConfirmButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("cb2c83d4-940d-4aa6-85a9-82a432c61f42", "Confirm");
			this.ConfirmButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(513, 6, true);
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ConfirmButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.ConfirmButton.TabIndex = 1;
			this.ConfirmButton.ToolTipCaption = null;
			this.ConfirmButton.UseVisualStyleBackColor = true;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|1c1b2960-bde6-40be-9ee6-cc810a85e227", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(609, 6, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// SplitConditionGroupBox
			// 
			this.SplitConditionGroupBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|ed8d097e-ac2a-461c-b415-79820ecda26c", "Split Config");
			this.SplitConditionGroupBox.Controls.Add(this.kSplitContainer2);
			this.SplitConditionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitConditionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitConditionGroupBox.Name = "SplitConditionGroupBox";
			this.SplitConditionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 273, true);
			this.SplitConditionGroupBox.TabIndex = 0;
			this.SplitConditionGroupBox.TabStop = false;
			// 
			// kSplitContainer2
			// 
			this.kSplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.kSplitContainer2.Name = "kSplitContainer2";
			this.kSplitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.kSplitContainer2.IsSplitterFixed = true;
			this.kSplitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			// 
			// kSplitContainer2.Panel1
			// 
			this.kSplitContainer2.Panel1.Controls.Add(this.SizeInKBEdit);
			this.kSplitContainer2.Panel1.Controls.Add(this.SizeInMBEdit);
			this.kSplitContainer2.Panel1.Controls.Add(this.zLabel1);
			this.kSplitContainer2.Panel1.Controls.Add(this.SplitButton);
			this.kSplitContainer2.Panel1.Controls.Add(this.zLabel3);
			// 
			// kSplitContainer2.Panel2
			// 
			this.kSplitContainer2.Panel2.Controls.Add(this.DocumentSplitConfigGrid);
			this.kSplitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 254, true);
			this.kSplitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(45);
			this.kSplitContainer2.TabIndex = 4;
			// 
			// SizeInKBEdit
			// 
			this.BindingSource.SetBindingMember(this.SizeInKBEdit, "MaxSizeInKb");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).MaxSizeInKb)));
			this.SizeInKBEdit.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|ffb74875-37ec-4936-9d76-3d85359ba7e8", "Document Max Size");
			this.SizeInKBEdit.DecimalPlaces = 0;
			this.SizeInKBEdit.Decimals = 0;
			this.SizeInKBEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 10, true);
			this.SizeInKBEdit.Name = "SizeInKBEdit";
			this.SizeInKBEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.SizeInKBEdit.TabIndex = 0;
			this.SizeInKBEdit.Text = "0";
			this.SizeInKBEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SizeInMBEdit
			// 
			this.BindingSource.SetBindingMember(this.SizeInMBEdit, "MaxSizeInMb");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).MaxSizeInMb)));
			this.SizeInMBEdit.DecimalPlaces = 2;
			this.SizeInMBEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 10, true);
			this.SizeInMBEdit.Name = "SizeInMBEdit";
			this.SizeInMBEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.SizeInMBEdit.TabIndex = 1;
			this.SizeInMBEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|1f0bbb93-21d7-4c12-ad2c-9edcbc9679a7", "KB");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 13, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 13, true);
			this.zLabel1.TabIndex = 2;
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|4110dec2-9ec3-40ff-a503-be168f03c100", "MB");
			this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 13, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 13, true);
			this.zLabel3.TabIndex = 2;
			// 
			// DocumentSplitConfigGrid
			// 
			this.DocumentSplitConfigGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DocumentSplitConfigGrid, "DocumentSplitConfigCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitConfigCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.DocumentSplitConfigInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitConfigCollection)).SyncRoot)).DocumentName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.DocumentSplitConfigInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitConfigCollection)).SyncRoot)).DocumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitConfigInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitConfigCollection)).SyncRoot)).DocType_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.DocumentSplitConfigInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitConfigCollection)).SyncRoot)).DescriptionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentScanning.Business.DocumentSplitConfigInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitConfigCollection)).SyncRoot)).StartPage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentScanning.Business.DocumentSplitConfigInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitConfigCollection)).SyncRoot)).EndPage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.DocumentSplitConfigInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitConfigCollection)).SyncRoot)).AppendPageNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.DocumentSplitConfigInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitConfigCollection)).SyncRoot)).IsPublished)));
			this.DocumentSplitConfigGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "DocumentName";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|5fe26d6b-b756-41fe-8e04-2cecbf2629d4", "Document Name");
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zDropEditColumnStyleInfo1.BindToList = "DocType_List";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|d3cfdd8b-0f88-4bea-9980-9e3a1d186adc", "Doc. Type");
			zDropEditColumnStyleInfo1.ColumnName = "DocumentType";
			zDropEditColumnStyleInfo1.ToolTip = "The 3-letter Document Type for this eDoc";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(57);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|8C99C28E-AF50-4A14-B4E9-2BC74756D30F", "Description");
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.ColumnName = "DescriptionType";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(57);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "StartPage";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|06827a82-d1cf-4173-add9-65b113f3161b", "Start Page");
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "EndPage";
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|f7e4c466-8e1f-4ba0-868f-f64edfa7064b", "End Page");
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);
			zCheckBoxColumnStyleInfo1.ColumnName = "AppendPageNumber";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|a694e6ee-e844-4297-95c5-3e0c2779bf1f", "Append Page No?");
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			zCheckBoxColumnStyleInfo2.ColumnName = "IsPublished";
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|d1032e2c-4804-40ca-8aec-9513a8389fc5", "Publish?");
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			this.DocumentSplitConfigGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DocumentSplitConfigGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DocumentSplitConfigGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DocumentSplitConfigGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DocumentSplitConfigGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.DocumentSplitConfigGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DocumentSplitConfigGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.DocumentSplitConfigGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentSplitConfigGrid.GridId = "f04d1e6b-53dd-4654-b146-a7055262ae8d";
			this.DocumentSplitConfigGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocumentSplitConfigGrid.LayoutKey = "zGrid1";
			this.DocumentSplitConfigGrid.LimitedColumns = null;
			this.DocumentSplitConfigGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentSplitConfigGrid.Name = "DocumentSplitConfigGrid";
			this.DocumentSplitConfigGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 205, true);
			this.DocumentSplitConfigGrid.TabIndex = 0;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.CloseButton);
			this.zPanel1.Controls.Add(this.ConfirmButton);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 556, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 35, true);
			this.zPanel1.TabIndex = 2;
			// 
			// zSplitResultGroupBox
			// 
			this.zSplitResultGroupBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|CA55A700-E79E-4507-8DCE-43E65CD08537", "Split Result");
			this.zSplitResultGroupBox.Controls.Add(this.DocumentSplitResultGrid);
			this.zSplitResultGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zSplitResultGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zSplitResultGroupBox.Name = "zSplitResultGroupBox";
			this.zSplitResultGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 279, true);
			this.zSplitResultGroupBox.TabIndex = 1;
			this.zSplitResultGroupBox.TabStop = false;
			// 
			// DocumentSplitResultGrid
			// 
			this.DocumentSplitResultGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DocumentSplitResultGrid, "DocumentSplitResultCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitResultCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.DocumentSplitResultInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitResultCollection)).SyncRoot)).DocumentName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.DocumentSplitResultInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitResultCollection)).SyncRoot)).DocumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitResultInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitResultCollection)).SyncRoot)).DocType_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.DocumentSplitResultInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitConfigCollection)).SyncRoot)).DescriptionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentScanning.Business.DocumentSplitResultInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitResultCollection)).SyncRoot)).StartPage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentScanning.Business.DocumentSplitResultInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitResultCollection)).SyncRoot)).EndPage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentScanning.Business.DocumentSplitResultInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitResultCollection)).SyncRoot)).ActualSizeInKb)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.DocumentSplitResultInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentSplitManager)(null)).DocumentSplitResultCollection)).SyncRoot)).IsPublished)));
			this.DocumentSplitResultGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|5fe26d6b-b756-41fe-8e04-2cecbf2629d4", "Document Name");
			zTextBoxColumnStyleInfo2.ColumnName = "DocumentName";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			zDropEditColumnStyleInfo2.BindToList = "DocType_List";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|d3cfdd8b-0f88-4bea-9980-9e3a1d186adc", "Doc. Type");
			zDropEditColumnStyleInfo2.ColumnName = "DocumentType";
			zDropEditColumnStyleInfo2.ToolTip = "The 3-letter Document Type for this eDoc";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(57);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|D9D30CC3-3188-4A88-946B-09F55C451CB5", "Description");
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.ColumnName = "DescriptionType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(57);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|06827a82-d1cf-4173-add9-65b113f3161b", "Start Page");
			zCalcEditColumnStyleInfo3.ColumnName = "StartPage";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|f7e4c466-8e1f-4ba0-868f-f64edfa7064b", "End Page");
			zCalcEditColumnStyleInfo4.ColumnName = "EndPage";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|83da4639-7c47-4a4a-9be3-84d59f4b26f3", "Actual Size (KB)");
			zCalcEditColumnStyleInfo5.ColumnName = "ActualSizeInKb";
			zCalcEditColumnStyleInfo5.Decimals = 0;
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83);
			zCheckBoxColumnStyleInfo3.ColumnName = "IsPublished";
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|ef1836ac-bec8-48de-b570-392dd3a84819", "Published");
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			zCheckBoxColumnStyleInfo3.IsReadOnly = true;
			this.DocumentSplitResultGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DocumentSplitResultGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DocumentSplitResultGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DocumentSplitResultGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.DocumentSplitResultGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.DocumentSplitResultGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.DocumentSplitResultGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.DocumentSplitResultGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentSplitResultGrid.GridId = "f04d1e6b-53dd-4654-b146-a7055262ae8d";
			this.DocumentSplitResultGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DocumentSplitResultGrid.IsWholeRowSelectedOnClick = true;
			this.DocumentSplitResultGrid.LayoutKey = "DocumentSplitResultGrid";
			this.DocumentSplitResultGrid.LimitedColumns = null;
			this.DocumentSplitResultGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DocumentSplitResultGrid.Name = "DocumentSplitResultGrid";
			this.DocumentSplitResultGrid.ReadOnly = true;
			this.DocumentSplitResultGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 260, true);
			this.DocumentSplitResultGrid.TabIndex = 0;
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kSplitContainer1.Name = "kSplitContainer1";
			this.kSplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.SplitConditionGroupBox);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.zSplitResultGroupBox);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 556, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(273);
			this.kSplitContainer1.TabIndex = 4;
			// 
			// SplitDocumentForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("SplitDocumentForm|981d8cb9-d3Ad-409d-965e-dffba30b143a", "Split Document");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 615, true);
			this.Controls.Add(this.kSplitContainer1);
			this.Controls.Add(this.zPanel1);
			this.DataSourceType = typeof(Enterprise.DocumentScanning.Business.DocumentSplitManager);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 386, true);
			this.Name = "SplitDocumentForm";
			this.ShowInTaskbar = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.kSplitContainer1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitConditionGroupBox.ResumeLayout(false);
			this.SplitConditionGroupBox.PerformLayout();
			this.kSplitContainer2.Panel1.ResumeLayout(false);
			this.kSplitContainer2.Panel1.PerformLayout();
			this.kSplitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer2)).EndInit();
			this.kSplitContainer2.ResumeLayout(false);
			this.kSplitContainer2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentSplitConfigGrid)).EndInit();
			this.DocumentSplitConfigGrid.ResumeLayout(false);
			this.DocumentSplitConfigGrid.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zSplitResultGroupBox.ResumeLayout(false);
			this.zSplitResultGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DocumentSplitResultGrid)).EndInit();
			this.DocumentSplitResultGrid.ResumeLayout(false);
			this.DocumentSplitResultGrid.PerformLayout();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
