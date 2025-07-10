using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceJobEntityCacheCollection : DependentBusinessObjectCollection<ComplianceJobEntityCache, ComplianceRiskStatus>
	{
		public ComplianceJobEntityCacheCollection(ComplianceRiskStatus master)
			: base(master)
		{
		}

		protected override string FkColumnName => ComplianceJobEntityCacheSchema.CJE_COR_ComplianceRisk.Name;
	}
}
