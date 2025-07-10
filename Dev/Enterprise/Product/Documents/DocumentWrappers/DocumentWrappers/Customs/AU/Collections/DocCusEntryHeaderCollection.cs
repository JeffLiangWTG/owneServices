using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCusEntryHeaderCollection : DocBaseCusEntryHeaderCollection
	{
		public DocCusEntryHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusEntryHeaderCollection(Enterprise.Customs.Business.ICusEntryHeaderCollection<Enterprise.Customs.Business.CusEntryHeader> collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCusEntryHeader this[int index]
		{
			get
			{
				return (DocCusEntryHeader)Elements[index];
			}
		}
	}
}
