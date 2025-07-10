using CargoWise.EntityFramework;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRuleCandidateCollection : NonPersistentBusinessObjectCollection<ComplianceRuleCandidate>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

		protected override bool AllowNewCore => false;

		public void AddAll(ComplianceRule[] rules)
		{
			foreach (var rule in rules)
			{
				Add(new ComplianceRuleCandidate(rule));
			}
		}
	}
}
