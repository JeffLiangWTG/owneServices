using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCusEntryLineCollection : DocBaseCusEntryLineCollection
	{
		public DocCusEntryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocCusEntryLineCollection(CusEntryLineCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocCusEntryLine this[int index]
		{
			get { return (DocCusEntryLine)Elements[index]; }
		}
	}
}
