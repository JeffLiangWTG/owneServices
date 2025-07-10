using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
#if WINZOR
using CargoWise.Windows.UI;
#endif
using CargoWise.Windows.UI.Controls.Internal;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
#if !DEBUG
using Enterprise.Upgrades;
#endif

namespace Enterprise.ZArchitecture.GUI
{
#if DEBUG
	public
#endif
 class HeartbeatRemoteLogoff : IHeartBeatRemoteLogoff
	{
		[SuppressMessage("Microsoft.Design", "CA1031")]
		public void OnRemoteLogoff()
		{
			if (Globals.CanShowDialogs)
			{
				var logOffMessage = Res.GetString(
					"31F10439-D82B-4545-AFE0-5F06A954E309",
					"Someone on another machine has logged in and has forced this application to exit.\r\nA user can be only logged in from one computer or terminal session at a time.");

				try
				{
					var currentUser = EnvProxy.Instance?.CurrentUser;
					if (currentUser != null)
					{
						var msg = Res.GetString(
							"EAB3A0A0-A856-4de1-AB2E-479D6024520E",
							"Someone on another machine has logged in as \"{0}\" and has forced this application to exit.\r\nA user can be only logged in from one computer or terminal session at a time.",
							currentUser.FullName);

						using (var form = new SelfLogoffForm())
						{
							form.Message = msg;
							ZFormModaliser.ShowDialogWithoutDispose(form);
						}
					}
					else
					{
						var caption = Res.GetString("47443C57-73DC-4527-BAA2-CCBC9070F6D4", "Remote Log Off");
						using (var msgBox = new ZMessageBox(logOffMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning))
						{
							ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
						}
					}
				}
				catch
				{
#if DEBUG
					if (Globals.IsTest)
					{
						throw new InvalidOperationException("Calling Application.Exit()");
					}

					throw; // re-throw in debug mode, ignored in release mode since system is determined to terminate
#else
					WindowsEventLogger.LogError(logOffMessage);
					Console.Error.Write(logOffMessage);
#endif
				}
			}

#if DEBUG
			if (Globals.IsTest)
			{
				throw new InvalidOperationException("Calling Application.Exit()");
			}
#endif

			OpenedFormCache.GetInstance().SaveAllFormsPositionAndSize();

			ExceptionReporter.SuppressGui();
			GCTracker.StopTracking();
			ApplicationDispatcher.Current?.BeginInvoke(new Action(Application.Exit));
			Thread.Sleep(10000);
			Process.GetCurrentProcess().Kill();
		}

		public void OnRemoteUpgradeLogoff(DateTime upgradeDateTimeUtc, Func<bool> updateExists)
		{
			if (LastUpgradeDateTimeUtc == upgradeDateTimeUtc)
			{
				return;
			}

			OpenedFormCache.GetInstance().SaveAllFormsPositionAndSize();

			LastUpgradeDateTimeUtc = upgradeDateTimeUtc;
			var dateTimelocal = TimeFactory.Instance.GetLocalTimeFromUtc(upgradeDateTimeUtc);

			var msg = Res.GetString("C7A666F5-C431-4FF3-BE96-4F3776664120", @"The system is scheduled to upgrade on {0}. Please save all work in progress and log out. Any unsaved changes will be lost.", dateTimelocal);

			var dispatcher = ApplicationDispatcher.Current;
			if (dispatcher != null)
			{
				dispatcher.BeginInvoke(new MethodInvoker(() =>
				{
#if WINZOR
					if (ZApplication.GetOpenForms().Length == 0)
					{
						return;
					}
#endif
					using (Db.DisableSchemaVersionCheck())
					using (var message = new ZPreemptibleMessageBox(msg, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, updateExists, TimeSpan.FromMinutes(5)))
					{
						ZFormModaliser.ShowMessageBoxWithoutDispose(message);
					}
				}));
			}
		}

		DateTime LastUpgradeDateTimeUtc = DateTime.MinValue;
	}
}
