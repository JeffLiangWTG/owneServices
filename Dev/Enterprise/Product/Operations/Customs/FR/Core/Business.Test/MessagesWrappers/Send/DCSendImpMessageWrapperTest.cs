using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Send.Testing
{
	class DCSendImpMessageWrapperTest : DCSendMessageWrapperTest
	{
		protected override ZString MessageType => EU.Business.MessageTypeList.Codes.Import;
		protected override ZString MessageSubType => MessageSubTypeList.Codes.IMC;
		protected override ZString Application => "DELTAC";
		protected override ZString SchemaID => "MessageCDecImp";
		protected override ZString SchemaVersion => "01032013";
		protected override ZString DeltaMode => OrgCusAccountDeltaGTypeList.Codes.G1;
		protected override ZInt ExpectedLiquidationCount => 3;
		protected override IDeclarationImportExport GetMessageWrapper(MessageSending.DeltaGJobDeclarationMessageSendingObject objectToSend, ErrorCollector itemErrorCollector) => new DCSendImpMessageWrapper(objectToSend, itemErrorCollector);
	}
}
