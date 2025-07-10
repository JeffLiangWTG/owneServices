using CargoWise.Customs.FR.MessageDefinitions.TP5.CD906C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CD906CProcessorTest : TP5BaseProcessorTest<Cd906CType, CD906CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.FunctionalRejection;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CD906CResponseMessage.xml");

		protected override ZString ExpectedMRN => "MRN1";

		protected override ZString ExpectedMessageStatus => Common.EU.LogicalStatusList.Codes.Error;
	}
}
