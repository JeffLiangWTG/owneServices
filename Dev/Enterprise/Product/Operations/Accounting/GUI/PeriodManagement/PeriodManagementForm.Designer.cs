using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.GUI.Aggregator;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class PeriodManagementForm
	{
private void InitializeComponent()
		{
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.FinancialYearCalcEdit = new ZArchitecture.ZCalcEdit();
			this.DescriptionGroupBox = new ZGroupBox();
			this.CloseGLAdjustmentsButton = new ZButton();
			this.ExpectedDebtorCreditorAggRes = new ZButton();
			this.ExpectedWipAcrAggRes = new ZButton();
			this.ReaggregateThisCompanyButton = new ZButton();
			this.ReSetPeriodsButton = new ZButton();
			this.EditPeriodEndDateButton = new ZButton();
			this.ReverseToPeriodEdit = new ZArchitecture.ZCalcEdit();
			this.zLabel2 = new ZArchitecture.ZLabel();
			this.PeriodExcludeCalcEdit = new ZArchitecture.ZCalcEdit();
			this.PurgeGLDButton = new ZButton();
			this.ReportOnlyButton = new ZButton();
			this.SimulateButton = new ZButton();
			this.ReAggregateSinglePeriodSingleCompany = new ZButton();
			this.ReaggregateAllCompaniesButton = new ZButton();
			this.SetUpNextYearButton = new ZButton();
			this.CloseGLButton = new ZButton();
			this.CloseSubledgerButton = new ZButton();
			this.PeriodsGrid = new ZArchitecture.ZGrid();
			this.PeriodInfoLabel = new ZArchitecture.ZLabel();
			this.RecoverPartOfGldPeriodsButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DescriptionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PeriodsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(PeriodManager);
			// 
			// FinancialYearCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FinancialYearCalcEdit, "FinancialYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PeriodManager)(null)).FinancialYear)));
			this.FinancialYearCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|0d865a39-def8-4d72-aa2f-7c6b72e124c6", "Financial year");
			this.FinancialYearCalcEdit.DecimalPlaces = 0;
			this.FinancialYearCalcEdit.Decimals = 0;
			this.FinancialYearCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.FinancialYearCalcEdit.Name = "FinancialYearCalcEdit";
			this.FinancialYearCalcEdit.ShowGroupSeparators = false;
			this.FinancialYearCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.FinancialYearCalcEdit.TabIndex = 1;
			this.FinancialYearCalcEdit.Text = "0";
			this.FinancialYearCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DescriptionGroupBox
			// 
			this.DescriptionGroupBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DescriptionGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|2156c630-ed0e-41dc-8421-d7b988a97069", "Periods");
			this.DescriptionGroupBox.Controls.Add(this.CloseGLAdjustmentsButton);
			this.DescriptionGroupBox.Controls.Add(this.ExpectedDebtorCreditorAggRes);
			this.DescriptionGroupBox.Controls.Add(this.ExpectedWipAcrAggRes);
			this.DescriptionGroupBox.Controls.Add(this.ReaggregateThisCompanyButton);
			this.DescriptionGroupBox.Controls.Add(this.ReSetPeriodsButton);
			this.DescriptionGroupBox.Controls.Add(this.EditPeriodEndDateButton);
			this.DescriptionGroupBox.Controls.Add(this.ReverseToPeriodEdit);
			this.DescriptionGroupBox.Controls.Add(this.zLabel2);
			this.DescriptionGroupBox.Controls.Add(this.PeriodExcludeCalcEdit);
			this.DescriptionGroupBox.Controls.Add(this.RecoverPartOfGldPeriodsButton);
			this.DescriptionGroupBox.Controls.Add(this.PurgeGLDButton);
			this.DescriptionGroupBox.Controls.Add(this.ReportOnlyButton);
			this.DescriptionGroupBox.Controls.Add(this.SimulateButton);
			this.DescriptionGroupBox.Controls.Add(this.ReAggregateSinglePeriodSingleCompany);
			this.DescriptionGroupBox.Controls.Add(this.ReaggregateAllCompaniesButton);
			this.DescriptionGroupBox.Controls.Add(this.SetUpNextYearButton);
			this.DescriptionGroupBox.Controls.Add(this.CloseGLButton);
			this.DescriptionGroupBox.Controls.Add(this.CloseSubledgerButton);
			this.DescriptionGroupBox.Controls.Add(this.PeriodsGrid);
			this.DescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.DescriptionGroupBox.Name = "DescriptionGroupBox";
			this.DescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(646, 635, true);
			this.DescriptionGroupBox.TabIndex = 3;
			this.DescriptionGroupBox.TabStop = false;
			// 
			// CloseGLAdjustmentsButton
			// 
			this.CloseGLAdjustmentsButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseGLAdjustmentsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|145c1873-21ef-4c48-96b9-515fb0c6f5f0", "Close GL Period for Adjustments");
			this.CloseGLAdjustmentsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 82, true);
			this.CloseGLAdjustmentsButton.Name = "CloseGLAdjustmentsButton";
			this.CloseGLAdjustmentsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 34, true);
			this.CloseGLAdjustmentsButton.TabIndex = 18;
			this.CloseGLAdjustmentsButton.Click += new EventHandler(this.CloseGLAdjustmentsButton_Click);
			// 
			// ExpectedDebtorCreditorAggRes
			// 
			this.ExpectedDebtorCreditorAggRes.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ExpectedDebtorCreditorAggRes.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|191391e7-2b59-44e0-b0ad-611285d4de0e", "Expected Debtor\\Creditor aggregation results");
			this.ExpectedDebtorCreditorAggRes.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 551, true);
			this.ExpectedDebtorCreditorAggRes.Name = "ExpectedDebtorCreditorAggRes";
			this.ExpectedDebtorCreditorAggRes.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 34, true);
			this.ExpectedDebtorCreditorAggRes.TabIndex = 17;
			this.ExpectedDebtorCreditorAggRes.Visible = false;
			this.ExpectedDebtorCreditorAggRes.Click += new EventHandler(this.ExpectedDebtorCreditorAggRes_Click);
			// 
			// ExpectedWipAcrAggRes
			// 
			this.ExpectedWipAcrAggRes.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ExpectedWipAcrAggRes.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|354143ce-b730-4c83-9c82-a6b5acfda219", "Expected WIP\\ACR aggregation results");
			this.ExpectedWipAcrAggRes.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 511, true);
			this.ExpectedWipAcrAggRes.Name = "ExpectedWipAcrAggRes";
			this.ExpectedWipAcrAggRes.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 34, true);
			this.ExpectedWipAcrAggRes.TabIndex = 16;
			this.ExpectedWipAcrAggRes.Visible = false;
			this.ExpectedWipAcrAggRes.Click += new EventHandler(this.ExpectedWipAcrAggRes_Click);
			// 
			// ReaggregateThisCompanyButton
			// 
			this.ReaggregateThisCompanyButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ReaggregateThisCompanyButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|510a42a4-aabb-4f1d-bf49-fad48fbb4ad5", "Re-Aggregate(Only This Company)");
			this.ReaggregateThisCompanyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 317, true);
			this.ReaggregateThisCompanyButton.Name = "ReaggregateThisCompanyButton";
			this.ReaggregateThisCompanyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 43, true);
			this.ReaggregateThisCompanyButton.TabIndex = 6;
			this.ReaggregateThisCompanyButton.Visible = false;
			this.ReaggregateThisCompanyButton.Click += new EventHandler(this.ReaggregateThisCompanyButton_Click);
			// 
			// ReSetPeriodsButton
			// 
			this.ReSetPeriodsButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ReSetPeriodsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|c46c35a6-4328-476a-abb0-0a36ecce05ca", "Re-Set Periods");
			this.ReSetPeriodsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 181, true);
			this.ReSetPeriodsButton.Name = "ReSetPeriodsButton";
			this.ReSetPeriodsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.ReSetPeriodsButton.TabIndex = 9;
			this.ReSetPeriodsButton.Visible = false;
			this.ReSetPeriodsButton.Click += new EventHandler(this.ReSetPeriodsButton_Click);
			// 
			// EditPeriodEndDateButton
			// 
			this.EditPeriodEndDateButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.EditPeriodEndDateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|881d39b0-8d7a-4af2-a2e7-35c447bb145d", "Edit Period End Date");
			this.EditPeriodEndDateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 151, true);
			this.EditPeriodEndDateButton.Name = "EditPeriodEndDateButton";
			this.EditPeriodEndDateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 24, true);
			this.EditPeriodEndDateButton.TabIndex = 4;
			this.EditPeriodEndDateButton.Click += new EventHandler(this.EditPeriodEndDateButton_Click);
			// 
			// ReverseToPeriodEdit
			// 
			this.ReverseToPeriodEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ReverseToPeriodEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|c0ed006a-c417-4e30-b03e-24112fb03385", "Reverse to Period");
			this.ReverseToPeriodEdit.DecimalPlaces = 0;
			this.ReverseToPeriodEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ReverseToPeriodEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.ReverseToPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 646, true);
			this.ReverseToPeriodEdit.Name = "ReverseToPeriodEdit";
			this.ReverseToPeriodEdit.ShowGroupSeparators = false;
			this.ReverseToPeriodEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.ReverseToPeriodEdit.TabIndex = 15;
			this.ReverseToPeriodEdit.Text = "0";
			this.ReverseToPeriodEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ReverseToPeriodEdit.Visible = false;
			// 
			// zLabel2
			// 
			this.zLabel2.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel2.AutoSize = true;
			this.zLabel2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|8617be69-3e01-4f6e-bac2-28f6b3b8b28b", "Periods from correction.");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 614, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 13, true);
			this.zLabel2.TabIndex = 13;
			this.zLabel2.Visible = false;
			// 
			// PeriodExcludeCalcEdit
			// 
			this.PeriodExcludeCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PeriodExcludeCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|e0d5e8af-a161-4748-bb4e-2fdf3926add0", "Exclude last");
			this.PeriodExcludeCalcEdit.DecimalPlaces = 0;
			this.PeriodExcludeCalcEdit.Decimals = 0;
			this.PeriodExcludeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(558, 591, true);
			this.PeriodExcludeCalcEdit.Name = "PeriodExcludeCalcEdit";
			this.PeriodExcludeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.PeriodExcludeCalcEdit.TabIndex = 12;
			this.PeriodExcludeCalcEdit.Text = "0";
			this.PeriodExcludeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PeriodExcludeCalcEdit.Visible = false;
			// 
			// PurgeGLDButton
			// 
			this.PurgeGLDButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PurgeGLDButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|44c0800a-67ec-4c22-a50d-6aa1ec585ba4", "Clear GLD configurations and Data (Only this Login Company)");
			this.PurgeGLDButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 418, true);
			this.PurgeGLDButton.Name = "PurgeGLDButton";
			this.PurgeGLDButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 50, true);
			this.PurgeGLDButton.TabIndex = 9;
			this.PurgeGLDButton.Visible = false;
			this.PurgeGLDButton.Click += new EventHandler(this.PurgeGLDButton_Click);
			// 
			// ReportOnlyButton
			// 
			this.ReportOnlyButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ReportOnlyButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|a87f871f-fcca-4f77-b3f6-353bc2f478c4", "Report Errors as XML");
			this.ReportOnlyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 390, true);
			this.ReportOnlyButton.Name = "ReportOnlyButton";
			this.ReportOnlyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.ReportOnlyButton.TabIndex = 8;
			this.ReportOnlyButton.Visible = false;
			this.ReportOnlyButton.Click += new EventHandler(this.ReportOnlyButton_Click);
			// 
			// SimulateButton
			// 
			this.SimulateButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SimulateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|8b60f1fe-7847-4f97-9f18-9c734941626c", "Re-Aggregate and Report");
			this.SimulateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 363, true);
			this.SimulateButton.Name = "SimulateButton";
			this.SimulateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.SimulateButton.TabIndex = 7;
			this.SimulateButton.Visible = false;
			this.SimulateButton.Click += new EventHandler(this.SimulateButton_Click);
			// 
			// ReAggregateSinglePeriodSingleCompany
			// 
			this.ReAggregateSinglePeriodSingleCompany.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ReAggregateSinglePeriodSingleCompany.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|8106f98a-2f91-4ce2-a880-daaef5257361", "Re-Aggregate (Single Period/Single Company)");
			this.ReAggregateSinglePeriodSingleCompany.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 206, true);
			this.ReAggregateSinglePeriodSingleCompany.Name = "ReAggregateSinglePeriodSingleCompany";
			this.ReAggregateSinglePeriodSingleCompany.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 62, true);
			this.ReAggregateSinglePeriodSingleCompany.TabIndex = 5;
			this.ReAggregateSinglePeriodSingleCompany.Visible = false;
			this.ReAggregateSinglePeriodSingleCompany.Click += new EventHandler(this.ReAggregateSinglePeriodSingleCompany_Click);
			// 
			// ReaggregateAllCompaniesButton
			// 
			this.ReaggregateAllCompaniesButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ReaggregateAllCompaniesButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|0debc420-402a-4288-879c-7aea7358beb8", "Re-Aggregate (All Companies)");
			this.ReaggregateAllCompaniesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 272, true);
			this.ReaggregateAllCompaniesButton.Name = "ReaggregateAllCompaniesButton";
			this.ReaggregateAllCompaniesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 43, true);
			this.ReaggregateAllCompaniesButton.TabIndex = 5;
			this.ReaggregateAllCompaniesButton.Visible = false;
			this.ReaggregateAllCompaniesButton.Click += new EventHandler(this.ReAggregateButton_Click);
			// 
			// SetUpNextYearButton
			// 
			this.SetUpNextYearButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SetUpNextYearButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|172f4070-8a9b-4486-9fa0-d018863ce9a4", "Set up next accounting year");
			this.SetUpNextYearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 122, true);
			this.SetUpNextYearButton.Name = "SetUpNextYearButton";
			this.SetUpNextYearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.SetUpNextYearButton.TabIndex = 3;
			this.SetUpNextYearButton.Click += new EventHandler(this.SetUpNextYearButton_Click);
			// 
			// CloseGLButton
			// 
			this.CloseGLButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseGLButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|16b4068b-30c9-4877-b679-510c8eb61692", "Close GL Period");
			this.CloseGLButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 53, true);
			this.CloseGLButton.Name = "CloseGLButton";
			this.CloseGLButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.CloseGLButton.TabIndex = 2;
			this.CloseGLButton.Click += new EventHandler(this.CloseGLButton_Click);
			// 
			// CloseSubledgerButton
			// 
			this.CloseSubledgerButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseSubledgerButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|0508eca9-a26b-4827-808e-04230f070a20", "Close Sub Ledger Period");
			this.CloseSubledgerButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 24, true);
			this.CloseSubledgerButton.Name = "CloseSubledgerButton";
			this.CloseSubledgerButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.CloseSubledgerButton.TabIndex = 1;
			this.CloseSubledgerButton.Click += new EventHandler(this.CloseSubledgerButton_Click);
			// 
			// PeriodsGrid
			// 
			this.PeriodsGrid.AllowNavigation = false;
			this.PeriodsGrid.AllowSorting = false;
			this.PeriodsGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PeriodsGrid, "Periods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((PeriodManager)(null)).Periods)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Period)(((System.Collections.IList)(((PeriodManager)(null)).Periods)).SyncRoot)).AM_Year)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Period)(((System.Collections.IList)(((PeriodManager)(null)).Periods)).SyncRoot)).AM_Period)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((Period)(((System.Collections.IList)(((PeriodManager)(null)).Periods)).SyncRoot)).AM_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((Period)(((System.Collections.IList)(((PeriodManager)(null)).Periods)).SyncRoot)).AM_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Period)(((System.Collections.IList)(((PeriodManager)(null)).Periods)).SyncRoot)).AM_IsSubLedgerClosed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Period)(((System.Collections.IList)(((PeriodManager)(null)).Periods)).SyncRoot)).AM_IsGeneralLedgerClosed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Period)(((System.Collections.IList)(((PeriodManager)(null)).Periods)).SyncRoot)).AM_IsSubledgerClosedForAdjustments)));
			this.PeriodsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AM_Year";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.ShowGroupSeparators = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "AM_Period";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.ShowGroupSeparators = false;
			zDateEditColumnStyleInfo1.ColumnName = "AM_StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo2.ColumnName = "AM_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.ColumnName = "AM_IsSubLedgerClosed";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.ColumnName = "AM_IsGeneralLedgerClosed";
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo3.ColumnName = "AM_IsSubledgerClosedForAdjustments";
			this.PeriodsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PeriodsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PeriodsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PeriodsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.PeriodsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PeriodsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.PeriodsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.PeriodsGrid.GridId = "0d8581ac-ab1b-4f32-a7ee-7c95177f8a57";
			this.PeriodsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PeriodsGrid.IsWholeRowSelectedOnClick = true;
			this.PeriodsGrid.LayoutKey = "zGrid1";
			this.PeriodsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 24, true);
			this.PeriodsGrid.Name = "PeriodsGrid";
			this.PeriodsGrid.ReadOnly = true;
			this.PeriodsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 603, true);
			this.PeriodsGrid.TabIndex = 0;
			this.PeriodsGrid.DoubleClick += new EventHandler(this.PeriodsGrid_DoubleClick);
			// 
			// PeriodInfoLabel
			// 
			this.PeriodInfoLabel.AutoSize = true;
			this.PeriodInfoLabel.IsFontBold = true;
			this.PeriodInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.PeriodInfoLabel.Name = "PeriodInfoLabel";
			this.PeriodInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.PeriodInfoLabel.TabIndex = 2;
			// 
			// RequeueGLDButton
			// 
			this.RecoverPartOfGldPeriodsButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RecoverPartOfGldPeriodsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodManagementForm|212CD5ED-EE36-4C85-AF20-2BFD79A7D7E4", "Regenerate GLD (This Company Only)");
			this.RecoverPartOfGldPeriodsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 471, true);
			this.RecoverPartOfGldPeriodsButton.Name = "RecoverPartOfGldPeriodsButton";
			this.RecoverPartOfGldPeriodsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 36, true);
			this.RecoverPartOfGldPeriodsButton.TabIndex = 10;
			this.RecoverPartOfGldPeriodsButton.ToolTipCaption = null;
			this.RecoverPartOfGldPeriodsButton.Visible = false;
			this.RecoverPartOfGldPeriodsButton.Click += new EventHandler(this.RecoverPartOfGldPeriodsButton_Click);
			// 
			// PeriodManagementForm
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PeriodInfoLabel);
			this.Controls.Add(this.DescriptionGroupBox);
			this.Controls.Add(this.FinancialYearCalcEdit);
			this.Name = "PeriodManagementForm";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 694, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DescriptionGroupBox.ResumeLayout(false);
			this.DescriptionGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PeriodsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
