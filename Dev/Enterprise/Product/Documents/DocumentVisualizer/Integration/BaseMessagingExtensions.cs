using System.Collections.Generic;
using CargoWise.Integration;

namespace Enterprise.DocumentVisualizer.Integration
{
	public abstract class BaseMessagingExtensions : IMessagingExtensions
	{
		public virtual string MessagePurposeCodeOverride => null;
		public virtual bool? ContinueWithSendingMessage(IUserNotifications notifications) => null;
		public virtual bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications) => null;
		public virtual bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications) => null;
		public virtual bool? ContinueWithResetToOriginal(IUserNotifications notifications) => null;
		public virtual bool? GetRequireMessageAmendmentReason() => null;
		public virtual bool? IsSendingAmendment() => null;
		public virtual string GetXmlNamespace() => null;
		public virtual string GetDocumentaryOverrideDocumentName() => null;
		public virtual string GetMessageStatus() => null;
		public virtual ICodeDescriptionPairList GetAmendmentOptions() => null;
		public virtual ICodeDescriptionPairList GetWithdrawalOptions() => null;
		public virtual KeyValuePair<string, string>[] GetAdditionalParametersForEvent() => null;
		public virtual bool? ShowEvents() => null;
		public virtual bool? ShowLastEventDetails() => null;
	}
}
