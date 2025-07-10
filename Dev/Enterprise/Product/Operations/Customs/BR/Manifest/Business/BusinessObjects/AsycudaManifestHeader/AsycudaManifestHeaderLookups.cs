using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList MessageStatusList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("MessageStatusCodeList",
					delegate
					{
						var result = new MessageStatusCodeList();
						result.RemoveCode(MessageStatusCodeList.Codes.Registered);
						result.RemoveCode(MessageStatusCodeList.Codes.NotSent);
						result.RemoveCode(MessageStatusCodeList.Codes.Updated);
						result.RemoveCode(MessageStatusCodeList.Codes.Unknown);
						result.RemoveCode(MessageStatusCodeList.Codes.Awaiting);
						result.RemoveCode(MessageStatusCodeList.Codes.Warning);
						return result;
					}
				);
			}
		}
	}
}
