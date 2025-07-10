using CargoWise.Customs.FR.MessageDefinitions.TP5.CC917C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC917CProcessorTest : TP5BaseProcessorTest<Cc917CType, CC917CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.XmlNack;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC917CResponseMessage.xml");

		protected override ZString ExpectedMessageStatus => LogicalStatusList.Codes.Error;
	}
}
