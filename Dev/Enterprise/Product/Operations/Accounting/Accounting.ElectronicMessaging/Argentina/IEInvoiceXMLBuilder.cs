using System.Xml.Linq;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public interface IEInvoiceXmlBuilder
	{
		XStreamingElement BuildXml(TransactionInfo transaction, AccEInvoicingBatch accBatch);
	}
}
