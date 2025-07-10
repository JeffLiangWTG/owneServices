using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVGoodsItemSpecialMentionWrapper : DocumentWrapper
{
	public static EVVGoodsItemSpecialMentionWrapper New(IEvvGoodsItemSpecialMention specialMention)
		=> new EVVGoodsItemSpecialMentionWrapper(Argument.NotNull(specialMention, nameof(specialMention)));

	EVVGoodsItemSpecialMentionWrapper(IEvvGoodsItemSpecialMention specialMention)
	{
		this.specialMention = specialMention;
	}

	readonly IEvvGoodsItemSpecialMention specialMention;

	public ZString Text => specialMention.Text;
}
