using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class AdditionalInformationWrapper : IAdditionalInformation
{
	public AdditionalInformationWrapper(AdditionalInfo additionalInfo)
	{
		this.additionalInfo = Argument.NotNull(additionalInfo, nameof(additionalInfo));
	}

	readonly AdditionalInfo additionalInfo;

	public string Code => additionalInfo.CSI_Code;

	public string Description => additionalInfo.CSI_Description;
}
