using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaTransferHeaderLookups : AutoAsycudaTransferHeaderLookups
	{
		public AsycudaTransferHeaderLookups(AutoAsycudaTransferHeader parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList TransferTypeList => Factory.GetCachedValue<CodeDescriptionPairList>("AsycudaTransferHeaderLookups|TransferTypeList", () => new TransferTypeList());
	}
}
