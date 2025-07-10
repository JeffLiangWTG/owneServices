using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public partial class CusHAWB
	{
		public new class Loader
		{
			public Loader(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			public CusHAWB CreateNewOnMawbLinkedToShipment(CusMAWB masterBill, ForwardingShipment shipment)
			{
				var cusHAWB = masterBill.ChildBills.AddNew();
				cusHAWB.CS_JS = shipment.PK;
				cusHAWB.IsNonStandardHawbNumberSoDoNotSynch = false;
				cusHAWB.SynchroniseFromShipment(shipment);
				return cusHAWB;
			}

			internal CusHAWB LoadHawbFromShipmentByPkOrFromHousebillOrFromConsolPKOrFromMasterBill(ForwardingShipment shipment, ForwardingConsol consol)
			{
				var hawb = LoadHawbFromShipmentDbOnly(shipment);
				if (hawb == null)
				{
					var hawbsMatchingHouseNumberWithoutShipments = FindAllHawbsWithoutShipment(shipment.JS_HouseBill.KeepAlphanumericCharacters());
					if (hawbsMatchingHouseNumberWithoutShipments.Length == 1)
					{
						return hawbsMatchingHouseNumberWithoutShipments[0];
					}
					hawb = (from CusHAWB hb in hawbsMatchingHouseNumberWithoutShipments where consol != null && consol.PK == hb.MAWB.CM_JK select hb).FirstOrDefault();
					if (hawb == null)
					{
						hawb = (from CusHAWB hb in hawbsMatchingHouseNumberWithoutShipments where consol != null && consol.JK_MasterBillNum.KeepAlphanumericCharacters() == hb.MAWB.CM_MAWB select hb).FirstOrDefault();
					}
				}
				return hawb;
			}

			internal CusHAWB LoadHawbFromShipmentDbOnly(ForwardingShipment shipment)
			{
				var dbQuery = new ZDBOnlyQuery(typeof(CusHAWB));
				dbQuery.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, false);
				dbQuery.AddToFilter(CusHAWBSchema.CS_JS, shipment.PK);
				dbQuery.AddToFilter(CusHAWBSchema.CS_IsActive, true);
				foreach (var baseCusHAWB in shipment.Factory.Load<Customs.Business.CusHAWB>(dbQuery))
				{
					var cusMAWBQuery = new ZQuery(CusMAWBSchema.PK, baseCusHAWB.CS_CM);
					cusMAWBQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
					cusMAWBQuery.AddToFilter(CusMAWBSchema.CM_IsActive, true);
					if (shipment.Factory.LoadTop1<Customs.Business.CusMAWB>(cusMAWBQuery) != null)
					{
						return shipment.Factory.Load<CusHAWB>(baseCusHAWB.PK);
					}
				}
				return null;
			}

			internal CusHAWB[] FindAllHawbsWithoutShipment(ZString hawbNo)
			{
				var results = FindAllHawbs(hawbNo);
				return (from CusHAWB h in results where h.CS_JS == ZGuid.Empty select h).ToArray();
			}

			public CusHAWB FindHawb(ZString hawbNo, ZString splitReference, ZString mawbNo, string airportAndShed = "")
			{
				var results = FindAllHawbs(hawbNo);
				return (from CusHAWB h in results
						where
						(splitReference.IsEmpty || h.HasSplits && h.Splits[splitReference] != null)
						&&
						(mawbNo.IsEmpty || h.MAWB.CM_MAWB == mawbNo.KeepAlphanumericCharacters())
						&&
						(string.IsNullOrEmpty(airportAndShed) || h.CS_WarehouseLocation == airportAndShed)
						select h).FirstOrDefault();
			}

			public CusHAWB[] FindAllHawbs(ZString hawbNo, bool exactMatchOnHawbNumber = true)
			{
				var query = new ZDBOnlyQuery(typeof(CusHAWB));
				var sqlOperator = exactMatchOnHawbNumber ? SQLComparisonOperator.Equal : SQLComparisonOperator.Contains;
				query.AddToFilter(CusHAWBSchema.CS_HAWB, sqlOperator, hawbNo);
				query.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, false);
				query.AddToFilter(CusHAWBSchema.CS_IsActive, true);
				var sub = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
				sub.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
				sub.AddToFilter(CusMAWBSchema.CM_IsActive, true);
				query.AddSubQuery(sub, JoinCondition.And);
				var hawbs = factory.Load<CusHAWB>(query);
				return hawbs;
			}

			public CusHAWB FindHawbOnShipment(ForwardingShipment shipment)
			{
				var query = new ZDBOnlyQuery(typeof(CusHAWB));
				query.AddToFilter(CusHAWBSchema.CS_JS, shipment.PK);
				query.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, false);
				query.AddToFilter(CusHAWBSchema.CS_IsActive, true);
				var sub = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
				sub.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
				sub.AddToFilter(CusMAWBSchema.CM_IsActive, true);
				query.AddSubQuery(sub, JoinCondition.And);
				return shipment.Factory.LoadTop1<CusHAWB>(query);
			}

			public CusHAWB FindExistingHawbOnMawb(CusMAWB mawb, string houseNumber)
			{
				return (from CusHAWB house
							in mawb.ChildBills
						where house.CS_HAWB == houseNumber.ToUpper()
						select house)
							.FirstOrDefault();
			}

			readonly BusinessObjectFactory factory;
		}
	}
}
