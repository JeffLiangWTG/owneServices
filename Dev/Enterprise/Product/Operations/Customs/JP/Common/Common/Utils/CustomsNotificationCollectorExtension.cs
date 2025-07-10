using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public static class CustomsNotificationCollectorExtension
{
	public static ZString GenerateFormattedMessageErrors(this ZNotificationCollector collector)
	{
		var errors = FormatNotifications(collector.GetMessageErrors());
		return MessageErrorsExistHeaderText + "\r\n\r\n" + errors + "\r\n\r\n" + MessageErrorConfirmationQuestionText;
	}

	public static ZString GenerateFormattedErrors(this ZNotificationCollector collector)
	{
		var errors = FormatNotifications(collector.GetErrors());
		return ErrorExistHeaderText + "\r\n\r\n" + errors;
	}

	static string FormatNotifications(IEnumerable<INotification> notifications) => Regex.Replace(notifications.ToUniqueMessageListString(), "(?<!\r)\n", "\r\n");

	static string ErrorExistHeaderText => Res.GetString("E81CCF20-CA53-471A-B981-B4251CBD38EF", "Please fix these errors before sending any messages:");

	static string MessageErrorsExistHeaderText => Res.GetString("39016D4F-13B9-4C54-A6F1-191CC24A1E3F", "It is likely that your message(s) will be rejected by Customs, as they have the following message errors:");

	static string MessageErrorConfirmationQuestionText => Res.GetString("38205D4B-76E1-4288-8AA1-2B0F83F76395", "Do you want to send the message(s) despite these errors?");
}
