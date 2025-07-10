using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

public sealed class SummaryProspectusDownloadContext : DocumentManagementServiceRequestContext<CusEntryHeader>
{
	public SummaryProspectusDownloadContext(CusEntryHeader entryHeader, IGlbCertificateProvider glbCertificateProvider) : base(entryHeader, glbCertificateProvider)
	{
	}

	protected override string ServiceId => Ucc6XmlConstants.DocumentManagementServiceRequest.ServiceIds.SummaryProspectusDownload;
}

