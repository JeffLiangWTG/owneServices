using Enterprise.Environment;

namespace Enterprise.Security.Testing
{
	sealed class NonInteractiveSecurityOverrideProviderTest : SecurityOverrideProviderTestCase
	{
		public void TestValidOverrideLogin()
		{
			bool isAllowed = Env.Security.ReopenJob.IsAllowed;
			Env.Security.ReopenJob.IsAllowed = false;
			NonInteractiveSecurityOverrideProvider testProvider = new NonInteractiveSecurityOverrideProvider();
			AssertEquals(false, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.ReopenJob].IsAllowed);

			SecurityTestObject.CreateTestUser(true, Env.Security.ReopenJob.Code, "tst", "testuser", "password");

			testProvider.OverrideLogin = "testuser";
			testProvider.OverridePassword = "password";

			AssertEquals(true, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.ReopenJob].IsAllowed);
			Env.Security.ReopenJob.IsAllowed = isAllowed;
		}

		public void TestInvalidOverrideLogin()
		{
			bool isAllowed = Env.Security.ReopenJob.IsAllowed;
			Env.Security.ReopenJob.IsAllowed = false;
			NonInteractiveSecurityOverrideProvider testProvider = new NonInteractiveSecurityOverrideProvider();
			testProvider.OverrideLogin = "invaliduser";
			testProvider.OverridePassword = "";

			AssertEquals(false, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.ReopenJob].IsAllowed);
			Env.Security.ReopenJob.IsAllowed = isAllowed;
		}

		#region implementation

		protected override SecurityOverrideProvider GetSecurityOverrideProvider()
		{
			return new NonInteractiveSecurityOverrideProvider();
		}

		#endregion
	}
}
