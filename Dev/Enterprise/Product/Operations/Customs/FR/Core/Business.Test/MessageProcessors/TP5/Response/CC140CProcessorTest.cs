using CargoWise.Customs.FR.MessageDefinitions.TP5.CC140C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC140CProcessorTest : TP5BaseProcessorTest<Cc140CType, CC140CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.RequestOnNonArrivedMovement;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC140CResponseMessage.xml");

		protected override ZString ExpectedMRN => "MRN1";

		protected override ZString ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

		protected override ZString ExpectedPhaseId => "015";

		protected override ZString ExpectedDepartureCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry;
	}
}
