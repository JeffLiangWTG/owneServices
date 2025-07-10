using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TIRLineWrapper : NctsSADLineCommonWrapper
{
	public TIRLineWrapper(NctsDepartureCargoDesc goodsItem) : base(goodsItem)
	{
	}

	protected override ZString DeclarationTypeCore => ZString.Empty;

	protected override ZString DispatchCountryCodeCore => ZString.Empty;

	protected override IETLineSpecialMentionGroup SpecialMentionGroupCore => new NctsSADLineSpecialMentionGroupWrapper(GoodsItem);
}
