using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBaseBulkChargeImporterDependentJobCollection : DependentJobCollection<InvoicingBaseBulkChargeImporter>
	{
		public InvoicingBaseBulkChargeImporterDependentJobCollection(InvoicingBaseBulkChargeImporter master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public new InvoicingBaseBulkChargeImporterDependentJob this[int index]
		{
			get { return (InvoicingBaseBulkChargeImporterDependentJob)Elements[index]; }
		}

		public new InvoicingBaseBulkChargeImporterDependentJob AddNew()
		{
			return (InvoicingBaseBulkChargeImporterDependentJob)base.AddNew();
		}

		public new InvoicingBaseBulkChargeImporterDependentJob AddNew(Type bizObjType)
		{
			return (InvoicingBaseBulkChargeImporterDependentJob)base.AddNew(bizObjType);
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			InvoicingBaseBulkChargeImporterDependentJob job = (InvoicingBaseBulkChargeImporterDependentJob)bizOAdded;
			job.ClearIsSelectedChangedEventHandlers();
			using (Master.GetUpdateSelectedLocalTotalSuspender(false))
			{
				job.IsSelectedForImport = true;
			}
			job.IsSelectedChanged += job_IsSelectedChanged;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery additionalFilter = base.CreateAdditionalFilter();
			additionalFilter.AddToFilter(JobHeaderSchema.JH_ParentTableCode, SQLComparisonOperator.NotEqual, RatingHeaderSchema.Constants.Prefix);
			return additionalFilter;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery relationshipFilter = base.CreateRelationshipFilter();
			relationshipFilter.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			return relationshipFilter;
		}

		void job_IsSelectedChanged(object sender, EventArgs e)
		{
			Master.UpdateSelectedLocalTotal();

#if DEBUG
			if (Globals.IsTest)
			{
				IsSelectedChangedTotalExecutionTimes++;
			}
#endif
		}

#if DEBUG
		internal int IsSelectedChangedTotalExecutionTimes;
#endif

		protected override void OnLoaded()
		{
			base.OnLoaded();
			Master.UpdateSelectedLocalTotal();
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			alternativeAdditionalFilter.AddToFilter(AdditionalFilter);
			base.Load(alternativeAdditionalFilter);
		}
	}
}
