using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.OperationalActions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing.OperationalActions
{
	class DeclarationUpdatePreviousDocumentsApplicatorValidationTest : BusinessObjectValidationTestCase
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

		public void TestCheckClass()
		{
			applicator.Validation.ValidateClass();
			AssertNoMessageError(applicator.ClassInfo, "You have not entered a Class.");

			applicator.DocumentCode = "bla";
			applicator.Validation.ValidateClass();
			AssertHasMessageError(applicator.ClassInfo, "You have not entered a Class.");

			applicator.Class = PreviousDocumentClassList.Codes.PreviousDocument;
			AssertNoMessageError(applicator.ClassInfo, "You have not entered a Class.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			applicator = new DeclarationUpdatePreviousDocumentsApplicator(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}
		DeclarationUpdatePreviousDocumentsApplicator applicator;
	}
}
