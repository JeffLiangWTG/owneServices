using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.CH.Business;

public class DocCusEntryLineCollection : DocBaseCusEntryLineCollection
{
	public DocCusEntryLineCollection(BusinessObjectFactory factory)
		: base(factory)
	{
	}

	public DocCusEntryLineCollection(Customs.Business.ICusEntryLineCollection<CusEntryLine> collectionSource, BusinessObjectFactory factoryToWrap)
		: base(collectionSource, factoryToWrap)
	{
	}

	public new DocCusEntryLine this[int index] => (DocCusEntryLine)base[index];
}
