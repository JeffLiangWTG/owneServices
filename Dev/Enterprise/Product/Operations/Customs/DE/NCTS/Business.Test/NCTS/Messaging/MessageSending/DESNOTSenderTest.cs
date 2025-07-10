using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(DESNOTSender))]
	sealed class DESNOTSenderTest : NctsHeaderSenderTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader.ArrivalMrnFromUser = "22DE12345678890";
			messagingObject.MessageType = NctsMessageTypeList.Codes.DESNOT;
		}

		protected override ZString ExpectedMessageSubType => NctsMessageSubTypeList.Codes.DestinationMessage;

		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETSNF);

		protected override ZString ExpectedLogbookRegistrationNumber => "22DE12345678890";

		protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.Arrival;

		protected override ZString MovementType => NctsMovementType.Codes.Arrival;

		protected override NctsHeaderSender GetNctsDeclarationSender() => new DESNOTSender(messagingObject);

		public void Test_BM_ArrivalDate_ShouldNotBeChangedWhenSend()
		{
			messagingObject.NctsHeader.ArrivalMovementHeader.BM_ArrivalDate =
				new ZDateTime(2024, 1, 10, 13, 50, 59);
			GetNctsDeclarationSender().Send();

			AssertEquals("BM_ArrivalDate", new ZDateTime(2024, 1, 10, 13, 50, 59),
				messagingObject.NctsHeader.ArrivalMovementHeader.BM_ArrivalDate);
		}
	}
}
