using CargoWise.Customs.FR.MessageDefinitions.TP5.CC004C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC004CProcessorTest : TP5BaseProcessorTest<Cc004CType, CC004CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.DeclarationAcceptance;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC004CResponseMessage.xml");

		protected override ZString ExpectedMRN => "MRN1";

		protected override ZString ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

		protected override ZString ExpectedPhaseId => "015";
	}
}
