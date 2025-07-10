using System.Net;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class AuthenticationModulePrioritiserTest : TestCase
	{
		public void TestOrdering()
		{
			Assert(GetFirstModule().AuthenticationType != "Digest");
			using (AuthenticationModulePrioritiser.MoveToFirstPositionTemporarily("Digest"))
			{
				AssertEquals("Digest", GetFirstModule().AuthenticationType);
			}
			Assert(GetFirstModule().AuthenticationType != "Digest");
		}

		IAuthenticationModule GetFirstModule()
		{
			var modules = AuthenticationManager.RegisteredModules;
			modules.MoveNext();
			return (IAuthenticationModule)modules.Current;
		}
	}
}
