using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNAttachmentLookups : AutoBMNCNAttachmentLookups
	{
		public BMNCNAttachmentLookups(AutoBMNCNAttachment parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList Types
		{
			get { return Factory.GetCachedValue<AttachmentTypeList>(); }
		}
	}
}
