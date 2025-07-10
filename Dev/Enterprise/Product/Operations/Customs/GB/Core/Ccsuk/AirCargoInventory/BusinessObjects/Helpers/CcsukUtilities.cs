using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public static class CcsukUtilities
	{
		public static bool IsShipmentValidForCcsuk(ForwardingShipment shipment, ForwardingConsol consol)
		{
			return LicenceAndPimaHelper.AgentEnabled &&
			   shipment != null
			   && shipment.JS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.UnitedKingdom, StringComparison.OrdinalIgnoreCase)
			   && shipment.JS_TransportMode == Enterprise.Core.Constants.TransportModes.Air
			   && !shipment.IsDomestic()
			   && !shipment.JS_HouseBill.IsEmpty
			   && consol != null
			   && !consol.IsDirect
			   && !consol.JK_MasterBillNum.IsEmpty
			   && Env.Security.AirCcsukHouse.IsAllowed;
		}

		public static bool IsDirectShipmentValidForCcsuk(ForwardingShipment shipment, ForwardingConsol consol)
		{
			return LicenceAndPimaHelper.AgentEnabled &&
				shipment != null
				&& shipment.JS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.UnitedKingdom, StringComparison.OrdinalIgnoreCase)
				&& shipment.JS_TransportMode == Enterprise.Core.Constants.TransportModes.Air
				&& shipment.IsDirectShipment
				&& consol != null
				&& consol.IsDirect;
		}

		public static string GetEnumDescription(Enum value)
		{
			var fieldInfo = value.GetType().GetField(value.ToString());
			var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
			return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
		}

		public static ZString LeftPadWithZeros(ZString hawbNumber)
		{
			return !hawbNumber.IsEmpty && hawbNumber.Length < 8 ? hawbNumber.PadLeft(8, '0') : hawbNumber;
		}

		public static bool IsEuropeanShipmentDescriptionCode(ICcsukCusAwb awb)
		{
			var sdc = awb.ShipmentDescriptionCode;
			return sdc == ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport || sdc == ShipmentDescriptionCodes.Codes.CommunityStatusFromOutsideEC;
		}

		public static ZString ListHawbsOnMawb(CusMAWB mawb)
		{
			var result = ZString.Empty;
			foreach (CusHAWB brotherHawb in mawb.ChildBills)
			{
				result += string.Format("	{0}: pres.: {1}, {2}, {3}\r\n",
													brotherHawb.CS_HAWB,
													brotherHawb.PresenceOnNetworkStatus,
													brotherHawb.CustomsActionCode.IsEmpty ? "no CAC" : "status " + brotherHawb.CustomsActionCode,
													brotherHawb.Shipment == null ? new ZString("no shipment") : brotherHawb.Shipment.JS_UniqueConsignRef
													);
			}
			return result;
		}

		static bool PresenceOnNetworkIsTransientOrNegative(ZString pres)
		{
			return (pres == PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection || pres == PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck || pres == PresenceOnNetworkList.Codes.NotOnCommDb || pres == PresenceOnNetworkList.Codes.NotOnCommDbDeleted || pres == PresenceOnNetworkList.Codes.SendPendingCheckYourCukServiceTask || pres.IsEmpty);
		}

		internal static bool PresenceOnNetworkIsNotTerminal(ZString pres)
		{
			return (pres != PresenceOnNetworkList.Codes.ArchivedOnCcsuk && pres != PresenceOnNetworkList.Codes.CompletedOnCcsUk);
		}

		internal static void UpdatePresenceToPendingIfRelevant(ICcsukCusAwb awb)
		{
			var pres = awb.PresenceOnNetworkStatus;
			if (pres == PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck || pres == PresenceOnNetworkList.Codes.NotOnCommDb || pres == PresenceOnNetworkList.Codes.NotOnCommDbDeleted || pres == PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc)
			{
				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.SendPendingCheckYourCukServiceTask;
			}
		}

		internal static void UpdatePresenceToYesIfCurrentlyTransientOrNegative(ICcsukCusAwb awb)
		{
			var currentPresence = awb.PresenceOnNetworkStatus;
			if (PresenceOnNetworkIsTransientOrNegative(currentPresence))
			{
				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			}
		}

		internal static void UpdatePresenceToYesIfNotCurrentlyTerminal(ICcsukCusAwb awb)
		{
			if (PresenceOnNetworkIsNotTerminal(awb.PresenceOnNetworkStatus))
			{
				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			}
		}

		internal static void ArchiveParentIfLastChildIsNowArchived(ICcsukCusAwb child, ICcsukCusAwb parent, IEnumerable<ICcsukCusAwb> parentsChildren)
		{
			var hasBrotherStillNotArchived = false;
			var hasMoreThanZeroChildren = false;
			foreach (var brother in parentsChildren)
			{
				hasMoreThanZeroChildren = true;
				if (brother.PK != child.PK)
				{
					if (brother.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.ArchivedOnCcsuk)
					{
						hasBrotherStillNotArchived = true;
						break;
					}
				}
			}
			if (!hasBrotherStillNotArchived && hasMoreThanZeroChildren)
			{
				parent.ArchiveOnCcsuk(ReasonForArchiving.LastChildRecordWasArchived);
			}
		}

		internal static void CompleteParentIfLastChildIsNowComplete(ICcsukCusAwb child, ICcsukCusAwb parent, IEnumerable<ICcsukCusAwb> parentsChildren)
		{
			var hasBrotherStillNotCompletedOrArchived = false;
			foreach (var brother in parentsChildren)
			{
				if (brother.PK != child.PK)
				{
					if (brother.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.ArchivedOnCcsuk && brother.PresenceOnNetworkStatus != PresenceOnNetworkList.Codes.CompletedOnCcsUk)
					{
						hasBrotherStillNotCompletedOrArchived = true;
						break;
					}
				}
			}
			if (!hasBrotherStillNotCompletedOrArchived)
			{
				parent.CompleteOnCcsuk(true);
			}
		}

		public static SplitConsignment[] FindDuplicateSplits(this ICcsukCusAwb awb)
		{
			return awb.Splits.Where(s => s.CustomsActionCode.IsEmpty || s.CustomsActionCode == CustomsStatusCodes.Codes.EntryOrRequestCancelled)
				.GroupBy(s => s.SplitReference)
				.Where(splitGroup => splitGroup.Count() > 1)
				.Select(splitGroup => splitGroup.OrderBy(s => s.GetPresenceOnNetworkStatusRank()).First())
				.OrderBy(split => split.SplitReference)
				.ToArray();
		}

		public static bool HasDuplicateSplits(this ICcsukCusAwb awb) => awb.FindDuplicateSplits().Length > 0;

		internal static int GetPresenceOnNetworkStatusRank(this SplitConsignment split)
		{
			return (string)split.PresenceOnNetworkStatus switch
			{
				PresenceOnNetworkList.Codes.NotOnCommDb => 1,
				PresenceOnNetworkList.Codes.NotOnCommDbDeleted => 2,
				PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck => 3,
				PresenceOnNetworkList.Codes.IsrRequestCancelled => 4,
				PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc => 5,
				PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection => 6,
				PresenceOnNetworkList.Codes.SendPendingCheckYourCukServiceTask => 7,
				PresenceOnNetworkList.Codes.OnCommDb =>	8,
				PresenceOnNetworkList.Codes.ArchivedOnCcsuk => 9,
				PresenceOnNetworkList.Codes.CompletedOnCcsUk => 10,
				_ => 0
			};
		}

		public static void DeleteSplits(this ICcsukCusAwb awb, SplitConsignment[] splits)
		{
			foreach (var s in splits)
			{
				awb.Splits.RemoveAndDelete(s);
			}
		}
	}

	public static class CcsukConstants
	{
		public const int PimaMaxLength = 14;
	}
}
