using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Business.Test
{
	public class EDIPredefinedNoteTypesTest : TestCaseWithFactory
	{
		public void TestInstance()
		{
			AssertEquals("EDIPredefinedNoteTypes.Instance should return the correct type", typeof(EDIPredefinedNoteTypes), EDIPredefinedNoteTypes.Instance.GetType());
		}

		public void TestChangeRequestPrerequisites()
		{
			AssertEquals("Feature Request Prerequisites", EDIPredefinedNoteTypes.Instance.FeatureRequestPrerequisites.Description);
			AssertEquals(StmNoteVisibility.PUB, EDIPredefinedNoteTypes.Instance.FeatureRequestPrerequisites.DefaultVisibility);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.FeatureRequestPrerequisites.IsOnlyOneAllowed);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.FeatureRequestPrerequisites.IsReadOnlyAfterAdd);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.FeatureRequestPrerequisites.IsTextOnly);
		}

		public void TestChangeRequestNotes()
		{
			AssertEquals("Feature Request Internal Notes", EDIPredefinedNoteTypes.Instance.FeatureRequestInternalNote.Description);
			AssertEquals(StmNoteVisibility.PUB, EDIPredefinedNoteTypes.Instance.FeatureRequestInternalNote.DefaultVisibility);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.FeatureRequestInternalNote.IsOnlyOneAllowed);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.FeatureRequestInternalNote.IsReadOnlyAfterAdd);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.FeatureRequestInternalNote.IsTextOnly);
		}

		public void TestBusinessRequirement()
		{
			AssertEquals("Business Requirements", EDIPredefinedNoteTypes.Instance.BusinessRequirements.Description);
			AssertEquals(StmNoteVisibility.PUB, EDIPredefinedNoteTypes.Instance.BusinessRequirements.DefaultVisibility);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.BusinessRequirements.IsOnlyOneAllowed);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.BusinessRequirements.IsReadOnlyAfterAdd);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.BusinessRequirements.IsTextOnly);
		}

		public void TestTechnicalSpecification()
		{
			AssertEquals("Technical Specification", EDIPredefinedNoteTypes.Instance.TechnicalSpecification.Description);
			AssertEquals(StmNoteVisibility.PUB, EDIPredefinedNoteTypes.Instance.TechnicalSpecification.DefaultVisibility);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.TechnicalSpecification.IsOnlyOneAllowed);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.TechnicalSpecification.IsReadOnlyAfterAdd);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.TechnicalSpecification.IsTextOnly);
		}

		public void TestIssueManagerAssignedTo()
		{
			AssertEquals("Assigned To History", EDIPredefinedNoteTypes.Instance.IssueManagerAssignedToHistory.Description);
			AssertEquals(StmNoteVisibility.PUB, EDIPredefinedNoteTypes.Instance.IssueManagerAssignedToHistory.DefaultVisibility);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.IssueManagerAssignedToHistory.IsOnlyOneAllowed);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.IssueManagerAssignedToHistory.IsReadOnlyAfterAdd);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.IssueManagerAssignedToHistory.IsTextOnly);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.IssueManagerAssignedToHistory.IsCustomNoteType);
		}

		public void TestTrainingCourseSurveyEmail()
		{
			AssertEquals("Training Course Survey Email", EDIPredefinedNoteTypes.Instance.TrainingCourseSurveyEmail.Description);
			AssertEquals(StmNoteVisibility.PUB, EDIPredefinedNoteTypes.Instance.TrainingCourseSurveyEmail.DefaultVisibility);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.TrainingCourseSurveyEmail.IsOnlyOneAllowed);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.TrainingCourseSurveyEmail.IsReadOnlyAfterAdd);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.TrainingCourseSurveyEmail.IsTextOnly);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.TrainingCourseSurveyEmail.IsCustomNoteType);
		}

		public void TestTrainingNotesWithCorrectMaxLength()
		{
			AssertEquals(2000000, EDIPredefinedNoteTypes.Instance.TrainingCourseInternal.TextOnlyMaxLength);
			AssertEquals(2000000, EDIPredefinedNoteTypes.Instance.TrainingCourseRecommendations.TextOnlyMaxLength);
			AssertEquals(2000000, EDIPredefinedNoteTypes.Instance.TrainingCourseReview.TextOnlyMaxLength);
			AssertEquals(2000000, EDIPredefinedNoteTypes.Instance.TrainingCourseSummary.TextOnlyMaxLength);
			AssertEquals(2000000, EDIPredefinedNoteTypes.Instance.TrainingCourseSurveyEmail.TextOnlyMaxLength);
		}

		public void TestLicenceDiscrepancy()
		{
			AssertEquals("Licence Discrepancy", EDIPredefinedNoteTypes.Instance.LicenceDiscrepancy.Description);
			AssertEquals(StmNoteVisibility.PUB, EDIPredefinedNoteTypes.Instance.LicenceDiscrepancy.DefaultVisibility);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.LicenceDiscrepancy.IsOnlyOneAllowed);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.LicenceDiscrepancy.IsReadOnlyAfterAdd);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.LicenceDiscrepancy.IsTextOnly);
		}

		public void TestStaffCalendarEmailAddress()
		{
			AssertEquals("Staff Calendar Email Address", EDIPredefinedNoteTypes.Instance.StaffCalendarEmailAddress.Description);
			AssertEquals(StmNoteVisibility.PUB, EDIPredefinedNoteTypes.Instance.StaffCalendarEmailAddress.DefaultVisibility);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.StaffCalendarEmailAddress.IsOnlyOneAllowed);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.StaffCalendarEmailAddress.IsReadOnlyAfterAdd);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.StaffCalendarEmailAddress.IsTextOnly);
		}

		public void TestIncidentComment()
		{
			AssertEquals("Incident Comment", EDIPredefinedNoteTypes.Instance.IncidentComment.Description);
			AssertEquals(StmNoteVisibility.PUB, EDIPredefinedNoteTypes.Instance.IncidentComment.DefaultVisibility);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.IncidentComment.IsOnlyOneAllowed);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.IncidentComment.IsReadOnlyAfterAdd);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.IncidentComment.IsTextOnly);
		}

		public void TestIncidentTriagePublishedDescription()
		{
			AssertEquals("Published Description", EDIPredefinedNoteTypes.Instance.IncidentTriagePublishedDescription.Description);
			AssertEquals(StmNoteVisibility.PUB, EDIPredefinedNoteTypes.Instance.IncidentTriagePublishedDescription.DefaultVisibility);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.IncidentTriagePublishedDescription.IsOnlyOneAllowed);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.IncidentTriagePublishedDescription.IsReadOnlyAfterAdd);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.IncidentTriagePublishedDescription.IsTextOnly);
		}

		public void TestIncidentTriageChecklistItemPublishedDescription()
		{
			AssertEquals("Published Description", EDIPredefinedNoteTypes.Instance.IncidentTriageChecklistItemPublishedDescription.Description);
			AssertEquals(StmNoteVisibility.PUB, EDIPredefinedNoteTypes.Instance.IncidentTriageChecklistItemPublishedDescription.DefaultVisibility);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.IncidentTriageChecklistItemPublishedDescription.IsOnlyOneAllowed);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.IncidentTriageChecklistItemPublishedDescription.IsReadOnlyAfterAdd);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.IncidentTriageChecklistItemPublishedDescription.IsTextOnly);
		}

		public void TestOpportunityStatusSummary()
		{
			AssertEquals("Opportunity Status Summary", EDIPredefinedNoteTypes.Instance.OpportunityStatusSummary.Description);
			AssertEquals(StmNoteVisibility.PUB, EDIPredefinedNoteTypes.Instance.OpportunityStatusSummary.DefaultVisibility);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.OpportunityStatusSummary.IsOnlyOneAllowed);
			AssertEquals(false, EDIPredefinedNoteTypes.Instance.OpportunityStatusSummary.IsReadOnlyAfterAdd);
			AssertEquals(true, EDIPredefinedNoteTypes.Instance.OpportunityStatusSummary.IsTextOnly);
		}
	}
}
