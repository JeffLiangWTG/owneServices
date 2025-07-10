using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public sealed class NctsUniqueTransactionIdentifierRequestContext : IUniqueTransactionIdentifierRequestContext
{
	public NctsUniqueTransactionIdentifierRequestContext(NctsHeader nctsHeader, IGlbCertificateProvider certificateProvider, ZString uniqueTransactionID, ZString messageNumber)
	{
		this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		this.certificateProvider = Argument.NotNull(certificateProvider, nameof(certificateProvider));
		this.uniqueTransactionID = Argument.NotNullOrEmpty(uniqueTransactionID, nameof(uniqueTransactionID));
		this.messageNumber = Argument.NotNullOrEmpty(messageNumber, nameof(messageNumber));
	}

	ZString IUniqueTransactionIdentifierRequestContext.UniqueTransactionID => uniqueTransactionID;

	BusinessObject IUniqueTransactionIdentifierRequestContext.RequestParent => nctsHeader.MovementHeader;

	ZString IUniqueTransactionIdentifierRequestContext.MessageNumber => messageNumber;

	IGlbExternalPassword IUniqueTransactionIdentifierRequestContext.MauCertificate
		=> mauCertificate ?? certificateProvider.GetMauCertificatePassword(nctsHeader.BH_CustomsProfile);

	readonly IGlbExternalPassword mauCertificate;

	readonly NctsHeader nctsHeader;
	readonly IGlbCertificateProvider certificateProvider;
	readonly ZString uniqueTransactionID;
	readonly ZString messageNumber;
}
