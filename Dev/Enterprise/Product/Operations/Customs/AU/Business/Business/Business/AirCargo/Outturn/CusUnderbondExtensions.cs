using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	static class CusUnderbondExtensions
	{
		internal static OutturnStatus GetOutturnStatus(this CusUnderbond underbond)
		{
			switch (underbond.OutturnStatus.Code)
			{
				case "":
				case CMRBaseStatuses.Codes.NotSent:
				case CMRBaseStatuses.Codes.WithdrawalAccepted:
				case CMRBaseStatuses.Codes.OriginalRejected:
					return OutturnStatus.ReadyForScanning;

				case CMRBaseStatuses.Codes.AwaitingResponseToAmendment:
				case CMRBaseStatuses.Codes.AwaitingResponseToOriginal:
				case CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal:
					return OutturnStatus.AwaitingResponseFromCustoms;

				default:
					return OutturnStatus.Sent;
			}
		}

		internal static UnderbondStatus GetUnderbondStatus(this CusUnderbond underbond)
		{
			var outturns = underbond.Outturns.Cast<CusOutturn>().ToArray();
			var outturnStatus = underbond.GetOutturnStatus();

			bool completedOnEarlierUnderbond = underbond.MAWB != null && underbond.MAWB.IsDCLOutturnComplete;

			if (outturns.Length == 0)
			{
				if (outturnStatus == OutturnStatus.AwaitingResponseFromCustoms)
				{
					return UnderbondStatus.WaitingCustomsResponse;
				}
				else
				{
					return completedOnEarlierUnderbond ? UnderbondStatus.CompletedOnEarlierUnderbond : UnderbondStatus.NotSend;
				}
			}
			else
			{
				if (outturns.Length == underbond.MAWB.ChildBills.Count)
				{
					if (outturnStatus == OutturnStatus.Sent)
					{
						return UnderbondStatus.FullySent;
					}
					else if (outturnStatus == OutturnStatus.AwaitingResponseFromCustoms)
					{
						return UnderbondStatus.WaitingCustomsResponse;
					}
					else
					{
						return completedOnEarlierUnderbond ? UnderbondStatus.CompletedOnEarlierUnderbond : UnderbondStatus.NotSend;
					}
				}
				else
				{
					if (outturnStatus == OutturnStatus.Sent)
					{
						return UnderbondStatus.PartiallySent;
					}
					else if (outturnStatus == OutturnStatus.AwaitingResponseFromCustoms)
					{
						return UnderbondStatus.WaitingCustomsResponse;
					}
					else
					{
						return completedOnEarlierUnderbond ? UnderbondStatus.CompletedOnEarlierUnderbond : UnderbondStatus.NotSend;
					}
				}
			}
		}

		internal static UnderbondStatus MergeUnderbondStatuses(List<UnderbondStatus> statusToMerge)
		{
			if (statusToMerge.Any(s => s == UnderbondStatus.WaitingCustomsResponse))
			{
				return UnderbondStatus.WaitingCustomsResponse;
			}

			if (statusToMerge.Where(s => s == UnderbondStatus.CompletedOnEarlierUnderbond || s == UnderbondStatus.NotSend).Count() == statusToMerge.Count)
			{
				return UnderbondStatus.NotSend;
			}

			if (statusToMerge.Where(s => s == UnderbondStatus.FullySent || s == UnderbondStatus.PartiallySent || s == UnderbondStatus.CompletedOnEarlierUnderbond).Count() == statusToMerge.Count)
			{
				return UnderbondStatus.FullySent;
			}

			return UnderbondStatus.PartiallySent;
		}

		internal static UnderbondStatus GetAccumulatedUnderbondStatus(this CusUnderbond underbond)
		{
			var mainUnderbondStatus = underbond.GetUnderbondStatus();
			var masterMAWB = underbond.MAWB;
			if (masterMAWB.IsStandAlone())
			{
				return mainUnderbondStatus == UnderbondStatus.PartiallySent ? UnderbondStatus.FullySent : mainUnderbondStatus;
			}

			var statuses = new List<UnderbondStatus>();
			statuses.Add(mainUnderbondStatus);

			foreach (IScanHouseBillProvider cusHAWB in underbond.MAWB.ChildBills)
			{
				if (cusHAWB.IsHVLVShipment())
				{
					var airCargo = masterMAWB.FindStandAloneAirCargo(cusHAWB.HouseBill);
					if (airCargo == null)
					{
						statuses.Add(UnderbondStatus.NotSend);
						continue;
					}
					var standAloneCargo = new ScanCusMAWB(airCargo);
					standAloneCargo.SelectedUnderbond = underbond;
					var standAloneUnderbond = standAloneCargo.GetUnderbond();
					if (standAloneUnderbond == null)
					{
						statuses.Add(UnderbondStatus.NotSend);
						continue;
					}

					statuses.Add(standAloneUnderbond.GetUnderbondStatus());
				}
			}

			return CusUnderbondExtensions.MergeUnderbondStatuses(statuses);
		}
	}
}
