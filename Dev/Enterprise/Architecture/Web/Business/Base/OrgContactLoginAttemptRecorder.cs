using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class OrgContactLoginAttemptRecorder : IOrgContactLoginAttemptRecorder
	{
		public void RecordLoginAttempt(string companyCode, string username, byte[] hash = null)
		{
			LoginAttemptRecorder.RecordFailedLoginAttempts(CombineLoginNameAndCompanyCode(companyCode, username), OrgContactSchema.Constants.Prefix, hash, WebDataRegistry.Instance.WebLoginAttempts.Value, WebDataRegistry.Instance.WebLoginLockoutMinutes.Value, User.WebUserCode);
		}

		public bool IsLockedOut(string companyCode, string username, byte[] hash)
		{
			return LoginAttemptRecorder.IsLockedOut(CombineLoginNameAndCompanyCode(companyCode, username), OrgContactSchema.Constants.Prefix, hash, WebDataRegistry.Instance.WebLoginLockoutMinutes.Value);
		}

		public bool IsAnonymousUserLockedOut(string companyCode, string username)
		{
			var lockedOut = LoginAttemptRecorder.IsLockedOut(CombineLoginNameAndCompanyCode(companyCode, username), OrgContactSchema.Constants.Prefix, null, WebDataRegistry.Instance.WebLoginLockoutMinutes.Value, true);

			if (!lockedOut && !string.IsNullOrEmpty(companyCode))
			{
				lockedOut = LoginAttemptRecorder.IsLockedOut(username, OrgContactSchema.Constants.Prefix, null, WebDataRegistry.Instance.WebLoginLockoutMinutes.Value, true);
			}

			return lockedOut;
		}

		public ZDateTime LockoutDateTimeLocal(string companyCode, string username)
		{
			var lockedOutTime = LoginAttemptRecorder.LockoutDateTimeLocal(CombineLoginNameAndCompanyCode(companyCode, username), OrgContactSchema.Constants.Prefix, WebDataRegistry.Instance.WebLoginLockoutMinutes.Value);

			if (!string.IsNullOrEmpty(companyCode))
			{
				var lockedOutTimeNoCompany = LoginAttemptRecorder.LockoutDateTimeLocal(username, OrgContactSchema.Constants.Prefix, WebDataRegistry.Instance.WebLoginLockoutMinutes.Value);
				if (!lockedOutTimeNoCompany.IsEmpty && (lockedOutTime.IsEmpty || lockedOutTimeNoCompany > lockedOutTime))
				{
					lockedOutTime = lockedOutTimeNoCompany;
				}
			}

			return lockedOutTime;
		}

		public void Unlock(string companyCode, string username, bool unlockEmptyCompany)
		{
			LoginAttemptRecorder.Unlock(CombineLoginNameAndCompanyCode(companyCode, username), OrgContactSchema.Constants.Prefix);

			if (unlockEmptyCompany && !string.IsNullOrEmpty(companyCode))
			{
				LoginAttemptRecorder.Unlock(username, OrgContactSchema.Constants.Prefix);
			}
		}

		public static string CombineLoginNameAndCompanyCode(string companyCode, string loginName)
		{
			return string.IsNullOrEmpty(companyCode) ? loginName : (loginName + " " + companyCode);
		}
	}
}
