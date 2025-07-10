namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public interface IMexicoEInvoicingDependencyFactory
	{
		ICertificateHelper GetCertificateHelper();
		ICFDiCancellationBuilder GetCFDiCancellationBuilder();
		ICompanyCredential GetCompanyCredential();
		ICFDiRelacionadosBuilder GetCFDiRelacionadosBuilder();
		IEInvoiceHelper GetEInvoiceHelper();
		ICFDiObtenerPDFBuilder GetCFDiObtenerPDFBuilder();
	}
}
