using CargoWise.Customs.FR.MessageDefinitions.TP5.CC057C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC057CProcessorTest : TP5BaseProcessorTest<Cc057CType, CC057CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.RejectionFromOfficeOfDestination;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC057CResponseMessage.xml");

		protected override ZString ExpectedMRN => "MRN1";

		protected override ZString ExpectedMessageStatus => Common.EU.LogicalStatusList.Codes.Invalid;
	}
}
