using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Accounting.GUI
{
	public abstract partial class SecurityOverrideProviderWithJobReopenSupport : SecurityOverrideProviderWithApprovalRequestSupport
	{
		public SecurityOverrideProviderWithJobReopenSupport(bool showApprovalRequestButton = false, bool alwaysCreateApprovalRequest = false, bool keepLoginFormResultAfterFirstUserAnswer = false)
			: base(showApprovalRequestButton, alwaysCreateApprovalRequest, keepLoginFormResultAfterFirstUserAnswer)
		{
		}

		static string TheseJobsAreCurrentlyClosed => Res.GetString("Accounting|SecurityOverrideProvider|TheseJobsAreCurrentlyClosed", "These jobs are currently closed.");
		static string BeforeProceedingTheyMustBeReopened => Res.GetString("Accounting|SecurityOverrideProvider|BeforeProceedingTheyMustBeReopened", "Before proceeding they must be re-opened.");
		static string AuthorizationUserRequiredForTheseJobs => Res.GetString("Accounting|SecurityOverrideProvider|ToReopenTheseJobsPleaseHaveAnAuthorisedUserEnterTheirUsernameAndPasswordBelow", "To re-open these jobs, please have an authorized user enter their username and password below.");
		static string SecurityToReopenJobs => Res.GetString("Accounting|SecurityOverrideProvider|YouDoNotHaveSecurityRightsToReopenClosedJobs", "You do not have security rights to Re-open Closed Jobs.");
		static string AUserWithSecurityRightsToReopenClosedJobs => Res.GetString("Accounting|SecurityOverrideProvider|AUserWithSecurityRightsToReopenClosedJobsCanAuthoriseThisTransaction", "A user with security rights to Re-open Closed jobs can authorize this transaction.");

		static string TheJobIsCurrentlyClosed => Res.GetString("Accounting|SecurityOverrideProvider|TheJobIsCurrentlyClosed", "The job is currently closed.");
		static string BeforeProceedingItMustBeReopened => Res.GetString("Accounting|SecurityOverrideProvider|BeforeProceedingItMustBeReopened", "Before proceeding it must be re-opened.");
		static string AuthorizationUserRequiredForTheJob => Res.GetString("Accounting|SecurityOverrideProvider|ToReopenTheJobPleaseHaveAnAuthorisedUserEnterTheirUsernameAndPasswordBelow", "To re-open the job, please have an authorized user enter their username and password below.");
		static string SecurityToReopenRestrictedJobs => Res.GetString("Accounting|SecurityOverrideProvider|YouDoNotHaveSecurityRightsToReopenRestritedClosedJobs", "You do not have security rights to Re-open Closed Jobs subjected to Re-Open Restriction.");
		static string AUserWithSecurityRightsToReopenRestrictedClosedJobs => Res.GetString("Accounting|SecurityOverrideProvider|AUserWithSecurityRightsToReopenRestrictedClosedJobsCanAuthoriseThisTransaction", "A user with security rights 'Allow Reopen Jobs Past Allowed Reopen Period' can authorize this transaction.");

		public static string ReopenClosedJobSecurityOverrideMessageMoreThanOneJobs
		{
			get
			{
				return
					TheseJobsAreCurrentlyClosed + "\r\n" +
					BeforeProceedingTheyMustBeReopened + "\r\n" +
					SecurityToReopenJobs + "\r\n" +
					AUserWithSecurityRightsToReopenClosedJobs +
					AuthorizationUserRequiredForTheseJobs + "\r\n";
			}
		}

		public static string ReopenClosedJobSecurityOverrideMessageOneJob
		{
			get
			{
				return
					TheJobIsCurrentlyClosed + "\r\n" +
					BeforeProceedingItMustBeReopened + "\r\n" +
					SecurityToReopenJobs + "\r\n" +
					AUserWithSecurityRightsToReopenClosedJobs +
					AuthorizationUserRequiredForTheJob + "\r\n";
			}
		}

		public static string ReopenRestrictedClosedJobSecurityOverrideMessageMoreThanOneJobs
		{
			get
			{
				return
					TheseJobsAreCurrentlyClosed + "\r\n" +
					BeforeProceedingTheyMustBeReopened + "\r\n" +
					SecurityToReopenRestrictedJobs + "\r\n" +
					AUserWithSecurityRightsToReopenRestrictedClosedJobs +
					AuthorizationUserRequiredForTheseJobs + "\r\n";
			}
		}

		public static string ReopenRestrictedClosedJobSecurityOverrideMessageOneJob
		{
			get
			{
				return
					TheJobIsCurrentlyClosed + "\r\n" +
					BeforeProceedingItMustBeReopened + "\r\n" +
					SecurityToReopenRestrictedJobs + "\r\n" +
					AUserWithSecurityRightsToReopenRestrictedClosedJobs +
					AuthorizationUserRequiredForTheJob + "\r\n";
			}
		}

		public static string ReopenClosedJobSecurityGrantedMessage
		{
			get { return Res.GetString("Accounting|SecurityOverrideProvider|YouAreAboutToReopenTheseClosedJobsDoYouWantToProceed", "You are about to reopen these closed jobs. Do you want to proceed?"); }
		}

		protected override string GetSecurityOverrideMessageCore(SecurityCheckpoint checkPoint)
		{
			string result = string.Empty;

			if (checkPoint == Env.Security.ReopenJob)
			{
				result = GetReopenClosedJobSecurityOverrideMessage();
			}
			else if (checkPoint == Env.Security.ReopenJobPastAllowedReOpenPeriod)
			{
				result = GetReopenRestrictedClosedJobSecurityOverrideMessage();
			}
			else
			{
				result = base.GetSecurityOverrideMessageCore(checkPoint);
			}

			return result;
		}

		protected override string GetSecurityGrantedMessage(SecurityCheckpoint checkPoint)
		{
			string result = string.Empty;

			if (checkPoint == Env.Security.ReopenJob || checkPoint == Env.Security.ReopenJobPastAllowedReOpenPeriod)
			{
				result = GetReopenClosedJobSecurityGrantedMessage();
			}

			return result;
		}

		protected virtual string GetReopenClosedJobSecurityOverrideMessage()
		{
			return string.Empty;
		}

		protected virtual string GetReopenClosedJobSecurityGrantedMessage()
		{
			return string.Empty;
		}

		protected virtual string GetReopenRestrictedClosedJobSecurityOverrideMessage()
		{
			return string.Empty;
		}
	}
}
