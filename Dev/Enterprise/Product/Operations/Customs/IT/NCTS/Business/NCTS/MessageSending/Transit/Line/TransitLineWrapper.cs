using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TransitLineWrapper : NctsSADLineCommonWrapper
{
	public TransitLineWrapper(NctsDepartureCargoDesc goodsItem) : base(goodsItem)
	{
	}

	protected override ZString DeclarationTypeCore => GoodsItem.BY_Type;

	protected override ZString DispatchCountryCodeCore => GoodsItem.BY_RN_NKCountryOfDispatch;

	protected override IETLineSpecialMentionGroup SpecialMentionGroupCore => new TransitLineSpecialMentionGroupWrapper(GoodsItem);
}
