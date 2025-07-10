using CargoWise.Common.JSON.Extensions;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.ElectronicMessaging.China
{
	public class ChinaSearchEInvoicePayloadCreator : IChinaPayloadCreator
	{
		public ChinaSearchEInvoicePayloadCreator(TransactionHeader transactionHeader)
		{
			TransactionHeader = transactionHeader;
		}

		readonly TransactionHeader TransactionHeader;
		const string ReqTypeConstant = "05";

		string IChinaPayloadCreator.CreatePayloadAsJson()
		{
			var data = new SearchInvoiceData()
			{
				SerialNumber = ChinaEInvoiceHelper.GetSerialNumber(TransactionHeader),
				Extend = ChinaEInvoiceHelper.GetExtend(TransactionHeader)
			};

			var searchInvoice = new ChinaSearchEInvoice()
			{
				ReqType = ReqTypeConstant,
				Data = data
			};

			return searchInvoice.ToJSON();
		}
	}
}
