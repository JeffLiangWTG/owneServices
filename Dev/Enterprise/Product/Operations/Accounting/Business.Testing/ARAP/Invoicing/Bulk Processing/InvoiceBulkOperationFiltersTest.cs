using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class InvoiceBulkOperationFiltersTest : FilterStripBusinessObjectTestCase
	{
		public void TextFiterMaxLengths()
		{
			AssertEquals($"{InvoiceBulkOperationFilterHelper.TransportModeFilterName} max length", JobConsolSchema.JK_TransportMode.MaxLength, Filters[InvoiceBulkOperationFilterHelper.TransportModeFilterName].MaxLength);
			AssertEquals($"{InvoiceBulkOperationFilterHelper.ContainerModeFilterName} max length", JobConsolSchema.JK_ConsolMode.MaxLength, Filters[InvoiceBulkOperationFilterHelper.ContainerModeFilterName].MaxLength);
			AssertEquals($"{InvoiceBulkOperationFilterHelper.FlightFilterName} max length", JobVoyageSchema.JV_VoyageFlight.MaxLength, Filters[InvoiceBulkOperationFilterHelper.FlightFilterName].MaxLength);
			AssertEquals($"{InvoiceBulkOperationFilterHelper.MasterBillFilterName} max length", JobConsolSchema.JK_MasterBillNum.MaxLength, Filters[InvoiceBulkOperationFilterHelper.MasterBillFilterName].MaxLength);
			AssertEquals($"{InvoiceBulkOperationFilterHelper.CoLoadMasterBillFilterName} max length", JobConsolSchema.JK_CoLoadMasterBill.MaxLength, Filters[InvoiceBulkOperationFilterHelper.CoLoadMasterBillFilterName].MaxLength);
			AssertEquals($"{InvoiceBulkOperationFilterHelper.ContainerNumberFilterName} max length", JobContainerSchema.JC_ContainerNum.MaxLength, Filters[InvoiceBulkOperationFilterHelper.ContainerNumberFilterName].MaxLength);
		}

		public virtual void TestRelatedMilestoneFilter()
		{
			AssertNotNull((ModuleDateFilter)Filters["Milestone Date (Related)"]);
			AssertNotNull((ModuleTextFilter)Filters["Milestone Completed (Related)"]);
			AssertNotNull((ModuleDateFilter)Filters["Next Milestone (Related)"]);
			AssertNotNull((ModuleDateFilter)Filters["Last Completed Milestone (Related)"]);
		}

		public void TestDefaultCreditorFilterValueOnApplyDefaults()
		{
			ZGuid expectedCreditorValue = TestObjectCreator.AALSHI.PK;
			AssertNotNull(Filters);
			Filters.DefaultCreditorFilterValue_ForTestOnly = expectedCreditorValue;
			Filters.GetQuery();

			((ModuleGuidFilter)Filters[InvoiceBulkOperationFilterHelper.CreditorFilterName]).Property = testObjectCreator.ABIGAS.PK;

			Filters.ApplyDefaults_ForTestOnly();

			AssertEquals("Creditor filter must have default value.", expectedCreditorValue, ((ModuleGuidFilter)filters[InvoiceBulkOperationFilterHelper.CreditorFilterName]).Property);
		}

		public virtual void TestDefaultCreditorFilterValueOnApplyDefaults_WhenTaxBranchReportingIsON()
		{
			Assert(true);
		}

		public void TestDefaultSupplierCostReferenceFilterValueOnApplyDefaults()
		{
			ZString expectedSupplierCostReference = "ABC";
			AssertNotNull(Filters);
			Filters.DefaultSupplierCostReferenceFilterValue_ForTestOnly = expectedSupplierCostReference;
			Filters.GetQuery();

			((ModuleNumberFilter)Filters[InvoiceBulkOperationFilterHelper.SupplierCostReferenceFilterName]).Property = expectedSupplierCostReference;

			Filters.ApplyDefaults_ForTestOnly();

			AssertEquals("Supplier Cost Reference filter must have default value.", expectedSupplierCostReference, ((ModuleNumberFilter)filters[InvoiceBulkOperationFilterHelper.SupplierCostReferenceFilterName]).Property);
		}

		public void TestFilterLayoutDoesNotUseTheSameFactoryAsPassedDuringConstruction()
		{
			var filterStripBizObj = GetNewFilterStripBusinessObject();
			AssertNotEquals("The filter object should not use the same factory as passed during construction", filterStripBizObj.Factory._Instance, Factory._Instance);
		}

		public abstract void TestCoLoadMasterBillFilter();

		#region Implementation

		protected InvoiceBulkOperationFilters Filters
		{
			get
			{
				if (filters == null)
				{
					filters = GetNewFilters();
					ActivateAlwaysVisibleFilters(filters);
				}
				return filters;
			}
		}
		InvoiceBulkOperationFilters filters;

		protected abstract InvoiceBulkOperationFilters GetNewFilters();

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		protected void ActivateAlwaysVisibleFilters(InvoiceBulkOperationFilters filters = null)
		{
			if (filters == null)
			{
				filters = Filters;
			}

			foreach (ModuleFilter filter in filters.AlwaysVisibleModuleFilters)
			{
				filter.IsActive = true;
			}
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, InvoiceBulkOperationFilterHelper.ReceivingAgentFilterName));
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, InvoiceBulkOperationFilterHelper.SendingAgentFilterName));
			return result;
		}

		#endregion

	}
}
