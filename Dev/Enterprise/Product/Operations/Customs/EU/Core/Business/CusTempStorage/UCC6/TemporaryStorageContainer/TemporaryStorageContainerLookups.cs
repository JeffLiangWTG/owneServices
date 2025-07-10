using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageContainerLookups : AsycudaContainerLookups
	{
		public TemporaryStorageContainerLookups(AutoAsycudaContainer parent) : base(parent)
		{
		}
		protected new AsycudaContainer Parent => (TemporaryStorageContainer)base.Parent;

		public CodeDescriptionPairList EmptyFullIndicatorList => Factory.GetCachedValue<EmptyFullIndicatorList>();
	}
}
