using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class TcpIpSenderReceiver
	{
		#region Ctor and startup checks

		public TcpIpSenderReceiver(ILogger logger, CcsukIpaddressesSetting ipAddressPairToTry)
		{
			reconnectCount = 0;
			this.ipAddressPairToTry = ipAddressPairToTry;
			this.logger = logger;
			CheckRegistryOptions();
			sendingFactory = new BusinessObjectFactory();
			sendingFactory.RefreshEnabled = false;
			sendingFactory.NameForDebugging = "CCSUK sender";
			hostnameShared = new Host().HostNameFormatted;
			maxLengthOfPayload = GBCustomsDataRegistry.Instance.CcsukMaximumTransmittablePayloadSize.Value;
		}

		void CheckRegistryOptions()
		{
			var payloadLimit = GBCustomsDataRegistry.Instance.CcsukMaximumTransmittablePayloadSize.Value;
			if (string.IsNullOrEmpty(GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.Value)
				||
				string.IsNullOrEmpty(GBCustomsDataRegistry.Instance.CcsukPassword.Value)
				||
				string.IsNullOrEmpty(RemoteHostIpAddress.ToString())
				||
				GBCustomsDataRegistry.Instance.CcsukRemotePort == 0
				||
				payloadLimit < 50 // size of a handshake message
				||
				payloadLimit > 0xA00000)  //0xA00000 - 10 meg - 10485760 bytes
			{
				throw new HostedServiceException(string.Format("Cannot initialise service task, the registry options are insufficient. Check in '{0}' for: Local host mnemonic, password, IP addresses, remote port and maximum payload size. Read the relevant update note thoroughly before changing these options.", GBCustomsDataRegistry.Instance.CcsukPassword.Categories));
			}
		}

		#endregion

		#region Publics

		public void ConnectAndLogonAndStartReceivingInboundMessages() => ConnectAndLogonAndStartReceivingInboundMessagesCore();

		/// <summary>
		/// Main thread
		/// </summary>

		protected virtual void ConnectAndLogonAndStartReceivingInboundMessagesCore()
		{
			Debug("======== Begin connectivity ========");
			shakenHandsStatus = ShakeHandStatus.Unknown;
			ConnectOutboundSocket();
			int listeningPort = CreateListenerAndStartListening();
			SendHandshakeButDoNotReadReply(listeningPort);
			AcceptInboundSocketAndStartWorkerThread();
			WaitForHandshakeResponseToAppear();
			if (shakenHandsStatus == ShakeHandStatus.DefinitelySucceeded)
			{
				LogonOrChangePassword();
				SendAndReceivePing();
			}
			else if (shakenHandsStatus == ShakeHandStatus.DefinitelyFailed)
			{
				var logMessage = "Handshake failed.";
				Debug(logMessage);
				Shutdown();
				throw new Exceptions.Handshake.HandshakeNotAcceptedException(logMessage);  // to turn task red
			}
			else
			{
				var logMessage = "Did not receive handshake response OK in time, cannot even try to log on";
				Debug(logMessage);
				Shutdown();
				throw new Exceptions.Handshake.HandshakeNotAcceptedException(logMessage);  // to turn task red
			}
		}

		void WaitForHandshakeResponseToAppear()
		{
			int handShakeCounter = 0;
			while (shakenHandsStatus == ShakeHandStatus.RequestSent && handShakeCounter < 100)
			{
				handShakeCounter++;
				if (shakenHandsStatus == ShakeHandStatus.DefinitelySucceeded || shakenHandsStatus == ShakeHandStatus.DefinitelyFailed)
				{
					Debug("Handshake response received, i=" + handShakeCounter);
					break;
				}
				else
				{
					Thread.Sleep(100);
				}
			}
		}

		/// <summary>
		/// Main thread... called periodically by the service task
		/// </summary>
		public bool SendAllWaitingOutboundInterchanges(int batchSize = 0)
		{
			if (shouldAbortThread)
			{
				return false;
			}
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(EDIInterchange));
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			query.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
			query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
			query.AddToFilter(EDIInterchangeSchema.EI_IsActive, 1);
			query.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name;

			if (batchSize > 0)
			{
				query.MaximumRows = batchSize;
			}

			sendingFactory.ClearQueryCache();

			Debug("Loading Interchanges...", LogType.Debug);

			var interchangesToSend = sendingFactory.Load<EDIInterchange>(query);

			Debug("Found " + interchangesToSend.Length + " to send", LogType.Debug);

			foreach (var bizObj in interchangesToSend)
			{
				var transientFactory = new BusinessObjectFactory();
				transientFactory.RefreshEnabled = false;
				Debug("Loading Interchange #" + bizObj.EI_InterchangeNum, LogType.Debug);
				var interchange = transientFactory.Load<EDIInterchange>(bizObj.PK);
				SendInterchangeAsCargoMessage(interchange);
			}
			return true;
		}

		#endregion

		#region Create initial sockets and rewire upon receive

		/// <summary>
		/// Main thread
		/// </summary>
		int CreateListenerAndStartListening()
		{
			int startPortRange = GBCustomsDataRegistry.Instance.CcsukLocalPortForBinding.Value;
			int listeningPort = IpAddress.FindNextAvailablePortInRangeOnAddress(startPortRange, startPortRange + 5, LocalBoundIpAddress);  // e.g. 5000
			if (listeningPort == -1)
			{
				throw new Exceptions.Sockets.CannotListenNoFreePortException();
			}

			tcpListener = new TcpListener(LocalBoundIpAddress, listeningPort);
			Debug(string.Format("Starting listening server on IP {0} and port {1}...", LocalBoundIpAddress, listeningPort));
			tcpListener.Start();
			Debug("...OK.");
			return listeningPort;
		}

		/// <summary>
		/// Main thread
		/// </summary>
		void ConnectOutboundSocket()
		{
			var remoteCukIp = RemoteHostIpAddress;
			if (LocalBoundIpAddress == IPAddress.Any)
			{
				Debug("Local endpoint for outbound socket will be IPAddress.Any");
				tcpClientForOutbound = new TcpClient();
			}
			else
			{
				Debug("Local endpoint for outbound socket will be " + LocalBoundIpAddress.ToString());
				tcpClientForOutbound = new TcpClient(new IPEndPoint(LocalBoundIpAddress, 0));
			}
			var port = GB.Registry.GBCustomsDataRegistry.Instance.CcsukRemotePort;
			Debug(string.Format("Connecting outbound socket to IP {0} and port {1}...", remoteCukIp, port));
			try
			{
				tcpClientForOutbound.Connect(remoteCukIp, port);
			}
			catch (SocketException ex)
			{
				throw new Exceptions.Sockets.CannotConnectException(ex);
			}
			Debug("... Connected=" + tcpClientForOutbound.Connected);
			if (!tcpClientForOutbound.Connected)
			{
				throw new Exceptions.Sockets.CannotConnectException();
			}
			outboundNetworkStream = tcpClientForOutbound.GetStream();
			outboundStreamWriter = new StreamWriter(outboundNetworkStream);
		}

		void AcceptInboundSocketAndStartWorkerThread()
		{
			receivingFactory = new BusinessObjectFactory();
			receivingFactory.RefreshEnabled = false;
			receivingFactory.NameForDebugging = "CCSUK receiver";

			Debug("Listener is trying to about to look for and then accept inbound pending connections...");
			int i = 0;
			while (i < 100 && !tcpListener.Pending())
			{
				Debug("No pending connection, will sleep for a bit, counter " + i.ToString());
				i++;
				Thread.Sleep(100);
			}
			if (!tcpListener.Pending())
			{
				// We need this because section 3.2.2.4 of the specs say that we will still get an inbound connection (and handshake response) even if the handshake was bad.... 
				//  BUT in reality we see NO inbound connection.  Without peeking at Pending() we block and hang. 
				throw new Exceptions.Handshake.NoInboundConnectionMadeException(i);
			}
			// Here, we have a pending connection, so it is OK top post a blocking call to Accept():
			Debug($"Listener has a pending connection and will accept inbound connection (blocking)...");
			inboundSocket = tcpListener.AcceptSocket();
			Debug("...accepted");

			// Start worker thread:			
			receivingFactory.ThreadSentry.RelinquishThreadOwnership();  // Main thread surrenders factory.
			dataReceivedWorkerThread = new Thread(new ThreadStart(ListenForAndWaitToReceiveInboundData));
			dataReceivedWorkerThread.Name = "CCSUK Inbound";
			dataReceivedWorkerThread.Start();
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]  // We catch a SocketException first, and the catching of an Exception is a back stop.  
		void ListenForAndWaitToReceiveInboundData()
		{
			int bufferSize = 2 + 1 + 15 + maxLengthOfPayload; // header intro, length of header, header, body. Approx 28000 bytes
			using (Db.DisposableActionForDbConnection())
			{
				receivingFactory.ThreadSentry.TakeThreadOwnership();  // worker thread assumes factory.
				while (!shouldAbortThread)
				{
					Debug("** Awaiting data on worker thread");
					var bytesRead = 0;
					var bufferForReceive = new byte[bufferSize];
					try
					{
						do
						{
							bytesRead += inboundSocket.Receive(bufferForReceive, bytesRead, bufferSize - bytesRead, SocketFlags.None); // block here until real data is received, until a shutdown signal (ICMP?) is receive (0 bytes are received - shutdown by CCSUK's end) or until the main thread interrupts our blocking call.
						} while (IsMoreDataExpected(bufferForReceive, bytesRead));
						Debug("** Data received, bytes=" + bytesRead);
					}
					catch (SocketException se)
					{
						if (se.SocketErrorCode == SocketError.Interrupted)  // see http://bytes.com/topic/c-sharp/answers/229446-blocking-tcp-sockets & http://support.ipswitch.com/kb/WSK-19980714-EM08.htm
						{
							Debug("** Wait was interrupted (WS=10004) - the listener will not be rewired as a socket shutdown is probably in progress. The likely cause of this is that the operating system issued a command to stop the service.");
							shouldAbortThread = true; // just in case
							receivingFactory.ThreadSentry.RelinquishThreadOwnership();
							return;
						}
						else
						{
							Debug("**Socket error during Receive(). " + se.SocketErrorCode + ". " + se.Message);
						}
					}
					catch (InvalidDataException ex)
					{
						ErrorReporter.ReportOnce("Error parsing stream", ex);
						shouldAbortThread = true;
						throw;
					}
					catch (Exception ex)
					{
						if (shouldAbortThread && inboundSocket == null)
						{
							Debug("** Inbound socket was null when attempting to block on Receive(), but ShouldAbortThread is set, meaning shutdown (at CW1 end) is in progress. Worker thread will end nicely. ");
						}
						else
						{
							Debug("** General error on inboundSocket.Receive(): " + ex.Message + (ex.InnerException?.Message ?? string.Empty));
						}
						receivingFactory.ThreadSentry.RelinquishThreadOwnership();
						return;
					}

					if (bytesRead == 0)
					{
						Debug("** Zero bytes read, assuming socket is being shut down by CCSUK");
						shouldAbortThread = true;
						receivingFactory.ThreadSentry.RelinquishThreadOwnership();
						return;
					}

					try
					{
						var dataReceived = new byte[bytesRead];
						Array.Copy(bufferForReceive, dataReceived, bytesRead);
						DataReceived(dataReceived);
					}
					catch (InvalidDataException ex)
					{
						ErrorReporter.ReportOnce("Error parsing stream", ex);
						shouldAbortThread = true;
						throw;
					}

					if (shouldAbortThread)
					{
						Debug("** Will not re-wire listener");
						receivingFactory.ThreadSentry.RelinquishThreadOwnership();
						break;
					}
				}
			}
		}

		/// <summary>
		/// Worker/second thread 
		/// </summary>
#if DEBUG
		protected
#endif
		void DataReceived(byte[] dataReceived)
		{
			MemoryStream ms = new MemoryStream(dataReceived);
			(Body responseBody, var payloadData) = new ResponseParser().GetResponseBackFromStream(ms, Debug);
			Debug(FormattableString.Invariant($"** {responseBody.GetType().Name} (Header: {payloadData.Header} Expected length: {payloadData.ExpectedLength}, Actual Length: {payloadData.ActualLength}) - {responseBody.PayloadAsString}"));
			if (responseBody is HandShakeResponse)
			{
				// Do not save handshake responses.
				HandShakeResponse hs = responseBody as HandShakeResponse;
				if (!hs.IsSuccessfulHandShake)
				{
					shakenHandsStatus = ShakeHandStatus.DefinitelyFailed;
					Debug("** Cannot shake hands. " + hs.HandShakeResponseCodeMeaning);
				}
				else
				{
					shakenHandsStatus = ShakeHandStatus.DefinitelySucceeded;
				}
			}
			else
			{
				// We expect to see only inbound cargo messages.  No other types.
				var isActualSameAsExpected = payloadData.ActualLength == payloadData.ExpectedLength;
				var saveException = SaveReceivedBodyToEdiInterchangeTable(responseBody, isActualSameAsExpected);
				AcknowledgeReceiptOfCargoMessageWithCargoResponse(saveException);
				GBCustomsDataRegistry.Instance.CcsukLastReceivedMessageDateTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());
			}
		}

		bool IsMoreDataExpected(byte[] dataReceived, int bytesRead)
		{
			if (bytesRead == 0)
			{
				return false;
			}
			else
			{
				(_, var payloadData) = new ResponseParser().GetResponseBackFromStream(new MemoryStream(dataReceived, 0, bytesRead), Debug);
				return bytesRead < ((payloadData.Header ?? string.Empty).Length + payloadData.ExpectedLength);
			}
		}

		#endregion

		#region Shake hands and log on

		/// <summary>
		/// Main thread
		/// </summary>
		void SendHandshakeButDoNotReadReply(int listeningPort)
		{
			// shake hands to tell ccsuk to phone us... but DO NOT read the reply yet cos they ain't saying owt
			HandShakeRequest hs = new HandShakeRequest(ipAddressPairToTry.CcsukParticipantIpAddress);
			hs.PortNumber = listeningPort;
			Debug("Sending handshake request");
			UploadButDoNotReadResponse(hs);  // handshake response comes in on LISTENING socket
			shakenHandsStatus = ShakeHandStatus.RequestSent;
		}

		/// <summary>
		/// Main thread
		/// </summary>
		public void LogonOrChangePassword()
		{
			ZString newPassword = GBCustomsDataRegistry.Instance.CcsukPassword_New.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
			if (!newPassword.IsEmpty)
			{
				ChangePassword(newPassword);
			}
			Logon();
		}

		/// <summary>
		/// Main thread
		/// </summary>
		protected void Logon()
		{
			LogonMessage logon = new LogonMessage();
			Debug("Logging on....");

			LogonResponse logonResponse = UploadAndReadResponseAndCastToThisType<LogonResponse>(logon, outboundNetworkStream).Data;
			if (!logonResponse.WasOperationSuccessful)
			{
				Debug("...Login failed. Error=" + logonResponse.ReasonForFailure);
				Shutdown();
				throw new Exceptions.ShortMessage.CannotLogonException(logonResponse);
			}
			else
			{
				GBCustomsDataRegistry.Instance.CcsukLastLogonDateTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());
				GBCustomsDataRegistry.Instance.CcsukIsConnected.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				GBCustomsDataRegistry.Instance.CcsukConnectedProcessController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, System.Environment.MachineName);
				GBCustomsDataRegistry.Instance.CcsukProcessID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, System.Diagnostics.Process.GetCurrentProcess().Id);
				Debug("...Logged on");
			}
		}

		/// <summary>
		/// Main thread
		/// </summary>
		void ChangePassword(string newPassword)
		{
			var passwordRequest = new PasswordRequestMessage(newPassword);
			Debug("Requesting to change password to " + newPassword + "...");
			var passwordResponse = UploadAndReadResponseAndCastToThisType<PasswordResponse>(passwordRequest, outboundNetworkStream).Data;
			var responseCode = passwordResponse.ResponseCode;
			if (responseCode == PasswordChangeResponseCodes.Codes.PasswordChangedSuccessfully)  // this response also simultaneously means "logged on OK". 
			{
				Debug("...password changed OK.");
				GBCustomsDataRegistry.Instance.CcsukPassword_New.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
				GBCustomsDataRegistry.Instance.CcsukPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newPassword);
			}
			else if (responseCode == PasswordChangeResponseCodes.Codes.HostDoesNotUsePassword)
			{
				Debug("...password change is unnecessary, wiping new password and calling Logon with old password.");
				GBCustomsDataRegistry.Instance.CcsukPassword_New.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			}
			else
			{
				Debug("...password change unsuccessful, ignoring and trying to call Logon anyway. Reason=" + new PasswordChangeResponseCodes().GetDescriptionFromCode(responseCode));
				GBCustomsDataRegistry.Instance.CcsukPassword_New.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			}
		}

		#endregion

		#region Sending outbound data

		/// <summary>
		/// Main thread
		/// </summary>
		void SendInterchangeAsCargoMessage(EDIInterchange outboundInterchange)
		{
			CusHAWB hawb = null;
			//outboundInterchange.Reload();  // no idea why this is needed, but without it the DB and application get out of synch if an existing interchange is marked as QUE for reprocessing (and that interchange then gets processed over and over).  Pffff.
			if (outboundInterchange.EI_RetryCount > 1)  // Since we have the overhead of a reconnect, only try twice (retry once)
			{
				outboundInterchange.EI_Status = EDIInterchange.Status.Failed;
				Debug(string.Format("Cannot send interchange #{0}, its retry count is too high.", outboundInterchange.EI_InterchangeNum));
			}
			else
			{
				CargoMessage cusDecOrSimilar = new CargoMessage(outboundInterchange.EI_HeaderText + outboundInterchange.EI_BodyText + outboundInterchange.EI_FooterText);
				if (CheckPayloadMaxLength(cusDecOrSimilar.PayloadAsString.Length, outboundInterchange))
				{
					Debug("Sending cargo message for interchange #" + outboundInterchange.EI_InterchangeNum + "...");
					var responseOrMaybeError = UploadAndReadResponseAndCastToThisType<CargoResponse>(cusDecOrSimilar, outboundNetworkStream, outboundInterchange);
					if (responseOrMaybeError.Data != null)
					{
						GBCustomsDataRegistry.Instance.CcsukLastSentMessageDateTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());

						Debug("Cargo message response received. " + responseOrMaybeError.Data.ResponseReasonCode);
						if (responseOrMaybeError.Data.WasOperationSuccessful)
						{
							outboundInterchange.EI_Status = EDIInterchange.Status.Sent;
							hawb = UpdateContainedOutboundMessagesToSentAndMarkLinkedJobAsAssumedOnNetwork(outboundInterchange);
						}
						else
						{
							if (responseOrMaybeError.Data.ShouldRetry)
							{
								outboundInterchange.EI_RetryCount += 1;
								outboundInterchange.Factory.Save();
								Debug("CCS-UK rejected a message but advised retry. Shutting down to reconnect");
								ReconnectOrFail();
							}
							else
							{
								Debug(string.Format("Cannot send interchange #{0}, the upload failed and the response said do not retry.", outboundInterchange.EI_InterchangeNum));
								outboundInterchange.EI_Status = EDIInterchange.Status.Failed;
							}
						}
					}
					else if (responseOrMaybeError.Error != null)
					{
						Debug(string.Format("Response from CCSUK when trying to upload interchange {0} was a 'Short Service Error Message'. Reconnecting. Reason={1}; ", outboundInterchange.EI_InterchangeNum, responseOrMaybeError.Error.ReasonForFailure));
						ReconnectOrFail();
					}
				}
				else
				{
					outboundInterchange.EI_Status = EDIInterchange.Status.Failed;
				}
			}

			using (hawb?.SuspendCalculateNprFromReceipts())
			using (hawb?.MAWB?.SuspendCalculateNprFromReceipts())
			{
				Debug("Saving Updates for interchange #" + outboundInterchange.EI_InterchangeNum + "...", LogType.Debug);
				outboundInterchange.Factory.Save();
				Debug("Saving Complete for interchange #" + outboundInterchange.EI_InterchangeNum + "...", LogType.Debug);
			}
		}

		bool CheckPayloadMaxLength(long lengthOfPayload, EDIInterchange outboundInterchange)
		{
			var isLengthOk = true;
			if (lengthOfPayload > maxLengthOfPayload)
			{
				Debug(string.Format("Cannot send more than {0} bytes to CCSUK. Size was {1} bytes. Outbound interchange #{2} has been failed. ", maxLengthOfPayload, lengthOfPayload, outboundInterchange.EI_InterchangeNum));
				isLengthOk = false;
			}
			return isLengthOk;
		}

		/// <summary>
		/// Main thread
		/// </summary>
		protected CusHAWB UpdateContainedOutboundMessagesToSentAndMarkLinkedJobAsAssumedOnNetwork(EDIInterchange outboundInterchange)
		{
			CusHAWB hawbToReturn = null;
			outboundInterchange.ContainedMessages.Load();

			foreach (EDIMessage msg in outboundInterchange.ContainedMessages)
			{
				msg.EM_Status = EDIMessage.Status.Sent;
				var hawb = msg.EM_LinkedObject as CusHAWB;
				if (hawb != null)
				{
					Debug("Reloading Hawb " + hawb.CS_HAWB + "...", LogType.Debug);
					hawb.Reload();  // Without the reload the current presence is incorrect. This reload will also nullify the outturns collection, forcing a lazy-reload, and thus ensuring that we don't reset the NPR to the wrong (original) value when uploading interchanges			
					Debug("Updating Presence On Network status for " + hawb.CS_HAWB + "...", LogType.Debug);
					hawb.SetPresenceOnNetworkStatusAfterMessageUpload();
					hawbToReturn = hawb;
				}
			}

			return hawbToReturn;
		}

		/// <summary>
		/// Main thread
		/// </summary>
		public bool SendAndReceivePing()
		{
			if (shouldAbortThread)
			{
				return false;
			}
			PollRequestMessage ping = new PollRequestMessage();
			Debug("Sending ping request....");
			var responseOrError = UploadAndReadResponseAndCastToThisType<IncomingPollQuasiResponse>(ping, outboundNetworkStream);
			IncomingPollQuasiResponse pingResponse = responseOrError.Data;
			if (pingResponse != null)
			{
				Debug("Echo received=" + pingResponse.ResponseCode);
			}
			else
			{
				Debug("!!! Could not retrieve echo response. Ignoring and continuing. " + responseOrError.ToString());
			}
			return true;
		}

		/// <summary>
		/// Main thread
		/// </summary>
		/// #if DEBUG
#if DEBUG
		protected virtual
#endif
		void UploadButDoNotReadResponse(Body payload)
		{
			CompleteMessage msg = new CompleteMessage(payload);
			Debug("Upload on outbound socket. Data=" + payload.PayloadAsString);
			outboundStreamWriter.Write(msg.PacketToDeliver);
			outboundStreamWriter.Flush();
		}

		/// <summary>
		/// Main thread
		/// </summary>
#if DEBUG
		protected virtual
#endif
		ResponseOrError<ExpectedType> UploadAndReadResponseAndCastToThisType<ExpectedType>(Body payload, NetworkStream socketStream, EDIInterchange outboundInterchange = null) where ExpectedType : Body
		{
			var response = UploadAndGetResponse(payload, outboundInterchange);
			if (response is ExpectedType)
			{
				return new ResponseOrError<ExpectedType>((ExpectedType)response, null);
			}
			else if (response is ErrorResponse)
			{
				return new ResponseOrError<ExpectedType>(null, (ErrorResponse)response);
			}
			else
			{
				return new ResponseOrError<ExpectedType>(null, null);
			}
		}

		/// <summary>
		/// Main thread
		/// </summary>
#if DEBUG
		protected virtual
#endif
		Body UploadAndGetResponse(Body payloadToUpload, EDIInterchange outboundInterchange)
		{
			ResponseParser responseParser = new ResponseParser();
			UploadButDoNotReadResponse(payloadToUpload);
			Body resp = null;
			try
			{
				int tries = 0;
				while (tries < 30)
				{
					tries++;
					resp = GetResponseFromOutboundStream(responseParser);
					if (resp == null)
					{
						Debug("Response was null, will sleep for 1 second, try #" + tries.ToString());
						Thread.Sleep(1000);
					}
					else
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
				Debug("Trying to read reply on main socket, saw exception. Will reconnect. " + ex.Message);
				ReconnectOrFail();
			}
			if (resp != null)
			{
				Debug("Response on outbound socket. Data=" + resp.PayloadAsString);
			}
			else
			{
				Debug("Trying to read reply on main socket, saw null. Will reconnect.");
				if (outboundInterchange != null)
				{
					outboundInterchange.EI_RetryCount += 1;
					outboundInterchange.Factory.Save();
				}
				ReconnectOrFail();
			}
			return resp;
		}

#if DEBUG
		protected virtual
#endif
		Body GetResponseFromOutboundStream(ResponseParser responseParser)
		{
			var result = responseParser.GetResponseBackFromStream(outboundNetworkStream, Debug);
			return result.body;
		}

		#endregion

		volatile static int reconnectCount;
		volatile ShakeHandStatus shakenHandsStatus;
		enum ShakeHandStatus
		{
			Unknown,
			RequestSent,
			DefinitelyFailed,
			DefinitelySucceeded
		}

		[SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
#if DEBUG
		protected virtual
#endif
		void ReconnectOrFail()
		{
			Debug("Reconnecting after a problem");
			reconnectCount++;
			try
			{
				Shutdown();
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				Debug("Could not shutdown nicely during reconnect. " + ex.Message);
			}
			if (reconnectCount > 10)
			{
				throw new HostedServiceException("Suffered errors too many times during the reconnection process, will not try to reconnect.");
			}
			else
			{
				try
				{
					ConnectAndLogonAndStartReceivingInboundMessages();
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}
					throw new HostedServiceException(string.Format("Could not reconnect after a problem. An initial problem occurred (see previous log entries) and {0} tried to reconnect and logon again, but a further exception was thrown (follows). It could be that CCSUK has gone down for maintenance, your network connectivity or NAT has become faulty, or your router needs rebooting. ", BrandingFactory.Instance.ProductName), ex);
				}
			}
		}

		#region Receive unsolicited data

		/// <summary>
		/// SECOND thread
		/// </summary>
		void AcknowledgeReceiptOfCargoMessageWithCargoResponse(Exception saveException)
		{
			CargoResponse cargoResponse = new CargoResponse(saveException, hostnameShared);
			CompleteMessage msg = new CompleteMessage(cargoResponse);
			byte[] uploadBytes = Encoding.Default.GetBytes(msg.PacketToDeliver);
			Debug("**Sending CargoResponse on second (inbound) socket. Blocking. Data=" + cargoResponse.PayloadAsString);
			try
			{
				if (inboundSocket != null)
				{
					inboundSocket.Send(uploadBytes);
					Debug("**Sent CargoResponse OK.");
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Debug("** !!! Could not send cargo response. " + ex.Message);
			}
			if (saveException != null)
			{
				Debug("** Save was not successful - shutting down and throwing to force a reconnect");
				Shutdown();
				throw new Exceptions.Misc.CannotSaveInboundException(saveException);
			}
		}

		/// <summary>
		/// SECOND thread
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "because ZDateTime incurs a hit to the DB which could be more than 3.333ms round trip ")]
		Exception SaveReceivedBodyToEdiInterchangeTable(Body body, bool isActualSameAsExpected)
		{
			string interchangeStringToSave = body.PayloadAsString;
			EDIInterchange interchange = null;
			try
			{
				if (interchangeStringToSave.StartsWith("<") && !isActualSameAsExpected)
				{
					throw new InvalidDataException("Expected XML payload with actual length not matching expected length");
				}
				if (IsValidXml(interchangeStringToSave, out XmlDocument xmlDocument))
				{
					using (Env.Instance.SuspendBranchAccessError())
					{
						interchange = CDSInterchange.CreateCdsInterchangeFromXML(receivingFactory, xmlDocument);
					}
				}
				else
				{
					using (Env.Instance.SuspendBranchAccessError())
					{
						interchange = EDIInterchange.CreateNewInterchangeFromString(receivingFactory, interchangeStringToSave, ApplicationCodeList.Codes.GbCcsuk);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Debug(string.Format("**Could not make a new instance of an EDIInterchange for received data. Will report failure to CCSUK. Error={0}.  data={1}", ex.Message, interchangeStringToSave));  // this will only happen in the unit test
				return ex;
			}

			interchange.EI_InterchangeNum = interchange.EI_InterchangeNum.MakeUniqueInterchangeNumber();
			interchange.EI_ReceiveTransmit = "RCV";
			interchange.EI_Status = "QUE";
			ZGuid gbPk;
			using (Env.Instance.SuspendBranchAccessError())
			{
				gbPk = RegistryPimaAndBadgeHelper.GetPrimaryBranchPkFromRegistryBasedOnPima(interchange.EI_To.Replace("/", ""), receivingFactory);
			}
			if (!gbPk.IsEmpty)
			{
				interchange.EI_GB = gbPk;  // Set message and interchange to be owned by right branch based on PIMA, rather than just current branch (i.e. StmScheduleTask.S5_GB)
			}
			foreach (EDIMessage receivedMessage in interchange.ContainedMessages)
			{
				receivedMessage.EM_ApplicationCode = CcsukEdiMessageDiverter.GetTargetApplicationCodeBasedOnInterchangeParties(interchange);  // e.g. GBE
				receivedMessage.EM_ReceiveTransmit = "RCV";
				receivedMessage.EM_Status = "QUE";
				receivedMessage.EM_GB = interchange.EI_GB;
			}
			Exception saveException = null;
			if (lastSavedDate != DateTime.MinValue && lastSavedDate.AddMilliseconds(4) > DateTime.UtcNow)
			{
				Thread.Sleep(4);  // EM_SystemCreateTimeUtc is only accurate to 3.333ms so we may need to sleep a tiny bit before saving the next message in order to provide resolution of successive messages using this field (for ordering of received messages to process)
			}
			try
			{
				receivingFactory.Save();
				lastSavedDate = DateTime.UtcNow;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Debug("**Could not save received interchange/messages. " + ex.Message);
				saveException = ex;
				var possibleDeadlockReason = ex.Message;
				if (ex.InnerException != null)
				{
					possibleDeadlockReason += ex.InnerException.Message;
				}
				if (possibleDeadlockReason.Contains("deadlocked on lock resources with another process") || possibleDeadlockReason.Contains("Rerun the transaction"))
				{
					try
					{
						Debug("** Trying to save again due to deadlock...");
						receivingFactory.Save();
						Debug("** ...saved OK this time");
						saveException = null;
					}
					catch (Exception ex2) when (!ex2.IsCriticalException())
					{
						saveException = ex2;
						Debug("**Could not save on second try.  Will report failure to CCS-UK. " + ex2.Message);
					}
				}
			}
			return saveException;
		}

		bool IsValidXml(string xml, out XmlDocument xmlDocument)
		{
			bool result = false;
			xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.LoadXml(xml);
				result = true;
			}
			catch (XmlException)
			{
			}

			return result;
		}

		#endregion

		#region Misc bits

		public void Shutdown()
		{
			GBCustomsDataRegistry.Instance.CcsukIsConnected.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Debug("Shutdown is in progress");
			shouldAbortThread = true;
			if (outboundStreamWriter != null)
			{
				try
				{
					outboundStreamWriter.Close();
					outboundStreamWriter.Dispose();
					outboundStreamWriter = null;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Debug(" !!! Could not stop outboundStreamWriter. Ignoring. " + ex.Message);
				}
			}
			if (outboundNetworkStream != null)
			{
				try
				{
					outboundNetworkStream.Close();
					outboundNetworkStream.Dispose();
					outboundNetworkStream = null;
				}

				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Debug(" !!! Could not stop outboundStream. Ignoring. " + ex.Message);
				}
			}
			if (tcpClientForOutbound != null)
			{
				try
				{
					tcpClientForOutbound.Close();
					tcpClientForOutbound = null;
				}
				catch (SocketException ex)
				{
					Debug(" !!! Could not terminate tcpClient. Ignoring. " + ex.SocketErrorCode + " " + ex.Message);
				}
			}
			if (tcpListener != null)
			{
				try
				{
					tcpListener.Stop();
					tcpListener = null;
				}
				catch (SocketException ex)
				{
					Debug(" !!! Could not terminate tcpListener. Ignoring. " + ex.SocketErrorCode + " " + ex.Message);
				}
			}
			if (inboundSocket != null)
			{
				try
				{
					if (inboundSocket.Connected)  // tcpListener.Stop should do this for us... but just in case
					{
						inboundSocket.Shutdown(SocketShutdown.Both);
						inboundSocket.Close();
					}
					inboundSocket = null;
				}
				catch (SocketException ex)
				{
					Debug(" !!! Could not terminate inboundSocket. Ignoring. " + ex.SocketErrorCode + " " + ex.Message);
				}
			}
			Debug("Joining worker thread...");
			if (dataReceivedWorkerThread != null) // only in testing
			{
				dataReceivedWorkerThread.Join(5000);
			}
			Debug("Shutdown OK");
		}

		[SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "During development it's easier to look at the output window than to look in service task logs (which lag and generally suck).  I will remove this for go-live. ")]
		void Debug(string p)
		{
			System.Diagnostics.Debug.WriteLine(p);  // During development it's easier to look at the output window than to look in service task logs (which lag and generally suck).  I will remove this for go-live.
			if (logger != null)
			{
				logger.Log(LogType.Information, p);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "During development it's easier to look at the output window than to look in service task logs (which lag and generally suck).  I will remove this for go-live. ")]
		void Debug(string p, LogType severity = LogType.Information)
		{
			System.Diagnostics.Debug.WriteLine(p);  // During development it's easier to look at the output window than to look in service task logs (which lag and generally suck).  I will remove this for go-live.
			if (logger != null)
			{
				logger.Log(severity, p);
			}
		}

		IPAddress LocalBoundIpAddress
		{
			get { return ipAddressPairToTry.LocalIpAddress.IsEmpty ? IPAddress.Any : IPAddress.Parse(ipAddressPairToTry.LocalIpAddress); }
		}

		internal IPAddress RemoteHostIpAddress
		{
			get
			{
				if (ipAddressPairToTry.DefaultTransportBasedOnIfHostedWithCargoWiseAndTransportValue == CcsukIpTransportList.Codes.VPN)
				{
					return IPAddress.Parse(GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_VPN);
				}
				else
				{
					return IPAddress.Parse(GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_ADSL);
				}
			}
		}

		#endregion

		#region Local variables

		readonly CcsukIpaddressesSetting ipAddressPairToTry;
		readonly int maxLengthOfPayload;
		volatile bool shouldAbortThread;
		TcpListener tcpListener;
		TcpClient tcpClientForOutbound;
		StreamWriter outboundStreamWriter;
		NetworkStream outboundNetworkStream;
		Socket inboundSocket;
		readonly BusinessObjectFactory sendingFactory;
		BusinessObjectFactory receivingFactory;
		readonly string hostnameShared;
		readonly ILogger logger;
		Thread dataReceivedWorkerThread;
		DateTime lastSavedDate;

		#endregion
	}
}
