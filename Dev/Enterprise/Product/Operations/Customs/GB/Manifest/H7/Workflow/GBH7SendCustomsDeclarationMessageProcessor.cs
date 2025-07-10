using System.Linq;
using CargoWise.Application;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.GB.H7.Business
{
	public class GBH7SendCustomsDeclarationMessageProcessor : EUH7SendCustomsDeclarationMessageProcessor
	{
		public GBH7SendCustomsDeclarationMessageProcessor(EU.H7.Business.AsycudaManifestHeader header) : base(header)
		{
		}

		public override int SendCustomsDeclaration(BaseMessageSendingObjectParent messageSendingParent)
		{
			var selectedSendingObjects = messageSendingParent.SendingObjectsCollection;
			var messageSender = ObjectFactory.Get<Integration.Customs.GB.GBH7.IH7MessageSender>("GBH7.IH7MessageSender", selectedSendingObjects.Cast<MessageSendingObject>());

			return messageSender.Send();
		}
	}
}
