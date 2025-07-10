using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVGoodsItemProducedDocumentWrapperCollection : DocumentWrapperCollection<EVVGoodsItemProducedDocumentWrapper>
{
	public static EVVGoodsItemProducedDocumentWrapperCollection New(IEnumerable<IEvvGoodsItemProducedDocument> producedDocuments, BusinessObjectFactory factory)
		=> new EVVGoodsItemProducedDocumentWrapperCollection(producedDocuments.EmptyIfNull(), factory);

	EVVGoodsItemProducedDocumentWrapperCollection(IEnumerable<IEvvGoodsItemProducedDocument> producedDocuments, BusinessObjectFactory factory)
		: base(producedDocuments, factory)
	{
	}

	protected override DocumentWrapper WrapObject(object objectToWrap)
	{
		return EVVGoodsItemProducedDocumentWrapper.New((IEvvGoodsItemProducedDocument)objectToWrap, Factory);
	}
}
