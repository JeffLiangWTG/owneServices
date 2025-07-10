using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCusEntryLineFeeCollection : DocBaseCusEntryLineFeeCollection
	{
		public DocCusEntryLineFeeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusEntryLineFeeCollection(CusEntryLineFeeCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCusEntryLineFee this[int index]
		{
			get { return (DocCusEntryLineFee)Elements[index]; }
		}
	}
}
