using System;
using System.Runtime.InteropServices;
using Microsoft.Web.Administration;
using Polly;
using WTG.DevTools.TestFramework;

namespace Enterprise.ZArchitecture.Web.Shared.Test
{
	class WebApplicationStartStopAppPool : ExecuteOrInvokeViaAppManager
	{
		public static void ExecuteOrInvoke(string siteName, string appPoolName, string action)
		{
			new WebApplicationStartStopAppPool { siteName = siteName, appPoolName = appPoolName, action = action }.ExecuteOrInvoke();
		}

		public override void Execute()
		{
			Policy
				.Handle<COMException>()
				.WaitAndRetry(10, _ => TimeSpan.FromSeconds(10))
				.Execute(DoExecute);
		}

		void DoExecute()
		{
			using (var serverManager = new ServerManager())
			{
				var appPool = serverManager.ApplicationPools[appPoolName];
				if (action == ActionStop)
				{
					appPool.Stop();
				}
				else if (action == ActionStart)
				{
					appPool.Start();
				}
				else
				{
					throw new InvalidOperationException("Unknown action " + action ?? "null");
				}
			}
		}

		protected override object[] GetState()
		{
			return new object[] { siteName, appPoolName, action };
		}

		protected override void ReadState(object[] state)
		{
			siteName = (string)state[0];
			appPoolName = (string)state[1];
			action = (string)state[2];
		}

		string siteName;
		string appPoolName;
		string action;

		public const string ActionStop = "stop";
		public const string ActionStart = "start";
	}
}
