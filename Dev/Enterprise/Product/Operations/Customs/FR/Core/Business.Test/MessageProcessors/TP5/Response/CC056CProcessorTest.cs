using CargoWise.Customs.FR.MessageDefinitions.TP5.CC056C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC056CProcessorTest : TP5BaseProcessorTest<Cc056CType, CC056CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.RejectionFromOfficeOfDeparture;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC056CResponseMessage.xml");

		protected override ZString ExpectedMRN => "MRN1";

		protected override ZString ExpectedMessageStatus => Common.EU.LogicalStatusList.Codes.Invalid;
	}
}
