using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public partial class CusMAWB
	{
		public new class Loader : Customs.Business.CusMAWB.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			// This can go away when CcsukConsolPlugin is entirely replaced by the new one
			public CusMAWB LoadFromConsolPkOrMasterNumber(ForwardingConsol consol)
			{
				var result = Load(consol);
				if (result == null && !consol.JK_MasterBillNum.IsEmpty)
				{
					result = FindFromMawbNumber(consol.JK_MasterBillNum.KeepAlphanumericCharacters(), "");
				}
				return result;
			}

			// This can go away when CcsukConsolPlugin is entirely replaced by the new one, and the corresponding shipment plugins are replaced too
			public CusMAWB Load(ForwardingConsol consol)
			{
				CusMAWB result = null;
				Customs.Business.CusMAWB[] mAWBs = FindMatchingMAWBs(consol.PK, false);
				if (mAWBs.Length == 0)
				{
					mAWBs = FindMatchingMAWBs(consol.PK, true);
				}
				if (mAWBs.Length == 1)
				{
					var mawbs = (from Customs.Business.CusMAWB m in mAWBs where m.CM_ApplicationCode == ApplicationCodeList.Codes.GbCcsuk select m).ToArray();
					if (mawbs.Any())
					{
						result = (CusMAWB)mawbs[0];
					}
				}
				else if (mAWBs.Length > 1)
				{
					var cusMAWBsDetails = new ZStringBuilder();
					foreach (var mawb in mAWBs)
					{
						cusMAWBsDetails.Append(string.Format("{0} ({1})", mawb.GetType().FullName, mawb.PK));
					}
					ErrorReporter.ReportOnce("BGB-CCSUK-CusMAWB - Did not expect so many records back for this CusMAWB and Consol", string.Format("Did not expect {1} records of CusMAWB for Consol ({0}):\r\n{2}", consol.PK, mAWBs.Length, cusMAWBsDetails.ToStringWithNewLineBetweenAppends())); // Column name used in error message, not key
				}
				return result;
			}

			public CusMAWB FindFromMawbNumber(ZString mawbNumber, ZString splitReference, ZString airportAndShed)
			{
				return FindFromMawbNumber(mawbNumber, 12, splitReference, airportAndShed).FirstOrDefault();
			}

			public CusMAWB FindFromMawbNumber(ZString mawbNumber, ZString airportAndShed)
			{
				return FindFromMawbNumber(mawbNumber, 12, "", airportAndShed).FirstOrDefault();
			}

			public CusMAWB FindFromMawbNumber(ZString mawbNumber)
			{
				return FindMAWBsFromMawbNumber(mawbNumber).FirstOrDefault();
			}

			public CusMAWB[] FindMAWBsFromMawbNumber(ZString mawbNumber)
			{
				return FindFromMawbNumber(mawbNumber, 12, "", "");
			}

			public CusMAWB FindFromMawbNumberAndAgentBadge(ZString mawbNumber, ZString agentBadge)
			{
				return FindFromMawbNumber(mawbNumber, 12, "", "", agentBadge).FirstOrDefault();
			}

			public CusMAWB[] FindFromMawbNumberWithoutConsol(ZString mawbNumber, ZString airportAndShed)
			{
				return (from CusMAWB m in FindFromMawbNumber(mawbNumber, 12, "", airportAndShed) where m.CM_JK.IsEmpty select m).ToArray();
			}

			public CusMAWB[] FindFromMawbNumber(ZString mawbNumber, int monthsTimeLimitForArrivalDate, ZString split, ZString airportAndShed, string agentBadge = "")
			{
				var allFoundMawbsAtAllLocations = FindMatchingMAWBs(mawbNumber);
				if (allFoundMawbsAtAllLocations != null)
				{
					var allBritishFOundMawbsWithinDateRange = (from CusMAWB mawb in allFoundMawbsAtAllLocations
															   where
															   (mawb.CM_ArrivalDate.IsEmpty || mawb.CM_ArrivalDate > ZDateTime.Now.AddMonths(-1 * Math.Abs(monthsTimeLimitForArrivalDate)))
															   && mawb.CM_ApplicationCode == ApplicationCodeList.Codes.GbCcsuk
															   && mawb.CM_IsActive
															   && (split.IsEmpty || (mawb.HasSplits && mawb.Splits[split] != null))
															   select mawb);
					if (allBritishFOundMawbsWithinDateRange.Any() && !airportAndShed.IsEmpty)
					{
						allBritishFOundMawbsWithinDateRange = allBritishFOundMawbsWithinDateRange.Where(mawb => mawb.CargoTerminalOperatorAirport + mawb.CargoTerminalOperator == airportAndShed);
					}
					if (allBritishFOundMawbsWithinDateRange.Any() && !String.IsNullOrEmpty(agentBadge))
					{
						allBritishFOundMawbsWithinDateRange = allBritishFOundMawbsWithinDateRange.Where(mawb => mawb.AgentBadge == agentBadge);
					}
					return allBritishFOundMawbsWithinDateRange.ToArray();
				}
				return null;
			}

			protected override ZString[] GetApplicationCodes()
			{
				return new ZString[] { ApplicationCodeList.Codes.GbCcsuk };
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusMAWB);
			}
		}
	}
}
