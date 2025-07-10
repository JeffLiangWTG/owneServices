using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business;

public class PNTSAdditionalReferenceProvider : IPNTSDocument
{
	public PNTSAdditionalReferenceProvider(TemporaryStorageAdditionalInfo additionalInfo)
	{
		this.additionalInfo = Argument.NotNull(additionalInfo, nameof(additionalInfo));
	}
	readonly TemporaryStorageAdditionalInfo additionalInfo;

	public string ReferenceNumber => additionalInfo.CSI_ReferenceNumber;

	public string Type => additionalInfo.CSI_Code;
}
