using System;
using System.Linq.Expressions;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.Ccsuk.Connection.Testing
{
	public class ExceptionalUpload : TransactionedTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			pair = new CcsukIpaddressesSetting();
			pair.LocalIpAddress = "127.1.2.3";
			pair.CcsukParticipantIpAddress = "172.22.1.2";
		}
		CcsukIpaddressesSetting pair;

		public void TestCheckRegistryOnStartup()
		{
			var expectedException = "Cannot initialise service task, the registry options are insufficient. Check in 'Customs/Country or Region Specific/United Kingdom/Service Providers/CCS-UK/Network' for: Local host mnemonic, password, IP addresses, remote port and maximum payload size. Read the relevant update note thoroughly before changing these options.";
			var logger = new TestServiceLogger();
			AssertExceptionThrown(typeof(HostedServiceException), expectedException, delegate
			{ new TcpIpSenderReceiverForTest(logger, pair); });
			SetupCcsukRegistryOptionsExceptPayloadSize();
			AssertNoExceptionThrown(delegate
			{ new TcpIpSenderReceiverForTest(logger, pair); });
			GBCustomsDataRegistry.Instance.CcsukMaximumTransmittablePayloadSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 49);  // too small
			AssertExceptionThrown(typeof(HostedServiceException), expectedException, delegate
			{ new TcpIpSenderReceiverForTest(logger, pair); });
			GBCustomsDataRegistry.Instance.CcsukMaximumTransmittablePayloadSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0xA00001);  // too big
			AssertExceptionThrown(typeof(HostedServiceException), expectedException, delegate
			{ new TcpIpSenderReceiverForTest(logger, pair); });
			GBCustomsDataRegistry.Instance.CcsukMaximumTransmittablePayloadSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0xA00000);  // maximum acceptable
			AssertNoExceptionThrown(delegate
			{ new TcpIpSenderReceiverForTest(logger, pair); });
			GBCustomsDataRegistry.Instance.CcsukMaximumTransmittablePayloadSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50); // minimum acceptable
			AssertNoExceptionThrown(delegate
			{ new TcpIpSenderReceiverForTest(logger, pair); });
		}

		public void TestBadUploadRetry()
		{
			var sm08Retry = "SM08TESTHOSTXY0202";  // 0202 = failed, but retry
			var logger = new TestServiceLogger();
			SetupCcsukRegistryOptionsExceptPayloadSize();
			pair.LocalIpAddress = "";// for IpAddress.Any local binding
			var sender = new TcpIpSenderReceiverForTest(logger, pair, sm08Retry);
			var factory = new BusinessObjectFactory();
			var queuedInterchange = MakeInterchange(factory);
			factory.Save();
			// Because we have not set-up any of the listeners properly, when we fail we try to reconnect from the very start (instead of jumping straight in at SendAllWaitingOutboundInterchanges), so we expect to see an error due to in ability to (re) connect
			AssertExceptionThrown(typeof(HostedServiceException), $"Could not reconnect after a problem. An initial problem occurred (see previous log entries) and {BrandingFactory.Instance.ProductName}", delegate
			{
				sender.SendAllWaitingOutboundInterchanges();
			}, assertStartsWith: true);
			queuedInterchange.Reload();
			AssertEquals("Failed interchange is still queued", "QUE", queuedInterchange.EI_Status);
			AssertEquals("Failed interchange marked as failing once", 1, queuedInterchange.EI_RetryCount);
			AssertContains(@"Information|Sending cargo message for interchange #DANIEL...
Information|Cargo message response received. 02
Information|CCS-UK rejected a message but advised retry. Shutting down to reconnect
Information|Reconnecting after a problem
Information|Shutdown is in progress
Information|Joining worker thread...
Information|Shutdown OK
Information|======== Begin connectivity ========
Information|Local endpoint for outbound socket will be IPAddress.Any
Information|Connecting outbound socket to IP 127.2.3.4 and port 4014...
", logger.ToString());
		}

		public void TestBadUploadNoRetry()
		{
			var sm08NoRetry = "SM08TESTHOSTXY0101";  // 0101 = failed, do not retry
			var logger = new TestServiceLogger();
			SetupCcsukRegistryOptionsExceptPayloadSize();
			var sender = new TcpIpSenderReceiverForTest(logger, pair, sm08NoRetry);
			var factory = new BusinessObjectFactory();
			var queuedInterchange = MakeInterchange(factory);
			factory.Save();
			sender.SendAllWaitingOutboundInterchanges();
			sender.SendAllWaitingOutboundInterchanges();  // two cycles
			AssertContains("Log shows we try once, fail, and do not retry despite two run cycles",
@"Information|Sending cargo message for interchange #DANIEL...
Information|Cargo message response received. 01
Information|Cannot send interchange #DANIEL, the upload failed and the response said do not retry.
", logger.ToString());
			queuedInterchange.Reload();
			AssertEquals("FAL", queuedInterchange.EI_Status);
			AssertEquals(0, queuedInterchange.EI_RetryCount);
		}

		public void TestNullResponseIncrementsRetryCount()
		{
			var logger = new TestServiceLogger();
			SetupCcsukRegistryOptionsExceptPayloadSize();
			var sender = new TcpIpSenderReceiverWhichGivesNullResponseForTest(logger, pair);
			var factory = new BusinessObjectFactory();
			var queuedInterchange = MakeInterchange(factory);
			factory.Save();
			try
			{
				// this will fail during the attempt to reconnect, meaning that the call to Save() in SendInterchangeAsCargoMessage is never struck
				sender.SendAllWaitingOutboundInterchanges();
			}
			catch
			{
				// this represents top-level handling done by the host
			}
			queuedInterchange.Reload();
			AssertEquals(1, queuedInterchange.EI_RetryCount);
		}

		public void TestGoodUpload()
		{
			var sm08Good = "SM08TESTHOSTXY0000";  // 0000 = success
			var logger = new TestServiceLogger();
			SetupCcsukRegistryOptionsExceptPayloadSize();
			var sender = new TcpIpSenderReceiverForTest(logger, pair, sm08Good);
			var factory = new BusinessObjectFactory();
			var queuedInterchange = MakeInterchange(factory);
			factory.Save();
			sender.SendAllWaitingOutboundInterchanges();
			sender.SendAllWaitingOutboundInterchanges();
			AssertContains("Normal process... try, succeed (00)",
@"Information|Sending cargo message for interchange #DANIEL...
Information|Cargo message response received. 00
", logger.ToString());
			queuedInterchange.Reload();
			AssertEquals("SNT", queuedInterchange.EI_Status);
			AssertEquals(0, queuedInterchange.EI_RetryCount);
		}

		public void TestInterchangeTooLargeToNotEvenTry()
		{
			var sm08Good = "SM08TESTHOSTXY0000";  // 0000 = success
			var logger = new TestServiceLogger();
			SetupCcsukRegistryOptionsExceptPayloadSize();
			var factory = new BusinessObjectFactory();
			var queuedInterchangeTooBig = MakeInterchange(factory);
			System.Threading.Thread.Sleep(100);
			var queuedInterchangeOK = MakeInterchange(factory);
			queuedInterchangeTooBig.EI_InterchangeNum = "LARGE";
			queuedInterchangeTooBig.EI_BodyText += "Large Body Text";
			GBCustomsDataRegistry.Instance.CcsukMaximumTransmittablePayloadSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EndToEndTests.OutboundInterchangeText.Length);
			factory.Save();
			var sender = new TcpIpSenderReceiverForTest(logger, pair, sm08Good);
			sender.SendAllWaitingOutboundInterchanges();
			sender.SendAllWaitingOutboundInterchanges();

			AssertContains("Log reports that one large message can't be uploaded, but it still can go on to try the next queued interchange without stumbling",
@"Information|Cannot send more than 175 bytes to CCSUK. Size was 190 bytes. Outbound interchange #LARGE has been failed.", logger.ToString());

			AssertContains("Log reports that one large message can't be uploaded, but it still can go on to try the next queued interchange without stumbling",
@"Information|Sending cargo message for interchange #DANIEL...", logger.ToString());

			AssertContains("Log reports that one large message can't be uploaded, but it still can go on to try the next queued interchange without stumbling",
@"Information|Cargo message response received. 00", logger.ToString());

			queuedInterchangeOK.Reload();
			queuedInterchangeTooBig.Reload();
			AssertEquals("SNT", queuedInterchangeOK.EI_Status);
			AssertEquals("FAL", queuedInterchangeTooBig.EI_Status);
		}

		public void TestOnlyActiveInterchangesAreSent()
		{
			var sm08Good = "SM08TESTHOSTXY0000";
			var logger = new TestServiceLogger();
			SetupCcsukRegistryOptionsExceptPayloadSize();
			var factory = new BusinessObjectFactory();
			System.Threading.Thread.Sleep(100);
			var queuedInterchangeActive = MakeInterchange(factory);
			var queuedInterchangeNotActive = MakeInterchange(factory);
			queuedInterchangeNotActive.EI_InterchangeNum = "NOTACTIVE";
			queuedInterchangeNotActive.EI_IsActive = false;
			GBCustomsDataRegistry.Instance.CcsukMaximumTransmittablePayloadSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EndToEndTests.OutboundInterchangeText.Length);
			factory.Save();
			var sender = new TcpIpSenderReceiverForTest(logger, pair, sm08Good);
			sender.SendAllWaitingOutboundInterchanges();
			sender.SendAllWaitingOutboundInterchanges();
			AssertContains("Log reports that only one message has been sent",
@"Information|Sending cargo message for interchange #DANIEL...
Information|Cargo message response received. 00
", logger.ToString());
			queuedInterchangeActive.Reload();
			queuedInterchangeNotActive.Reload();
			AssertEquals("Interchange is active so status will be 'SNT'", "SNT", queuedInterchangeActive.EI_Status);
			AssertEquals("Interchange is NOT active so status will remain as 'QUE'", "QUE", queuedInterchangeNotActive.EI_Status);
		}

		static EDIInterchange MakeInterchange(BusinessObjectFactory factory)
		{
			var queuedInterchange = EDIInterchange.CreateNewInterchangeFromString(factory, EndToEndTests.OutboundInterchangeText, "CUK");
			queuedInterchange.EI_ReceiveTransmit = "TRX";
			queuedInterchange.EI_Status = "QUE";
			queuedInterchange.EI_InterchangeNum = "DANIEL";
			return queuedInterchange;
		}

		static void SetupCcsukRegistryOptionsExceptPayloadSize()
		{
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Anything");
			GBCustomsDataRegistry.Instance.CcsukPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Anything");
			GBCustomsDataRegistry.Instance.CcsukRemoteIpAddress_ADSL = "127.2.3.4";
		}

		class TcpIpSenderReceiverForTest : TcpIpSenderReceiver
		{
			public TcpIpSenderReceiverForTest(ILogger logger, CcsukIpaddressesSetting pair, string payloadToReturn = "")
				: base(logger, pair)
			{
				this.payloadToReturn = payloadToReturn;
			}

			protected override ResponseOrError<ExpectedType> UploadAndReadResponseAndCastToThisType<ExpectedType>(Body payload, System.Net.Sockets.NetworkStream socketStream, EDIInterchange outboundInterchange)
			{
				return new ResponseOrError<ExpectedType>(new CargoResponse(payloadToReturn) as ExpectedType, null);
			}

			readonly ZString payloadToReturn;
		}

		class TcpIpSenderReceiverWhichGivesNullResponseForTest : TcpIpSenderReceiver
		{
			public TcpIpSenderReceiverWhichGivesNullResponseForTest(ILogger logger, CcsukIpaddressesSetting pair)
				: base(logger, pair)
			{
			}

			protected override Body GetResponseFromOutboundStream(ResponseParser responseParser) => null;

			protected override void UploadButDoNotReadResponse(Body payload) => Expression.Empty();

			protected override void ReconnectOrFail() => throw new Exception("Could not reconnect, let's pretend there was no handshake, but make sure we still save the new retry count");
		}
	}
}
