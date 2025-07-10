using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.AU;

namespace Enterprise.Client.Wow
{
	public class WowDocCusContainerCollection : DocCusContainerCollection
	{
		public WowDocCusContainerCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public WowDocCusContainerCollection(ICusContainerCollection<BaseCusContainer> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new WowDocCusContainer this[int index]
		{
			get { return (WowDocCusContainer)Elements[index]; }
		}
	}
}
