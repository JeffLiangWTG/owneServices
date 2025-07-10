using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

public sealed class Ucc6ElectronicFolderStatusRequestContext : DocumentManagementServiceRequestContext<CusEntryHeader>
{
	public Ucc6ElectronicFolderStatusRequestContext(CusEntryHeader header, IGlbCertificateProvider glbCertificateProvider)
		: base(header, glbCertificateProvider)
	{
	}

	protected override string ServiceId => Ucc6XmlConstants.DocumentManagementServiceRequest.ServiceIds.EFStatusRequest;
}
