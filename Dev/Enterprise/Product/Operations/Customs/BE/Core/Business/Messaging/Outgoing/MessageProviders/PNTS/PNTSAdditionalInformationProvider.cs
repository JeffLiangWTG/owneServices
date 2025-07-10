using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business;

public class PNTSAdditionalInformationProvider : IPNTSAdditionalInformation
{
	public PNTSAdditionalInformationProvider(TemporaryStorageAdditionalInfo additionalInfo)
	{
		this.additionalInfo = Argument.NotNull(additionalInfo, nameof(additionalInfo));
	}
	readonly TemporaryStorageAdditionalInfo additionalInfo;

	public string Text => additionalInfo.CSI_Description;

	public string Code => additionalInfo.CSI_Code;
}
