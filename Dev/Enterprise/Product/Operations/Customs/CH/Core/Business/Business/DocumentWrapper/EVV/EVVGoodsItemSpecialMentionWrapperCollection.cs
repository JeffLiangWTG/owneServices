using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVGoodsItemSpecialMentionWrapperCollection : DocumentWrapperCollection<EVVGoodsItemSpecialMentionWrapper>
{
	public static EVVGoodsItemSpecialMentionWrapperCollection New(IEnumerable<IEvvGoodsItemSpecialMention> specialMentions, BusinessObjectFactory factory)
		=> new EVVGoodsItemSpecialMentionWrapperCollection(specialMentions.EmptyIfNull(), factory);

	EVVGoodsItemSpecialMentionWrapperCollection(IEnumerable<IEvvGoodsItemSpecialMention> specialMentions, BusinessObjectFactory factory)
		: base(specialMentions, factory)
	{
	}

	protected override DocumentWrapper WrapObject(object objectToWrap)
	{
		return EVVGoodsItemSpecialMentionWrapper.New((IEvvGoodsItemSpecialMention)objectToWrap);
	}
}
