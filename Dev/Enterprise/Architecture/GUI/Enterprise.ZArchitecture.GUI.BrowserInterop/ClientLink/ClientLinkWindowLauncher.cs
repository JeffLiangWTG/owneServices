using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Authentication.Primitives;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.BrowserInterop.ClientLink;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json.Linq;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	class ClientLinkWindowLauncher : IBrowserInteropWindow, ISendMessageToBrowser
	{
		readonly string windowTitle;
		readonly Action<string> launchWebUrl;
		readonly Func<string, IHubConnectionAndProxy> getHubConnectionAndProxy;

		internal ClientLinkModal form;
		readonly string url;
		public event EventHandler ClientLinkLaunched;
		protected volatile LauncherState state;
		protected readonly IMessageTransportLayer clientLinkMessageHandler;
		protected IHubConnectionAndProxy hubConnectionAndProxy;

		public ClientLinkWindowLauncher(
			string windowTitle,
			Uri url,
			Action<string> launchWebUrl = null,
			Func<string, IHubConnectionAndProxy> getHubConnectionAndProxy = null)
		{
			this.windowTitle = windowTitle;
			this.url = url.ToString();
			this.launchWebUrl = launchWebUrl ?? (url => WebUrlLauncher.Launch(url));
			this.getHubConnectionAndProxy = getHubConnectionAndProxy ?? ((clientLinkId) => ClientLinkMethods.GetHubConnectionAndProxy(clientLinkId));
			clientLinkMessageHandler = new MessageTransportLayer(this);
		}

		public IMessageTransportLayer MessageTransportLayer => clientLinkMessageHandler;
		public void Close()
		{
			form.Activate();
			form.Close();
		}

		void SetStatusText(string text)
		{
			Dispatcher.Invoke(() => form.SetStatusText(text));
		}

		public bool? ShowDialog()
		{
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (form = new ClientLinkModal(windowTitle, () => SetStateAndCancelToken(LauncherState.DialogClosed, cancellationTokenSource)))
			{
				var clientLinkTask = LaunchClientLinkAsync(cancellationTokenSource.Token);
#if DEBUG
				TaskRegistryForTest.RegisterTask(clientLinkTask, nameof(LaunchClientLinkAsync));
#endif

				this.AddBrowserCloseCommandHandler(result =>
				{
					state = LauncherState.Completed;
					this.Close();
				});
				this.AddPeerDisconnectedCommandHandler(() =>
				{
					SetStateAndCancelToken(LauncherState.PeerDisconnected, cancellationTokenSource);
				});
				form.ShowDialog();
				if (state == LauncherState.Working)
				{
					state = LauncherState.DialogClosed;
				}
				cancellationTokenSource.Cancel();
#if DEBUG
				clientLinkTask.Wait();
#endif
			}
			return null;
		}

		internal async Task LaunchClientLinkAsync(CancellationToken cancellationToken)
		{
			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					var clientLinkId = await ClientLinkMethods.GetClientLinkIdAsync();
					hubConnectionAndProxy = getHubConnectionAndProxy(clientLinkId);
					using (HubProxyReceivedHandler(hubConnectionAndProxy.HubProxy))
					{
						state = LauncherState.Working;
						hubConnectionAndProxy.Closed += () =>
						{
							if (state == LauncherState.Working)
							{
								state = LauncherState.NetworkDisconnected;
							}
						};

						await hubConnectionAndProxy.Start();

						SetStatusText(Res.GetString("45d1c59a-7895-c69c-4196-402f0cf38dd9", "Connection to Glow established, launching browser..."));

						launchWebUrl(ClientLinkMethods.GetClientLinkConnectionUrl(url, clientLinkId));

						ClientLinkLaunched?.Invoke(this, null);

						while (state == LauncherState.Working)
						{
							if (cancellationToken.IsCancellationRequested)
							{
								break;
							}
							try
							{
								await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
							}
							catch (OperationCanceledException)
							{
								break;
							}
						}

						switch (state)
						{
							case LauncherState.DialogClosed:
							case LauncherState.Completed:
								await clientLinkMessageHandler.SendToBrowserAsync<JToken>(WebViewCommands.Completed, null).ConfigureAwait(false);
								break;
							case LauncherState.PeerDisconnected:
							case LauncherState.NetworkDisconnected:
								SetStatusText(ClientLinkMethods.GetConnectionLostMessage());
								break;
							default:
								throw new UnreachableCodeException($"The default block should not be reached, but it does now, because of the unexpected state '{state}'");
						}
					}
				}
			}
			catch (AuthorizationFailureException ex) when (ex.AuthenticationResult == AuthenticationResult.ThirdPartyUserValidationRequired)
			{
				SetStatusText(Res.GetString(
					"21f2d1d1-eadb-4415-ad04-a7fde06fa0f4",
					"The logged in user {0} requires an external login to access Glow, which is not supported by Client Link at this time.\r\n\r\nPlease try again with another user.",
					Env.CurrentUser.LoginName
				));
			}
			catch (Exception ex) when (ex is AuthorizationFailureException || ex is ClientLinkException || ex is InvalidOperationException)
			{
#pragma warning disable CW1161 // Res.GetString Analyzer
				SetStatusText("Error: " + ex.Message);
#pragma warning restore CW1161 // Res.GetString Analyzer
			}
			finally
			{
				hubConnectionAndProxy?.Dispose();
			}
		}

		void SetStateAndCancelToken(LauncherState newState, CancellationTokenSource cancellationTokenSource)
		{
			state = newState;
			cancellationTokenSource.Cancel();
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

		internal void HandleMessage(IList<JToken> tokens)
		{
			Dispatcher.Invoke(() =>
			{
				ClientLinkMethods.HandleMessage(MessageTransportLayer, tokens);
			});
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

		[ThreadSafe]
		public static SynchronizationContext DispatcherForTest = null;

		static SynchronizationContext Dispatcher
		{
			get
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return DispatcherForTest;
				}
#endif
				return ApplicationDispatcher.Current;
			}
		}
	}
}
