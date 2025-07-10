using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent) => false;
		public override bool AllowModificationMessage(IMessageParent parent) => false;
		public override bool AllowOriginalMessage(IMessageParent parent) => false;
		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent)
		{
			var header = (parent as AsycudaBill)?.Header ?? (parent as AsycudaManifestHeader);
			return !header?.RegistrationStatus.IsEmpty ?? false;
		}

		protected override bool IsLastMessageStatusSentToCustoms(IMessageParent parent)
		{
			var header = (parent as AsycudaBill)?.Header ?? (parent as AsycudaManifestHeader);
			return !header?.AMA_MessageStatus.In(new ZString[] { "", "NOT", "ERR" }) ?? false;
		}

		public override bool HasManifestBeenSubmittedToCustoms(IMessageParent parent)
		{
			var result = false;

			if (parent is AsycudaManifestHeader manifestHeader)
			{
				result = manifestHeader?.Messages.Any() ?? false;
			}
			else if (parent is AsycudaBill bill)
			{
				result = HasManifestBeenAcceptedByCustoms(parent) || IsLastMessageStatusSentToCustoms(parent);
			}
			return result;
		}

		public override CodeDescriptionPairList GetRegistrationStatusListCore(BusinessObjectFactory factory, ZString countryCode) => factory.GetCachedValue<EUICS2CustomsStatusList>();
	}
}
