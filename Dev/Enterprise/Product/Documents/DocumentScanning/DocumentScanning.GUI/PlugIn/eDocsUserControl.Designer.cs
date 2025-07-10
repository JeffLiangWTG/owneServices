using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class eDocsUserControl : ZUserControl, IReadOnlyToggleControl
	{
		CargoWise.Windows.UI.KSplitContainer eDocsSplitContainer;
		internal Enterprise.ZArchitecture.GUI.ZPictureBox IconPictureBox;
		internal MasterFiles.GUI.RequiredDocumentsUserControl RequiredDocumentsUserControl;
		ZGroupBox DocStorageGroupBox;
		internal ZCheckBox CompanySpecificCheckBox;
		GraphicalDisplayControl documentPreviewEDocsControl;
		internal ZPanel DocumentPreviewPanel;
		internal ZCheckBox PreviewDocumentsCheckBox;
		internal ZCheckBox DepartmentSpecificCheckBox;
		internal ZCheckBox BranchSpecificCheckBox;
		internal ZCheckBox ShowDeletedDocumentsCheckBox;
		internal ZButton GenerateNumberButton;
		internal ZButton AddEDocsButton;
		internal ZPanel DescriptionAndIconPanel;
		internal ZLabel ProgramNameLabel;
		internal ZTextBox DocumentStorageLocationTextBox;
		internal DocumentsZGrid StorageDocsGrid;
		internal ZButton RefreshButton;
		internal ZLabel NotEditableLabel;
		internal ZLabel SpecificDocumentLabel;
		internal ZGrid RelatedParentsGrid;
		KSplitContainer SplitContainerMain;

		ZForm parentForm;
		IPreviewableDocument previewableDocument;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();

			var auditDetailsResString = Res.GetData("01BCB826-71A7-4CA8-8035-4B9CB4DBF582", "Audit Details");

			this.eDocsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.DocStorageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StorageDocsGrid = new Enterprise.DocumentScanning.GUI.DocumentsZGrid();
			this.DescriptionAndIconPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DocumentStorageLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreviewDocumentsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GenerateNumberButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DepartmentSpecificCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RefreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddEDocsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BranchSpecificCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CompanySpecificCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SpecificDocumentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ShowDeletedDocumentsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ProgramNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IconPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.NotEditableLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RequiredDocumentsUserControl = new Enterprise.MasterFiles.GUI.RequiredDocumentsUserControl();
			this.RelatedParentsGrid = new ZGrid();
			this.DocumentPreviewPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.documentPreviewEDocsControl = new Enterprise.DocumentScanning.GUI.GraphicalDisplayControl();
			this.SplitContainerMain = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.eDocsSplitContainer)).BeginInit();
			this.eDocsSplitContainer.Panel1.SuspendLayout();
			this.eDocsSplitContainer.Panel2.SuspendLayout();
			this.eDocsSplitContainer.SuspendLayout();
			this.DocStorageGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StorageDocsGrid)).BeginInit();
			this.StorageDocsGrid.SuspendLayout();
			this.DescriptionAndIconPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).BeginInit();
			this.RequiredDocumentsUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedParentsGrid)).BeginInit();
			this.RelatedParentsGrid.SuspendLayout();
			this.DocumentPreviewPanel.SuspendLayout();
			this.documentPreviewEDocsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainerMain)).BeginInit();
			this.SplitContainerMain.Panel1.SuspendLayout();
			this.SplitContainerMain.Panel2.SuspendLayout();
			this.SplitContainerMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentScanning.Business.StorageMain);
			// 
			// eDocsSplitContainer
			// 
			this.eDocsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eDocsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.eDocsSplitContainer.Name = "eDocsSplitContainer";
			this.eDocsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// eDocsSplitContainer.Panel1
			// 
			this.eDocsSplitContainer.Panel1.Controls.Add(this.DocStorageGroupBox);
			// 
			// eDocsSplitContainer.Panel2
			// 
			this.eDocsSplitContainer.Panel2.Controls.Add(this.RequiredDocumentsUserControl);
			this.eDocsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 567, true);
			this.eDocsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(349);
			this.eDocsSplitContainer.TabIndex = 99;
			// 
			// DocStorageGroupBox
			// 
			this.DocStorageGroupBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|19eb0b8b-a819-4e48-b1a5-e690f5ee42d1", "Document Storage");
			this.DocStorageGroupBox.Controls.Add(this.StorageDocsGrid);
			this.DocStorageGroupBox.Controls.Add(this.DescriptionAndIconPanel);
			this.DocStorageGroupBox.Controls.Add(this.NotEditableLabel);
			this.DocStorageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocStorageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocStorageGroupBox.Name = "DocStorageGroupBox";
			this.DocStorageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 349, true);
			this.DocStorageGroupBox.TabIndex = 17;
			this.DocStorageGroupBox.TabStop = false;
			// 
			// StorageDocsGrid
			// 
			this.StorageDocsGrid.AllowDrop = true;
			this.StorageDocsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.StorageDocsGrid, "RelatedParentMains.eDocsView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_Date)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_DateLocalBranchTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_DocType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_DocType_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_DocType_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_DescMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_FileNameWithExtension)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_FriendlyFileDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_IsSystemGenerated)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_IsDeleted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_IsPublished)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_LastEditingUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_LastEditingUserFullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_SaveVersions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).CompanyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).BranchCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).DepartmentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).HumanReadableAttachmentSize)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_DocSource_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_RDS_NKDocSource)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).SC_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDocsBase)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).eDocsView)).SyncRoot)).ParseStatus)));
			this.StorageDocsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "SC_Date";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.ToolTip = "Date that this eDoc was last changed";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zDateEditColumnStyleInfo2.ColumnName = "SC_DateLocalBranchTime";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zDropEditColumnStyleInfo1.BindToList = "SC_DocType_List";
			zDropEditColumnStyleInfo1.ColumnName = "SC_DocType";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.ToolTip = "The 3-letter Document Type for this eDoc";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo1.ColumnName = "SC_DocType_Description";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
			zTextBoxColumnStyleInfo2.ColumnName = "SC_DescMultilingual";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ToolTip = "Description for this eDoc";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo3.ColumnName = "SC_FileNameWithExtension";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ToolTip = "Filename for this eDoc, if applicable";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo4.ColumnName = "SC_FriendlyFileDescription";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ToolTip = "Program used to open this file";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zCheckBoxColumnStyleInfo1.ColumnName = "SC_IsSystemGenerated";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.ToolTip = null;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zCheckBoxColumnStyleInfo2.ColumnName = "SC_IsDeleted";
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.ToolTip = "Indicates whether this eDoc has been deleted and is waiting for permanent deletio" +
	"n or restoration";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zCheckBoxColumnStyleInfo3.ColumnName = "SC_IsPublished";
			zCheckBoxColumnStyleInfo3.IsReadOnly = true;
			zCheckBoxColumnStyleInfo3.ToolTip = "Indicates whether this eDoc is published to the web.";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo5.ColumnName = "SC_LastEditingUser";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.ToolTip = "The initials of the user that last edited this eDoc";
			zTextBoxColumnStyleInfo5.GroupName = auditDetailsResString;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo6.ColumnName = "SC_LastEditingUserFullName";
			zTextBoxColumnStyleInfo6.GroupName = auditDetailsResString;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.ToolTip = "The name and initials of the user that last edited this eDoc";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo4.ColumnName = "SC_SaveVersions";
			zCheckBoxColumnStyleInfo4.IsReadOnly = true;
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|CompanyCode", "Company", "visible company");
			zTextBoxColumnStyleInfo7.ColumnName = "CompanyCode";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|SC_GB_Branch", "Branch", "visible branch");
			zTextBoxColumnStyleInfo8.ColumnName = "BranchCode";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|SC_GE_Department", "Department", "visible department");
			zTextBoxColumnStyleInfo9.ColumnName = "DepartmentCode";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo10.ColumnName = "HumanReadableAttachmentSize";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo11.ColumnName = "SC_DocSource_Description";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
			zTextBoxColumnStyleInfo12.ColumnName = "SC_RDS_NKDocSource";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
			zTextBoxColumnStyleInfo13.ColumnName = "SC_Language";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
			zDateEditColumnStyleInfo3.ColumnName = "SC_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo3.GroupName = auditDetailsResString;
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo15.ColumnName = "ParseStatus";
			zTextBoxColumnStyleInfo15.IsReadOnly = true;
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
			zTextBoxColumnStyleInfo18.GroupName = auditDetailsResString;
			zTextBoxColumnStyleInfo18.ColumnName = "SC_SystemCreateUserFullName";
			zTextBoxColumnStyleInfo18.IsReadOnly = true;
			zTextBoxColumnStyleInfo18.IsVisible = false;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo16.GroupName = auditDetailsResString;
			zTextBoxColumnStyleInfo16.ColumnName = "SC_SystemCreateUser";
			zTextBoxColumnStyleInfo16.IsReadOnly = true;
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zDateEditColumnStyleInfo5.GroupName = auditDetailsResString;
			zDateEditColumnStyleInfo5.ColumnName = "SC_SystemLastEditTimeUtc";
			zDateEditColumnStyleInfo5.IsReadOnly = true;
			zDateEditColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);

			this.StorageDocsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.StorageDocsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.StorageDocsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.StorageDocsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.StorageDocsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.StorageDocsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.StorageDocsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.StorageDocsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.StorageDocsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			
			this.StorageDocsGrid.DisableImportDataMenuItem = true;
			this.StorageDocsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StorageDocsGrid.DocumentManipulationTarget = null;
			this.StorageDocsGrid.DragDropTarget = null;
			this.StorageDocsGrid.GridId = "ead75410-880d-412f-a33c-03737ec76b05";
			this.StorageDocsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StorageDocsGrid.IsWholeRowSelectedOnClick = true;
			this.StorageDocsGrid.LayoutKey = "StorageDocsGrid";
			this.StorageDocsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.StorageDocsGrid.Name = "StorageDocsGrid";
			this.StorageDocsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.StorageDocsGrid.ShowCopyLinkMenuItem = true;
			this.StorageDocsGrid.ShowCopyMenuItem = true;
			this.StorageDocsGrid.ShowCutMenuItem = true;
			this.StorageDocsGrid.ShowDeleteMenuItem = true;
			this.StorageDocsGrid.ShowDeletePermanentlyMenuItem = true;
			this.StorageDocsGrid.ShowDeliverDocumentMenuItem = true;
			this.StorageDocsGrid.ShowEditPropertiesMenuItem = true;
			this.StorageDocsGrid.ShowParseDocumentMenuItem = false;
			this.StorageDocsGrid.ShowPasteMenuItem = true;
			this.StorageDocsGrid.ShowRestoreMenuItem = true;
			this.StorageDocsGrid.ShowSplitDocumentMenuItem = true;
			this.StorageDocsGrid.ShowViewMenuItem = true;
			this.StorageDocsGrid.ShowReviewParsedResultsMenuItem = false;
			this.StorageDocsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 177, true);
			this.StorageDocsGrid.TabIndex = 18;

			// TODO This if statement will be removed when Shipamax feature is released to clients. 
			if (EDocsParsingHelper.IsDocumentParsingEnabled())
			{
				this.StorageDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
				this.StorageDocsGrid.ShowParseDocumentMenuItem = true;
				this.StorageDocsGrid.ShowReviewParsedResultsMenuItem = true;
			}
			// 
			// DescriptionAndIconPanel
			// 
			this.DescriptionAndIconPanel.AutoScroll = true;
			this.DescriptionAndIconPanel.Controls.Add(this.DocumentStorageLocationTextBox);
			this.DescriptionAndIconPanel.Controls.Add(this.PreviewDocumentsCheckBox);
			this.DescriptionAndIconPanel.Controls.Add(this.GenerateNumberButton);
			this.DescriptionAndIconPanel.Controls.Add(this.DepartmentSpecificCheckBox);
			this.DescriptionAndIconPanel.Controls.Add(this.RefreshButton);
			this.DescriptionAndIconPanel.Controls.Add(this.AddEDocsButton);
			this.DescriptionAndIconPanel.Controls.Add(this.BranchSpecificCheckBox);
			this.DescriptionAndIconPanel.Controls.Add(this.CompanySpecificCheckBox);
			this.DescriptionAndIconPanel.Controls.Add(this.SpecificDocumentLabel);
			this.DescriptionAndIconPanel.Controls.Add(this.ShowDeletedDocumentsCheckBox);
			this.DescriptionAndIconPanel.Controls.Add(this.ProgramNameLabel);
			this.DescriptionAndIconPanel.Controls.Add(this.IconPictureBox);
			this.DescriptionAndIconPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.DescriptionAndIconPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 193, true);
			this.DescriptionAndIconPanel.Name = "DescriptionAndIconPanel";
			this.DescriptionAndIconPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 130, true);
			this.DescriptionAndIconPanel.TabIndex = 20;
			// 
			// DocumentStorageLocationTextBox
			// 
			this.DocumentStorageLocationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DocumentStorageLocationTextBox, "SM_PhysicalLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).SM_PhysicalLocation)));
			this.DocumentStorageLocationTextBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|cf214737-b416-4038-99a4-6f081a66dd23", "Storage Location");
			this.DocumentStorageLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 94, true);
			this.DocumentStorageLocationTextBox.Name = "DocumentStorageLocationTextBox";
			this.DocumentStorageLocationTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DocumentStorageLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.DocumentStorageLocationTextBox.TabIndex = 10;
			// 
			// PreviewDocumentsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PreviewDocumentsCheckBox, "RelatedParentMains.ShowPreview");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).ShowPreview)));
			this.PreviewDocumentsCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|2EE6B146-ECF1-4B13-A139-684E91DCA0DE", "Enable Preview");
			this.PreviewDocumentsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PreviewDocumentsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(38, 51, true);
			this.PreviewDocumentsCheckBox.Name = "PreviewDocumentsCheckBox";
			this.PreviewDocumentsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 17, true);
			this.PreviewDocumentsCheckBox.TabIndex = 5;
			this.PreviewDocumentsCheckBox.UseVisualStyleBackColor = true;
			this.PreviewDocumentsCheckBox.EditableInViewMode = true;
			this.PreviewDocumentsCheckBox.CheckedChanged += new System.EventHandler(this.PreviewDocumentsCheckBox_CheckStateChanged);
			// 
			// GenerateNumberButton
			// 
			this.GenerateNumberButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.GenerateNumberButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|e0ea66c9-c3db-4636-8ff2-03bb4af49aaf", "Generate Storage Number");
			this.GenerateNumberButton.IsCaptionOverridden = false;
			this.GenerateNumberButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 91, true);
			this.GenerateNumberButton.Name = "GenerateNumberButton";
			this.GenerateNumberButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.GenerateNumberButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 23, true);
			this.GenerateNumberButton.TabIndex = 11;
			this.GenerateNumberButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.GenerateNumberButton.ToolTipCaption = null;
			this.GenerateNumberButton.UseVisualStyleBackColor = true;
			this.GenerateNumberButton.Click += new System.EventHandler(this.GenerateNumberButton_Click);
			// 
			// DepartmentSpecificCheckBox
			// 
			this.DepartmentSpecificCheckBox.AutoSize = true;
			this.DepartmentSpecificCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.DepartmentSpecificCheckBox, "RelatedParentMains.ShowDocumentsForAllDepartments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).ShowDocumentsForAllDepartments)));
			this.DepartmentSpecificCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|FD191B9B-AC90-40A9-8790-226E49B54B39", "All Departments");
			this.DepartmentSpecificCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DepartmentSpecificCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(471, 51, true);
			this.DepartmentSpecificCheckBox.Name = "DepartmentSpecificCheckBox";
			this.DepartmentSpecificCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.DepartmentSpecificCheckBox.TabIndex = 8;
			this.DepartmentSpecificCheckBox.UseVisualStyleBackColor = false;
			// 
			// RefreshButton
			// 
			this.RefreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RefreshButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|3edcb532-9e72-48c7-b2fd-2e6b19ccab09", "Refresh");
			this.RefreshButton.IsCaptionOverridden = false;
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(612, 91, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 23, true);
			this.RefreshButton.TabIndex = 12;
			this.RefreshButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RefreshButton.ToolTipCaption = null;
			this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// AddEDocsButton
			// 
			this.AddEDocsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AddEDocsButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|5c111574-6f21-4523-b704-0f54fc8b8815", "Add eDocs");
			this.AddEDocsButton.IsCaptionOverridden = false;
			this.AddEDocsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(726, 91, true);
			this.AddEDocsButton.Name = "AddEDocsButton";
			this.AddEDocsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AddEDocsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.AddEDocsButton.TabIndex = 13;
			this.AddEDocsButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AddEDocsButton.ToolTipCaption = null;
			this.AddEDocsButton.Click += new System.EventHandler(this.AddEDocsButton_Click);
			// 
			// BranchSpecificCheckBox
			// 
			this.BranchSpecificCheckBox.AutoSize = true;
			this.BranchSpecificCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.BranchSpecificCheckBox, "RelatedParentMains.ShowDocumentsForAllBranches");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).ShowDocumentsForAllBranches)));
			this.BranchSpecificCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|F01C44FE-DE47-4EFA-B467-6F0F21AAF24A", "All Branches");
			this.BranchSpecificCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BranchSpecificCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 51, true);
			this.BranchSpecificCheckBox.Name = "BranchSpecificCheckBox";
			this.BranchSpecificCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.BranchSpecificCheckBox.TabIndex = 7;
			this.BranchSpecificCheckBox.UseVisualStyleBackColor = false;
			// 
			// CompanySpecificCheckBox
			// 
			this.CompanySpecificCheckBox.AutoSize = true;
			this.CompanySpecificCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.CompanySpecificCheckBox, "RelatedParentMains.ShowDocumentsForAllCompanies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).ShowDocumentsForAllCompanies)));
			this.CompanySpecificCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|22B123F2-9D85-4D22-92BE-9CCC6F4720E3", "All Companies");
			this.CompanySpecificCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CompanySpecificCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 51, true);
			this.CompanySpecificCheckBox.Name = "CompanySpecificCheckBox";
			this.CompanySpecificCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 17, true);
			this.CompanySpecificCheckBox.TabIndex = 6;
			this.CompanySpecificCheckBox.UseVisualStyleBackColor = false;
			// 
			// SpecificDocumentLabel
			// 
			this.SpecificDocumentLabel.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|D6BCB361-64FE-4F67-85C9-AB0B7C017EE1", "Show Documents for");
			this.SpecificDocumentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SpecificDocumentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 51, true);
			this.SpecificDocumentLabel.Name = "SpecificDocumentLabel";
			this.SpecificDocumentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 17, true);
			this.SpecificDocumentLabel.TabIndex = 18;
			// 
			// ShowDeletedDocumentsCheckBox
			// 
			this.ShowDeletedDocumentsCheckBox.AutoSize = true;
			this.ShowDeletedDocumentsCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ShowDeletedDocumentsCheckBox, "RelatedParentMains.ViewIncludesDeletedDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).ViewIncludesDeletedDocuments)));
			this.ShowDeletedDocumentsCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|78350bca-86ae-4af3-a30e-cad93764b0bf", "All Deleted");
			this.ShowDeletedDocumentsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowDeletedDocumentsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(578, 51, true);
			this.ShowDeletedDocumentsCheckBox.Name = "ShowDeletedDocumentsCheckBox";
			this.ShowDeletedDocumentsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 17, true);
			this.ShowDeletedDocumentsCheckBox.TabIndex = 9;
			this.ShowDeletedDocumentsCheckBox.UseVisualStyleBackColor = false;
			this.ShowDeletedDocumentsCheckBox.CheckedChanged += new System.EventHandler(this.ShowDeletedDocumentsCheckBox_CheckedChanged);
			// 
			// ProgramNameLabel
			// 
			this.ProgramNameLabel.AutoSize = true;
			this.ProgramNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ProgramNameLabel, false);
			this.ProgramNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 8, true);
			this.ProgramNameLabel.Name = "ProgramNameLabel";
			this.ProgramNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.ProgramNameLabel.TabIndex = 3;
			// 
			// IconPictureBox
			// 
			this.IconPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 4, true);
			this.IconPictureBox.Name = "IconPictureBox";
			this.IconPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 32, true);
			this.IconPictureBox.TabIndex = 2;
			this.IconPictureBox.TabStop = false;
			// 
			// NotEditableLabel
			// 
			this.NotEditableLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.NotEditableLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.NotEditableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 323, true);
			this.NotEditableLabel.Name = "NotEditableLabel";
			this.NotEditableLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 23, true);
			this.NotEditableLabel.TabIndex = 16;
			this.NotEditableLabel.Visible = false;
			// 
			// RequiredDocumentsUserControl
			// 
			this.RequiredDocumentsUserControl.AllowDrop = true;
			this.RequiredDocumentsUserControl.AutoScroll = true;
			this.RequiredDocumentsUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.RequiredDocumentsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(((Enterprise.DocumentScanning.Business.StorageMain)(null)))));
			this.RequiredDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RequiredDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RequiredDocumentsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			this.RequiredDocumentsUserControl.Name = "RequiredDocumentsUserControl";
			this.RequiredDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 214, true);
			this.RequiredDocumentsUserControl.TabIndex = 20;
			// 
			// RelatedParentsGrid
			// 
			this.RelatedParentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RelatedParentsGrid, "RelatedParentMains");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageMain)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.StorageMain)(null)).RelatedParentMains)).SyncRoot)).DocumentOwnerDescription)));
			this.RelatedParentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eDocsUserControl|6150db7b-6fbe-4aa2-8a9d-67719145c18d", "Related eDocs");
			zTextBoxColumnStyleInfo14.ColumnName = "DocumentOwnerDescription";
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.RelatedParentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.RelatedParentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedParentsGrid.GridId = "f2b4c764-ca08-42bc-bc53-41cceadda250";
			this.RelatedParentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedParentsGrid.LayoutKey = "zGrid1";
			this.RelatedParentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedParentsGrid.Name = "RelatedParentsGrid";
			this.RelatedParentsGrid.ReadOnly = true;
			this.RelatedParentsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.RelatedParentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 567, true);
			this.RelatedParentsGrid.TabIndex = 17;
			this.RelatedParentsGrid.DoubleClick += new System.EventHandler(this.DoubleClickOn_RelatedParents);
			// 
			// DocumentPreviewPanel
			// 
			this.DocumentPreviewPanel.Controls.Add(this.documentPreviewEDocsControl);
			this.DocumentPreviewPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.DocumentPreviewPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1030, 0, true);
			this.DocumentPreviewPanel.Name = "DocumentPreviewPanel";
			this.DocumentPreviewPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 567, true);
			this.DocumentPreviewPanel.TabIndex = 21;
			this.DocumentPreviewPanel.Visible = false;
			// 
			// documentPreviewEDocsControl
			// 
			this.documentPreviewEDocsControl.AllowDrop = true;
			this.documentPreviewEDocsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.documentPreviewEDocsControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.documentPreviewEDocsControl.Document = null;
			this.documentPreviewEDocsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.documentPreviewEDocsControl.Name = "documentPreviewEDocsControl";
			this.documentPreviewEDocsControl.ReadOnly = false;
			this.documentPreviewEDocsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 567, true);
			this.documentPreviewEDocsControl.TabIndex = 21;
			// 
			// SplitContainerMain
			// 
			this.SplitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.SplitContainerMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainerMain.Name = "SplitContainerMain";
			// 
			// SplitContainerMain.Panel1
			// 
			this.SplitContainerMain.Panel1.Controls.Add(this.RelatedParentsGrid);
			// 
			// SplitContainerMain.Panel2
			// 
			this.SplitContainerMain.Panel2.Controls.Add(this.eDocsSplitContainer);
			this.SplitContainerMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1030, 567, true);
			this.SplitContainerMain.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144);
			this.SplitContainerMain.TabIndex = 100;
			// 
			// eDocsUserControl
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainerMain);
			this.Controls.Add(this.DocumentPreviewPanel);
			this.Name = "eDocsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1157, 567, true);
			this.Resize += new System.EventHandler(this.eDocsUserControl_Resize);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.eDocsSplitContainer.Panel1.ResumeLayout(false);
			this.eDocsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.eDocsSplitContainer)).EndInit();
			this.eDocsSplitContainer.ResumeLayout(false);
			this.eDocsSplitContainer.PerformLayout();
			this.DocStorageGroupBox.ResumeLayout(false);
			this.DocStorageGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.StorageDocsGrid)).EndInit();
			this.StorageDocsGrid.ResumeLayout(false);
			this.StorageDocsGrid.PerformLayout();
			this.DescriptionAndIconPanel.ResumeLayout(false);
			this.DescriptionAndIconPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.IconPictureBox)).EndInit();
			this.RequiredDocumentsUserControl.ResumeLayout(true);
			this.RequiredDocumentsUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedParentsGrid)).EndInit();
			this.RelatedParentsGrid.ResumeLayout(false);
			this.RelatedParentsGrid.PerformLayout();
			this.DocumentPreviewPanel.ResumeLayout(false);
			this.DocumentPreviewPanel.PerformLayout();
			this.documentPreviewEDocsControl.ResumeLayout(true);
			this.documentPreviewEDocsControl.PerformLayout();
			this.SplitContainerMain.Panel1.ResumeLayout(false);
			this.SplitContainerMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainerMain)).EndInit();
			this.SplitContainerMain.ResumeLayout(false);
			this.SplitContainerMain.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
