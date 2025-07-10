using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class MessageDataProviderFactoryTest : TestCaseWithFactory
{
	public void TestGetMessageDataProvider()
	{
		CombineAssertions(() =>
		{
			var controller = MessageDataProviderFactory.Instance.Value;
			AssertType<CONTRLDataProvider>("CONTRL", controller.GetMessageDataProvider(CreateEDIMessage(AEConstants.Messaging.MessageTypes.CONTRL)));
			AssertType<CUSRESDataProvider>("CUSRES", controller.GetMessageDataProvider(CreateEDIMessage(AEConstants.Messaging.MessageTypes.CUSRES)));
			AssertType<XTTERRDataProvider>("XTTERR", controller.GetMessageDataProvider(CreateEDIMessage(AEConstants.Messaging.MessageTypes.XTTERR)));
			AssertType<DOCSUCDataProvider>("DOCSUC", controller.GetMessageDataProvider(CreateEDIMessage(AEConstants.Messaging.MessageTypes.DOCSUC)));
			AssertType<DOCERRDataProvider>("DOCERR", controller.GetMessageDataProvider(CreateEDIMessage(AEConstants.Messaging.MessageTypes.DOCERR)));
			NUnit.Framework.Assert.That(controller.GetMessageDataProvider(CreateEDIMessage("XXX")), Is.Null, "Unknown message type");
		});
	}

	AEEDIMessage CreateEDIMessage(string messageType)
	{
		var result = Factory.New<AEEDIMessage>();
		result.EM_MessageType = messageType;
		result.EM_MessageText = "MESSAGE";
		return result;
	}
}
