using Enterprise.RemotePrinting.Server.RPSCore;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	public class AuthenticationTest : TestCase
	{
		[TestDate(2051, 01, 01, 12, 00, 00)] // ZDateTime will return time in future
		public void TestSameDateTimeUsedForNonce1()
		{
			var auth = Authentication.Instance;
			auth.UseProductionDateTimeForTest = true;
			try
			{
				var nonce = auth.GetCurrentNonce();
				Assert("Nonce should be valid", auth.IsValidNonce(nonce));
			}
			finally
			{
				auth.UseProductionDateTimeForTest = false;
			}
		}

		[TestDate(2001, 01, 01, 12, 00, 00)] // ZDateTime will return time in past
		public void TestSameDateTimeUsedForNonce2()
		{
			var auth = Authentication.Instance;
			auth.UseProductionDateTimeForTest = true;
			try
			{
				var nonce = auth.GetCurrentNonce();
				Assert("Nonce should be valid", auth.IsValidNonce(nonce));
			}
			finally
			{
				auth.UseProductionDateTimeForTest = false;
			}
		}

		[TestDate(2022, 02, 22, 12, 22, 22)] // ZDateTime will return time in past
		public void TestIsValidNonce()
		{
			var auth = Authentication.Instance;
			var nonce = auth.GetCurrentNonce();
			Assert("Nonce should be valid", auth.IsValidNonce(nonce));

			TestDateAttribute.AddMinutes(2);
			Assert("Nonce should become stale", !auth.IsValidNonce(nonce));
		}

		public void TestIsSupportUser()
		{
			Assert("CWSupport user should start with CWSupport-", Authentication.IsSupportUser("CWSupport-TESTTOKEN"));
		}
	}
}
