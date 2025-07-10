using CargoWise.Customs.FR.MessageDefinitions.TP5.CC028C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC028CProcessorTest : TP5BaseProcessorTest<Cc028CType, CC028CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.MrnAllocated;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC028CResponseMessage.xml");

		protected override ZString ExpectedMRN => "MRN28";

		protected override ZString ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

		protected override ZString ExpectedDepartureCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;

		protected override ZString ExpectedDepartureCustomsStatusWhenBM_CustomsStatusIsREL => NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

		protected override ZString ExpectedPhaseId => "015";

		protected override ZDateTime ExpectedEntryDate => new ZDateTime(2024,05,01);
	}
}
