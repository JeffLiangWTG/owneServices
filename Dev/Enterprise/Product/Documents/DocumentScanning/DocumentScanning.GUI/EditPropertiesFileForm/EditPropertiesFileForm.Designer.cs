using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class EditPropertiesFileForm : ZChildForm, IEditResponse
	{
		internal ZDropEditWithFixedWidth companyDropEdit;
		internal ZDropEditWithFixedWidth branchDropEdit;
		internal ZDropEditWithFixedWidth departmentDropEdit;
		private ZCheckBox IsSpecificCheckBox;
		private ZArchitecture.ZLabel CompanySpecificLabel;
		private ZArchitecture.ZLabel BranchSpecificLabel;
		private ZArchitecture.ZLabel DepartmentSpecificLabel;
		private ZCheckBox ApplyToAllCheckBox;
		private ZDropEdit zDropEdit1;
		private ZButton zButton1;

		internal ZButton OkButton;
		private ZArchitecture.ZTextBox NameTextBox;
		internal ZButton CancelRenameButton;
		private ZCheckBox IsPublishedCheckBox;
		internal ZButton CreateNewDocumentTypeButton;
		private ZDropEdit SC_DocTypeDropDownEdit;
		private ZArchitecture.ZTextBox SC_DescTextBox;
		private ZArchitecture.ZTextBox parseTypeTextBox;
		internal ZArchitecture.GUI.ZCheckBox isParsingEnabledCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox SaveVersionsCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZButton PreviewButton;

		new void InitializeComponent()
		{
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelRenameButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsPublishedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SaveVersionsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CreateNewDocumentTypeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SC_DocTypeDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SC_DescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.companyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.branchDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.departmentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.IsSpecificCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CompanySpecificLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BranchSpecificLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DepartmentSpecificLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ApplyToAllCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zButton1 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PreviewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.parseTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.isParsingEnabledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SC_DocTypeDropDownEdit.SuspendLayout();
			this.companyDropEdit.SuspendLayout();
			this.branchDropEdit.SuspendLayout();
			this.departmentDropEdit.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 320, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 23, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentScanning.Business.StorageDocsBase);
			// 
			// OkButton
			// 
			this.OkButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("EditPropertiesFileForm|cbeff55a-4012-4271-b236-f1285e366e93", "&OK");
			this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 293, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.OkButton.TabIndex = 16;
			this.OkButton.ToolTipCaption = null;
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// CancelRenameButton
			// 
			this.CancelRenameButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("EditPropertiesFileForm|8e0526ab-2564-422e-a8e1-7844dc10fda4", "&Cancel");
			this.CancelRenameButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelRenameButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 293, true);
			this.CancelRenameButton.Name = "CancelRenameButton";
			this.CancelRenameButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelRenameButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelRenameButton.TabIndex = 17;
			this.CancelRenameButton.ToolTipCaption = null;
			this.CancelRenameButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// NameTextBox
			// 
			this.BindingSource.SetBindingMember(this.NameTextBox, "SC_FileNameForRenaming");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(null)).SC_FileNameForRenaming)));
			this.NameTextBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("EditPropertiesFileForm|27b02cb9-716e-4528-9eee-54324cde9a10", "File Name");
			this.NameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 15, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.NameTextBox.TabIndex = 0;
			// 
			// IsPublishedCheckBox
			// 
			this.IsPublishedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsPublishedCheckBox, "SC_IsPublished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(null)).SC_IsPublished)));
			this.IsPublishedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsPublishedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPublishedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 151, true);
			this.IsPublishedCheckBox.Name = "IsPublishedCheckBox";
			this.IsPublishedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsPublishedCheckBox.TabIndex = 8;
			// 
			// SaveVersionsCheckBox
			// 
			this.SaveVersionsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SaveVersionsCheckBox, "SC_SaveVersions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(null)).SC_SaveVersions)));
			this.SaveVersionsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SaveVersionsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SaveVersionsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 151, true);
			this.SaveVersionsCheckBox.Name = "SaveVersionsCheckBox";
			this.SaveVersionsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.SaveVersionsCheckBox.TabIndex = 9;
			// 
			// CreateNewDocumentTypeButton
			// 
			this.CreateNewDocumentTypeButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("EditPropertiesFileForm|5bcab12c-ed70-4a5e-8c40-b995297a55c2", "Create New Doc Type");
			this.CreateNewDocumentTypeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 37, true);
			this.CreateNewDocumentTypeButton.Name = "CreateNewDocumentTypeButton";
			this.CreateNewDocumentTypeButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CreateNewDocumentTypeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 19, true);
			this.CreateNewDocumentTypeButton.TabIndex = 2;
			this.CreateNewDocumentTypeButton.ToolTipCaption = null;
			this.CreateNewDocumentTypeButton.Click += new System.EventHandler(this.CreateNewDocumentTypeButton_Click);
			// 
			// SC_DocTypeDropDownEdit
			// 
			this.SC_DocTypeDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SC_DocTypeDropDownEdit, "SC_DocType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(null)).SC_DocType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(null)).SC_DocType_List)));
			this.SC_DocTypeDropDownEdit.BindToList = "SC_DocType_List";
			this.SC_DocTypeDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 37, true);
			this.SC_DocTypeDropDownEdit.MaxItemsToShowInDropDown = 20;
			this.SC_DocTypeDropDownEdit.Name = "SC_DocTypeDropDownEdit";
			this.SC_DocTypeDropDownEdit.ShowDescriptionBox = false;
			this.SC_DocTypeDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.SC_DocTypeDropDownEdit.TabIndex = 1;
			// 
			// SC_DescTextBox
			// 
			this.BindingSource.SetBindingMember(this.SC_DescTextBox, "SC_DescMultilingual");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(null)).SC_DescMultilingual)));
			this.SC_DescTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SC_DescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 59, true);
			this.SC_DescTextBox.Name = "SC_DescTextBox";
			this.SC_DescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.SC_DescTextBox.TabIndex = 3;
			// 
			// companyDropEdit
			// 
			this.companyDropEdit.AllowDrop = true;
			this.companyDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.companyDropEdit, "CompanyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(null)).CompanyCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.companyDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.companyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 187, true);
			this.companyDropEdit.Name = "companyDropEdit";
			this.companyDropEdit.PreBoundMaxLength = 3;
			this.companyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.companyDropEdit.TabIndex = 11;
			// 
			// branchDropEdit
			// 
			this.branchDropEdit.AllowDrop = true;
			this.branchDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.branchDropEdit, "BranchCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(null)).BranchCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.branchDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.branchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 213, true);
			this.branchDropEdit.Name = "branchDropEdit";
			this.branchDropEdit.PreBoundMaxLength = 3;
			this.branchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.branchDropEdit.TabIndex = 12;
			// 
			// departmentDropEdit
			// 
			this.departmentDropEdit.AllowDrop = true;
			this.departmentDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.departmentDropEdit, "DepartmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(null)).DepartmentCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.departmentDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.departmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 239, true);
			this.departmentDropEdit.Name = "departmentDropEdit";
			this.departmentDropEdit.PreBoundMaxLength = 3;
			this.departmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.departmentDropEdit.TabIndex = 13;
			// 
			// IsSpecificCheckBox
			// 
			this.IsSpecificCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSpecificCheckBox, "IsCompanyBranchDepartmentSpecific");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(null)).IsCompanyBranchDepartmentSpecific)));
			this.IsSpecificCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsSpecificCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSpecificCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 169, true);
			this.IsSpecificCheckBox.Name = "IsSpecificCheckBox";
			this.IsSpecificCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsSpecificCheckBox.TabIndex = 10;
			// 
			// CompanySpecificLabel
			// 
			this.CompanySpecificLabel.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("EditPropertiesFileForm|C3C19B01-BBC8-4998-B890-45ECF241F3FD", "Company:");
			this.CompanySpecificLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CompanySpecificLabel, false);
			this.CompanySpecificLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 187, true);
			this.CompanySpecificLabel.Name = "CompanySpecificLabel";
			this.CompanySpecificLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 13, true);
			this.CompanySpecificLabel.TabIndex = 18;
			this.CompanySpecificLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// BranchSpecificLabel
			// 
			this.BranchSpecificLabel.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("EditPropertiesFileForm|262D01FA-539B-4A43-B954-D55C4294483B", "Branch:");
			this.BranchSpecificLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BranchSpecificLabel, false);
			this.BranchSpecificLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 213, true);
			this.BranchSpecificLabel.Name = "BranchSpecificLabel";
			this.BranchSpecificLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 13, true);
			this.BranchSpecificLabel.TabIndex = 19;
			this.BranchSpecificLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DepartmentSpecificLabel
			// 
			this.DepartmentSpecificLabel.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("EditPropertiesFileForm|0A21C8C9-2075-48A5-B530-52C1A06BC099", "Department:");
			this.DepartmentSpecificLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DepartmentSpecificLabel, false);
			this.DepartmentSpecificLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 239, true);
			this.DepartmentSpecificLabel.Name = "DepartmentSpecificLabel";
			this.DepartmentSpecificLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 13, true);
			this.DepartmentSpecificLabel.TabIndex = 20;
			this.DepartmentSpecificLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonPreview
			// 
			this.PreviewButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("EditPropertiesFileForm|401dc7f7-5104-42af-8ec4-8f8ac11b3023", "&Preview");
			this.PreviewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 293, true);
			this.PreviewButton.Name = "buttonPreview";
			this.PreviewButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PreviewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.PreviewButton.TabIndex = 15;
			this.PreviewButton.ToolTipCaption = null;
			this.PreviewButton.Click += new System.EventHandler(this.PreviewButton_Click);
			// 
			// ApplyToAllCheckBox
			// 
			this.ApplyToAllCheckBox.AutoSize = true;
			this.ApplyToAllCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("EditPropertiesFileForm|6AB12AD0-C60F-486F-98DA-C8AD4DF3A2BB", "Apply to All");
			this.ApplyToAllCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ApplyToAllCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ApplyToAllCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 260, true);
			this.ApplyToAllCheckBox.Name = "ApplyToAllCheckBox";
			this.ApplyToAllCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ApplyToAllCheckBox.TabIndex = 14;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "SC_RDS_NKDocSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(null)).SC_RDS_NKDocSource)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(null)).SC_DocSource_List)));
			this.zDropEdit1.BindToList = "SC_DocSource_List";
			this.zDropEdit1.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("37943930-5939-451c-8cb6-3a1070941580", "Document Source");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 85, true);
			this.zDropEdit1.MaxItemsToShowInDropDown = 20;
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.ShowDescriptionBox = false;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zDropEdit1.TabIndex = 4;
			// 
			// zButton1
			// 
			this.zButton1.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("c268f40b-cbab-49a9-9159-c332b74db05a", "Create New Source");
			this.zButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 86, true);
			this.zButton1.Name = "zButton1";
			this.zButton1.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 19, true);
			this.zButton1.TabIndex = 5;
			this.zButton1.ToolTipCaption = null;
			this.zButton1.Click += new System.EventHandler(this.CreateNewDocumentSourceButton_Click);
			// 
			// parseTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.parseTypeTextBox, "ParseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(null)).ParseType)));
			this.parseTypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.parseTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 127, true);
			this.parseTypeTextBox.Name = "parseTypeTextBox";
			this.parseTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.parseTypeTextBox.TabIndex = 7;
			this.parseTypeTextBox.Visible = EDocsParsingHelper.IsDocumentParsingEnabled();
			// 
			// isParsingEnabledCheckBox
			// 
			this.isParsingEnabledCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isParsingEnabledCheckBox, "IsParsingEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(null)).IsParsingEnabled)));
			this.isParsingEnabledCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isParsingEnabledCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isParsingEnabledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 109, true);
			this.isParsingEnabledCheckBox.Name = "isParsingEnabledCheckBox";
			this.isParsingEnabledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.isParsingEnabledCheckBox.TabIndex = 6;
			this.isParsingEnabledCheckBox.Visible = EDocsParsingHelper.IsDocumentParsingEnabled();
			// 
			// EditPropertiesFileForm
			// 
			this.CancelButton = this.CancelRenameButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("EditPropertiesFileForm|c93a4302-77fb-4e2e-a596-d43a3e3f1e64", "Edit Document");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 345, true);
			this.Controls.Add(this.zButton1);
			this.Controls.Add(this.zDropEdit1);
			this.Controls.Add(this.ApplyToAllCheckBox);
			this.Controls.Add(this.DepartmentSpecificLabel);
			this.Controls.Add(this.BranchSpecificLabel);
			this.Controls.Add(this.CompanySpecificLabel);
			this.Controls.Add(this.IsSpecificCheckBox);
			this.Controls.Add(this.companyDropEdit);
			this.Controls.Add(this.branchDropEdit);
			this.Controls.Add(this.departmentDropEdit);
			this.Controls.Add(this.SaveVersionsCheckBox);
			this.Controls.Add(this.IsPublishedCheckBox);
			this.Controls.Add(this.CreateNewDocumentTypeButton);
			this.Controls.Add(this.SC_DocTypeDropDownEdit);
			this.Controls.Add(this.SC_DescTextBox);
			this.Controls.Add(this.PreviewButton);
			this.Controls.Add(this.NameTextBox);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.CancelRenameButton);
			this.Controls.Add(this.parseTypeTextBox);
			this.Controls.Add(this.isParsingEnabledCheckBox);
			this.DataSourceAssemblyName = "DocumentScanning";
			this.DataSourceType = typeof(Enterprise.DocumentScanning.Business.StorageDocsBase);
			this.DataSourceTypeName = "Enterprise.DocumentScanning.Business.StorageDocsBase";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "EditPropertiesFileForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.PreviewButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelRenameButton, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.NameTextBox, 0);
			this.Controls.SetChildIndex(this.SC_DescTextBox, 0);
			this.Controls.SetChildIndex(this.SC_DocTypeDropDownEdit, 0);
			this.Controls.SetChildIndex(this.CreateNewDocumentTypeButton, 0);
			this.Controls.SetChildIndex(this.IsPublishedCheckBox, 0);
			this.Controls.SetChildIndex(this.SaveVersionsCheckBox, 0);
			this.Controls.SetChildIndex(this.departmentDropEdit, 0);
			this.Controls.SetChildIndex(this.branchDropEdit, 0);
			this.Controls.SetChildIndex(this.companyDropEdit, 0);
			this.Controls.SetChildIndex(this.IsSpecificCheckBox, 0);
			this.Controls.SetChildIndex(this.CompanySpecificLabel, 0);
			this.Controls.SetChildIndex(this.BranchSpecificLabel, 0);
			this.Controls.SetChildIndex(this.DepartmentSpecificLabel, 0);
			this.Controls.SetChildIndex(this.ApplyToAllCheckBox, 0);
			this.Controls.SetChildIndex(this.zDropEdit1, 0);
			this.Controls.SetChildIndex(this.zButton1, 0);
			this.Controls.SetChildIndex(this.parseTypeTextBox, 0);
			this.Controls.SetChildIndex(this.isParsingEnabledCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SC_DocTypeDropDownEdit.ResumeLayout(true);
			this.SC_DocTypeDropDownEdit.PerformLayout();
			this.companyDropEdit.ResumeLayout(true);
			this.companyDropEdit.PerformLayout();
			this.branchDropEdit.ResumeLayout(true);
			this.branchDropEdit.PerformLayout();
			this.departmentDropEdit.ResumeLayout(true);
			this.departmentDropEdit.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.parseTypeTextBox.ResumeLayout(true);
			this.parseTypeTextBox.PerformLayout();
			this.isParsingEnabledCheckBox.ResumeLayout(true);
			this.isParsingEnabledCheckBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private Container components = null;
		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}
	}
}
