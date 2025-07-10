using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class MessageProcessorFactoryTest : TestCaseWithFactory
{
	public void TestGetMessageProcessor()
	{
		CombineAssertions(() =>
		{
			var controller = MessageProcessorFactory.Instance.Value;
			AssertType<CONTRLMessageProcessor>("CONTRL", controller.GetMessageProcessor(CreateEDIMessage(AEConstants.Messaging.MessageTypes.CONTRL)));
			AssertType<CUSRESMessageProcessor>("CUSRES", controller.GetMessageProcessor(CreateEDIMessage(AEConstants.Messaging.MessageTypes.CUSRES)));
			AssertType<XTTERRMessageProcessor>("XTTERR", controller.GetMessageProcessor(CreateEDIMessage(AEConstants.Messaging.MessageTypes.XTTERR)));
			AssertType<DOCSUCMessageProcessor>("DOCSUC", controller.GetMessageProcessor(CreateEDIMessage(AEConstants.Messaging.MessageTypes.DOCSUC)));
			AssertType<DOCERRMessageProcessor>("DOCERR", controller.GetMessageProcessor(CreateEDIMessage(AEConstants.Messaging.MessageTypes.DOCERR)));
			NUnit.Framework.Assert.That(controller.GetMessageProcessor(CreateEDIMessage("XXX")), Is.Null, "Unknown message type");
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
