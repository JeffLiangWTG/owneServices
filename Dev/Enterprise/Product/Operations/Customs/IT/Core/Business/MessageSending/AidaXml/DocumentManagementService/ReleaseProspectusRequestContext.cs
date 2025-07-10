using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

public sealed class ReleaseProspectusRequestContext : DocumentManagementServiceRequestContext<CusEntryHeader>
{
	public ReleaseProspectusRequestContext(CusEntryHeader entryHeader, IGlbCertificateProvider glbCertificateProvider)
		: base(entryHeader, glbCertificateProvider)
	{
	}

	protected override string ServiceId => Ucc6XmlConstants.DocumentManagementServiceRequest.ServiceIds.ReleaseProspectusRequest;
}
