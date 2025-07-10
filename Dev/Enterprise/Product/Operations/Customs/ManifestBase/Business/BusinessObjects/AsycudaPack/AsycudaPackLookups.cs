//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAsycudaPackLookups
//
//    This class should be used for overriding collections in AutoAsycudaPackLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.ManifestBase.Extensions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaPackLookups : AutoAsycudaPackLookups
	{
		public AsycudaPackLookups(AutoAsycudaPack parent)
			: base(parent)
		{
		}

		public virtual CodeDescriptionPairList PackUQList => Factory.GetPackageTypeList();

		public virtual CodeDescriptionPairList WeightUQList => Factory.GetWeightUQList();

		public virtual CodeDescriptionPairList VolumeUQList => Factory.GetVolumeUQList();

		public virtual IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => Parent.Bill?.Header?.Containers;

		protected new AsycudaPack Parent => (AsycudaPack)base.Parent;
	}
}
