using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class XTERRMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessage_MessageStatusIsERR_WhenConnectedToAsycudaManifestHeader()
		{
			var factory = Factory;
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "785";
			header.Bills.AddNew();

			var message = factory.New<ILXERResponseMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ILCustoms;
			message.EM_Status = "QUE";
			message.EM_ReceiveTransmit = "RCV";
			message.EM_LinkedObject = header;
			factory.Save();

			var processor = new XTERRMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			factory.Save();
			message.Reload();
			AssertEquals("The message status is PRS", "PRS", message.EM_Status);
			AssertEquals("The AMA_MessageStatus is Error", "ERR", header.AMA_MessageStatus);
		}
	}
}
