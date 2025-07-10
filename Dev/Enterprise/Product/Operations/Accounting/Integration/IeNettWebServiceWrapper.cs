using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	public interface IeNettWebServiceWrapper
	{
		bool ProcessOfflinePayment(AccTransactionMatchLink matchLink, AccTransactionHeader header);
		bool ProcessOfflinePayment(ZDecimal amount, AccTransactionHeader header);
		GetFxQuoteResult GetFxQuote(OrgHeader toClient, ZString paymentMethod, ZString currency, ZDecimal amount);
#if DEBUG
		GetFxQuoteResult GetFxQuote(OrgHeader toClient, ZString paymentMethod, ZString currency, ZDecimal amount, ZDecimal testModifier);
#endif

		ZString DisplayClientList(BusinessObjectCollection comPayRegisteredOrganisations);
		eNettResponseWithMessages GetContainerStorageFee(BusinessObject invoice);
		eNettWebServiceResult ProcessCreditCard(IeNettTransaction header, string cardSecurityCode);
		eNettWebServiceResult ProcessDirectDebitFx(IeNettTransaction header, int quoteID);
	}

	public struct GetFxQuoteResult
	{
		public bool Success;
		public int QuoteID;
		public string Message;
		public ZDecimal LocalAmount;
		public ZDecimal Rate;
	}

	public struct eNettResponseWithMessages
	{
		public readonly bool Success;
		public readonly string[] Messages;

		public eNettResponseWithMessages(bool success, string message)
		{
			this.Success = success;
			this.Messages = new string[] { message };
		}

		public eNettResponseWithMessages(bool success, string[] messages)
		{
			this.Success = success;
			this.Messages = messages;
		}
	}

	public struct eNettWebServiceResult
	{
		public bool success;
		public string errorCode;
		public string errorMessage;
	}
}
