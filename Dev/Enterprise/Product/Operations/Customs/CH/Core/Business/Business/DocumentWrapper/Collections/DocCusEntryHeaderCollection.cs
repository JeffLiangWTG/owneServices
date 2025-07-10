using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.CH.Business;

public class DocCusEntryHeaderCollection : DocBaseCusEntryHeaderCollection
{
	public DocCusEntryHeaderCollection(BusinessObjectFactory factory)
		: base(factory)
	{
	}

	public DocCusEntryHeaderCollection(Customs.Business.ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> collectionSource, BusinessObjectFactory factoryToWrap)
		: base(collectionSource, factoryToWrap)
	{
	}

	public new DocBaseCusEntryHeader this[int index] => (DocBaseCusEntryHeader)Elements[index];
}
