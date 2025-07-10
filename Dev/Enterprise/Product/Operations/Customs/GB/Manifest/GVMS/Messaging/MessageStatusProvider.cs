using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.GVMS
{
	public class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent) => false;
		public override bool AllowModificationMessage(IMessageParent parent) => false;
		public override bool AllowOriginalMessage(IMessageParent parent) => false;
		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => false;
		public override bool MessageStatusCanBeReset(IMessageParent parent) => false;

		public override bool HasManifestBeenSubmittedToCustoms(IMessageParent parent)
		{
			if (parent is AsycudaManifestHeader manifestHeader)
			{
				return manifestHeader.Messages.Any(x => x.IsInDatabase);
			}
			return base.HasManifestBeenSubmittedToCustoms(parent);
		}

		public override CodeDescriptionPairList GetRegistrationStatusListCore(BusinessObjectFactory factory, ZString countryCode) => new GVMSCustomsStatus();
	}
}
