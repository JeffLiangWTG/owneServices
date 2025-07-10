using CargoWise.Common;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	internal class RNSUserNotificationWrapper : IUserNotification
	{
		public RNSUserNotificationWrapper(ILogger logger)
		{
			this.logger = Argument.NotNull(logger, "logger");
		}
		readonly ILogger logger;

		#region IUserNotification Members

		public string ShowQuestion(string message, string caption, int answerLength, CodeDescriptionPairList answerList, string defaultAnswer = null)
		{
			return string.Empty;
		}

		public bool ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString)
		{
			return true;
		}

		public bool ShowConfirmation(string message, string caption, bool warning = false)
		{
			return true;
		}

		public void ShowWarning(string message, string caption)
		{
			logger.Log(Integration.LogType.Warning, message);
		}

		public void ShowError(string message, string caption)
		{
			logger.Log(Integration.LogType.Warning, message);
		}

		public void ShowInformation(string message, string caption)
		{
			logger.Log(Integration.LogType.Information, message);
		}

		#endregion

	}
}
