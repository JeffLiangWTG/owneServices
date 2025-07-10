using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.Connection;
using Enterprise.Customs.GB.Ccsuk.Connection.Testing;

namespace Enterprise.Customs.GB.Ccsuk.TestProgramExe
{
	class CcsukTestRunner
	{
		public CcsukTestRunner(DebugMethod method)
		{
			this.debugMethod = method;
		}

		public string TempDirForInboundInterchange { get; set; }
		public string DatabaseName { get; set; }
		public string ServerName { get; set; }
		public int ListenPort { get; set; }
		public string ListenIP { get; set; }

		internal void DoBadHandshakeTest()
		{
			handShakeResponseToGive = "***Hand Shake***0002DANIELROXX0003";  // 0003 = bad

			CargoWise.Data.Db.InitializeDatabaseDetails(ServerName, DatabaseName);

			CreateListenerAndStartListeningOnThePortWeWereToldTo();
			AcceptUnsolicitedInboundConnectionAndProcessHandshake();
			ShutDownNicely();
		}

		internal void DoReceiveFromParticipant()
		{
			debug("Writing first debug message to log file...");
			debug("... written.");
			ExpectInitialiseSession();
			ExpectReceiveCargoMessage();
			ExpectLogoff();
		}

		internal void DoSendToParticipant()
		{
			ExpectInitialiseSession();
			SendCargoMessageOnOutboundSocket();
			ExpectLogoff();
		}

		internal void DoSendToParticipantCDS()
		{
			ExpectInitialiseSession();
			SendCargoMessageOnOutboundSocketCDS();
			ExpectLogoff();
		}

		internal void DoSendToParticipantUtf8()
		{
			ExpectInitialiseSession();
			SendCargoMessageOnOutboundSocketUtf8();
			ExpectLogoff();
		}

		internal void DoSendToParticipantInChunks()
		{
			ExpectInitialiseSession();
			SendCargoMessageOnOutboundSocketInChunks();
			ExpectLogoff();
		}

		internal void DoSendToParticipantNothing()
		{
			ExpectInitialiseSession();
			outboundStreamWriter.Close();
			ExpectLogoff();
		}

		internal void ShutDownNicely()
		{
			debug("Shutting down...");
			debug("outboundStreamWriter.Dispose()");
			outboundStreamWriter.Dispose();
			debug("networkStreamOutboundToParticipant.Dispose()");
			networkStreamOutboundToParticipant.Dispose();
			debug("tcpClientOutboundBackToParticipant.Close()");
			tcpClientOutboundBackToParticipant.Close();
			debug("inboundSocket.Shutdown(SocketShutdown.Both)");
			inboundSocket.Shutdown(SocketShutdown.Both);
			debug("tcpListenerInboundFromParticipant.Stop()");
			tcpListenerInboundFromParticipant.Stop();
			debug("Shut down OK.  About to terminate. Thank you for using the CCSUK Faker");
		}

		void ExpectInitialiseSession()
		{
			CargoWise.Data.Db.InitializeDatabaseDetails(ServerName, DatabaseName);  // otherwise the data layer (wrongly) assumes we want to access Odyssey when we ask to look in the registry

			CreateListenerAndStartListeningOnThePortWeWereToldTo();
			AcceptUnsolicitedInboundConnectionAndProcessHandshake();
			while (!isLoggedOn)
			{
				ReadInboundMessageOnMainSocket();  // logon request
			}
			while (!hasPinged)
			{
				ReadInboundMessageOnMainSocket();  // ping request
			}
		}

		void ExpectReceiveCargoMessage()
		{
			debug("Expecting to receive cargo message on second socket");
			RequestParser parser = new RequestParser();
			int bufferSize = 2 + 1 + 15 + 0xA00000;
			bufferForReceive = new byte[bufferSize];
			debug("Receiving data on main socket (Blocking)");
			inboundSocket.Receive(bufferForReceive);
			debug("Received: " + ((ZString)Encoding.Default.GetString(bufferForReceive)).Left(50));
			using (MemoryStream readStream = new MemoryStream(bufferForReceive))
			{
				(CargoMessage cargoMessage, var _) = parser.GetResponseBackFromStream<CargoMessage>(readStream, DebugForTest);
				if (cargoMessage != null && (cargoMessage.PayloadAsString.StartsWith("UNB") || cargoMessage.PayloadAsString.StartsWith("<MetaData>")))
				{
					SendCargoResponse();
					SaveCargoMessagePayloadoFileSystemForTest(cargoMessage);
				}
			}
		}

		void DebugForTest(string info)
		{ }

		void SaveCargoMessagePayloadoFileSystemForTest(CargoMessage cargoMessage)
		{
			System.IO.File.WriteAllText(Path.Combine(TempDirForInboundInterchange, ".interchange.txt"), cargoMessage.PayloadAsString);
		}

		void ExpectLogoff()
		{
			while (isLoggedOn)
			{
				ReadInboundMessageOnMainSocket(); // logoff request
			}
		}

		void SendCargoMessageOnOutboundSocket()
		{
			debug("Sending cargo message on second socket");
			CargoMessage cargoMessage = new CargoMessage("UNB+UNOA:2+CUKCTM98CHFEXP:IATA+CUKFFW98000LXA:IATA+100621:1028+101AA102846000+++A'UNH+09615869266168+CONTRL:4:1:UN+E0B15CA2C31845A48A5322359EE28DDB'UCI+DTICHIEFEDI+CUK98000CAR+CHIEF+4'UCM+22+CUSDEC:D:04A:UN:109730+4'UCS+8'UCD+10+3:1'UNT+6+09615869266168'UNZ+1+101AA102846000'");
			SendOnOutboundSocket(cargoMessage);
			debug("Reading reply on second socket");
			ResponseParser responseParser = new ResponseParser();
			var cargoConfirmation = responseParser.GetResponseBackFromStream<CargoResponse>(networkStreamOutboundToParticipant, DebugForTest);
			debug("Received CargoResponse message on second socket. " + cargoConfirmation.body.PayloadAsString);
		}

		void SendCargoMessageOnOutboundSocketCDS()
		{
			debug("Sending cargo message on second socket");
			CargoMessage cargoMessage = new CargoMessage(@"<MetaData></MetaData><?ccsuk senderid=""CUKSYS98HELP01"" recipientid=""CUKFFW98000LXA""?>");
			SendOnOutboundSocket(cargoMessage);
			debug("Reading reply on second socket");
			ResponseParser responseParser = new ResponseParser();
			var cargoConfirmation = responseParser.GetResponseBackFromStream<CargoResponse>(networkStreamOutboundToParticipant, DebugForTest);
			debug("Received CargoResponse message on second socket. " + cargoConfirmation.body.PayloadAsString);
		}

		void SendCargoMessageOnOutboundSocketUtf8()
		{
			debug("Sending cargo message on second socket");
			CargoMessage cargoMessage = new CargoMessage(@"<MetaData><foo>m³</foo></MetaData><?ccsuk senderid=""CUKSYS98HELP01"" recipientid=""CUKFFW98000LXA""?>");
			SendOnOutboundSocket(cargoMessage);
			debug("Reading reply on second socket");
			ResponseParser responseParser = new ResponseParser();
			var cargoConfirmation = responseParser.GetResponseBackFromStream<CargoResponse>(networkStreamOutboundToParticipant, DebugForTest);
			debug("Received CargoResponse message on second socket. " + cargoConfirmation.body.PayloadAsString);
		}

		void SendCargoMessageOnOutboundSocketInChunks()
		{
			debug("Sending cargo message on second socket");
			var cargoMessage = new CargoMessage(@"<MetaData><Data>Very long long long long long long long long long long long long long long long long message split in chunks</Data></MetaData><?ccsuk senderid=""CUKSYS98HELP01"" recipientid=""CUKFFW98000LXA""?>");
			SendOnOutboundSocket(cargoMessage, 32);
			debug("Reading reply on second socket");
			var responseParser = new ResponseParser();
			var cargoConfirmation = responseParser.GetResponseBackFromStream<CargoResponse>(networkStreamOutboundToParticipant, DebugForTest);
			debug("Received CargoResponse message on second socket. " + cargoConfirmation.body.PayloadAsString);
		}

		void ReadInboundMessageOnMainSocket()
		{
			int bufferSize = 2 + 1 + 15 + 0xA00000;
			bufferForReceive = new byte[bufferSize];
			debug("Receiving data on main socket (Blocking)");
			inboundSocket.Receive(bufferForReceive);
			debug("Received: " + ((ZString)Encoding.Default.GetString(bufferForReceive)).Left(50));
			RequestParser parser = new RequestParser();
			Body request = null;
			using (MemoryStream readStream = new MemoryStream(bufferForReceive))
			{
				var content = parser.GetResponseBackFromStream(readStream, DebugForTest);
				request = content.body;
			}

			if (request != null)
			{
				debug("Object received was a " + request.GetType().Name);
				var logonMessage = request as Connection.Testing.LogonMessage;
				PollRequestMessage pollRequest = request as PollRequestMessage;
				CargoMessage cargoMessage = request as CargoMessage;
				LogoffMessage logoffMessage = request as LogoffMessage;

				if (logonMessage != null && request.PayloadAsString.Contains("SM01"))
				{
					SendLogonSuccessfulResponse();
				}
				else if (pollRequest != null && request.PayloadAsString.Contains("SM09"))
				{
					SendPingResponse();
				}
				else if (cargoMessage != null && request.PayloadAsString.Contains("SM08"))
				{
					SendCargoResponse();
				}
				else if (logoffMessage != null && request.PayloadAsString.Contains("SM03"))
				{
					SendLogoffResponse();
				}
			}
		}

		void SendLogoffResponse()
		{
			LogoffResponse logoffResponse = new LogoffResponse("SM04DANIELROXX0000");
			SendResponseOnInboundSocket(logoffResponse);
			isLoggedOn = false;
		}

		void SendCargoResponse()
		{
			CargoResponse cargoResponse = new CargoResponse("SM08DANIELROXX0000");
			SendResponseOnInboundSocket(cargoResponse);
		}

		void SendPingResponse()
		{
			PollResponseMessage pingResponse = new PollResponseMessage("SM09DANIELROXX0100");  // test-only class
			SendResponseOnInboundSocket(pingResponse);
			hasPinged = true;
		}

		void SendLogonSuccessfulResponse()
		{
			LogonResponse logonResponse = new LogonResponse("SM02DANIELROXX0000");  // successful logon
			SendResponseOnInboundSocket(logonResponse);
			isLoggedOn = true;
		}

		void SendResponseOnInboundSocket(Body body)
		{
			CompleteMessage msg = new CompleteMessage(body);
			byte[] uploadBytes = Encoding.Default.GetBytes(msg.PacketToDeliver);
			debug(string.Format("Sending response on primary socket: {0} {1}", body.GetType().Name, msg.PacketToDeliver));
			inboundSocket.Send(uploadBytes);
			debug("Sent OK");
		}

		void CreateListenerAndStartListeningOnThePortWeWereToldTo()
		{
			tcpListenerInboundFromParticipant = new TcpListener(IPAddress.Parse(ListenIP), ListenPort);
			debug(string.Format("Binding and starting listener on {0}:{1}", ListenIP, ListenPort));
			tcpListenerInboundFromParticipant.Start();
			debug(".... started");
		}

		void AcceptUnsolicitedInboundConnectionAndProcessHandshake()
		{
			debug("About to accept inbound connection on primary socket (blocking)");
			int bufferSize = 2 + 1 + 15 + 0xA00000;
			inboundSocket = tcpListenerInboundFromParticipant.AcceptSocket();  // blocking....
			debug("Accepted from " + inboundSocket.RemoteEndPoint.ToString());
			bufferForReceive = new byte[bufferSize];
			debug("Receiving bytes on primary socket (expecting handshake message)");
			inboundSocket.Receive(bufferForReceive);
			using (MemoryStream stream = new MemoryStream(bufferForReceive))
			{
				RequestParser parser = new RequestParser();

				(HandShakeRequest request, var _) = parser.GetResponseBackFromStream<HandShakeRequest>(stream, DebugForTest);
				if (request != null)
				{
					CallBackAndSendHandShakeResponse(request.IpForCallbackDeclaredByParticipantTESTonly, request.PortNumber);
				}
			}
		}

		void CallBackAndSendHandShakeResponse(string ip, int port)
		{
			debug(string.Format("Received HandShakeRequest on primary socket.  About to phone back at {0}:{1} and send HandShakeResponse", ip, port));
			tcpClientOutboundBackToParticipant = new TcpClient();
			tcpClientOutboundBackToParticipant.Connect(IPAddress.Parse(ip), port);  // blocking for a time-out period, then throws
			debug("Connected");
			SendHandshakeResponse(tcpClientOutboundBackToParticipant);
		}

		void SendHandshakeResponse(TcpClient tcpClient)
		{
			if (networkStreamOutboundToParticipant == null)
			{
				debug("Getting socket stream to which to write");
				networkStreamOutboundToParticipant = tcpClient.GetStream();
				outboundStreamWriter = new StreamWriter(networkStreamOutboundToParticipant);
				outboundStreamWriter.AutoFlush = true;
			}
			HandShakeResponse response = new HandShakeResponse(handShakeResponseToGive);  // this is just a hard-coded successful reply
			SendOnOutboundSocket(response);
		}

		string handShakeResponseToGive = "***Hand Shake***0002DANIELROXX0000";

		void SendOnOutboundSocket(Body response, int chunkSize = 0)
		{
			var msg = new CompleteMessage(response);
			debug(string.Format("Sending business object of type {0} on secondary socket. Data={1}", response.GetType().Name, msg.PacketToDeliver.Left(100)));
			var totalLength = msg.PacketToDeliver.Length;
			if (chunkSize <= 0)
			{
				chunkSize = totalLength;
			}
			for (var offset = 0; offset < totalLength; offset += chunkSize)
			{
				outboundStreamWriter.Write(msg.PacketToDeliver.SubstringSafe(offset, chunkSize));
				if (chunkSize != totalLength)
				{
					Thread.Sleep(100); // Simulate some delays in network for the receiver to get data in chunks
				}
			}
		}

		void debug(string message)
		{
			debugMethod(message, TempDirForInboundInterchange);
		}

		internal int ExitCode { get; private set; }
		StreamWriter outboundStreamWriter;
		NetworkStream networkStreamOutboundToParticipant;
		TcpClient tcpClientOutboundBackToParticipant;
		TcpListener tcpListenerInboundFromParticipant;
		Socket inboundSocket;
		byte[] bufferForReceive;
		bool isLoggedOn;
		bool hasPinged;
		public delegate void DebugMethod(string message, string tempDirectory);
		readonly DebugMethod debugMethod;
	}
}
