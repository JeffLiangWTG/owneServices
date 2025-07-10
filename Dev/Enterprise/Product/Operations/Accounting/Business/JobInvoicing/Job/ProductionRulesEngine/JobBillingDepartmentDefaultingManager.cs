using System.Linq;
using CargoWise.Types;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobBillingDepartmentDefaultingManager : JobBillingDefaultingManager, IJobBillingDepartmentDefaultingManager
	{
		public JobBillingDepartmentDefaultingManager(IFactLoaderProvider factLoaderProvider)
			: base(factLoaderProvider)
		{
		}

		protected override RulesContextType ConetxtType => RulesContextType.JobBillingDepartmentDefaulting;

		protected override void SetDefaultValue(ProductionRulesEngineResult result)
		{
			var departmentResultFact = result.Facts.OfType<DepartmentResultFact>()?.FirstOrDefault();
			if (departmentResultFact != null)
			{
				DefaultValue = new ZGuid(departmentResultFact.DepartmentToDefault.Fact.PK);
			}
		}
	}
}
