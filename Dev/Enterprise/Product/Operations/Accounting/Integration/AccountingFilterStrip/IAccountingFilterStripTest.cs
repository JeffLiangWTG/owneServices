using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Integration.Testing
{
	public interface IAccountingFilterStripTestImplementation
	{
	}

#if DEBUG
	public interface IAccountingFilterStripTestDataSupplier<FilteredBOType>
		where FilteredBOType : BusinessObject
	{
		BusinessObjectFactory Factory { get; }
		FilteredBOType GetNewBusinessObjectForFilterCollection();

		(
			FilteredBOType filteredBO1,
			FilteredBOType filteredBO2,
			FilteredBOType filteredBO3,
			FilteredBOType filteredBO4,
			FilteredBOType filteredBO5,
			FilteredBOType filteredBO6) GetSixNewBusinessObjectsForFilterCollection();

		JobHeader GetJobForBusinessObjectFromFilterCollection(FilteredBOType bizo);
		IJobHeaderParent GetJobParent(FilteredBOType bizo);
		ModuleIdentifier FilterStripModuleID { get; }
		ControllerID ControllerIDForBillingIfDifferFromModuleController { get; }
		object GetCustomFormWithJobInvoicing();
		bool IsRevenueFiltersAdded { get; }
	}

	public interface IAccountingFilterStripTestImplementation<FilteredBOType> : IAccountingFilterStripTestImplementation
		where FilteredBOType : BusinessObject
	{
		void SetUp();
		void TearDown();

		void SetTestDataSupplier(IAccountingFilterStripTestDataSupplier<FilteredBOType> testDataSupplier);

		void TestControlForAmountFilters();
		void TestAddJobManagementFiltersOnlyIfSecurityIsAllowed();

		void TestLocalJobReferenceFilter();
		void TestSupplierCostReferenceFilter();
		void TestChargesWithDebtorFilter();
		void TestChargesWithCreditorFilter();

		void TestJobBranchManagementCodeFilter();
		void TestBranchFilter();
		void TestDepartmentFilter();
		void TestOperationStaffFilter();
		void TestSalesStaffFilter();
		void TestTaxBranchFilter();
		void TestTaxBranchFilterExist();

		void TestJobOpenDateFilter();
		void TestJobCloseDateFilter();
		void TestJobRevenueRecognitionDateFilter();
		void TestJobOpenOrCloseDateFilter();

		void TestJobProfitFilter();
		void TestJobMarginPercentFilter();
		void TestJobRevenueAmountFilter();
		void TestJobWIPAmountFilter();
		void TestJobWIPAmountExcludingDeferredChargesFilter();
		void TestJobWIPAmountDeferredChargesOnlyFilter();
		void TestJobCostAmountFilter();
		void TestJobAccrualAmountFilter();
		void TestAmountFiltersWhenJobsDoesntHaveTransactionLines();
		void TestAmountFiltersTogether();

		void TestHasAccrualWIPFilter();
		void TestHasAccrualWIPFilterTogether();

		void TestJobManagementFiltersWithMaxAmounts();
	}
#endif
}
