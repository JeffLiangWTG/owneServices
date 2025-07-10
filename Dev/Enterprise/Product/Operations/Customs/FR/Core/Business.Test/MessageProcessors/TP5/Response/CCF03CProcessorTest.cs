using CargoWise.Customs.FR.MessageDefinitions.TP5.CCF03C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing;

sealed class CCF03CProcessorTest : TP5BaseProcessorTest<Ccf03CType, CCF03CProcessor>
{
	protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.PositiveAcknowledgeF03;

	protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CCF03CResponseMessage.xml");

	protected override ZString ExpectedMRN => "MRN1";

	protected override ZString ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

	protected override ZString ExpectedDepartureCustomsStatus => NctsTransitStatusList.Codes.Acknowledged;

	protected override ZString ExpectedPhaseId => NctsMovementHeaderTransactionStatusList.Codes.Arrival;

	protected override ZDateTime ExpectedEntryDate => new ZDateTime(2024, 9, 18, 0, 0 ,0);
}
