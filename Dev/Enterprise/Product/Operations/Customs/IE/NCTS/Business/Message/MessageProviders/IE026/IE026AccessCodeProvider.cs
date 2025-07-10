using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE026AccessCodeProvider : IIE026AccessCode
	{
		public IE026AccessCodeProvider(GuaranteeAccessCodesSendingAction sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}
		readonly GuaranteeAccessCodesSendingAction sendingObject;

		public string Current => sendingObject.CurrentCode;

		public string New => sendingObject.NewAccessCode;
	}
}
