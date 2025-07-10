using Enterprise.Accounting.ElectronicMessaging.Argentina;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency
{
	public interface IArgentinaEInvoicingDependencyFactory
	{
		IComprobanteCAERequestBuilder GetComprobanteCAERequestBuilder();
		IAuthRequestBuilder GetAuthRequestBuilder();
		IItemDetailEInvoiceXmlBuilder GetItemDetailEInvoiceXmlBuilder();
		IComplianceSequenceRetriever GetComplianceSequenceRetriever();
		IArgentinaEInvoiceHelper GetArgentinaEInvoiceHelper();
	}
}
