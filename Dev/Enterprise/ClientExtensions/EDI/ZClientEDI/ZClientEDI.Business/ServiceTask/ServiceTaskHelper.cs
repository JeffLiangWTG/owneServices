using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.ServiceTask
{
	public static class ServiceTaskHelper
	{
		public static GlbBranch GetBranchForEDIServiceTasks(BusinessObjectFactory businessObjectFactory = null)
		{
			var branchPK = EDIDataRegistry.Instance.BranchForEDIServiceTasks.Value;
			var factory = businessObjectFactory ?? new BusinessObjectFactory();

			var branch = factory.Load<GlbBranch>(branchPK);
			if (branch == null || !branch.GB_IsActive)
			{
				var query = new ZQuery(GlbBranchSchema.GB_IsActive, true);
				query.OrderBy = GlbBranch.Schema.GB_SystemCreateTimeUtc;
				branch = factory.LoadTop1<GlbBranch>(query);
			}
			return branch;
		}
	}
}
