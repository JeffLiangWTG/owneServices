using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.CH.Business;

public class DocEdecCusEntryLineCollection : DocBaseCusEntryLineCollection
{
	public DocEdecCusEntryLineCollection(BusinessObjectFactory factory)
		: base(factory)
	{
	}

	public DocEdecCusEntryLineCollection(Customs.Business.ICusEntryLineCollection<CusEntryLine> collectionSource, BusinessObjectFactory factoryToWrap)
		: base(collectionSource, factoryToWrap)
	{
	}

	public new DocEdecCusEntryLine this[int index] => (DocEdecCusEntryLine)base[index];
}
