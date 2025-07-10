using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

public interface INctsDepartureMovementHeaderLookups
{
	CodeDescriptionPairList PaymentPartyList { get; }
	CodeDescriptionPairList DefermentApprovalNumberList { get; }
	BondedWarehouseCollection BondedWarehouseCollection { get; }
	CodeDescriptionPairList CustomsChannelCodeList { get; }
	CodeDescriptionPairList NctsParticipantTypeList { get; }
}
