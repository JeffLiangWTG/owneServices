using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

public class EURCertificateOfOriginWrapper : EU.Business.Documents.CertificateOfOrigin.EURCertificateOfOriginWrapper
{
	public EURCertificateOfOriginWrapper(JobDeclaration declaration) : base(declaration)
	{
	}

	public EURCertificateOfOriginWrapper(CusEntryHeader entryHeader) : base(entryHeader)
	{
	}

	protected override EU.Business.Documents.CertificateOfOrigin.ICustomsEndorsement GetNewCustomsEndorsement()
	{
		if (EntryHeader is CusEntryHeader actualEntryHeader)
		{
			return new CustomsEndorsementWrapper(actualEntryHeader);
		}
		return base.GetNewCustomsEndorsement();
	}
}
