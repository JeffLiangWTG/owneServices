using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Authentication.Primitives;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.BrowserInterop.ClientLink;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json.Linq;
using WinzorFramework;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	class ClientLinkWinzorLauncher : IBrowserInteropWindow, ISendMessageToBrowser
	{
		readonly Form form;
		protected readonly string url;
		protected volatile LauncherState state;
		protected readonly IMessageTransportLayer clientLinkMessageHandler;
		protected IHubConnectionAndProxy hubConnectionAndProxy;

		public ClientLinkWinzorLauncher(Uri url)
		{
			this.url = url.ToString();
			form = WinzorDispatcher.Current.CurrentContext.Form;
			clientLinkMessageHandler = new MessageTransportLayer(this);
		}

		void SetStatusText(string text)
		{
			form.InvokeWinzorDispatcherAsync(() => Globals.Message.ShowError(text)).GetAwaiter().GetResult();
		}

		public IMessageTransportLayer MessageTransportLayer => clientLinkMessageHandler;

		// Winzor Client Link form closing is taken care by the client app
		public void Close() { }

		public bool? ShowDialog()
		{
			using var dialogCts = new CancellationTokenSource();
			_ = Task.Run(() => LaunchClientLinkAsync(dialogCts), dialogCts.Token);

			this.AddBrowserCloseCommandHandler(result =>
			{
				state = LauncherState.Completed;
			});
			this.AddPeerDisconnectedCommandHandler(() =>
			{
				state = LauncherState.PeerDisconnected;
			});
			this.AddBrowserReadyCommandHandler(() =>
			{
				state = LauncherState.Working;
			});

			WinzorDispatcher.Current.RunMessageLoop(dialogCts);
			return null;
		}

		internal async Task LaunchClientLinkAsync(CancellationTokenSource dialogCts)
		{
			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					var clientLinkId = await ClientLinkMethods.GetClientLinkIdAsync();
					hubConnectionAndProxy = ClientLinkMethods.GetHubConnectionAndProxy(clientLinkId);
					using (HubProxyReceivedHandler(hubConnectionAndProxy.HubProxy))
					{
						state = LauncherState.Connecting;
						hubConnectionAndProxy.Closed += () =>
						{
							if (state == LauncherState.Working)
							{
								state = LauncherState.NetworkDisconnected;
							}
						};

						await hubConnectionAndProxy.Start();

						var clientLinkUri = new Uri(ClientLinkMethods.GetClientLinkConnectionUrl(url, clientLinkId));
						await form.CargoWiseClientServices.WindowService.RequestRestrictedWindowOpenAsync(clientLinkUri,
							() =>
							{
								state = LauncherState.DialogClosed;
								return Task.CompletedTask;
							});

						var timeout = TimeSpan.FromMinutes(5);
						var stopwatch = new Stopwatch();
						stopwatch.Start();

						while (state == LauncherState.Working || state == LauncherState.Connecting)
						{
							await Task.Delay(TimeSpan.FromMilliseconds(500));
							// Connecting state is when the system sends a loading restricted webview2 request until the modal view is ready.
							// If there are any issues, that cannot make the modal view ready, system should timeout the request.
							// Working is when the modal view is ready and user is filtering to get correct data, we should not timeout here, let user take as much time as they wants
							if (stopwatch.Elapsed >= timeout && state == LauncherState.Connecting)
							{
								await form.CargoWiseClientServices.WindowService.RequestRestrictedFormCloseAsync();
								SetStatusText(ClientLinkMethods.GetConnectionLostMessage());
								break;
							}
						}

						switch (state)
						{
							case LauncherState.DialogClosed:
							case LauncherState.Completed:
								await clientLinkMessageHandler.SendToBrowserAsync<JToken>(WebViewCommands.Completed, null).ConfigureAwait(false);
								break;
						}
					}
				}
			}
			catch (Exception ex) when (ex is AuthorizationFailureException || ex is ClientLinkException || ex is InvalidOperationException)
			{
				SetStatusText(ex.Message);
			}
			finally
			{
				hubConnectionAndProxy?.Dispose();
				dialogCts.Cancel();
			}
		}

		void HandleMessage(IList<JToken> tokens)
		{
			form.InvokeWinzorDispatcherAsync(() => ClientLinkMethods.HandleMessage(clientLinkMessageHandler, tokens));
		}

		// ASP.NET SignalR Hubs API: https://learn.microsoft.com/en-us/aspnet/signalr/overview/guide-to-the-api/hubs-api-guide-net-client
		// It would be better to use `hubProxy.On<BrowserMessageEventArgs<JToken>>("handleMessage", action)` instead of converting it manually,
		// but it seems the Newtonsoft used by SignalR is different, and it does not respect the [JsonProperty] attributes in `BrowserMessageEventArgs`
		// Similarly, `token.ToObject<BrowserMessageEventArgs<JToken>>` does not work either.
		IDisposable HubProxyReceivedHandler(IHubProxy proxy)
		{
			var subscription = proxy.Subscribe("handleMessage");
			subscription.Received += HandleMessage;
			return new DisposableAction(() => subscription.Received -= HandleMessage);
		}

		async Task ISendMessageToBrowser.SendToBrowserAsync(string message)
		{
			try
			{
				await ClientLinkMethods.SendToBrowserAsync(hubConnectionAndProxy, message);
			}
			catch (AggregateException ae)
			{
				foreach (var e in ae.InnerExceptions)
				{
					if (e is InvalidOperationException)
					{
						SetStatusText(ClientLinkMethods.GetConnectionLostMessage());
					}
				}
			}
		}
	}
}
