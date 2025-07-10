using CargoWise.Common;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	internal class B3AutoSendingLogAndUserNotificationWrapper : IUserNotification
	{
		public B3AutoSendingLogAndUserNotificationWrapper(JobDeclaration declaration, ILogger logger)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.declaration = Argument.NotNull(declaration, "declaration");
			b3SendingNotificationHelper = new B3SendingNotificationHelper(declaration);
		}

		readonly ILogger logger;
		readonly JobDeclaration declaration;
		readonly B3SendingNotificationHelper b3SendingNotificationHelper;

		#region IUserNotification Members

		public bool ShowConfirmation(string message, string caption, bool warning = false)
		{
			return true;
		}

		public bool ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString)
		{
			return true;
		}

		public void ShowError(string message, string caption)
		{
			logger.Warning(string.Format("Failed to automatically send CAD for {0} due to {1}.", declaration.HumanReadableName, message));

			var emailBodyText = string.Format("<html><body><p>{0}</p></body></html>",
				Res.GetString("3208322d-80fa-4026-a05e-3b88676316d6", "Failed to automatically send CAD for {0} due to {1}.",
					b3SendingNotificationHelper.GetHyperLink(), message));
			b3SendingNotificationHelper.SendEmail(Res.GetString("7ea04be8-4d33-4767-a0d9-f0e1a6689c18", "CAD Auto Sending Warning"), emailBodyText);
		}

		public void ShowInformation(string message, string caption)
		{
			logger.Information(message);
		}

		public string ShowQuestion(string message, string caption, int answerLength, ZArchitecture.Core.CodeDescriptionPairList answerList, string defaultAnswer = null)
		{
			return string.Empty;
		}

		public void ShowWarning(string message, string caption)
		{
			logger.Warning(message);
		}

		#endregion
	}
}
