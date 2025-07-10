using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using CargoWise.Common;
using Enterprise.RemotePrinting.Types;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace Enterprise.RemotePrinting.Client
{
	public class HubConnectionManager : IDisposable
	{
		readonly ConnectionRegistryManager connectionRegistryManager;
		readonly HubClientController clientController;
		readonly List<IClientTransport> transports;

		public event EventHandler<LogEventArgs> LogInformation;
		public event EventHandler<LogEventArgs> LogErrorMessage;

		WebClientConfiguration connectionConfig;
		RemoteHubProxy hubProxy;
		PrintHubConnection connection;
		readonly IErrorResponseWebRequestProcessor responseProcessor;

		public HubConnectionManager(ConnectionRegistryManager connectionRegistryManager, HubClientController clientController, IErrorResponseWebRequestProcessor responseProcessor)
		{
			this.connectionRegistryManager = connectionRegistryManager;
			this.clientController = clientController;

			var autoTransportHttpClient = new PrintDefaultHttpClient();
			transports = new List<IClientTransport>()
			{
				new AutoTransport(autoTransportHttpClient, new List<IClientTransport>()
				{
					new PrintWebSocketTransport(autoTransportHttpClient),
					new ServerSentEventsTransport(autoTransportHttpClient),
					new LongPollingTransport(autoTransportHttpClient)
				}),
				new PrintWebSocketTransport(new PrintDefaultHttpClient()),
				new ServerSentEventsTransport(new PrintDefaultHttpClient()),
				new LongPollingTransport(new PrintDefaultHttpClient())
			};

			this.responseProcessor = responseProcessor;
		}

		public void Start(string configName)
		{
			var config = connectionRegistryManager.GetWebClientConfiguration(configName);
			if (!string.IsNullOrEmpty(responseProcessor.ServiceUrl) && !string.Equals(config.WebServiceUrl, responseProcessor.ServiceUrl, StringComparison.OrdinalIgnoreCase))
			{
				config.WebServiceUrl = responseProcessor.ServiceUrl;
			}
			Start(config);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Log Info")]
		void Start(WebClientConfiguration config)
		{
			Exception lastEx = null;
			LogInfo("Connecting to hub.");
			for (int i = 0; i < transports.Count; i++)
			{
				var transportType = transports[i].GetType().Name;

				MethodDelegate action = () =>
				{
					LogInfo("Connecting with transport: " + transportType);

					UnhookConnection();
					connection = CreateHubConnection(config);
					connectionConfig = config;
					HookConnection();

					UnhookHubProxy();
					hubProxy = new RemoteHubProxy(connection, config);
					HookHubProxy();
					hubProxy.RegisterClient(clientController);

					connection.Start(transports[i]).Wait();
					clientController.Start(hubProxy);

					LogInfo("Registered on hub and awaiting commands.");
					return true;
				};

				try
				{
					var success = responseProcessor.Process(action, out var retryAction, out _);
					if (!success && retryAction != null && retryAction != action)
					{
						responseProcessor.Process(retryAction, out _, out _);
					}
					return;
				}
				catch (Exception ex)
				{
					LogInfo(transportType + " failed: " + ex.Message);
					lastEx = ex;
				}
			}

			// If we are here - it did not connect
			throw new ApplicationException("Could not connect to SignalR", lastEx);
		}

#if DEBUG
		protected
#endif
		PrintHubConnection CreateHubConnection(WebClientConfiguration config)
		{
			const string LegacyUrlPostfix = "/RemotePrintingService.asmx";

			var serviceUrl = string.IsNullOrEmpty(this.responseProcessor.ServiceUrl) ? config.WebServiceUrl : this.responseProcessor.ServiceUrl;
			var url = serviceUrl.TrimEnd('/');
			if (url.EndsWith(LegacyUrlPostfix, StringComparison.OrdinalIgnoreCase))
			{
				url = url.Substring(0, url.Length - LegacyUrlPostfix.Length) + "/SignalR";
			}

			var newConnection = NewHubConnectionCore(url);
			newConnection.Credentials = new NetworkCredential(config.WebServiceUser, ProtectedDataHelper.Unprotect(config.WebServicePwd));

			// ServerTimeout property is only available in .Net Core SignalR version. Perhaps TransportConnectTimeout will do same.
			var timeout = TimeSpan.FromSeconds(config.RemotePrintingServiceTimeoutInSeconds);
			newConnection.TransportConnectTimeout = timeout;
			newConnection.DeadlockErrorTimeout = timeout;

			var proxy = Configurator.GetProxy(config);
			if (proxy != null)
			{
				newConnection.Proxy = proxy;
			}

			return newConnection;
		}

		protected virtual PrintHubConnection NewHubConnectionCore(string url)
		{
			return new PrintHubConnection(url);
		}

		#region Connection and HubProxy events

		void HookHubProxy()
		{
			UnhookHubProxy();
			if (hubProxy != null)
			{
				hubProxy.LogInfo += LogInfo;
			}
		}

		void UnhookHubProxy()
		{
			if (hubProxy != null)
			{
				hubProxy.LogInfo -= LogInfo;
			}
		}

		void HookConnection()
		{
			UnhookConnection();
			if (connection != null)
			{
#if DEBUG
				connection.Received += Connection_Received;
#endif

				connection.StateChanged += Connection_StateChanged;
				connection.Error += Connection_Error;
				connection.Logged += LogInfo;
			}
		}

		void UnhookConnection()
		{
			if (connection != null)
			{
#if DEBUG
				connection.Received -= Connection_Received;
#endif

				connection.StateChanged -= Connection_StateChanged;
				connection.Error -= Connection_Error;
				connection.Logged -= LogInfo;
			}
		}

#if DEBUG
		void Connection_Received(string body)
		{
			LogInfo($"Received Message: {body.Substring(0, Math.Min(body.Length, 100))}...");
		}
#endif

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "LogInfo")]
		void Connection_StateChanged(StateChange change)
		{
			switch (change.NewState)
			{
				case ConnectionState.Connected:
					LogInfo("Hub connection established.");
					return;

				case ConnectionState.Reconnecting:
					LogInfo("Attempting to reconnect to hub.");
					return;

				case ConnectionState.Disconnected:
					if (WasPreviouslyConnected(change.OldState))
					{
						ForceRestart(connectionConfig);
					}

					return;
			}
		}

		void Connection_Error(Exception ex)
		{
			HandleConnectionError(ex, connectionConfig.EnableVerboseLogging);
		}

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Append")]
#if DEBUG
		protected
#endif
		void HandleConnectionError(Exception ex, bool verbose)
		{
			if (verbose)
			{
				var sb = new StringBuilder();
				sb.Append("SignalR connection error: ");
				ErrorReporter.FillExceptionMessageAndStacktrace(ex, sb);
				LogError(sb.ToString());
			}
			else
			{
				LogError("SignalR connection error: " + ErrorReporter.GetExceptionMessage(ex));
			}
		}

		static bool WasPreviouslyConnected(ConnectionState oldState)
			=> oldState == ConnectionState.Connected || oldState == ConnectionState.Reconnecting;

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "LogInfo")]
		protected void ForceRestart(WebClientConfiguration config)
		{
			if (isDisposed)
			{
				return;
			}

			LogInfo("Hub connection has been dropped and could not be reestablished. Please check your network connection & server.");
			LogInfo("This client will attempt to reconnect to the hub periodically.");

			if (forceReconnectionTimer != null)
			{
				forceReconnectionTimer.Dispose();
			}

			while (!isDisposed)
			{
				try
				{
					DisposeConnectionAndHubProxy();

					if (ShouldPauseSignalRConnectionForAWhile(config))
					{
						LogInfo(string.Format(CultureInfo.InvariantCulture, "Too many frequent connection attempts to SignalR Hub. Print Nudging will be suspended for {0} minutes from now on.", config.PauseSignalRMinutes));
						CreateTimerToPauseSignalRConnectionForAWhile(config);

						return;
					}

					Start(config);
					return;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					LogError(string.Format(CultureInfo.InvariantCulture, "Hub reconnection attempt has failed with message '{0}'", ErrorReporter.GetExceptionMessage(ex)));
					Thread.Sleep(10_000);
				}
			}
		}

		protected virtual void CreateTimerToPauseSignalRConnectionForAWhile(WebClientConfiguration config)
		{
			forceReconnectionTimer = new Timer(_ => ForceRestart(config), null, TimeSpan.FromMinutes(config.PauseSignalRMinutes), Timeout.InfiniteTimeSpan);
		}

		protected virtual bool ShouldPauseSignalRConnectionForAWhile(WebClientConfiguration config)
		{
			var now = DateTime.UtcNow;
			ReconnectionTimestamps.RemoveAll(t => (now - t).TotalMinutes > config.ReconnectionLimitMinutes);

			if (ReconnectionTimestamps.Count > config.ReconnectionAttempts)
			{
				return true;
			}

			ReconnectionTimestamps.Add(now);
			return false;
		}

		readonly List<DateTime> ReconnectionTimestamps = new List<DateTime>();
		Timer forceReconnectionTimer;

		public void Dispose()
		{
			isDisposed = true;

			DisposeConnectionAndHubProxy();
		}

		bool isDisposed;

		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Proxy is disposed. Analyser doesn't recognise that the null coelescance returns the same object.")]
		void DisposeConnectionAndHubProxy()
		{
			UnhookHubProxy();
			hubProxy?.Dispose();
			hubProxy = null;

			UnhookConnection();
			connection?.Dispose();
			connection = null;
		}

		void LogInfo(string message)
			=> LogInformation?.Invoke(this, new LogEventArgs(message));

		void LogError(string message)
			=> LogErrorMessage?.Invoke(this, new LogEventArgs(message));

		bool IsConnected => !isDisposed && ((hubProxy?.Connection?.State ?? ConnectionState.Disconnected) == ConnectionState.Connected);

		public void UpdatePrinters(string name, string[] printers)
		{
			if (IsConnected)
			{
				hubProxy?.UpdatePrinters(name, printers);
			}
		}
	}
}
