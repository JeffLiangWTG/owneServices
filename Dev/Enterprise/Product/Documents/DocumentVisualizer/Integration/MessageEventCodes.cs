using System.Collections.Generic;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Integration
{
	public static class MessageEventCodes
	{
		public static IEnumerable<string> All
		{
			get
			{
				yield return Events.DataExportCode;
				yield return Events.StatusUpdatedCode;

				foreach (var code in SentMessagesEventCodes)
				{
					yield return code;
				}

				yield return Events.MessagePendingProcessingCode;

				foreach (var code in MessageStatusConfirmationEventCodes)
				{
					yield return code;
				}
			}
		}

		public static IEnumerable<string> MessageStatusEventCodes
		{
			get
			{
				foreach (var code in SentMessagesEventCodes)
				{
					yield return code;
				}

				yield return Events.MessageReceivedCode;
				yield return Events.MessagePendingProcessingCode;

				yield return Events.StatusUpdatedCode;
				yield return Events.InterchangeReceiptAcknowledgedCode;

				yield return Events.InterchangeSentCode;

				foreach (var code in MessageStatusConfirmationEventCodes)
				{
					yield return code;
				}
			}
		}

		public static IEnumerable<string> SentMessagesEventCodes
		{
			get
			{
				yield return Events.MessageSentCode;
				yield return Events.MessageWithdrawCancelRequestCode;
			}
		}

		public static IEnumerable<string> MessageStatusConfirmationEventCodes
		{
			get
			{
				foreach (var code in AcceptanceEventCodes)
				{
					yield return code;
				}

				foreach (var code in RejectionEventCodes)
				{
					yield return code;
				}
			}
		}

		public static IEnumerable<string> AcceptanceEventCodes
		{
			get
			{
				yield return Events.MessageAcceptedCode;
				yield return Events.AuthorisedCode;
				yield return Events.MessageWithdrawCancelAcceptedCode;
				yield return Events.InterchangeReceiptAcknowledgedCode;
			}
		}

		public static IEnumerable<string> RejectionEventCodes
		{
			get
			{
				yield return Events.MessageRejectedCode;
				yield return Events.AuthorisationRejectedCode;
				yield return Events.InterchangeRejectedCode;
			}
		}
	}
}
