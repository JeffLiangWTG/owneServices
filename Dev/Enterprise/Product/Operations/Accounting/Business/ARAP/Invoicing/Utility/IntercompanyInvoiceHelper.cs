using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class IntercompanyInvoiceHelper
	{
		public static ZString GetReceivingOperator(BusinessObjectFactory factory, ZGuid jobPK)
		{
			var receivingJob = GetReceivingJob(factory, jobPK);
			return (receivingJob != null) ? receivingJob.JH_GS_NKRepOps : ZString.Empty;
		}

		public static ZGuid GetReceivingBranch(BusinessObjectFactory factory, ZGuid jobPK)
		{
			var receivingJob = GetReceivingJob(factory, jobPK);
			return (receivingJob != null) ? receivingJob.JH_GB : ZGuid.Empty;
		}

		public static ZGuid GetReceivingDepartment(BusinessObjectFactory factory, ZGuid jobPK)
		{
			var receivingJob = GetReceivingJob(factory, jobPK);
			return (receivingJob != null) ? receivingJob.JH_GE : ZGuid.Empty;
		}

		static JobHeader GetReceivingJob(BusinessObjectFactory factory, ZGuid jobPK)
		{
			if (jobPK.IsValid)
			{
				var job = factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.PK, jobPK));
				if (job != null)
				{
					var query = new ZQuery(JobHeaderSchema.JH_JobNum, job.JH_JobNum);
					query.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

					return factory.LoadTop1<JobHeader>(query);
				}
			}

			return null;
		}
	}
}
