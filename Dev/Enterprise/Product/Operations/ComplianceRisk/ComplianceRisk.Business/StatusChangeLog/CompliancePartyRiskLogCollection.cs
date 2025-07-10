using CargoWise.EntityFramework;

namespace Enterprise.ComplianceRisk.Business
{
	public class CompliancePartyRiskLogCollection : NonPersistentBusinessObjectCollection<CompliancePartyRiskLog>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

		protected override bool AllowNewCore => false;
	}
}
