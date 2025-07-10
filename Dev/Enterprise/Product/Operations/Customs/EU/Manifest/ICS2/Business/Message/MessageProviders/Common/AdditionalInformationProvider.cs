using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AdditionalInformationProvider : IAdditionalInformation
	{
		public AdditionalInformationProvider(CusSupportingInfo additionalInfo)
		{
			additionalInformation = Argument.NotNull(additionalInfo, nameof(additionalInfo));
		}

		readonly CusSupportingInfo additionalInformation;

		public string Code => additionalInformation.CSI_Code.IsEmpty ? null : (string)additionalInformation.CSI_Code;

		public string Text => additionalInformation.CSI_Description.IsEmpty ? null : (string)additionalInformation.CSI_Description;

		public string Type => additionalInformation.CSI_SubType;
	}
}
