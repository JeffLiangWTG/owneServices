using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.GUI.ExternalTariffApplicationLauncher;
using Enterprise.MasterFiles.Business;
using Websocket.Client;
using Websocket.Client.Models;

namespace Enterprise.Customs.Common.GUI
{
	public sealed class BorderWiseWebSocketClient : IBorderWiseWebSocketClient
	{
		readonly string tariffClassificationMode;
		readonly string orgCode;
		readonly string countryCode;
		readonly string cargoWiseClientId;
		readonly int cargoWiseDatabaseNumber;
		readonly TimeSpan maxBackoffSeconds = new TimeSpan(0, 0, 30);
		readonly TimeSpan webSocketConnectionTimeout = new TimeSpan(0, 30, 0);
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "WebSocket message")]
		readonly string webSocketConnectedMessage = "CargoWiseOne WebSocket connected.";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "WebSocket message")]
		readonly string webSocketUnderProcessMessage = "Tariff classification is under process in CW1.";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error message")]
		readonly string webSocketProcessingErrorMessage = "Unable to process received message Data";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Authentication")]
		readonly string authentication = "Authentication";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "LicenseCode")]
		readonly string licenseCode = "LicenseCode";
		readonly SemaphoreSlim ensureConnectionLock = new(1, 1);

		int failureCount;
		bool isReconnectInProgress;
		bool hasAlreadyConnectedToHub;
		IDisposable reconnectionSubscription;
		IDisposable messageSubscription;
		IDisposable disconnectionSubscription;
		CancellationTokenSource reconnectCts;

		public BorderWiseWebSocketClient(ZGuid webSocketClientId, string orgCode, string countryCode, string cargoWiseClientId, int cargoWiseDatabaseNumber)
		{
			tariffClassificationMode = TariffClassificationMode.Single;
			this.orgCode = orgCode;
			this.countryCode = countryCode;
			this.cargoWiseClientId = cargoWiseClientId;
			this.cargoWiseDatabaseNumber = cargoWiseDatabaseNumber;
			WebSocketClientId = webSocketClientId;
		}

		public BorderWiseWebSocketClient(ZGuid webSocketClientId, string orgCode, string countryCode, string cargoWiseClientId, int cargoWiseDatabaseNumber, BorderWiseTariffExchangeModelV2 borderWiseTariffExchangeModelV2)
		{
			tariffClassificationMode = TariffClassificationMode.Batch;
			this.orgCode = orgCode;
			this.countryCode = countryCode;
			this.cargoWiseClientId = cargoWiseClientId;
			this.cargoWiseDatabaseNumber = cargoWiseDatabaseNumber;
			WebSocketClientId = webSocketClientId;
			BorderWiseTariffExchangeModelV2 = borderWiseTariffExchangeModelV2;
		}

		public event WebsocketDataReceivedEventHandler WebSocketDataReceived;

		public BorderWiseTariffExchangeModelV2 BorderWiseTariffExchangeModelV2 { get; set; }

		public bool IsMessageReceived { get; set; }

		public string JobStatusInBW { get; set; }

		public IWebsocketClient WebSocketClient { get; set; }

		public Task WebSocketClientDisposeDelayTask { get; set; }

		public ZGuid WebSocketClientId { get; set; }

		public void ClearWebSocketDataReceivedEvent() => WebSocketDataReceived = null;

		public void DisposeWebSocketClient()
		{
			ResetReConnectToBwwHub();

			if (WebSocketClient == null)
			{
				return;
			}

			IsMessageReceived = false;
			WebSocketClient.Dispose();
			WebSocketClient = null;
		}

		public bool SendMessage(BorderWiseWebSocketMessageStatus status,
			string data,
			string mode = "Single",
			string borderWiseWebSocketMessageType = null,
			Guid? clientId = null,
			int maxConnectionWaitRetries = 10,
			int connectionWaitRetryDelayMs = 300)
		{
			var retryCount = 0;
			var isConnected = false;

			// Retry logic
			while (retryCount < maxConnectionWaitRetries && !isConnected)
			{
				if (WebSocketClient?.IsRunning == true)
				{
					isConnected = true;
				}
				else
				{
					retryCount++;
					Task.Delay(connectionWaitRetryDelayMs).Wait(); // Wait before retrying
				}
			}

			if (!isConnected)
			{
				ErrorReporter.ReportOnce($"Unable to send message. WebSocket connection could not be established after multiple retry attempts.");
				return false;
			}

			// Construct the message
			var borderWiseWebSocketMessage = new BorderWiseExchangeMessage
			{
				ClientId = clientId ?? WebSocketClientId.ToGuid(),
				CargoWiseClientId = cargoWiseClientId,
				SystemNameRecipient = status == BorderWiseWebSocketMessageStatus.STA ? "BWW.API" : "BorderWiseWeb",
				SystemNameSender = "CargoWiseOne",
				Data = data,
				Status = status,
				Mode = mode,
				Email = Environment.Env.CurrentUser?.EmailAddress,
				OrgCode = orgCode,
				CountryCode = countryCode,
				CargoWiseDatabaseNumber = cargoWiseDatabaseNumber
			};

			if (!string.IsNullOrEmpty(borderWiseWebSocketMessageType))
			{
				borderWiseWebSocketMessage.Type = borderWiseWebSocketMessageType;
			}

			// Serialize the message to JSON
			var message = Utilities.SerializeToJson(borderWiseWebSocketMessage);

			// Send the message
			WebSocketClient.Send(message);
			return true;
		}

		public void SendMessageAndDisposeWebSocketClient(
			BorderWiseWebSocketMessageStatus borderWiseWebSocketMessageStatus,
			string data,
			bool isDispose = true,
			string mode = "Single",
			bool sendMessage = true)
		{
			if (WebSocketClient == null)
			{
				return;
			}

			WebSocketClientDisposeDelayTask = Task.Run(async () =>
			{
				try
				{
					if (sendMessage)
					{
						SendMessage(borderWiseWebSocketMessageStatus, data, mode);
					}

					JobStatusInBW = string.Empty;
					IsMessageReceived = false;
				}
				finally
				{
					if (sendMessage)
					{
						await Task.Delay(500);
					}

					if (isDispose)
					{
						DisposeWebSocketClient();
					}
				}
			});
		}

		public void EnsureWebSocketConnection()
		{
			_ = Task.Run(async () =>
			{
				if (WebSocketClient?.IsRunning == true)
				{
					return;
				}

				ensureConnectionLock.Wait();

				try
				{
					if (reconnectCts != null || isReconnectInProgress)
					{
						ResetReConnectToBwwHub();
						WebSocketClient?.Dispose();
						WebSocketClient = null;
					}

					await ConnectToBwwHub();
				}
				finally
				{
					ensureConnectionLock.Release();
				}
			});
#if DEBUG
			Thread.Sleep(2000);
#endif
		}

		async Task ConnectToBwwHub(bool cancelReconnect = false)
		{
			try
			{
				if (WebSocketClientId == ZGuid.Empty)
				{
					WebSocketClientId = ZGuid.NewZGuid();
				}

				if (WebSocketClient == null)
				{
					WebSocketClient = CreateWebSocketClient();
				}

				ConfigureWebSocketClient();

				await WebSocketClient.Start();
			}
			catch (Exception ex)
			{
				HandleConnectionException(ex);
			}
		}

		IWebsocketClient CreateWebSocketClient()
		{
			var url = new Uri(ZArchitecture.Environment.DataRegistry.Instance.BorderWiseWebSocketHubUrl);
			var factory = new Func<ClientWebSocket>(() =>
			{
				var client = new ClientWebSocket
				{
					Options = { KeepAliveInterval = TimeSpan.FromSeconds(5) }
				};

				var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(ZArchitecture.Environment.DataRegistry.Instance.BorderWiseApiKey));
				var licenseCode = GlbCompany.CurrentCompany.GetLicenceKeyIdentifier(string.Empty);
				client.Options.SetRequestHeader(authentication, Utilities.ComputeSha256Hash(decodedString + licenseCode));
				client.Options.SetRequestHeader(this.licenseCode, licenseCode);

				return client;
			});

			return new WebsocketClient(url, factory);
		}

		void ConfigureWebSocketClient()
		{
			reconnectionSubscription?.Dispose();
			messageSubscription?.Dispose();
			disconnectionSubscription?.Dispose();

			WebSocketClient.IsReconnectionEnabled = false;
			reconnectionSubscription = WebSocketClient.ReconnectionHappened.Subscribe(OnReconnectionHappened);
			messageSubscription = WebSocketClient.MessageReceived.Subscribe(OnMessageReceived);
			disconnectionSubscription = WebSocketClient.DisconnectionHappened.Subscribe(OnDisconnectionHappened);
		}

		void OnReconnectionHappened(ReconnectionInfo type)
		{
			failureCount = 0;
			isReconnectInProgress = false;

			var data = tariffClassificationMode == TariffClassificationMode.Batch
				? Utilities.SerializeToJson(new BorderWiseTariffExchangeModelV2
				{
					JobPk = BorderWiseTariffExchangeModelV2?.JobPk ?? Guid.Empty,
					JobNumber = BorderWiseTariffExchangeModelV2?.JobNumber
				})
				: null;

			SendMessage(BorderWiseWebSocketMessageStatus.STA, data, tariffClassificationMode);
		}

		void OnMessageReceived(ResponseMessage msg)
		{
			try
			{
				var response = Utilities.DeserializeFromJson<BorderWiseExchangeMessage>(msg.Text);
				if (response == null)
				{
					return;
				}

				HandleMessageResponse(response);
			}
			catch
			{
				SendMessageAndDisposeWebSocketClient(BorderWiseWebSocketMessageStatus.ERR, webSocketProcessingErrorMessage, tariffClassificationMode == TariffClassificationMode.Single, tariffClassificationMode);
			}
		}

		void OnDisconnectionHappened(DisconnectionInfo info)
		{
			if (info.Type == DisconnectionType.Exit || info.Type == DisconnectionType.ByServer || info.Type == DisconnectionType.ByUser)
			{
				return;
			}

			failureCount++;
			isReconnectInProgress = false;
			_ = ReConnectToBwwHub();
		}

		async Task ReConnectToBwwHub()
		{
			if (isReconnectInProgress)
			{
				return;
			}

			isReconnectInProgress = true;
			reconnectCts = new();
			var token = reconnectCts.Token;
			var random = new Random();
			var startTime = ZDateTime.UtcNow;
			Exception lastException = null;

			try
			{
				while (!token.IsCancellationRequested)
				{
					if (ZDateTime.UtcNow - startTime > webSocketConnectionTimeout)
					{
						ErrorReporter.ReportOnce($"Failed to connect to Bww hub, exception: {lastException?.InnerException}");
						ResetReConnectToBwwHub();
						break;
					}

					var backoffBase = Math.Pow(1.3, Math.Min(failureCount, 20));
					var jitter = random.NextDouble() * 0.5 + 0.75;

					var backoffSeconds = backoffBase * jitter;
					var delaySeconds = Math.Min(backoffSeconds, maxBackoffSeconds.TotalSeconds);// 30 seconds
					var delay = TimeSpan.FromSeconds(delaySeconds);

					await Task.Delay(delay, reconnectCts.Token);

					try
					{
						WebSocketClient?.Dispose();
						WebSocketClient = null;

						await ConnectToBwwHub();
						break;
					}
					catch (WebSocketException ex)
					{
						lastException = ex;
						failureCount++;
					}
				}
			}
			catch (OperationCanceledException)
			{
			}
			finally
			{
				isReconnectInProgress = false;
			}
		}

		void ResetReConnectToBwwHub()
		{
			reconnectCts?.Cancel();
			failureCount = 0;
			isReconnectInProgress = false;
		}

		void HandleMessageResponse(BorderWiseExchangeMessage response)
		{
			var responseData = DeserializeData(response.Data);

			if (response.Status == BorderWiseWebSocketMessageStatus.ACK)
			{
				HandleAcknowledgement(response, responseData);
			}
			else if (response.Status == BorderWiseWebSocketMessageStatus.MSG || response.Status == BorderWiseWebSocketMessageStatus.FIN)
			{
				HandleMessage(response, responseData);
			}
		}

		void HandleAcknowledgement(BorderWiseExchangeMessage response, BorderWiseTariffExchangeModelV2 responseData)
		{
			if (responseData == null || hasAlreadyConnectedToHub || response.Mode != TariffClassificationMode.Batch)
			{
				return;
			}

			var responseJobStatus = responseData.JobStatus;
			var responseMessage = responseData.Message;

			if ((responseJobStatus == TariffClassificationJobStatus.Completed ||
				(responseJobStatus == TariffClassificationJobStatus.ProcessingInCW1 &&
				string.Equals(responseData.CurrentlyEditedBy, Environment.Env.CurrentUser?.EmailAddress, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(responseMessage, webSocketConnectedMessage, StringComparison.OrdinalIgnoreCase))))
			{
				var data = new BorderWiseTariffExchangeModelV2
				{
					JobPk = responseData.JobPk,
					JobNumber = responseData.JobNumber,
					JobStatus = TariffClassificationJobStatus.Completed
				};
				SendMessage(BorderWiseWebSocketMessageStatus.MSG, Utilities.SerializeToJson(data), tariffClassificationMode);
			}
			else if (responseJobStatus == TariffClassificationJobStatus.NotFound && BorderWiseTariffExchangeModelV2.BorderWiseInvoiceLines.Any())
			{
				SendMessage(BorderWiseWebSocketMessageStatus.MSG, Utilities.SerializeToJson(BorderWiseTariffExchangeModelV2), tariffClassificationMode);
			}
			else if (responseJobStatus == TariffClassificationJobStatus.New ||
					 responseJobStatus == TariffClassificationJobStatus.Working ||
					 responseJobStatus == TariffClassificationJobStatus.Edited ||
					 (responseJobStatus == TariffClassificationJobStatus.ProcessingInCW1 &&
					  responseMessage?.ToLowerInvariant() != webSocketUnderProcessMessage.ToLowerInvariant()))
			{
				OnWebSocketDataReceived(new WebSocketDataReceivedEventArgs(response.ClientId, null, responseData.JobPk, responseData.JobStatus));
			}

			if (string.Equals(responseMessage, webSocketConnectedMessage, StringComparison.OrdinalIgnoreCase))
			{
				hasAlreadyConnectedToHub = true;
			}
		}

		void HandleMessage(BorderWiseExchangeMessage response, BorderWiseTariffExchangeModelV2 responseData)
		{
			if (response.Status == BorderWiseWebSocketMessageStatus.MSG && response.Type == nameof(BorderWiseTariffExchangeModelV2))
			{
				if (response.Mode == TariffClassificationMode.Single)
				{
					OnWebSocketDataReceived(new WebSocketDataReceivedEventArgs(response.ClientId, responseData?.BorderWiseInvoiceLines));
				}
				else if (response.Mode == TariffClassificationMode.Batch)
				{
					IsMessageReceived = true;
					OnWebSocketDataReceived(new WebSocketDataReceivedEventArgs(response.ClientId, responseData?.BorderWiseInvoiceLines, responseData.JobPk, responseData?.JobStatus));
				}
			}
			else
			{
				ErrorReporter.ReportOnce($"Unable to process received message Data with BorderWiseWebSocketMessageType {response.Type}");
			}
		}

		void HandleConnectionException(Exception ex)
		{
			if (!(ex.InnerException is TaskCanceledException || ex is OperationCanceledException))
			{
				ErrorReporter.ReportOnce($"Failed to communicate to Bww hub, exception: {ex.InnerException}");
			}
		}

		void OnWebSocketDataReceived(WebSocketDataReceivedEventArgs e)
		{
			WebSocketDataReceived?.Invoke(this, e);
		}

		BorderWiseTariffExchangeModelV2 DeserializeData(string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				return null;
			}

			try
			{
				return Utilities.DeserializeFromJson<BorderWiseTariffExchangeModelV2>(data);
			}
			catch
			{
				return null;
			}
		}
	}
}
