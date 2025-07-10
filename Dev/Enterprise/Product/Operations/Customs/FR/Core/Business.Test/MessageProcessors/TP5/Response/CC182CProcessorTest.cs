using CargoWise.Customs.FR.MessageDefinitions.TP5.CC182C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class CC182CProcessorTest : TP5BaseProcessorTest<Cc182CType, CC182CProcessor>
	{
		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.ForwardedIncidentNotificationToEd;

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC182CResponseMessage.xml");

		protected override ZString ExpectedMRN => "MRN1";

		protected override ZString ExpectedDepartureCustomsStatus => NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered;

		protected override ZString ExpectedMessageStatus => Common.EU.LogicalStatusList.Codes.Accepted;

		protected override ZString ExpectedPhaseId => NctsMovementHeaderTransactionStatusList.Codes.Declaration;
	}
}
