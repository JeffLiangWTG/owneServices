using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.NZ
{
	public class DocCusContainerCollection : DocBaseCusContainerCollection
	{
		public DocCusContainerCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusContainerCollection(CusContainerCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCusContainer this[int index]
		{
			get { return (DocCusContainer)base[index]; }
		}
	}
}
