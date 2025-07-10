using System.Collections.Specialized;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.Wow
{
	public class LoggedSendsMessagesToAUCustoms : ISendsMessagesToCustoms
	{
		public LoggedSendsMessagesToAUCustoms(JobDeclaration declaration, INotifications notify)
		{
			this.fDeclaration = declaration;
			this.fNotify = notify;
		}

		#region ISendsMessagesToCustoms Members

		public bool ContinueWithSendDespiteOldExchangeRates()
		{
			return true;
		}

		public bool ContinueWithSaveAndAmendEntry()
		{
			return true;
		}

		public bool ContinueWithDeleteAndWithdrawEntry()
		{
			return true;
		}

		public bool ResendDeclaration()
		{
			return true;
		}

		public void NotifyUserOfAnInvalidOperation(string text)
		{
			fNotify.Notify(new ErrorNotification(
				WowErrorType.CustomsDeclarationMergeError, fDeclaration.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString()));
			fNotify.Notify(new ErrorNotification(
				WowErrorType.CustomsDeclarationMergeError, fDeclaration.NotificationsIncludingChildren.GetMessageErrors().ToUniqueMessageListString()));
		}

		public void NotifyUserOfASuccessfulSend(string text)
		{
		}

		public void WarnUserAboutSomething(string message, string caption)
		{
		}

		public bool ContinueWithDoCPDecsAndRemergeEntry()
		{
			return true;
		}

		public EXIT1MessageType GetExit1MessageType()
		{
			return new EXIT1MessageType();
		}

		public bool YesNoQuery(string question, string caption, Customs.Business.MessageStyle messageStyle = Customs.Business.MessageStyle.Question)
		{
			fNotify.Notify(new ErrorNotification(
				WowErrorType.CustomsDeclarationMergeError, "The following question was answered 'yes': " + question));
			return true;
		}

		public Customs.Business.YesNoCancel YesNoCancelQuery(string question, string caption, Customs.Business.MessageStyle messageStyle = Customs.Business.MessageStyle.Question)
		{
			fNotify.Notify(new ErrorNotification(
				WowErrorType.CustomsDeclarationMergeError, "The following question was answered 'cancel': " + question));
			return Customs.Business.YesNoCancel.Cancel;
		}

		public bool AskUserToContinueWithAction(string message, string caption, BusinessObject topLevelBusinessObject = null)
		{
			var result = true;
			if (ContinueWithAction(message, caption))
			{
				if (Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed && topLevelBusinessObject != null)
				{
					var businessObject = topLevelBusinessObject as EnterpriseBusinessObject;
					if (businessObject != null)
					{
						var supervisorOverrides = new Customs.Business.SupervisorOverrides(businessObject,
							Customs.Business.SupervisorOverridesContext.SendingMessages);
						result = Customs.GUI.SupervisorOverridesHelper.IsSupervisorApproved(supervisorOverrides, businessObject.Logs);
					}
				}
			}
			else
			{
				result = false;
			}
			return result;
		}

		public bool ContinueWithAction(string message, string caption)
		{
			fNotify.Notify(new ErrorNotification(
				WowErrorType.CustomsDeclarationMergeError, "The following question was answered 'yes': " + message));
			return true;
		}

		public bool ContinueWithSend(StringCollection warnings)
		{
			foreach (string warning in warnings)
			{
				fNotify.Notify(new ErrorNotification(
					WowErrorType.CustomsDeclarationMergeError, "The following warnings were encountered: " + warning));
			}
			return true;
		}

		public void MessageSendErrorAlert(StringCollection errors)
		{
			foreach (string error in errors)
			{
				fNotify.Notify(new ErrorNotification(
					WowErrorType.CustomsDeclarationMergeError, "The following errors were encountered: " + error));
			}
		}

		public Enterprise.Customs.Business.SingleMessageManager[] WhichMessagesShouldWeSend(Enterprise.Customs.Business.SingleMessageManager[] allManagers)
		{
			return System.Array.Empty<Enterprise.Customs.Business.SingleMessageManager>();
		}

		public Enterprise.Customs.Business.SingleMessageManager[] WhichMessagesShouldWeWithdraw(Enterprise.Customs.Business.SingleMessageManager[] allManagers)
		{
			return System.Array.Empty<Enterprise.Customs.Business.SingleMessageManager>();
		}

		public Enterprise.Customs.Business.SingleMessageManager[] WhichMessagesShouldWeReset(Enterprise.Customs.Business.SingleMessageManager[] allManagers)
		{
			return System.Array.Empty<Enterprise.Customs.Business.SingleMessageManager>();
		}

		bool Enterprise.Customs.Business.ISendsMessagesToCustoms.ShowUserConfirmation(string message, string caption, string confirmationPrompt, string confirmationString)
		{
			return true;
		}

		public ContinueWithSave GetAmendmentOrWithdrawalReason(Enterprise.Customs.Business.AmendmentWithdrawalReason amendmentReason)
		{
			return ContinueWithSave.Yes;
		}

		public ContinueWithSave GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(JobDeclaration declaration, Customs.Business.EntryMessageStatusFilterType filterType, bool forceRegeneration, bool showCPQAFormAlways)
		{
			return ContinueWithSave.Yes;
		}

		public ContinueWithSave GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(JobDeclaration declaration, CusEntryHeader[] entries)
		{
			return ContinueWithSave.Yes;
		}

		public ContinueWithSave GenerateWithdrawDecQuestionAndShowCPQAForm(JobDeclaration declaration)
		{
			return ContinueWithSave.Yes;
		}

		public ContinueWithSave GenerateWithdrawDecQuestionAndShowCPQAForm(JobDeclaration declaration, CusEntryHeader[] entries)
		{
			return ContinueWithSave.Yes;
		}

		public Customs.Business.DeferredAmendmentSavingOptions ShowBackdoorForSavingOnAmendmentFormGetConfirmationFromUsers()
		{
			return new Customs.Business.DeferredAmendmentSavingOptions();
		}

		public ContinueWithSave GetAmendmentWithdrawalReason(Customs.Business.AmendmentWithdrawalReason reason)
		{
			return ContinueWithSave.No;
		}

		#endregion

		#region Implementation

		readonly JobDeclaration fDeclaration;
		readonly INotifications fNotify;

		#endregion

	}
}
