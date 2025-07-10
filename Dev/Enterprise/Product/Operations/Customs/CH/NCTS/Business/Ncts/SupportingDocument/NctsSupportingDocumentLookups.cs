using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using CoreConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsSupportingDocumentLookups : NctsSupportingDocumentPhase5Lookups
{
	public NctsSupportingDocumentLookups(NctsSupportingDocument parent) : base(parent)
	{
	}

	public override ZZRefCusCodeListCombinedCollection TypeCodeList
	{
		get
		{
			var isNationalTransit = ((NctsDepartureMovementHeader)Parent?.Header?.MovementHeader)?.IsNationalTransitSwitzerland ?? false;

			return isNationalTransit ? Factory.GetCachedValue("CH.NctsSupportingDocumentLookups.TypeCodeList", () => GetTypeCodeListNationalTransit()) : base.TypeCodeList;
		}
	}

	ZZRefCusCodeListCombinedCollection GetTypeCodeListNationalTransit()
	{
		return CusSupportingInfoHelper.GetTypeCodeList(Factory, CoreConstants.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, false, CusSupportingInfoHelper.GetLevelAttributeValue(Parent), Core.Constants.CountryCodes.Switzerland);
	}
}
