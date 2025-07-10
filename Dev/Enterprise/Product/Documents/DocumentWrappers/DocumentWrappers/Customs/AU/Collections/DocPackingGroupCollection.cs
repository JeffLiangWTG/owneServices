using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocPackingGroupCollection : DocumentWrapperCollection
	{
		public DocPackingGroupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocPackingGroupCollection(PackingGroupCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocPackingGroup this[int index]
		{
			get
			{
				return (DocPackingGroup)Elements[index];
			}
		}
	}
}
