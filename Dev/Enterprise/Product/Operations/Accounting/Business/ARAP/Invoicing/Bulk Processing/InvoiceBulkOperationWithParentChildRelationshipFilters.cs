using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class InvoiceBulkOperationWithParentChildRelationshipFilters : InvoiceBulkOperationFilters
	{
		public InvoiceBulkOperationWithParentChildRelationshipFilters(InvoicingBase parentInvoice)
			: base(parentInvoice.Factory)
		{
			if (!(parentInvoice is APInvoice || parentInvoice is APCreditNote))
			{
				throw new ArgumentException("This class only supports APInvoice and APCreditNote.");
			}
			this.ParentInvoice = parentInvoice;
		}

		public InvoiceBulkOperationWithParentChildRelationshipFilters()
		{
		}

		protected readonly InvoicingBase ParentInvoice;

		protected void SetDefaultValueInFilters()
		{
			DefaultCreditorFilterValue = ParentInvoice.AH_OH;
			DefaultSupplierCostReferenceFilterValue = ParentInvoice.AH_ChequeOrReference;

			var isTaxBranchApplicable = AccountingMasterFilesUtils.IsTaxBranchApplicableForTransaction(ParentInvoice);
			if (isTaxBranchApplicable)
			{
				DefaultTaxBranchFilterValue = ParentInvoice.AH_GB_TaxBranch;
			}
		}

		public ZQuery GetChildQuery()
		{
			IsChildFiltersGenerationRunning = true;

			try
			{
				return GetQuery();
			}
			finally
			{
				IsChildFiltersGenerationRunning = false;
			}
		}

		protected bool IsChildFiltersGenerationRunning;

		#region SubGroups

		#region MilestoneSubGroup

		protected ModuleFilterSubGroup MilestoneSubGroup
		{
			get
			{
				return milestoneSubGroup ?? (milestoneSubGroup = new MilestoneSubGroupImplementation(this));
			}
		}
		ModuleFilterSubGroup milestoneSubGroup;

		class MilestoneSubGroupImplementation : ModuleFilterSubGroup
		{
			public MilestoneSubGroupImplementation(InvoiceBulkOperationWithParentChildRelationshipFilters filter)
				: base()
			{
				this.Filter = filter;
			}
			readonly InvoiceBulkOperationWithParentChildRelationshipFilters Filter;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return !Filter.IsChildFiltersGenerationRunning ? filter : new ZQuery();
			}
		}

		#endregion

		#endregion

		#region OnModuleFiltersCreated

		protected override void OnModuleFiltersCreated()
		{
			base.OnModuleFiltersCreated();
			CustomiseMileStoneFilters();
		}

		void CustomiseMileStoneFilters()
		{
			var milestoneRelatedDateFilter = (ModuleDateFilter)this["Milestone Date"];
			milestoneRelatedDateFilter.SubGroup = MilestoneSubGroup;
			var miestoneRelatedCompletedFilter = (ModuleTextFilter)this["Milestone Completed"];
			miestoneRelatedCompletedFilter.SubGroup = MilestoneSubGroup;
			var milestoneRelatedNextFilter = (ModuleDateFilter)this["Next Milestone"];
			milestoneRelatedNextFilter.SubGroup = MilestoneSubGroup;
			var milestoneRelatedLastCompletedFilter = (ModuleDateFilter)this["Last Completed Milestone"];
			milestoneRelatedLastCompletedFilter.SubGroup = MilestoneSubGroup;

			var anyOpenTaskAssignedToFilter = (ModuleNkFilter)this["Any Open Task Assigned To"];
			anyOpenTaskAssignedToFilter.SubGroup = MilestoneSubGroup;
			var nextTaskAssignedToFilter = (ModuleNkFilter)this["Next Task Assigned To"];
			nextTaskAssignedToFilter.SubGroup = MilestoneSubGroup;
			var tasksFilter = (ModuleGuidForeignCollectionFilter)this["Tasks"];
			tasksFilter.SubGroup = MilestoneSubGroup;
			var exceptionsFilter = (ModuleGuidForeignCollectionFilter)this["Exceptions"];
			exceptionsFilter.SubGroup = MilestoneSubGroup;
			var milestonesFilter = (ModuleGuidForeignCollectionFilter)this["Milestones"];
			milestonesFilter.SubGroup = MilestoneSubGroup;
			var triggersFilter = (ModuleGuidForeignCollectionFilter)this["Triggers"];
			triggersFilter.SubGroup = MilestoneSubGroup;
		}

		#endregion
	}
}
