using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public class DocDocumentCollection : DocumentWrapperCollection
{
	public DocDocumentCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	public DocDocumentCollection(IEnumerable<CusSupportingInfo> collectionSource, BusinessObjectFactory factoryToWrap)
		: base(collectionSource, factoryToWrap)
	{
	}

	public new DocDocumentDataWrapper this[int index] => (DocDocumentDataWrapper)base[index];
}
