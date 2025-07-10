using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusHAWBShipmentCollection : BusinessObjectCollection<CusHAWB>, Integration.Customs.GB.CCSUK.ICusHAWBCollection
	{
		public CusHAWBShipmentCollection(BusinessObjectFactory factory, ZGuid parentPK)
			: base(factory)
		{
			this.parentPK = parentPK;
		}

		Integration.Customs.GB.CCSUK.ICusHAWB Integration.Customs.GB.CCSUK.ICusHAWBCollection.this[int index] => base[index];

		Integration.Customs.GB.CCSUK.ICusHAWB IBusinessObjectCollection<Integration.Customs.GB.CCSUK.ICusHAWB>.this[int index] => base[index];

		public IEnumerator<Integration.Customs.GB.CCSUK.ICusHAWB> GetEnumerator()
		{
			foreach (var item in (IEnumerable<BusinessObject>)this)
			{
				yield return (Integration.Customs.GB.CCSUK.ICusHAWB)item;
			}
		}

		Integration.Customs.GB.CCSUK.ICusHAWB IBusinessObjectCollection<Integration.Customs.GB.CCSUK.ICusHAWB>.AddNew()
		{
			return base.AddNew();
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = base.CreateAdditionalFilter();
			query.AddToFilter(CusHAWBSchema.CS_JS, parentPK);
			query.AddToFilter(CusHAWBSchema.CS_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			query.AddToFilter(CusHAWBSchema.CS_CM, SQLComparisonOperator.NotEqual, DBNull.Value);
			query.OrderBy = CusHAWBSchema.Constants.CS_SystemCreateTimeUtc;
			return query;
		}

		readonly ZGuid parentPK;
	}
}
