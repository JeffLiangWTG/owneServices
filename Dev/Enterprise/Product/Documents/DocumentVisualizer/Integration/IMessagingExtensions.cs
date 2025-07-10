using System.Collections.Generic;
using CargoWise.Integration;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IMessagingExtensions
	{
		// original
		bool? ContinueWithSendingMessage(IUserNotifications notifications);

		// amendment
		bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications);
		bool? GetRequireMessageAmendmentReason();
		bool? IsSendingAmendment();
		ICodeDescriptionPairList GetAmendmentOptions();

		// withdrawal (cancellation)
		bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications);
		ICodeDescriptionPairList GetWithdrawalOptions();

		// reset to original
		bool? ContinueWithResetToOriginal(IUserNotifications notifications);

		// xml
		string GetXmlNamespace();
		string GetDocumentaryOverrideDocumentName();

		// other
		string GetMessageStatus();
		KeyValuePair<string, string>[] GetAdditionalParametersForEvent();

		// document view
		bool? ShowEvents();
		bool? ShowLastEventDetails();
	}
}
