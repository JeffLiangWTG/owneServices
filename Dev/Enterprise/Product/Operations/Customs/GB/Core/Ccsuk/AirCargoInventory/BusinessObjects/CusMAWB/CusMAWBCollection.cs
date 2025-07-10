using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusMAWBCollection : BusinessObjectCollection<CusMAWB>
	{
		public CusMAWBCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = base.CreateAdditionalFilter();
			query.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			return query;
		}
	}
}
