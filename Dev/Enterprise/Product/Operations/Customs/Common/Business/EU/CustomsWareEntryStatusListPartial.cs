using CargoWise.Types;

namespace Enterprise.Customs.Common.EU
{
	public partial class CustomsWareEntryStatusList
	{
		public static ZString GetCodeFromCustomsWareStatusText(ZString statusText)
		{
			switch (statusText.ToUpper())
			{
				case "ACCEPTED":
					return Codes.Accepted;
				case "ACKNOWLEDGED":
					return Codes.Acknowledged;
				case "AMENDMENTREQUESTED":
					return Codes.AmendmentRequested;
				case "CANCELLED":
					return Codes.Cancelled;
				case "CLEARED":
					return Codes.Cleared;
				case "CONTROL":
					return Codes.Control;
				case "CORRECTED":
					return Codes.Corrected;
				case "CREATED":
					return Codes.Created;
				case "ERROR":
					return Codes.Error;
				case "EXPORTED":
					return Codes.Exported;
				case "FALLBACK":
					return Codes.Fallback;
				case "INFO":
					return Codes.Info;
				case "INVALID":
					return Codes.Invalid;
				case "PENDING":
					return Codes.Pending;
				case "PRELODGED":
					return Codes.Prelodged;
				case "QUEUED":
					return Codes.Queued;
				case "REFUSED":
					return Codes.Refused;
				case "REJECTED":
					return Codes.Rejected;
				case "RELEASED":
					return Codes.Released;
				case "REQUESTED":
					return Codes.Requested;
				case "SAVED":
					return Codes.Saved;
				case "SUBMITTED":
					return Codes.Submitted;
				case "UPDATED":
					return Codes.Updated;
				case "VALID":
					return Codes.Valid;
				case "SENT":
					return Codes.Sent;
				default:
					return "";
			}
		}
	}
}
