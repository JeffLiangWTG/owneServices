using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CSARSFAssessmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckType()
		{
			csaRSFAssessment.Type = "HXU";
			csaRSFAssessment.Validation.ValidateType();
			AssertHasWarningContaining(csaRSFAssessment.TypeInfo, ListValidation.InvalidCodeMessage);
			csaRSFAssessment.Type = CustomsAssessmentsCodes.Codes.K100B;
			csaRSFAssessment.Validation.ValidateType();
			AssertNoWarningContaining(csaRSFAssessment.TypeInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestReferenceNumber()
		{
			csaRSFAssessment.Type = CustomsAssessmentsCodes.Codes.B2Dash1;
			csaRSFAssessment.ReferenceNumber = "abcdefghijklmn";
			csaRSFAssessment.Validation.ValidateReferenceNumber();
			AssertHasWarningContaining(csaRSFAssessment.ReferenceNumberInfo, "Reference Number should be composed of 14 digits numbers.");
			csaRSFAssessment.ReferenceNumber = "1";
			csaRSFAssessment.Validation.ValidateReferenceNumber();
			AssertHasWarningContaining(csaRSFAssessment.ReferenceNumberInfo, "Reference Number should be composed of 14 digits numbers.");
			csaRSFAssessment.ReferenceNumber = "12345678901234";
			csaRSFAssessment.Validation.ValidateReferenceNumber();
			AssertNoWarningContaining(csaRSFAssessment.ReferenceNumberInfo, "Reference Number should be composed of 14 digits numbers.");

			AssertNumberic_UpTo10Digits(CustomsAssessmentsCodes.Codes.K23);
			AssertNumberic_UpTo10Digits(CustomsAssessmentsCodes.Codes.PAAndCP);

			AssertAlphaAndNumberic_UpTo17Digits(CustomsAssessmentsCodes.Codes.K100B);
			AssertAlphaAndNumberic_UpTo17Digits(CustomsAssessmentsCodes.Codes.K9);
			AssertAlphaAndNumberic_UpTo17Digits(CustomsAssessmentsCodes.Codes.K25);
			AssertAlphaAndNumberic_UpTo17Digits(CustomsAssessmentsCodes.Codes.K29);
		}

		void AssertNumberic_UpTo10Digits(string type)
		{
			csaRSFAssessment.Type = type;
			csaRSFAssessment.ReferenceNumber = "12345678901";
			csaRSFAssessment.Validation.ValidateReferenceNumber();
			AssertHasWarningContaining(csaRSFAssessment.ReferenceNumberInfo, "Reference Number should be number and up to 10 digits.");
			csaRSFAssessment.ReferenceNumber = "abvdefghij";
			csaRSFAssessment.Validation.ValidateReferenceNumber();
			AssertHasWarningContaining(csaRSFAssessment.ReferenceNumberInfo, "Reference Number should be number and up to 10 digits.");
			csaRSFAssessment.ReferenceNumber = "1234567890";
			csaRSFAssessment.Validation.ValidateReferenceNumber();
			AssertNoWarningContaining(csaRSFAssessment.ReferenceNumberInfo, "Reference Number should be number and up to 10 digits.");
		}

		void AssertAlphaAndNumberic_UpTo17Digits(string type)
		{
			csaRSFAssessment.Type = type;
			csaRSFAssessment.ReferenceNumber = "12345678901234abcd";
			csaRSFAssessment.Validation.ValidateReferenceNumber();
			AssertHasWarningContaining(csaRSFAssessment.ReferenceNumberInfo, "Reference Number should be alpha/number and up to 17 digits.");
			csaRSFAssessment.ReferenceNumber = "-";
			csaRSFAssessment.Validation.ValidateReferenceNumber();
			AssertHasWarningContaining(csaRSFAssessment.ReferenceNumberInfo, "Reference Number should be alpha/number and up to 17 digits.");
			csaRSFAssessment.ReferenceNumber = "12345678901234abc";
			csaRSFAssessment.Validation.ValidateReferenceNumber();
			AssertNoWarningContaining(csaRSFAssessment.ReferenceNumberInfo, "Reference Number should be alpha/number and up to 17 digits.");
		}

		protected override void SetUp()
		{
			rSF = Factory.New<CusStatementHeader>();
			line = rSF.StatementLines.AddNew();
			line.B3_EntryType = CSARSFAssessmentTypes.Codes.CustomsAssessment;
			csaRSFAssessment = new CSARSFAssessment(line);
		}
		CSARSFAssessment csaRSFAssessment;
		CusStatementHeader rSF;
		CusStatementLine line;
	}
}
