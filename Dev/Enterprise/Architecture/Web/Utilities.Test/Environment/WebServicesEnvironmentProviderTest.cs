using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment.Testing
{
	internal class WebServicesEnvironmentProviderTest : TestCase
	{
		public void TestEnablingWebServicesEnvironmentProviderSetsIsWeb()
		{
			// Arrange
			var oldEnvProvider = Env.GetCurrentProvider();
			Globals.IsTest_ForTest.Value = false;

			try
			{
				using (var provider = new WebServicesEnvironmentProviderForTest())
				{
					// Act
					WebInitialiser.Initialise(false, provider);

					// Assert
					Assert(Globals.IsWebService);
					Assert(!Globals.IsWeb);
				}
			}
			finally
			{
				Globals.IsTest_ForTest.Value = true;
				oldEnvProvider.Enable();
			}
		}
	}
}
