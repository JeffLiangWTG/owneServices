using CargoWise.Customs.FR.MessageDefinitions.TP5.CC928C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC928CProcessorTest : TP5BaseProcessorTest<Cc928CType, CC928CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.PositiveAcknowledge928;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC928CResponseMessage.xml");

		protected override ZString ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

		protected override ZString ExpectedPhaseId => "015";

		protected override ZString ExpectedDepartureCustomsStatus => NctsTransitStatusList.Codes.Acknowledged;
	}
}
