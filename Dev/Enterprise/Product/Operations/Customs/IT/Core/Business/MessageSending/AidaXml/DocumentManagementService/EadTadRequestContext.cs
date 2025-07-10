using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

public sealed class EadTadRequestContext<TBusiness, TMessageParent> : DocumentManagementServiceRequestContext<TBusiness, TMessageParent>
	where TBusiness : BusinessObject, ICustomsProfileDataProvider, IMovementReferenceNumberProvider
	where TMessageParent : BusinessObject
{
	public EadTadRequestContext(TBusiness parent, TMessageParent messageParent, IGlbCertificateProvider glbCertificateProvider) : base(parent, messageParent, glbCertificateProvider)
	{
	}

	protected override string ServiceId => Ucc6XmlConstants.DocumentManagementServiceRequest.ServiceIds.EadTadRequest;
}
