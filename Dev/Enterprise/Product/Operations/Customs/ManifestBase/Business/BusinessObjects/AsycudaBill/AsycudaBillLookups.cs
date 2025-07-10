//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAsycudaBillLookups
//
//    This class should be used for overriding collections in AutoAsycudaBillLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaBillLookups : AutoAsycudaBillLookups
	{
		public AsycudaBillLookups(AutoAsycudaBill parent)
			: base(parent)
		{
		}

		public virtual IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => Parent.Header?.Containers;

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;
	}
}

