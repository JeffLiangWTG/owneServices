using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public sealed class TemporaryStorageHeaderUniqueTransactionIdentifierRequestContext : IUniqueTransactionIdentifierRequestContext
{
	public TemporaryStorageHeaderUniqueTransactionIdentifierRequestContext(TemporaryStorageHeader temporaryStorageHeader, IGlbCertificateProvider certificateProvider, ZString uniqueTransactionID, ZString messageNumber)
	{
		this.temporaryStorageHeader = Argument.NotNull(temporaryStorageHeader, nameof(temporaryStorageHeader));
		this.certificateProvider = Argument.NotNull(certificateProvider, nameof(certificateProvider));
		this.uniqueTransactionID = Argument.NotNullOrEmpty(uniqueTransactionID, nameof(uniqueTransactionID));
		this.messageNumber = Argument.NotNullOrEmpty(messageNumber, nameof(messageNumber));
	}

	ZString IUniqueTransactionIdentifierRequestContext.UniqueTransactionID => uniqueTransactionID;

	BusinessObject IUniqueTransactionIdentifierRequestContext.RequestParent => temporaryStorageHeader;

	ZString IUniqueTransactionIdentifierRequestContext.MessageNumber => messageNumber;

	IGlbExternalPassword IUniqueTransactionIdentifierRequestContext.MauCertificate
		=> mauCertificate ?? certificateProvider.GetMauCertificatePassword(temporaryStorageHeader.AMA_CustomsProfile);

	readonly IGlbExternalPassword mauCertificate;

	readonly TemporaryStorageHeader temporaryStorageHeader;
	readonly IGlbCertificateProvider certificateProvider;
	readonly ZString uniqueTransactionID;
	readonly ZString messageNumber;
}
