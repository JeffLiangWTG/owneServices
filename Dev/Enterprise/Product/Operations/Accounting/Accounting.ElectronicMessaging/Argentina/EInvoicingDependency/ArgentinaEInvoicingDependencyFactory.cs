using Enterprise.Accounting.ElectronicMessaging.Argentina;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency
{
	public class ArgentinaEInvoicingDependencyFactory : IArgentinaEInvoicingDependencyFactory
	{
		IComprobanteCAERequestBuilder IArgentinaEInvoicingDependencyFactory.GetComprobanteCAERequestBuilder() => new ComprobanteCAERequestBuilder();
		IAuthRequestBuilder IArgentinaEInvoicingDependencyFactory.GetAuthRequestBuilder() => new AuthRequestBuilder();
		IItemDetailEInvoiceXmlBuilder IArgentinaEInvoicingDependencyFactory.GetItemDetailEInvoiceXmlBuilder() => new ItemDetailEInvoiceXmlBuilder();
		IComplianceSequenceRetriever IArgentinaEInvoicingDependencyFactory.GetComplianceSequenceRetriever() => new ComplianceSequenceRetriever();
		IArgentinaEInvoiceHelper IArgentinaEInvoicingDependencyFactory.GetArgentinaEInvoiceHelper() => new ArgentinaEInvoiceHelper();
	}
}
