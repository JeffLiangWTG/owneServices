using System;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Startup
{
	class StartupCheckForthcomingSystemUpgrade : IPostLoginTask
	{
		public string TaskDescription
		{
			get { return (NoResString)"Check Forthcoming System Upgrade"; }
		}

		public void Execute()
		{
			if (Timer != null)
			{
				return;
			}

			Timer = new System.Timers.Timer();
			Timer.Interval = 1000;
			Timer.Elapsed += Timer_Elapsed;
			Timer.Start();
		}

		void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (ApplicationDispatcher.Current != null)
			{
				if (Timer != null)
				{
					Timer.Stop();
					Timer.Dispose();
					Timer = null;
					ApplicationDispatcher.Current.Invoke(CheckForthcomingSystemUpgrade);
				}
			}
		}

		void CheckForthcomingSystemUpgrade()
		{
			DateTime upgradeDateTime = GetSystemUpgradeDateTime();

			if (upgradeDateTime != DateTime.MinValue)
			{
				var logoff = ObjectFactory.Get<IHeartBeatRemoteLogoff>();
				if (logoff != null)
				{
					logoff.OnRemoteUpgradeLogoff(upgradeDateTime, () => GetSystemUpgradeDateTime() != DateTime.MinValue);
				}
			}
		}

		protected DateTime GetSystemUpgradeDateTime()
		{
			return Heartbeat.GetSystemUpgradeDateTime();
		}

		System.Timers.Timer Timer;

		public bool ShouldExecute()
		{
			return true;
		}
	}
}
