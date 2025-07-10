using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment.Testing
{
	internal class WebEnvironmentProviderTest : TestCase
	{
		public void TestWebEnvironmentIsUsingMultiThreadUserContext()
		{
			using (var provider = new WebEnvironmentProvider() { IsTest = true })
			{
				AssertType(typeof(MultiThreadUserContextManager), provider.Instance.UserContextManagerForTesting);
				provider.Instance.Dispose();
			}
		}

		public void TestWebEnvironmentIsUsingOptionialUserContext()
		{
			var context = new SessionUserContextManager();
			using (var provider = new WebEnvironmentProvider(() => context) { IsTest = true })
			{
				AssertSame(context, provider.Instance.UserContextManagerForTesting);
				provider.Instance.Dispose();
			}
		}

		public void TestEnablingWebEnvironmentProviderSetsIsWeb()
		{
			// Arrange
			var oldEnvProvider = Env.GetCurrentProvider();
			Globals.IsTest_ForTest.Value = false;

			try
			{
				using (var provider = new WebEnvironmentProvider() { IsTest = true })
				{
					// Act
					WebInitialiser.Initialise(false, provider);

					// Assert
					Assert(Globals.IsWeb);
					Assert(!Globals.IsWebService);

					provider.Instance.Dispose();
				}
			}
			finally
			{
				Globals.IsTest_ForTest.Value = true;
				oldEnvProvider.Enable();
			}
		}

		public void TestEnablingWebServiceEnvironmentProviderSetsIsWeb()
		{
			// Arrange
			var oldEnvProvider = Env.GetCurrentProvider();
			Globals.IsTest_ForTest.Value = false;

			try
			{
				using (var provider = new WebServiceEnvironmentProvider())
				{
					// Act
					WebInitialiser.Initialise(false, provider);

					// Assert
					Assert(Globals.IsWeb);
					Assert(!Globals.IsWebService);

					provider.Instance.Dispose();
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
