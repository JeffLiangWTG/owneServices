using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class AROutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestCreateNewInterchangeProvider()
		{
			var message1 = CreateMessage("MESSAGE1", MessageTypes.Codes.ARA);
			var message2 = CreateMessage("MESSAGE2", MessageTypes.Codes.ARD);

			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new AROutgoingMessageProcessor(logger);
			processor.ProcessMessage(CancellationToken.None);

			CombineAssertions(() =>
			{
				var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("NumberOfInterchanges", 2, interchangesCreated.Length);

				message1.Reload();
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message1.EM_Status);
				var interchange1 = interchangesCreated.Single(i => i.PK == message1.EM_EI);
				AssertEquals("EI_InterchangeNum", message1.EM_MessageNum, interchange1.EI_InterchangeNum);
				AssertEquals("EI_InterchangeNum", "00000001", interchange1.EI_InterchangeNum);

				message2.Reload();
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message2.EM_Status);
				var interchange2 = interchangesCreated.Single(i => i.PK == message2.EM_EI);
				AssertEquals("EI_InterchangeNum", message2.EM_MessageNum, interchange2.EI_InterchangeNum);
				AssertEquals("EI_InterchangeNum", "00000002", interchange2.EI_InterchangeNum);
			});
		}

		ARMessage CreateMessage(ZString messageText, ZString messageType)
		{
			var message = Factory.New<ARMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ARCustoms;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = messageText;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}
	}
}
