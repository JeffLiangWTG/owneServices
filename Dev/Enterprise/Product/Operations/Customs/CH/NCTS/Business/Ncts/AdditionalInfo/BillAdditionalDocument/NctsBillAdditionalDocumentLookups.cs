using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsBillAdditionalDocumentLookups : EU.NCTS.Business.NctsBillAdditionalDocumentLookups
{
	public NctsBillAdditionalDocumentLookups(NctsBillAdditionalDocument parent) : base(parent)
	{
	}

	new NctsBillAdditionalDocument Parent => (NctsBillAdditionalDocument)base.Parent;

	public override CodeDescriptionPairList SubTypeList
	{
		get
		{
			var isNationalTransitSwitzerland = Parent?.Header?.MovementHeader is NctsDepartureMovementHeader movementHeader && movementHeader.IsNationalTransitSwitzerland;

			return Factory.GetCachedValue("CH.NctsBillAdditionalDocumentLookups.SubTypeList_" + isNationalTransitSwitzerland, () =>
			{
				var list = new AdditionalInfoSubTypeList();
				if (isNationalTransitSwitzerland)
				{
					list.RemoveCode(AdditionalInfoSubTypeList.Codes.AdditionalReference);
				}
				return list;
			});
		}
	}

	public new ZZRefCusCodeListCombinedCollection TypeCodeList
	{
		get
		{
			var subType = Parent.GetCodeTypeBySubType();
			var isNationalTransit = Parent?.Header?.IsNationalTransitSwitzerland ?? false;
			return subType.IsEmpty ? new ZZRefCusCodeListCombinedCollection(Factory) : Factory.GetCachedValue("NctsBillAdditionalDocumentLookups.TypeCodeList.SubType_" + subType + ".IsNationalTransit_" + isNationalTransit , delegate
			{
				return CusSupportingInfoHelper.GetTypeCodeList(Factory, subType, true, (NoResString)"House", Core.Constants.CountryCodes.Switzerland);
			});
		}
	}
}
