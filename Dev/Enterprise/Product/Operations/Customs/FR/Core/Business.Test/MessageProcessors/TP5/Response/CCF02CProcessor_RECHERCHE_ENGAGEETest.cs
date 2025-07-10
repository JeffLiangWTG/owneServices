using CargoWise.Customs.FR.MessageDefinitions.TP5.CCF02C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CCF02CProcessor_RECHERCHE_ENGAGEETest : TP5BaseProcessorTest<Ccf02CType, CCF02CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.StatusUpdateNotification;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CCF02CResponseMessage_RECHERCHE_ENGAGEE.xml");

		protected override ZString ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

		protected override ZString ExpectedDepartureCustomsStatus => ZString.Empty;

		protected override ZString ExpectedPhaseId => ZString.Empty;
	}
}
