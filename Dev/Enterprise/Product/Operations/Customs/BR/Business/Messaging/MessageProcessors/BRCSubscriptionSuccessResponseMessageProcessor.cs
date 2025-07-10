using System.Collections.Generic;
using System.Text.Json;
using CargoWise.Customs.BR.MessageDefinitions.CommonEvents.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCSubscriptionSuccessResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCSubscriptionSuccessResponseMessageProcessor(LoggingInformation logger)
				: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("5D9ED5CF-EDAF-4693-9AEB-FA4FBBA655B1", "Subscription Success Response");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.SUB };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Success };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is GlbExternalPassword_BRS externalPassword)
			{
				var outgoingMessage = BRMessageHelper.GetOutgoingMessage(message);

				if (outgoingMessage != null && outgoingMessage.EM_MessageSubType == EDIMessageSubTypeList.Codes.Cancel)
				{
					externalPassword.GP_MailBoxID = ZString.Empty;
					externalPassword.GP_StatusReason = GlbExternalPassword_BRS.StatusReasons.Canceled;
					externalPassword.GP_PasswordStatus = BRPasswordStatusList.Codes.Canceled;
				}
				else
				{
					var json = JsonDocument.Parse(message.EM_MessageText).RootElement;
					if (json.TryGetProperty(nameof(NotificationSubscription.id), out _) &&
							JsonSerializer.Deserialize<NotificationSubscription>(message.EM_MessageText) is
								NotificationSubscription notificationSubscription)
					{
						externalPassword.GP_MailBoxID = notificationSubscription.id.ToString();
						externalPassword.GP_StatusReason = GlbExternalPassword_BRS.StatusReasons.Subscribed;
						externalPassword.GP_PasswordStatus = BRPasswordStatusList.Codes.Accepted;
					}
					else
					{
						message.EM_Status = EDIMessage.Status.Failed;
						Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed.");
					}
				}
			}
		}
	}
}
