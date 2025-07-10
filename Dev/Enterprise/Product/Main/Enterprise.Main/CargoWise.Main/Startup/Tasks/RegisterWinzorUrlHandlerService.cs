using System;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Main;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Microsoft.JSInterop;

namespace Enterprise.Startup
{
	class RegisterWinzorUrlHandlerService : IPostLoginTask
	{
		public string TaskDescription => Res.GetString("1F2220DF-EFD6-4D7D-A0D6-0294018812FF", "Registering Winzor URL Handler Service");

		public void Execute()
		{
			var urlHandlerInstanceInfo = new UrlHandlerInstanceInfo
			{
				LicenceKeyIdentifier = ((IGlbCompany)Env.CurrentCompany).LicenceKeyIdentifier,
				Domain = InstanceDetails.Current?.Domain,
				Instance = InstanceDetails.Current?.Instance
			};

			StartupOpenMainFormTask.MainFormInstance.RegisterAfterRenderAction(() =>
			{
				var windowService = StartupOpenMainFormTask.MainFormInstance.CargoWiseClientServices!.WindowService;
				return windowService.RegisterUrlHandlerAsync(urlHandlerInstanceInfo, ExecuteUrlAsync);
			});
		}

		[JSInvokable]
		async Task<bool> ExecuteUrlAsync(string url)
		{
			var result = new TaskCompletionSource<bool>();
			await StartupOpenMainFormTask.MainFormInstance.InvokeWinzorDispatcherAsync(() =>
			{
				try
				{
					result.SetResult(EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false));
				}
				catch (EnterpriseUrlHandlerException)
				{
					result.SetResult(false);
					// Ignore this exception for consistency with CW1.
				}
				catch (Exception ex)
				{
					result.SetResult(false);
					ExceptionReporter.Instance.HandleUnhandledException(ex);
				}
			});
			return await result.Task;
		}

		public bool ShouldExecute()
		{
			return true;
		}
	}
}
