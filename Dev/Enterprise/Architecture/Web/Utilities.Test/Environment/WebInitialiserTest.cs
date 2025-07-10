using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Initialisation;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment.Testing
{
	internal class WebInitialiserTest : BaseInitialiserTest
	{
		public void TestEnvironmentPropertiesAreSetToTheCorrectValueOnStartupWithDefaults()
		{
			AssertEnvironmentProperties("Initialise", false, false, typeof(WebExceptionReporter), typeof(WinFormsEnvironment), typeof(BaseDbEnvironment));
		}

		public void TestEnvironmentPropertiesAreSetToTheCorrectValueOnStartupWithErrorReportsEnabled()
		{
			AssertEnvironmentProperties("InitialiseErrorReport", false, false, typeof(WebExceptionReporter), typeof(WinFormsEnvironment), typeof(BaseDbEnvironment));
		}

		public void TestEnvironmentPropertiesAreSetToTheCorrectValueOnStartupWithErrorReportsDisabled()
		{
			AssertEnvironmentProperties("InitialiseNoErrorReport", false, false, null, typeof(WinFormsEnvironment), typeof(BaseDbEnvironment));
		}

		public void TestEnvironmentPropertiesAreSetToTheCorrectValueOnStartupWithDifferentEnvironment()
		{
			AssertEnvironmentProperties("InitialiseErrorReportWithEnvProvider", false, false, typeof(WebExceptionReporter), typeof(WebEnvironment), typeof(WebDbEnvironment));
		}
	}
}
