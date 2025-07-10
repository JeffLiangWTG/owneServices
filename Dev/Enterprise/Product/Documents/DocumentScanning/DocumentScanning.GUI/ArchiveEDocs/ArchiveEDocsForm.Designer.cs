using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class ArchiveEDocsForm : ZChildForm
	{
		ZGrid SearchTypeGrid;
		Enterprise.ZArchitecture.GUI.ZButton BurnCDButton;
		Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		Enterprise.ZArchitecture.GUI.ZGroupBox ArchivedDocumentsGroupBox;
		internal Enterprise.ZArchitecture.ZGrid ArchivedDocumentsGrid;
		Enterprise.ZArchitecture.ZLabel TotalSizeLabel;
		Enterprise.ZArchitecture.ZLabel WarningLabel;
		Enterprise.ZArchitecture.GUI.ZGroupBox SelectOrganisationGroupbox;
		Enterprise.ZArchitecture.GUI.ZDateEdit ETAFromDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit ETAToDateEdit;
		Enterprise.ZArchitecture.GUI.ZGroupBox SelectDatesGroupBox;
		internal Enterprise.ZArchitecture.ZGrid ListToArchiveGrid;
		Enterprise.ZArchitecture.GUI.ZButton AddToCDButton;
		Enterprise.ZArchitecture.GUI.ZDateEdit ETDToDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit ETDFromDateEdit;
		Enterprise.ZArchitecture.ZLabel OrganisationInstructionsLabel;
		Enterprise.ZArchitecture.ZLabel DatesInstructionsLabel;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox OrganisationFindbox;
		Enterprise.ZArchitecture.GUI.ZCheckBox IsConsignorCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox IsConsigneeCheckBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox SelectTypesGroupBox;
		Enterprise.ZArchitecture.ZLabel InstructionsLabel;
		ZCheckBox RelatedEDocsCheckBox;
		ZDateEdit JobsClosedToDateEdit;
		ZDateEdit JobsClosedFromDateEdit;
		private ZLabel zLabel1;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ArchivedDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.InstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ListToArchiveGrid = new Enterprise.ZArchitecture.ZGrid();
			this.WarningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalSizeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ArchivedDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BurnCDButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ETAToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SelectOrganisationGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsConsigneeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsConsignorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OrganisationFindbox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OrganisationInstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ETAFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SelectDatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DatesInstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.JobsClosedToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JobsClosedFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ETDToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ETDFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AddToCDButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectTypesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SearchTypeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RelatedEDocsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ArchivedDocumentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ListToArchiveGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ArchivedDocumentsGrid)).BeginInit();
			this.SelectOrganisationGroupbox.SuspendLayout();
			this.SelectDatesGroupBox.SuspendLayout();
			this.SelectTypesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SearchTypeGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 646, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 26, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentScanning.Business.ArchiveEDocsManager);
			// 
			// ArchivedDocumentsGroupBox
			// 
			this.ArchivedDocumentsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ArchivedDocumentsGroupBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|3eeb32bf-f28a-4976-8637-68f31d7e8cbe", "eDocs to Copy to CD");
			this.ArchivedDocumentsGroupBox.Controls.Add(this.zLabel1);
			this.ArchivedDocumentsGroupBox.Controls.Add(this.InstructionsLabel);
			this.ArchivedDocumentsGroupBox.Controls.Add(this.ListToArchiveGrid);
			this.ArchivedDocumentsGroupBox.Controls.Add(this.WarningLabel);
			this.ArchivedDocumentsGroupBox.Controls.Add(this.TotalSizeLabel);
			this.ArchivedDocumentsGroupBox.Controls.Add(this.ArchivedDocumentsGrid);
			this.ArchivedDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 271, true);
			this.ArchivedDocumentsGroupBox.Name = "ArchivedDocumentsGroupBox";
			this.ArchivedDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 337, true);
			this.ArchivedDocumentsGroupBox.TabIndex = 8;
			this.ArchivedDocumentsGroupBox.TabStop = false;
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|ccac93e4-14fa-42e8-a1ed-4226c9f9104d", "Total Size (Megabytes)");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(574, 313, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 13, true);
			this.zLabel1.TabIndex = 13;
			// 
			// InstructionsLabel
			// 
			this.InstructionsLabel.AutoSize = true;
			this.InstructionsLabel.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|68439e69-934f-450c-b7db-1149f0125f77", "To remove items from the CD, right click on the grid row with your mouse and select \'Remove from CD\'");
			this.InstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.InstructionsLabel.Name = "InstructionsLabel";
			this.InstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 13, true);
			this.InstructionsLabel.TabIndex = 12;
			// 
			// ListToArchiveGrid
			// 
			this.ListToArchiveGrid.AllowNavigation = false;
			this.ListToArchiveGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.ListToArchiveGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ListToArchiveGrid, "ListToArchive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).ListToArchive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).ListToArchive)).SyncRoot)).DocumentOwnerDescription)));
			this.ListToArchiveGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|b0aa57f6-5a48-4790-b8cb-fd6c887d69a8", "eDoc Location", "Related eDocs", "");
			zTextBoxColumnStyleInfo1.ColumnName = "DocumentOwnerDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.ListToArchiveGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ListToArchiveGrid.GridId = "b8c20e8b-a632-40a5-a1ac-6d6f27167bd0";
			this.ListToArchiveGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ListToArchiveGrid.LayoutKey = "ListToArchiveGrid";
			this.ListToArchiveGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 40, true);
			this.ListToArchiveGrid.Name = "ListToArchiveGrid";
			this.ListToArchiveGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.ListToArchiveGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 261, true);
			this.ListToArchiveGrid.TabIndex = 0;
			// 
			// WarningLabel
			// 
			this.WarningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.WarningLabel.AutoSize = true;
			this.WarningLabel.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|79f043a7-c68d-4885-b1d0-2751c26ee703", "Note: Most data CDs hold approx 650M. Check the capacity of your CD before burning.");
			this.WarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 313, true);
			this.WarningLabel.Name = "WarningLabel";
			this.WarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 13, true);
			this.WarningLabel.TabIndex = 11;
			// 
			// TotalSizeLabel
			// 
			this.TotalSizeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalSizeLabel, "TotalSize");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).TotalSize)));
			this.TotalSizeLabel.IsFontBold = true;
			this.TotalSizeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(713, 308, true);
			this.TotalSizeLabel.Name = "TotalSizeLabel";
			this.TotalSizeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 23, true);
			this.TotalSizeLabel.TabIndex = 9;
			this.TotalSizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ArchivedDocumentsGrid
			// 
			this.ArchivedDocumentsGrid.AllowNavigation = false;
			this.ArchivedDocumentsGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.ArchivedDocumentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ArchivedDocumentsGrid, "ListToArchive.PublishedEDocsAndFiles");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).ListToArchive)).SyncRoot)).PublishedEDocsAndFiles)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).ListToArchive)).SyncRoot)).PublishedEDocsAndFiles)).SyncRoot)).SC_Date)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).ListToArchive)).SyncRoot)).PublishedEDocsAndFiles)).SyncRoot)).SC_DocType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).ListToArchive)).SyncRoot)).PublishedEDocsAndFiles)).SyncRoot)).SC_DescriptionForWeb)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).ListToArchive)).SyncRoot)).PublishedEDocsAndFiles)).SyncRoot)).SC_LastEditingUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).ListToArchive)).SyncRoot)).PublishedEDocsAndFiles)).SyncRoot)).SC_IsSystemGenerated)));
			this.ArchivedDocumentsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "SC_Date";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo2.ColumnName = "SC_DocType";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|8b430f36-36f2-4fee-a753-8da4551ba81e", "Description");
			zTextBoxColumnStyleInfo3.ColumnName = "SC_DescriptionForWeb";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|25274077-22c2-4858-93fb-42256a3c5cd7", "Last Edited By");
			zTextBoxColumnStyleInfo4.ColumnName = "SC_LastEditingUser";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.ColumnName = "SC_IsSystemGenerated";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			this.ArchivedDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ArchivedDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ArchivedDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ArchivedDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ArchivedDocumentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ArchivedDocumentsGrid.GridId = "dc7f933a-430c-44d1-9719-4d859a299d51";
			this.ArchivedDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ArchivedDocumentsGrid.IsWholeRowSelectedOnClick = true;
			this.ArchivedDocumentsGrid.LayoutKey = "zGrid1";
			this.ArchivedDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 40, true);
			this.ArchivedDocumentsGrid.Name = "ArchivedDocumentsGrid";
			this.ArchivedDocumentsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.ArchivedDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 261, true);
			this.ArchivedDocumentsGrid.TabIndex = 2;
			// 
			// BurnCDButton
			// 
			this.BurnCDButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BurnCDButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|567cd099-bd64-482b-a8ec-94393f630ea1", "Create eDocs CD");
			this.BurnCDButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 616, true);
			this.BurnCDButton.Name = "BurnCDButton";
			this.BurnCDButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.BurnCDButton.TabIndex = 6;
			this.BurnCDButton.Click += new System.EventHandler(this.BurnCDButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|a0f5252c-3d3d-4eab-ae8e-6892087a39c2", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(708, 616, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ETAToDateEdit
			// 
			this.ETAToDateEdit.AutoCompleteMonthThreshold = 1;
			this.ETAToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ETAToDateEdit, "SA_ETATo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).SA_ETATo)));
			this.ETAToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 84, true);
			this.ETAToDateEdit.Name = "ETAToDateEdit";
			this.ETAToDateEdit.TabIndex = 8;
			// 
			// SelectOrganisationGroupbox
			// 
			this.SelectOrganisationGroupbox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|fe542d84-138f-48c8-a601-cb8644eff8e5", "1. Select Organization (Required)");
			this.SelectOrganisationGroupbox.Controls.Add(this.IsConsigneeCheckBox);
			this.SelectOrganisationGroupbox.Controls.Add(this.IsConsignorCheckBox);
			this.SelectOrganisationGroupbox.Controls.Add(this.OrganisationFindbox);
			this.SelectOrganisationGroupbox.Controls.Add(this.OrganisationInstructionsLabel);
			this.SelectOrganisationGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 3, true);
			this.SelectOrganisationGroupbox.Name = "SelectOrganisationGroupbox";
			this.SelectOrganisationGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 128, true);
			this.SelectOrganisationGroupbox.TabIndex = 2;
			this.SelectOrganisationGroupbox.TabStop = false;
			// 
			// IsConsigneeCheckBox
			// 
			this.IsConsigneeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsConsigneeCheckBox, "SA_IncludeConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).SA_IncludeConsignee)));
			this.IsConsigneeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsConsigneeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 82, true);
			this.IsConsigneeCheckBox.Name = "IsConsigneeCheckBox";
			this.IsConsigneeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 17, true);
			this.IsConsigneeCheckBox.TabIndex = 2;
			// 
			// IsConsignorCheckBox
			// 
			this.IsConsignorCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsConsignorCheckBox, "SA_IncludeConsignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).SA_IncludeConsignor)));
			this.IsConsignorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsConsignorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 102, true);
			this.IsConsignorCheckBox.Name = "IsConsignorCheckBox";
			this.IsConsignorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 17, true);
			this.IsConsignorCheckBox.TabIndex = 3;
			// 
			// OrganisationFindbox
			// 
			this.BindingSource.SetBindingMember(this.OrganisationFindbox, "SA_Organisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).SA_Organisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).OrganisationList)));
			this.OrganisationFindbox.BindToList = "OrganisationList";
			this.OrganisationFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 56, true);
			this.OrganisationFindbox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OrganisationFindbox.Name = "OrganisationFindbox";
			this.OrganisationFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.OrganisationFindbox.TabIndex = 1;
			// 
			// OrganisationInstructionsLabel
			// 
			this.OrganisationInstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.OrganisationInstructionsLabel.Name = "OrganisationInstructionsLabel";
			this.OrganisationInstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 40, true);
			this.OrganisationInstructionsLabel.TabIndex = 18;
			this.OrganisationInstructionsLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// ETAFromDateEdit
			// 
			this.ETAFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.ETAFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ETAFromDateEdit, "SA_ETAFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).SA_ETAFrom)));
			this.ETAFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 84, true);
			this.ETAFromDateEdit.Name = "ETAFromDateEdit";
			this.ETAFromDateEdit.TabIndex = 6;
			// 
			// SelectDatesGroupBox
			// 
			this.SelectDatesGroupBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|d25ee4d0-8227-4dac-90f4-8b5cb0e286b5", "2. Select Dates (Optional)");
			this.SelectDatesGroupBox.Controls.Add(this.DatesInstructionsLabel);
			this.SelectDatesGroupBox.Controls.Add(this.JobsClosedToDateEdit);
			this.SelectDatesGroupBox.Controls.Add(this.JobsClosedFromDateEdit);
			this.SelectDatesGroupBox.Controls.Add(this.ETAToDateEdit);
			this.SelectDatesGroupBox.Controls.Add(this.ETAFromDateEdit);
			this.SelectDatesGroupBox.Controls.Add(this.ETDToDateEdit);
			this.SelectDatesGroupBox.Controls.Add(this.ETDFromDateEdit);
			this.SelectDatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 137, true);
			this.SelectDatesGroupBox.Name = "SelectDatesGroupBox";
			this.SelectDatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 128, true);
			this.SelectDatesGroupBox.TabIndex = 3;
			this.SelectDatesGroupBox.TabStop = false;
			// 
			// DatesInstructionsLabel
			// 
			this.DatesInstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.DatesInstructionsLabel.Name = "DatesInstructionsLabel";
			this.DatesInstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 40, true);
			this.DatesInstructionsLabel.TabIndex = 0;
			this.DatesInstructionsLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// JobsClosedToDateEdit
			// 
			this.JobsClosedToDateEdit.AutoCompleteMonthThreshold = 1;
			this.JobsClosedToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JobsClosedToDateEdit, "SA_JobClosedTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).SA_JobClosedTo)));
			this.JobsClosedToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 107, true);
			this.JobsClosedToDateEdit.Name = "JobsClosedToDateEdit";
			this.JobsClosedToDateEdit.TabIndex = 12;
			// 
			// JobsClosedFromDateEdit
			// 
			this.JobsClosedFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.JobsClosedFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JobsClosedFromDateEdit, "SA_JobClosedFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).SA_JobClosedFrom)));
			this.JobsClosedFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 107, true);
			this.JobsClosedFromDateEdit.Name = "JobsClosedFromDateEdit";
			this.JobsClosedFromDateEdit.TabIndex = 10;
			// 
			// ETDToDateEdit
			// 
			this.ETDToDateEdit.AutoCompleteMonthThreshold = 1;
			this.ETDToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ETDToDateEdit, "SA_ETDTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).SA_ETDTo)));
			this.ETDToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 60, true);
			this.ETDToDateEdit.Name = "ETDToDateEdit";
			this.ETDToDateEdit.TabIndex = 4;
			// 
			// ETDFromDateEdit
			// 
			this.ETDFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.ETDFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ETDFromDateEdit, "SA_ETDFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).SA_ETDFrom)));
			this.ETDFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 60, true);
			this.ETDFromDateEdit.Name = "ETDFromDateEdit";
			this.ETDFromDateEdit.TabIndex = 2;
			// 
			// AddToCDButton
			// 
			this.AddToCDButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|264bdc20-a363-4745-be29-83060802d00f", "Find Matches and Add to CD");
			this.AddToCDButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(622, 239, true);
			this.AddToCDButton.Name = "AddToCDButton";
			this.AddToCDButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 23, true);
			this.AddToCDButton.TabIndex = 4;
			this.AddToCDButton.Click += new System.EventHandler(this.AddToCDButton_Click);
			// 
			// SelectTypesGroupBox
			// 
			this.SelectTypesGroupBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|60db09a3-7476-49bf-96fd-d649d78f80c6", "3. Select Search Type (Required)");
			this.SelectTypesGroupBox.Controls.Add(this.SearchTypeGrid);
			this.SelectTypesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 3, true);
			this.SelectTypesGroupBox.Name = "SelectTypesGroupBox";
			this.SelectTypesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(374, 209, true);
			this.SelectTypesGroupBox.TabIndex = 0;
			this.SelectTypesGroupBox.TabStop = false;
			// 
			// SearchTypeGrid
			// 
			this.SearchTypeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SearchTypeGrid, "SearchTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).SearchTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.SearchType)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).SearchTypes)).SyncRoot)).IsFilterOn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.SearchType)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).SearchTypes)).SyncRoot)).Name)));
			this.SearchTypeGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|53542dc2-eb82-4ab1-bf07-7ee017d22ecd", "Include");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsFilterOn";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|70399d09-5866-4a24-a1ee-d7a97f31581c", "Search Type");
			zTextBoxColumnStyleInfo5.ColumnName = "Name";
			this.SearchTypeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.SearchTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SearchTypeGrid.GridId = "a1a1400d-6f67-41e7-a37a-2f8f58226c25";
			this.SearchTypeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SearchTypeGrid.LayoutKey = "SearchTypeGrid";
			this.SearchTypeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 20, true);
			this.SearchTypeGrid.Name = "SearchTypeGrid";
			this.SearchTypeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 183, true);
			this.SearchTypeGrid.TabIndex = 0;
			// 
			// RelatedEDocsCheckBox
			// 
			this.RelatedEDocsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RelatedEDocsCheckBox, "SA_IncludeRelatedEDocs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.ArchiveEDocsManager)(null)).SA_IncludeRelatedEDocs)));
			this.RelatedEDocsCheckBox.Checked = true;
			this.RelatedEDocsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.RelatedEDocsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RelatedEDocsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 243, true);
			this.RelatedEDocsCheckBox.Name = "RelatedEDocsCheckBox";
			this.RelatedEDocsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 17, true);
			this.RelatedEDocsCheckBox.TabIndex = 1;
			// 
			// ArchiveEDocsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 672, true);
			this.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ArchiveEDocsForm|113059b8-6d01-42d7-a866-241bc64a91df", "Create eDocs CD");
			this.Controls.Add(this.ArchivedDocumentsGroupBox);
			this.Controls.Add(this.SelectTypesGroupBox);
			this.Controls.Add(this.BurnCDButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.SelectOrganisationGroupbox);
			this.Controls.Add(this.RelatedEDocsCheckBox);
			this.Controls.Add(this.AddToCDButton);
			this.Controls.Add(this.SelectDatesGroupBox);
			this.DataSourceAssemblyName = "DocumentScanning";
			this.DataSourceType = typeof(Enterprise.DocumentScanning.Business.ArchiveEDocsManager);
			this.DataSourceTypeName = "Enterprise.DocumentScanning.Business.ArchiveEDocsManager";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "ArchiveEDocsForm";
			this.Controls.SetChildIndex(this.SelectDatesGroupBox, 0);
			this.Controls.SetChildIndex(this.AddToCDButton, 0);
			this.Controls.SetChildIndex(this.RelatedEDocsCheckBox, 0);
			this.Controls.SetChildIndex(this.SelectOrganisationGroupbox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.BurnCDButton, 0);
			this.Controls.SetChildIndex(this.SelectTypesGroupBox, 0);
			this.Controls.SetChildIndex(this.ArchivedDocumentsGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ArchivedDocumentsGroupBox.ResumeLayout(false);
			this.ArchivedDocumentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ListToArchiveGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ArchivedDocumentsGrid)).EndInit();
			this.SelectOrganisationGroupbox.ResumeLayout(false);
			this.SelectOrganisationGroupbox.PerformLayout();
			this.SelectDatesGroupBox.ResumeLayout(false);
			this.SelectTypesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SearchTypeGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
