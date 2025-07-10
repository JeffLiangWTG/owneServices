using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class GoodsDeclarationOppositeInformationDataProvider : IOppositeInformation
{
	public static GoodsDeclarationOppositeInformationDataProvider New(MovementReferenceNumberSupportingInfo movementReferenceNumber)
		=> movementReferenceNumber == null ? null : new GoodsDeclarationOppositeInformationDataProvider(movementReferenceNumber);

	GoodsDeclarationOppositeInformationDataProvider(MovementReferenceNumberSupportingInfo movementReferenceNumber)
	{
		this.movementReferenceNumber = movementReferenceNumber;
	}
	readonly MovementReferenceNumberSupportingInfo movementReferenceNumber;

	public string ReferenceNumber => movementReferenceNumber.Parent?.BM_PaperlessInbondNum + "-" + movementReferenceNumber.CSI_LineNo;

	public string Detail => null;

	public string Text => CustomsMessageHelper.PartnerTopic;
}
