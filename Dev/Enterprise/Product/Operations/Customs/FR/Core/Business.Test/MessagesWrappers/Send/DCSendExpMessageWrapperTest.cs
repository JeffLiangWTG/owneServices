using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Send.Testing
{
	public class DCSendExpMessageWrapperTest : DCSendMessageWrapperTest
	{
		protected override IDeclarationImportExport GetMessageWrapper(MessageSending.DeltaGJobDeclarationMessageSendingObject objectToSend, ErrorCollector itemErrorCollector) => new DCSendExpMessageWrapper(objectToSend, itemErrorCollector);
		protected override ZString SchemaID => "MessageCDecExp";
		protected override ZString SchemaVersion => "01032013";
		protected override ZString Application => "DELTAC";
		protected override ZString MessageType => EU.Business.MessageTypeList.Codes.Export;
		protected override ZString MessageSubType => MessageSubTypeList.Codes.EXC;
		protected override ZString DeltaMode => OrgCusAccountDeltaGTypeList.Codes.G1;
		protected override ZInt ExpectedLiquidationCount => 3;
	}
}
