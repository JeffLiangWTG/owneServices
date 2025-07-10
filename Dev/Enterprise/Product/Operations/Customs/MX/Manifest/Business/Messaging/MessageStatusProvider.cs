using System.Linq;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent)
		{
			var result = false;
			if (parent is AsycudaManifestHeader header)
			{
				result = header.Bills.Cast<AsycudaBill>().Any(x => x.CanSendCancellationMessage());
			}
			return result;
		}

		public override bool AllowModificationMessage(IMessageParent parent)
		{
			var result = false;
			if (parent is AsycudaManifestHeader header)
			{
				result = header.Bills.Cast<AsycudaBill>().Any(x => x.CanSendModificationMessage());
			}
			return result;
		}

		public override bool AllowOriginalMessage(IMessageParent parent)
		{
			var result = false;
			if (parent is AsycudaManifestHeader header)
			{
				result = header.Bills.Cast<AsycudaBill>().Any(x => x.CanSendOriginalMessage());
			}
			return result;
		}

		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => false;
		public override bool MessageStatusCanBeReset(IMessageParent parent) => false;
	}
}
