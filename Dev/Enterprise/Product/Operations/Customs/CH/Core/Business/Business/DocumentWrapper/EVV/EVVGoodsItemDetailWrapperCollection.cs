using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVGoodsItemDetailWrapperCollection : DocumentWrapperCollection<EVVGoodsItemDetailWrapper>
{
	public static EVVGoodsItemDetailWrapperCollection New(IEnumerable<IEvvGoodsItemDetail> details, BusinessObjectFactory factory)
		=> new EVVGoodsItemDetailWrapperCollection(details.EmptyIfNull(), factory);

	EVVGoodsItemDetailWrapperCollection(IEnumerable<IEvvGoodsItemDetail> details, BusinessObjectFactory factory)
		: base(details, factory)
	{
	}

	protected override DocumentWrapper WrapObject(object objectToWrap)
	{
		return EVVGoodsItemDetailWrapper.New((IEvvGoodsItemDetail)objectToWrap, Factory);
	}
}
