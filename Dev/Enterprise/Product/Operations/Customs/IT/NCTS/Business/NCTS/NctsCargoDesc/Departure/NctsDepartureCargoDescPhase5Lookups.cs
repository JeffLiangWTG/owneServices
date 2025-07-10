using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class NctsDepartureCargoDescPhase5Lookups : EU.NCTS.Business.NctsDepartureCargoDescPhase5Lookups, INctsDepartureCargoDescLookups
{
	public NctsDepartureCargoDescPhase5Lookups(NctsDepartureCargoDesc parent) : base(parent)
	{
	}

	public ICodeDescriptionPairList CPCList => new CodeDescriptionPairList();

	public CodeDescriptionPairList StatusList => Factory.GetCachedValue<NctsDeletionStatusList>();

	public CodeDescriptionPairList PortTaxRateList => new CodeDescriptionPairList();
}
