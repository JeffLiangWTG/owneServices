using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public class AdditionalInformationWrapper : IAdditionalInformation
{
	public AdditionalInformationWrapper(AdditionalInfo additionalInfo)
	{
		this.additionalInfo = Argument.NotNull(additionalInfo, nameof(additionalInfo));
	}

	readonly AdditionalInfo additionalInfo;

	string IAdditionalInformation.Code => additionalInfo.CSI_Code;

	string IAdditionalInformation.Description => additionalInfo.CSI_Description;
}
