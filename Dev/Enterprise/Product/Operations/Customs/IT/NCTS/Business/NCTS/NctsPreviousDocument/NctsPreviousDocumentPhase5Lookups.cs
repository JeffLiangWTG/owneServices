using System.Collections;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using UniversalReferenceConstants = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsPreviousDocumentPhase5Lookups : EU.NCTS.Business.NctsPreviousDocumentPhase5Lookups
{
	public NctsPreviousDocumentPhase5Lookups(EU.NCTS.Business.NctsPreviousDocument parent) : base(parent)
	{
	}

	public override ICollection CodeList => GetTypeCodeList();

	public override CodeDescriptionPairList PackTypeList => PreviousDocumentLookupsHelper.GetPackageTypeList(Factory);

	protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;

	public override CodeDescriptionPairList UnitOfQuantityList => ShouldRestrictToKilogramUnit
	? PreviousDocumentLookupsHelper.GetKGMUomList(Factory)
	: UniversalReferenceHelper.GetEuropeanUnionEUNCustomsUQCodeList(Factory);

	#region Implementation

	ZZRefCusCodeListCombinedCollection GetTypeCodeList()
	{
		var result = CusSupportingInfoHelper.GetTypeCodeList(Factory,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS,
			includeParent: true,
			levelAttributeValue: UniversalReferenceConstants.RefCusCodeListLevelTypes.Item,
			Core.Constants.CountryCodes.Italy);
		result.Load();
		result.Sort(nameof(ZZRefCusCodeListCombined.ZZD_Code));

		return result;
	}

	bool ShouldRestrictToKilogramUnit => Parent.IsTypeN337
		&& Parent.Parent is NctsDepartureCargoDesc goodsItem
		&& goodsItem.IsPhase5Departure;

	#endregion
}
