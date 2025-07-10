using System.Globalization;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture.Data.Mutex
{
	public static class Extensions
	{
		public static string GetUserWithLock(this LockInfo lockInfo) => lockInfo?.UserWithLock?.GS_FullName ?? Res.GetString("{89990C7C-AC71-4140-9E72-F5E8D15C6B77}", "Unknown");

		public static string GetMutexLockByInfo(this ZGlobalMutex mutex)
		{
			var lockInfo = mutex.GetLockInfo();
			string result;
			if (lockInfo == null)
			{
				result = Res.GetString("{679525BB-6CDB-4302-AF14-0AB50E29FE17}", "Someone else");
			}
			else
			{
				result = string.Format(CultureInfo.InvariantCulture, Res.GetString("{BD8C94C2-EDA9-49C4-9580-2ADCFE42F6DF}", "{0} ({1} since {2})", lockInfo.GetUserWithLock(), lockInfo.HostName, lockInfo.LockStartTime));
			}

			return result;
		}
	}
}
