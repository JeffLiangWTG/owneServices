using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Send.Testing
{
	class DCVAASendImpMessageWrapperTest : DCSendImpMessageWrapperTest
	{
		protected override IDeclarationImportExport GetMessageWrapper(MessageSending.DeltaGJobDeclarationMessageSendingObject objectToSend, ErrorCollector itemErrorCollector) => new DCVAASendImpMessageWrapper(objectToSend, itemErrorCollector);
		protected override ZInt ExpectedLiquidationCount => 5;
	}
}
