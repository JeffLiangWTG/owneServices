using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceRuleTargetCountry : NonPersistentBusinessObject
	{
		public ComplianceRuleTargetCountry(RefCountry refCountry)
			: base(refCountry.Factory)
		{
			country = refCountry;
		}

		readonly RefCountry country;

		public ZString Code => country.RN_Code;

		public ZString Name => country.RN_Desc;
	}
}
