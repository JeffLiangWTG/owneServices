using Enterprise.Customs.AU.Declaration.Business.RexOwnershipSoap;
using Enterprise.Messaging.Business.XmlMessaging;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCForwardMessageBuilder : NEXDOCMessageBuilder
	{
		public NEXDOCForwardMessageBuilder(QuarantineExDocHeader header) : base(header)
		{
			this.header = header;
		}

		readonly QuarantineExDocHeader header;

		public XmlEDIMessage CreateNewMessage()
		{
			var forwardOwnership = new RexForwardOwnership
			{
				identification = CreateNewIdentification(),
				clientGroup = header.QH_ForwardeeEDIUserIdentifier,
				requiresAcceptance = header.QH_ForwardRequiresAcceptance
			};

			var holdUntilStatus = GetStatusType(header.QH_ForwardStatus);

			if (holdUntilStatus != null && holdUntilStatus.HasValue)
			{
				forwardOwnership.holdUntilStatus = holdUntilStatus.Value;
				forwardOwnership.holdUntilStatusSpecified = true;
			}

			var message = CreateNewMessage(forwardOwnership.Serialize());
			message.EM_MessageSubType = NEXDOCMessageType.Codes.REXForward;
			return message;
		}

		ForwardCompletionStatusType? GetStatusType(string status)
		{
			switch (status)
			{
				case EXDOCComplianceStatusCodes.Codes.Order:
					return ForwardCompletionStatusType.ORDER;
				case EXDOCComplianceStatusCodes.Codes.Final:
					return ForwardCompletionStatusType.FINAL;
				case EXDOCComplianceStatusCodes.Codes.Initial:
					return ForwardCompletionStatusType.INIT;
				case EXDOCComplianceStatusCodes.Codes.Completed:
					return ForwardCompletionStatusType.COMP;
				case EXDOCComplianceStatusCodes.Codes.CertificateReady:
					return ForwardCompletionStatusType.CTRD;
				default:
					return null;
			}
		}
	}
}
