using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.OperationalActions;

namespace Enterprise.Customs.EU.Business.Testing.OperationalActions
{
	public class DeclarationUpdateSupportingDocumentsApplicatorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDocumentCode()
		{
			applicator.Validation.ValidateDocumentCode();
			AssertNoMessageError(applicator.DocumentCodeInfo, "You have not entered a Document Code.");

			applicator.ReferenceNumber = "bla";
			applicator.Validation.ValidateDocumentCode();
			AssertHasMessageError(applicator.DocumentCodeInfo, "You have not entered a Document Code.");

			applicator.DocumentCode = "test1";
			AssertNoMessageError(applicator.DocumentCodeInfo, "You have not entered a Document Code.");
		}

		public void TestCheckReferenceNumber()
		{
			applicator.Validation.ValidateReferenceNumber();
			AssertNoMessageError(applicator.ReferenceNumberInfo, "You have not entered a Reference Number.");

			applicator.DocumentCode = "bla";
			applicator.Validation.ValidateReferenceNumber();
			AssertHasMessageError(applicator.ReferenceNumberInfo, "You have not entered a Reference Number.");

			applicator.ReferenceNumber = "test1";
			AssertNoMessageError(applicator.ReferenceNumberInfo, "You have not entered a Reference Number.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			applicator = new DeclarationUpdateSupportingDocumentsApplicator(Factory);
		}
		DeclarationUpdateSupportingDocumentsApplicator applicator;
	}
}
