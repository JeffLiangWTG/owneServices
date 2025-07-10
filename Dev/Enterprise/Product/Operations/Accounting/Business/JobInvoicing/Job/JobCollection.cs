using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	[ModuleID(ModuleId.JobHeader)]
	public partial class JobCollection : BusinessObjectCollection<Job>, IBindingList
	{
		public JobCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public JobCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("You cannot directly add to this collection. You need to use the JobHeader.Loader to create a new Job.");
		}

		protected override BusinessObject AddNewCore(Type bizoType)
		{
			throw new NotSupportedException("You cannot directly add to this collection. You need to use the JobHeader.Loader to create a new Job.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public Job GetJobForOperationsPlugin(IJobInvoicingPlugIn plugin)
		{
			foreach (Job job in this)
			{
				if (job.JH_ParentID == plugin.PK)
				{
					return job;
				}
			}
			return null;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(JobHeaderSchema.JH_IsActive, SQLComparisonOperator.Equal, true);

			return query;
		}

		object IBindingList.AddNew()
		{
			//RelatedCurrencyManager.ParentManager_CurrentItemChanged annoyingly calls AddNew(), so IBindingList.AddNew will return a null Job
			return Factory.GetNull<Job>();
		}
	}
}
