using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	sealed class TP5OutgoingInterchangeProviderTest : TestCaseWithFactory
	{
		public void TestInterchangeType()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var messageObject = new TP5MessageSendingObject(nctsHeader);
			messageObject.MessageType = "015";
			var sender = new TP5MessageSender(messageObject, new EU.Business.ErrorCollector());
			sender.Send();

			var processor = new TP5OutgoingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessMessage(CancellationToken.None);

			CombineAssertions(() =>
			{
				var interchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GenericMessageDelivery));
				AssertEquals("Interchange created.", 1, interchanges.Length);
				AssertEquals("Interchange Type: ", "FR5", interchanges[0].EI_InterchangeType);
			});
		}
	}
}
