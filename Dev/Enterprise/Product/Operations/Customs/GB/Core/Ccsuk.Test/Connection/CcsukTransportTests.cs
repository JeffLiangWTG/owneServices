using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.Connection.Exceptions.Handshake;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.Ccsuk.Connection.Testing
{
	public class CcsukTransportTests : TestCaseWithFactory
	{
		public void TestIpAddressHelperAllGoodAddressesDoesNotContainIPCProfilesIfHosted()
		{
			var localMachinesIpAddresses = Dns.GetHostEntry(System.Environment.MachineName).AddressList;
			var ipV4InternetworkAddress = (from IPAddress a in localMachinesIpAddresses where a.AddressFamily == AddressFamily.InterNetwork select a.ToString()).First();
			var pair1VPN = new CcsukIpaddressesSetting(ipV4InternetworkAddress, "172.3.3.3", "VPN PROFILE", 1);
			pair1VPN.Transport = CcsukIpTransportList.Codes.VPN;
			var pair2IPC = new CcsukIpaddressesSetting(ipV4InternetworkAddress, "172.3.3.3", "IPC PROFILE", 2);
			pair2IPC.Transport = CcsukIpTransportList.Codes.IPC;
			var ipPairs = new CcsukIpAddressesSettingCollection();
			ipPairs.Add(pair1VPN);
			ipPairs.Add(pair2IPC);
			GBCustomsDataRegistry.Instance.CcsukIpAddresses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ipPairs);

			EnvProxy.SetHostedLocationForTest("LON");
			AssertEquals("pre-req", true, EnvProxy.IsHostedWithCargowise);
			var testLogger = new PasswordChangeTests.TestLogger();
			var helper = new IpAddressHelper(testLogger);
			var nextAddressPair = helper.GetNextAddress();
			AssertEquals("VPN profile is allowed when CW1 is hosted", "VPN PROFILE", nextAddressPair.FriendlyName);
			nextAddressPair = helper.GetNextAddress();
			AssertEquals("No more pairs", null, nextAddressPair);
			var loggerString = testLogger.ToString();
			AssertContains("...the IPC profile [IPC PROFILE] cannot be used in a hosted environment", loggerString);

			EnvProxy.SetHostedLocationForTest(string.Empty);
			AssertEquals("pre-req", false, EnvProxy.IsHostedWithCargowise);
			testLogger = new PasswordChangeTests.TestLogger();
			helper = new IpAddressHelper(testLogger);
			nextAddressPair = helper.GetNextAddress();
			AssertEquals("VPN profile is allowed when CW1 is not hosted", "VPN PROFILE", nextAddressPair.FriendlyName);
			nextAddressPair = helper.GetNextAddress();
			AssertEquals("IPC profile is allowed when CW1 is not hosted", "IPC PROFILE", nextAddressPair.FriendlyName);
			nextAddressPair = helper.GetNextAddress();
			AssertEquals("No more pairs", null, nextAddressPair);
			loggerString = testLogger.ToString();
			AssertNotContains("...the IPC profile [IPC PROFILE] cannot be used in a hosted environment", loggerString);
		}

		[TestDate(2015, 8, 22, 14, 00, 00)]
		public void TestShouldPingNow()
		{
			Assert(CcsukSession.ShouldPingNow);  // No last date
			GBCustomsDataRegistry.Instance.CcsukLastPingDateTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZDateTime(2015, 8, 22, 14, 00, 00).ToDateTime());
			Assert(!CcsukSession.ShouldPingNow); // Last pinged 10 mins ago
			GBCustomsDataRegistry.Instance.CcsukLastPingDateTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZDateTime(2015, 8, 22, 13, 49, 59).ToDateTime());
			Assert(CcsukSession.ShouldPingNow); // Last pinged 11 mins ago
			GBCustomsDataRegistry.Instance.CcsukLastPingDateTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZDateTime(2015, 8, 22, 13, 51, 01).ToDateTime());
			Assert(!CcsukSession.ShouldPingNow); // Last pinged 9 mins ago
			GBCustomsDataRegistry.Instance.CcsukLastPingDateTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZDateTime(2015, 8, 22, 13, 55, 01).ToDateTime());
			Assert(!CcsukSession.ShouldPingNow); // Last pinged 5 mins ago

			GBCustomsDataRegistry.Instance.CcsukPingPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 60); // 1 min
			Assert(CcsukSession.ShouldPingNow);  // Last pinged 5 mins ago, but period is now 1 min - shoudl ping
			GBCustomsDataRegistry.Instance.CcsukLastPingDateTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZDateTime(2015, 8, 22, 13, 59, 01).ToDateTime());
			Assert(!CcsukSession.ShouldPingNow); // Last pinged <1 min ago, not due yet
		}

		public void TestSelectionOfLocalIpAddressAgain()
		{
			var localMachinesIpAddresses = Dns.GetHostEntry(System.Environment.MachineName).AddressList;
			var firstIpV4InternetworkAddress = (from IPAddress a in localMachinesIpAddresses where a.AddressFamily == AddressFamily.InterNetwork select a.ToString()).First();  // v4
			var pair1NotAvailable = new CcsukIpaddressesSetting("1.2.3.4", "172.1.1.1", "Not available", 1);
			var pair2IgnoreLoopback = new CcsukIpaddressesSetting("127.2.2.2", "172.2.2.2", "Loopback", 1);
			var pair3UseMe = new CcsukIpaddressesSetting(firstIpV4InternetworkAddress, "172.3.3.3", "Use this one", 3);
			var ipPairs = new CcsukIpAddressesSettingCollection();
			ipPairs.Add(pair1NotAvailable);
			ipPairs.Add(pair2IgnoreLoopback);
			ipPairs.Add(pair3UseMe);
			GBCustomsDataRegistry.Instance.CcsukIpAddresses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ipPairs);
			var testLogger = new PasswordChangeTests.TestLogger();
			var helper = new IpAddressHelper(testLogger);
			var addressPair = helper.GetNextAddress();
			AssertEquals("Check local address. " + testLogger.ToString(), firstIpV4InternetworkAddress, addressPair.LocalIpAddress);
			AssertEquals("Check participant address. " + testLogger.ToString(), "172.3.3.3", addressPair.CcsukParticipantIpAddress);
		}

		public void TestGetNextFreePort()
		{
			// We need to ensure that we consider the local bound IP as well as the port. So if you have two sockets bound to 1.1.1.1:5000 and 2.2.2.2:5000 and you're asking for the next free port for 3.3.3.3, it shoudl give 5000.  
			var addressA = "127.144.1.1";
			var addressB = "127.144.1.2";
			var addressC = "127.144.1.3";

			MakeListenerBoundTo(addressA, 5000);
			AssertNextFreePortIs(5001, addressA);
			AssertNextFreePortIs(5000, addressB);
			AssertNextFreePortIs(5000, addressC);

			MakeListenerBoundTo(addressA, 5001);
			AssertNextFreePortIs(5002, addressA);
			AssertNextFreePortIs(5000, addressB);
			AssertNextFreePortIs(5000, addressC);

			MakeListenerBoundTo(addressB, 5000);
			AssertNextFreePortIs(5002, addressA);
			AssertNextFreePortIs(5001, addressB);
			AssertNextFreePortIs(5000, addressC);

			MakeListenerBoundTo(addressB, 5001);
			AssertNextFreePortIs(5002, addressA);
			AssertNextFreePortIs(5002, addressB);
			AssertNextFreePortIs(5000, addressC);

			MakeListenerBoundTo(addressB, 5002);
			AssertNextFreePortIs(5002, addressA);
			AssertNextFreePortIs(5003, addressB);
			AssertNextFreePortIs(5000, addressC);

			MakeListenerBoundTo(addressB, 5003);
			AssertNextFreePortIs(5002, addressA);
			AssertNextFreePortIs(5004, addressB);
			AssertNextFreePortIs(5000, addressC);

			MakeListenerBoundTo(addressB, 5004);
			AssertNextFreePortIs(5002, addressA);
			AssertNextFreePortIs(5005, addressB);
			AssertNextFreePortIs(5000, addressC);

			MakeListenerBoundTo(addressB, 5005);
			AssertNextFreePortIs(5002, addressA);
			AssertNextFreePortIs(-1, addressB);  // limit blown
			AssertNextFreePortIs(5000, addressC);

			listeners.ForEach(l => l.Stop());
		}

		void AssertNextFreePortIs(int expectedNextFreePort, string ipAddress)
		{
			var actualNextFreePort = IpAddress.FindNextAvailablePortInRangeOnAddress(5000, 5005, IPAddress.Parse(ipAddress));
			AssertEquals("For local IP address " + ipAddress + " we expect the next free port to be " + actualNextFreePort, expectedNextFreePort, actualNextFreePort);
		}

		List<TcpListener> listeners;

		void MakeListenerBoundTo(string ipAddress, int port)
		{
			var listener = new TcpListener(IPAddress.Parse(ipAddress), port);
			listener.Start();
			if (listeners == null)
			{
				listeners = new List<TcpListener>();
			}
			listeners.Add(listener);
		}

		public void TestStreamReader()
		{
			byte[] bytesToBeRead = StringToByteArray("05212SM08TESTHOSTAA0201");
			Stream stream = new MemoryStream(bytesToBeRead);
			var result = new ResponseParser().ReadPayloadFromStream(stream, DebugForTest);
			CombineAssertions("Body Only", () =>
			{
				AssertEquals("SM08TESTHOSTAA0201", result.payload);  // body only :)
				AssertEquals("Header", "05212", result.payloadData.Header);
				AssertEquals("Expected Len (12Hex)", 18, result.payloadData.ExpectedLength);
				AssertEquals("Actual Len", 18, result.payloadData.ActualLength);
			});

			bytesToBeRead = StringToByteArray("05212SM08TESTHOSTAA0201                    ");
			stream = new MemoryStream(bytesToBeRead);
			result = new ResponseParser().ReadPayloadFromStream(stream, DebugForTest);
			CombineAssertions("Extra empty data", () =>
			{
				AssertEquals("SM08TESTHOSTAA0201", result.payload);  // body only, without trailing crap
				AssertEquals("Header", "05212", result.payloadData.Header);
				AssertEquals("Expected Len (12Hex)", 18, result.payloadData.ExpectedLength);
				AssertEquals("Actual Len", 18, result.payloadData.ActualLength);
			});

			bytesToBeRead = StringToByteArray("05220SM08TESTHOSTAA0201");
			stream = new MemoryStream(bytesToBeRead);
			result = new ResponseParser().ReadPayloadFromStream(stream, DebugForTest);
			CombineAssertions("Stream shorter than expected", () =>
			{
				AssertEquals("SM08TESTHOSTAA0201", result.payload);
				AssertEquals("Header", "05220", result.payloadData.Header);
				AssertEquals("Expected Len (20Hex)", 32, result.payloadData.ExpectedLength);
				AssertEquals("Actual Len", 18, result.payloadData.ActualLength);
			});

			bytesToBeRead = StringToByteArray("05212SM02DDILHR    0000");
			stream = new MemoryStream(bytesToBeRead);
			var response = new ResponseParser().GetResponseBackFromStream(stream, DebugForTest);
			CombineAssertions("Real text from CCSUK", () =>
			{
				AssertEquals(typeof(LogonResponse), response.body.GetType());
				AssertEquals("SM02DDILHR    0000", response.body.PayloadAsString);
				AssertEquals("Header", "05212", response.payloadData.Header);
				AssertEquals("Expected Len (12Hex)", 18, response.payloadData.ExpectedLength);
				AssertEquals("Actual Len", 18, response.payloadData.ActualLength);
			});
		}

		#region TEST MAKING MESSAGES

		public void TestMakeLogon()
		{
			Connection.LogonMessage req = new Connection.LogonMessage();  // i.e. live
			AssertEquals("SM01TESTHOSTXYLET ME IN     ", req.PayloadAsString);
		}

		public void TestMakeLogonWithShortHost()
		{
			GB.Registry.GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Shorty");
			Connection.LogonMessage req = new Connection.LogonMessage();
			AssertEquals("SM01SHORTY    LET ME IN     ", req.PayloadAsString);
			GB.Registry.GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestHostXy");
		}

		public void TestMakeLogoff()
		{
			LogoffMessage req = new LogoffMessage();
			AssertEquals("SM03TESTHOSTXY", req.PayloadAsString);
		}

		public void TestMakePassword()
		{
			PasswordRequestMessage req = new PasswordRequestMessage("secret new pwd");
			AssertEquals("SM06TESTHOSTXYLET ME IN     SECRET NEW PWD", req.PayloadAsString);
		}

		public void TestMakeConnectionPoll()
		{
			PollRequestMessage req = new PollRequestMessage();
			AssertEquals("SM09TESTHOSTXY0000", req.PayloadAsString);
		}

		public void TestMakeFullMessageWithProtocolHeader()
		{
			CompleteMessage message = new CompleteMessage();
			message.Body = new CargoMessage("ABCDEFGHIJLKMNOPQR");
			AssertEquals("05212ABCDEFGHIJLKMNOPQR", message.PacketToDeliver);

			message.Body = new CargoMessage("ABCDEFGHIJLKMNOPQ");  // one char shorter
			AssertEquals("05211ABCDEFGHIJLKMNOPQ", message.PacketToDeliver);

			message.Body = new CargoMessage("X");
			AssertEquals("0411X", message.PacketToDeliver);  // length of 'X' is 1.  Length of '1' is 1. Length of '11' is 2.  2 + 2 = 04. 			

			string body = "ssdgfjhsgfsgfjhsgfjhgfhjfwitbrviuwartbvirtcywaetbrtrciuawcrtbjdgfcjhzgfjhgjsdfgsdjfgsjfgsjdfgsjdfgsdjfgssdgfjhsgfsgfjhsgfjhgfhjfwitbrviuwartbvirtcywaetbrtrciuawcrtbjdgfcjhzgfjhgjsdfgsdjfgsjfgsjdfgsjdfgsdjfgssdgfjhsgfsgfjhsgfjhgfhjfwitbrviuwartbvirtcywaetbrtrciuawcrtbjdgfcjhzgfjhgjsdfgsdjfgsjfgsjdfgsjdfgsdjfgssdgfjhsgfsgfjhsgfjhgfhjfwitbrviuwartbvirtcywaetbrtrciuawcrtbjdgfcjhzgfjhgjsdfgsdjfgsjfgsjdfgsjdfgsdjfgssdgfjhsgfsgfjhsgfjhgfhjfwitbrviuwartbvirtcywaetbrtrciuawcrtbjdgfcjhzgfjhgjsdfgsdjfgsjfgsjdfgsjdfgsdjfgssdgfjhsgfsgfjhsgfjhgfhjfwitbrviuwartbvirtcywaetbrtrciuawcrtbjdgfcjhzgfjhgjsdfgsdjfgsjfgsjdfgsjdfgsdjfgssdgfjhsgfsgfjhsgfjhgfhjfwitbrviuwartbvirtcywaetbrtrciuawcrtbjdgfcjhzgfjhgjsdfgsdjfgsjfgsjdfgsjdfgsdjfg sjfg sjfg suyft dgc jsf jksadgh fhegrh g";
			message.Body = new CargoMessage(body);
			AssertEquals("0632FA" + body, message.PacketToDeliver);

			// These below sent by Aileen @ CCSUK, so they are defo real:
			message.Body = new CargoMessage("SM02DDILHR    0000");
			AssertEquals("05212SM02DDILHR    0000", message.PacketToDeliver);

			message.Body = new CargoMessage("SM01DDILHR    DDILHR00000000");
			AssertEquals("0521CSM01DDILHR    DDILHR00000000", message.PacketToDeliver);

			message.Body = new CargoMessage("***Hand Shake***0001DDILHR    10.220.117.68  05007");
			AssertEquals("05232***Hand Shake***0001DDILHR    10.220.117.68  05007", message.PacketToDeliver);
		}

		public void TestMakeHandShake()
		{
			HandShakeRequest hs = new HandShakeRequest("193.113.92.16");
			hs.PortNumber = 1234;
			AssertEquals("***Hand Shake***0001TESTHOSTXY193.113.92.16  01234", hs.PayloadAsString);
		}

		public void TestPortIsShutdownWhenCannotLogonException()
		{
			var localMachinesIpAddresses = Dns.GetHostEntry(System.Environment.MachineName).AddressList;
			var firstIpV4InternetworkAddress = (from IPAddress a in localMachinesIpAddresses where a.AddressFamily == AddressFamily.InterNetwork select a.ToString()).First();  // v4
			var validAddress = new CcsukIpaddressesSetting(firstIpV4InternetworkAddress, "172.3.3.3", "Use this one", 1);
			var item2 = MakeNewCcsukIpAddress(2);
			var item3 = MakeNewCcsukIpAddress(3);
			var coll = new CcsukIpAddressesSettingCollection();
			coll.Add(validAddress);
			coll.Add(item2);
			coll.Add(item3);

			GBCustomsDataRegistry.Instance.CcsukIpAddresses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, coll);
			var testLogger = new PasswordChangeTests.TestLogger();
			var helper = new IpAddressHelper(testLogger);
			var addressPair = helper.GetNextAddress();

			var session = new CcsukSessionForShutdownTest(testLogger);

			var loggerString = testLogger.ToString();
			AssertContains("Shutdown is in progress", loggerString);
		}

		public void TestCannotLogonException()
		{
			var anyUser = Factory.NewWithValidTestData<GlbStaff>();
			anyUser.GS_EmailAddress = "wtg@wisetechglobal.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(anyUser);
			Factory.Save();
			GBCustomsDataRegistry.Instance.NotificationCcsukErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			var host = Factory.New<StmServiceHost>();
			host.SH_HostName = "ServiceHostForTest";
			Factory.Save();

			var localMachinesIpAddresses = Dns.GetHostEntry(System.Environment.MachineName).AddressList;
			var firstIpV4InternetworkAddress = (from IPAddress a in localMachinesIpAddresses where a.AddressFamily == AddressFamily.InterNetwork select a.ToString()).First();  // v4
			var validAddress = new CcsukIpaddressesSetting(firstIpV4InternetworkAddress, "172.3.3.3", "Use this one", 1);
			var item2 = MakeNewCcsukIpAddress(2);
			var item3 = MakeNewCcsukIpAddress(3);
			var coll = new CcsukIpAddressesSettingCollection();
			coll.Add(validAddress);
			coll.Add(item2);
			coll.Add(item3);

			var governorMock = new Mock<IServiceManagerGovernor>();

			using (ObjectFactory.Substitute(governorMock.Object))
			{
				GBCustomsDataRegistry.Instance.CcsukIpAddresses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, coll);
				var testLogger = new PasswordChangeTests.TestLogger();
				var helper = new IpAddressHelper(testLogger);
				var addressPair = helper.GetNextAddress();

				var session = new CcsukSessionForExceptionTest(testLogger);

				var loggerString = testLogger.ToString();
				AssertContains("CW1 will not attempt to use any other profiles as no amount of retries will resolve the login failure", loggerString);
				AssertContains("CUK service task has been deactivated", loggerString);

				governorMock.Verify(g => g.SetServiceTaskIsActive("CUK", false), Times.Once);

				AssertEquals(1, Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("wtg@wisetechglobal.com", Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.RecipientsAsDelimitedString());
				var body = Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body;
				AssertContains(@"Could not log on to CCSUK network.  This is most likely because the password is incorrect or the host has been barred.<BR/>
The CUK service task has been deactivated.<BR/>
", body);
				AssertContains("Exception message:<BR/>", body);
				AssertContains(BrandingFactory.Instance.ProductName, body);
			}
		}

		// Copy-pasted from Customs/GB/Core/Registry.Test/Business/CcsUkIpAddressesTests.cs to avoid project reference.
		static CcsukIpaddressesSetting MakeNewCcsukIpAddress(int index)
		{
			var ip = new CcsukIpaddressesSetting();
			ip.LocalIpAddress = "10.44.1." + index.ToString();
			ip.CcsukParticipantIpAddress = "172.22.1." + index.ToString();
			ip.Sequence = index * 10;
			ip.FriendlyName = "Daniel." + index;
			return ip;
		}

		#endregion

		#region TEST UNDERSTAND INCOMING MESSAGES

		public void TestShortMessageResponseParser()
		{
			// The 0000 are not necessarily meaningful

			string logonResponse = "SM02TESTHOSTXY0000";
			string logoffResponse = "SM04TESTHOSTXY0000";
			string errorResponse = "SM05TESTHOSTXY0000";
			string passwordResponse = "SM07TESTHOSTXY0000";
			string cargoResponse = "SM08TESTHOSTXY0000";
			string pollResponse = "SM09TESTHOSTXY0000";

			ResponseParser parser = new ResponseParser();
			Dictionary<string, Type> types = new Dictionary<string, Type>();
			types.Add(logonResponse, typeof(LogonResponse));
			types.Add(logoffResponse, typeof(LogoffResponse));
			types.Add(errorResponse, typeof(ErrorResponse));
			types.Add(cargoResponse, typeof(CargoResponse));
			types.Add(passwordResponse, typeof(PasswordResponse));
			types.Add(pollResponse, typeof(IncomingPollQuasiResponse));
			Body result;

			foreach (string bodyText in types.Keys)
			{
				Type t = types[bodyText];
				string messageForDat = string.Format("Input '{0}' should get Body of type '{1}'", bodyText, t.Name);
				result = parser.LoadMessageFromText(bodyText);
				AssertEquals(messageForDat, t, result.GetType());
			}
		}

		public void TestUnderstandLogonResponse()
		{
			LogonResponse response = new LogonResponse("SM02TESTHOSTXY0000");
			AssertEquals(true, response.WasOperationSuccessful);

			response = new LogonResponse("SM02TESTHOSTXY0100");
			AssertEquals(false, response.WasOperationSuccessful);
			AssertEquals("Host Identity Invalid", response.ReasonForFailure);
		}

		public void TestUnderstandLogoffResponse()
		{
			LogoffResponse response = new LogoffResponse("SM04TESTHOSTXY0000");
			AssertEquals(true, response.WasOperationSuccessful);

			response = new LogoffResponse("SM04TESTHOSTXY0200");
			AssertEquals(false, response.WasOperationSuccessful);
			AssertEquals("No Logon Session current on this circuit", response.ReasonForFailure);
		}

		public void TestUnderstandErrorResponse()
		{
			ErrorResponse response = new ErrorResponse("SM05TESTHOSTXY0000");
			AssertEquals(false, response.WasOperationSuccessful);  // All errors imply failure

			response = new ErrorResponse("SM05TESTHOSTXY0300");
			AssertEquals(false, response.WasOperationSuccessful);
			AssertEquals("Unknown Service Message", response.ReasonForFailure);
		}

		public void TestUnderstandPasswordResponse()
		{
			PasswordResponse response = new PasswordResponse("SM07TESTHOSTXY0000");
			AssertEquals(true, response.WasOperationSuccessful);
			response.SaveIfSuccessful("foo");
			AssertEquals("foo", GB.Registry.GBCustomsDataRegistry.Instance.CcsukPassword.Value);

			response = new PasswordResponse("SM07TESTHOSTXY0400");
			AssertEquals(false, response.WasOperationSuccessful);
			AssertEquals("New password same as existing password", response.ReasonForFailure);
		}

		public void TestUnderstandCargoResponse()
		{
			CargoResponse response = new CargoResponse("SM08TESTHOSTXY0000");
			AssertEquals(true, response.WasOperationSuccessful);

			response = new CargoResponse("SM08TESTHOSTXY0101");
			AssertEquals(false, response.WasOperationSuccessful);
			AssertEquals(false, response.ShouldRetry);

			response = new CargoResponse("SM08TESTHOSTXY0202");
			AssertEquals(false, response.WasOperationSuccessful);
			AssertEquals(true, response.ShouldRetry);
			AssertEquals("The cargo message was not delivered due to an internal error at the receiving end. The Participant System should terminate the Application and Socket Sessions, re-establish Socket and Application Sessions and then re-try the message.",
				response.ReasonForFailure);
		}

		public void TestUnderstandHandShakeResponseSuccess()
		{
			string bodyString = "***Hand Shake***0002TESTHOSTXY0000";
			ResponseParser parser = new ResponseParser();
			Body body = parser.LoadMessageFromText(bodyString);
			HandShakeResponse handShakeResponse = body as HandShakeResponse;
			AssertNotNull(handShakeResponse);
			AssertEquals(HandShakeResponseCodes.Descriptions.ValidParticipant, handShakeResponse.HandShakeResponseCodeMeaning);
			AssertEquals(true, handShakeResponse.IsSuccessfulHandShake);
		}

		public void TestUnderstandHandShakeResponseFailed()
		{
			string bodyString = "***Hand Shake***0002TestHostXY0003";
			ResponseParser parser = new ResponseParser();
			Body body = parser.LoadMessageFromText(bodyString);
			HandShakeResponse handShakeResponse = body as HandShakeResponse;
			AssertNotNull(handShakeResponse);
			AssertEquals(HandShakeResponseCodes.Descriptions.IpMismatch, handShakeResponse.HandShakeResponseCodeMeaning);
			AssertEquals(false, handShakeResponse.IsSuccessfulHandShake);
		}

		public void TestResponseParserDoesNotThrowExceptionWithBadStream()
		{
			var parser = new ResponseParser();
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			string streamData = "ThisStreamIsInvalid";
			writer.Write(streamData);
			writer.Flush();
			stream.Position = 0;
			AssertExceptionThrown("Unexpected exception thrown!",
				typeof(InvalidDataException),
				string.Format("Invalid Hexadecimal character lengthOfBodyLength: '{0}',  Header Position: 0\r\nStream Data:\r\n{1}", streamData[2], streamData),
				() => parser.ReadPayloadFromStream(stream, DebugForTest));
		}

		public void TestCargoResponse_MakeResponseBody()
		{
			var hostName = "UnitTest";
			Exception fakedException = null;
			var response = new CargoResponse(fakedException, hostName);
			AssertEquals(true, response.WasOperationSuccessful);

			fakedException = new ApplicationException("Some random and unexpected exception");
			response = new CargoResponse(fakedException, hostName);
			AssertEquals(false, response.WasOperationSuccessful);
			AssertEquals(true, response.ShouldRetry);

			fakedException = new MessageProcessingException("Simulated interchange extraction failure", "Who cares", false, false);
			response = new CargoResponse(fakedException, hostName);
			AssertEquals(false, response.WasOperationSuccessful);
			AssertEquals(false, response.ShouldRetry);
		}

		public void TestCargoResponseWithDodgyMessage()
		{
			var pair = new CcsukIpaddressesSetting();
			pair.LocalIpAddress = "127.1.2.3";
			pair.CcsukParticipantIpAddress = "172.22.1.2";
			var logger = new TestServiceLogger();
			var tcpIp = new TcpIpSenderReceiverForTest(logger, pair);

			var msg = "05241This is some very dodgy data which is not a valid EDIFACT Message";

			AssertExceptionThrown(typeof(Exceptions.Misc.CannotSaveInboundException), () => tcpIp.DataReceivedTest(StringToByteArray(msg)));
			AssertEquals(7, logger.Count);
			AssertContains("Data=SM08TESTHOSTXY0201", logger[2]);
		}

		public void TestResponseWithInvalidMessageLength()
		{
			var pair = new CcsukIpaddressesSetting();
			pair.LocalIpAddress = "127.1.2.3";
			pair.CcsukParticipantIpAddress = "172.22.1.2";
			var logger = new TestServiceLogger();
			var tcpIp = new TcpIpSenderReceiverForTest(logger, pair);

			var msg = "05263This message is shorter than the 99 bytes specified (hex 63)";

			AssertExceptionThrown(typeof(Exceptions.Misc.CannotSaveInboundException), () => tcpIp.DataReceivedTest(StringToByteArray(msg)));
			AssertEquals(7, logger.Count);
			AssertContains("** CargoMessage (Header: 05263 Expected length: 99, Actual Length: 60) - This message is shorter than the 99 bytes specified (hex 63)", logger[0]);
		}

		public void TestResponseWithMismatchedXMLMessageLength()
		{
			var pair = new CcsukIpaddressesSetting();
			pair.LocalIpAddress = "127.1.2.3";
			pair.CcsukParticipantIpAddress = "172.22.1.2";
			var logger = new TestServiceLogger();
			var tcpIp = new TcpIpSenderReceiverForTest(logger, pair);

			var msg = "05214<xml>TEST DATA</";

			AssertExceptionThrown(typeof(Exceptions.Misc.CannotSaveInboundException), () => tcpIp.DataReceivedTest(StringToByteArray(msg)));
			AssertEquals(7, logger.Count);
			AssertContains("** CargoMessage (Header: 05214 Expected length: 20, Actual Length: 16) - <xml>TEST DATA</", logger[0]);
			AssertContains("Expected XML payload with actual length not matching expected length", logger[1]);
		}

		string handShakeBodyToParseKnownToBeCrap;
		public void TestUnderstandHandShakeResponseCrap()
		{
			this.handShakeBodyToParseKnownToBeCrap = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";
			AssertExceptionThrown(typeof(BadHandshakeIdentifier), RunHandshakeParser);

			this.handShakeBodyToParseKnownToBeCrap = "***Hand Shake***0003TestHostXY0000";  // 3 bad
			AssertExceptionThrown(typeof(BadHandshakeMessageType), RunHandshakeParser);

			this.handShakeBodyToParseKnownToBeCrap = "***Hand Shake***0002TestHostXY0004";  // 4 is unknown code
			AssertExceptionThrown(typeof(UnexpectedResponseCode), RunHandshakeParser);
		}

		void RunHandshakeParser()
		{
			HandShakeResponse response = new HandShakeResponse(this.handShakeBodyToParseKnownToBeCrap);
		}

		void DebugForTest(string info)
		{ }

		#endregion

		#region Impl

		protected override void SetUp()
		{
			base.SetUp();
			SetupRego();
		}

		void SetupRego()
		{
			GB.Registry.GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestHostXy");
			GB.Registry.GBCustomsDataRegistry.Instance.CcsukPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "let me in");
			GB.Registry.GBCustomsDataRegistry.Instance.CcsukNetworkIpToDeclareForCallBack_Legacy.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "193.113.92.16");
		}

		public static byte[] StringToByteArray(string str)
		{
			System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
			return encoding.GetBytes(str);
		}

		#endregion

		class TcpIpSenderReceiverForTest : TcpIpSenderReceiver
		{
			public TcpIpSenderReceiverForTest(ILogger logger, CcsukIpaddressesSetting ipAddressPairToTry) : base(logger, ipAddressPairToTry)
			{
			}

			public void DataReceivedTest(byte[] dataReceived)
			{
				DataReceived(dataReceived);
			}
		}

		class TcpIpSenderReceiverForTestThatThrowsCannotLogonException : TcpIpSenderReceiver
		{
			public TcpIpSenderReceiverForTestThatThrowsCannotLogonException(ILogger logger, CcsukIpaddressesSetting pair, string payloadToReturn = "SM02TESTHOSTXY0100")
				: base(logger, pair)
			{
				this.payloadToReturn = payloadToReturn;
			}

			protected override void ConnectAndLogonAndStartReceivingInboundMessagesCore()
			{
				throw new Exceptions.ShortMessage.CannotLogonException(new LogonResponse("SM02TESTHOSTXY0100"));
			}

			protected override ResponseOrError<ExpectedType> UploadAndReadResponseAndCastToThisType<ExpectedType>(Body payload, NetworkStream socketStream, EDIInterchange outboundInterchange = null)
			{
				var response = new LogonResponse(payloadToReturn);
				return new ResponseOrError<ExpectedType>(response as ExpectedType, null);
			}

			protected ZString payloadToReturn;
		}

		class TcpIpSenderReceiverForShutdownTest : TcpIpSenderReceiverForTestThatThrowsCannotLogonException
		{
			public TcpIpSenderReceiverForShutdownTest(ILogger logger, CcsukIpaddressesSetting pair)
				: base(logger, pair)
			{
			}

			protected override void ConnectAndLogonAndStartReceivingInboundMessagesCore()
			{
				Logon();
			}
		}

		class CcsukSessionForExceptionTest : CcsukSession
		{
			public CcsukSessionForExceptionTest(ILogger logger) : base(logger)
			{
			}

			protected override TcpIpSenderReceiver GetTcpIpSenderReceiver(ILogger logger, CcsukIpaddressesSetting ipAddress) => new TcpIpSenderReceiverForTestThatThrowsCannotLogonException(logger, ipAddress);
		}

		class CcsukSessionForShutdownTest : CcsukSession
		{
			public CcsukSessionForShutdownTest(ILogger logger) : base(logger)
			{
			}

			protected override TcpIpSenderReceiver GetTcpIpSenderReceiver(ILogger logger, CcsukIpaddressesSetting ipAddress) => new TcpIpSenderReceiverForShutdownTest(logger, ipAddress);
		}
	}
}
