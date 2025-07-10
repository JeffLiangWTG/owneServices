#if DEBUG

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class PeriodManagementForm
	{
		public void GenerateNewPeriod_ForTestOnly(Business.PeriodManagement.NewYearPeriodSettings newYearSettings, BusinessObjectFactory newFactory, Business.PeriodManagement.PeriodManager newPeriodManager)
		{
			GenerateNewPeriod(newYearSettings, newFactory, newPeriodManager);
		}

		public Business.PeriodManagement.PeriodManager PeriodManager_ForTestOnly
		{
			get { return PeriodManager; }
			set { PeriodManager = value; }
		}

		public AccPeriodManagement LastPeriod_ForTestOnly => LastPeriod;

		public ZArchitecture.ZCalcEdit FinancialYearCalcEdit_ForTestOnly
		{
			get { return FinancialYearCalcEdit; }
			set { FinancialYearCalcEdit = value; }
		}

		public ZButton CloseSubledgerButton_ForTestOnly
		{
			get { return CloseSubledgerButton; }
			set { CloseSubledgerButton = value; }
		}

		public ZButton CloseGLButton_ForTestOnly
		{
			get { return CloseGLButton; }
			set { CloseGLButton = value; }
		}

		public ZButton SetUpNextYearButton_ForTestOnly
		{
			get { return SetUpNextYearButton; }
			set { SetUpNextYearButton = value; }
		}

		public ZButton EditPeriodEndDateButton_ForTestOnly
		{
			get { return EditPeriodEndDateButton; }
			set { EditPeriodEndDateButton = value; }
		}

		public ZButton ReSetPeriodsButton_ForTestOnly
		{
			get { return ReSetPeriodsButton; }
			set { ReSetPeriodsButton = value; }
		}

		public ZButton ExpectedWipAcrAggRes_ForTestOnly
		{
			get { return ExpectedWipAcrAggRes; }
			set { ExpectedWipAcrAggRes = value; }
		}

		public ZButton ExpectedDebtorCreditorAggRes_ForTestOnly
		{
			get { return ExpectedDebtorCreditorAggRes; }
			set { ExpectedDebtorCreditorAggRes = value; }
		}

		public ZButton ReaggregateAllCompaniesButton_ForTestOnly
		{
			get { return ReaggregateAllCompaniesButton; }
			set { ReaggregateAllCompaniesButton = value; }
		}

		public ZButton ReaggregateThisCompanyButton_ForTestOnly
		{
			get { return ReaggregateThisCompanyButton; }
			set { ReaggregateThisCompanyButton = value; }
		}

		public ZButton SimulateButton_ForTestOnly
		{
			get { return SimulateButton; }
			set { SimulateButton = value; }
		}

		public ZButton ReportOnlyButton_ForTestOnly
		{
			get { return ReportOnlyButton; }
			set { ReportOnlyButton = value; }
		}

		public ZButton RecoverPartOfGldPeriodsButton_ForTestOnly
			=> RecoverPartOfGldPeriodsButton;

		public ZArchitecture.ZCalcEdit PeriodExcludeCalcEdit_ForTestOnly
		{
			get { return PeriodExcludeCalcEdit; }
			set { PeriodExcludeCalcEdit = value; }
		}

		public ZArchitecture.ZLabel ZLabel2_ForTestOnly
		{
			get { return zLabel2; }
			set { zLabel2 = value; }
		}

		public ZArchitecture.ZCalcEdit ReverseToPeriodEdit_ForTestOnly
		{
			get { return ReverseToPeriodEdit; }
			set { ReverseToPeriodEdit = value; }
		}

		public ZButton PurgeGLDButton_ForTestOnly
		{
			get { return PurgeGLDButton; }
			set { PurgeGLDButton = value; }
		}

		public ZButton CloseGLAdjustmentsButton_ForTestOnly
		{
			get { return CloseGLAdjustmentsButton; }
			set { CloseGLAdjustmentsButton = value; }
		}

		public void OpenPeriodEndDateEditFormNewYearSetting_ForTestOnly(Business.PeriodManagement.NewYearPeriodSettings newYearSettings)
		{
			OpenPeriodEndDateEditFormNewYearSetting(newYearSettings);
		}

		public ZArchitecture.ZGrid PeriodsGrid_ForTestOnly
		{
			get { return PeriodsGrid; }
			set { PeriodsGrid = value; }
		}
	}
}

#endif
