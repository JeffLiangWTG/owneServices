using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Services.OperationalActions.Support;
using OperationalActionConstants = Enterprise.Customs.CA.Business.OperationalAction.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.OperationalAction
{
	public class OperationalActionLogAndUserNotificationWrapper : IUserNotification, IOperationalActionSectionLog
		, IMessageInstructionUserNotification
	{
		public const string DefaultScheduleActionNone = "NON";
		public const string DefaultScheduleActionCancel = "CAN";

		public OperationalActionLogAndUserNotificationWrapper(IUserNotification notifier, IOperationalActionSectionLog log, ZBool ignoreAllWarnings, ZBool autoRecalculateDutyAndTax, ZBool suppressNotificationPopout)
			: base()
		{
			this.logger = log;
			this.notifier = notifier;
			this.ignoreAllWarnings = ignoreAllWarnings;
			this.autoRecalculateDutyAndTax = autoRecalculateDutyAndTax;
			this.suppressNotificationPopout = suppressNotificationPopout;
		}

		public OperationalActionLogAndUserNotificationWrapper(IUserNotification notifier, IOperationalActionSectionLog log, ZBool ignoreAllWarnings, ZBool suppressNotificationPopout)
			: base()
		{
			this.logger = log;
			this.notifier = notifier;
			this.ignoreAllWarnings = ignoreAllWarnings;
			this.suppressNotificationPopout = suppressNotificationPopout;
		}

		readonly IOperationalActionSectionLog logger;
		readonly IUserNotification notifier;

		#region User Notification controls

		readonly ZBool ignoreAllWarnings;
		readonly ZBool autoRecalculateDutyAndTax;
		readonly ZBool suppressNotificationPopout;

		#endregion

		#region IUserNotification Members

		public bool ShowConfirmation(string message, string caption, bool warning = false)
		{
			var result = false;
			logger.Notify(OperationalActionLogErrorLevel.Informational, OperationalActionConstants.AwaitingConfirmation + message);
			if ((message == B3CADBaseMessageManager.RecalculateDutyAndTax_NewRate || message == B3CADBaseMessageManager.RecalculateDutyAndTax)
				&& autoRecalculateDutyAndTax)
			{
				result = true;
				logger.Notify(OperationalActionLogErrorLevel.Informational, OperationalActionConstants.AutoAnswer + YesNoList.Descriptions.Yes);
			}
			else
			{
				result = ignoreAllWarnings || notifier.ShowConfirmation(message, caption, warning);
				logger.Notify(OperationalActionLogErrorLevel.Informational, (ignoreAllWarnings ? OperationalActionConstants.AutoAnswer : OperationalActionConstants.UsersAnswer) + (result ? YesNoList.Descriptions.Yes : YesNoList.Descriptions.No));
			}
			return result;
		}

		public bool ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString)
		{
			var result = false;
			logger.Notify(OperationalActionLogErrorLevel.Informational, OperationalActionConstants.AwaitingConfirmation + message);
			result = ignoreAllWarnings || notifier.ShowConfirmation(message, caption, confirmationPrompt, confirmationString);
			logger.Notify(OperationalActionLogErrorLevel.Informational, (ignoreAllWarnings ? OperationalActionConstants.AutoAnswer : OperationalActionConstants.UsersAnswer) + (result ? YesNoList.Descriptions.Yes : YesNoList.Descriptions.No));
			return result;
		}

		public string ShowQuestion(string message, string caption, int answerLength, CodeDescriptionPairList answerList, string defaultAnswer = null)
		{
			logger.Notify(OperationalActionLogErrorLevel.Informational, OperationalActionConstants.AwaitingAnswer + message);
			var result = notifier.ShowQuestion(message, caption, answerLength, answerList);
			logger.Notify(OperationalActionLogErrorLevel.Informational, OperationalActionConstants.UsersAnswer + result);
			return result;
		}

		public void ShowError(string message, string caption)
		{
			logger.Notify(OperationalActionLogErrorLevel.Error, OperationalActionConstants.EncounterError);
			logger.Notify(OperationalActionLogErrorLevel.Error, message);
			if (!suppressNotificationPopout)
			{
				notifier.ShowError(message, caption);
			}
		}

		public void ShowInformation(string message, string caption)
		{
			logger.Notify(OperationalActionLogErrorLevel.Informational, message);
			if (!suppressNotificationPopout)
			{
				notifier.ShowInformation(message, caption);
			}
		}

		public void ShowWarning(string message, string caption)
		{
			logger.Notify(OperationalActionLogErrorLevel.Warning, message);
			if (!suppressNotificationPopout)
			{
				notifier.ShowWarning(message, caption);
			}
		}

		#endregion

		#region IOperationalActionSectionLog Members

		public void BumpSectionProgress()
		{
			logger.BumpSectionProgress();
		}

		public void Notify(OperationalActionLogErrorLevel errorLevel, string text)
		{
			logger.Notify(errorLevel, text);
		}

		public void NotifyFormat(OperationalActionLogErrorLevel errorLevel, string format, params object[] args)
		{
			logger.NotifyFormat(errorLevel, format, args);
		}

		public void SetSectionProgressMax(int max)
		{
			logger.SetSectionProgressMax(max);
		}

		#endregion

		#region IMessageInstructionUserNotification Members

		public bool ShowMessageInstructionForm(MessageInstruction instruction)
		{
			var result = ignoreAllWarnings;
			if (instruction.ContainsValidationErrors)
			{
				logger.Notify(OperationalActionLogErrorLevel.Informational, OperationalActionConstants.AwaitingConfirmation + instruction.ValidationErrorsMessage);
			}
			if (instruction.ContainsAdditionalWarnings)
			{
				logger.Notify(OperationalActionLogErrorLevel.Informational, OperationalActionConstants.AwaitingConfirmation + instruction.AdditionalWarningsMessage);
			}
			if (!result)
			{
				var innerNoficiation = this.notifier as IMessageInstructionUserNotification;
				if (innerNoficiation != null)
				{
					result = innerNoficiation.ShowMessageInstructionForm(instruction);
					logger.Notify(OperationalActionLogErrorLevel.Informational, (OperationalActionConstants.UsersAnswer) + (result ? OperationalActionConstants.ContinueSendingDespiteOfRationalityWarnings : OperationalActionConstants.MessageSendingCancelled));
				}
			}
			else
			{
				logger.Notify(OperationalActionLogErrorLevel.Informational, (OperationalActionConstants.AutoAnswer) + OperationalActionConstants.ContinueSendingDespiteOfRationalityWarnings);
			}
			return result;
		}

		#endregion
	}
}
