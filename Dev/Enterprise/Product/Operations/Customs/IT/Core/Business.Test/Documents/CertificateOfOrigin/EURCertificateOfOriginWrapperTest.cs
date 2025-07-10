using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class EURCertificateOfOriginWrapperTest : TestCaseWithFactory
{
	public void TestCustomsEndorsement()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var wrapperFromEntryHeader = GetNewWrapperFromEntryHeader(entryHeader);
		AssertType<CustomsEndorsementWrapper>("CustomsEndorsement (from EntryHeader) Type", wrapperFromEntryHeader.CustomsEndorsement);

		var wrapperFromDeclaration = GetNewWrapperFromDeclaration(declaration);
		AssertType<EU.Business.Documents.CertificateOfOrigin.CustomsEndorsementWrapper>("CustomsEndorsement (from JobDeclaration) Type", wrapperFromDeclaration.CustomsEndorsement);
	}

	EU.Business.Documents.CertificateOfOrigin.IEURCertificateOfOrigin GetNewWrapperFromDeclaration(JobDeclaration declaration) => new EURCertificateOfOriginWrapper(declaration);

	EU.Business.Documents.CertificateOfOrigin.IEURCertificateOfOrigin GetNewWrapperFromEntryHeader(CusEntryHeader entryHeader) => new EURCertificateOfOriginWrapper(entryHeader);
}
