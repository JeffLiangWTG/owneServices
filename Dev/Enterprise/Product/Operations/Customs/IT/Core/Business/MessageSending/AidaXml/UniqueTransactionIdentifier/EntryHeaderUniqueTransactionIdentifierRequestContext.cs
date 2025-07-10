using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;

public sealed class EntryHeaderUniqueTransactionIdentifierRequestContext : IUniqueTransactionIdentifierRequestContext
{
	public EntryHeaderUniqueTransactionIdentifierRequestContext(CusEntryHeader entryHeader, IGlbCertificateProvider certificateProvider, ZString uniqueTransactionID, ZString messageNumber)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		this.certificateProvider = Argument.NotNull(certificateProvider, nameof(certificateProvider));
		this.uniqueTransactionID = Argument.NotNullOrEmpty(uniqueTransactionID, nameof(uniqueTransactionID));
		this.messageNumber = Argument.NotNullOrEmpty(messageNumber, nameof(messageNumber));
	}

	ZString IUniqueTransactionIdentifierRequestContext.UniqueTransactionID => uniqueTransactionID;

	BusinessObject IUniqueTransactionIdentifierRequestContext.RequestParent => entryHeader;

	ZString IUniqueTransactionIdentifierRequestContext.MessageNumber => messageNumber;

	IGlbExternalPassword IUniqueTransactionIdentifierRequestContext.MauCertificate
		=> mauCertificate ??= certificateProvider.GetMauCertificatePassword(declaration.JE_CustomsProfile);

	IGlbExternalPassword mauCertificate;

	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;
	readonly IGlbCertificateProvider certificateProvider;
	readonly ZString uniqueTransactionID;
	readonly ZString messageNumber;
}
