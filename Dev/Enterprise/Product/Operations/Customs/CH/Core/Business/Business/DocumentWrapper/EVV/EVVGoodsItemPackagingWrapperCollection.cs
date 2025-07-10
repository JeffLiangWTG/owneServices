using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVGoodsItemPackagingWrapperCollection : DocumentWrapperCollection<EVVGoodsItemPackagingWrapper>
{
	public static EVVGoodsItemPackagingWrapperCollection New(IEnumerable<IEvvGoodsItemPackaging> packagings, BusinessObjectFactory factory)
		=> new EVVGoodsItemPackagingWrapperCollection(packagings.EmptyIfNull(), factory);

	EVVGoodsItemPackagingWrapperCollection(IEnumerable<IEvvGoodsItemPackaging> packagings, BusinessObjectFactory factory)
		: base(packagings, factory)
	{
	}

	protected override DocumentWrapper WrapObject(object objectToWrap)
	{
		return EVVGoodsItemPackagingWrapper.New((IEvvGoodsItemPackaging)objectToWrap, Factory);
	}
}
