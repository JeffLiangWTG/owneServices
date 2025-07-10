using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM413AndIM415DeclarationProvider : IIM413AndIM415Declaration
	{
		protected readonly MessageSendingObject messageSendingObject;
		readonly bool generateNewLRN;

		public IM413AndIM415DeclarationProvider(MessageSendingObject messageSendingObject, bool generateNewLRN)
		{
			this.messageSendingObject = messageSendingObject;
			this.generateNewLRN = generateNewLRN;
		}

		public string AdditionalDeclarationType => messageSendingObject.SubStyle;

		public string CustomsOfficeLodgement => messageSendingObject.Bill.Header.AMA_CustomsOffice;

		public string DeferredPayment => messageSendingObject.Bill.Header.AMA_PaymentAccountNumber;

		public IParties Parties => CachedValueHelper.GetValue(ref partiesCached, () => new PartiesProvider((AsycudaManifestHeader)messageSendingObject.Bill.Header));
		CachedValue<IParties> partiesCached;

		public string PreferredPaymentMethod => messageSendingObject.Bill.Header.AMA_PaymentMethod;

		public string LRN => generateNewLRN ? AISOutboundEDIMessage.LRNPlaceHolder : messageSendingObject.LocalReferenceNumber.ToString();
	}
}
