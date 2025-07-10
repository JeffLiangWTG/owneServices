namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public class MexicoEInvoicingDependencyFactory : IMexicoEInvoicingDependencyFactory
	{
		ICFDiCancellationBuilder IMexicoEInvoicingDependencyFactory.GetCFDiCancellationBuilder() => new CFDiCancellationBuilder();
		ICFDiRelacionadosBuilder IMexicoEInvoicingDependencyFactory.GetCFDiRelacionadosBuilder() => new CFDiRelacionadosBuilder();
		ICertificateHelper IMexicoEInvoicingDependencyFactory.GetCertificateHelper() => new CertificateHelper();
		ICompanyCredential IMexicoEInvoicingDependencyFactory.GetCompanyCredential() => new CompanyCredential();
		IEInvoiceHelper IMexicoEInvoicingDependencyFactory.GetEInvoiceHelper() => new EInvoiceHelper();
		ICFDiObtenerPDFBuilder IMexicoEInvoicingDependencyFactory.GetCFDiObtenerPDFBuilder() => new CFDiObtenerPDFBuilder();
	}
}
