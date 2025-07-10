using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class ServiceInstructionHelperTest : TestCaseWithFactory
	{
		#region TestGetHandlingInstructionsWithResultAppended_WithOrganisation

		public void TestGetHandlingInstructionsWithResultAppended_WithOrganisation()
		{
			var org = Factory.New<OrgHeader>();
			org.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handling Instruction.");
			org.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "Service Instruction.");

			AssertEquals("Handling Instruction.\r\nService Instruction.", ServiceInstructionHelper.GetHandlingInstructionsWithResultAppended(org, ""));
		}

		#endregion

		#region TestGetHandlingInstructionsWithResultAppended_WithOrganisationAndResultToAppend

		public void TestGetHandlingInstructionsWithResultAppended_WithOrganisationAndResultToAppend()
		{
			var org = Factory.New<OrgHeader>();
			org.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handling Instruction.");

			AssertEquals("TEST\r\nHandling Instruction.", ServiceInstructionHelper.GetHandlingInstructionsWithResultAppended(org, "TEST"));
		}

		#endregion
	}
}
