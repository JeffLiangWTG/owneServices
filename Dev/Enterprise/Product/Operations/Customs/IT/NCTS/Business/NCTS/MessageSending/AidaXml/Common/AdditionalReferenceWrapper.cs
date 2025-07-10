using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using EUAdditionalInfo = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class AdditionalReferenceWrapper : IAdditionalReference
{
	public AdditionalReferenceWrapper(EUAdditionalInfo additionalInfo)
	{
		this.additionalInfo = Argument.NotNull(additionalInfo, nameof(additionalInfo));
	}

	string IAdditionalReference.ReferenceNumber => additionalInfo.CSI_ReferenceNumber;

	string IAdditionalReference.ReferenceType => additionalInfo.CSI_Code;

	readonly EUAdditionalInfo additionalInfo;
}
