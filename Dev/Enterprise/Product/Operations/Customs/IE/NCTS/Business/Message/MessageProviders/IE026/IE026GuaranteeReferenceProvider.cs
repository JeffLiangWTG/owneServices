using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE026GuaranteeReferenceProvider : IIE026GuaranteeReference
	{
		public IE026GuaranteeReferenceProvider(GuaranteeAccessCodesSendingAction sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}
		readonly GuaranteeAccessCodesSendingAction sendingObject;

		public string Grn => sendingObject.GuaranteeReferenceNumber;

		public string MasterAccessCode => sendingObject.MasterCode;

		public IIE026AccessCode AccessCode => accessCodeProvider ?? (accessCodeProvider = new IE026AccessCodeProvider(sendingObject));
		IIE026AccessCode accessCodeProvider;
	}
}
