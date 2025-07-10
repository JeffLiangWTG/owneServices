using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	partial class ZStmNoteUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			var zNoteDescriptionColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZNoteDescriptionColumnStyleInfo();
			var zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			var zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			var zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			var zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			var zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			var zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			var zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			var zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			var zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			groupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			NoteGrid = new Enterprise.ZArchitecture.GUI.ZStmNoteGrid();
			NoteSplitter = new CargoWise.Windows.UI.KSplitter();
			BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			ShowRelatedNotesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			ShowNotesForAllCompaniesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			NoteRichTextBox = new Enterprise.ZArchitecture.GUI.Internal.ZStmNoteRichTextBox();
			SecurityPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			SecurityDeniedLabel = new Enterprise.ZArchitecture.ZLabel();
			NotePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)BindingSource).BeginInit();
			groupBox1.SuspendLayout();
			TopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)NoteGrid).BeginInit();
			BottomPanel.SuspendLayout();
			SecurityPanel.SuspendLayout();
			NotePanel.SuspendLayout();
			SuspendLayout();
			// 
			// BindingSource
			// 
			BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.Notes);
			// 
			// groupBox1
			// 
			groupBox1.Controls.Add(TopPanel);
			groupBox1.Controls.Add(NoteSplitter);
			groupBox1.Controls.Add(BottomPanel);
			groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			LabelCaptionRenderProvider.SetLabelCaptionVisible(groupBox1, false);
			groupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 522, true);
			groupBox1.TabIndex = 0;
			groupBox1.TabStop = false;
			// 
			// TopPanel
			//
			TopPanel.Controls.Add(NoteGrid);
			TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			TopPanel.Name = "TopPanel";
			TopPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 0, 8, 0, true);
			TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 295, true);
			TopPanel.TabIndex = 0;
			// 
			// NoteGrid
			// 
			NoteGrid.AllowNavigation = false;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((System.Collections.IList)((Notes)null).VisibleNotes);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_Description);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_NoteType_DescriptiveText);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_NoteSource);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_CreatedByUserInitials);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_LastModifiedByUserName);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_NoteContextModuleCaption);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_NoteContextDirectionCaption);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_NoteContextFreightModeCaption);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_NoteDataAsTextConcatenatedAndTrimmed);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_CreatedByUserName);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((System.Collections.IList)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_Description_List);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((System.Collections.IList)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_NoteType_List);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((System.Collections.IList)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_NoteContextModule_List);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((System.Collections.IList)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_NoteContextDirection_List);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((System.Collections.IList)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_NoteContextFreightMode_List);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZDateTime)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_CreatedDateUtc);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZDateTime)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_LastModifiedDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZDateTime)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_CreatedDateLocal);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZDateTime)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_LastModifiedDateLocal);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZGuid)((StmNote)((System.Collections.IList)((Notes)null).VisibleNotes).SyncRoot).ST_GC_RelatedCompany);
			NoteGrid.CaptionVisible = false;
			zNoteDescriptionColumnStyleInfo1.BindToList = "ST_Description_List";
			zNoteDescriptionColumnStyleInfo1.ColumnName = "ST_Description";
			zNoteDescriptionColumnStyleInfo1.IsMandatory = true;
			zNoteDescriptionColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zNoteDescriptionColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zDropEditColumnStyleInfo1.BindToList = "ST_NoteType_List";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|8dea942b-4a42-46ca-ac0f-b4473b146e5d", "Visibility");
			zDropEditColumnStyleInfo1.ColumnName = "ST_NoteType_DescriptiveText";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|3ee0600b-fc5b-435c-a575-b30ce6b31c4e", "Source");
			zTextBoxColumnStyleInfo1.ColumnName = "ST_NoteSource";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|8c8866f8-82dd-465e-88a9-ba32aa0441ed", "Created (UTC)", "Date Created (UTC)", "");
			zDateEditColumnStyleInfo1.ColumnName = "ST_CreatedDateUtc";
			zDateEditColumnStyleInfo1.GroupName = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNotUserControl|248fb6cb-e215-421b-a602-495032f752e7", "Created");
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|f36a0b22-3602-4004-be22-b834b27e3710", "By", "User", "The user who created this note.");
			zTextBoxColumnStyleInfo2.ColumnName = "ST_CreatedByUserInitials";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNotUserControl|248fb6cb-e215-421b-a602-495032f752e7", "Created");
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|c19fab8a-5ed1-4bee-bb8b-ef57afa737b2", "Last Modified (UTC)");
			zDateEditColumnStyleInfo2.ColumnName = "ST_LastModifiedDate";
			zDateEditColumnStyleInfo2.GroupName = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNotUserControl|6d9d1042-cc7d-47c2-8f71-0bf6b6018133", "Last Modified");
			zDateEditColumnStyleInfo2.IsVisible = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|1f76c1a3-f7f2-4067-a688-2a65d5072808", "Created (local)", "Date Created (local)", "");
			zDateEditColumnStyleInfo3.ColumnName = "ST_CreatedDateLocal";
			zDateEditColumnStyleInfo3.GroupName = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNotUserControl|248fb6cb-e215-421b-a602-495032f752e7", "Created");
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|3f4a8361-cdf1-456f-a970-c2409caf4920", "Last Modified (local)");
			zDateEditColumnStyleInfo4.ColumnName = "ST_LastModifiedDateLocal";
			zDateEditColumnStyleInfo4.GroupName = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNotUserControl|6d9d1042-cc7d-47c2-8f71-0bf6b6018133", "Last Modified");
			zDateEditColumnStyleInfo4.IsVisible = true;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|fcd8faaa-4749-4599-ad8f-9e50aa9be38a", "By");
			zTextBoxColumnStyleInfo3.ColumnName = "ST_LastModifiedByUserName";
			zTextBoxColumnStyleInfo3.GroupName = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNotUserControl|6d9d1042-cc7d-47c2-8f71-0bf6b6018133", "Last Modified");
			zTextBoxColumnStyleInfo3.IsVisible = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo2.BindToList = "ST_NoteContextModule_List";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|dd96c008-20ef-4ed4-8b07-b1eec1f577b8", "Module", "Module Context", "");
			zDropEditColumnStyleInfo2.ColumnName = "ST_NoteContextModuleCaption";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|b771cb2a-86b0-4025-962c-10668ca741c7", "Note Context");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zDropEditColumnStyleInfo3.BindToList = "ST_NoteContextDirection_List";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|9da9b8a1-b909-4f50-8aa0-a2959d3f4121", "Direction", "Direction Context", "");
			zDropEditColumnStyleInfo3.ColumnName = "ST_NoteContextDirectionCaption";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|b771cb2a-86b0-4025-962c-10668ca741c7", "Note Context");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			zDropEditColumnStyleInfo4.BindToList = "ST_NoteContextFreightMode_List";
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|f503e5d3-49eb-44fc-82e5-17c512cc5904", "Freight", "Freight Context", "");
			zDropEditColumnStyleInfo4.ColumnName = "ST_NoteContextFreightModeCaption";
			zDropEditColumnStyleInfo4.GroupName = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|b771cb2a-86b0-4025-962c-10668ca741c7", "Note Context");
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|19c53a5d-c4f8-43ea-be45-3f1ca321d49d", "Note Text");
			zTextBoxColumnStyleInfo4.ColumnName = "ST_NoteDataAsTextConcatenatedAndTrimmed";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|ae375b22-f203-46c0-b773-0157513b279d", "By User Full Name");
			zTextBoxColumnStyleInfo5.ColumnName = "ST_CreatedByUserName";
			zTextBoxColumnStyleInfo5.IsVisible = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|31f359d9-ca6f-4f8a-8bbd-7de7e7614bfc", "Company");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ST_GC_RelatedCompany";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbCompany;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			NoteGrid.ColumnStyles.Add(zNoteDescriptionColumnStyleInfo1);
			NoteGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			NoteGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			NoteGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			NoteGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			NoteGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			NoteGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			NoteGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			NoteGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			NoteGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			NoteGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			NoteGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			NoteGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			NoteGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			NoteGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			NoteGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			NoteGrid.GridId = "41242c9a-bb31-4c2a-b5cc-2db18db8f2f1";
			NoteGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			NoteGrid.LayoutKey = "NoteGrid";
			NoteGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			NoteGrid.Name = "NoteGrid";
			NoteGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 295, true);
			NoteGrid.TabIndex = 0;
			// 
			// NoteSplitter
			// 
			NoteSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			NoteSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 311, true);
			NoteSplitter.MinExtra = 60;
			NoteSplitter.MinSize = 80;
			NoteSplitter.Name = "NoteSplitter";
			NoteSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 8, true);
			NoteSplitter.TabIndex = 1;
			NoteSplitter.TabStop = false;
			// 
			// BottomPanel
			// 
			BottomPanel.Controls.Add(ShowRelatedNotesCheckBox);
			BottomPanel.Controls.Add(ShowNotesForAllCompaniesCheckBox);
			BottomPanel.Controls.Add(NoteRichTextBox);
			BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 319, true);
			BottomPanel.Name = "BottomPanel";
			BottomPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 0, 8, 8, true);
			BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 200, true);
			BottomPanel.TabIndex = 2;
			// 
			// ShowRelatedNotesCheckBox
			// 
			ShowRelatedNotesCheckBox.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			BindingSource.SetBindingMember(ShowRelatedNotesCheckBox, "ShowRelatedNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((bool)((Enterprise.ZArchitecture.Business.Notes)null).ShowRelatedNotes);
			ShowRelatedNotesCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|ac6aeb6f-f3a7-435e-9e22-02d55d827840", "Show &Related Notes");
			ShowRelatedNotesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			LabelCaptionRenderProvider.SetLabelCaptionVisible(ShowRelatedNotesCheckBox, false);
			ShowRelatedNotesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 0, true);
			ShowRelatedNotesCheckBox.Name = "ShowRelatedNotesCheckBox";
			ShowRelatedNotesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 24, true);
			ShowRelatedNotesCheckBox.TabIndex = 2;
			// 
			// ShowNotesForAllCompaniesCheckBox
			// 
			ShowNotesForAllCompaniesCheckBox.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			BindingSource.SetBindingMember(ShowNotesForAllCompaniesCheckBox, "ShowNotesForAllCompanies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((bool)((Enterprise.ZArchitecture.Business.Notes)null).ShowNotesForAllCompanies);
			ShowNotesForAllCompaniesCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|ED4925DF-9262-4633-91D6-5BD93791279F", "Sho&w Notes for All Companies");
			ShowNotesForAllCompaniesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			LabelCaptionRenderProvider.SetLabelCaptionVisible(ShowNotesForAllCompaniesCheckBox, false);
			ShowNotesForAllCompaniesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 0, true);
			ShowNotesForAllCompaniesCheckBox.Name = "ShowNotesForAllCompaniesCheckBox";
			ShowNotesForAllCompaniesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 24, true);
			ShowNotesForAllCompaniesCheckBox.TabIndex = 1;
			// 
			// NoteRichTextBox
			// 
			NoteRichTextBox.BindToRtfNote = "ST_NoteData";
			NoteRichTextBox.BindToTextNote = "ST_NoteText";
			NoteRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			NoteRichTextBox.IsDescriptionVisibleInTextMode = true;
			NoteRichTextBox.IsModifyButtonVisible = true;
			NoteRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			NoteRichTextBox.Name = "NoteRichTextBox";
			NoteRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 192, true);
			NoteRichTextBox.TabIndex = 0;
			// 
			// SecurityPanel
			// 
			SecurityPanel.Controls.Add(SecurityDeniedLabel);
			SecurityPanel.Dock = System.Windows.Forms.DockStyle.Top;
			SecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			SecurityPanel.Name = "SecurityPanel";
			SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 24, true);
			SecurityPanel.TabIndex = 1;
			// 
			// SecurityDeniedLabel
			// 
			SecurityDeniedLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNoteUserControl|dfd44c7d-bf9e-421e-b3a3-53ffd8a45d77", "You do not have security access to modify some of these settings.");
			SecurityDeniedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			SecurityDeniedLabel.ForeColor = System.Drawing.Color.Red;
			SecurityDeniedLabel.IsFontBold = true;
			SecurityDeniedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			SecurityDeniedLabel.Name = "SecurityDeniedLabel";
			SecurityDeniedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 24, true);
			SecurityDeniedLabel.TabIndex = 0;
			SecurityDeniedLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// NotePanel
			// 
			NotePanel.Controls.Add(groupBox1);
			NotePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			NotePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			NotePanel.Name = "NotePanel";
			NotePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			NotePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 528, true);
			NotePanel.TabIndex = 0;
			// 
			// ZStmNoteUserControl
			// 
			CaptionRenderingEnabled = true;
			Controls.Add(NotePanel);
			Controls.Add(SecurityPanel);
			Name = "ZStmNoteUserControl";
			Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 552, true);
			((System.ComponentModel.ISupportInitialize)BindingSource).EndInit();
			groupBox1.ResumeLayout(false);
			TopPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)NoteGrid).EndInit();
			BottomPanel.ResumeLayout(false);
			SecurityPanel.ResumeLayout(false);
			NotePanel.ResumeLayout(false);
			ResumeLayout(false);
		}

		CargoWise.Windows.UI.KSplitter NoteSplitter;
		ZPanel BottomPanel;
		ZPanel TopPanel;
		Enterprise.ZArchitecture.GUI.ZCheckBox ShowRelatedNotesCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox ShowNotesForAllCompaniesCheckBox;
		internal ZPanel SecurityPanel;
		ZPanel NotePanel;
		ZLabel SecurityDeniedLabel;
		internal ZStmNoteGrid NoteGrid;
		internal ZStmNoteRichTextBox NoteRichTextBox;
		ZGroupBox groupBox1;

#endregion
	}
}
