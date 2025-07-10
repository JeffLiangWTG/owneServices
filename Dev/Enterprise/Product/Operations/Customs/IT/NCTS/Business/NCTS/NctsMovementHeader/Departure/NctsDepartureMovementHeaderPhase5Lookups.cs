using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class NctsDepartureMovementHeaderPhase5Lookups : EU.NCTS.Business.NctsDepartureMovementHeaderPhase5Lookups, INctsDepartureMovementHeaderLookups
{
	public NctsDepartureMovementHeaderPhase5Lookups(NctsDepartureMovementHeader parent) : base(parent)
	{
	}

	public CodeDescriptionPairList PaymentPartyList => new CodeDescriptionPairList();

	public CodeDescriptionPairList DefermentApprovalNumberList => new CodeDescriptionPairList();

	public CodeDescriptionPairList CustomsChannelCodeList => Factory.GetCachedValue<CustomsChannelCodeList>();

	public CodeDescriptionPairList NctsParticipantTypeList => new CodeDescriptionPairList();

	protected override CodeDescriptionPairList TransportAtBorderTypeOfIdListCore =>
		TransportAtBorderTypeOfIdLookupsHelper.GetCachedTransportAtBorderTypeOfIdList(Parent);
}
