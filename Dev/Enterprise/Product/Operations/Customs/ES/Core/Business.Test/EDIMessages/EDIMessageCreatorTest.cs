using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.Customs.ES.Business.EDIMessages.Testing
{
	public class EDIMessageCreatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("When MessageBuilder is null", () => new EDIMessageCreator(null, Factory));
				AssertExceptionThrown<ArgumentNullException>("When Factory is null", () => new EDIMessageCreator(new Mock<IMessageBuilderBase>().Object, null));
			});
		}

		public void TestCreateMessage()
		{
			var expectedMessageType = "AAA";
			var expectedMessageSubType = "BBB";
			var expectedMessageText = "Message Text";
			var expectedIsTestMessage = true;
			var expectedCertificateName = "CertName";
			var certificate = Factory.New<Enterprise.MasterFiles.Business.GlbExternalPassword>();
			var expectedCertificatePK = certificate.PK;
			var expectedBusinessObjectReference = "Reference";

			var builderMock = new Mock<IMessageBuilderBase>();
			builderMock.Setup(m => m.MessageType).Returns(expectedMessageType);
			builderMock.Setup(m => m.MessageSubType).Returns(expectedMessageSubType);
			builderMock.Setup(m => m.UnsignedMessageText).Returns("Text");
			builderMock.Setup(m => m.GetSignedMessageText()).Returns(expectedMessageText);

			var providerMock = new Mock<IESEDIMessageCollectionProvider>();
			providerMock.Setup(m => m.IsTest).Returns(expectedIsTestMessage);
			providerMock.Setup(m => m.CertificateName).Returns(expectedCertificateName);
			providerMock.Setup(m => m.CertificatePK).Returns(expectedCertificatePK);
			providerMock.Setup(m => m.BusinessObjectReference).Returns(expectedBusinessObjectReference);
			builderMock.Setup(m => m.Provider).Returns(providerMock.Object);

			var messageCreator = new EDIMessageCreator(builderMock.Object, Factory);
			var message = messageCreator.CreateMessage();

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", expectedMessageType, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", expectedMessageSubType, message.EM_MessageSubType);
				AssertEquals("message.EM_IsTestMessage", expectedIsTestMessage, message.EM_IsTestMessage);
				AssertEquals("message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("message.EM_MessageNum", "1", message.EM_MessageNum);
				AssertEquals("message.EM_ApplicationReference", expectedCertificateName, message.EM_ApplicationReference);
				AssertEquals("message.EM_GP", expectedCertificatePK, message.EM_GP);
				AssertEquals("message.BusinessObjectReference", expectedBusinessObjectReference, message.BusinessObjectReference);
				AssertEquals("message.EM_MessageText", expectedMessageText, message.EM_MessageText);

				AssertNull("message doesn't have interchange", message.Interchange);
			});
		}
	}
}
