using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusHAWBCollectionNonDependentShowMastersToo : BusinessObjectCollection<CusHAWB>
	{
		public CusHAWBCollectionNonDependentShowMastersToo(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var q = base.CreateAdditionalFilter();
			q.AddToFilter(CusHAWBSchema.CS_IsActive, true);
			return q;
		}
	}
}
