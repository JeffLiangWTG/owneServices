using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public static class NumberOfPiecesReleasedHelper
	{
		public static int GetTotalPiecesReleased(ICcsukCusAwb awb, Event eventCode)
		{
			return (from StmALog log in GetReleaseDatesAndCounts(awb, eventCode) select int.Parse(log.SL_Reference)).Sum();
		}

		public static void ReleaseMorePieces(ICcsukCusAwb awb, ZInt additionalPieces, Event eventType)
		{
			var logs = ((BusinessObject)awb).GetLogs();
			// The type of release to effect shoudl come from the caller (who has knowledge of the message in question), not assumed from the current profile
			logs.AddNew(eventType, additionalPieces.ToString(), ZDateTimeOffset.Now);
		}

		public static int LastPiecesReleased(ICcsukCusAwb awb, Event eventCode)
		{
			var result = 0;
			var mostRecentLog = GetReleaseDatesAndCounts(awb, eventCode).FirstOrDefault();
			if (mostRecentLog != null)
			{
				var parseOK = int.TryParse(mostRecentLog.SL_Reference, out result);
				if (!parseOK)
				{
					result = 0;
				}
			}
			return result;
		}

		public static IEnumerable<StmALog> GetReleaseDatesAndCounts(ICcsukCusAwb awb, Event eventCode)
		{
			if (awb != null)
			{
				BusinessObject bizObjectForLogs;
				GetBizFromAwb(awb, out bizObjectForLogs);
				if (bizObjectForLogs != null)
				{
					var logs = bizObjectForLogs.GetLogs();
					if (logs != null)
					{
						var allLogs = logs.GetAllLogs();
						if (allLogs != null)
						{
							return (from StmALog log in allLogs
									where log.SL_SE_NKEvent == eventCode.Code && !log.SL_IsCancelled && log.SL_Reference.IsNumbersOnlyOrEmpty  // IsCancelled is set when customs delete a job's movement via FSN/CX, even after pieces are released, and we must wipe our knowledge of them. 
									orderby log.SL_EventTime descending
									select log);
						}
					}
				}
			}
			return System.Array.Empty<StmALog>();
		}

		public static ZString ReleaseDatesAndCountsFormatted(ICcsukCusAwb awb, Event eventCode)
		{
			var sb = new ZStringBuilder();
			foreach (var log in GetReleaseDatesAndCounts(awb, eventCode))
			{
				sb.Append(string.Format(CultureInfo.InvariantCulture, "{0} piece(s) released for {1} by {2} at {3}", log.SL_Reference, awb.Profile, log.SL_GS_NKUser, log.SL_EventTime.ToString("yyyy-MM-dd hh:mm", CultureInfo.InvariantCulture)));
			}
			return sb.ToStringWithNewLineBetweenAppends();
		}

		static void GetBizFromAwb(ICcsukCusAwb awb, out BusinessObject bizObjectForLogs)
		{
			bizObjectForLogs = (BusinessObject)awb;
			if (awb is CusMAWB)
			{
				var mawb = awb as CusMAWB;
				bizObjectForLogs = mawb.MasterLevelHouseHelper;
			}
		}

		internal static void ResetAllReleaseCountsToZeroUponCxStatusUpdate(ICcsukCusAwb awb)
		{
			var bizO = awb as EnterpriseBusinessObject;
			if (bizO != null)
			{
				bizO.Logs.AddNew(Events.CustomsEntryStatus
									, string.Format("All release counts reset for CX update. Old values: agent C1={0}, shed RRA={1}. Include cancelled logs to see old values.", GetTotalPiecesReleased(awb, AgentC1Event), GetTotalPiecesReleased(awb, ShedEvent))
									, ZDateTimeOffset.Now);
			}

			var allLogs = new List<StmALog>();
			allLogs.AddRange(GetReleaseDatesAndCounts(awb, ShedEvent));
			allLogs.AddRange(GetReleaseDatesAndCounts(awb, AgentC1Event));
			foreach (var log in allLogs)
			{
				log.Cancel();
			}
		}

		public static Event ShedEvent => Events.DeliveryOrderReceived;
		public static Event AgentC1Event => Events.Released;
	}
}
