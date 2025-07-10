using Enterprise.Customs.AU.Declaration.Business.RexOwnershipSoap;
using Enterprise.Messaging.Business.XmlMessaging;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCWithdrawMessageBuilder : NEXDOCMessageBuilder
	{
		public NEXDOCWithdrawMessageBuilder(QuarantineExDocHeader header) : base(header)
		{
		}

		public XmlEDIMessage CreateNewMessage()
		{
			var message = CreateNewMessage(new RexWithdrawOwnership
			{
				identification = CreateNewIdentification()
			}.Serialize());
			message.EM_MessageSubType = NEXDOCMessageType.Codes.WithdrawalOwnership;
			return message;
		}
	}
}
