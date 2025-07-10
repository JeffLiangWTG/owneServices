using CargoWise.Customs.FR.MessageDefinitions.TP5.CC019C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC019CProcessorTest : TP5BaseProcessorTest<Cc019CType, CC019CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.DiscrepanciesAtDestination;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC019CResponseMessage.xml");

		protected override ZString ExpectedMRN => "MRN1";

		protected override ZString ExpectedDepartureCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination;

		protected override ZString ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

		protected override ZString ExpectedPhaseId => "015";
	}
}
