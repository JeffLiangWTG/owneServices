using System.Collections.Generic;
using System.Net;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class RequestAuthorisationHelperTest : TestCase
	{
		public void TestIsRequestPermitted()
		{
			CombineAssertions(() =>
			{
				foreach (var (ipAddress, shouldBePermitted) in GetTestCases())
				{
					var address = IPAddress.Parse(ipAddress);
					AssertEquals($"Should IP address {ipAddress} be permitted", shouldBePermitted, RequestAuthorisationHelper.IsRequestPermittedFromAddress(address));
				}
			});
		}

		static IEnumerable<(string ipAddress, bool shouldBePermitted)> GetTestCases()
		{
			yield return ("10.0.0.1", true);
			yield return ("10.255.255.254", true);
			yield return ("11.0.0.1", false);
			yield return ("11.255.255.254", false);
			yield return ("9.0.0.1", false);
			yield return ("9.255.255.254", false);
			yield return ("127.0.0.1", false);
		}
	}
}
