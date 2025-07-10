using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRuleTargetCountryCollection : NonPersistentBusinessObjectCollection<ComplianceRuleTargetCountry>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		protected override bool AllowNewCore => false;

		public void AddAll(RefCountry[] countries)
		{
			foreach (var country in countries)
			{
				Add(new ComplianceRuleTargetCountry(country));
			}
		}
	}
}
