using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITMessageSenderBaseTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("factory is required", () => new ITMessageSender(null, messageGeneratorMock.Object, sendableEntryMock.Object));
		AssertExceptionThrown<ArgumentNullException>("messageGenerator is required", () => new ITMessageSender(Factory, null, sendableEntryMock.Object));
		AssertExceptionThrown<ArgumentNullException>("sendableEntry is required", () => new ITMessageSender(Factory, messageGeneratorMock.Object, null));
		AssertNoExceptionThrown(() => new ITMessageSender(Factory, messageGeneratorMock.Object, sendableEntryMock.Object));
	}

	public void TestSendCallsPreProcessBeforeSending()
	{
		var itEdiMessage = Factory.New<ITEDIMessage>();
		itEdiMessage.MessageNumberStrategy = new Mock<IMessageNumberStrategy>().Object;
		fountainProviderMock.Setup(obj => obj.HasClonableNumberRanges).Returns(false);
		messageGeneratorMock.Setup(obj => obj.GenerateMessage()).Returns(itEdiMessage);
		sendableEntryMock.Setup(x => x.PreProcessBeforeSending());
		sendableEntryMock.Setup(x => x.CustomsProfile).Returns("1234-DEC1");
		var messageSender = new ITMessageSender(Factory, messageGeneratorMock.Object, sendableEntryMock.Object);
		var messageResult = messageSender.Send();
		sendableEntryMock.Verify(x => x.PreProcessBeforeSending(), Times.Once);
		AssertNotNull("Message result", messageResult);
	}

	protected override void SetUp()
	{
		base.SetUp();
		fountainProviderMock = new Mock<ICustomsMessageFountainProvider>();
		messageGeneratorMock = new Mock<IOutgoingCustomsMessageCreationStrategy>();
		sendableEntryMock = new Mock<ISendableCustomsEntry>();

		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1", "AUTHUSER-001")
			.Build();
		Factory.Save();
	}

	Mock<ICustomsMessageFountainProvider> fountainProviderMock;
	Mock<IOutgoingCustomsMessageCreationStrategy> messageGeneratorMock;
	Mock<ISendableCustomsEntry> sendableEntryMock;
}
