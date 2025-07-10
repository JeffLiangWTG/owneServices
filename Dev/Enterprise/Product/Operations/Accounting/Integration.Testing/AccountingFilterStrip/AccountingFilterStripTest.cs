using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Integration.Testing
{
	public abstract class AccountingFilterStripTest<FilteredBOType> : TestCaseWithFactory, IAccountingFilterStripTest<FilteredBOType>, IAccountingFilterStripTestDataSupplier<FilteredBOType> where FilteredBOType : BusinessObject
	{
		public virtual (FilteredBOType filteredBO1, FilteredBOType filteredBO2, FilteredBOType filteredBO3, FilteredBOType filteredBO4, FilteredBOType filteredBO5, FilteredBOType filteredBO6) GetSixNewBusinessObjectsForFilterCollection() => (GetNewBusinessObjectForFilterCollection(), GetNewBusinessObjectForFilterCollection(), GetNewBusinessObjectForFilterCollection(), GetNewBusinessObjectForFilterCollection(), GetNewBusinessObjectForFilterCollection(), GetNewBusinessObjectForFilterCollection());
		protected abstract FilteredBOType GetNewBusinessObjectForFilterCollection();
		protected virtual JobHeader GetJobForBusinessObjectFromFilterCollection(FilteredBOType bizo) => new JobHeader.Loader(GetJobParent(bizo)).TryLoadOrCreateWithoutMutexForTestOnly();
		protected virtual IJobHeaderParent GetJobParent(FilteredBOType bizo) => bizo as IJobHeaderParent;
		protected abstract ModuleIdentifier FilterStripModuleID
		{
			get;
		}

		protected virtual ControllerID ControllerIDForBillingIfDifferFromModuleController
		{
			get
			{
				return null;
			}
		}

		protected virtual bool ShouldUseBillingFilters
		{
			get
			{
				return true;
			}
		}

		protected virtual object GetCustomFormWithJobInvoicing()
		{
			return null;
		}

		protected virtual bool IsRevenueFiltersAdded
		{
			get
			{
				return true;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestImplementation.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestImplementation.TearDown();
		}

		IAccountingFilterStripTestImplementation<FilteredBOType> TestImplementation
		{
			get
			{
				if (TestImplementation_innerValue == null)
				{
					Type testImplementationInterfaceType = typeof(IAccountingFilterStripTestImplementation<>).GetInterfaces()[0];
					Type testImplementationObjectType = ObjectFactory.GetType(testImplementationInterfaceType);
					Type testImplementationObjectGenericType = testImplementationObjectType.MakeGenericType(typeof(FilteredBOType));
					TestImplementation_innerValue = (IAccountingFilterStripTestImplementation<FilteredBOType>)Activator.CreateInstance(testImplementationObjectGenericType);
					TestImplementation_innerValue.SetTestDataSupplier(this);
				}

				return TestImplementation_innerValue;
			}
		}

		IAccountingFilterStripTestImplementation<FilteredBOType> TestImplementation_innerValue;
		#region IAccountingFilterStripTestWrapper Members
		[RequiresSTA]
		public void TestControlForAmountFilters()
		{
			TestImplementation.TestControlForAmountFilters();
		}

		[GuiTest]
		public void TestAddJobManagementFiltersOnlyIfSecurityIsAllowed()
		{
			TestImplementation.TestAddJobManagementFiltersOnlyIfSecurityIsAllowed();
		}

		public void TestLocalJobReferenceFilter()
		{
			TestImplementation.TestLocalJobReferenceFilter();
		}

		public void TestSupplierCostReferenceFilter()
		{
			if (ShouldUseBillingFilters)
			{
				TestImplementation.TestSupplierCostReferenceFilter();
			}
			else
			{
				Assert("TestSupplierCostReferenceFilter not applicable", true);
			}
		}

		public void TestJobBranchManagementCodeFilter()
		{
			TestImplementation.TestJobBranchManagementCodeFilter();
		}

		public void TestBranchFilter()
		{
			TestImplementation.TestBranchFilter();
		}

		public void TestChargesWithDebtorFilter()
		{
			if (ShouldUseBillingFilters)
			{
				TestImplementation.TestChargesWithDebtorFilter();
			}
			else
			{
				Assert("BillingFilters not applicable", true);
			}
		}

		public void TestChargesWithCreditorFilter()
		{
			if (ShouldUseBillingFilters)
			{
				TestImplementation.TestChargesWithCreditorFilter();
			}
			else
			{
				Assert("BillingFilters not applicable", true);
			}
		}

		public void TestDepartmentFilter()
		{
			TestImplementation.TestDepartmentFilter();
		}

		public void TestOperationStaffFilter()
		{
			TestImplementation.TestOperationStaffFilter();
		}

		public void TestSalesStaffFilter()
		{
			TestImplementation.TestSalesStaffFilter();
		}

		public void TestTaxBranchFilter()
		{
			TestImplementation.TestTaxBranchFilter();
		}

		public void TestTaxBranchFilterExist()
		{
			TestImplementation.TestTaxBranchFilterExist();
		}

		public void TestJobOpenDateFilter()
		{
			TestImplementation.TestJobOpenDateFilter();
		}

		public void TestJobCloseDateFilter()
		{
			TestImplementation.TestJobCloseDateFilter();
		}

		public void TestJobRevenueRecognitionDateFilter()
		{
			TestImplementation.TestJobRevenueRecognitionDateFilter();
		}

		public void TestJobOpenOrCloseDateFilter()
		{
			TestImplementation.TestJobOpenOrCloseDateFilter();
		}

		public void TestJobManagementFiltersWithMaxAmounts()
		{
			TestImplementation.TestJobManagementFiltersWithMaxAmounts();
		}

		public void TestJobProfitFilter()
		{
			TestImplementation.TestJobProfitFilter();
		}

		public void TestJobMarginPercentFilter()
		{
			TestImplementation.TestJobMarginPercentFilter();
		}

		public void TestJobRevenueAmountFilter()
		{
			TestImplementation.TestJobRevenueAmountFilter();
		}

		public void TestJobWIPAmountFilter()
		{
			TestImplementation.TestJobWIPAmountFilter();
		}

		public void TestJobWIPAmountExcludingDeferredChargesFilter()
		{
			TestImplementation.TestJobWIPAmountExcludingDeferredChargesFilter();
		}

		public void TestJobWIPAmountDeferredChargesOnlyFilter()
		{
			TestImplementation.TestJobWIPAmountDeferredChargesOnlyFilter();
		}

		public void TestJobCostAmountFilter()
		{
			TestImplementation.TestJobCostAmountFilter();
		}

		public void TestJobAccrualAmountFilter()
		{
			TestImplementation.TestJobAccrualAmountFilter();
		}

		public void TestAmountFiltersWhenJobsDoesntHaveTransactionLines()
		{
			TestImplementation.TestAmountFiltersWhenJobsDoesntHaveTransactionLines();
		}

		public void TestAmountFiltersTogether()
		{
			TestImplementation.TestAmountFiltersTogether();
		}

		public void TestHasAccrualWIPFilter()
		{
			TestImplementation.TestHasAccrualWIPFilter();
		}

		public void TestHasAccrualWIPFilterTogether()
		{
			TestImplementation.TestHasAccrualWIPFilterTogether();
		}

		void IAccountingFilterStripTestImplementation<FilteredBOType>.SetUp()
		{
			SetUp();
		}

		void IAccountingFilterStripTestImplementation<FilteredBOType>.TearDown()
		{
			TearDown();
		}

		void IAccountingFilterStripTestImplementation<FilteredBOType>.SetTestDataSupplier(IAccountingFilterStripTestDataSupplier<FilteredBOType> testDataSupplier)
		{
		}

		#endregion
		#region IAccountingFilterStripTestDataSupplier Members
		BusinessObjectFactory IAccountingFilterStripTestDataSupplier<FilteredBOType>.Factory
		{
			get
			{
				return Factory;
			}
		}

		FilteredBOType IAccountingFilterStripTestDataSupplier<FilteredBOType>.GetNewBusinessObjectForFilterCollection()
		{
			return GetNewBusinessObjectForFilterCollection();
		}

		JobHeader IAccountingFilterStripTestDataSupplier<FilteredBOType>.GetJobForBusinessObjectFromFilterCollection(FilteredBOType bizo)
		{
			return GetJobForBusinessObjectFromFilterCollection(bizo);
		}

		IJobHeaderParent IAccountingFilterStripTestDataSupplier<FilteredBOType>.GetJobParent(FilteredBOType bizo)
		{
			return GetJobParent(bizo);
		}

		ModuleIdentifier IAccountingFilterStripTestDataSupplier<FilteredBOType>.FilterStripModuleID
		{
			get
			{
				return FilterStripModuleID;
			}
		}

		ControllerID IAccountingFilterStripTestDataSupplier<FilteredBOType>.ControllerIDForBillingIfDifferFromModuleController
		{
			get
			{
				return ControllerIDForBillingIfDifferFromModuleController;
			}
		}

		object IAccountingFilterStripTestDataSupplier<FilteredBOType>.GetCustomFormWithJobInvoicing()
		{
			return GetCustomFormWithJobInvoicing();
		}

		bool IAccountingFilterStripTestDataSupplier<FilteredBOType>.IsRevenueFiltersAdded
		{
			get
			{
				return IsRevenueFiltersAdded;
			}
		}
		#endregion
	}

	interface IAccountingFilterStripTest<FilteredBOType> : IAccountingFilterStripTestImplementation<FilteredBOType> where FilteredBOType : BusinessObject
	{
	}
}
