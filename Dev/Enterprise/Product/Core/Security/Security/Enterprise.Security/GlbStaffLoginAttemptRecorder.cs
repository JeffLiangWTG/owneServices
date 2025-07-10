using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security
{
	public class GlbStaffLoginAttemptRecorder : IGlbStaffLoginAttemptRecorder
	{
		public bool IsAnonymousUserLockedOut(string loginName)
		{
			return LoginAttemptRecorder.IsLockedOut(loginName, GlbStaffSchema.Constants.Prefix, null, DataRegistry.Instance.LoginLockoutMinutes, true);
		}

		public bool IsLockedOut(string loginName, byte[] loginHash)
		{
			return LoginAttemptRecorder.IsLockedOut(loginName, GlbStaffSchema.Constants.Prefix, loginHash, DataRegistry.Instance.LoginLockoutMinutes);
		}

		public ZDateTime LockoutDateTimeLocal(string loginName)
		{
			return LoginAttemptRecorder.LockoutDateTimeLocal(loginName, GlbStaffSchema.Constants.Prefix, DataRegistry.Instance.LoginLockoutMinutes);
		}

		public void RecordLoginAttempt(string loginName, byte[] loginHash)
		{
			LoginAttemptRecorder.RecordFailedLoginAttempts(
				loginName,
				GlbStaffSchema.Constants.Prefix,
				loginHash,
				DataRegistry.Instance.LoginAttempts,
				DataRegistry.Instance.LoginLockoutMinutes,
				User.ServiceUserCode);
		}

		public void Unlock(string loginName)
		{
			LoginAttemptRecorder.Unlock(loginName, GlbStaffSchema.Constants.Prefix);
		}
	}
}
