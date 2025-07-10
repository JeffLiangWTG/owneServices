using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WhsItemReceiveTransportationUnitWrapperCollection : GenericWrapperCollection<WhsItemReceiveTransportationUnitWrapper>
	{
		public WhsItemReceiveTransportationUnitWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsItemReceiveTransportationUnitWrapperCollection(IEnumerable<ZGuid> rtuPKs, IEnumerable<ZGuid> asnPKs, BusinessObjectFactory factory)
			: base(factory)
		{
			AddRTUsFrom(rtuPKs, asnPKs);
		}

		void AddRTUsFrom(IEnumerable<ZGuid> rtuPKs, IEnumerable<ZGuid> asnPKs)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemReceiveTransportationUnit));
			query.AddToFilter(WhsItemReceiveTransportationUnitSchema.PK, rtuPKs);

			var subQuery = new ZDBOnlySubQuery(typeof(WhsItemReceiveASNRTUPivot), WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit);
			subQuery.AddToFilter(new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRP_TransitReceiveASN, asnPKs));
			query.AddSubQuery(subQuery, JoinCondition.Or);

			var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(query);

			foreach (var rtu in rtus)
			{
				Add(new WhsItemReceiveTransportationUnitWrapper(rtu, Factory));
			}
		}
	}
}
