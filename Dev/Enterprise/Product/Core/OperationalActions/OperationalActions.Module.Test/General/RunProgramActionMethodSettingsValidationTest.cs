using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Module
{
	sealed class RunProgramActionMethodSettingsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidatePath()
		{
			var settings = new RunProgramActionMethodSettings();

			settings.Path = ZString.Empty;
			AssertHasError(settings.PathInfo, "Please enter a value.");

			settings.Path = @"C:\Windows\system32\notepad.exe";
			AssertNoErrors(settings.PathInfo);

			settings.Path = @"\\networkpath\notepad.exe";
			AssertNoErrors(settings.PathInfo);

			settings.Path = @"IamNotAValidPath";
			AssertHasError(settings.PathInfo, "Please enter a valid absolute program path.");
		}
	}
}
