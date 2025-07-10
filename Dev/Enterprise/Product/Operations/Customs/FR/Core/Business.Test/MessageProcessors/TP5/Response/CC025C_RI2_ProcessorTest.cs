using CargoWise.Customs.FR.MessageDefinitions.TP5.CC025C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC025C_RI2_ProcessorTest : TP5BaseProcessorTest<Cc025CType, CC025CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.GoodsReleaseNotification;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC025C_RI2_ResponseMessage.xml");

		protected override ZString ExpectedMRN => "MRN1";

		protected override ZString ExpectedPhaseId => NctsMovementHeaderTransactionStatusList.Codes.Arrival;

		protected override ZDateTime ExpectedReleaseDate => new(2024, 08, 01);

		protected override ZString ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

		protected override ZString ExpectedArrivalCustomsStatus => NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease;
	}
}
