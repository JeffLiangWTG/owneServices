using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Security;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class MessageSendingValidation : Customs.Business.MessageSendingValidation
	{
		public static MessageSendingValidation New(
			BusinessObject topLevelBusinessObjectForValidation,
			IEnumerable<INotification> messageErrors,
			IEnumerable<INotification> warnings,
			SecurityCheckpoint sendMessageWithErrorsSecurityCheckpoint,
			bool refreshValidation = true)
				=> new MessageSendingValidation(topLevelBusinessObjectForValidation, messageErrors, warnings, sendMessageWithErrorsSecurityCheckpoint, refreshValidation);

		MessageSendingValidation(
			BusinessObject topLevelBusinessObjectForValidation,
			IEnumerable<INotification> messageErrors,
			IEnumerable<INotification> warnings,
			SecurityCheckpoint sendMessageWithErrorsSecurityCheckpoint,
			bool refreshValidation = true)
				: base(topLevelBusinessObjectForValidation, messageErrors, sendMessageWithErrorsSecurityCheckpoint, refreshValidation)
		{
			this.warnings = warnings;
		}
		readonly IEnumerable<INotification> warnings;

		public MessageSendingNotificationCollection CheckBusinessObjectLevelWarning()
		{
			var result = new MessageSendingNotificationCollection();
			if (TopLevelBusinessObjectForValidation != null && TopLevelBusinessObjectForValidation.HasWarnings)
			{
				var warningsAsString = warnings.ToUniqueMessageListString();
				if (warningsAsString.Length > 0)
				{
					var messageText = Res.GetString("0EB07E32-1592-4968-A7A3-32B02FC129A7", "Your message(s) have the following warnings:") + "\r\n\r\n" + warningsAsString;
					result.AddWarning(messageText);
				}
			}
			return result;
		}
	}
}
