using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Security;

namespace Enterprise.Customs.DE.Business
{
	public sealed class MessageSendingValidation : Customs.Business.MessageSendingValidation
	{
		public static MessageSendingValidation New(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors, IEnumerable<INotification> warnings, SecurityCheckpoint sendMessageWithErrorsSecurityCheckpoint, bool refreshValidation = true)
			=> new MessageSendingValidation(topLevelBusinessObjectForValidation, messageErrors, warnings, sendMessageWithErrorsSecurityCheckpoint, refreshValidation);

		MessageSendingValidation(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors, IEnumerable<INotification> warnings, SecurityCheckpoint sendMessageWithErrorsSecurityCheckpoint, bool refreshValidation = true) : base(topLevelBusinessObjectForValidation, messageErrors, sendMessageWithErrorsSecurityCheckpoint, refreshValidation)
		{
			this.warnings = warnings;
		}
		readonly IEnumerable<INotification> warnings;

		public MessageSendingNotificationCollection CheckBusinessObjectLevelWarning()
		{
			var result = new MessageSendingNotificationCollection();
			if (TopLevelBusinessObjectForValidation != null)
			{
				if (refreshValidation)
				{
					var declaration = TopLevelBusinessObjectForValidation as JobDeclaration;
					using (declaration?.UnRegisterEditableChildObjectsForMessageValidation())
					{
						TopLevelBusinessObjectForValidation.LoadChildEditableObjects();
						TopLevelBusinessObjectForValidation.RunPreSaveValidation();
					}
				}
				if (TopLevelBusinessObjectForValidation.HasWarnings)
				{
					var warningCollector = warnings;
					var warningsAsString = warningCollector.ToUniqueMessageListString();
					if (warningsAsString.Length > 0)
					{
						var messageText = Res.GetString("9F2E50E2-6773-46B1-B511-88A2DCF55C85", "Your message(s) have the following warnings:") + "\r\n\r\n" + warningsAsString;
						result.AddWarning(messageText);
					}
				}
			}
			return result;
		}
	}
}
