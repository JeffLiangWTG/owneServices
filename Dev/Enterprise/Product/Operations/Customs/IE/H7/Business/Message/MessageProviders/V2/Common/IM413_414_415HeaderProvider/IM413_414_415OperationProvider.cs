using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public class IM413_414_415OperationProvider : IOperation
	{
		public IM413_414_415OperationProvider(MessageSendingObject messageSendingObject, bool generateNewLRN)
		{
			this.messageSendingObject = messageSendingObject;
			this.generateNewLRN = generateNewLRN;
		}

		readonly bool generateNewLRN;
		protected readonly MessageSendingObject messageSendingObject;

		public string MsgType => ImportDeclarationTypeList.Codes.H7;

		public string DeclarationType => null;

		public string AdditionalDeclarationType => messageSendingObject.SubStyle;

		public string LanguageCode => null;

		public string PreferredPaymentMethod => messageSendingObject.Bill.Header.AMA_PaymentMethod;

		public string LRN => generateNewLRN ? AISOutboundEDIMessage.LRNPlaceHolder : messageSendingObject.LocalReferenceNumber.ToString();
	}
}
