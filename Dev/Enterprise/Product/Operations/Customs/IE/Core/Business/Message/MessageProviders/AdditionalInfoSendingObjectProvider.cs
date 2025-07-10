using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;

namespace Enterprise.Customs.IE.Business
{
	public class AdditionalInfoSendingObjectProvider : IAdditionalInformation
	{
		public AdditionalInfoSendingObjectProvider(AdditionalInfoSendingObject sendingObject)
		{
			Argument.NotNull(sendingObject, nameof(sendingObject));
			Code = sendingObject.DocumentType;
			Text = sendingObject.DocumentInformation;
		}

		public string Code { get; }

		public string Text { get; }
	}
}
