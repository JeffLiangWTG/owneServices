using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentTriageDiagnosticCriteriaPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckIMO_IMD_DiagnosticCriteria()
		{
			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Level = IncidentTriageLevels.Codes.PreliminaryAssignment;
			var triage2 = Factory.NewWithValidTestData<IncidentTriage>();
			triage2.IMT_Level = IncidentTriageLevels.Codes.PreliminaryAssignment;

			var criteria1 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria1.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom;
			var criteria2 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria2.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.DiagnosticFactor;
			var criteria3 = Factory.NewWithValidTestData<IncidentDiagnosticCriteria>();
			criteria3.IMD_Type = IncidentDiagnosticCriteriaTypes.Codes.PrimarySymptom;
			Factory.Save();

			var pivot = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot.IMO_IMT_Triage = triage1.PK;
			pivot.IMO_IMD_DiagnosticCriteria = criteria2.PK;
			AssertHasError(pivot.IMO_IMD_DiagnosticCriteriaInfo, "Only Diagnostic Criteria of type 'SMP' can be attached to a 'Preliminary Assignment' Triage Node.");
			pivot.IMO_IMD_DiagnosticCriteria = criteria1.PK;
			pivot.IMO_IMT_Triage = triage2.PK;
			AssertNoErrors(pivot.IMO_IMD_DiagnosticCriteriaInfo);
			Factory.Save();

			var pivot2 = Factory.New<IncidentTriageDiagnosticCriteriaPivot>();
			pivot2.IMO_IMT_Triage = triage1.PK;
			pivot2.IMO_IMD_DiagnosticCriteria = criteria1.PK;
			AssertHasError(pivot2.IMO_IMD_DiagnosticCriteriaInfo, "The 'SMP' Diagnostic Criteria can only be attached to a single 'Preliminary Assignment' Triage Node.");
			pivot2.IMO_IMD_DiagnosticCriteria = criteria3.PK;
			pivot2.RunPreSaveValidation();
			AssertNoErrors(pivot2.IMO_IMD_DiagnosticCriteriaInfo);
		}
	}
}
