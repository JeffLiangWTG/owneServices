using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Services.OperationalActions.Support;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.OperationalActions
{
	public class LVXConsolidationOperationalActionMethodApplicator : OperationalActionMethodApplicator
	{
		public LVXConsolidationOperationalActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("115b33b0-e227-4ef4-9593-d5f214a444cb", "Consolidate operational action"), factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var declarations = from target in targets select target as JobDeclaration;
			new LVXJobsConsolidateRunner(new OperationalActionSectionLogWrapper(log), Factory).ConsolidateLVXJobs(declarations.ToArray(), true);
		}
	}
}
