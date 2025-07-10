using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class ESH7SendCustomsDeclarationMessageProcessor : EUH7SendCustomsDeclarationMessageProcessor
	{
		public ESH7SendCustomsDeclarationMessageProcessor(EU.H7.Business.AsycudaManifestHeader header) : base(header)
		{
		}

		public override int SendCustomsDeclaration(BaseMessageSendingObjectParent messageSendingParent)
		{
			var selectedSendingObjects = messageSendingParent.SelectedSendingObjects;

			var messageSender = new H7MessageSender(selectedSendingObjects.Cast<H7MessageSendingObject>());
			var messageBuildersToSend = messageSender.GetMessageBuildersData();
			var messagesInfo = messageSender.Send(messageBuildersToSend);

			return messagesInfo.MessagesSent;
		}
	}
}
