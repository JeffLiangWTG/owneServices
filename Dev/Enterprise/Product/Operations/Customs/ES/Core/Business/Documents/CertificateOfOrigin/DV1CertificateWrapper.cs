using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Documents.DocDataObjects;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin
{
	public class DV1CertificateWrapper : EU.Business.Documents.CertificateOfOrigin.DV1CertificateWrapper
	{
		public DV1CertificateWrapper(JobDeclaration declaration) : base(declaration)
		{
		}

		public DV1CertificateWrapper(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override EU.Business.Documents.DocDataObjects.EntryHeaderDataObject GetNewEntryHeaderDataObject(EU.Business.Declaration.CusEntryHeader entryHeader) => new EntryHeaderDataObject((CusEntryHeader)entryHeader);
	}
}
