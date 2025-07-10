using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportSupportingDocumentValidationDecider))]
	sealed class UCC6ImportSupportingDocumentValidationDeciderTest : TestCaseWithFactory
	{
		public void TestSupportBR20311Rule()
		{
			var validationDecider = new UCC6ImportSupportingDocumentValidationDecider();
			AssertEquals(true, validationDecider.SupportBR20311Rule);
		}

		public void TestSupportC0612Rule()
		{
			var validationDecider = new UCC6ImportSupportingDocumentValidationDecider();
			AssertEquals(false, validationDecider.SupportC0612Rule);
		}

		public void TestShouldCheckIssuingAuthorityNameForEucdm()
		{
			var validationDecider = new UCC6ImportSupportingDocumentValidationDecider();
			AssertEquals(false, validationDecider.ShouldCheckIssuingAuthorityNameForEucdm);
		}
	}
}
