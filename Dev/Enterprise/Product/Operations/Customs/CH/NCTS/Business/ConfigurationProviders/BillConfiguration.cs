using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class BillConfiguration : EU.NCTS.Business.BillConfiguration
{
	protected override INctsBillPhase5ValidationDecider GetBillDeparturePhase5ValidationDecider() => new NctsBillDeparturePhase5ValidationDecider();

	protected override INctsBillAdditionalDocumentValidationDecider GetBillAdditionalDocumentPhase5ValidationDecider() => new NctsBillAdditionalDocumentPhase5ValidationDecider();
}
