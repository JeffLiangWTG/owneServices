using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM413DeclarationProvider : IM413AndIM415DeclarationProvider, IIM413Declaration
	{
		public IM413DeclarationProvider(MessageSendingObject messageSendingObject) : base(messageSendingObject, generateNewLRN: false)
		{
		}

		public string DetailsAmended => messageSendingObject.AmendmentInvalidationReason;

		public string MRN => messageSendingObject.Bill.MovementReferenceNumber;
	}
}
