using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

public sealed class Eur1RequestContext : DocumentManagementServiceRequestContext<CusEntryHeader>
{
	public Eur1RequestContext(CusEntryHeader entryHeader, IGlbCertificateProvider glbCertificateProvider) : base(entryHeader, glbCertificateProvider)
	{
	}

	protected override string ServiceId => Ucc6XmlConstants.DocumentManagementServiceRequest.ServiceIds.Eur1Request;
}
