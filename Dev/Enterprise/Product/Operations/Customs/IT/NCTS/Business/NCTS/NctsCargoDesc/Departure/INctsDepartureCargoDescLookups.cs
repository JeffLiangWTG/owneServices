using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

public interface INctsDepartureCargoDescLookups
{
	ICodeDescriptionPairList CPCList { get; }

	CodeDescriptionPairList StatusList { get; }

	CodeDescriptionPairList PortTaxRateList { get; }

	RefCountryStatesCollection OriginStates { get; }
}
