using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class AdditionalReferenceWrapper : IAdditionalReference
{
	public AdditionalReferenceWrapper(AdditionalInfo additionalReference)
	{
		this.additionalReference = Argument.NotNull(additionalReference, nameof(additionalReference));
	}

	readonly AdditionalInfo additionalReference;

	string IAdditionalReference.ReferenceNumber => additionalReference.CSI_ReferenceNumber;

	string IAdditionalReference.ReferenceType => additionalReference.CSI_Code;
}
