using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AdditionalInformationProvider : IAdditionalInformation
	{
		readonly AdditionalInfoSendingObject additionalInfoObject;

		public AdditionalInformationProvider(AdditionalInfoSendingObject additionalInfoObject)
		{
			this.additionalInfoObject = Argument.NotNull(additionalInfoObject, nameof(additionalInfoObject));
		}

		public string Code => additionalInfoObject.DocumentType;

		public string Text => additionalInfoObject.DocumentInformation;
	}
}
