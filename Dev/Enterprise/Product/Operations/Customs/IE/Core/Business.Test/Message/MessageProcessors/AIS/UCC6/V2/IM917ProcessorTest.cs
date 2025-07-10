using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM917;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM917Processor))]
	class IM917ProcessorTest : EntryHeaderMessageProcessorTest<IM917Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM917Provider>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			(var declaration, var entry, var outgoingMessage, var incomingMessage) = CreateSetupData();
			incomingMessage.EM_MessageText = "<GREETING>HELLO</GREETING>";
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				AssertNoExceptionThrown(() => processor.ProcessMessage(incomingMessage));
				CombineAssertions("Process", () =>
				{
					AssertEquals("EM_Status", EDIMessage.Status.Failed, incomingMessage.EM_Status);
					AssertEquals("CH_Status", ZString.Empty, entry.CH_Status);
					AssertEquals("CH_EntryStatus", ZString.Empty, entry.CH_EntryStatus);
				});
			}
		}

		protected override void AssertProcessResultCore(CusEntryHeader entry, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CH_Status", LogicalStatusList.Codes.Error, entry.CH_Status);
			AssertEquals("CH_EntryStatus", CustomsWareEntryStatusList.Codes.Error, entry.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A Syntax Error Notification (IM917) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Line Number</td><td>1</td></tr><tr><td>Error Reason</td><td>BAD</td></tr><tr><td>Error Column Number</td><td>2</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Line Number</td><td>3</td></tr><tr><td>Error Reason</td><td>THRILLER</td></tr><tr><td>Error Column Number</td><td>4</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Syntax Error Notification (IM917) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "IM917: Syntax Error Notification";

		protected override IM917Processor Processor => new IM917Processor(logger, typeof(Im917));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM917;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText()
		{
			return Serialize(
			new Im917
			{
				XmlNegativeAcknowledgement = new System.Collections.ObjectModel.Collection<XmlNegativeAcknowledgement>(new[]
				{
					new XmlNegativeAcknowledgement
					{
						ErrorLineNumber = "1",
						ErrorColumnNumber = "2",
						ErrorReason = "BAD"
					},
					new XmlNegativeAcknowledgement
					{
						ErrorLineNumber = "3",
						ErrorColumnNumber = "4",
						ErrorReason = "THRILLER"
					}
				})
			});
		}

		public void TestEntryStatus()
		{
			var incomingMessage = CreateNewIncomingMessage();

			var messageAttacheeMock = new Mock<IAISMessageAttachee> { CallBase = true };
			messageAttacheeMock.As<Integration.Customs.IEH7.IAsycudaBill>().SetupGet(s => s.ABL_BillStatus).Returns("ACC");
			var status = Processor.GetEntryStatus(incomingMessage, messageAttacheeMock.Object, Processor.GetDataProvider(incomingMessage));

			AssertNull("AsycudaBill Status", status);

			var messageAttacheeMock1 = new Mock<IAISMessageAttachee> { CallBase = true };
			messageAttacheeMock1.As<Integration.Customs.IE.ICusEntryHeader>().SetupGet(s => s.CH_EntryStatus).Returns("ACC");
			status = Processor.GetEntryStatus(incomingMessage, messageAttacheeMock1.Object, Processor.GetDataProvider(incomingMessage));

			AssertEquals("CusEntryHeader Status", "ERR", status);
		}
	}
}
