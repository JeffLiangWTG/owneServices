using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent) => false;

		public override bool AllowModificationMessage(IMessageParent parent)
		{
			return parent != null && (parent.MessageStatus == MessageStatusCodeList.Codes.Accepted || parent.MessageStatus == MessageStatusCodeList.Codes.Error || parent.MessageStatus == MessageStatusCodeList.Codes.Sent);
		}

		public override bool AllowOriginalMessage(IMessageParent parent)
		{
			return parent != null && (parent.MessageStatus == MessageStatusCodeList.Codes.Cancel || parent.MessageStatus == MessageStatusCodeList.Codes.Error || parent.MessageStatus == CargoWise.Types.ZString.Empty);
		}

		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => parent != null && parent.MessageStatus == MessageStatusCodeList.Codes.Sent;
		public override bool MessageStatusCanBeReset(IMessageParent parent) => false;
		public override bool HasManifestBeenSubmittedToCustoms(IMessageParent parent) => false;
	}
}
