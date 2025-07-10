using System.Collections.Specialized;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;
using Moq;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class BasicMessageManagerTest : TestCaseWithFactory
	{
		public void TestSendMessage()
		{
			const string expectedErrorMessage = "No Valid digital sign certificate found for signing this message – please review your staff or company configuration";
			var header = Factory.NewWithValidTestData<CusEntryHeader>();
			var builder = new ILDEC275MessageBuilder(header);
			var messageManager = new BasicMessageManager(header, builder, "275");

			var senderMock = new Mock<ISendsMessagesToCustoms>();
			var errors = new StringCollection();
			messageManager.SendMessage(senderMock.Object);

			AssertNoExceptionThrown("MessageSendErrorAlert was not called with the expected error message.",
				() =>
				{
					senderMock.Verify(
					sender => sender.MessageSendErrorAlert(It.Is<StringCollection>(e => e.Contains(expectedErrorMessage))),
					Times.Once);
				});
		}
	}
}
