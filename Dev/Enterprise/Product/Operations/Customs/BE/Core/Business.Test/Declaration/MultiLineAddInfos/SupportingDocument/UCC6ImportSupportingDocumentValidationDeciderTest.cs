using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class UCC6ImportSupportingDocumentValidationDeciderTest : TestCaseWithFactory
{
	public void TestSupportBR20311Rule()
	{
		var supportingDocumentValidationDecider = new UCC6ImportSupportingDocumentValidationDecider();
		AssertEquals(false, supportingDocumentValidationDecider.SupportBR20311Rule);
	}

	public void TestSupportC0612Rule()
	{
		var supportingDocumentValidationDecider = new UCC6ImportSupportingDocumentValidationDecider();
		AssertEquals(false, supportingDocumentValidationDecider.SupportC0612Rule);
	}

	public void TestShouldCheckIssuingAuthorityNameForEucdm()
	{
		var supportingDocumentValidationDecider = new UCC6ImportSupportingDocumentValidationDecider();
		AssertEquals(false, supportingDocumentValidationDecider.ShouldCheckIssuingAuthorityNameForEucdm);
	}
}
