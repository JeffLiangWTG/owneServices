using System.Linq;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.GB.ICS
{
	public class ICSMessageStatusProvider : MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent) => false;

		public override bool AllowModificationMessage(IMessageParent parent) => false;

		public override bool AllowOriginalMessage(IMessageParent parent) => false;

		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => false;

		public override bool HasManifestBeenSubmittedToCustoms(IMessageParent parent)
		{
			if (parent is AsycudaManifestHeader manifestHeader)
			{
				return manifestHeader.Messages.Any(x => x.IsInDatabase);
			}
			return base.HasManifestBeenSubmittedToCustoms(parent);
		}
	}
}
