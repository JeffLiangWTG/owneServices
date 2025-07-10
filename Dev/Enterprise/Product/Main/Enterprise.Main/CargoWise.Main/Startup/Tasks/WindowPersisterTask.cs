using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
#if WINZOR
using WinzorFramework;
#endif
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class WindowPersisterTask : IPostLoginTask
	{
		public string TaskDescription
		{
			get { return Res.GetString("82e07763-bee1-493d-a58f-3682ab8c7406", "Reopening windows and forms"); }
		}

		public void Execute()
		{
			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var persistArgument = CommandLineArguments.UsedToLaunchApplication?[ApplicationArguments.OptionPersist] as string;
					if (!string.IsNullOrEmpty(persistArgument))
					{
						if (!Env.Registry.RememberOpenedFormsAfterUpgrade ||
							Globals.Message.Show(Res.GetString("AB09D635-A27F-4E2C-9548-A7A2BF1D5958", "Do you want to pick up forms where left?"),
							Res.GetString("93522643-7F90-447D-908A-C3ABAF141E30", "Reopen Forms"),
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Information) == DialogResult.Yes)
						{
							Run(() =>
							{
								WindowPersister.OpenFormsFromUrls(persistArgument);
							});
						}
					}

					Run(() =>
					{
						WindowPersister.LoadUrlsFromRegistry();
					});
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ExceptionReporter.Instance.HandleUnhandledException(ex);
				}
			}
		}

		void Run(Action action)
		{
#if WINZOR
			WinzorDispatcher.Current.InvokeAsync(() =>
			{
				using (WinzorDispatcher.Current.WithContext(new ServerInitiatedCallbackContext(WinzorDispatcher.Current.CurrentContext)))
				{
					action.Invoke();
				}
			});
			WinzorDispatcher.Current.DoEvents();
#else
			Task.Run(() =>
			{
				action.Invoke();
			});
#endif
		}

		public bool ShouldExecute()
		{
#if DEBUG
			return !Globals.IsTest;
#else
			return true;
#endif
		}
	}
}
