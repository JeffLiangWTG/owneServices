using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Au = Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.Wow
{
	internal class WowMessageSendingValidation : MessageSendingValidation
	{
		internal WowMessageSendingValidation(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrorNotificationCollector)
			: base(topLevelBusinessObjectForValidation, messageErrorNotificationCollector)
		{
		}

		internal static WowMessageSendingValidation New(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrorNotificationCollector)
		{
			return new WowMessageSendingValidation(topLevelBusinessObjectForValidation, messageErrorNotificationCollector);
		}

		internal static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		protected override MessageSendingNotificationCollection CheckBusinessObjectLevelValidationCore()
		{
			MessageSendingNotificationCollection result = new MessageSendingNotificationCollection();
			JobDeclaration declaration = TopLevelBusinessObjectForValidation as JobDeclaration;
			if (declaration != null && IsTAndIZero(declaration.CustomsEntryHeaders))
			{
				string messageText = MessageNoTNIEntered + "\r\n\r\nDo you want to send the message(s) despite these errors?";
				result.AddWarning(messageText);
			}
			return result;
		}

		protected override bool CheckBusinessObjectLevelValidationCore(Enterprise.Customs.Business.ISendsMessagesToCustoms notifier, bool isSendingInTestMode)
		{
			MessageSendingNotificationCollection notifications = CheckBusinessObjectLevelValidationCore();

			bool result = true;
			if (notifications.ContainsWarning())
			{
				result = notifier.AskUserToContinueWithAction(notifications.NotificationsAsString(), "Continue to Send", TopLevelBusinessObjectForValidation);
			}

			if (result)
			{
				notifications = base.CheckBusinessObjectLevelValidationCore();
				if (notifications.ContainsError())
				{
					result = false;
					notifier.NotifyUserOfAnInvalidOperation(notifications.NotificationsAsString());
				}
				else if (notifications.ContainsWarning())
				{
					result = notifier.AskUserToContinueWithAction(notifications.NotificationsAsString(), "Continue to Send", TopLevelBusinessObjectForValidation);
				}
			}
			return result;
		}

		static bool IsTAndIZero(ICusEntryHeaderCollection<Au.CusEntryHeader> entryHeaders)
		{
			bool result = false;
			foreach (var entryHeader in entryHeaders)
			{
				if (entryHeader.TransportAndInsurance.Amount == ZDecimal.Zero)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		internal const string MessageNoTNIEntered = "It is likely that your message(s) will be rejected by Customs, as you have not included OFT and/or ONS amount.";
	}
}
