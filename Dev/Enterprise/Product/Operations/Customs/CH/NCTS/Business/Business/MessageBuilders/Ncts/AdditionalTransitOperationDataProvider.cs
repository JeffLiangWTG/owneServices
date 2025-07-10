using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.NCTS.Business;

public class AdditionalTransitOperationDataProvider : IAdditionalTransitOperation
{
	public static IEnumerable<AdditionalTransitOperationDataProvider> NewCollection(AdditionalTransitOperationCollection additionalTransitOperations)
		=> additionalTransitOperations?.Select(x => new AdditionalTransitOperationDataProvider(x));

	AdditionalTransitOperationDataProvider(AdditionalTransitOperation additionalTransitOperation)
	{
		this.additionalTransitOperation = additionalTransitOperation;
	}
	readonly AdditionalTransitOperation additionalTransitOperation;

	public int SequenceNumber => additionalTransitOperation.CSI_LineNo;

	public string Type => additionalTransitOperation.CSI_IssuerType;

	public string ReferenceNumber => additionalTransitOperation.CSI_ReferenceNumber;

	public string DescriptionOfGoods => additionalTransitOperation.CSI_Description;

	public decimal GrossMass => additionalTransitOperation.CSI_Quantity;

	public int NumberOfPackages => additionalTransitOperation.CSI_PackQty;

	public bool? SealIsValid => additionalTransitOperation.CSI_Status.ToOptionalSealIsValid();
}
