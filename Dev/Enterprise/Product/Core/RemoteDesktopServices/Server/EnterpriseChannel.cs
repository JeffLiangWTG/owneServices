using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Win32;

namespace Enterprise.RemoteDesktopServices.Server
{
	public partial class EnterpriseChannel : MessageChannel
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string MutexName = "Local\\WTG.EnterpriseChannel.Initialization.9A6F0DA8-F5D0-412B-A255-0D6C9DBABAD6";

		public EnterpriseChannel()
		{
			rdpUniqueInstanceMutex = new Mutex(false, MutexName, out var createdNewMutex);
			CreatedNewMutex = createdNewMutex;
		}

		enum ConnectionAttemptResult
		{
			Success = 0,
			CouldNotConnect = 1,
			InitializationMessageTimeout = 2,
			OpenFileSupportedNotRegistered = 3,
			OpenFileSupportedNotReplied = 4,
			OperationCancelled = 5,
		}

		readonly Mutex rdpUniqueInstanceMutex;
		public bool CreatedNewMutex { get; }

		public static void Initialize(XmlMessageHandler<InitializationMessage> initializationMessageHandler = null, bool useLegacyCitrix = false)
		{
			initializationMessageHandler ??= new InitializationMessageHandler();
			MessageHandlers.Register(EnterpriseChannelMessageTypes.InitializationNew, initializationMessageHandler);
			MessageHandlers.Register(EnterpriseChannelMessageTypes.Initialization, initializationMessageHandler);
			MessageHandlers.Register(EnterpriseChannelMessageTypes.CheckDriveMapping, new CheckDriveMappingHandler());

			Instance = useLegacyCitrix ? new EnterpriseChannelCitrixLegacy() : new EnterpriseChannel();
			Instance.HookSessionMonitorAndDisconnectOnAppExit();
			Instance.EstablishConnection();
		}

		void EstablishConnection()
		{
			sessionMonitor.InitialConnect();
		}

		static ConnectionAttemptResult TryToConnect(TimeSpan waitingTime, ConnectReason connectReason, out int errorCode, out Exception exception, [CallerMemberName] string methodName = null)
		{
			errorCode = 0;
			exception = null;

			try
			{
				errorCode = Instance.Connect();

				var terminalService = ObjectFactory.Get<TerminalService>();
				var supportedClientVersion = terminalService.IsCitrixICA ? ClientCitrixVersion.Version : ClientVersion.Version;

				Instance.ActivityLogger.Log($@"{methodName} method has been called when connecting.
IsCitrix: {terminalService.IsCitrixICA}, IsRemoteAppSession: {terminalService.IsRemoteAppSession}, IsWTSSession: {terminalService.IsWTSSession}
ClientSessionProtocolType: {terminalService.GetClientSessionProtocolType(numberOfTries: 1)}, TerminalService.LastWin32Error: {terminalService.LastWin32Error}
ChannelName: {Instance.GetChannelName()}, DynamicChannel: {Instance.GetDynamicChannel()}
SupportedClientVersion: {supportedClientVersion}
");

				if (!Instance.IsConnected)
				{
					return ConnectionAttemptResult.CouldNotConnect;
				}

				if (!InitializationMessageHandler.InitializationCompleted.WaitOne(waitingTime))
				{
					return ConnectionAttemptResult.InitializationMessageTimeout;
				}

				Instance.ActivityLogger.Log($@"Initialization completed, client version: [{InitializationMessageHandler.RemoteVersion}], client supported message types: [{string.Join(";", InitializationMessageHandler.RegisteredRemoteMessageTypes)}]");

				if (Array.IndexOf(InitializationMessageHandler.RegisteredRemoteMessageTypes, EnterpriseChannelMessageTypes.OpenFileSupported) == -1)
				{
					return ConnectionAttemptResult.OpenFileSupportedNotRegistered;
				}

				if (EnvProxy.Instance.Registry.RemoteAppSendTestingMessageOnInitialize && Instance.SendMessageWithTimeOut(EnterpriseChannelMessageTypes.OpenFileSupported, waitingTime, Array.Empty<byte>()) == FeedBackMessageResults.TimeOut)
				{
					return ConnectionAttemptResult.OpenFileSupportedNotReplied;
				}

				SendInfoOnConnect(connectReason);

				Instance.ActivityLogger.Log("Connection completed");

				return ConnectionAttemptResult.Success;
			}
			catch (OperationCanceledException operationCancelledException)
			{
				ShowDisconnectedError();
				exception = operationCancelledException;
				return ConnectionAttemptResult.OperationCancelled;
			}
		}

		static void ReportOnceIfConnected(string key, string message)
		{
			Instance.ActivityLogger.Log($"Connection failed due to [{key}] with the error message [{message}]");
			if (!Instance.applicationExitFlag)
			{
				ErrorReporter.ReportOnce(key, Instance.ActivityLogger.ToString());
			}
		}

		static TimeSpan GetReconnectionTimeout()
		{
			try
			{
				return TimeSpan.FromSeconds(EnvProxy.Instance.Registry.RemoteAppWaitingForReconnectionTimeoutInSeconds);
			}
			catch
			{
				return TimeSpan.FromSeconds(30);
			}
		}

		public void ShowDragDropTrackingInfoForm()
		{
			if (DragDropTrackingInfoForm == null)
			{
				DragDropTrackingInfoForm = new DragDropTrackingForm();

				DragDropTrackingInfoForm.Shown += DragDropTrackingInfoForm_Shown;
				DragDropTrackingInfoForm.FormClosed += DragDropTrackingInfoForm_FormClosed;
			}

			DragDropTrackingInfoForm.Show();

			void DragDropTrackingInfoForm_Shown(object sender, EventArgs e)
			{
				DragDropTrackingInfoForm.Shown -= DragDropTrackingInfoForm_Shown;

				TrackingInfoLogger.Instance.NewLog(() => ActivityLogger.ToString());
			}

			void DragDropTrackingInfoForm_FormClosed(object sender, FormClosedEventArgs e)
			{
				DragDropTrackingInfoForm.FormClosed -= DragDropTrackingInfoForm_FormClosed;
				DragDropTrackingInfoForm.Dispose();
				DragDropTrackingInfoForm = null;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Nothing we can do here if network is broken.")]
		static void Reconnect()
		{
			InitializationMessageHandler.InitializationCompleted.Reset();
			Instance.sessionMonitor.PostReconnect();
		}

		static void ShowDisconnectedError()
		{
			Globals.Message.ShowError(Res.GetString("D713E9C5-3FC6-486D-A582-E6B8ACD38E76", "The connection is broken. Please check your network connectivity and try again."));
		}

		static void SendInfoOnConnect(ConnectReason connectReason)
		{
			SendRemoteAppSettings();
			SendRDPVersionInfo();
			SendWCAAuthenticationInfo();

			void SendRemoteAppSettings()
			{
				if (InitializationMessageHandler.IsUrlAuthenticationSupported)
				{
					Instance.Send(Serialize(
						EnterpriseChannelMessageTypes.RemoteAppSettings,
						new RemoteAppSettingsMessage
						{
							TimeoutForCheckDriveMapping = EnvProxy.Instance.Registry.RemoteAppCheckDriveMappingTimeoutInSeconds,
							applicationId = EnvProxy.Instance.Registry.MicrosoftOffice365ApplicationIdForDragDrop,
							tenantId = EnvProxy.Instance.Registry.MicrosoftOffice365TenantIdForDragDrop,
							attachmentEmailFetchLimit = EnvProxy.Instance.Registry.MicrosoftOffice365AttachmentEmailFetchLimitForDragDrop,
						}));
				}
				else
				{
					ReportOnceIfConnected(
						$"EnterpriseChannel-SendInfo RemoteAppSettings NotRegistered [{connectReason}]",
						$"RemoteAppSettings message won't be sent because its handler isn't registered on client side [{InitializationMessageHandler.RemoteVersion}]");
				}
			}

			void SendRDPVersionInfo()
			{
				if (InitializationMessageHandler.IsDragDropLiteSupported)
				{
					var supportedClientVersion = ObjectFactory.Get<TerminalService>().IsCitrixICA ? ClientCitrixVersion.Version : ClientVersion.Version;
					Instance.Send(Serialize(EnterpriseChannelMessageTypes.ServerRDPVersion, new ServerRDPVersionMessage(supportedClientVersion)));
				}
				else if (ClientVersion.Version >= Instance.serverRdpVersionSupportedVerison)
				{
					ReportOnceIfConnected(
						$"EnterpriseChannel-SendInfo ServerRDPVersion NotRegistered [{connectReason}]",
						$"ServerRDPVersion message won't be sent because its handler isn't registered on client side [{InitializationMessageHandler.RemoteVersion}]");
				}
			}

			void SendWCAAuthenticationInfo()
			{
				var tokenBasedAccessEnabled = SystemDataRegistry.Instance.WiseCloudAccessorTokenBasedAccess.Value;
				var domainHint = tokenBasedAccessEnabled ? SystemDataRegistry.Instance.DomainHint.Value : string.Empty;
				var authenticationType = tokenBasedAccessEnabled ? WCAAuthenticationMessage.WCAAuthenticationType.TokenBased : WCAAuthenticationMessage.WCAAuthenticationType.UsernameAndPassword;

				if (tokenBasedAccessEnabled || SystemDataRegistry.Instance.RevertToUsernameAndPasswordAuthentication.Value)
				{
					Instance.Send(Serialize(EnterpriseChannelMessageTypes.WCAAuthentication, new WCAAuthenticationMessage(authenticationType, domainHint)));
				}
			}
		}

		public static void InitializeUI(string licenseKey, string branchCode)
		{
			MessageHandlers.Register(EnterpriseChannelMessageTypes.StartDrop, new StartDropHandler());
			MessageHandlers.Register(EnterpriseChannelMessageTypes.DragDrop, new DragDropHandler());
			MessageHandlers.Register(EnterpriseChannelMessageTypes.DragDropLite, new DragDropLiteHandler());
			MessageHandlers.Register(EnterpriseChannelMessageTypes.OpenFileChanged, new OpenFileChangedHandler());
			MessageHandlers.Register(EnterpriseChannelMessageTypes.SaveSentEmail, (IMessageHandler)ObjectFactory.Get("SaveSentEmailHandler"));
			MessageHandlers.Register(EnterpriseChannelMessageTypes.DragStatus, new DragStatusHandler());
			MessageHandlers.Register(EnterpriseChannelMessageTypes.DragOver, new DragOverHandler());
			MessageHandlers.Register(EnterpriseChannelMessageTypes.SessionSwitch, new SessionSwitchHandler(new WtsSessionSwitchCallback(null, Instance.OnClientSessionSwitch)));
			MessageHandlers.Register(EnterpriseChannelMessageTypes.DragCallBack, new DragCallBackHandler());
			MessageHandlers.Register(EnterpriseChannelMessageTypes.MicrosoftOffice365Message, new MicrosoftOffice365ObjectHandler());

			if (Instance.IsConnected && InitializationMessageHandler.InitializationCompleted.WaitOne(0))
			{
				try
				{
					Instance.Send(Serialize(
						EnterpriseChannelMessageTypes.RemoteAppSettings,
						new RemoteAppSettingsMessage(licenseKey, branchCode, null)
						{
							TimeoutForCheckDriveMapping = EnvProxy.Instance.Registry.RemoteAppCheckDriveMappingTimeoutInSeconds,
							domain = InstanceDetails.Current.Domain,
							instance = InstanceDetails.Current.Instance,
						}));
				}
				catch (OperationCanceledException)
				{
					ShowDisconnectedError();
				}
			}
		}

		public static EnterpriseChannel Instance { get; private set; }
		public DragDropTrackingForm DragDropTrackingInfoForm { get; private set; }

		protected virtual string GetChannelName()
			=> ObjectFactory.Get<TerminalService>().IsCitrixICA ? EnterpriseChannelInfo.CitrixChannelName : EnterpriseChannelInfo.ChannelName;

		protected virtual bool GetDynamicChannel() => true;

		const int ERROR_SUCCESS = 0;
		int Connect()
		{
			lock (isTransitioningSyncObject)
			{
				if (virtualChannelStream != null)
				{
					return ERROR_SUCCESS;
				}

#if DEBUG
				ConnectedCount += 1;
#endif

				channelHandle = WtsApi.Instance.VirtualChannelOpen(GetChannelName(), GetDynamicChannel());
				if (channelHandle == IntPtr.Zero)
				{
					return Marshal.GetLastWin32Error();
				}

				virtualChannelStream = WtsApi.Instance.VirtualChannelGetStream(channelHandle);

				cancelReadLoopEvent.Reset();
				ReadLoopStartEvent.Reset();
				readThread = new Thread(ReadLoop);
				readThread.Start();

				ReadLoopStartEvent.WaitOne();
				Send(Serialize(EnterpriseChannelMessageTypes.ServerMessageLoopReady, Array.Empty<byte>()));

				return ERROR_SUCCESS;
			}
		}

		void HookSessionMonitorAndDisconnectOnAppExit()
		{
			Application.ThreadExit += OnApplicationThreadExit;
			sessionMonitor = new HiddenFormWtsSessionChangeMonitor(new WtsSessionSwitchCallback(OnInitialConnect, OnServerSessionSwitch));
		}

		void OnApplicationThreadExit(object sender, EventArgs e)
		{
			try
			{
				applicationExitFlag = true;
				if (ApplicationDispatcher.MainThread == Thread.CurrentThread)
				{
					Application.ThreadExit -= OnApplicationThreadExit;
					SendCleanExitMessage();
					Close();

					void SendCleanExitMessage()
					{
						try
						{
							if (IsConnected)
							{
								var result = BeginSendMessage<CargoWiseOneCleanExitMessage, bool>(EnterpriseChannelMessageTypes.CWCleanExit, new CargoWiseOneCleanExitMessage(true));
								result.AsyncWaitHandle.WaitOne(2000);
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							// swallow any exception to avoid interfering in Close method
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}
		}

		[Conditional("DEBUG")]
		internal void ApplicationThreadExit()
		{
			OnApplicationThreadExit(this, EventArgs.Empty);
		}

		public event EventHandler OnClosed;

		public void Close()
		{
			OnClosed?.Invoke(this, EventArgs.Empty);
			sessionMonitor?.Close();
			OnDisconnect();
			DragDropTrackingInfoForm?.Dispose();
			rdpUniqueInstanceMutex.Dispose();
		}

		bool applicationExitFlag;
		volatile Stream virtualChannelStream;
		HiddenFormWtsSessionChangeMonitor sessionMonitor;
		readonly object writeSync = new object();
		readonly object isTransitioningSyncObject = new object();

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context")]
		const string SendErrorMessage = "Sending to the remote desktop failed";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context")]
		const string NO_PROCESS_MESSAGE = "No process is on the other end of the pipe.";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context")]
		const string IOERROR_HANDLE_INVALID = "The handle is invalid.";
		public const int ERROR_NETNAME_DELETED = unchecked((int)0x80070040);
		public const int ERROR_INVALID_FUNCTION = unchecked((int)0x80070001);

		public override bool Send(byte[] data)
		{
#if DEBUG
			OnSendingMessage?.Invoke(this, data);
#endif

			try
			{
				lock (writeSync)
				{
					var stream = virtualChannelStream;
					if (stream != null)
					{
						// Not assuming the first 4 bytes are message type since the channel can be used to send any arbitrary data
						TrackingInfoLogger.Instance.NewLog(() => $"Sending message: {string.Join(", ", data.Take(4))}");
						return SendCore(stream, data);
					}
					else
					{
						throw new OperationCanceledException(SendErrorMessage);
					}
				}
			}
			catch (OperationCanceledException operationCanceledException) when (operationCanceledException.Message.Equals(SendErrorMessage))
			{
				TrackingInfoLogger.Instance.NewLog(() => $"Failed to send message");
				throw;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				TrackingInfoLogger.Instance.NewLog(() => $"Failed to send message. Exception: {ex}");
				if (IsConnected)
				{
					if (sessionMonitor != null)
					{
						if (ex is ObjectDisposedException || (ex is IOException && (ex.IsHResult(ERROR_INVALID_FUNCTION) || ex.IsHResult(ERROR_NETNAME_DELETED) || ex.Message.StartsWith(NO_PROCESS_MESSAGE, StringComparison.Ordinal))))
						{
							Reconnect();
						}
						else if (ex is UnauthorizedAccessException)
						{
							throw new OperationCanceledException(SendErrorMessage, ex);
						}
						else
						{
							ErrorReporter.ReportOnce(ex.Message, ex);
						}
					}
				}
				else
				{
					// Assume the channel is disconnected which is treated as...
					throw new OperationCanceledException(SendErrorMessage, ex);
				}
				return false;
			}
		}

		protected virtual bool SendCore(Stream stream, byte[] data)
		{
			IAsyncResult result;
			lock (writeLock)
			{
				result = stream.BeginWrite(data, 0, data.Length, null, null);
			}
			result.AsyncWaitHandle.WaitOne();
			lock (writeLock)
			{
				stream.EndWrite(result);
			}
			TrackingInfoLogger.Instance.NewLog(() => $"Message sent");
			return true;
		}
		readonly object writeLock = new object();

		protected sealed override void OnErrorDuringSendingReturnCallbackMessage(string returnTypeInfo, Exception unexpectedException)
		{
			if (!IsConnected
				&& unexpectedException is OperationCanceledException operationCanceledException
				&& operationCanceledException.Message.Equals(SendErrorMessage)
				&& operationCanceledException.InnerException is null)
			{
				return;
			}

			ErrorReporter.ReportOnce(nameof(OnErrorDuringSendingReturnCallbackMessage), $"Return: [{returnTypeInfo}]", unexpectedException);
		}

		readonly AutoResetEvent cancelReadLoopEvent = new AutoResetEvent(false);
		public AutoResetEvent ReadLoopStartEvent = new AutoResetEvent(false);
#if DEBUG
		public AutoResetEvent ReadLoopStartEventForTest = new AutoResetEvent(false);
		public Thread ReadThreadForTest { get { return readThread; } }
		public int ConnectedCount { get; set; }
#endif

		protected virtual int ChannelPDULength
		{
			get
			{
				return MSCHANNEL_PDU_LENGTH;
			}
		}
		const int MSCHANNEL_PDU_LENGTH = 1608; // see pchannel.h

		[SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Baseline")]
		void ReadLoop()
		{
#if DEBUG
			ReadLoopStartEventForTest.Set();
#endif
			ReadLoopStartEvent.Set();

			ActivityLogger.Log("ReadLoop thread started");

			byte[] buffer = new byte[ChannelPDULength];
			WaitHandle[] waitHandles = new WaitHandle[] { null, cancelReadLoopEvent };

			try
			{
				Stream stream;
				while (!cancellationToken.Token.IsCancellationRequested &&
						null != (stream = virtualChannelStream))
				{
					IAsyncResult result;
					lock (readLock)
					{
						result = stream.BeginRead(buffer, 0, buffer.Length, null, null);
					}
					waitHandles[0] = result.AsyncWaitHandle;
					int index = WaitHandle.WaitAny(waitHandles);
					if (index == 1)
					{
						break;
					}

					int length;
					lock (readLock)
					{
						length = stream.EndRead(result);
					}
					HandleRead(buffer, length);
				}
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ActivityLogger.Log($"ReadLoop thread exception: {ex}");

				// Only exceptions while we are connected need reporting.
				if (IsConnected)
				{
					// Give the disconnect notification time to arrive
					if (!cancelReadLoopEvent.WaitOne(2000) && IsConnected && sessionMonitor != null)
					{
						if (ex is OperationCanceledException
							|| (ex is IOException && (ex.IsHResult(ERROR_NETNAME_DELETED) || ex.Message.StartsWith(NO_PROCESS_MESSAGE, StringComparison.Ordinal) || ex.Message == IOERROR_HANDLE_INVALID)))
						{
							// Disconnection notification hasn't arrived.
							// It could be that the virtual channel is closed, but the connection is not.
							// Post a reconnect to the session monitor so the reconnection occurs on
							// the same thread as real disconnects.
							try
							{
								Reconnect();
							}
							catch (OperationCanceledException)
							{
								ShowDisconnectedError();
							}
						}
						else if (
#if NETFRAMEWORK
							ex is System.Runtime.Serialization.SerializationException ||
#endif
							ex is XmlException ||
							(ex.InnerException != null && ex.InnerException is XmlException) ||
							ex is MiddleChunkException)
						{
							ReportOnceException(ex);
						}
						else
						{
							ErrorReporter.ReportOnce(string.Format("{0} (HRESULT: 0x{1:X8})", ex.Message, ex.GetHResult()), ex);
						}
					}
#if DEBUG
					MessageHandledEvent.Set();
#endif
				}
			}

			ActivityLogger.Log("ReadLoop thread stopped");
		}
		readonly object readLock = new object();
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void ReportOnceException(Exception ex)
		{
			currentMessageStream.Position = 0;
			var messageDataReader = new StreamReader(currentMessageStream);

			string xmlContent = null;
			var message = "\r\nCorrupted xml";
			const int msgLimitLength = 1048576;
			if (currentMessageStream.Length > msgLimitLength)
			{
				var xmlEx = ex as XmlException ?? ex.InnerException as XmlException;
				if (xmlEx != null)
				{
					var errorLineNumber = xmlEx.LineNumber;
					var startCutPos = 0;
					while (errorLineNumber-- > 0)
					{
						xmlContent = messageDataReader.ReadLine();
					}

					if (xmlContent != null && xmlContent.Length > msgLimitLength)
					{
						startCutPos = Math.Max(0, xmlEx.LinePosition - msgLimitLength / 2);
						xmlContent = xmlContent.Substring(startCutPos, Math.Min(xmlContent.Length - startCutPos, msgLimitLength));
					}

					message += startCutPos > 0
						? string.Format(CultureInfo.CurrentCulture, " line {0} position {1}:\r\n", xmlEx.LineNumber, startCutPos)
						: string.Format(CultureInfo.CurrentCulture, " line {0}:\r\n", xmlEx.LineNumber);
				}
				else
				{
					message = "\r\n";
				}
			}
			else
			{
				message += ":\r\n";
				xmlContent = messageDataReader.ReadToEnd();
			}

			ErrorReporter.ReportOnce(
				ex.Message + message + (xmlContent ?? string.Empty).Replace("<", "&lt;").Replace(">", "&gt;") +
				(headerData.Length > 0 ? "\r\n\r\nHeaders:\r\n" + headerData.ToString().Trim() : string.Empty), ex);
		}

		int chunkCounter;
		readonly StringBuilder headerData = new StringBuilder();

#if DEBUG
		public bool IsHandlingDragDropMessage { get; private set; }
		public AutoResetEvent MessageHandledEvent = new AutoResetEvent(false);

		bool IsPartOfInitializationMessage(string messageType)
		{
			return messageType == EnterpriseChannelMessageTypes.Initialization || messageType == EnterpriseChannelMessageTypes.InitializationNew || messageType == EnterpriseChannelMessageTypes.ReturnCallback;
		}
#endif

		protected virtual void HandleRead(byte[] buffer, int length)
		{
			if (length > 0)
			{
				int fileSize = BitConverter.ToInt32(buffer, 0);
				int chunksCount = (int)Math.Ceiling((double)fileSize / (buffer.Length - 8));

				WtsApi.ChannelFlags flags = (WtsApi.ChannelFlags)BitConverter.ToUInt32(buffer, 4);
				WtsApi.ChannelFlags channelFlag = flags & WtsApi.ChannelFlags.Only;

				if (channelFlag != WtsApi.ChannelFlags.Only)
				{
					if (channelFlag == WtsApi.ChannelFlags.First)
					{
						headerData.Clear();
						headerData.Capacity = chunksCount * 70;
						chunkCounter = 0;
					}

					headerData.AppendFormat((NoResString)"Chunk#: {0}	File size:{1}	Chunk size:{2}	Flags: {3}\r\n",
								chunkCounter++,
								fileSize,
								(length - 8),
								flags);
				}

				if (channelFlag == WtsApi.ChannelFlags.Only || channelFlag == WtsApi.ChannelFlags.First)
				{
					currentMessageType = Encoding.ASCII.GetString(buffer, 8, 4);
					TrackingInfoLogger.Instance.NewLog(() => $"Received message: {currentMessageType}");
				}

#if DEBUG
				bool isTestMessageHandled =
					!IsPartOfInitializationMessage(currentMessageType) &&
					(channelFlag == WtsApi.ChannelFlags.Only || channelFlag == WtsApi.ChannelFlags.Last);
#endif

				if (currentMessageType == EnterpriseChannelMessageTypes.DragDrop)
				{
					switch (channelFlag)
					{
						case WtsApi.ChannelFlags.Only:
							ReceiveBeginMessage(buffer, 8, length - 8, fileSize);
							ReceiveEndMessage();
							break;
						case WtsApi.ChannelFlags.First:
							ReceiveFirstChunkedMessage(buffer, 8, length - 8);
							break;
						case WtsApi.ChannelFlags.Middle:
							ReceiveMiddleChunkedMessage(buffer, 8, length - 8);
							break;
						case WtsApi.ChannelFlags.Last:
							ReceiveEndChunkedMessage(buffer, 8, length - 8, fileSize);
							break;
					}

#if DEBUG
					if (isTestMessageHandled)
					{
						IsHandlingDragDropMessage = true;
						MessageHandledEvent.Set();
					}
#endif
				}
				else
				{
					switch (channelFlag)
					{
						case WtsApi.ChannelFlags.Only:
							ReceiveBeginMessage(buffer, 8, length - 8, fileSize);
							ReceiveEndMessage();
							break;
						case WtsApi.ChannelFlags.First:
							ReceiveBeginMessage(buffer, 8, length - 8, fileSize);
							break;
						case WtsApi.ChannelFlags.Middle:
							ReceiveAppendMessage(buffer, 8, length - 8);
							break;
						case WtsApi.ChannelFlags.Last:
							ReceiveAppendMessage(buffer, 8, length - 8);
							ReceiveEndMessage();
							break;
					}

#if DEBUG
					if (isTestMessageHandled)
					{
						IsHandlingDragDropMessage = false;
						MessageHandledEvent.Set();
					}
#endif
				}
			}
		}

		IntPtr channelHandle;
		Thread readThread;
		readonly CancellationTokenSource cancellationToken = new();

#if DEBUG
		internal int Connect_ForTest()
		{
			return Connect();
		}

		internal static TimeSpan GetReconnectionTimeout_ForTest()
		{
			return GetReconnectionTimeout();
		}

		internal event EventHandler OnDisconnecting;
#endif

		public void OnDisconnect([CallerMemberName] string methodName = null)
		{
#if DEBUG
			ConnectedCount -= 1;
			OnDisconnecting?.Invoke(this, EventArgs.Empty);
#endif
			lock (isTransitioningSyncObject)
			{
				if (virtualChannelStream != null)
				{
					cancelReadLoopEvent.Set();
					VirtualChannelClose();
					virtualChannelStream.Close();
					virtualChannelStream = null;
					CancelAllAsyncOperations();
					if (!readThread.Join(TimeSpan.FromSeconds(10)))
					{
						cancellationToken.Cancel();
					}
					readThread = null;
				}
				InitializationMessageHandler.RegisteredRemoteMessageTypes = Array.Empty<string>();
				EnvProxy.RegisteredRemoteMessageTypesCopyForGlobal = Array.Empty<string>();
			}
			ActivityLogger.Log($"{methodName} method has been called when disconnecting");
		}

		protected virtual void VirtualChannelClose()
		{
			WtsApi.Instance.VirtualChannelClose(channelHandle);
		}

		public SessionSwitchEventHandler ClientSessionSwitch;

		void OnClientSessionSwitch(SessionSwitchReason reason)
		{
			ClientSessionSwitch?.Invoke(this, new SessionSwitchEventArgs(reason));
		}

		public SessionSwitchEventHandler ServerSessionSwitch;

		void OnInitialConnect()
		{
			ConnectionAttemptResult attemptResult;
			if ((attemptResult = TryToConnect(TimeSpan.FromMilliseconds(3000), ConnectReason.FirstAttempOnInitializing, out var errorCode, out var exception)) != ConnectionAttemptResult.Success)
			{
				ActivityLogger.Log($"Connection failed due to [{attemptResult}] on [{ConnectReason.FirstAttempOnInitializing}] in [{TimeSpan.FromMilliseconds(3000)}] seconds with the error code [{errorCode}]");
				InitializationMessageHandler.InitializationCompleted.Reset();
				Instance.OnDisconnect();

				if (attemptResult == ConnectionAttemptResult.CouldNotConnect)
				{
					Thread.Sleep(5000);
				}

				var reconnectionTimeout = GetReconnectionTimeout();
				if ((attemptResult = TryToConnect(GetReconnectionTimeout(), ConnectReason.SecondAttempOnInitializing, out errorCode, out exception)) != ConnectionAttemptResult.Success)
				{
					ReportConnectionFailed(attemptResult, ConnectReason.SecondAttempOnInitializing, reconnectionTimeout, errorCode, exception);
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context")]
		void ReportConnectionFailed(ConnectionAttemptResult result, ConnectReason connectReason, TimeSpan waitingTime, int errorCode, Exception exception)
		{
			const string errorMessageHeader = "Connect attempt failed";

			switch (result)
			{
				case ConnectionAttemptResult.CouldNotConnect:
					ActivityLogger.Log($"Connection failed due to [{ConnectionAttemptResult.CouldNotConnect}] on [{connectReason}] with the error code [{errorCode}]");
					break;

				case ConnectionAttemptResult.InitializationMessageTimeout:
					ActivityLogger.Log($"Connection failed due to [{ConnectionAttemptResult.InitializationMessageTimeout}] on [{connectReason}] within [{waitingTime}]");
					break;

				case ConnectionAttemptResult.OpenFileSupportedNotRegistered:
					ReportOnceIfConnected(
						$"EnterpriseChannel-Connect OpenFileSupported NotRegistered [{connectReason}]",
						$"{errorMessageHeader} because OpenFileSupported handler isn't registered on client side [{InitializationMessageHandler.RemoteVersion}]");
					break;

				case ConnectionAttemptResult.OpenFileSupportedNotReplied:
					ActivityLogger.Log($"Connection failed due to [{ConnectionAttemptResult.OpenFileSupportedNotReplied}] on [{connectReason}] for no replied by client side [{InitializationMessageHandler.RemoteVersion}] in [{waitingTime}]");
					break;

				case ConnectionAttemptResult.OperationCancelled:
					ErrorReporter.ReportOnce($"EnterpriseChannel-Connect OperationCanceledException [{connectReason}]", $"{errorMessageHeader}, {ActivityLogger}", exception);
					break;

				case ConnectionAttemptResult.Success:
					break;
			}
		}

		internal void OnServerSessionSwitch(SessionSwitchReason reason)
		{
			ServerSessionSwitch?.Invoke(this, new SessionSwitchEventArgs(reason));

			switch (reason)
			{
				case SessionSwitchReason.RemoteConnect:
					InitializationMessageHandler.InitializationCompleted.Reset();
					var timeout = GetReconnectionTimeout();
					var result = TryToConnect(timeout, ConnectReason.RemoteConnectOnServerSessionSwitch, out var errorCode, out var exception);
					if (result != ConnectionAttemptResult.Success)
					{
						ReportConnectionFailed(result, ConnectReason.RemoteConnectOnServerSessionSwitch, timeout, errorCode, exception);
					}
					break;
				case SessionSwitchReason.RemoteDisconnect:
					OnDisconnect();
					break;
			}
		}

		public override bool IsConnected
		{
			get { return virtualChannelStream != null; }
		}

		protected sealed override void RecordMessageHandlerException(Exception unexpectedException, string message)
		{
			ErrorReporter.ReportOnce(nameof(RecordMessageHandlerException), message, unexpectedException);
		}

		void ReceiveBeginMessage(byte[] data, int offset, int length, int streamSize)
		{
			offset += 4;
			currentMessageStream = streamSize == 0 ? new MemoryStream() : new MemoryStream(streamSize);
			currentMessageStream.Write(data, offset, length - 4);
		}

		void ReceiveAppendMessage(byte[] data, int offset, int length)
		{
			if (currentMessageStream != null)
			{
				currentMessageStream.Write(data, offset, length);
			}
		}

		void ReceiveEndMessage()
		{
			if (currentMessageStream != null && currentMessageType != null)
			{
				currentMessageStream.Position = 0;
				TrackingInfoLogger.Instance.NewLog(() => $"Handling message: {currentMessageType}");
				MessageHandlers.HandleMessage(this, currentMessageType, currentMessageStream);
				TrackingInfoLogger.Instance.NewLog(() => $"Handled message: {currentMessageType}");
			}
			currentMessageType = null;
			currentMessageStream = null;
		}

		void ReceiveFirstChunkedMessage(byte[] data, int offset, int length)
		{
			offset += 4;
			chunkedMessageStreams.Clear();
			chunkedMessageStreams.Add(data.Skip(offset).Take(length - 4).ToArray());
		}

		void ReceiveMiddleChunkedMessage(byte[] data, int offset, int length)
		{
			chunkedMessageStreams.Add(data.Skip(offset).Take(length).ToArray());
		}

		void ReceiveEndChunkedMessage(byte[] data, int offset, int length, int fileSize)
		{
			chunkedMessageStreams.Add(data.Skip(offset).Take(length).ToArray());
			currentMessageStream = fileSize > 0 ? new MemoryStream(fileSize) : new MemoryStream();

			int chunksCount = chunkedMessageStreams.Count;

			// Try fixing out-of-order chunks as best as possible before failing and notifying user.
			bool canHandleChunks = TryHandleChunkedMessageBySwapingLastChunk();

			if (!canHandleChunks && chunksCount <= tryReorderChunksThreshold)
			{
				canHandleChunks = TryHandleChunkedMessagesByReorderingMiddleChunks();
			}

			currentMessageType = null;
			currentMessageStream.Dispose();
			currentMessageStream = null;
			chunkedMessageStreams.Clear();

			if (!canHandleChunks)
			{
#if DEBUG
				Globals.Message.ShowWarning(dataCorruptedRetryMessage);
#else
				ApplicationDispatcher.Current?.Invoke(() => Globals.Message.ShowWarning(dataCorruptedRetryMessage));
#endif
			}
		}
		const int tryReorderChunksThreshold = 15;
		[SuppressMessage("CargoWiseOne", "CW1098:DoNotInitializeStringFieldsWithResGetString", Justification = "Baseline")]
		protected readonly string dataCorruptedRetryMessage = Res.GetString("c14d1e5f-9701-4967-8943-7dccf9255817", "Unable to receive data successfully. Please try again in a few minutes.");

		bool TryHandleChunkedMessageBySwapingLastChunk()
		{
			int chunksCount = chunkedMessageStreams.Count;
			int lastChunkIndex = FindLastChunkIndex();

			// Try putting actual 'Last' chunk to the last position, and keep the rest in a same order.
			if (lastChunkIndex > 0 && lastChunkIndex < chunksCount - 1)
			{
				byte[] lastChunkStream = chunkedMessageStreams[lastChunkIndex];

				for (int index = lastChunkIndex; index < chunksCount - 1; index++)
				{
					chunkedMessageStreams[index] = chunkedMessageStreams[index + 1];
				}

				chunkedMessageStreams[chunksCount - 1] = lastChunkStream;
			}

			return TryHandleChunkedMessages();
		}

		bool TryHandleChunkedMessagesByReorderingMiddleChunks()
		{
			int chunksCount = chunkedMessageStreams.Count;

			// Try reordering the last 10 'Middle' chunks one at the time to all posible positions (excluding 'Last' chunk and last postion).
			int endIndex = chunksCount - 2;
			int startIndex = Math.Max(0, endIndex - 10);

			for (int chunkIndex = startIndex; chunkIndex <= endIndex; chunkIndex++)
			{
				for (int positionIndex = 0; positionIndex < chunksCount - 1; positionIndex++)
				{
					if (positionIndex == chunkIndex)
					{
						continue;
					}

					byte[] chunkStream = chunkedMessageStreams[chunkIndex];
					chunkedMessageStreams[chunkIndex] = chunkedMessageStreams[positionIndex];
					chunkedMessageStreams[positionIndex] = chunkStream;

					if (TryHandleChunkedMessages())
					{
						return true;
					}

					chunkStream = chunkedMessageStreams[positionIndex];
					chunkedMessageStreams[positionIndex] = chunkedMessageStreams[chunkIndex];
					chunkedMessageStreams[chunkIndex] = chunkStream;
				}
			}

			return false;
		}

		bool TryHandleChunkedMessages()
		{
			currentMessageStream.Position = 0;

			chunkedMessageStreams
				.ToList()
				.ForEach(stream => currentMessageStream.Write(stream, 0, stream.Length));

			if (currentMessageType != null && currentMessageStream != null)
			{
				try
				{
					currentMessageStream.Position = 0;
					TrackingInfoLogger.Instance.NewLog(() => $"Try handling message: {currentMessageType}");
					MessageHandlers.HandleMessage(this, currentMessageType, currentMessageStream);
					TrackingInfoLogger.Instance.NewLog(() => $"Handled message: {currentMessageType}");
				}
				catch (Exception ex) when (ex is InvalidOperationException || ex is IOException)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Exception in try handling message: {ex}");
					return false;
				}
			}

			return true;
		}

		int FindLastChunkIndex()
		{
			var middleChunkedLength = chunkedMessageStreams.Where((stream, index) => index > 0).Select(stream => stream.Length).Max();

			// Due to (potentially) unstable or slow network, chunks can come out of order, and the 'Last' chunk arrive as 'Middle' chunk (see WI00110215).
			byte[] lastChunkedStream = null;
			List<byte[]> lastChunkedStreams =
				chunkedMessageStreams.Where((stream, index) => index > 0 && stream.Length < middleChunkedLength).ToList();
			if (lastChunkedStreams.Count == 1)
			{
				lastChunkedStream = lastChunkedStreams.SingleOrDefault();
			}
			else if (lastChunkedStreams.Count > 1)
			{
				currentMessageStream.Position = 0;
				chunkedMessageStreams
				.ToList()
				.ForEach(stream => currentMessageStream.Write(stream, 0, stream.Length));

				throw new MiddleChunkException("Chuncks contains more than one 'Middle' chunk");
			}

			return lastChunkedStream != null ? chunkedMessageStreams.IndexOf(lastChunkedStream) : -1;
		}

		protected string currentMessageType;
		protected Stream currentMessageStream;
		readonly List<byte[]> chunkedMessageStreams = new List<byte[]>();

		[Serializable]
		public class MiddleChunkException : Exception
		{
			public MiddleChunkException(string message) : base(message)
			{
			}

#if NETFRAMEWORK
			protected MiddleChunkException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif

			public MiddleChunkException(string message, Exception ex)
				: base(message, ex)
			{
			}
		}

#if DEBUG
		public bool IsReadThreadAlive
		{
			get
			{
				Thread t = readThread;
				return t != null && t.IsAlive;
			}
		}

		public static void Reset()
		{
			Instance = null;
		}

		[field: SuppressMessage("CargoWiseOne", "CW1021")]
		internal static event EventHandler<byte[]> OnSendingMessage;
#endif

		internal readonly Logger ActivityLogger = new Logger();

		internal sealed class Logger
		{
			public void Log(string message)
			{
				var lockTaken = false;
				try
				{
					spinLock.Enter(ref lockTaken);
					builder.Append($"[{ZDateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}], {message}, thread ID [{Thread.CurrentThread.ManagedThreadId}], session ID [{Process.GetCurrentProcess().SessionId}].");
				}
				finally
				{
					if (lockTaken)
					{
						spinLock.Exit(false);
					}
				}
			}

			[Conditional("DEBUG")]
			public void Clear() => builder.Clear();

			public override string ToString() => builder.ToStringWithNewLineBetweenAppends();

			readonly ZStringBuilder builder = new ZStringBuilder((NoResString)"Activities:");

			SpinLock spinLock = new SpinLock(false);
		}

		readonly Version serverRdpVersionSupportedVerison = new(4, 5, 0);
	}
}
