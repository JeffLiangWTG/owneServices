using System;
using System.Collections.Generic;

namespace Enterprise.Customs.Common.US
{
	partial class AESDirectCustomsEntryStatus : IStatusList
	{
		public bool IsStatusClear(string status)
		{
			return status == Codes.ReplacementSEDClear ||
				status == Codes.OriginalSEDClear ||
				status == Codes.Warning ||
				status == Codes.WarningCorrectRetransmit ||
				status == Codes.Verification ||
				status == Codes.Compliance ||
				status == Codes.DeleteSEDClear;
		}

		public bool IsWaitingForResponse(string status)
		{
			return status == Codes.AwaitingOriginalResponse ||
				status == Codes.AwaitingReplacementResponse ||
				status == Codes.AwaitingDeleteResponse;
		}

		public bool IsWithdrawnStatus(string status)
		{
			return status == Codes.DeleteSEDClear;
		}

		public bool IsPartialStatus(string status)
		{
			return false;
		}

		public bool IsArrivalExportBTATransmissionStatus(string status)
		{
			return false;
		}

		public IReadOnlyList<string> GetFirstClearStatusFor(ImportMessageStatusList.MessageType messageType)
		{
			if (messageType == ImportMessageStatusList.MessageType.Export)
			{
				return new string[] { Codes.OriginalSEDClear, Codes.ReplacementSEDClear, Codes.Warning, Codes.WarningCorrectRetransmit, Codes.Verification, Codes.Compliance };
			}
			else
			{
				throw new NotSupportedException(string.Format("Message type '{0}' is not supported in {1}", messageType, GetType().FullName));
			}
		}

		public IReadOnlyList<string> RejectStatusInterested
		{
			get { return Array.Empty<string>(); }
		}

		public IReadOnlyList<string> AcceptedStatusToCancelRejectStatusInterested
		{
			get { return Array.Empty<string>(); }
		}
	}
}
