using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.Schema;
using Biz = Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Module
{
	class CcsukMasterAndHouseCombinedFilterStripBusinessObject : CcsukAirInventoryHouseFilterStripBusinessObject
	{
		public CcsukMasterAndHouseCombinedFilterStripBusinessObject()
			: this(true)
		{
		}

		public CcsukMasterAndHouseCombinedFilterStripBusinessObject(bool isCreateDefaultTextFilterStrips)
			: base(isCreateDefaultTextFilterStrips)
		{
		}

		protected override void EnforceNotMasterHouse(ZDBOnlyQuery query)
		{
		}

		protected override ZQuery GetHasSplitsQuery(ZBool value)
		{
			var houseQuery = GetHasSplitsQueryHouse(value);
			var masterQuery = GetHasSplitsQueryMaster(value);
			var overallQuery = new ZQuery();
			overallQuery.AddToFilter(houseQuery);
			overallQuery.AddToFilter(masterQuery, JoinCondition.Or);
			return overallQuery;
		}

		ZQuery GetHasSplitsQueryHouse(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(CusHAWB));
			var sub = new ZDBOnlySubQuery(typeof(Biz.CusPartShip), CusPartShipSchema.CG_CS, !value);
			sub.AddToFilter(CusPartShipSchema.CG_CS, SQLComparisonOperator.NotEqual, DBNull.Value);
			sub.AddToFilter(CusPartShipSchema.CG_CM_LinkToPartMaster, SQLComparisonOperator.Equal, DBNull.Value);
			query.AddSubQuery(CusHAWBSchema.PK, sub, JoinCondition.And);
			query.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, false);
			return query;
		}

		ZQuery GetHasSplitsQueryMaster(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(CusMAWB));
			var sub = new ZDBOnlySubQuery(typeof(Biz.CusPartShip), CusPartShipSchema.CG_CM_LinkToPartMaster, !value);
			sub.AddToFilter(CusPartShipSchema.CG_CS, SQLComparisonOperator.Equal, DBNull.Value);
			sub.AddToFilter(CusPartShipSchema.CG_CM_LinkToPartMaster, SQLComparisonOperator.NotEqual, DBNull.Value);
			query.AddSubQuery(CusHAWBSchema.CS_CM, sub, JoinCondition.And);
			query.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, true);
			return query;
		}
	}
}
