using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Core;
using IMessageParent = Enterprise.Customs.ASYCUDA.Business.IMessageParent;

namespace Enterprise.Customs.ASYCUDAManifest.Business
{
	public class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent) => false;

		public override bool AllowModificationMessage(IMessageParent parent) => false;

		public override bool AllowOriginalMessage(IMessageParent parent) => true;

		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => false;

		public override bool HasManifestBeenSubmittedToCustoms(IMessageParent parent)
		{
			var result = base.HasManifestBeenSubmittedToCustoms(parent);
			if (!result)
			{
				var header = (parent as AsycudaManifestHeader)
							?? (parent as AsycudaBill)?.Header;

				result = header?.Messages.Any() ?? false;
			}
			return result;
		}

		public override CodeDescriptionPairList GetRegistrationStatusListCore(BusinessObjectFactory factory, ZString countryCode)
		{
			var list = new AsycudaRegistrationStatuses();
			list.AddRange(base.GetRegistrationStatusListCore(factory, countryCode));
			return list;
		}
	}
}
