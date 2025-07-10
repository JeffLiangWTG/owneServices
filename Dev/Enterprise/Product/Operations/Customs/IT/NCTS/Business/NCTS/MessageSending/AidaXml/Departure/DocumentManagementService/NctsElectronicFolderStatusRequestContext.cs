using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public sealed class NctsElectronicFolderStatusRequestContext : DocumentManagementServiceRequestContext<NctsHeader, NctsDepartureMovementHeader>
{
	public NctsElectronicFolderStatusRequestContext(NctsHeader nctsHeader, IGlbCertificateProvider glbCertificateProvider) : base(nctsHeader, nctsHeader.MovementHeader, glbCertificateProvider)
	{
	}

	protected override string ServiceId => Ucc6XmlConstants.DocumentManagementServiceRequest.ServiceIds.EFStatusRequest;
}
