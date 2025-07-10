using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.CA.Business.OperationalAction
{
	public class CARNSQueryOperationalActionRunner
	{
		public CARNSQueryOperationalActionRunner(OperationalActionLogAndUserNotificationWrapper logandNotificationWrapper, ISendsMessagesToCustoms sendMessagesToCustoms)
		{
			this.logAndNotificationWrapper = logandNotificationWrapper;
			this.sendMessagesToCustoms = sendMessagesToCustoms;
		}

		readonly OperationalActionLogAndUserNotificationWrapper logAndNotificationWrapper;
		readonly ISendsMessagesToCustoms sendMessagesToCustoms;

		public void PerformFunctionOperationalAction(BusinessObject[] targets)
		{
			if (targets.Length == 0)
			{
				logAndNotificationWrapper.Notify(OperationalActionLogErrorLevel.Error, Constants.NoDeclarationToSend);
			}
			else
			{
				logAndNotificationWrapper.SetSectionProgressMax(targets.Length);
				RunOperationalActionSendMessage(targets);
			}
		}

		void RunOperationalActionSendMessage(BusinessObject[] targets)
		{
			foreach (var target in targets)
			{
				logAndNotificationWrapper.Notify(OperationalActionLogErrorLevel.Informational, Constants.Seperator);
				var newFactory = new BusinessObjectFactory();
				var targetDeclaration = newFactory.Load<JobDeclaration>(target.PK);
				var errorMessage = string.Empty;
				if (IsJobEligibleForSending(targetDeclaration, out errorMessage))
				{
					logAndNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Informational, Constants.SendingMessageForDeclaration + Constants.NotifyFormatParaHolder, targetDeclaration.GetDeclarationIdLink());
					logAndNotificationWrapper.Notify(OperationalActionLogErrorLevel.Informational, SendMessageForDeclaration(targetDeclaration) ? Constants.Successful : Constants.Failed);
				}
				else if (!string.IsNullOrWhiteSpace(errorMessage))
				{
					logAndNotificationWrapper.Notify(OperationalActionLogErrorLevel.Error, errorMessage);
				}
			}
		}

		bool IsJobEligibleForSending(JobDeclaration targetDeclaration, out string errorMessage)
		{
			var result = false;
			if (targetDeclaration == null)
			{
				errorMessage = Constants.InvalidDeclaration;
			}
			else
			{
				logAndNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Informational, Constants.ProcessingDeclaration + Constants.NotifyFormatParaHolder, targetDeclaration.GetDeclarationIdLink());
				if (!(targetDeclaration.JE_MessageType == JobMessageTypeList.Codes.Import))
				{
					errorMessage = Constants.DeclarationNotEligableForRNSQuery;
				}
				else
				{
					var checker = new CanSendDeclarationChecker(targetDeclaration);
					try
					{
						result = checker.AtLeastOneEntryExists() && checker.RequiredEntryExist(MessageTypeList.Codes.EDIRelease);
						errorMessage = checker.LastErrorMessage;
					}
					catch (System.ArgumentException ex)
					{
						result = false;
						errorMessage = ex.Message;
					}
				}
			}
			return result;
		}

		bool SendMessageForDeclaration(JobDeclaration targetDeclaration)
		{
			var result = false;
			targetDeclaration.MessageInitiator = sendMessagesToCustoms;
			var declaration = targetDeclaration;
			var relEntryHeader = declaration?.ReleaseEntryHeader;
			if (relEntryHeader != null)
			{
				var dataWrapper = new StatusQueryMessageWrapper(relEntryHeader);
				var manager = new RNSMessageManager(dataWrapper, logAndNotificationWrapper, true);
				result = manager.SendMessage(MessageSubTypes.Request, false);
			}
			return result;
		}
	}
}
