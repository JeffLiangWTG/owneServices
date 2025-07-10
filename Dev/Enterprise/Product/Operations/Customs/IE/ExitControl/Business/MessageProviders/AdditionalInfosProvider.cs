using CargoWise.Customs.IE.MessageContracts.Interfaces;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public class AdditionalInfosProvider : IDocument
	{
		public AdditionalInfosProvider(AdditionalInfo additionalInfo)
		{
			this.additionalInfo = additionalInfo;
		}
		readonly AdditionalInfo additionalInfo;

		public string Type => additionalInfo.CSI_Code;

		public string Reference => additionalInfo.CSI_ReferenceNumber;
	}
}
