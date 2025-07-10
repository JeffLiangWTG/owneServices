using CargoWise.Customs.FR.MessageDefinitions.TP5.CCF02C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CCF02CProcessor_NON_LIB_POUR_TRANSTest : TP5BaseProcessorTest<Ccf02CType, CCF02CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.StatusUpdateNotification;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CCF02CResponseMessage_NON_LIB_POUR_TRANS.xml");

		protected override ZString ExpectedMessageStatus => LogicalStatusList.Codes.Accepted;

		protected override ZString ExpectedDepartureCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.RejectedAtOrigin;

		protected override ZString ExpectedPhaseId => ZString.Empty;
	}
}
