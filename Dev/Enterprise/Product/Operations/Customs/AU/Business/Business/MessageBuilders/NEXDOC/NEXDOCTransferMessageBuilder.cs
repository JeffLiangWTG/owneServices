using Enterprise.Customs.AU.Declaration.Business.RexOwnershipSoap;
using Enterprise.Messaging.Business.XmlMessaging;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCTransferMessageBuilder : NEXDOCMessageBuilder
	{
		public NEXDOCTransferMessageBuilder(QuarantineExDocHeader header) : base(header)
		{
			this.header = header;
		}

		readonly QuarantineExDocHeader header;

		public XmlEDIMessage CreateNewMessage()
		{
			var message = CreateNewMessage(new RexTransferOwnership
			{
				identification = CreateNewIdentification(),
				clientGroup = header.QH_TransfereeEDIUserIdentifier,
				exporter = header.QH_TransfereeExporterNumber
			}.Serialize());
			message.EM_MessageSubType = NEXDOCMessageType.Codes.REXTransfer;
			return message;
		}
	}
}
