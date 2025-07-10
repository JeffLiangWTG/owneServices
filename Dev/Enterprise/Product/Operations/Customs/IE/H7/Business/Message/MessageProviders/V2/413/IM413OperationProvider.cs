using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.H7.Business
{
	public class IM413OperationProvider : IM413_414_415OperationProvider, IIM413Operation
	{
		public IM413OperationProvider(MessageSendingObject messageSendingObject) : base(messageSendingObject, false)
		{
		}

		public string CustomsRegistrationNumber => null;

		public string DetailsAmended => null;

		public string MRN => messageSendingObject.Bill.MovementReferenceNumber;
	}
}
