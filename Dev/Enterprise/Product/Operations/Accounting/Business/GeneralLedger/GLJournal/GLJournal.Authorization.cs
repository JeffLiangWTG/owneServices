using Enterprise.Environment;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public partial class GLJournal
	{
		internal enum AuthorisationRequiredType
		{
			NoAuthorisationRequired,
			HasAuthorisationRights,
			AuthorisationRequired
		}

		internal bool IsLevelAuthorizationRequired
		{
			get
			{
				if (IsNoteJournal)
				{
					return false;
				}

				ClearLineAuthorizationCache();
				var securityCheckpoint = GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPointByCachedData(this, ref levelAuthorizationCache);
				return !securityCheckpoint.IsAllowed || GLJournalApprovalAuthorizationHelper.IsUserHasRightButNotAllowedToApproveOwnJournal(securityCheckpoint);
			}
		}

		internal AuthorisationRequiredType GetLineAuthorisationRequiredType(GLJournalLine gLJournalLine)
		{
			if (gLJournalLine.IsNoteJournal)
			{
				return AuthorisationRequiredType.HasAuthorisationRights;
			}

			var result = AuthorisationRequiredType.NoAuthorisationRequired;
			var securityCheckpoint = GLJournalApprovalAuthorizationHelper.RequiredSecurityCheckPointByCachedData(gLJournalLine, levelAuthorizationCache);

			if (securityCheckpoint != Env.Security.None)
			{
				if (!securityCheckpoint.IsAllowed || GLJournalApprovalAuthorizationHelper.IsUserHasRightButNotAllowedToApproveOwnJournal(securityCheckpoint))
				{
					result = AuthorisationRequiredType.AuthorisationRequired;
				}
				else if (securityCheckpoint.IsAllowed)
				{
					result = AuthorisationRequiredType.HasAuthorisationRights;
				}
			}

			return result;
		}

		void ClearLineAuthorizationCache()
		{
			levelAuthorizationCache = null;
		}
		object levelAuthorizationCache;
	}
}
