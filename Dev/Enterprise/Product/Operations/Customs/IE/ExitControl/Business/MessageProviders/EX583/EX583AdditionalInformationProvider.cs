using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public class EX583AdditionalInformationProvider : IAdditionalInformation
	{
		public EX583AdditionalInformationProvider(AdditionalInfoSendingObject sendingObject)
		{
			additionalInfoSendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}

		readonly AdditionalInfoSendingObject additionalInfoSendingObject;

		public string Code => additionalInfoSendingObject.DocumentType;

		public string Text => additionalInfoSendingObject.DocumentInformation;
	}
}
