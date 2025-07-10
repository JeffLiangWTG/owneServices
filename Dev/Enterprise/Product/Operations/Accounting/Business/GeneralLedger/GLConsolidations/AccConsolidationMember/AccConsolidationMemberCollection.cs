using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public class AccConsolidationMemberCollection : ActiveBusinessObjectCollection<AccConsolidationMember>
	{
		public AccConsolidationMemberCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AccConsolidationMemberCollection(BusinessObjectFactory factory, ICollectionRelationship relationship) : base(factory, relationship)
		{
		}
	}
}