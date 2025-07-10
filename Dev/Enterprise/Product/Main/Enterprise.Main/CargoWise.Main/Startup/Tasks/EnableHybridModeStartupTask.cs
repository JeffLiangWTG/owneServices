using System;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.BlazorWinFormsInterop;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Core;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class EnableHybridModeStartupTask : IPostLoginTask
	{
		public EnableHybridModeStartupTask()
			: this(ObjectFactory.Get<IWinFormsListener>())
		{
		}

		public EnableHybridModeStartupTask(IWinFormsListener winFormsListener)
		{
			this.winFormsListener = winFormsListener;
		}

		public string TaskDescription => Res.GetString("875438aa-9775-468e-9f32-cbe03c4e18ef", "Updating CargoWise Client");

		public bool ShouldExecute()
		{
			using (Db.DisposableActionForDbConnection())
			{
				return GlbStaff.CurrentUser.CheckWinzorEnabledForUser();
			}
		}

		public void Execute()
		{
			// Retrieving this value here makes testing setup much easier as we're still on the same thread as the test
			var mainFormEnabled = GlbStaff.CurrentUser.CheckFeatureEnabledForUser(StmFeatureTest.WinzorMainFormFeatureCode);
			var webVersionEnabled = GlbStaff.CurrentUser.CheckFeatureEnabledForUser(StmFeatureTest.WebVersion);
			Task = Task.Run(() => DoExecute(mainFormEnabled, webVersionEnabled));
			BackgroundApplicationStartupTask.RegisterAdditionalTask(Task);
		}

		void DoExecute(bool mainFormEnabledForUser, bool webVersionEnabled)
		{
			if (RunClientLocally() || EnsureClientApplicationInstalledOnRemoteClient())
			{
				using (Db.DisposableActionForDbConnection())
				{
					if (mainFormEnabledForUser)
					{
						Uri blazorUrl = new Uri(Env.Registry.BlazorUrl);
						string winzorUri = blazorUrl.GetLeftPart(UriPartial.Authority);

						ObjectFactory.Get<IBlazorClientAppLauncher>().Launch(new Uri(winzorUri));
					}
					else
					{
						if (!webVersionEnabled)
						{
							winFormsListener.Initialise();
						}
					}
				}
			}
		}

		// Developers on their local PC, whether using it directly or remote desktopping in to it, should run locally.
		// Customers NOT using RemoteApp is not a supported scenario for hybrid mode.
		// In either of the above cases we don't install the client app.
		bool RunClientLocally()
		{
			var terminalService = ObjectFactory.Get<TerminalService>();
			bool isRemoteSession = terminalService.IsRemoteAppSession && (terminalService.IsWTSSession || terminalService.IsCitrixICA);
			return !isRemoteSession;
		}

		bool EnsureClientApplicationInstalledOnRemoteClient()
		{
			//Temperary remove Hybrid mode support for now,
			// adding it back from: WI00745433 - Hybrid-mode installer can handle MSIX
			return false;
		}

		public Task Task
		{
			get;
			private set;
		}

		readonly IWinFormsListener winFormsListener;
	}
}
