using System.Collections.Generic;

namespace Enterprise.Customs.Common.US
{
	public interface IStatusList
	{
		string GetDescriptionFromCode(string code);
		bool IsStatusClear(string status);
		bool IsWaitingForResponse(string status);
		bool IsWithdrawnStatus(string status);

		/// <summary>
		/// for inbond, an entry can be partially cleared if there are multiple bills of lading and a subset of them have a clearance
		/// </summary>
		bool IsPartialStatus(string status);

		bool IsArrivalExportBTATransmissionStatus(string status);

		IReadOnlyList<string> GetFirstClearStatusFor(ImportMessageStatusList.MessageType messageType);

		IReadOnlyList<string> RejectStatusInterested { get; }
		/// <summary>
		/// This list is used to cancel a reject status logged using the 'RejectStatusInterested' list.
		/// The following list is needed as system should not cancel EntrySummaryReplaceReject status because Entry Date Update is accepted.
		/// </summary>
		IReadOnlyList<string> AcceptedStatusToCancelRejectStatusInterested { get; }
	}
}
