using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GenericJob
{
	public class GenericJobFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public GenericJobFetchStrategy(GenericJob genericJob)
			: base(genericJob)
		{
		}

		GenericJob Job
		{
			get { return BusinessObject as GenericJob; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Type consumerType = Job.GetConsumerType();
			if (consumerType != null && !typeof(NonPersistentBusinessObject).IsAssignableFrom(consumerType))
			{
				Factory.AddFetchHint(consumerType, ((IJobInvoicingPlugIn)Job).PK);
				Factory.AddFetchHint(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, Job.VJ_ForeignKey);
			}
		}
	}
}