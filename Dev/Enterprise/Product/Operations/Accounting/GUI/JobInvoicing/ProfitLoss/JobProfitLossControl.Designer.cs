using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobProfitLossControl
	{


		#region Component Designer generated code

		protected ZPanel SummaryTabPanel;
		protected ZPanel DetailsTabPanel;
		protected ZCodeFindBox SummaryChargeCodeFindBox;
		protected ZGuidFindBox SummaryDepartmentFindBox;
		protected ZGuidFindBox SummaryBranchFindBox;
		protected ZTemplateTabControl TabControl;
		protected ZButton ClearButton;
		protected ZCheckBox ShowReversedTransCheckBox;
		protected ZGuidFindBox DepartmentFindBox;
		protected ZGuidFindBox BranchFindBox;
		protected ZCodeFindBox ChargeCodeFindBox;
		protected ZButton FindButton;
		protected ZTabPage DetailsTabPage;
		protected ZTabPage SummaryTabPage;
		protected ZButton SummaryFindButton;
		protected ZButton SummaryClearButton;
		protected ZTabPage GlobalJobCostingTabPage;
		private ZPanel GlobalJobCostingTabPanel;
		private ZButton zButton5;
		private ZButton zButton6;
		private ZCheckBox GJCShowReversedTransCheckBox;
		private ZCodeFindBox GJCChargeCodeFindBox;
		private ZGuidFindBox GJCBranchFindBox;
		private ZGuidFindBox GJCDepartmentFindBox;
		private ZGuidFindBox GJCCompanyFindBox;
		private ZLabel GlobalJobCostingSecurityLabel;
		private System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo7 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo8 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo9 = new ZGuidFindBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo10 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo11 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo12 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo13 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new ZDateEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo14 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo15 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new ZDateEditColumnStyleInfo();
			this.TabControl = new ZTemplateTabControl();
			this.SummaryTabPage = new ZTabPage();
			this.SummaryTabPanel = new ZPanel();
			this.SummaryChargeHidingMessageLabel = new ZLabel();
			this.SummaryJobProfitLossTotalsControl = new JobProfitLossTotalsControl();
			this.JobProfitReportButton = new ZButton();
			this.SummaryRecognizedChargesDropEdit = new ZDropEdit();
			this.SummaryClearButton = new ZButton();
			this.SummaryChargeCodeFindBox = new ZCodeFindBox();
			this.SummaryFindButton = new ZButton();
			this.ProfitLossSummaryGrid = new ZGrid();
			this.SummaryBranchFindBox = new ZGuidFindBox();
			this.SummaryDepartmentFindBox = new ZGuidFindBox();
			this.DetailsTabPage = new ZTabPage();
			this.DetailsTabPanel = new ZPanel();
			this.ChargeHidingMessageLabel = new ZLabel();
			this.DetailsRecognizedChargesDropEdit = new ZDropEdit();
			this.DetailsJobProfitLossTotalsControl = new JobProfitLossTotalsControl();
			this.DetailsJobProfitReportButton = new ZButton();
			this.ClearButton = new ZButton();
			this.FindButton = new ZButton();
			this.ProfitLossGrid = new ZGrid();
			this.ShowReversedTransCheckBox = new ZCheckBox();
			this.ChargeCodeFindBox = new ZCodeFindBox();
			this.BranchFindBox = new ZGuidFindBox();
			this.DepartmentFindBox = new ZGuidFindBox();
			this.GlobalJobCostingTabPage = new ZTabPage();
			this.GlobalJobCostingTabPanel = new ZPanel();
			this.GlobalJobProfitLossTotalsControl = new JobProfitLossTotalsControl();
			this.GJCCompanyFindBox = new ZGuidFindBox();
			this.zButton5 = new ZButton();
			this.zButton6 = new ZButton();
			this.GJCProfitLossGrid = new ZGrid();
			this.GJCShowReversedTransCheckBox = new ZCheckBox();
			this.GJCChargeCodeFindBox = new ZCodeFindBox();
			this.GJCBranchFindBox = new ZGuidFindBox();
			this.GJCDepartmentFindBox = new ZGuidFindBox();
			this.GlobalJobCostingSecurityLabel = new ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.SummaryTabPage.SuspendLayout();
			this.SummaryTabPanel.SuspendLayout();
			this.SummaryJobProfitLossTotalsControl.SuspendLayout();
			this.SummaryRecognizedChargesDropEdit.SuspendLayout();
			this.SummaryChargeCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProfitLossSummaryGrid)).BeginInit();
			this.ProfitLossSummaryGrid.SuspendLayout();
			this.SummaryBranchFindBox.SuspendLayout();
			this.SummaryDepartmentFindBox.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.DetailsTabPanel.SuspendLayout();
			this.DetailsRecognizedChargesDropEdit.SuspendLayout();
			this.DetailsJobProfitLossTotalsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProfitLossGrid)).BeginInit();
			this.ProfitLossGrid.SuspendLayout();
			this.ChargeCodeFindBox.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.DepartmentFindBox.SuspendLayout();
			this.GlobalJobCostingTabPage.SuspendLayout();
			this.GlobalJobCostingTabPanel.SuspendLayout();
			this.GlobalJobProfitLossTotalsControl.SuspendLayout();
			this.GJCCompanyFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GJCProfitLossGrid)).BeginInit();
			this.GJCProfitLossGrid.SuspendLayout();
			this.GJCChargeCodeFindBox.SuspendLayout();
			this.GJCBranchFindBox.SuspendLayout();
			this.GJCDepartmentFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(IJobProfitLoss);
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.SummaryTabPage);
			this.TabControl.Controls.Add(this.DetailsTabPage);
			this.TabControl.Controls.Add(this.GlobalJobCostingTabPage);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 488, true);
			this.TabControl.TabIndex = 0;
			this.TabControl.SelectedIndexChanged += new EventHandler(TabControl_SelectedIndexChanged);
			this.TabControl.SelectedIndexChanging += new EventHandler(TabControl_SelectedIndexChanging);
			// 
			// SummaryTabPage
			// 
			this.SummaryTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|86812bb4-c862-4fb2-8980-59020bec94df", "Summary");
			this.SummaryTabPage.Controls.Add(this.SummaryTabPanel);
			this.SummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SummaryTabPage.Name = "SummaryTabPage";
			this.SummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 461, true);
			this.SummaryTabPage.TabIndex = 1;
			// 
			// SummaryTabPanel
			// 
			this.SummaryTabPanel.Controls.Add(this.SummaryChargeHidingMessageLabel);
			this.SummaryTabPanel.Controls.Add(this.SummaryJobProfitLossTotalsControl);
			this.SummaryTabPanel.Controls.Add(this.JobProfitReportButton);
			this.SummaryTabPanel.Controls.Add(this.SummaryRecognizedChargesDropEdit);
			this.SummaryTabPanel.Controls.Add(this.SummaryClearButton);
			this.SummaryTabPanel.Controls.Add(this.SummaryChargeCodeFindBox);
			this.SummaryTabPanel.Controls.Add(this.SummaryFindButton);
			this.SummaryTabPanel.Controls.Add(this.ProfitLossSummaryGrid);
			this.SummaryTabPanel.Controls.Add(this.SummaryBranchFindBox);
			this.SummaryTabPanel.Controls.Add(this.SummaryDepartmentFindBox);
			this.SummaryTabPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SummaryTabPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SummaryTabPanel.Name = "SummaryTabPanel";
			this.SummaryTabPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 461, true);
			this.SummaryTabPanel.TabIndex = 0;
			// 
			// SummaryChargeHidingMessageLabel
			// 
			this.SummaryChargeHidingMessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("22c21bba-6a03-452b-a7b1-43774e0f44e7", "Charges entered / posted to branch / dept outside your login permission are not listed.");
			this.SummaryChargeHidingMessageLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SummaryChargeHidingMessageLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SummaryChargeHidingMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SummaryChargeHidingMessageLabel.Name = "SummaryChargeHidingMessageLabel";
			this.SummaryChargeHidingMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 15, true);
			this.SummaryChargeHidingMessageLabel.TabIndex = 0;
			this.SummaryChargeHidingMessageLabel.Visible = false;
			// 
			// SummaryJobProfitLossTotalsControl
			// 
			this.SummaryJobProfitLossTotalsControl.AllowDrop = true;
			this.SummaryJobProfitLossTotalsControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.SummaryJobProfitLossTotalsControl, ".");
			this.SummaryJobProfitLossTotalsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 320, true);
			this.SummaryJobProfitLossTotalsControl.Name = "SummaryJobProfitLossTotalsControl";
			this.SummaryJobProfitLossTotalsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 137, true);
			this.SummaryJobProfitLossTotalsControl.TabIndex = 7;
			// 
			// JobProfitReportButton
			// 
			this.JobProfitReportButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.JobProfitReportButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|00b129b8-2228-4cda-bc41-5940a61eb4d6", "Print Job Profit");
			this.JobProfitReportButton.EditableInViewMode = true;
			this.JobProfitReportButton.IsCaptionOverridden = false;
			this.JobProfitReportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(871, 329, true);
			this.JobProfitReportButton.Name = "JobProfitReportButton";
			this.JobProfitReportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.JobProfitReportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 23, true);
			this.JobProfitReportButton.TabIndex = 8;
			this.JobProfitReportButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.JobProfitReportButton.ToolTipCaption = null;
			this.JobProfitReportButton.Click += new EventHandler(this.JobProfitReportButton_Click);
			// 
			// SummaryRecognizedChargesDropEdit
			// 
			this.SummaryRecognizedChargesDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SummaryRecognizedChargesDropEdit, "Filter+RecognizedChargesFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((IJobProfitLoss)(null)).Filter.RecognizedChargesFilter)));
			this.SummaryRecognizedChargesDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|d3544bf3-28ca-4261-af73-e12c86c7e6dd", "Recognized Charges");
			this.SummaryRecognizedChargesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 44, true);
			this.SummaryRecognizedChargesDropEdit.Name = "SummaryRecognizedChargesDropEdit";
			this.SummaryRecognizedChargesDropEdit.ShouldResizeByMaxLength = true;
			this.SummaryRecognizedChargesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.SummaryRecognizedChargesDropEdit.TabIndex = 3;
			// 
			// SummaryClearButton
			// 
			this.SummaryClearButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SummaryClearButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|ff9d5680-040b-46a7-a363-9f70c8023537", "Clear Filter");
			this.SummaryClearButton.EditableInViewMode = true;
			this.SummaryClearButton.IsCaptionOverridden = false;
			this.SummaryClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 39, true);
			this.SummaryClearButton.Name = "SummaryClearButton";
			this.SummaryClearButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SummaryClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 23, true);
			this.SummaryClearButton.TabIndex = 5;
			this.SummaryClearButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SummaryClearButton.ToolTipCaption = null;
			this.SummaryClearButton.Click += new EventHandler(this.ClearButton_Click);
			// 
			// SummaryChargeCodeFindBox
			// 
			this.SummaryChargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SummaryChargeCodeFindBox, "Filter+ChargeCodeFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IJobProfitLoss)(null)).Filter.ChargeCodeFilter)));
			this.SummaryChargeCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|60370ac0-e3b5-4eb4-b68b-bf9295e35322", "Charge Code");
			this.SummaryChargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 20, true);
			this.SummaryChargeCodeFindBox.Name = "SummaryChargeCodeFindBox";
			this.SummaryChargeCodeFindBox.PopupCaption = null;
			this.SummaryChargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.SummaryChargeCodeFindBox.TabIndex = 0;
			// 
			// SummaryFindButton
			// 
			this.SummaryFindButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SummaryFindButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|0348ce5f-adb6-4c87-9bfd-b6ac5a117f94", "Refresh");
			this.SummaryFindButton.EditableInViewMode = true;
			this.SummaryFindButton.IsCaptionOverridden = false;
			this.SummaryFindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 15, true);
			this.SummaryFindButton.Name = "SummaryFindButton";
			this.SummaryFindButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SummaryFindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 23, true);
			this.SummaryFindButton.TabIndex = 4;
			this.SummaryFindButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SummaryFindButton.ToolTipCaption = null;
			this.SummaryFindButton.Click += new EventHandler(this.FindButton_Click);
			// 
			// ProfitLossSummaryGrid
			// 
			this.ProfitLossSummaryGrid.AllowNavigation = false;
			this.ProfitLossSummaryGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProfitLossSummaryGrid, "ProfitLossSummaryFilteredDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_JH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_ChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_JobLocalReferenceNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_Revenue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_WIP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_Cost)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_Accrual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_LineAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossSummaryDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossSummaryFilteredDetails)).SyncRoot)).ZZ_Calc_MarginPercentage)));
			this.ProfitLossSummaryGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|a45736c6-261b-4df8-8e23-28f779c5886f", "Charge Code");
			zGuidFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ZZ_Calc_AC";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|2198c1d9-4766-43eb-a979-8241eb88d270", "Job Number");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "ZZ_Calc_JH";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|bd45a532-3395-4db1-a149-3fb40fc8de9d", "Branch");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "ZZ_Calc_GB";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|f382d3b7-646f-482c-9da0-a5883914762b", "Dept");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "ZZ_Calc_GE";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|9131ca20-0dd6-4a28-879b-e1037067f077", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ZZ_Calc_ChargeCodeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|0d78bfa1-6ab0-4b8f-ae03-09b7ceb00205", "Job Local Reference");
			zTextBoxColumnStyleInfo2.ColumnName = "ZZ_Calc_JobLocalReferenceNum";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "Decimals";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|1f34ea5f-d9f7-4f64-a6c9-81aa7b28a186", "Revenue");
			zCalcEditColumnStyleInfo1.ColumnName = "ZZ_Calc_Revenue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "Decimals";
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|997ebb9e-0bf8-423e-9a51-1de7c4ea6d7f", "WIP");
			zCalcEditColumnStyleInfo2.ColumnName = "ZZ_Calc_WIP";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = "Decimals";
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|fe240fc7-151b-4dee-bf79-b28fe4da31b7", "Cost");
			zCalcEditColumnStyleInfo3.ColumnName = "ZZ_Calc_Cost";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = "Decimals";
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|6b41eaea-281a-4f28-ad36-29d77a30c981", "Accrual");
			zCalcEditColumnStyleInfo4.ColumnName = "ZZ_Calc_Accrual";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = "Decimals";
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|45a8607f-d556-4352-81ab-a9246fdef5ea", "Profit/Loss");
			zCalcEditColumnStyleInfo5.ColumnName = "ZZ_Calc_LineAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|1d213391-b35e-41ac-9d84-5a60091cb633", "Margin");
			zCalcEditColumnStyleInfo6.ColumnName = "ZZ_Calc_MarginPercentage";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ProfitLossSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ProfitLossSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ProfitLossSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.ProfitLossSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.ProfitLossSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProfitLossSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ProfitLossSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ProfitLossSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ProfitLossSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ProfitLossSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ProfitLossSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ProfitLossSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.ProfitLossSummaryGrid.GridId = "6018b90a-88b3-401e-bf0f-dd2de0f2c345";
			this.ProfitLossSummaryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProfitLossSummaryGrid.IsWholeRowSelectedOnClick = true;
			this.ProfitLossSummaryGrid.LayoutKey = "ProfitLossGrid";
			this.ProfitLossSummaryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 84, true);
			this.ProfitLossSummaryGrid.Name = "ProfitLossSummaryGrid";
			this.ProfitLossSummaryGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ProfitLossSummaryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 230, true);
			this.ProfitLossSummaryGrid.TabIndex = 6;
			// 
			// SummaryBranchFindBox
			// 
			this.SummaryBranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SummaryBranchFindBox, "Filter+BranchFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IJobProfitLoss)(null)).Filter.BranchFilter)));
			this.SummaryBranchFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|6d6553eb-7c02-407f-8f42-b4723009b802", "Branch");
			this.SummaryBranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 44, true);
			this.SummaryBranchFindBox.Name = "SummaryBranchFindBox";
			this.SummaryBranchFindBox.PopupCaption = null;
			this.SummaryBranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.SummaryBranchFindBox.TabIndex = 2;
			// 
			// SummaryDepartmentFindBox
			// 
			this.SummaryDepartmentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SummaryDepartmentFindBox, "Filter+DepartmentFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IJobProfitLoss)(null)).Filter.DepartmentFilter)));
			this.SummaryDepartmentFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|bd177ddb-83f4-4bce-82c2-4a76f6037777", "Department");
			this.SummaryDepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 20, true);
			this.SummaryDepartmentFindBox.Name = "SummaryDepartmentFindBox";
			this.SummaryDepartmentFindBox.PopupCaption = null;
			this.SummaryDepartmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.SummaryDepartmentFindBox.TabIndex = 1;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|e7124222-c617-44e0-aa38-5e947f72c421", "Details");
			this.DetailsTabPage.Controls.Add(this.DetailsTabPanel);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 461, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// DetailsTabPanel
			// 
			this.DetailsTabPanel.Controls.Add(this.ChargeHidingMessageLabel);
			this.DetailsTabPanel.Controls.Add(this.DetailsRecognizedChargesDropEdit);
			this.DetailsTabPanel.Controls.Add(this.DetailsJobProfitLossTotalsControl);
			this.DetailsTabPanel.Controls.Add(this.DetailsJobProfitReportButton);
			this.DetailsTabPanel.Controls.Add(this.ClearButton);
			this.DetailsTabPanel.Controls.Add(this.FindButton);
			this.DetailsTabPanel.Controls.Add(this.ProfitLossGrid);
			this.DetailsTabPanel.Controls.Add(this.ShowReversedTransCheckBox);
			this.DetailsTabPanel.Controls.Add(this.ChargeCodeFindBox);
			this.DetailsTabPanel.Controls.Add(this.BranchFindBox);
			this.DetailsTabPanel.Controls.Add(this.DepartmentFindBox);
			this.DetailsTabPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsTabPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsTabPanel.Name = "DetailsTabPanel";
			this.DetailsTabPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 461, true);
			this.DetailsTabPanel.TabIndex = 0;
			// 
			// ChargeHidingMessageLabel
			// 
			this.ChargeHidingMessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("472c3c66-01e5-420e-bcf0-6224f2552002", "Charges entered / posted to branch / dept outside your login permission are not listed.");
			this.ChargeHidingMessageLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ChargeHidingMessageLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ChargeHidingMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargeHidingMessageLabel.Name = "ChargeHidingMessageLabel";
			this.ChargeHidingMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 15, true);
			this.ChargeHidingMessageLabel.TabIndex = 0;
			this.ChargeHidingMessageLabel.Visible = false;
			// 
			// DetailsRecognizedChargesDropEdit
			// 
			this.DetailsRecognizedChargesDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DetailsRecognizedChargesDropEdit, "Filter+RecognizedChargesFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((IJobProfitLoss)(null)).Filter.RecognizedChargesFilter)));
			this.DetailsRecognizedChargesDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|870fc2db-2f3f-4162-967f-c0c14b22459a", "Recognized Charges");
			this.DetailsRecognizedChargesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 44, true);
			this.DetailsRecognizedChargesDropEdit.Name = "DetailsRecognizedChargesDropEdit";
			this.DetailsRecognizedChargesDropEdit.ShouldResizeByMaxLength = true;
			this.DetailsRecognizedChargesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.DetailsRecognizedChargesDropEdit.TabIndex = 4;
			// 
			// DetailsJobProfitLossTotalsControl
			// 
			this.DetailsJobProfitLossTotalsControl.AllowDrop = true;
			this.DetailsJobProfitLossTotalsControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DetailsJobProfitLossTotalsControl, ".");
			this.DetailsJobProfitLossTotalsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 319, true);
			this.DetailsJobProfitLossTotalsControl.Name = "DetailsJobProfitLossTotalsControl";
			this.DetailsJobProfitLossTotalsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 139, true);
			this.DetailsJobProfitLossTotalsControl.TabIndex = 8;
			// 
			// DetailsJobProfitReportButton
			// 
			this.DetailsJobProfitReportButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailsJobProfitReportButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|960c688f-2ae9-4894-9a80-b059f2c4f18d", "Print Job Profit");
			this.DetailsJobProfitReportButton.IsCaptionOverridden = false;
			this.DetailsJobProfitReportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(870, 327, true);
			this.DetailsJobProfitReportButton.Name = "DetailsJobProfitReportButton";
			this.DetailsJobProfitReportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DetailsJobProfitReportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 23, true);
			this.DetailsJobProfitReportButton.TabIndex = 9;
			this.DetailsJobProfitReportButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.DetailsJobProfitReportButton.ToolTipCaption = null;
			this.DetailsJobProfitReportButton.Click += new EventHandler(this.JobProfitReportButton_Click);
			// 
			// ClearButton
			// 
			this.ClearButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|3c3b5c04-a236-4d61-839a-a4b6f37767fa", "Clear Filter");
			this.ClearButton.IsCaptionOverridden = false;
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 39, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 23, true);
			this.ClearButton.TabIndex = 6;
			this.ClearButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ClearButton.ToolTipCaption = null;
			this.ClearButton.Click += new EventHandler(this.ClearButton_Click);
			// 
			// FindButton
			// 
			this.FindButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.FindButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|a3ff7d0e-7321-4eca-b1d2-fa5847b06851", "Refresh");
			this.FindButton.IsCaptionOverridden = false;
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 15, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 23, true);
			this.FindButton.TabIndex = 5;
			this.FindButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.FindButton.ToolTipCaption = null;
			this.FindButton.Click += new EventHandler(this.FindButton_Click);
			// 
			// ProfitLossGrid
			// 
			this.ProfitLossGrid.AllowNavigation = false;
			this.ProfitLossGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProfitLossGrid, "ProfitLossFilteredDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_JH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_ConsolNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_ChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_LineAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_LineType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_FullyPaidDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_JobLocalReferenceNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_RecognizedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_RecognitionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_ReversalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_SystemCreateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetailView)(((System.Collections.IList)(((IJobProfitLoss)(null)).ProfitLossFilteredDetails)).SyncRoot)).ZY_Calc_AuditedBy)));
			this.ProfitLossGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|bc475c3f-d9a4-47ca-bfc4-8094121548f2", "Charge Code");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "ZY_Calc_AC";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|6bc73a5b-3051-4454-87e5-1743f68c6688", "Job Number");
			zGuidFindBoxColumnStyleInfo6.ColumnName = "ZY_Calc_JH";
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|56a01769-ed2d-49e4-b615-776f3b12d078", "Consol Number");
			zTextBoxColumnStyleInfo3.ColumnName = "ZY_Calc_ConsolNum";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|d6b5bd06-3011-4272-9f7a-b04e3fbcac2c", "Branch");
			zGuidFindBoxColumnStyleInfo7.ColumnName = "ZY_Calc_GB";
			zGuidFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|f9789abd-955d-4eef-bfa0-462164cd0e0b", "Dept");
			zGuidFindBoxColumnStyleInfo8.ColumnName = "ZY_Calc_GE";
			zGuidFindBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|a7a7e55f-a942-4a60-a04f-629f0913c090", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "ZY_Calc_ChargeCodeDescription";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|1ff47238-46ec-4643-a73f-07adc38a7193", "Invoice Date");
			zDateEditColumnStyleInfo1.ColumnName = "ZY_Calc_InvoiceDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = "Decimals";
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|704a6bd4-ca93-47c4-b881-070ecd045e72", "Amount");
			zCalcEditColumnStyleInfo7.ColumnName = "ZY_Calc_LineAmount";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|1199e0a1-728e-4b4f-b299-a5dc70574c58", "Type");
			zTextBoxColumnStyleInfo5.ColumnName = "ZY_Calc_LineType";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|ac2063c8-c593-4855-b0d9-f5306cc46e2a", "Post Date");
			zDateEditColumnStyleInfo2.ColumnName = "ZY_Calc_PostDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|bc908fda-e23e-487c-8e1b-96d15ad033dd", "Fully Paid Date");
			zDateEditColumnStyleInfo3.ColumnName = "ZY_Calc_FullyPaidDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|c6300c2c-695a-46dd-9cf3-034604806045", "Transaction No");
			zTextBoxColumnStyleInfo6.ColumnName = "ZY_Calc_TransactionNum";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|99984b25-13a7-4fa7-ae8b-15845518c42b", "Ledger");
			zTextBoxColumnStyleInfo7.ColumnName = "ZY_Calc_Ledger";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|53387b58-c4b0-4b20-840f-0ead4c795c73", "Trans Type");
			zTextBoxColumnStyleInfo8.ColumnName = "ZY_Calc_TransactionType";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|0136b20a-40c5-4eeb-a386-8b0f34974359", "Job Local Reference");
			zTextBoxColumnStyleInfo9.ColumnName = "ZY_Calc_JobLocalReferenceNum";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|df903f9e-591d-4d71-8747-0a67cc37c8c6", "Org.", "Organization");
			zGuidFindBoxColumnStyleInfo9.ColumnName = "ZY_Calc_OH";
			zGuidFindBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|04e1f79a-8969-45d8-95f7-bd76a50fb661", "Recognized Date");
			zDateEditColumnStyleInfo4.ColumnName = "ZY_Calc_RecognizedDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|87c88935-5cbb-45c8-972e-0ff5357ee8c7", "Recognition Type");
			zTextBoxColumnStyleInfo10.ColumnName = "ZY_Calc_RecognitionType";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|c86f5d18-f01c-44ee-bcc0-3eb4a762c3ae", "Date Reversed");
			zDateEditColumnStyleInfo5.ColumnName = "ZY_Calc_ReversalDate";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|8ece6e76-3d1f-4658-abd1-4fff300066a0", "Audit Details");
			zDateEditColumnStyleInfo6.ColumnName = "ZY_Calc_SystemCreateTime";
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|F6D87826-F2F0-4F71-89D9-706A11496ACC", "Audited By");
			zTextBoxColumnStyleInfo11.ColumnName = "ZY_Calc_AuditedBy";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ProfitLossGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.ProfitLossGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.ProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ProfitLossGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo7);
			this.ProfitLossGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo8);
			this.ProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ProfitLossGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ProfitLossGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.ProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ProfitLossGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ProfitLossGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ProfitLossGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo9);
			this.ProfitLossGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ProfitLossGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.ProfitLossGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.ProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ProfitLossGrid.GridId = "f545d99c-48e8-4a92-96c1-56de700516cd";
			this.ProfitLossGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProfitLossGrid.IsWholeRowSelectedOnClick = true;
			this.ProfitLossGrid.LayoutKey = "ProfitLossGrid";
			this.ProfitLossGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 84, true);
			this.ProfitLossGrid.Name = "ProfitLossGrid";
			this.ProfitLossGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ProfitLossGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 229, true);
			this.ProfitLossGrid.TabIndex = 7;
			// 
			// ShowReversedTransCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ShowReversedTransCheckBox, "Filter+ShowReversedFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((IJobProfitLoss)(null)).Filter.ShowReversedFilter)));
			this.ShowReversedTransCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|9c0f209d-fa0a-4dbf-bd63-2acf49f64c5f", "Show Reversed WIP and Accrual Transactions");
			this.ShowReversedTransCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowReversedTransCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(726, 9, true);
			this.ShowReversedTransCheckBox.Name = "ShowReversedTransCheckBox";
			this.ShowReversedTransCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 34, true);
			this.ShowReversedTransCheckBox.TabIndex = 2;
			// 
			// ChargeCodeFindBox
			// 
			this.ChargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargeCodeFindBox, "Filter+ChargeCodeFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IJobProfitLoss)(null)).Filter.ChargeCodeFilter)));
			this.ChargeCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|65d9743f-61f9-47b0-9f3d-86471a4e6058", "Charge Code");
			this.ChargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 20, true);
			this.ChargeCodeFindBox.Name = "ChargeCodeFindBox";
			this.ChargeCodeFindBox.PopupCaption = null;
			this.ChargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ChargeCodeFindBox.TabIndex = 0;
			// 
			// BranchFindBox
			// 
			this.BranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchFindBox, "Filter+BranchFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IJobProfitLoss)(null)).Filter.BranchFilter)));
			this.BranchFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|d02303b7-6023-4a08-88f3-15c0fb9fd62a", "Branch");
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 44, true);
			this.BranchFindBox.Name = "BranchFindBox";
			this.BranchFindBox.PopupCaption = null;
			this.BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.BranchFindBox.TabIndex = 3;
			// 
			// DepartmentFindBox
			// 
			this.DepartmentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartmentFindBox, "Filter+DepartmentFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IJobProfitLoss)(null)).Filter.DepartmentFilter)));
			this.DepartmentFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|5d97f86c-5ea2-4ed3-afb6-4be9c5964f60", "Department");
			this.DepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 20, true);
			this.DepartmentFindBox.Name = "DepartmentFindBox";
			this.DepartmentFindBox.PopupCaption = null;
			this.DepartmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.DepartmentFindBox.TabIndex = 1;
			// 
			// GlobalJobCostingTabPage
			// 
			this.GlobalJobCostingTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.GlobalJobCostingTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|f8a2e1c3-4c97-4723-986c-406716a34bfe", "Global Job Costing");
			this.GlobalJobCostingTabPage.Controls.Add(this.GlobalJobCostingTabPanel);
			this.GlobalJobCostingTabPage.Controls.Add(this.GlobalJobCostingSecurityLabel);
			this.GlobalJobCostingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GlobalJobCostingTabPage.Name = "GlobalJobCostingTabPage";
			this.GlobalJobCostingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 461, true);
			this.GlobalJobCostingTabPage.TabIndex = 2;
			// 
			// GlobalJobCostingTabPanel
			// 
			this.GlobalJobCostingTabPanel.Controls.Add(this.GlobalJobProfitLossTotalsControl);
			this.GlobalJobCostingTabPanel.Controls.Add(this.GJCCompanyFindBox);
			this.GlobalJobCostingTabPanel.Controls.Add(this.zButton5);
			this.GlobalJobCostingTabPanel.Controls.Add(this.zButton6);
			this.GlobalJobCostingTabPanel.Controls.Add(this.GJCProfitLossGrid);
			this.GlobalJobCostingTabPanel.Controls.Add(this.GJCShowReversedTransCheckBox);
			this.GlobalJobCostingTabPanel.Controls.Add(this.GJCChargeCodeFindBox);
			this.GlobalJobCostingTabPanel.Controls.Add(this.GJCBranchFindBox);
			this.GlobalJobCostingTabPanel.Controls.Add(this.GJCDepartmentFindBox);
			this.GlobalJobCostingTabPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GlobalJobCostingTabPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GlobalJobCostingTabPanel.Name = "GlobalJobCostingTabPanel";
			this.GlobalJobCostingTabPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 461, true);
			this.GlobalJobCostingTabPanel.TabIndex = 0;
			// 
			// GlobalJobProfitLossTotalsControl
			// 
			this.GlobalJobProfitLossTotalsControl.AllowDrop = true;
			this.GlobalJobProfitLossTotalsControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.GlobalJobProfitLossTotalsControl, "GlobalJobCostingProfitLoss");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IJobProfitLoss)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)))));
			this.GlobalJobProfitLossTotalsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 321, true);
			this.GlobalJobProfitLossTotalsControl.Name = "GlobalJobProfitLossTotalsControl";
			this.GlobalJobProfitLossTotalsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 137, true);
			this.GlobalJobProfitLossTotalsControl.TabIndex = 8;
			// 
			// GJCCompanyFindBox
			// 
			this.GJCCompanyFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GJCCompanyFindBox, "Filter+CompanyFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IJobProfitLoss)(null)).Filter.CompanyFilter)));
			this.GJCCompanyFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|d03039ef-94dc-43b6-b6db-93ad7f0e27d6", "Company");
			this.GJCCompanyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 16, true);
			this.GJCCompanyFindBox.Name = "GJCCompanyFindBox";
			this.GJCCompanyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.GJCCompanyFindBox.TabIndex = 1;
			// 
			// zButton5
			// 
			this.zButton5.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zButton5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|e1854ac7-d905-4d84-b806-d2b29bb8ad05", "Clear Filter");
			this.zButton5.IsCaptionOverridden = false;
			this.zButton5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 39, true);
			this.zButton5.Name = "zButton5";
			this.zButton5.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButton5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 23, true);
			this.zButton5.TabIndex = 6;
			this.zButton5.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.zButton5.ToolTipCaption = null;
			this.zButton5.Click += new EventHandler(this.ClearButton_Click);
			// 
			// zButton6
			// 
			this.zButton6.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zButton6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|e33bb013-d505-4683-8614-a1eebb8d1139", "Refresh");
			this.zButton6.IsCaptionOverridden = false;
			this.zButton6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 15, true);
			this.zButton6.Name = "zButton6";
			this.zButton6.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButton6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 23, true);
			this.zButton6.TabIndex = 5;
			this.zButton6.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.zButton6.ToolTipCaption = null;
			this.zButton6.Click += new EventHandler(this.FindButton_Click);
			// 
			// GJCProfitLossGrid
			// 
			this.GJCProfitLossGrid.AllowNavigation = false;
			this.GJCProfitLossGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GJCProfitLossGrid, "GlobalJobCostingProfitLoss");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_JH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_ChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_LineAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_LineType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_JobLocalReferenceNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_GC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_LocalCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ProfitLossDetail)(((System.Collections.IList)(((IJobProfitLoss)(null)).GlobalJobCostingProfitLoss)).SyncRoot)).ZY_Calc_RecognizedDate)));
			this.GJCProfitLossGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|f00cda09-0824-4a8e-a588-10f1ffc56153", "Charge Code");
			zGuidFindBoxColumnStyleInfo10.ColumnName = "ZY_Calc_AC";
			zGuidFindBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|5c86adde-6032-413b-a0d4-e20b93336a66", "Job Number");
			zGuidFindBoxColumnStyleInfo11.ColumnName = "ZY_Calc_JH";
			zGuidFindBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|d1861f59-ddc9-4a1b-a2e9-b0bb6ec3dd59", "Branch");
			zGuidFindBoxColumnStyleInfo12.ColumnName = "ZY_Calc_GB";
			zGuidFindBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|e0c600cf-7dc5-4c35-81c7-f50bfef0450e", "Dept");
			zGuidFindBoxColumnStyleInfo13.ColumnName = "ZY_Calc_GE";
			zGuidFindBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|801b3635-e3cb-4aa8-9c76-f96fbbf87e74", "Description");
			zTextBoxColumnStyleInfo12.ColumnName = "ZY_Calc_ChargeCodeDescription";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|c75cf3e0-80bc-4cbc-bc41-6d4543191e21", "Invoice Date");
			zDateEditColumnStyleInfo7.ColumnName = "ZY_Calc_InvoiceDate";
			zDateEditColumnStyleInfo7.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = "Decimals";
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|f85653a6-df44-44f1-b054-e2202c0af89f", "Amount");
			zCalcEditColumnStyleInfo8.ColumnName = "ZY_Calc_LineAmount";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|d1e79c03-f6e9-4c04-9953-c2490bf1df25", "Type");
			zTextBoxColumnStyleInfo13.ColumnName = "ZY_Calc_LineType";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDateEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|17ce0753-9a2e-42c1-a1cc-390596e327d6", "Post Date");
			zDateEditColumnStyleInfo8.ColumnName = "ZY_Calc_PostDate";
			zDateEditColumnStyleInfo8.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|542393e8-06d7-4e8c-b019-5022d8f7033a", "Transaction No");
			zTextBoxColumnStyleInfo14.ColumnName = "ZY_Calc_TransactionNum";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|783cff66-4a7f-4f2a-a82b-8065653117fb", "Ledger");
			zTextBoxColumnStyleInfo15.ColumnName = "ZY_Calc_Ledger";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|52effbd8-1912-4d58-8bd6-6f357e87acff", "Trans Type");
			zTextBoxColumnStyleInfo16.ColumnName = "ZY_Calc_TransactionType";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|9091da22-70d5-4bc3-8bcb-a6d4d6d7764b", "Job Local Reference");
			zTextBoxColumnStyleInfo17.ColumnName = "ZY_Calc_JobLocalReferenceNum";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|44fa7860-f9d1-4123-857c-a4d48b2083cd", "Organization");
			zGuidFindBoxColumnStyleInfo14.ColumnName = "ZY_Calc_OH";
			zGuidFindBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|165694f6-8881-445e-8031-17d895228f24", "Company");
			zGuidFindBoxColumnStyleInfo15.ColumnName = "ZY_Calc_GC";
			zGuidFindBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|2b267638-6079-4b8e-beac-9dfb0936af2a", "Company Currency");
			zTextBoxColumnStyleInfo18.ColumnName = "ZY_Calc_LocalCurrency";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|d6297d72-5a8f-4690-8a4f-ff20528a139e", "Recognized Date");
			zDateEditColumnStyleInfo9.ColumnName = "ZY_Calc_RecognizedDate";
			zDateEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.GJCProfitLossGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo10);
			this.GJCProfitLossGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo11);
			this.GJCProfitLossGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo12);
			this.GJCProfitLossGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo13);
			this.GJCProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.GJCProfitLossGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.GJCProfitLossGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.GJCProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.GJCProfitLossGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.GJCProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.GJCProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.GJCProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.GJCProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.GJCProfitLossGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo14);
			this.GJCProfitLossGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo15);
			this.GJCProfitLossGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.GJCProfitLossGrid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.GJCProfitLossGrid.GridId = "ab402246-2b3b-41ff-9541-4f1c5b39414a";
			this.GJCProfitLossGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GJCProfitLossGrid.IsWholeRowSelectedOnClick = true;
			this.GJCProfitLossGrid.LayoutKey = "ProfitLossGrid";
			this.GJCProfitLossGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 80, true);
			this.GJCProfitLossGrid.Name = "GJCProfitLossGrid";
			this.GJCProfitLossGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.GJCProfitLossGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 235, true);
			this.GJCProfitLossGrid.TabIndex = 7;
			// 
			// GJCShowReversedTransCheckBox
			// 
			this.BindingSource.SetBindingMember(this.GJCShowReversedTransCheckBox, "Filter+ShowReversedFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((IJobProfitLoss)(null)).Filter.ShowReversedFilter)));
			this.GJCShowReversedTransCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|1bd8c295-ee88-46a9-9757-2327dd9e3de7", "Show Reversed Transactions");
			this.GJCShowReversedTransCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GJCShowReversedTransCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(726, 6, true);
			this.GJCShowReversedTransCheckBox.Name = "GJCShowReversedTransCheckBox";
			this.GJCShowReversedTransCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 40, true);
			this.GJCShowReversedTransCheckBox.TabIndex = 2;
			// 
			// GJCChargeCodeFindBox
			// 
			this.GJCChargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GJCChargeCodeFindBox, "Filter+ChargeCodeFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IJobProfitLoss)(null)).Filter.ChargeCodeFilter)));
			this.GJCChargeCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|5efc1f3c-2f3e-4c7e-8140-f9677d0d085b", "Charge Code");
			this.GJCChargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 16, true);
			this.GJCChargeCodeFindBox.Name = "GJCChargeCodeFindBox";
			this.GJCChargeCodeFindBox.PopupCaption = null;
			this.GJCChargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.GJCChargeCodeFindBox.TabIndex = 0;
			// 
			// GJCBranchFindBox
			// 
			this.GJCBranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GJCBranchFindBox, "Filter+BranchFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IJobProfitLoss)(null)).Filter.BranchFilter)));
			this.GJCBranchFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|74af61ab-b006-4859-b3fe-9ac72d74a6f1", "Branch");
			this.GJCBranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 40, true);
			this.GJCBranchFindBox.Name = "GJCBranchFindBox";
			this.GJCBranchFindBox.PopupCaption = null;
			this.GJCBranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.GJCBranchFindBox.TabIndex = 3;
			// 
			// GJCDepartmentFindBox
			// 
			this.GJCDepartmentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GJCDepartmentFindBox, "Filter+DepartmentFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IJobProfitLoss)(null)).Filter.DepartmentFilter)));
			this.GJCDepartmentFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobProfitLossControl|d60ba5c2-a245-4864-a0c5-e5633819660a", "Department");
			this.GJCDepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(466, 40, true);
			this.GJCDepartmentFindBox.Name = "GJCDepartmentFindBox";
			this.GJCDepartmentFindBox.PopupCaption = null;
			this.GJCDepartmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.GJCDepartmentFindBox.TabIndex = 4;
			// 
			// GlobalJobCostingSecurityLabel
			// 
			this.GlobalJobCostingSecurityLabel.AutoSize = true;
			this.GlobalJobCostingSecurityLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.GlobalJobCostingSecurityLabel.IsFontBold = true;
			this.GlobalJobCostingSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 200, true);
			this.GlobalJobCostingSecurityLabel.Name = "GlobalJobCostingSecurityLabel";
			this.GlobalJobCostingSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.GlobalJobCostingSecurityLabel.TabIndex = 1;
			this.GlobalJobCostingSecurityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// JobProfitLossControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TabControl);
			this.Name = "JobProfitLossControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 488, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.SummaryTabPage.ResumeLayout(false);
			this.SummaryTabPage.PerformLayout();
			this.SummaryTabPanel.ResumeLayout(false);
			this.SummaryTabPanel.PerformLayout();
			this.SummaryJobProfitLossTotalsControl.ResumeLayout(true);
			this.SummaryJobProfitLossTotalsControl.PerformLayout();
			this.SummaryRecognizedChargesDropEdit.ResumeLayout(true);
			this.SummaryRecognizedChargesDropEdit.PerformLayout();
			this.SummaryChargeCodeFindBox.ResumeLayout(true);
			this.SummaryChargeCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProfitLossSummaryGrid)).EndInit();
			this.ProfitLossSummaryGrid.ResumeLayout(false);
			this.ProfitLossSummaryGrid.PerformLayout();
			this.SummaryBranchFindBox.ResumeLayout(true);
			this.SummaryBranchFindBox.PerformLayout();
			this.SummaryDepartmentFindBox.ResumeLayout(true);
			this.SummaryDepartmentFindBox.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.DetailsTabPanel.ResumeLayout(false);
			this.DetailsTabPanel.PerformLayout();
			this.DetailsRecognizedChargesDropEdit.ResumeLayout(true);
			this.DetailsRecognizedChargesDropEdit.PerformLayout();
			this.DetailsJobProfitLossTotalsControl.ResumeLayout(true);
			this.DetailsJobProfitLossTotalsControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProfitLossGrid)).EndInit();
			this.ProfitLossGrid.ResumeLayout(false);
			this.ProfitLossGrid.PerformLayout();
			this.ChargeCodeFindBox.ResumeLayout(true);
			this.ChargeCodeFindBox.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.DepartmentFindBox.ResumeLayout(true);
			this.DepartmentFindBox.PerformLayout();
			this.GlobalJobCostingTabPage.ResumeLayout(false);
			this.GlobalJobCostingTabPage.PerformLayout();
			this.GlobalJobCostingTabPanel.ResumeLayout(false);
			this.GlobalJobCostingTabPanel.PerformLayout();
			this.GlobalJobProfitLossTotalsControl.ResumeLayout(true);
			this.GlobalJobProfitLossTotalsControl.PerformLayout();
			this.GJCCompanyFindBox.ResumeLayout(true);
			this.GJCCompanyFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GJCProfitLossGrid)).EndInit();
			this.GJCProfitLossGrid.ResumeLayout(false);
			this.GJCProfitLossGrid.PerformLayout();
			this.GJCChargeCodeFindBox.ResumeLayout(true);
			this.GJCChargeCodeFindBox.PerformLayout();
			this.GJCBranchFindBox.ResumeLayout(true);
			this.GJCBranchFindBox.PerformLayout();
			this.GJCDepartmentFindBox.ResumeLayout(true);
			this.GJCDepartmentFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}