using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class AdditionalTransitOperationLookups : CusSupportingInfoLookups
{
	public AdditionalTransitOperationLookups(AdditionalTransitOperation parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList IssuerTypeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1150, ValuationDate);

	public CodeDescriptionPairList StateOfSealsValidList => Factory.GetCachedValue<Universal.CodeDescriptionPairLists.YesNoList>();

	public override CodeDescriptionPairList PackTypeList => NctsPackageLookups.GetPackageUnitTypeList(Factory);

	new AdditionalTransitOperation Parent => (AdditionalTransitOperation)base.Parent;

	ZDateTime ValuationDate => Parent.Parent?.ValuationDate ?? ZDateTime.Now;
}
