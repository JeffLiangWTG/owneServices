using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	sealed class DocBaseCusContainerCollectionTestClass : DocBaseCusContainerCollection
	{
		public DocBaseCusContainerCollectionTestClass(ICusContainerCollection<BaseCusContainer> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocBaseCusContainerTestClass this[int index]
		{
			get
			{
				return (DocBaseCusContainerTestClass)Elements[index];
			}
		}
	}
}
