using Enterprise.Customs.ES.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsArrivalMovementHeaderLookups : EU.NCTS.Business.NctsArrivalMovementHeaderLookups
{
	public NctsArrivalMovementHeaderLookups(NctsArrivalMovementHeader parent)
		: base(parent)
	{
	}

	public override CodeDescriptionPairList DeclarationTypeList => Factory.GetCachedValue<NctsArrivalDocTypeList>();

	public CodeDescriptionPairList LocationsList => LocationsHelper.GetESLocations(Factory);

	protected override CodeDescriptionPairList NctsTransitStatusListCore => Parent.IsPhase5 ? Factory.GetCachedValue<ESNCTS5ArrivalCustomsStatusList>() : Factory.GetCachedValue<NctsTransitStatusList>();

	protected override CodeDescriptionPairList NctsMovementHeaderTransactionStatusListCore => Factory.GetCachedValue<ESNctsMovementHeaderTransactionStatusList>();
}
