using CargoWise.Common;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public sealed class IvistoRequestContext : IIvistoRequestContext
{
	public IvistoRequestContext(CusEntryHeader header, IGlbCertificateProvider glbCertificateProvider)
	{
		entryHeader = Argument.NotNull(header, nameof(header));
		declaration = Argument.NotNull(header.Declaration, nameof(header.Declaration));
		this.glbCertificateProvider = Argument.NotNull(glbCertificateProvider, nameof(glbCertificateProvider));
	}

	IGlbMauExternalPassword IIvistoRequestContext.MauCertificate
		=> mauPassword ?? (mauPassword = glbCertificateProvider.GetMauCertificatePassword(declaration.JE_CustomsProfile));

	CusEntryHeader IIvistoRequestContext.EntryHeader => entryHeader;

	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;
	readonly IGlbCertificateProvider glbCertificateProvider;
	IGlbMauExternalPassword mauPassword;
}
