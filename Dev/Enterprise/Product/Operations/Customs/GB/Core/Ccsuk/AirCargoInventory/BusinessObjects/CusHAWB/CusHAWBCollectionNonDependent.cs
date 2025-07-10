using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusHAWBCollectionNonDependent : BusinessObjectCollection<CusHAWB>
	{
		public CusHAWBCollectionNonDependent(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, false);
			return query;
		}
	}
}
