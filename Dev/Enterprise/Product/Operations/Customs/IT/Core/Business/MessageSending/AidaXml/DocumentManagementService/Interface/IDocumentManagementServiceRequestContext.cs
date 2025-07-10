using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business;

public interface IDocumentManagementServiceRequestContext
{
	IAidaXmlSigner XmlSigner { get; }

	ICryptokiGlbExternalPassword CryptokiCertificate { get; }

	IGlbMauExternalPassword MauCertificate { get; }

	string ServiceId { get; }
}

public interface IDocumentManagementServiceRequestContext<TBusiness, TMessageParent> : IDocumentManagementServiceRequestContext
	where TBusiness : BusinessObject, ICustomsProfileDataProvider, IMovementReferenceNumberProvider
	where TMessageParent : BusinessObject
{
	TBusiness BusinessObject { get; }
	TMessageParent MessageParent { get; }
}
