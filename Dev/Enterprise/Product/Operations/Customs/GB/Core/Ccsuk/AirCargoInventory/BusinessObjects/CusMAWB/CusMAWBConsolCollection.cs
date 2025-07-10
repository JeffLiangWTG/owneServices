using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusMAWBConsolCollection : BusinessObjectCollection<CusMAWB>, Integration.Customs.GB.CCSUK.ICusMAWBCollection
	{
		public CusMAWBConsolCollection(BusinessObjectFactory factory, ZGuid parentPK)
			: base(factory)
		{
			this.parentPK = parentPK;
		}

		Integration.Customs.GB.CCSUK.ICusMAWB Integration.Customs.GB.CCSUK.ICusMAWBCollection.this[int index] => base[index];

		Integration.Customs.GB.CCSUK.ICusMAWB IBusinessObjectCollection<Integration.Customs.GB.CCSUK.ICusMAWB>.this[int index] => base[index];

		public IEnumerator<Integration.Customs.GB.CCSUK.ICusMAWB> GetEnumerator()
		{
			foreach (var item in (IEnumerable<BusinessObject>)this)
			{
				yield return (Integration.Customs.GB.CCSUK.ICusMAWB)item;
			}
		}

		Integration.Customs.GB.CCSUK.ICusMAWB IBusinessObjectCollection<Integration.Customs.GB.CCSUK.ICusMAWB>.AddNew()
		{
			return base.AddNew();
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = base.CreateAdditionalFilter();
			query.AddToFilter(CusMAWBSchema.CM_JK, parentPK);
			query.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			return query;
		}

		readonly ZGuid parentPK;
	}
}
