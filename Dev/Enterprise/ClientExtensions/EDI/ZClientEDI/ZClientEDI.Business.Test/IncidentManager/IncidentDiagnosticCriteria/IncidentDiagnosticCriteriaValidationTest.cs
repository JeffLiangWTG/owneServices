using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentDiagnosticCriteriaValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIMD_Description()
		{
			var diagnosticCriteria = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria.IMD_Description = string.Empty;
			AssertHasError(diagnosticCriteria.IMD_DescriptionInfo, "Please enter a Description.");
			diagnosticCriteria.IMD_Description = "This is description";
			AssertNoError(diagnosticCriteria.IMD_DescriptionInfo, "Please enter a Description.");
		}

		public void TestIMD_Type()
		{
			var diagnosticCriteria = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria.IMD_Type = string.Empty;
			AssertHasError(diagnosticCriteria.IMD_TypeInfo, "Please enter a Type.");
			diagnosticCriteria.IMD_Type = "SVC";
			AssertHasError(diagnosticCriteria.IMD_TypeInfo, "Enter a valid Type.");
			diagnosticCriteria.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom;
			AssertNoError(diagnosticCriteria.IMD_TypeInfo, "Enter a valid Type.");
			diagnosticCriteria.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.DiagnosticFactor;
			AssertNoError(diagnosticCriteria.IMD_TypeInfo, "Enter a valid Type.");
		}

		public void TestIMD_Question()
		{
			var diagnosticCriteria = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria.IMD_Question = string.Empty;
			diagnosticCriteria.IMD_InternalSupportNote = string.Empty;
			diagnosticCriteria.Validation.ValidateIMD_Question();
			AssertHasError(diagnosticCriteria.IMD_QuestionInfo, "Please enter either a client question or an internal support note, or both.");
			diagnosticCriteria.IMD_Question = "This is question.";
			diagnosticCriteria.IMD_InternalSupportNote = string.Empty;
			diagnosticCriteria.Validation.ValidateIMD_Question();
			AssertNoError(diagnosticCriteria.IMD_QuestionInfo, "Please enter either a client question or an internal support note, or both.");
			diagnosticCriteria.IMD_Question = string.Empty;
			diagnosticCriteria.IMD_InternalSupportNote = "This is Internal Support Note.";
			diagnosticCriteria.Validation.ValidateIMD_Question();
			AssertNoError(diagnosticCriteria.IMD_QuestionInfo, "Please enter either a client question or an internal support note, or both.");
		}

		public void TestIMD_InternalSupportNote()
		{
			var diagnosticCriteria = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria.IMD_Question = string.Empty;
			diagnosticCriteria.IMD_InternalSupportNote = string.Empty;
			diagnosticCriteria.Validation.ValidateIMD_InternalSupportNote();
			AssertHasError(diagnosticCriteria.IMD_InternalSupportNoteInfo, "Please enter either a client question or an internal support note, or both.");
			diagnosticCriteria.IMD_Question = "This is question.";
			diagnosticCriteria.IMD_InternalSupportNote = string.Empty;
			diagnosticCriteria.Validation.ValidateIMD_InternalSupportNote();
			AssertNoError(diagnosticCriteria.IMD_InternalSupportNoteInfo, "Please enter either a client question or an internal support note, or both.");
			diagnosticCriteria.IMD_Question = string.Empty;
			diagnosticCriteria.IMD_InternalSupportNote = "This is Internal Support Note.";
			diagnosticCriteria.Validation.ValidateIMD_InternalSupportNote();
			AssertNoError(diagnosticCriteria.IMD_InternalSupportNoteInfo, "Please enter either a client question or an internal support note, or both.");
		}

		public void TestIMD_Keywords()
		{
			var diagnosticCriteria = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			diagnosticCriteria.IMD_Keywords = string.Empty;
			AssertHasError(diagnosticCriteria.IMD_KeywordsInfo, "Please enter a Keywords.");
			diagnosticCriteria.IMD_Keywords = "This is kewords.";
			AssertNoError(diagnosticCriteria.IMD_KeywordsInfo, "Please enter a Keywords.");
		}
	}
}
