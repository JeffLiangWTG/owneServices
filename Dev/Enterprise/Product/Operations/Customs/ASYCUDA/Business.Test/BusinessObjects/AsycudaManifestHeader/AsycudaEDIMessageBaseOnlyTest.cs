using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaEDIMessageBaseOnlyTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestNoUnnessaryCallToSupportNoteMessageInterpretationCore()
		{
			var messageMock1 = Factory.NewMoq<AsycudaEDIMessage>();
			var message1 = messageMock1.Object;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message1.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			messageMock1.Protected()
				.Verify<AsycudaEventMessageInterpretationGenerator>("GetNewAsycudaEventMessageInterpretationGeneratorCore", Times.Never(), ItExpr.IsAny<UniversalEvent>());
			messageMock1.Protected()
				.Setup<bool>("SupportNoteMessageInterpretationCore").Returns(false);
			_ = message1.EM_MessageInterpretation;
			messageMock1.VerifyAll();

			var messageMock2 = Factory.NewMoq<AsycudaEDIMessage>();
			var message2 = messageMock2.Object;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			_ = message2.EM_MessageInterpretation;
			messageMock2.Protected()
				.Verify<bool>("SupportNoteMessageInterpretationCore", Times.Never());
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			_ = message2.EM_MessageInterpretation;
			message2.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message2.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			_ = message2.EM_MessageInterpretation;
			messageMock2.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestUniversalEventAndMessageInterpretationIsCachedCorrectly()
		{
			var messageMock = Factory.NewMoq<AsycudaEDIMessage>();
			var message = messageMock.Object;
			messageMock.Protected()
				.Setup<AsycudaEventMessageInterpretationGenerator>("GetNewAsycudaEventMessageInterpretationGeneratorCore", ItExpr.IsAny<UniversalEvent>())
				.Returns((AsycudaEventMessageInterpretationGenerator)null);
			messageMock.Protected()
				.Setup<bool>("SupportNoteMessageInterpretationCore").Returns(true);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;

			var messageText = @"
<UniversalEvent version=""2.0"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<Workflow>
			<ActionPurpose>{0}</ActionPurpose>
			</Workflow>
		</DataContext>
		<EventTime>2018-01-18T22:49:00</EventTime>
		<EventType>MRR</EventType>
	</Event>
</UniversalEvent>
";
			var data = string.Format(messageText, "123");
			message.EM_MessageText = data;
			var textReader = new StringReader(data);
			messageMock.Setup(m => m.GetEM_MessageTextReader(It.IsAny<bool>())).Returns(textReader);
			_ = message.EM_MessageInterpretation;
			messageMock.VerifyAll();

			messageMock.Reset();
			messageMock.Protected()
				.Verify<AsycudaEventMessageInterpretationGenerator>("GetNewAsycudaEventMessageInterpretationGeneratorCore", Times.Never(), ItExpr.IsAny<UniversalEvent>());
			textReader.Dispose();
			messageMock.Verify(m => m.GetEM_MessageTextReader(It.IsAny<bool>()), Times.Never);
			_ = message.EM_MessageInterpretation;
			messageMock.VerifyAll();
			data = string.Format(messageText, "456");
			message.EM_MessageText = data;
			messageMock.Protected()
				.Setup<AsycudaEventMessageInterpretationGenerator>("GetNewAsycudaEventMessageInterpretationGeneratorCore", ItExpr.IsAny<UniversalEvent>())
				.Returns((AsycudaEventMessageInterpretationGenerator)null);
			textReader = new StringReader(data);
			messageMock.Setup(m => m.GetEM_MessageTextReader(It.IsAny<bool>())).Returns(textReader);
			_ = message.EM_MessageInterpretation;
			messageMock.Verify(m => m.GetEM_MessageTextReader(It.IsAny<bool>()), Times.Never);
			messageMock.Protected()
				.Verify("GetNewAsycudaEventMessageInterpretationGeneratorCore", Times.Never(), ItExpr.IsAny<UniversalEvent>());
			textReader.Dispose();
		}
	}
}
