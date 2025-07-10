using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

public sealed class SummaryProspectusRequestContext : DocumentManagementServiceRequestContext<CusEntryHeader>
{
	public SummaryProspectusRequestContext(CusEntryHeader entryHeader, IGlbCertificateProvider glbCertificateProvider) : base(entryHeader, glbCertificateProvider)
	{
	}

	protected override string ServiceId => Ucc6XmlConstants.DocumentManagementServiceRequest.ServiceIds.SummaryProspectusRequest;
}
