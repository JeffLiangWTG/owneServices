using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVGoodsItemWrapperCollection : DocumentWrapperCollectionWithLanguage<IEvvGoodsItem, EVVGoodsItemWrapper>
{
	public static EVVGoodsItemWrapperCollection New(IEnumerable<IEvvGoodsItem> goodsItem, BusinessObjectFactory factory, ZString documentLanguage)
		=> new EVVGoodsItemWrapperCollection(goodsItem.EmptyIfNull(), factory, documentLanguage);

	EVVGoodsItemWrapperCollection(IEnumerable<IEvvGoodsItem> goodsItems, BusinessObjectFactory factory, ZString documentLanguage)
		: base(goodsItems, factory, documentLanguage)
	{
	}

	protected override DocumentWrapper WrapObject(IEvvGoodsItem objectToWrap, ZString documentLanguage)
	{
		return EVVGoodsItemWrapper.New(objectToWrap, Factory, documentLanguage);
	}
}
