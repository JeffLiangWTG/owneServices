using CargoWise.EntityFramework;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceCommodityRiskLogCollection : NonPersistentBusinessObjectCollection<ComplianceCommodityRiskLog>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

		protected override bool AllowNewCore => false;
	}
}
