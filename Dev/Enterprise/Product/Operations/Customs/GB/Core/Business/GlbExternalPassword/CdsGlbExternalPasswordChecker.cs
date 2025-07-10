using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business
{
	public abstract class AbstractCdsGlbExternalPasswordChecker<T> where T : BusinessObject
	{
		protected AbstractCdsGlbExternalPasswordChecker(T businessObject)
		{
			this.businessObject = businessObject;
		}

		protected readonly T businessObject;

		public const int ExpiredMonths = 18;

		protected abstract ZString GetEori();

		protected abstract ZString GetBadge();

		public bool PasswordExistsAndOkToSendToCds
		{
			get
			{
				var declarantEori = GetEori();
				var badge = GetBadge();
				var externalPassword = GetPassword(declarantEori, badge);
				if (externalPassword != null)
				{
					switch (externalPassword.Status)
					{
						case PasswordStatusList.Codes.PasswordOK:
						case PasswordStatusList.Codes.Valid:
							return ExternalPasswordIsStillFresh(externalPassword, declarantEori, badge) && IsExternalPasswordEnabledForCDS(externalPassword, declarantEori, badge);

						case PasswordStatusList.Codes.Invalid:
							ShowError("Credentials are invalid, await further updates from CDS via eHub.", declarantEori, badge, externalPassword.StatusMessage);
							return false;

						case PasswordStatusList.Codes.Deactivated:
							ShowError("Credentials are inactive, await further updates from CDS via eHub.", declarantEori, badge, externalPassword.StatusMessage);
							return false;

						default:
							ShowError("Credentials are in an unexpected state (" + externalPassword.Status + ").", declarantEori, badge, externalPassword.StatusMessage);
							return false;
					}
				}
				else
				{
					ShowError("No company-level CDS credentials exist.", declarantEori, badge);
					return false;
				}
			}
		}

		public static bool ExternalPasswordAboutToExpire(GlbExternalPassword_GB externalPassword)
		{
			return externalPassword.GP_IssueDate.AddMonths(ExpiredMonths - 1) < ZDateTime.UtcNow;
		}

		bool ExternalPasswordIsStillFresh(GlbExternalPassword_GB externalPassword, string declarantEori, string badge)
		{
			if (externalPassword.GP_ExpiryDate < ZDateTime.UtcNow)
			{
				ShowError("Credentials have expired. Ensure that your EHU service task is running, and refresh them if necessary.", declarantEori, badge, externalPassword.StatusMessage);
				return false;
			}
			if (ExternalPasswordAboutToExpire(externalPassword))
			{
				var message = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Your credentials were issued on {0} and will expire {1} months hence on {2}. You willl need to refresh them soon.", externalPassword.GP_IssueDate.ToStandardDateTimeString(), ExpiredMonths, externalPassword.GP_IssueDate.AddMonths(ExpiredMonths).ToStandardDateTimeString());
				ShowWarning(message, declarantEori, badge, externalPassword.StatusMessage);
				return true;
			}
			return true;
		}

		bool IsExternalPasswordEnabledForCDS(GlbExternalPassword_GB externalPassword, string declarantEori, string badge)
		{
			if (!externalPassword.IsTokenForCDS)
			{
				ShowError("No valid token that is applicable to CDS was found", declarantEori, badge);
				return false;
			}
			return true;
		}

		void ShowError(string mainText, string declarantEori, string badge, string statusMessage = "")
		{
			ShowError(GetFriendlyMessage(mainText, statusMessage, declarantEori, badge));
		}

		string GetFriendlyMessage(string mainText, string statusMessage, string declarantEori, string badge)
		{
			return mainText + "\r\n" + statusMessage + "\r\nEORI " + declarantEori + ", badge " + badge + ".\r\nPlease see eLearning unit 1BGB045 on My Account for assistance.";
		}

		void ShowWarning(string mainText, string declarantEori, string badge, string statusMessage = "")
		{
			ShowWarning(GetFriendlyMessage(mainText, statusMessage, declarantEori, badge));
		}

		protected virtual void ShowError(ZString error)
		{
		}

		protected virtual void ShowWarning(ZString warning)
		{
		}

		GlbExternalPassword_GB GetPassword(ZString declarantEori, ZString badge)
		{
			return GBCustomsCDSXmlCredentialConfigurationHandler.FindAndLoadGlbExternalPassword(badge, declarantEori, businessObject.Factory);
		}
	}

	public class BaseCdsGlbExternalPasswordChecker : AbstractCdsGlbExternalPasswordChecker<JobDeclaration>
	{
		protected BaseCdsGlbExternalPasswordChecker(JobDeclaration businessObject) : base(businessObject) { }

		protected override ZString GetEori()
		{
			return ((Declaration.JobDeclaration)businessObject)?.GetEori() ?? ZString.Empty;
		}

		protected override ZString GetBadge()
		{
			return businessObject.JE_CustomsProfile;
		}
	}

	public class CdsGlbExternalPasswordChecker : BaseCdsGlbExternalPasswordChecker
	{
		public CdsGlbExternalPasswordChecker(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms) : base(declaration)
		{
			this.sendMessagesToCustoms = sendMessagesToCustoms;
		}

		protected override void ShowError(ZString error)
		{
			sendMessagesToCustoms.NotifyUserOfAnInvalidOperation(error);
		}

		protected override void ShowWarning(ZString warning)
		{
			sendMessagesToCustoms.WarnUserAboutSomething(warning, "CDS credentials are nearly expired");
		}

		readonly ISendsMessagesToCustoms sendMessagesToCustoms;
	}

	public class CdsGlbExternalPasswordCheckerWithNotifications : BaseCdsGlbExternalPasswordChecker
	{
		readonly MessageSendingNotificationCollection notifications;

		public CdsGlbExternalPasswordCheckerWithNotifications(JobDeclaration declaration, MessageSendingNotificationCollection notifications) : base(declaration)
		{
			this.notifications = notifications;
		}

		protected override void ShowError(ZString error)
		{
			notifications.AddError(error);
		}

		protected override void ShowWarning(ZString warning)
		{
			notifications.AddWarning(warning);
		}
	}
}
