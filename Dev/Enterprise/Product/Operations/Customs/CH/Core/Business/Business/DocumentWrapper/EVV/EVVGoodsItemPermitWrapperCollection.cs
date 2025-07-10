using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVGoodsItemPermitWrapperCollection : DocumentWrapperCollection<EVVGoodsItemPermitWrapper>
{
	public static EVVGoodsItemPermitWrapperCollection New(IEnumerable<IEvvGoodsItemPermit> permits, BusinessObjectFactory factory)
		=> new EVVGoodsItemPermitWrapperCollection(permits.EmptyIfNull(), factory);

	EVVGoodsItemPermitWrapperCollection(IEnumerable<IEvvGoodsItemPermit> permits, BusinessObjectFactory factory)
		: base(permits, factory)
	{
	}

	protected override DocumentWrapper WrapObject(object objectToWrap)
	{
		return EVVGoodsItemPermitWrapper.New((IEvvGoodsItemPermit)objectToWrap, Factory);
	}
}
