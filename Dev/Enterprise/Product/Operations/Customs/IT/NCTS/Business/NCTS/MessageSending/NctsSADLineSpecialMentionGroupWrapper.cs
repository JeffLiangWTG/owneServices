using CargoWise.Common;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSADLineSpecialMentionGroupWrapper : IETLineSpecialMentionGroup
{
	public NctsSADLineSpecialMentionGroupWrapper(NctsDepartureCargoDesc goodsItem)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
	}

	readonly NctsDepartureCargoDesc goodsItem;

	public IETLineSpecialMentionInfoAdditionalInformation AdditionalInformation => new NctsSADLineSpecialMentionInfoAdditionalInformationWrapper(goodsItem);

	public ISpecialMentionEoriInfo Eori => new NctsSADLineSpecialMentionEoriInfoWrapper();

	public ISpecialMentionUnloadingDataInfo UnloadingData => UnloadingDataCore;
	protected virtual ISpecialMentionUnloadingDataInfo UnloadingDataCore => SADSpecialMentionUnloadingDataInfoWrapper.Empty();
}
