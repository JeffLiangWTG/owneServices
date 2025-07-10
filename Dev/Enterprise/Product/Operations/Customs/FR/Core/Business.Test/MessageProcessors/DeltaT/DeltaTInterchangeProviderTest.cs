using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class DeltaTInterchangeProviderTest : TestCaseWithFactory
	{
		public void TestPopulateInterchangeCorrectly()
		{
			var msg = MessageProcessorTestHelper.CreateEDIMessageForTesting(Factory,
				ApplicationCodeList.Codes.FRCustomsMessage,
				ReceiveTransmitList.Codes.Transmit,
				EDIMessageStatusList.Codes.Queued,
				GlbBranch.CurrentBranch.PK,
				"BLA,Bla MSG0001",
				MessageTypeList.Codes.DTF15,
				MessageSubTypeList.Codes.DT
				);
			Factory.Save();

			var processor = new DeltaTOutgoingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			msg.Reload();

			var interchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GenericMessageDelivery));
			AssertEquals(1, interchanges.Length);

			var interchange = interchanges.Cast<EDIInterchange>().First(x => x.PK == msg.EM_EI);
			AssertEquals("EI_InterchangeType should be FRC", "FRC", interchange.EI_InterchangeType);
		}
	}
}
