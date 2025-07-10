using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment.Testing
{
	internal class WebEnvironmentTest : TestCase
	{
		public void TestConstractorWithOptionalUserContextManager()
		{
			using (var environment = new WebEnvironment())
			{
				AssertType(typeof(MultiThreadUserContextManager), environment.UserContextManagerForTesting);
			}

			using (var environment = new WebEnvironment(new SessionUserContextManager()))
			{
				AssertType(typeof(SessionUserContextManager), environment.UserContextManagerForTesting);
			}
		}
	}
}
