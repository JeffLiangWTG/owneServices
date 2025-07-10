using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment.Testing
{
	internal class WebServicesEnvironmentTest : TestCase
	{
		public void TestIsWebServiceEnvironment()
		{
			using (var testEnv = new WebServicesEnvironment())
			{
				AssertEquals(nameof(testEnv.IsWebService), true, testEnv.IsWebService);
			}
		}
	}
}
