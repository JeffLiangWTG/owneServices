using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

public class ITATRCertificateOfOriginWrapper : EU.Business.Documents.CertificateOfOrigin.ATRCertificateOfOriginWrapper
{
	public ITATRCertificateOfOriginWrapper(JobDeclaration declaration) : base(declaration)
	{
	}

	public ITATRCertificateOfOriginWrapper(CusEntryHeader entryHeader) : base(entryHeader)
	{
	}

	protected override EU.Business.Documents.CertificateOfOrigin.ICustomsEndorsement GetNewCustomsEndorsement() => new CustomsEndorsementWrapper((CusEntryHeader)EntryHeader);
}
