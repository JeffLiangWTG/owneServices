using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentTriageChecklistItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIMC_SupportDescription()
		{
			var checklistItem = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			checklistItem.IMC_SupportDescription = string.Empty;
			AssertHasError(checklistItem.IMC_SupportDescriptionInfo, "Please enter a Support Description.");
		}

		public void TestIMC_Category()
		{
			var checklistItem = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();
			checklistItem.IMC_Category = "AAA";
			AssertHasError(checklistItem.IMC_CategoryInfo, "Enter a valid Category.");
		}

		public void TestPublishedDescriptionShouldBeMandatoryIfIMC_IsPublished()
		{
			var checklistItem = Factory.NewWithValidTestData<IncidentTriageChecklistItem>();

			AssertEquals("Precondition: Default should be false", false, checklistItem.IMC_IsPublished);
			AssertEquals("Precondition", true, checklistItem.PublishedDescriptionText.IsEmpty);
			AssertNoErrors("Empty published description is allowed", checklistItem.PublishedDescriptionTextInfo);

			checklistItem.IMC_IsPublished = true;
			AssertHasError("Published description must be entered", checklistItem.PublishedDescriptionTextInfo, "Please enter a Published Description.");

			checklistItem.PublishedDescriptionText = "Hello";
			AssertNoErrors("Entering published description should remove error", checklistItem.PublishedDescriptionTextInfo);
		}
	}
}
