using Enterprise.Customs.AU.Declaration.Business.RexOwnershipSoap;
using Enterprise.Messaging.Business.XmlMessaging;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCAcknowledgeMessageBuilder : NEXDOCMessageBuilder
	{
		public NEXDOCAcknowledgeMessageBuilder(QuarantineNexDocNotification notification) : base(notification)
		{
		}

		public XmlEDIMessage CreateNewMessage(bool isAccepted)
		{
			return CreateNewMessage(new RexAcknowledgeOwnership()
			{
				identification = CreateNewIdentification(),
				isAccepted = isAccepted
			}.Serialize());
		}
	}
}
