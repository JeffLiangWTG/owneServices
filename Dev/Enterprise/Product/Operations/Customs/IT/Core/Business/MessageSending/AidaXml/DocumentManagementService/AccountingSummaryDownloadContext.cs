using Enterprise.Customs.IT.Business.Declaration;
namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

public sealed class AccountingSummaryDownloadContext : DocumentManagementServiceRequestContext<CusEntryHeader>
{
	public AccountingSummaryDownloadContext(CusEntryHeader entryHeader, IGlbCertificateProvider glbCertificateProvider) : base(entryHeader, glbCertificateProvider)
	{
	}

	protected override string ServiceId => Ucc6XmlConstants.DocumentManagementServiceRequest.ServiceIds.AccountingSummaryDownload;
}
