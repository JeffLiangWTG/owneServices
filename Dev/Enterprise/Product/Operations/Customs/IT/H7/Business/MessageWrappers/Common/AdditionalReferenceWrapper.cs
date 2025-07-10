using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class AdditionalReferenceWrapper : IAdditionalReference
{
	public AdditionalReferenceWrapper(AdditionalInfo additionalReference)
	{
		this.additionalReference = Argument.NotNull(additionalReference, nameof(additionalReference));
	}

	readonly AdditionalInfo additionalReference;

	public string ReferenceType => additionalReference.CSI_Code;

	public string ReferenceNumber => additionalReference.CSI_ReferenceNumber;
}
