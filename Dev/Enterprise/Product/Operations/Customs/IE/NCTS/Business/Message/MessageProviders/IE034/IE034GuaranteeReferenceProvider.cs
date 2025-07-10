using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE034GuaranteeReferenceProvider : IIE034GuaranteeReference
	{
		public IE034GuaranteeReferenceProvider(QueryOnGuaranteeSendingAction sendingAction, QueryOnGuaranteeSendingObject sendingObject)
		{
			this.sendingAction = sendingAction;
			this.sendingObject = sendingObject;
		}
		readonly QueryOnGuaranteeSendingAction sendingAction;
		readonly QueryOnGuaranteeSendingObject sendingObject;

		public string Grn => sendingObject.GuaranteeReferenceNumber;

		public IIE034GuaranteeQuery GuaranteeQuery => new IE034GuaranteeQueryProvider(sendingAction);

		public string OwnerIdentificationNumber => null;

		public string AccessCode => sendingObject.AccessCode;
	}
}
