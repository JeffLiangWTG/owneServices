using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class CusAWBExtensions
	{
		public static CusMAWB FindStandAloneAirCargo(this CusMAWB host, ZString houseBillNumber)
		{
			ZQuery findFilter = new ZQuery(CusMAWBSchema.CM_MAWB, host.CM_MAWB);
			findFilter.AddToFilter(CusMAWBSchema.CM_JK, DBNull.Value);
			findFilter.AddToFilter(CusMAWBSchema.CM_ApplicationCode, host.CM_ApplicationCode);
			findFilter.AddToFilter(CusMAWBSchema.PK, SQLComparisonOperator.NotEqual, host.PK);
			if (!houseBillNumber.IsEmpty)
			{
				findFilter.AddToFilter(CusMAWBSchema.CM_MasterHouseBill, houseBillNumber);
			}

			var mawbRecycleperiod = Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
			if (mawbRecycleperiod > 0)
			{
				var dateQuery = new ZQuery(CusMAWBSchema.CM_ArrivalDate, ZDateTime.Empty);
				dateQuery.AddToFilter(JoinCondition.Or, CusMAWBSchema.CM_ArrivalDate, SQLComparisonOperator.GreaterThan, ZDateTime.Now.AddMonths(-mawbRecycleperiod));
				findFilter.AddToFilter(dateQuery);
			}
			var standAloneCargos = host.Factory.Load<CusMAWB>(findFilter);
			if (standAloneCargos.Length > 1)
			{
				throw new InvalidOperationException(String.Format("There are more than one stand alone air cargo for house bill '{0}'", houseBillNumber));
			}

			if (standAloneCargos.Length == 0)
			{
				return null;
			}

			return standAloneCargos[0];
		}

		public static CusUnderbond[] GetUnderbonds(this CusMAWB host)
		{
			if (host.Underbonds == null)
			{
				return null;
			}

			var underbonds = host.Underbonds.Cast<CusUnderbond>().ToArray();
			if (underbonds.Length == 0)
			{
				return null;
			}

			return underbonds;
		}

		public static CusUnderbond GetMatchedToSelectedUnderbond(this CusMAWB host, CusUnderbond selectedUnderbond)
		{
			if (selectedUnderbond == null)
			{
				throw new InvalidOperationException("Underbond is null");
			}

			var underbonds = host.GetUnderbonds();
			if (underbonds == null)
			{
				return null;
			}

			var standAloneUnderbonds = underbonds.Where
				(
					u => u.C4_FlightNo == selectedUnderbond.C4_FlightNo
					&& u.C4_ArrivalDate == selectedUnderbond.C4_ArrivalDate
					&& u.C4_OriginPremiseID == selectedUnderbond.C4_OriginPremiseID
					&& u.C4_DestinationPremiseID == selectedUnderbond.C4_DestinationPremiseID
				).ToArray();

			if (standAloneUnderbonds.Length == 0)
			{
				return null;
			}

			if (standAloneUnderbonds.Length > 1)
			{
				var unprocessOutturnUnderbonds = standAloneUnderbonds.Where(u => u.OutturnStatus.Code == CMRBaseStatuses.Codes.NotSent || u.OutturnStatus.Code.IsEmpty).ToArray();
				if (unprocessOutturnUnderbonds.Length > 0)
				{
					return unprocessOutturnUnderbonds[0];
				}
			}
			return standAloneUnderbonds[0];
		}

		public static CusUnderbond[] GetSentUnderbonds(this CusMAWB host)
		{
			List<CusUnderbond> sentUndebonds = new List<CusUnderbond>();
			foreach (var underbond in host.GetUnderbonds())
			{
				if (underbond.GetOutturnStatus() == OutturnStatus.Sent)
				{
					sentUndebonds.Add(underbond);
				}
			}

			return sentUndebonds.ToArray();
		}

		public static bool IsStandAlone(this CusMAWB host)
		{
			return host.Consol == null;
		}
	}
}
