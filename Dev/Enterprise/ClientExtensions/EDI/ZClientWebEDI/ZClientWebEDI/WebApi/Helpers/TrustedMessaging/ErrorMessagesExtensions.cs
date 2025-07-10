using System.Text;
using WTG.TrustedMessaging.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class ErrorMessagesExtensions
	{
		public static string GetSingleLineMessages(this ErrorMessages errorMessages)
		{
			var builder = new StringBuilder();
			foreach (var message in errorMessages.Messages)
			{
				builder.Append($"{message.Code} {message.Message} ");
			}
			return builder.ToString().TrimEnd();
		}
	}
}
