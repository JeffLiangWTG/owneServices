using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class GoodsDeclarationDataProvider : IGoodsDeclaration
{
	public static IEnumerable<GoodsDeclarationDataProvider> NewCollection(MovementReferenceNumberSupportingInfoCollection movementReferenceNumbers)
		=> movementReferenceNumbers?.Select(x => new GoodsDeclarationDataProvider(x));

	GoodsDeclarationDataProvider(MovementReferenceNumberSupportingInfo movementReferenceNumber)
	{
		this.movementReferenceNumber = movementReferenceNumber;
	}
	readonly MovementReferenceNumberSupportingInfo movementReferenceNumber;

	public int SequenceNumber => movementReferenceNumber.CSI_LineNo;

	public string MRN => movementReferenceNumber.CSI_ReferenceNumber;

	public string AdditionalInformation => movementReferenceNumber.CSI_Description.ReturnNullIfEmpty();

	public bool? SealIsValid => movementReferenceNumber.CSI_Status.ToOptionalSealIsValid();

	public IOppositeInformation OppositeInformation => oppositeInformation ?? (oppositeInformation = GoodsDeclarationOppositeInformationDataProvider.New(movementReferenceNumber));
	IOppositeInformation oppositeInformation;
}
