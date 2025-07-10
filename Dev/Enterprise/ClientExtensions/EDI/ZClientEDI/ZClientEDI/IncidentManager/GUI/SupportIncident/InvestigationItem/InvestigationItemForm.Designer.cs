using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;


namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class InvestigationItemForm
	{

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.kTableLayoutPanel1 = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.itemTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ResponseOptionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.investigationItemDiagnosticCriteriaModuleButtonGrid = new Enterprise.Client.EDI.IncidentManager.GUI.InvestigationItemDiagnosticCriteriaModuleButtonGrid();
			this.RelatedDiagnosticCriteriaGridLabel = new Enterprise.ZArchitecture.ZLabel();
			this.responseResultGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AskOrderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ResponseOptionResultLabel = new Enterprise.ZArchitecture.ZLabel();
			this.askClientCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.activeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.typeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.kTableLayoutPanel1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResponseOptionsGrid)).BeginInit();
			this.ResponseOptionsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.investigationItemDiagnosticCriteriaModuleButtonGrid.InnerGrid)).BeginInit();
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.responseResultGrid)).BeginInit();
			this.responseResultGrid.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.typeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 758, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.zPanel1);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 736, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 736, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 736, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 758, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.SaveButtonUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 7, true);
			this.SaveButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 25, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.kTableLayoutPanel1);
			this.zPanel1.Controls.Add(this.askClientCheckBox);
			this.zPanel1.Controls.Add(this.activeCheckBox);
			this.zPanel1.Controls.Add(this.typeDropEdit);
			this.zPanel1.Controls.Add(this.descriptionTextBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 736, true);
			this.zPanel1.TabIndex = 1;
			// 
			// kTableLayoutPanel1
			// 
			this.kTableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.kTableLayoutPanel1.ColumnCount = 1;
			this.kTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.kTableLayoutPanel1.Controls.Add(this.zGroupBox1, 0, 0);
			this.kTableLayoutPanel1.Controls.Add(this.zGroupBox2, 0, 1);
			this.kTableLayoutPanel1.Controls.Add(this.kSplitContainer1, 0, 2);
			this.kTableLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 119, true);
			this.kTableLayoutPanel1.Name = "kTableLayoutPanel1";
			this.kTableLayoutPanel1.RowCount = 3;
			this.kTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.kTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.kTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.kTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(13)));
			this.kTableLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 615, true);
			this.kTableLayoutPanel1.TabIndex = 7;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.itemTextTextBox);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox1, false);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 201, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// itemTextTextBox
			// 
			this.itemTextTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.itemTextTextBox, "INV_ItemText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).INV_ItemText)));
			this.itemTextTextBox.CaptionResourceString = ZClientEDI.Res.GetData("2631a371-113b-46bc-8d9c-8ae845d7cf88", "Item Text");
			this.itemTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.itemTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 17, true);
			this.itemTextTextBox.Multiline = true;
			this.itemTextTextBox.Name = "itemTextTextBox";
			this.itemTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.itemTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 181, true);
			this.itemTextTextBox.TabIndex = 6;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Controls.Add(this.ResponseOptionsGrid);
			this.zGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 207, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 201, true);
			this.zGroupBox2.TabIndex = 1;
			this.zGroupBox2.TabStop = false;
			this.zGroupBox2.Text = "Response Options";
			// 
			// ResponseOptionsGrid
			// 
			this.ResponseOptionsGrid.AllowNavigation = false;
			this.ResponseOptionsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ResponseOptionsGrid, "ResponseOptions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).ResponseOptions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItemResponseOption)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).ResponseOptions)).SyncRoot)).INR_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItemResponseOption)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).ResponseOptions)).SyncRoot)).INR_ResponseOption)));
			this.ResponseOptionsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("5d7f6da6-ac49-4e55-aaab-44cb72ae9e64", "#");
			zCalcEditColumnStyleInfo1.ColumnName = "INR_Sequence";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("60805549-7f34-491a-83ee-ea8991b96d86", "Option Text");
			zTextBoxColumnStyleInfo1.ColumnName = "INR_ResponseOption";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ResponseOptionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ResponseOptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ResponseOptionsGrid.CopySelectedRowsAllowed = false;
			this.ResponseOptionsGrid.GridId = "2387d058-9ed7-4bbf-b990-653be1524a9c";
			this.ResponseOptionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ResponseOptionsGrid.LayoutKey = "zGrid1";
			this.ResponseOptionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 27, true);
			this.ResponseOptionsGrid.Name = "ResponseOptionsGrid";
			this.ResponseOptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 165, true);
			this.ResponseOptionsGrid.TabIndex = 0;
			this.ResponseOptionsGrid.RowsDeleting += new System.EventHandler<Enterprise.ZArchitecture.RowsDeletingEventArgs>(this.ResponseOptionsGrid_RowDeleting);
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 411, true);
			this.kSplitContainer1.Name = "kSplitContainer1";
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.investigationItemDiagnosticCriteriaModuleButtonGrid);
			this.kSplitContainer1.Panel1.Controls.Add(this.RelatedDiagnosticCriteriaGridLabel);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.responseResultGrid);
			this.kSplitContainer1.Panel2.Controls.Add(this.zPanel2);
			this.kSplitContainer1.Panel2.Controls.Add(this.ResponseOptionResultLabel);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 201, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(280);
			this.kSplitContainer1.SplitterWidth = 16;
			this.kSplitContainer1.TabIndex = 2;
			// 
			// investigationItemDiagnosticCriteriaModuleButtonGrid
			// 
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.investigationItemDiagnosticCriteriaModuleButtonGrid, "DiagnosticCriteriaPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).DiagnosticCriteriaPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).Lookups.DiagnosticCriteriaNotLinked)));
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.BindToFindBoxList = "Lookups+DiagnosticCriteriaNotLinked";
			zTextBoxColumnStyleInfo2.ColumnName = "IncidentDiagnosticCriteria+IMD_Description";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "IncidentDiagnosticCriteria+IMD_Type";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("c62668c3-9b4f-45ee-bd5c-e717f1edd154", "Ask Order");
			zTextBoxColumnStyleInfo4.ColumnName = "DIL_Order";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = ZClientEDI.Res.GetData("521f4e76-5f28-4e92-b78c-fea1ec81218d", "Confirm Option");
			zTextBoxColumnStyleInfo5.ColumnName = "ConfirmOption";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = ZClientEDI.Res.GetData("a74b482e-8a9d-4eb0-955a-d3d34606657c", "Negate Option");
			zTextBoxColumnStyleInfo6.ColumnName = "NegateOption";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.GridId = null;
			// 
			// 
			// 
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.InnerGrid.GridId = null;
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.InnerGrid.Name = "Grid";
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(9, 53, true);
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.Name = "investigationItemDiagnosticCriteriaModuleButtonGrid";
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.ReadOnly = false;
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.ShowNewButton = false;
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 179, true);
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.TabIndex = 0;
			// 
			// RelatedDiagnosticCriteriaGridLabel
			// 
			this.RelatedDiagnosticCriteriaGridLabel.CaptionResourceString = ZClientEDI.Res.GetData("893ee7e6-c147-474c-8a98-c1351cb4cc2d", "Related Diagnostic Criteria");
			this.RelatedDiagnosticCriteriaGridLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.RelatedDiagnosticCriteriaGridLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RelatedDiagnosticCriteriaGridLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedDiagnosticCriteriaGridLabel.Name = "RelatedDiagnosticCriteriaGridLabel";
			this.RelatedDiagnosticCriteriaGridLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 23, true);
			this.RelatedDiagnosticCriteriaGridLabel.TabIndex = 1;
			this.RelatedDiagnosticCriteriaGridLabel.UseMnemonic = false;
			// 
			// responseResultGrid
			// 
			this.responseResultGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.responseResultGrid, "DiagnosticCriteriaPivots.InvestigationResultPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.DiagnosticCriteriaInvestigationItemLink)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).DiagnosticCriteriaPivots)).SyncRoot)).InvestigationResultPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.IncidentManager.Business.DiagnosticCriteriaInvestigationResult)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.DiagnosticCriteriaInvestigationItemLink)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).DiagnosticCriteriaPivots)).SyncRoot)).InvestigationResultPivots)).SyncRoot)).INR_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.DiagnosticCriteriaInvestigationResult)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.DiagnosticCriteriaInvestigationItemLink)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).DiagnosticCriteriaPivots)).SyncRoot)).InvestigationResultPivots)).SyncRoot)).INR_ResponseOption)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.DiagnosticCriteriaInvestigationResult)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.DiagnosticCriteriaInvestigationItemLink)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).DiagnosticCriteriaPivots)).SyncRoot)).InvestigationResultPivots)).SyncRoot)).DCR_ResponseResultDescription)));
			this.responseResultGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("4d972a85-e3e3-4b7d-8fd7-bb53827af120", "#");
			zCalcEditColumnStyleInfo2.ColumnName = "INR_Sequence";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = ZClientEDI.Res.GetData("35946225-020c-4dc3-99b7-c4bdc999d22d", "Option Text");
			zTextBoxColumnStyleInfo7.ColumnName = "INR_ResponseOption";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("6a91c029-b452-45aa-8728-f94920cb7f6a", "Result");
			zDropEditColumnStyleInfo1.ColumnName = "DCR_ResponseResultDescription";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.responseResultGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.responseResultGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.responseResultGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.responseResultGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.responseResultGrid.GridId = "c19cbc93-a651-4393-8c87-8b2a1f29dd96";
			this.responseResultGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.responseResultGrid.LayoutKey = "responseResultGrid";
			this.responseResultGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.responseResultGrid.Name = "responseResultGrid";
			this.responseResultGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 154, true);
			this.responseResultGrid.TabIndex = 0;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.AskOrderTextBox);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 177, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 25, true);
			this.zPanel2.TabIndex = 3;
			// 
			// AskOrderTextBox
			// 
			this.BindingSource.SetBindingMember(this.AskOrderTextBox, "DiagnosticCriteriaPivots.DIL_Order");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Client.EDI.IncidentManager.Business.DiagnosticCriteriaInvestigationItemLink)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).DiagnosticCriteriaPivots)).SyncRoot)).DIL_Order)));
			this.AskOrderTextBox.CaptionResourceString = ZClientEDI.Res.GetData("96898bd0-d364-4bd8-9940-45978e89a94d", "Ask Order");
			this.AskOrderTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AskOrderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 4, true);
			this.AskOrderTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, 2, 2, 2, true);
			this.AskOrderTextBox.Name = "AskOrderTextBox";
			this.AskOrderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 17, true);
			this.AskOrderTextBox.TabIndex = 1;
			// 
			// ResponseOptionResultLabel
			// 
			this.ResponseOptionResultLabel.CaptionResourceString = ZClientEDI.Res.GetData("0dd8c11b-4a98-4958-91e6-85718d2faec9", "Set response option for selected diagnostic criteria");
			this.ResponseOptionResultLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ResponseOptionResultLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ResponseOptionResultLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResponseOptionResultLabel.Name = "ResponseOptionResultLabel";
			this.ResponseOptionResultLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 23, true);
			this.ResponseOptionResultLabel.TabIndex = 2;
			this.ResponseOptionResultLabel.UseMnemonic = false;
			// 
			// askClientCheckBox
			// 
			this.BindingSource.SetBindingMember(this.askClientCheckBox, "INV_AskClient");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).INV_AskClient)));
			this.askClientCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("8c1cdb50-237c-4a0a-a659-a87a712327cd", "Ask the Client");
			this.askClientCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 91, true);
			this.askClientCheckBox.Name = "askClientCheckBox";
			this.askClientCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 24, true);
			this.askClientCheckBox.TabIndex = 5;
			this.askClientCheckBox.UseVisualStyleBackColor = true;
			// 
			// activeCheckBox
			// 
			this.BindingSource.SetBindingMember(this.activeCheckBox, "INV_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).INV_IsActive)));
			this.activeCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("4cadf3e2-884c-4164-997f-4c67177daff6", "Is Active");
			this.activeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 57, true);
			this.activeCheckBox.Name = "activeCheckBox";
			this.activeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 24, true);
			this.activeCheckBox.TabIndex = 4;
			this.activeCheckBox.UseVisualStyleBackColor = true;
			// 
			// typeDropEdit
			// 
			this.typeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.typeDropEdit, "INV_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).INV_Type)));
			this.typeDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("48491635-1b22-4e30-ac17-ab877416e290", "Type");
			this.typeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 57, true);
			this.typeDropEdit.Name = "typeDropEdit";
			this.typeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 17, true);
			this.typeDropEdit.TabIndex = 2;
			// 
			// descriptionTextBox
			// 
			this.descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "INV_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem)(null)).INV_Description)));
			this.descriptionTextBox.CaptionResourceString = ZClientEDI.Res.GetData("2a23b894-c920-4de4-9a0f-844534c195a6", "Description");
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 21, true);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 17, true);
			this.descriptionTextBox.TabIndex = 1;
			// 
			// InvestigationItemForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 814, true);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem);
			this.DataSourceTypeName = "Enterprise.Client.EDI.IncidentManager.Business.InvestigationItem";
			this.Name = "InvestigationItemForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Investigation Item";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.kTableLayoutPanel1.ResumeLayout(false);
			this.kTableLayoutPanel1.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResponseOptionsGrid)).EndInit();
			this.ResponseOptionsGrid.ResumeLayout(false);
			this.ResponseOptionsGrid.PerformLayout();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.investigationItemDiagnosticCriteriaModuleButtonGrid.InnerGrid)).EndInit();
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.ResumeLayout(true);
			this.investigationItemDiagnosticCriteriaModuleButtonGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.responseResultGrid)).EndInit();
			this.responseResultGrid.ResumeLayout(false);
			this.responseResultGrid.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.typeDropEdit.ResumeLayout(true);
			this.typeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZPanel zPanel1;
		private ZTextBox itemTextTextBox;
		private ZCheckBox askClientCheckBox;
		private ZCheckBox activeCheckBox;
		private ZDropEdit typeDropEdit;
		private ZTextBox descriptionTextBox;
		private KTableLayoutPanel kTableLayoutPanel1;
		private ZGroupBox zGroupBox1;
		private ZGroupBox zGroupBox2;
		private ZGrid ResponseOptionsGrid;
		private KSplitContainer kSplitContainer1;
		private InvestigationItemDiagnosticCriteriaModuleButtonGrid investigationItemDiagnosticCriteriaModuleButtonGrid;
		private ZGrid responseResultGrid;
		private ZTextBox AskOrderTextBox;
		private ZLabel RelatedDiagnosticCriteriaGridLabel;
		private ZLabel ResponseOptionResultLabel;
		private ZPanel zPanel2;
	}
}
