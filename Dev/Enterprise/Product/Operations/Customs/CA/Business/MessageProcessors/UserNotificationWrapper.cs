using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	class UserNotificationWrapper : Customs.Business.MessageManagers.IUserNotification
	{
		public UserNotificationWrapper(IXmlImportLogger logger)
		{
			this.logger = Argument.NotNull(logger, "logger");
		}
		readonly IXmlImportLogger logger;

		#region IUserNotification Members

		public string ShowQuestion(string message, string caption, int answerLength, CodeDescriptionPairList answerList, string defaultAnswer = null) => ZString.Empty;

		public bool ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString) => true;

		public bool ShowConfirmation(string message, string caption, bool warning = false) => true;

		public void ShowWarning(string message, string caption) => logger.Log(Integration.LogType.Warning, message);

		public void ShowError(string message, string caption) => logger.Log(Integration.LogType.Warning, message);

		public void ShowInformation(string message, string caption) => logger.Log(Integration.LogType.Information, message);

		#endregion
	}
}
