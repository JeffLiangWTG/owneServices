using System.Collections.Generic;
using CargoWise.Customs.BR.MessageDefinitions.Subscription.Incoming;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class BRCSubscriptionErrorResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCSubscriptionErrorResponseMessageProcessor(LoggingInformation logger)
				: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("CE137FBE-A38D-4014-9C37-B0F5C4997F60", "Subscription Error Response");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.SUB };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Error };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			var outgoingMessage = BRMessageHelper.GetOutgoingMessage(message);

			ProcessMessage(message, outgoingMessage);

			var subscriptionError = BRMessageHelper.DeserializeObject<ErrorNotification>(message.EM_MessageText);
			message.EM_MessageInterpretation = new SubscriptionErrorMessagePrettyFormatter(subscriptionError).GetFormattedMessageText();

			if (message.EM_LinkedObject is GlbExternalPassword_BRS externalPassword && externalPassword.Staff is GlbStaff staff)
			{
				var subject = $"Customs error response has been received for subscription {externalPassword.GP_UserID} for staff {staff.GS_FullName}";
				var heading = $"Subscription {externalPassword.GP_UserID}" + (NoResString)" failed";
				SendErrorNotification(message, outgoingMessage, subject, heading);
			}
		}

		public static void ProcessMessage(EDIMessage incomingMessage, EDIMessage outgoingMessage)
		{
			if (incomingMessage.EM_LinkedObject is GlbExternalPassword_BRS externalPassword)
			{
				if (outgoingMessage != null && outgoingMessage.EM_MessageSubType == EDIMessageSubTypeList.Codes.Cancel)
				{
					externalPassword.GP_PasswordStatus = BRPasswordStatusList.Codes.Rejected;
					externalPassword.GP_StatusReason = GlbExternalPassword_BRS.StatusReasons.CancelationRejected;
				}
				else
				{
					externalPassword.GP_PasswordStatus = BRPasswordStatusList.Codes.Rejected;
					externalPassword.GP_StatusReason = GlbExternalPassword_BRS.StatusReasons.SubscriptionRejected;
				}
			}
		}
	}
}
