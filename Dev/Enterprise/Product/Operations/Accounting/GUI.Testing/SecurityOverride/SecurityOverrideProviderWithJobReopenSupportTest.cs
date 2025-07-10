using Enterprise.Environment;

namespace Enterprise.Accounting.GUI.Testing
{
	public abstract class SecurityOverrideProviderWithJobReopenSupportTest<SecurityProviderType> : SecurityOverrideProviderWithApprovalRequestSupportTest<SecurityProviderType>
				where SecurityProviderType : SecurityOverrideProviderWithJobReopenSupport
	{
		public override void TestSecurityOverrideMessage()
		{
			base.TestSecurityOverrideMessage();

			var testProvider = GetSecurityProvider();
			AssertEquals(testProvider.GetReopenClosedJobSecurityOverrideMessage_ForTestOnly(), testProvider.GetSecurityOverrideMessage_ForTestOnly(Env.Security.ReopenJob));
			AssertEquals(testProvider.GetReopenRestrictedClosedJobSecurityOverrideMessage_ForTestOnly(), testProvider.GetSecurityOverrideMessage_ForTestOnly(Env.Security.ReopenJobPastAllowedReOpenPeriod));
			AssertEquals(Env.Security.Commodity.DefaultSecurityOverrideMessage, testProvider.GetSecurityOverrideMessage_ForTestOnly(Env.Security.Commodity));
		}

		public void TestSecurityGrantedMessage()
		{
			var testProvider = GetSecurityProvider();
			AssertEquals(testProvider.GetReopenClosedJobSecurityGrantedMessage_ForTestOnly(), testProvider.GetSecurityGrantedMessage_ForTestOnly(Env.Security.ReopenJob));
			AssertEquals(testProvider.GetReopenClosedJobSecurityGrantedMessage_ForTestOnly(), testProvider.GetSecurityGrantedMessage_ForTestOnly(Env.Security.ReopenJobPastAllowedReOpenPeriod));
		}
	}
}
