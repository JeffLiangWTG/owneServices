using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.DocumentManagementService;

public abstract class DocumentManagementServiceRequestContext<TBusiness, TMessageParent> : IDocumentManagementServiceRequestContext<TBusiness, TMessageParent>
	where TBusiness : BusinessObject, ICustomsProfileDataProvider, IMovementReferenceNumberProvider
	where TMessageParent : BusinessObject
{
	protected DocumentManagementServiceRequestContext(TBusiness parent, TMessageParent messageParent, IGlbCertificateProvider glbCertificateProvider)
	{
		BusinessObject = Argument.NotNull(parent, nameof(parent));
		MessageParent = Argument.NotNull(messageParent, nameof(messageParent));
		this.glbCertificateProvider = Argument.NotNull(glbCertificateProvider, nameof(glbCertificateProvider));
	}

	protected abstract string ServiceId { get; }

	#region IDocumentManagementServiceRequestContext Implementation

	IAidaXmlSigner IDocumentManagementServiceRequestContext.XmlSigner => xmlSigner ??= new AidaXmlSigner();

	IAidaXmlSigner xmlSigner;

	ICryptokiGlbExternalPassword IDocumentManagementServiceRequestContext.CryptokiCertificate
		=> cryptokeiCertificate ??= glbCertificateProvider.GetCryptokiCertificate();

	ICryptokiGlbExternalPassword cryptokeiCertificate;

	IGlbMauExternalPassword IDocumentManagementServiceRequestContext.MauCertificate
		=> mauPassword ??= glbCertificateProvider.GetMauCertificatePassword(BusinessObject.CustomsProfile);

	IGlbMauExternalPassword mauPassword;

	public TBusiness BusinessObject { get; }

	public TMessageParent MessageParent { get; }

	string IDocumentManagementServiceRequestContext.ServiceId => ServiceId;

	#endregion

	readonly IGlbCertificateProvider glbCertificateProvider;
}

public abstract class DocumentManagementServiceRequestContext<TBusiness> : DocumentManagementServiceRequestContext<TBusiness, TBusiness>, IDocumentManagementServiceRequestContext<TBusiness, TBusiness>
	where TBusiness : BusinessObject, ICustomsProfileDataProvider, IMovementReferenceNumberProvider
{
	protected DocumentManagementServiceRequestContext(TBusiness parent, IGlbCertificateProvider glbCertificateProvider) : base(parent, parent, glbCertificateProvider)
	{
	}
}
