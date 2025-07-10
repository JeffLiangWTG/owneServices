using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class NctsTariffColumnStyle : ZBaseFindBoxColumnStyle
{
	public NctsTariffColumnStyle(NctsTariffColumnStyleInfo info) : base(() => CreateFindBoxControl(info), info)
	{
	}

	static ZFindBoxUserControl CreateFindBoxControl(NctsTariffColumnStyleInfo info)
	{
		return new NctsTariffGridFindBox
		{
			GetCountryCode = info.GetCountryCode,
			TariffType = info.TariffType,
			PartialDescriptionMinLengthForSearch = info.PartialDescriptionMinLengthForSearch,
			GetEffectiveDate = info.GetEffectiveDate,
			SelectNomenclatureModes = info.SelectNomenclatureModes,
			GetSelectNomenclatureModes = info.GetSelectNomenclatureModes,
			ShowDescriptionFilterOnNonNomenclatureTariffModule = info.ShowDescriptionFilterOnNonNomenclatureTariffModule,
			NeedLoadParentDataGroup = info.NeedLoadParentDataGroup
		};
	}
}
