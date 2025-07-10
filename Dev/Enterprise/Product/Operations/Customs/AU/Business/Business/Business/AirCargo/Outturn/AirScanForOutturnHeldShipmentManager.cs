using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirScanForOutturnHeldShipmentManager : ScanForOutturnHeldShipmentManager
	{
		public AirScanForOutturnHeldShipmentManager(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override IEnumerable<IScanHouseBillProvider> GetHouseBills()
		{
			var hawbQuery = new ZDBOnlyQuery(typeof(CusHAWBBase));
			hawbQuery.AddToFilter(CusHAWBSchema.CS_IsHeldAtOutturn, true);
			if (!OutturningPremiseID.IsEmpty)
			{
				var cusUnderbonQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
				cusUnderbonQuery.AddToFilter(CusUnderbondSchema.C4_DestinationPremiseID, OutturningPremiseID);
				cusUnderbonQuery.MaximumRows = 1;
				hawbQuery.AddSubQuery(CusHAWBSchema.CS_CM, cusUnderbonQuery, JoinCondition.And);
			}
			return CusHAWBBase.LoadAllFromQuery(hawbQuery, Factory).OfType<CusHAWB>();
		}

		public override OutturnLineCollection GetNewOutturnLineCollection()
		{
			return new AirOutturnLineCollection(Factory);
		}

		protected override OutturnLine GetNewOutturnLine()
		{
			return new AirOutturnLine();
		}

		protected override IScanHouseBillProvider GetMatchingOutturnedHouseBill(ZString houseBillNo)
		{
			CusHAWB result = null;
			if (!houseBillNo.IsEmpty)
			{
				var dbOnlyQuery = new ZDBOnlyQuery(typeof(CusHAWBBase));
				dbOnlyQuery.AddToFilter(CusHAWBSchema.CS_HAWB, houseBillNo);
				var cusHawbs = CusHAWBBase.LoadAllFromQuery(dbOnlyQuery, Factory, loadRecentOnly: true).OfType<CusHAWB>();
				var latestOutturned = ZDateTime.MinSmallDateTimeValue;
				foreach (var hawb in cusHawbs)
				{
					if (hawb.CargoReceivedAtDepotLogs.Count > 0 && hawb.CargoReceivedAtDepotLogs[0].SL_EventTime > latestOutturned)
					{
						result = hawb;
						latestOutturned = hawb.CargoReceivedAtDepotLogs[0].SL_EventTime;
					}
				}
			}
			return result;
		}

		protected override bool IsConditionalClearToBeConsideredAsClear
		{
			get { return AUCustomsDataRegistry.Instance.AirCargoOutturnScanningCONDCLEARReleaseStatus.Value == Core.Constants.AUCustoms.CondClearReleaseStatus.Clear; }
		}
	}
}
