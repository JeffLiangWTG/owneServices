using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(DESREMSender))]
	sealed class DESREMSenderTest : NctsHeaderSenderTest
	{
		protected override ZString ExpectedMessageSubType => NctsMessageSubTypeList.Codes.DestinationMessage;

		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETSRD);

		protected override ZString ExpectedLogbookRegistrationNumber => "22DE12345678890";

		protected override ZString ExpectedPhaseStatus => NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;

		protected override ZString ExpectedCustomsStatus => NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;

		protected override ZString MovementType => NctsMovementType.Codes.Arrival;

		protected override NctsHeaderSender GetNctsDeclarationSender() => new DESREMSender(messagingObject);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader.ArrivalMrnFromUser = "22DE12345678890";
			messagingObject.MessageType = NctsMessageTypeList.Codes.DESREM;
		}
	}
}
