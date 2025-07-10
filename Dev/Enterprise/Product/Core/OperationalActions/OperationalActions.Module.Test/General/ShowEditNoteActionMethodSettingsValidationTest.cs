using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Module
{
	sealed class ShowEditNoteActionMethodSettingsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateNoteDescription()
		{
			var settings = new ShowEditNoteActionMethodSettings();

			settings.NoteDescription = ZString.Empty;
			AssertHasError(settings.NoteDescriptionInfo, "Please enter a value.");

			settings.NoteDescription = "Xyz";
			AssertNoErrors(settings.NoteDescriptionInfo);
		}
	}
}
