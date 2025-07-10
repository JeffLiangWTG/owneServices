using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	[TestedType(typeof(UCC6ImportSupportingDocumentValidationDecider))]
	sealed class UCC6ImportSupportingDocumentValidationDeciderTest : TestCaseWithFactory
	{
		public void TestSupportBR20311Rule()
		{
			var supportingDocumentValidationDecider = new UCC6ImportSupportingDocumentValidationDecider();
			AssertEquals(true, supportingDocumentValidationDecider.SupportBR20311Rule);
		}

		public void TestSupportC0612Rule()
		{
			var validationDecider = new UCC6ImportSupportingDocumentValidationDecider();
			AssertEquals(expected: false, validationDecider.SupportC0612Rule);
		}

		public void TestShouldCheckIssuingAuthorityNameForEucdm()
		{
			var supportingDocumentValidationDecider = new UCC6ImportSupportingDocumentValidationDecider();
			AssertEquals(true, supportingDocumentValidationDecider.ShouldCheckIssuingAuthorityNameForEucdm);
		}
	}
}
