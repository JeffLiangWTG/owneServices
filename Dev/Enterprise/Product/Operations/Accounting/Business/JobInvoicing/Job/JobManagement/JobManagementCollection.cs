
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobManagementCollection : BusinessObjectCollection<JobManagement>
	{
		public JobManagementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public JobManagementCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
		}
	}
}
