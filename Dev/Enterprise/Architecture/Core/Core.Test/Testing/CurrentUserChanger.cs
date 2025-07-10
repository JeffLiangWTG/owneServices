using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public static class CurrentUserChanger
	{
		public static IDisposable SwitchToNewUserTemporarily(string loginName)
		{
			return SwitchToNewUserTemporarily(loginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
		}

		public static IDisposable SwitchToNewUserTemporarily(string loginName, Guid branchPK, Guid departmentPK)
		{
			return EnvProxy.Instance.SetTemporaryUserContext(loginName, branchPK, departmentPK);
		}
	}
}
