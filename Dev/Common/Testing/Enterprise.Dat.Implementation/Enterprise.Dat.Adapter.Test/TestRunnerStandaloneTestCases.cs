using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing
{
	sealed class TestRunnerStandaloneTestCases : TestCase
	{
#if NETFRAMEWORK
		[TargetFrameworks(TargetFramework.NetCore)]
#endif
		public void TestNet8()
		{
			AssertContains("8", RuntimeInformation.FrameworkDescription);
		}
	}
}
