using CargoWise.Customs.FR.MessageDefinitions.TP5.CC022C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC022CProcessorTest : TP5BaseProcessorTest<Cc022CType, CC022CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.NotificationToAmendDeclaration;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC022CResponseMessage.xml");

		protected override ZString ExpectedMRN => "MRN1";

		protected override ZString ExpectedDepartureCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;

		protected override ZString ExpectedMessageStatus => Common.EU.LogicalStatusList.Codes.Accepted;

		protected override ZString ExpectedPhaseId => NctsMovementHeaderTransactionStatusList.Codes.Declaration;
	}
}
