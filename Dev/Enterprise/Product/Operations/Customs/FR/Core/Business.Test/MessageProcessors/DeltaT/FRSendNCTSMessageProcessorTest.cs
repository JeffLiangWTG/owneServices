using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	[TestedType(typeof(FRSendNCTSMessageProcessor))]
	public class FRSendNCTSMessageProcessorTest : AutoSendNCTSMessageProcessorTest<FRSendNCTSMessageProcessor>
	{
		public void TestMSNEventLoggedIfMessageSent()
		{
			NctsHeader nctsHeader = base.Factory.New<NctsHeader>();
			PrepareNctsHeader(nctsHeader);
			var processor = CreateProcessor(nctsHeader);
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			processor.Process(notificationBuffer);
			CombineAssertions(() =>
			{
				AssertCollectionContains("MSN event should be added to the NCTS header logs.", "MSN", nctsHeader.Logs.GetAllLogs().Select(l => l.SL_SE_NKEvent));
				AssertEquals("MSN event should have the reference as message type.", "015", nctsHeader.Logs.GetAllLogs().Where(l => l.SL_SE_NKEvent == "MSN").First().SL_Reference);
				Assert("No errors logged if message successfully sent.", !notificationBuffer.HasErrors);
			});
		}

		public void TestMGFEventLoggedIfSendMessageFailed()
		{
			NctsHeader nctsHeader = base.Factory.New<NctsHeader>();
			PrepareNctsHeader(nctsHeader);
			var processor = new FRSendNCTSMessageProcessorForTest(nctsHeader) as IProcessor;
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			processor.Process(notificationBuffer);
			CombineAssertions(() =>
			{
				AssertCollectionContains("MGF event should be added to the NCTS header logs.", "MGF", nctsHeader.Logs.GetAllLogs().Select(l => l.SL_SE_NKEvent));
				AssertEquals("MGF event should have the reference as message type.", "015", nctsHeader.Logs.GetAllLogs().Where(l => l.SL_SE_NKEvent == "MGF").First().SL_Reference);
				Assert("An error logged if message successfully sent.", notificationBuffer.HasErrors);
				AssertEquals("Error Count", 1, notificationBuffer.Events.Length);
				AssertEquals("FR AutoNCTSMessage failed to send for header: NCT00000001, Message Type: 015.\r\n", notificationBuffer.AsString);
			});
		}

		protected override ZString ExpectedMessageDescription => "Delta T (NCTS)";

		protected override void AssertEntryAndMessageResultForEndToEndTest(EU.NCTS.Business.NctsHeader nctsHeader)
		{
			AssertEquals(EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationSent, nctsHeader.EffectiveMessageStatus);

			AssertEquals(1, nctsHeader.Messages.Count);
			var message = nctsHeader.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.FRCustomsMessage, message.EM_ApplicationCode);
			AssertEquals(MessageSubTypeList.Codes.DT, message.EM_MessageSubType);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessageStatusList.Codes.Queued, message.EM_Status);
		}

		protected override IProcessor CreateProcessor(EU.NCTS.Business.NctsHeader nctsHeader)
		{
			return new FRSendNCTSMessageProcessor((NctsHeader)nctsHeader);
		}

		protected override void PrepareNctsHeader(EU.NCTS.Business.NctsHeader nctsHeader)
		{
			base.PrepareNctsHeader(nctsHeader);
			var header = (NctsHeader)nctsHeader;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.DetailedDepartureStatusCode = NctsDetailedStatusList.Codes.BoardingNotification;
		}

		protected override ZString ExpectedNctsHeaderErrors => @"System cannot send Delta T (NCTS) message because of following errors on Job:NCTS12345, please fix all of them and try again.
Error - XA_Data: Data only accepts Western European languages characters.
Error - LocalReferenceNumber: [7] Customer Reference Number only accepts Western European languages characters.";
	}

	public class FRSendNCTSMessageProcessorForTest : FRSendNCTSMessageProcessor
	{
		public FRSendNCTSMessageProcessorForTest(NctsHeader header) : base(header)
		{
		}

		public new IAutoSendNCTSMessageRule[] AutoSendNctsMessageRules => base.AutoSendNctsMessageRules;

		protected override INctsMessageSender MessageSender
		{
			get
			{
				var senderMock = new Mock<INctsMessageSender>();
				senderMock.Setup(x => x.CreateMessage(It.IsAny<NctsHeader>(), It.IsAny<ISendsMessagesToCustoms>(), It.IsAny<NctsMessageFunctionSet>()))
							.Returns(false);
				return senderMock.Object;
			}
		}
	}
}
